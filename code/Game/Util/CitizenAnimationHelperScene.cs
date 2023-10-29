using System;

namespace Sandbox;

/// <summary>
/// A struct to help you set up your citizen based animations
/// </summary>
public struct CitizenAnimationHelperScene
{
	SceneModel Owner;

	public CitizenAnimationHelperScene( SceneModel entity )
	{
		Owner = entity;
	}

	public void SetAnimLookAt( string name, Transform origin, Vector3 eyePositionInWorld, Vector3 lookatPositionInWorld )
	{
		Vector3 value = (lookatPositionInWorld - eyePositionInWorld) * origin.Rotation.Inverse;
		Owner.SetAnimParameter( name, value );
	}

	public void SetAnimParameter( string param, float value )
	{
		Owner.SetAnimParameter( param, value );
	}

	/// <summary>
	/// Have the player look at this point in the world
	/// </summary>
	public void WithLookAt( Transform origin, Vector3 eyepos, Vector3 look, float eyesWeight = 1.0f, float headWeight = 1.0f, float bodyWeight = 1.0f )
	{
		SetAnimLookAt( "aim_eyes", origin, eyepos, look );
		SetAnimLookAt( "aim_head", origin, eyepos, look );
		SetAnimLookAt( "aim_body", origin, eyepos, look );

		Owner.SetAnimParameter( "aim_eyes_weight", eyesWeight );
		Owner.SetAnimParameter( "aim_head_weight", headWeight );
		Owner.SetAnimParameter( "aim_body_weight", bodyWeight );
	}

	public void WithVelocity( Vector3 Velocity )
	{
		var dir = Velocity;
		var forward = Owner.Rotation.Forward.Dot( dir );
		var sideward = Owner.Rotation.Right.Dot( dir );

		var angle = MathF.Atan2( sideward, forward ).RadianToDegree().NormalizeDegrees();

		Owner.SetAnimParameter( "move_direction", angle );
		Owner.SetAnimParameter( "move_speed", Velocity.Length );
		Owner.SetAnimParameter( "move_groundspeed", Velocity.WithZ( 0 ).Length );
		Owner.SetAnimParameter( "move_y", sideward );
		Owner.SetAnimParameter( "move_x", forward );
		Owner.SetAnimParameter( "move_z", Velocity.z );
	}

	public void WithWishVelocity( Vector3 Velocity )
	{
		var dir = Velocity;
		var forward = Owner.Rotation.Forward.Dot( dir );
		var sideward = Owner.Rotation.Right.Dot( dir );

		var angle = MathF.Atan2( sideward, forward ).RadianToDegree().NormalizeDegrees();

		Owner.SetAnimParameter( "wish_direction", angle );
		Owner.SetAnimParameter( "wish_speed", Velocity.Length );
		Owner.SetAnimParameter( "wish_groundspeed", Velocity.WithZ( 0 ).Length );
		Owner.SetAnimParameter( "wish_y", sideward );
		Owner.SetAnimParameter( "wish_x", forward );
		Owner.SetAnimParameter( "wish_z", Velocity.z );
	}

	public Rotation AimAngle
	{
		set
		{
			value = Owner.Rotation.Inverse * value;
			var ang = value.Angles();

			Owner.SetAnimParameter( "aim_body_pitch", ang.pitch );
			Owner.SetAnimParameter( "aim_body_yaw", ang.yaw );
		}
	}

	public float AimEyesWeight
	{
		get => 0;
		set => Owner.SetAnimParameter( "aim_eyes_weight", value );
	}

	public float AimHeadWeight
	{
		get => 0;
		set => Owner.SetAnimParameter( "aim_head_weight", value );
	}

	public float AimBodyWeight
	{
		get => 0;
		set => Owner.SetAnimParameter( "aim_headaim_body_weight_weight", value );
	}


	public float FootShuffle
	{
		get => 0;
		set => Owner.SetAnimParameter( "move_shuffle", value );
	}

	public float DuckLevel
	{
		get => 0;
		set => Owner.SetAnimParameter( "duck", value );
	}

	public float VoiceLevel
	{
		get => 0;
		set => Owner.SetAnimParameter( "voice", value );
	}

	public bool IsSitting
	{
		get => false;
		set => Owner.SetAnimParameter( "b_sit", value );
	}

	public bool IsGrounded
	{
		get => false;
		set => Owner.SetAnimParameter( "b_grounded", value );
	}

	public bool IsSwimming
	{
		get => false;
		set => Owner.SetAnimParameter( "b_swim", value );
	}

	public bool IsClimbing
	{
		get => false;
		set => Owner.SetAnimParameter( "b_climbing", value );
	}

	public bool IsNoclipping
	{
		get => false;
		set => Owner.SetAnimParameter( "b_noclip", value );
	}

	public bool IsWeaponLowered
	{
		get => false;
		set => Owner.SetAnimParameter( "b_weapon_lower", value );
	}

	public enum HoldTypes
	{
		None,
		Pistol,
		Rifle,
		Shotgun,
		HoldItem,
		Punch,
		Swing,
		RPG
	}

	public HoldTypes HoldType
	{
		get => 0;
		set => Owner.SetAnimParameter( "holdtype", (int)value );
	}

	public enum Hand
	{
		Both,
		Right,
		Left
	}

	public Hand Handedness
	{
		get => 0;
		set => Owner.SetAnimParameter( "holdtype_handedness", (int)value );
	}

	public void TriggerJump()
	{
		Owner.SetAnimParameter( "b_jump", true );
	}

	public void TriggerAttack()
	{
		Owner.SetAnimParameter( "b_attack", true );
	}

	public void TriggerReload()
	{
		Owner.SetAnimParameter( "b_reload", true );
	}

	public void TriggerDeploy()
	{
		Owner.SetAnimParameter( "b_deploy", true );
	}

	public void TriggerHit()
	{
		Owner.SetAnimParameter( "hit_bone", 0 );
		Owner.SetAnimParameter( "hit_direction", Vector3.Up * 10f );
		Owner.SetAnimParameter( "hit_offset", Vector3.Forward * 60f );
		Owner.SetAnimParameter( "hit_strength", 0.75f );
		Owner.SetAnimParameter( "hit", true );

	}

	public enum MoveStyles
	{
		Auto,
		Walk,
		Run
	}

	/// <summary>
	/// We can force the model to walk or run, or let it decide based on the speed.
	/// </summary>
	public MoveStyles MoveStyle
	{
		get => 0;
		set => Owner.SetAnimParameter( "move_style", (int)value );
	}

	public enum SpecialMovement
	{
		None,
		LedgeGrab,
		Roll,
		Slide
	}

	/// <summary>
	/// We can force the model to walk or run, or let it decide based on the speed.
	/// </summary>
	public SpecialMovement SpecialMove
	{
		get => 0;
		set => Owner.SetAnimParameter( "special_movement_states", (int)value );
	}
}
