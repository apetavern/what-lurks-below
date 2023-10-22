using Sandbox;
using System.Collections.Generic;
using System;
using BrickJam.Player;

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

	AnimatedModelComponent BossModel { get; set; }

	public List<GameObject> Eyeballs { get; set; } = new List<GameObject>();

	public bool StartedFight = false;

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
		foreach ( var item in doorBlockers.Children )
		{
			item.GetComponent<ModelComponent>( false, true ).Enabled = true;
			item.GetComponent<ColliderBaseComponent>( false, true ).Enabled = true;
		}

	}

	public override void Update()
	{
		if ( !StartedFight )
		{
			return;
		}
	}
}
