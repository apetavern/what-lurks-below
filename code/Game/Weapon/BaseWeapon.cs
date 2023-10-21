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

	public int defaultAmmoCount = 6;

	public virtual int AmmoCount
	{
		get => defaultAmmoCount;
		set => defaultAmmoCount = value;
	}
	public virtual int MaxAmmo => 6;
	public virtual float Damage => 10f;
	public virtual float TraceLength => 1000f;

	public bool Reloading;

	public TimeSince TimeSinceReloadStart;
	public virtual float ReloadTime => 1f;

	protected AnimatedModelComponent c_AnimatedModel;


	public BBox LastHitBbox;


	internal BaseWeapon( bool enabled, string name ) : base( enabled, name )
	{
		c_AnimatedModel = AddComponent<AnimatedModelComponent>( false );
	}

	public void SetActive( GameObject parent )
	{
		c_AnimatedModel.GameObject.Parent = parent;
		c_AnimatedModel.Model = Model;
		c_AnimatedModel.Enabled = true;
		c_AnimatedModel.BoneMergeTarget = parent.GetComponent<AnimatedModelComponent>( false );
	}

	public void SetInactive()
	{
		c_AnimatedModel.Enabled = false;
	}

	public virtual void PrimaryFire( Vector3 position, Vector3 direction )
	{
		if ( Reloading )
		{
			if ( TimeSinceReloadStart > ReloadTime )
			{
				AmmoCount = MaxAmmo;
				Reloading = false;
			}
			else
			{
				return;
			}
		}
		if ( AmmoCount > 0 || AmmoCount == -1 )
		{
			var muzzle = c_AnimatedModel.GetAttachmentTransform( "muzzle" );

			var tr = Physics.Trace.Ray( muzzle.Position, muzzle.Position + direction.Normal * TraceLength )
				.WithAnyTags( "solid", "enemy" )
				.Run();

			if ( tr.Hit && tr.Body.GameObject is GameObject hitObject )
			{
				HealthComponent hitHealth = hitObject.Parent?.GetComponent<HealthComponent>() ?? null;

				LastHitBbox = hitObject.GetBounds();

				if ( hitHealth == null )
				{
					hitHealth = hitObject.GetComponent<HealthComponent>();
				}

				if ( hitHealth != null )
				{
					hitHealth.Damage( Damage );
				}
			}
			if ( AmmoCount > 0 )
			{
				AmmoCount--;
			}
		}
		else
		{
			//reload
			TimeSinceReloadStart = 0f;
			Reloading = true;
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
