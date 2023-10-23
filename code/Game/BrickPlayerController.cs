using BrickJam.Game;
using BrickJam.Player;
using Sandbox;
using System;
using System.Linq;
using BrickJam.Game.UI;

public class BrickPlayerController : BaseComponent
{
	[Property] public Vector3 Gravity { get; set; } = new Vector3( 0, 0, 800 );

	[Range( 0, 400 )]
	[Property] public float CameraDistance { get; set; } = 200.0f;

	public Vector3 WishVelocity { get; private set; }
	public GameObject Player => GameObject;

	[Property] public GameObject Body { get; set; }
	[Property] public GameObject Eye { get; set; }
	[Property] public GameObject Camera { get; set; }
	[Property] bool FirstPerson { get; set; }

	public Angles EyeAngles;

	Vector3 EyeStartPos;

	float AimMultiplier = 1f;

	NavGenComponent navgen;

	public Vector3 startpos;

	TimeSince TimeSinceLastGlowstick;

	public void SpawnGlowstick()
	{
		if ( TimeSinceLastGlowstick > 1f )
		{
			var prefab = ResourceLibrary.Get<PrefabFile>( "prefabs/glowstick.object" );
			var bone = Body.GetComponent<AnimatedModelComponent>().SceneObject.GetBoneWorldTransform( "hold_R" );

			var go = SceneUtility.Instantiate( prefab.Scene, bone.Position, Rotation.FromPitch( 90 ) );

			go.GetComponent<GlowstickComponent>( false, true ).Velocity = Body.Transform.Rotation.Forward * 250f + Vector3.Up * 150f;
			go.GetComponent<GlowstickComponent>( false, true ).Player = GameObject;
			go.SetParent( navgen.GameObject.Children.First() );
		}
	}

	public override void OnEnabled()
	{
		base.OnEnabled();

		startpos = Transform.Position;

		// Update camera position
		Camera = Scene.GetAllObjects( true ).Where( X => X.GetComponent<CameraComponent>( false ) != null ).FirstOrDefault();
		if ( Camera is not null )
		{
			var camPos = Eye.Transform.Position - EyeAngles.ToRotation().Forward * CameraDistance;

			if ( FirstPerson ) camPos = Eye.Transform.Position + EyeAngles.ToRotation().Forward * 8;

			Camera.Transform.Position = camPos;
			Camera.Transform.Rotation = EyeAngles.ToRotation();
		}

		EyeStartPos = Eye.Transform.LocalPosition;

		HealthComponent healthComponent = GameObject.GetComponent<HealthComponent>( false );
		healthComponent.OnDamage += TakeDamage;
	}

	public void TakeDamage()
	{
		var helper = new CitizenAnimationHelperScene( Body.GetComponent<AnimatedModelComponent>().SceneObject );
		helper.TriggerHit();
	}

	public override void DrawGizmos()
	{
		base.DrawGizmos();
		Gizmo.Draw.Line( Eye.Transform.Position - Transform.Position, Eye.Transform.Position - Transform.Position + Eye.Transform.Rotation.Forward * 100f );
	}

	public GameObject GetClosestAimableObjectInViewcone()
	{
		var PotentialAimTargets = Scene.GetAllObjects( true ).Where( X => X.GetComponent<AimableTargetComponent>( false ) != null && Vector3.DistanceBetween( X.Transform.Position, Transform.Position ) < 378 );
		GameObject closestTarget = null;
		float closestDistance = float.MaxValue;

		float yourFieldOfViewAngle = 25f;

		foreach ( var target in PotentialAimTargets )
		{
			Vector3 toTarget = target.GetBounds().Center - Eye.Transform.Position;

			if ( Vector3.Dot( toTarget.Normal, Eye.Transform.Rotation.Forward.Normal ) >= MathF.Cos( MathX.DegreeToRadian( yourFieldOfViewAngle ) ) )
			{
				float distanceToTarget = toTarget.Length;

				if ( distanceToTarget < closestDistance )
				{
					closestDistance = distanceToTarget;
					closestTarget = target;
				}
			}
		}

		return closestTarget;
	}


	public bool CameraControl;

