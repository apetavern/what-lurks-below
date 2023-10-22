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

	AnimatedModelComponent BossModel { get; set; }

	public List<GameObject> Eyeballs { get; set; }

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
			ball.SetParent( GameObject );
			ball.AddComponent<HealthComponent>().Health = 100;
			ball.AddComponent<GooBossEyeball>().pos = eye;
			ball.AddComponent<ColliderBoxComponent>().Tags = "enemy";
			ball.AddComponent<AimableTargetComponent>();
			ball.AddComponent<ModelComponent>();
		}
	}

	public void TriggerDamage( GooBossEyeball eye, bool killingBlow = false )
	{
		BossModel.Set( "hit", true );

		if ( killingBlow )
		{
			BossModel.SceneObject.SetBodyGroup( eye.pos.ToString(), 1 );

			Eyeballs.Remove( eye.GameObject );

			eye.GameObject.Destroy();
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
