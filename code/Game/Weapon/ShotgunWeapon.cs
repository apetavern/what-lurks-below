using System;
using System.Linq;
using BrickJam.Player;
using Sandbox;

namespace BrickJam.Weapons;

public class ShotgunWeapon : BaseWeapon
{
	public static InventoryItem ShotgunItem => GetInventoryItem();

	public override Model Model { get; set; } = Model.Load( "weapons/rust_pumpshotgun/rust_pumpshotgun.vmdl" );
	public override CitizenAnimationHelperScene.HoldTypes HoldType => CitizenAnimationHelperScene.HoldTypes.Shotgun;
	public override CitizenAnimationHelperScene.Hand Handedness => CitizenAnimationHelperScene.Hand.Both;
	public override CitizenAnimationHelperScene.Hand AlternateHandedness => CitizenAnimationHelperScene.Hand.Both;
	public override bool CanAimFocus => true;
	public override int AmmoCount
	{
		get => Inventory.Instance.ShotgunAmmoCount;
		set
		{
			if ( Inventory.Instance.ShotgunAmmoItem is not null )
				Inventory.Instance.ShotgunAmmoItem.Quantity = value;
		}
	}
	public override int MaxAmmo => 6;
	public override float Damage => 8f;
	public override float ReloadTime => 0.75f;

	internal ShotgunWeapon( bool enabled, string name ) : base( enabled, name )
	{
		defaultAmmoCount = MaxAmmo;
	}

	private static InventoryItem GetInventoryItem()
	{
		return ResourceLibrary.GetAll<InventoryItem>().FirstOrDefault( i => i.Name == "Shotgun" );
	}

	public ParticleSystem muzzleflash;

	public override void OnIdle( ref CitizenAnimationHelperScene helper )
	{
		helper.HoldType = HoldType;
		helper.Handedness = Handedness;
	}

	public override void PrimaryFire( Vector3 position, Vector3 direction )
	{
		if ( Reloading )
		{
			if ( TimeSinceReloadStart > ReloadTime )
			{
				CurrentClip += Math.Min( AmmoCount, MaxAmmo ) + 1;
				AmmoCount -= CurrentClip - 1;
				Reloading = false;
			}
			else
			{
				return;
			}
		}
		if ( CurrentClip > 0 || AmmoCount == -1 )
		{
			var muzzle = c_AnimatedModel.GetAttachmentTransform( "muzzle" );

			var pelletcount = 5;

			float offsetMult = 150f;

			for ( int i = 0; i < pelletcount; i++ )
			{
				var tr = Physics.Trace.Ray( muzzle.Position, muzzle.Position + direction.Normal * TraceLength + Vector3.Random * offsetMult )
					.WithAnyTags( "solid", "enemy" )
					.Run();

				Gizmo.Draw.Line( tr.StartPosition, tr.EndPosition );

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

						if ( hitHealth.Health > 0 )
						{
							CreateDamageToast( hitObject, tr.HitPosition, (damage - 0.5f).CeilToInt() );
						}

						hitHealth.Damage( damage );
					}
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

	public override void OnPrimaryPressed( ref CitizenAnimationHelperScene helper )
	{
		if ( CurrentClip > 0 )
		{
			helper.TriggerAttack();
			var muzzletr = GetComponent<AnimatedModelComponent>().GetAttachmentTransform( "muzzle" );

			if ( muzzleflash == null )
			{
				muzzleflash = new GameObject( true, "flash" ).AddComponent<ParticleSystem>();
				muzzleflash.GameObject.SetParent( this );
				muzzleflash.Particles = Sandbox.ParticleSystem.Load( "particles/pistol_muzzleflash.vpcf" );
			}

			muzzleflash.Transform.Position = muzzletr.Position;
			muzzleflash.Transform.Rotation = muzzletr.Rotation;

			muzzleflash.OnEnabled();

			Sound.FromWorld( "weapons/rust_pumpshotgun/sounds/rust_pumpshotgun.shoot.sound", Transform.Position );
		}
		else if ( AmmoCount > 0 )
		{
			helper.TriggerReload();
		}
	}

	public override void OnSecondaryPressed( ref CitizenAnimationHelperScene helper )
	{
		// Log.Info( "Secondary Pressed - Pistol" );
	}

	public override void OnSecondaryHeld( ref CitizenAnimationHelperScene helper )
	{
		if ( CanAimFocus )
			helper.Handedness = AlternateHandedness;
	}

	protected override void Update()
	{
		base.Update();

		if ( ResourceBar.Instance is not null )
			ResourceBar.Instance.Ammo = AmmoCount;
	}
}
