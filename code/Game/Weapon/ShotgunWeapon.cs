using BrickJam.Game.UI;
using BrickJam.Player;
using Sandbox;

namespace BrickJam.Game.Weapon;

public class ShotgunWeapon : BaseWeapon
{
	public override Model Model { get; set; } = Model.Load( "weapons/rust_pumpshotgun/rust_pumpshotgun.vmdl" );
	public override CitizenAnimationHelperScene.HoldTypes HoldType => CitizenAnimationHelperScene.HoldTypes.Shotgun;
	public override CitizenAnimationHelperScene.Hand Handedness => CitizenAnimationHelperScene.Hand.Both;
	public override CitizenAnimationHelperScene.Hand AlternateHandedness => CitizenAnimationHelperScene.Hand.Both;
	public override bool CanAimFocus => true;
	public override int AmmoCount
	{
		get => base.AmmoCount;
		set
		{
			base.AmmoCount = value;
		}
	}
	public override int MaxAmmo => 6;
	public override float Damage => 6f;
	public override float ReloadTime => 0.75f;

	internal ShotgunWeapon( bool enabled, string name ) : base( enabled, name )
	{
		defaultAmmoCount = MaxAmmo;
	}

	public ParticleSystem muzzleflash;

	public override void OnIdle( CitizenAnimationHelperScene helper )
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
						hitHealth.Damage( Damage );
					}
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

	public override void OnPrimaryPressed( CitizenAnimationHelperScene helper )
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
		if ( AmmoCount > 0 )
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
		else
		{
			helper.TriggerReload();
		}
	}

	public override void OnSecondaryPressed( CitizenAnimationHelperScene helper )
	{
		Log.Info( "Secondary Pressed - Pistol" );
	}

	public override void OnSecondaryHeld( CitizenAnimationHelperScene helper )
	{
		if ( CanAimFocus )
			helper.Handedness = AlternateHandedness;
	}

	protected override void Update()
	{
		base.Update();

		Gizmo.Draw.LineBBox( LastHitBbox );

		if ( ResourceBar.Instance is not null )
			ResourceBar.Instance.Ammo = AmmoCount;
	}
}
