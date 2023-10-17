using BrickJam.Player;
using Sandbox;

namespace BrickJam.Game.Weapon;

public class BaseWeapon : GameObject
{
	public virtual Model Model { get; set; }
	public virtual CitizenAnimationHelperScene.HoldTypes HoldType => CitizenAnimationHelperScene.HoldTypes.None;
	public virtual CitizenAnimationHelperScene.Hand Handedness => CitizenAnimationHelperScene.Hand.Left;
	public virtual CitizenAnimationHelperScene.Hand AlternateHandedness => CitizenAnimationHelperScene.Hand.Left;
	public virtual bool CanAimFocus => false;

	public virtual float Damage => 10f;
	public virtual float TraceLength => 1000f;

	protected AnimatedModelComponent c_AnimatedModel;

	internal BaseWeapon( bool enabled, string name, Scene scene ) : base( enabled, name, scene )
	{
		c_AnimatedModel = AddComponent<AnimatedModelComponent>( false );
	}

	public void SetActive( GameObject parent )
	{
		c_AnimatedModel.GameObject.Parent = parent;
		c_AnimatedModel.Model = Model;
		c_AnimatedModel.Enabled = true;
	}

	public void SetInactive()
	{
		c_AnimatedModel.Enabled = false;
	}

	public virtual void PrimaryFire( Vector3 position, Vector3 direction )
	{
		var muzzle = c_AnimatedModel.GetAttachmentTransform( "muzzle" );
		// why's it on the floor
		Log.Info( muzzle.Position );
		
		var tr = Physics.Trace.Ray( muzzle.Position, muzzle.Position + direction * TraceLength )
			.WithoutTags( "trigger" )
			.Run();

		// Debug bc wtf
		if ( tr.Hit )
		{
			Log.Info( tr.Body.GameObject );
			Gizmo.Draw.LineThickness = 4f;
			Gizmo.Draw.Line( tr.StartPosition, tr.EndPosition );
		}

		if ( tr.Hit && tr.Body.GameObject is GameObject hitObject )
		{
			HealthComponent hitHealth = hitObject.GetComponent<HealthComponent>();
			if ( hitHealth != null )
			{
				hitHealth.Damage( Damage );

				Log.Info( "yay!" );
			}
		}
	}

	public virtual void SecondaryFire( Vector3 position, Vector3 direction )
	{

	}

	public virtual void OnIdle( CitizenAnimationHelperScene helper ) { }

	public virtual void OnPrimaryPressed( CitizenAnimationHelperScene helper ) { }

	public virtual void OnPrimaryHeld( CitizenAnimationHelperScene helper ) { }

	public virtual void OnSecondaryPressed( CitizenAnimationHelperScene helper ) { }

	public virtual void OnSecondaryHeld( CitizenAnimationHelperScene helper ) { }
}
