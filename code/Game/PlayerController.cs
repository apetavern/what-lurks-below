using Sandbox;
using System.Drawing;
using System.Linq;

public class PlayerController : BaseComponent
{
	[Property] public Vector3 Gravity { get; set; } = new Vector3( 0, 0, 800 );
	[Property] public float CameraDistance { get; set; } = 200.0f;

	public Vector3 WishVelocity { get; private set; }

	[Property] GameObject Body { get; set; }
	[Property] GameObject Eye { get; set; }
	[Property] public GameObject Camera { get; set; }
	[Property] bool FirstPerson { get; set; }

	public Angles EyeAngles;

	public override void OnEnabled()
	{
		base.OnEnabled();
		// Update camera position
		Camera = Scene.GetAllObjects( true ).Where( X => X.GetComponent<CameraComponent>( false ) != null ).FirstOrDefault();
		if ( Camera is not null )
		{
			var camPos = Eye.Transform.Position - EyeAngles.ToRotation().Forward * CameraDistance;

			if ( FirstPerson ) camPos = Eye.Transform.Position + EyeAngles.ToRotation().Forward * 8;

			Camera.Transform.Position = camPos;
			Camera.Transform.Rotation = EyeAngles.ToRotation();
		}
	}

	public override void Update()
	{
		// Eye input
		EyeAngles.pitch = 0;

		if ( Input.Down( "Left" ) ) EyeAngles.yaw += Time.Delta * 90f;
		if ( Input.Down( "Right" ) ) EyeAngles.yaw -= Time.Delta * 90f;

		//EyeAngles.yaw -= Input.MouseDelta.x * 0.1f;
		EyeAngles.roll = 0;

		Camera = Scene.GetAllObjects( true ).Where( X => X.GetComponent<CameraComponent>( false ) != null ).FirstOrDefault();


		// read inputs
		BuildWishVelocity();

		var cc = GameObject.GetComponent<CharacterController>();


		if ( Transform.Position.z < -100f )
		{
			var room = Scene.GetAllObjects( true ).Where( X => X.GetComponent<RoomChunkComponent>( false ) != null ).FirstOrDefault();

			Transform.Position = room.Transform.Position.WithZ( 0 );
			WishVelocity = Vector3.Zero;
			cc.Velocity = Vector3.Zero;
		}

		// rotate body to look angles
		if ( Body is not null )
		{
			Body.Transform.Rotation = new Angles( 0, EyeAngles.yaw, 0 ).ToRotation();
			var helper = new CitizenAnimationHelperScene( Body.GetComponent<AnimatedModelComponent>().SceneModel );
			helper.WithVelocity( cc.Velocity / 2f );
			helper.IsGrounded = cc.IsOnGround;
			if ( Input.Pressed( "Jump" ) && cc.IsOnGround )
			{
				helper.TriggerJump();
			}

			if ( Input.Down( "Attack2" ) )
			{
				helper.Handedness = CitizenAnimationHelperScene.Hand.Both;
				helper.HoldType = CitizenAnimationHelperScene.HoldTypes.Pistol;
			}
			else
			{
				helper.Handedness = CitizenAnimationHelperScene.Hand.Left;
				helper.HoldType = CitizenAnimationHelperScene.HoldTypes.Pistol;
			}

			if ( Input.Pressed( "Attack1" ) )
			{
				helper.TriggerAttack();
				Sound.FromWorld( "weapons/rust_pistol/sound/rust_pistol.shoot.sound", Transform.Position + Vector3.Up * 32f );
			}

			if ( Input.Down( "Duck" ) )
			{
				helper.DuckLevel = 1f;
				if ( Input.Pressed( "Jump" ) )
				{
					helper.SpecialMove = CitizenAnimationHelperScene.SpecialMovement.Roll;
				}
				else
				{
					helper.SpecialMove = CitizenAnimationHelperScene.SpecialMovement.None;
				}
			}
			else
			{
				helper.DuckLevel = 0f;
			}
		}

		if ( cc.IsOnGround && Input.Down( "Jump" ) && !Input.Down( "Duck" ) )
		{
			float flGroundFactor = 1.0f;
			float flMul = 268.3281572999747f * 1.2f;
			//if ( Duck.IsActive )
			//	flMul *= 0.8f;

			cc.Punch( Vector3.Up * flMul * flGroundFactor );
			//	cc.IsOnGround = false;
		}

		if ( cc.IsOnGround )
		{
			cc.Velocity = cc.Velocity.WithZ( 0 );
			cc.Accelerate( WishVelocity );
			cc.ApplyFriction( 4.0f );
		}
		else
		{
			cc.Velocity -= Gravity * Time.Delta * 0.5f;
			cc.Accelerate( WishVelocity.ClampLength( 50 ) );
			cc.ApplyFriction( 0.1f );
		}

		cc.Move();

		if ( !cc.IsOnGround )
		{
			cc.Velocity -= Gravity * Time.Delta * 0.5f;
		}
		else
		{
			cc.Velocity = cc.Velocity.WithZ( 0 );
		}
	}

	public void BuildWishVelocity()
	{
		var rot = EyeAngles.ToRotation();

		WishVelocity = 0;

		if ( Input.Down( "Forward" ) ) WishVelocity += rot.Forward;
		if ( Input.Down( "Backward" ) ) WishVelocity += rot.Backward;


		WishVelocity = WishVelocity.WithZ( 0 );

		if ( !WishVelocity.IsNearZeroLength ) WishVelocity = WishVelocity.Normal;

		if ( Input.Down( "Run" ) ) WishVelocity *= 150.0f;
		else WishVelocity *= 85.0f;
	}
}
