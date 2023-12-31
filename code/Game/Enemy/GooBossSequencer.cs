using Sandbox;
using System.Collections.Generic;
using System;
using BrickJam.Player;
using System.Linq;
using BrickJam.Components;
using Coroutines;
using Coroutines.Stallers;

namespace BrickJam;

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

	[Property] GameObject IntroBezier { get; set; }

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
			item.GetComponent<Collider>().Enabled = false;
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
			ball.AddComponent<ColliderBoxComponent>();
			ball.Tags.Add( "enemy" );
			ball.AddComponent<AimableTargetComponent>();

			ball.SetParent( GameObject );
			Eyeballs.Add( ball );
		}
	}

	TimeUntil timerIdleSound = new Random().Float( 4f, 12f );

	private void MakeIdleSounds()
	{
		if ( timerIdleSound > 0f ) return;
		if ( !string.IsNullOrEmpty( IdleSound ) )
		{
			Sound.FromWorld( IdleSound, Transform.Position );
		}

		timerIdleSound = new Random().Float( 4f, 12f );
	}

	private void OnDeath()
	{
		BossModel.Set( "die", true );

		if ( !string.IsNullOrEmpty( DeathSound ) )
			Sound.FromWorld( DeathSound, Transform.Position );

		Coroutine.Start( OnDeathCoroutine );
	}

	private CoroutineMethod OnDeathCoroutine()
	{
		yield return new WaitForSeconds( 10 );

		var modelcomp = GetComponent<AnimatedModelComponent>( false, true );
		while ( modelcomp.SceneObject.ColorTint.a > 0 )
		{
			var col = modelcomp.SceneObject.ColorTint;
			col.a -= Time.Delta;
			modelcomp.SceneObject.ColorTint = col;
			yield return new WaitForNextFrame();
		}

		yield return new WaitForNextFrame();

		var flags = PlayerFlagsComponent.Instance;
		if ( flags is not null )
		{
			flags.InBossSequence = false;
		}

		GameObject.Destroy();
	}

	public override void OnDestroy()
	{
		var flags = PlayerFlagsComponent.Instance;
		if ( flags is not null )
		{
			flags.InBossSequence = false;
		}

		base.OnDestroy();
	}


	public void TriggerDamage( GooBossEyeball eye, bool killingBlow = false )
	{
		BossModel.Set( "hit", true );

		if ( !string.IsNullOrEmpty( HurtSound ) )
			Sound.FromWorld( HurtSound, Transform.Position );

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

	private void OnTriggered()
	{
		if ( !StartedFight )
		{
			foreach ( var item in doorBlockers.Children )
			{
				item.GetComponent<ModelComponent>( false, true ).Enabled = true;
				item.GetComponent<Collider>( false, true ).Enabled = true;
			}

			Scene.GetAllObjects( true ).Where( X => X.GetComponent<CameraComponent>( false ) != null ).First().GetComponent<CameraComponent>( false ).FieldOfView = 80f;
			Coroutine.Start( OnTriggeredCoroutine );
		}
	}

	private CoroutineMethod OnTriggeredCoroutine()
	{
		var animateCoroutine = Coroutine.Start( IntroBezier.GetComponent<BezierAnimationComponent>().AnimateObjectCoroutine,
			roomTrigger.GetComponent<CameraTriggerComponent>().CameraPoint, 5f, true );
		yield return new WaitForCoroutine( animateCoroutine );

		StartedFight = true;
		EyesOpen = true;
		var flags = PlayerFlagsComponent.Instance;
		if ( flags is not null )
		{
			flags.InBossSequence = true;
		}
	}

	private CoroutineMethod SpawnEnemiesCoroutine()
	{
		int amountOfEnemies = 7 - Eyeballs.Count;
		for ( int i = 0; i < amountOfEnemies; i++ )
		{
			yield return new WaitForSeconds( Game.Random.Float( 1, 6 ) );

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

			yield return new WaitForSeconds( 0.6f );

			if ( !string.IsNullOrEmpty( AttackSound ) )
				Sound.FromWorld( AttackSound, spawnpos );

			List<GameObject> enemiesToSpawn = new();
			if ( Enemy1 != null ) enemiesToSpawn.Add( Enemy1 );
			if ( Enemy2 != null ) enemiesToSpawn.Add( Enemy2 );
			if ( Enemy3 != null && Eyeballs.Count < 4 ) enemiesToSpawn.Add( Enemy3 );

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
		MakeIdleSounds();

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
				Coroutine.Start( SpawnEnemiesCoroutine );
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
