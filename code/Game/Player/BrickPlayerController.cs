using Sandbox;
using System;
using System.Linq;
using System.Collections.Generic;
using BrickJam.Components;
using BrickJam.Map;

namespace BrickJam.Player;

public class BrickPlayerController : SingletonComponent<BrickPlayerController>, INetworkSerializable
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

	public Vector3 startpos;

	TimeSince TimeSinceLastGlowstick;

	private AnimatedModelComponent modelComponent;
	private CharacterController characterController;

	[Broadcast]
	public void SpawnGlowstick()
	{
		if ( TimeSinceLastGlowstick > 1f )
		{
			var prefab = ResourceLibrary.Get<PrefabFile>( "prefabs/glowstick.object" );
			var bone = Body.GetComponent<AnimatedModelComponent>().SceneObject.GetBoneWorldTransform( "hold_R" );

			var go = SceneUtility.Instantiate( prefab.Scene, bone.Position, Rotation.FromPitch( 90 ) );

			go.GetComponent<GlowstickComponent>( false, true ).Velocity = Body.Transform.Rotation.Forward * 250f + Vector3.Up * 150f;
			go.GetComponent<GlowstickComponent>( false, true ).Player = GameObject;
			go.SetParent( MapGeneratorComponent.Instance.GeneratedMapParent );

			go.Network.Spawn();
		}
	}

	public override void OnEnabled()
	{
		base.OnEnabled();

		if ( IsProxy )
			return;

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
		healthComponent.OnDeath += OnDeath;

		modelComponent = Body.GetComponent<AnimatedModelComponent>();

		modelComponent.Set( "holdtype_pose_hand", 0.07f );
	}

	public void TakeDamage()
	{
		var helper = new CitizenAnimationHelperScene( Body.GetComponent<AnimatedModelComponent>().SceneObject );
		helper.TriggerHit();
	}

	public bool IsDead;

	IEnumerable<BoneCollection.Bone> ExcludedBones;

	List<Transform> LocalExcludedTransforms = new List<Transform>();

	public void OnDeath()
	{
		Stats.Save();
		Body.GetComponent<AnimatedModelComponent>().Set( "die", true );

		IsDead = true;
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

	public int BoneNameToIndex( string name )
	{
		return Body.GetComponent<AnimatedModelComponent>().Model.Bones.GetBone( name ).Index;
	}

	public override void FixedUpdate()
	{
		if ( IsProxy )
			return;

		BuildWishVelocity();


		if ( characterController is null )
			characterController = GetComponent<CharacterController>();

		var navgen = NavGenComponent.Instance;
		if ( navgen is null || !navgen.GameObject.IsValid() )
		{
			return;
		}

		if ( !navgen.Initialized )
		{
			if ( Transform.Position.z > 500 )
			{
				characterController.Velocity -= Vector3.Up * Gravity * Time.Delta;
			}
			else
			{
				Transform.Position = Transform.Position.WithZ( 500 );
				characterController.Velocity = Vector3.Zero;
			}
			characterController.Move();
			characterController.IsOnGround = false;
			var helper = new CitizenAnimationHelperScene( modelComponent.SceneObject );
			helper.WithVelocity( characterController.Velocity - Vector3.Up * Gravity );
			helper.IsGrounded = characterController.IsOnGround;
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
		else
		{
			navgen.SetInitialize();
		}

		characterController = GetComponent<CharacterController>();

		if ( characterController.IsOnGround && Input.Down( "Jump" ) )
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

			characterController.Velocity = Body.Transform.Rotation.Forward * flMul * flGroundFactor;


			characterController.ApplyFriction( 0.1f );
			//	cc.IsOnGround = false;

		}

		if ( characterController.IsOnGround )
		{
			characterController.Velocity = characterController.Velocity.WithZ( 0 );
			characterController.Accelerate( WishVelocity );
			characterController.ApplyFriction( 4.0f );
		}
		else
		{
			characterController.Velocity -= Gravity * Time.Delta * 0.5f;
			characterController.Accelerate( WishVelocity.ClampLength( 50 ) );
			characterController.ApplyFriction( 0.1f );
		}

		characterController.Move();

		if ( !characterController.IsOnGround )
		{
			characterController.Velocity -= Gravity * Time.Delta * 0.5f;
		}
		else
		{
			characterController.Velocity = characterController.Velocity.WithZ( 0 );
		}
	}

	CitizenAnimationHelperScene helper;

	bool helperInitialized;

	Vector3 LookAtPos;

	public override void Update()
	{
		if ( IsDead )
		{
			return;
		}

		if ( MapGeneratorComponent.Instance != null && MapGeneratorComponent.Instance.Pickups.Count > 0 )
		{
			foreach ( var pickup in MapGeneratorComponent.Instance.Pickups
				.Where( p => p.IsValid && p.Transform.Position.Distance( Transform.Position ) < 50 ) )
				pickup.GetComponent<PickupHint>().TimeSincePlayerInRange = 0;
		}


		if ( !IsProxy )
		{
			if ( Input.Pressed( "Flashlight" ) )
			{
				SpawnGlowstick();
			}


			EyeAngles.pitch += Input.MouseDelta.y * 0.1f;
			EyeAngles.yaw -= Input.MouseDelta.x * 0.1f;

			EyeAngles.roll = 0;

			if ( Camera.IsValid() )
			{
				Camera.GetComponent<DepthOfField>().FocalDistance = Vector3.DistanceBetween( Camera.Transform.Position, Eye.Transform.Position );
			}

			// Update camera position

			if ( Camera.IsValid() )
			{
				var camPos = Eye.Transform.Position - (EyeAngles.ToRotation() * Rotation.FromPitch( 15f )).Forward * CameraDistance;

				if ( FirstPerson ) camPos = Eye.Transform.Position + EyeAngles.ToRotation().Forward * 8;

				var tr = Scene.PhysicsWorld.Trace.Ray( Eye.Transform.Position, camPos )
					//.WithAnyTags( "solid" )//Add this back in later maybe, cba to go through and add solid tags everywhere
					.WithoutTags( "trigger" )
					.Radius( 16 )
					.Run();

				camPos = tr.EndPosition;

				var camrot = EyeAngles.ToRotation() * Rotation.FromPitch( 15f );

				//if ( Input.Down( "Attack2" ) )
				//{
				camPos += Eye.Transform.Rotation.Right * 16f;
				camrot *= Rotation.FromPitch( -5f );
				//}

				Camera.Transform.Position = camPos;// Vector3.Lerp( Camera.Transform.Position, camPos, Time.Delta * 10f );
				Camera.Transform.Rotation = camrot;// Rotation.Lerp( Camera.Transform.Rotation, camrot, Time.Delta * 50f );
			}

			if ( Transform.Position.z < -300f )
			{
				var room = Scene.GetAllObjects( true ).Where( X => X.GetComponent<RoomChunkComponent>( false ) != null ).FirstOrDefault();

				if ( room is not null )
				{
					Transform.Position = room.Transform.Position.WithZ( 0 );
					WishVelocity = Vector3.Zero;
					characterController.Velocity = Vector3.Zero;
				}
			}
		}

		// rotate body to look angles
		if ( Body is not null )
		{
			Body.Transform.Rotation = new Angles( 0, EyeAngles.yaw, 0 ).ToRotation();

			if ( !helperInitialized )
			{
				helper = new CitizenAnimationHelperScene( modelComponent.SceneObject );
				helperInitialized = true;
			}
			helper.WithVelocity( characterController.Velocity );
			helper.IsGrounded = characterController.IsOnGround;


			Eye.Transform.Rotation = new Angles( EyeAngles.pitch, EyeAngles.yaw, 0 ).ToRotation();

			if ( !IsProxy )
			{

				if ( Input.Pressed( "Jump" ) && characterController.IsOnGround )
				{
					helper.SpecialMove = CitizenAnimationHelperScene.SpecialMovement.Roll;
				}
				else
				{
					helper.SpecialMove = CitizenAnimationHelperScene.SpecialMovement.None;
				}

				if ( Input.Down( "Attack2" ) )
				{
					AimMultiplier = 0.5f;

					var closest = GetClosestAimableObjectInViewcone();

					if ( closest != null )
					{
						Vector3 targetPos = closest.GetBounds().Center;
						Eye.Transform.Rotation = Rotation.LookAt( targetPos - Eye.Transform.Position, Vector3.Up );

						LookAtPos = targetPos;

						helper.WithLookAt( Body.Transform.World, Eye.Transform.Position, LookAtPos );
					}
					else
					{
						LookAtPos = Eye.Transform.Position + Eye.Transform.Rotation.Forward * 100f;

						helper.WithLookAt( Body.Transform.World, Eye.Transform.Position, LookAtPos );
					}
				}
				else
				{
					LookAtPos = Eye.Transform.Position + Eye.Transform.Rotation.Forward * 100f;
					AimMultiplier = 1f;
					helper.WithLookAt( Body.Transform.World, Eye.Transform.Position, LookAtPos );
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
		}
	}

	public void BuildWishVelocity()
	{
		var rot = EyeAngles.ToRotation();

		WishVelocity = 0;

		if ( Input.Down( "Forward" ) ) WishVelocity += rot.Forward.WithZ( 0 );
		if ( Input.Down( "Backward" ) ) WishVelocity += rot.Backward.WithZ( 0 );

		if ( Input.Down( "Left" ) ) WishVelocity += rot.Left.WithZ( 0 );
		if ( Input.Down( "Right" ) ) WishVelocity += rot.Right.WithZ( 0 );


		WishVelocity = WishVelocity.WithZ( 0 );

		if ( !WishVelocity.IsNearZeroLength ) WishVelocity = WishVelocity.Normal;

		if ( Input.Down( "Run" ) ) WishVelocity *= 150.0f * AimMultiplier;
		else WishVelocity *= 85.0f * AimMultiplier;
	}

	public void Write( ref ByteStream stream )
	{
		stream.Write( EyeAngles );
		if ( characterController is null )
		{
			characterController = GetComponent<CharacterController>();
		}
		stream.Write( WishVelocity );
		stream.Write( helper.DuckLevel > 0f );
		stream.Write( LookAtPos );
	}

	public void Read( ByteStream stream )
	{

		EyeAngles = stream.Read<Angles>();

		if ( Body.IsValid() )
		{
			var helper = new CitizenAnimationHelperScene( Body.GetComponent<AnimatedModelComponent>().SceneObject );

			Vector3 movevec = stream.Read<Vector3>();
			helper.WithVelocity( movevec );

			float ducklevel = stream.Read<bool>() ? 1f : 0f;
			helper.DuckLevel = ducklevel;

			Vector3 lookpos = stream.Read<Vector3>();
			helper.WithLookAt( Body.Transform.World, Body.Transform.Position + Vector3.Up * 64f, lookpos );
		}
	}
}
