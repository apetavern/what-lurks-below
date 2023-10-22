using Sandbox;
using System.Collections.Generic;
using System;
using BrickJam.Player;
using BrickJam.Game;

public enum EyeballPosition
{
	middle,
	right1,
	right2,
	left1,
	left2
}

public sealed class GooBossSequencer : BaseComponent
{
	[Property] GameObject roomTrigger { get; set; }

	[Property] GameObject doorBlockers { get; set; }

	[Property] public string IdleSound { get; set; } = "";
	[Property] public string HurtSound { get; set; } = "";
	[Property] public string AttackSound { get; set; } = "";
	[Property] public string DeathSound { get; set; } = "";

	[Property] public GameObject Enemy1 { get; set; }
	[Property] public GameObject Enemy2 { get; set; }
	[Property] public GameObject Enemy3 { get; set; }

	AnimatedModelComponent BossModel { get; set; }

	public List<GameObject> Eyeballs { get; set; } = new List<GameObject>();

	public bool StartedFight = false;

	public bool EyesOpen = true;

	List<GameObject> ActiveEnemies { get; set; } = new List<GameObject>();

	public override void OnStart()
	{
		roomTrigger.GetComponent<CameraTriggerComponent>().OnTriggered += OnTriggered;
		foreach ( var item in doorBlockers.Children )
		{
			item.GetComponent<ModelComponent>().Enabled = false;
			item.GetComponent<ColliderBaseComponent>().Enabled = false;
		}

		BossModel = GetComponent<AnimatedModelComponent>();

		foreach ( var eye in Enum.GetValues<EyeballPosition>() )
		{
			var ball = new GameObject( true, eye.ToString() );

			ball.AddComponent<HealthComponent>();
			if ( eye == EyeballPosition.right2 || eye == EyeballPosition.left2 )
			{
				ball.GetComponent<HealthComponent>().InitialHealth = 50f;
				ball.GetComponent<HealthComponent>().Health = 50f;
			}
			else
			{
				ball.GetComponent<HealthComponent>().InitialHealth = 100f;
				ball.GetComponent<HealthComponent>().Health = 100f;
			}

			ball.AddComponent<GooBossEyeball>().pos = eye;
			ball.GetComponent<GooBossEyeball>( false ).boss = this;
			ball.AddComponent<ColliderBoxComponent>().Tags = "enemy";
			ball.AddComponent<AimableTargetComponent>();

			ball.SetParent( Scene );
			Eyeballs.Add( ball );
		}
	}

	public async void OnDeath()
	{
		GetComponent<AnimatedModelComponent>().Set( "die", true );

		if ( !string.IsNullOrEmpty( DeathSound ) )
			Sound.FromWorld( DeathSound, Transform.Position );

		await GameTask.DelaySeconds( 2f );
		var modelcomp = GetComponent<AnimatedModelComponent>( false, true );
		while ( modelcomp.SceneObject.ColorTint.a > 0 )
		{
			var col = modelcomp.SceneObject.ColorTint;
			col.a -= Time.Delta * 2f;
			modelcomp.SceneObject.ColorTint = col;
			await GameTask.DelaySeconds( Time.Delta );
		}

		await GameTask.DelaySeconds( Time.Delta );

		GameObject.Destroy();
	}


	public void TriggerDamage( GooBossEyeball eye, bool killingBlow = false )
	{
		BossModel.Set( "hit", true );

		if ( killingBlow )
		{
			BossModel.SceneObject.SetBodyGroup( eye.pos.ToString(), 1 );

			Eyeballs.Remove( eye.GameObject );

			if ( Eyeballs.Count == 0 )
			{
				OnDeath();
			}
		}
	}

	public void OnTriggered()
	{
		StartedFight = true;
		EyesOpen = true;
		foreach ( var item in doorBlockers.Children )
		{
			item.GetComponent<ModelComponent>( false, true ).Enabled = true;
			item.GetComponent<ColliderBaseComponent>( false, true ).Enabled = true;
		}

	}

	public async void SpawnEnemies()
	{
		int amountOfEnemies = 7 - Eyeballs.Count;
		for ( int i = 0; i < amountOfEnemies; i++ )
		{
			await GameTask.DelaySeconds( Game.Random.Float( 1f, 6f ) );

			List<Transform> enemySpawnPositions = new()
			{
				BossModel.GetAttachmentTransform( "leftmouth" ),
				BossModel.GetAttachmentTransform( "middlemouth" ),
				BossModel.GetAttachmentTransform( "rightmouth" )
			};

			Vector3 spawnpos = Vector3.Zero;

			int chosenspawn = Game.Random.Int( 0, enemySpawnPositions.Count - 1 );

			spawnpos = enemySpawnPositions[chosenspawn].Position;

			switch ( chosenspawn )
			{
				case 0:
					BossModel.Set( "spitleft", true );
					break;
				case 1:
					BossModel.Set( "spitmiddle", true );
					break;
				case 2:
					BossModel.Set( "spitright", true );
					break;
				default:
					break;
			}

			await GameTask.DelaySeconds( 0.6f );

			List<GameObject> enemiesToSpawn = new();
			if ( Enemy1 != null ) enemiesToSpawn.Add( Enemy1 );
			if ( Enemy2 != null ) enemiesToSpawn.Add( Enemy2 );
			if ( Enemy3 != null ) enemiesToSpawn.Add( Enemy3 );

			if ( enemiesToSpawn.Count > 0 )
			{
				var enemy = enemiesToSpawn[Game.Random.Int( 0, enemiesToSpawn.Count - 1 )];
				var enemyObject = SceneUtility.Instantiate( enemy, GameObject.Transform.Position, GameObject.Transform.Rotation );
				enemyObject.Transform.Position = spawnpos;
				enemyObject.SetParent( GameObject.Parent );
				enemyObject.GetComponent<EnemyController>( false ).AggroRange = 1024f;
				ActiveEnemies.Add( enemyObject );
			}
		}
		Spawning = false;
	}

	public TimeSince TimeSinceEyesOpened;

	bool Spawning;

	public override void Update()
	{
		if ( !StartedFight )
		{
			return;
		}

		BossModel.Set( "closeeyes", !EyesOpen );

		if ( TimeSinceEyesOpened > 15f )
		{
			if ( EyesOpen )
			{
				Spawning = true;
				SpawnEnemies();
				EyesOpen = false;
				foreach ( var eye in Eyeballs )
				{
					eye.GetComponent<GooBossEyeball>().HitsThisCycle = 0;
				}
			}
		}

		if ( !EyesOpen && !Spawning )
		{
			if ( ActiveEnemies.Count > 0 && !ActiveEnemies[0].Active )
			{
				ActiveEnemies.RemoveAt( 0 );
			}

			if ( ActiveEnemies.Count == 0 )
			{
				TimeSinceEyesOpened = 0f;
				EyesOpen = true;
			}
		}

	}
}