	public override void Update()
	{
		var cc = GameObject.GetComponent<CharacterController>();

		var pickups = Scene.GetAllObjects( true ).Where( x => x.GetComponent<ItemPickup>() != null );
		pickups = pickups.Where( p => p.Transform.Position.Distance( Transform.Position ) < 50 );

		foreach ( var pickup in pickups )
		{
			if ( pickup.GetComponent<WorldPanel>() is null )
			{
				var wp = pickup.AddComponent<WorldPanel>();
				wp.LookAtCamera = true;
				wp.RenderScale = 1.8f;
				wp.PanelSize = new Vector2( 1200, 500 );
			}

			if ( pickup.GetComponent<PickupHint>() is null )
			{
				var hint = pickup.AddComponent<PickupHint>();

				if ( pickup.GetComponent<ItemPickup>() is not ItemPickup itemPickup )
					return;

				if ( itemPickup.Item is null )
					return;

				hint?.SetItem( itemPickup );
			}
			else
				pickup.GetComponent<PickupHint>().TimeSincePlayerInRange = 0;
		}

		if ( navgen == null )
		{
			navgen = Scene.GetAllObjects( true ).FirstOrDefault( x => x.GetComponent<NavGenComponent>() != null ).GetComponent<NavGenComponent>();
			return;
		}

		if ( navgen != null && !navgen.Initialized )
		{
			if ( Transform.Position.z > 500 )
			{
				cc.Velocity -= Vector3.Up * Gravity * Time.Delta;
			}
			else
			{
				Transform.Position = Transform.Position.WithZ( 500 );
				cc.Velocity = Vector3.Zero;
			}
			cc.Move();
			cc.IsOnGround = false;
			var helper = new CitizenAnimationHelperScene( Body.GetComponent<AnimatedModelComponent>().SceneObject );
			helper.WithVelocity( cc.Velocity - Vector3.Up * Gravity );
			helper.IsGrounded = cc.IsOnGround;
			helper.HoldType = CitizenAnimationHelperScene.HoldTypes.None;

			if ( Camera == null )
			{
				Camera = Scene.GetAllObjects( true ).Where( X => X.GetComponent<CameraComponent>( true ) != null ).FirstOrDefault();
			}

			if ( Camera != null )
			{
				Camera.GetComponent<DepthOfField>().FocalDistance = Vector3.DistanceBetween( Camera.Transform.Position, Eye.Transform.Position );
			}
			return;
		}

		if ( Input.Pressed( "Flashlight" ) )
		{
			SpawnGlowstick();
		}

		if ( Input.Down( "Attack2" ) )
		{
			EyeAngles.pitch += Input.MouseDelta.y * 0.1f;
			EyeAngles.yaw -= Input.MouseDelta.x * 0.1f;

			if ( Input.Pressed( "Backward" ) && !CameraControl )
			{
				EyeAngles.yaw += 180f;
			}
		}
		else
		{
			if ( Input.Down( "Left" ) ) EyeAngles.yaw += Time.Delta * 90f * AimMultiplier;
			if ( Input.Down( "Right" ) ) EyeAngles.yaw -= Time.Delta * 90f * AimMultiplier;

			EyeAngles.pitch = 0;
		}

		EyeAngles.roll = 0;

		if ( Camera != null )
		{
			Camera.GetComponent<DepthOfField>().FocalDistance = Vector3.DistanceBetween( Camera.Transform.Position, Eye.Transform.Position );
		}

		if ( CameraControl )
		{
			// Update camera position

			if ( Camera is not null )
			{
				var camPos = Eye.Transform.Position - (EyeAngles.ToRotation() * Rotation.FromPitch( 15f )).Forward * CameraDistance;

				if ( FirstPerson ) camPos = Eye.Transform.Position + EyeAngles.ToRotation().Forward * 8;

				var tr = Physics.Trace.Ray( Eye.Transform.Position, camPos )
					.WithAnyTags( "solid" )
					.WithoutTags( "trigger" )
					.Radius( 16 )
					.Run();

				camPos = tr.EndPosition;

				var camrot = EyeAngles.ToRotation() * Rotation.FromPitch( 15f );

				if ( Input.Down( "Attack2" ) )
				{
					camPos += Eye.Transform.Rotation.Right * 16f;
					camrot *= Rotation.FromPitch( -5f );
				}

				Camera.Transform.Position = Vector3.Lerp( Camera.Transform.Position, camPos, Time.Delta * 10f );
				Camera.Transform.Rotation = Rotation.Lerp( Camera.Transform.Rotation, camrot, Time.Delta * 50f );
			}
		}

		CameraControl = true;

		// read inputs
		BuildWishVelocity();



		if ( Transform.Position.z < -300f )
		{
			var room = Scene.GetAllObjects( true ).Where( X => X.GetComponent<RoomChunkComponent>( false ) != null ).FirstOrDefault();

			if ( room is not null )
			{
				Transform.Position = room.Transform.Position.WithZ( 0 );
				WishVelocity = Vector3.Zero;
				cc.Velocity = Vector3.Zero;
			}
		}

		// rotate body to look angles
		if ( Body is not null )
		{
			Body.Transform.Rotation = new Angles( 0, EyeAngles.yaw, 0 ).ToRotation();

			var helper = new CitizenAnimationHelperScene( Body.GetComponent<AnimatedModelComponent>().SceneObject );
			helper.WithVelocity( cc.Velocity );
			helper.IsGrounded = cc.IsOnGround;

			if ( Input.Pressed( "Jump" ) && cc.IsOnGround )
			{
				helper.SpecialMove = CitizenAnimationHelperScene.SpecialMovement.Roll;
			}
			else
			{
				helper.SpecialMove = CitizenAnimationHelperScene.SpecialMovement.None;
			}

			Eye.Transform.Rotation = new Angles( EyeAngles.pitch, EyeAngles.yaw, 0 ).ToRotation();

			if ( Input.Down( "Attack2" ) )
			{
				AimMultiplier = 0.5f;

				var closest = GetClosestAimableObjectInViewcone();

				if ( closest != null )
				{
					Vector3 targetPos = closest.GetBounds().Center;
					Eye.Transform.Rotation = Rotation.LookAt( targetPos - Eye.Transform.Position, Vector3.Up );
					helper.WithLookAt( new Transform( Body.Transform.Position, Body.Transform.Rotation ), Eye.Transform.Position, targetPos );
				}
				else
				{
					helper.WithLookAt( new Transform( Body.Transform.Position, Body.Transform.Rotation ), Eye.Transform.Position, Eye.Transform.Position + Eye.Transform.Rotation.Forward * 100f );
				}
			}
			else
			{
				AimMultiplier = 1f;
				helper.WithLookAt( new Transform( Body.Transform.Position, Body.Transform.Rotation ), Eye.Transform.Position, Eye.Transform.Position + Eye.Transform.Rotation.Forward * 100f );
			}

			if ( Input.Down( "Duck" ) )
			{
				Eye.Transform.LocalPosition = EyeStartPos / 2f;

				helper.DuckLevel = 1f;
			}
			else
			{
				Eye.Transform.LocalPosition = EyeStartPos;

				helper.DuckLevel = 0f;
			}
		}

		if ( cc.IsOnGround && Input.Down( "Jump" ) )
		{
			float flGroundFactor = 1.0f;
			float flMul = 268.3281572999747f * 1.2f;
			//if ( Duck.IsActive )
			//	flMul *= 0.8f;

			//cc.Punch( Body.Transform.Rotation.Forward * flMul * flGroundFactor );

			if ( Input.Down( "Left" ) )
			{
				Body.Transform.Rotation *= Rotation.FromYaw( 90 );
			}

			if ( Input.Down( "Right" ) )
			{
				Body.Transform.Rotation *= Rotation.FromYaw( -90 );
			}

			if ( Input.Down( "Backward" ) )
			{
				Body.Transform.Rotation *= Rotation.FromYaw( 180 );

				if ( !CameraControl )
				{
					EyeAngles.yaw += 180;
					Eye.Transform.Rotation *= Rotation.FromYaw( 180 );
				}
			}

			cc.Velocity = Body.Transform.Rotation.Forward * flMul * flGroundFactor;


			cc.ApplyFriction( 0.1f );
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

		if ( Input.Down( "Run" ) ) WishVelocity *= 150.0f * AimMultiplier;
		else WishVelocity *= 85.0f * AimMultiplier;
	}
}
