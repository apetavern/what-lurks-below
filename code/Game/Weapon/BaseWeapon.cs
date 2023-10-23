using System;
using BrickJam.Player;
using Sandbox;

namespace BrickJam.Game.Weapon;

public enum WeaponAmmoType
{
	None,
	Pistol,
	Shotgun
}

public class BaseWeapon : GameObject
{
	public virtual Model Model { get; set; }
	public virtual CitizenAnimationHelperScene.HoldTypes HoldType => CitizenAnimationHelperScene.HoldTypes.None;
	public virtual CitizenAnimationHelperScene.Hand Handedness => CitizenAnimationHelperScene.Hand.Left;
	public virtual CitizenAnimationHelperScene.Hand AlternateHandedness => CitizenAnimationHelperScene.Hand.Left;
	public virtual bool CanAimFocus => false;

	public int defaultAmmoCount = 6;

	public int CurrentClip = 0;
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
				CurrentClip += Math.Min( AmmoCount, MaxAmmo ) + 1;
				AmmoCount -= CurrentClip;
				Reloading = false;
			}
			else
			{
				return;
			}
		}
		if ( CurrentClip > 0 || MaxAmmo == -1 )
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
					var damage = Damage + new Random().Int( -1, 1 );
					hitHealth.Damage( damage );

					CreateDamageToast( hitObject, tr.HitPosition, (damage - 0.5f).CeilToInt() );
				}
			}
			CurrentClip--;
		}
		else if ( AmmoCount > 0 )
		{
			//reload
			TimeSinceReloadStart = 0f;
			Reloading = true;
		}
	}

	protected void CreateDamageToast( GameObject gameObject, Vector3 hitPosition, int damage )
	{
		if ( gameObject is null )
			return;

		var damagePanel = gameObject.GetComponent<WorldPanel>();

		if ( damagePanel is null )
		{
			damagePanel = gameObject.AddComponent<WorldPanel>();
			damagePanel.LookAtCamera = true;
			damagePanel.RenderScale = 1f;
			damagePanel.PanelSize = new Vector2( 500, 1000 );
		}

		var verticalOffset = 20f;
		damagePanel.Position = hitPosition + Vector3.Up * verticalOffset;

		var damageToast = gameObject.AddComponent<DamageToast>();
		damageToast.CreateWithDamage( damage );
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
