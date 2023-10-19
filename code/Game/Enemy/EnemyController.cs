using BrickJam.Player;
using Sandbox;
using System.Linq;
using System;
using System.Collections.Generic;

namespace BrickJam.Game;

[Icon( "smart_toy", "red", "white" )]
public class EnemyController : BaseComponent
{
	[Property] public float Speed { get; set; } = 1f;
	[Property] public float Friction { get; set; } = 0.5f;
	[Property] public float AggroRange { get; set; } = 1f;

	[Property] public float AttackRange { get; set; } = 64f;

	[Property] public float AttackDamage { get; set; } = 1f;

	[Property] public float TimeBetweenAttacks { get; set; } = 1f;

	[Property] public GameObject Body { get; set; }

	public SceneModel model;

	public bool IsAggro = false;

	GameObject Player;
	CharacterController _characterController;

	List<Vector3> path { get; set; } = new List<Vector3>();

	int pathIndex { get; set; } = 0;

	NavGenComponent navgen { get; set; }

	public override void OnEnabled()
	{
		Player = Scene.GetAllObjects( true ).FirstOrDefault( x => x.Name == "player" );

		// Destroy on death
		HealthComponent healthComponent = GameObject.GetComponent<HealthComponent>();
		healthComponent.OnDeath += () => GameObject.Destroy();
		healthComponent.OnDamage += OnDamaged;

		var rng = new Random();

		Transform.Scale = Vector3.One * rng.Float( 0.9f, 1.3f );

		model = Body.GetComponent<AnimatedModelComponent>().SceneModel;

		navgen = Scene.GetAllObjects( true ).FirstOrDefault( x => x.GetComponent<NavGenComponent>() != null ).GetComponent<NavGenComponent>();
	}

	public TimeSince TimeSinceDamage;

	public void OnDamaged()
	{
		model.SetAnimParameter( "hit", true );
		_characterController.Velocity = -Body.Transform.Rotation.Forward * 100f;
		TimeSinceDamage = 0f;
	}

	float nextAttackTime;

	public async void UpdatePathToPlayer()
	{
		var result = await navgen.GeneratePath( Transform.Position, Player.Transform.Position );

		foreach ( var item in result )
		{
			path.Add( item.Position );
		}
	}

	public override void Update()
	{
		base.Update();

		if ( path.Count == 0 && navgen.Initialized )
		{
			path.Add( Transform.Position );
			UpdatePathToPlayer();
		}

		Vector3 myPosition = Transform.Position;
		Vector3 playerPosition = Player.Transform.Position;

		/*if ( path.Count > 1 )
		{
			playerPosition = path[pathIndex];

			if ( Vector3.DistanceBetween( Transform.Position, playerPosition ) < AggroRange )
			{
				pathIndex++;
				pathIndex = int.Clamp( pathIndex, 0, path.Count - 1 );
			}
		}*/

		IsAggro = (playerPosition.Distance( myPosition ) <= AggroRange) || TimeSinceDamage < 10f;

		_characterController ??= GameObject.GetComponent<CharacterController>();


		if ( IsAggro )
		{
			float distanceToPlayer = Vector3.DistanceBetween( myPosition, playerPosition );



			// Check if the player is within attack range
			if ( distanceToPlayer < AttackRange )
			{
				// Attack the player
				if ( Time.Now >= nextAttackTime )
				{
					Player.GetComponent<HealthComponent>().Damage( AttackDamage );
					model.SetAnimParameter( "attack", true );
					nextAttackTime = Time.Now + TimeBetweenAttacks;
				}
			}

			//Stop a little closer than the attack range to the player
			if ( distanceToPlayer > AttackRange - (AttackRange / 4f) )
			{
				// Move towards player when in range
				Vector3 direction = (playerPosition - myPosition).Normal;
				Vector3 wishVelocity = (direction * Speed).WithZ( 0 );
				_characterController.Accelerate( wishVelocity );
				_characterController.ApplyFriction( Friction );
			}
			else
			{
				_characterController.ApplyFriction( Friction / 3f );
			}
		}
		else
		{
			// Slow down when out of range
			_characterController.ApplyFriction( Friction / 3f );
		}

		_characterController.Move();

		// Rotate body towards target
		if ( _characterController.Velocity.LengthSquared > 0.1f )
		{
			model.SetAnimParameter( "moving", true );
			model.SetAnimParameter( "velocity", _characterController.Velocity.LengthSquared / 8000f );

			model.SetAnimParameter( "hastarget", true );

			SetAnimLookAt( "looktarget", new Transform( Body.Transform.Position, Body.Transform.Rotation ), model.GetAttachment( "eyes" ).Value.Position, playerPosition + Vector3.Up * 64f );

			Rotation targetRotation = Rotation.LookAt( playerPosition - myPosition, Vector3.Up );
			Body.Transform.Rotation = Rotation.Lerp( Body.Transform.Rotation, targetRotation, Time.Delta * 10f );
		}
	}

	public void SetAnimLookAt( string name, Transform origin, Vector3 eyePositionInWorld, Vector3 lookatPositionInWorld )
	{
		Vector3 value = (lookatPositionInWorld - eyePositionInWorld) * origin.Rotation.Inverse;
		model.SetAnimParameter( name, value );
	}

	public override void DrawGizmos()
	{
		base.DrawGizmos();

		if ( !Gizmo.IsSelected ) return;

		// Draw red sphere for aggro range
		Gizmo.Draw.Color = Color.Red;
		Gizmo.Draw.LineSphere( new() { Center = Vector3.Zero, Radius = AggroRange }, 8 );
	}
}
