using BrickJam.Game.UI;
using Sandbox;

namespace BrickJam.Game.Weapon;

public class PistolWeapon : BaseWeapon
{
	public override Model Model { get; set; } = Model.Load( "models/weapons/usp/usp.vmdl" );
	public override CitizenAnimationHelperScene.HoldTypes HoldType => CitizenAnimationHelperScene.HoldTypes.Pistol;
	public override CitizenAnimationHelperScene.Hand Handedness => CitizenAnimationHelperScene.Hand.Left;
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
	public override int MaxAmmo => 15;
	public override float Damage => 6f;
	public override float ReloadTime => 0.6f;

	internal PistolWeapon( bool enabled, string name ) : base( enabled, name )
	{
		defaultAmmoCount = MaxAmmo;
	}

	public ParticleSystem muzzleflash;

	public override void OnIdle( CitizenAnimationHelperScene helper )
	{
		helper.HoldType = HoldType;
		helper.Handedness = Handedness;
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

			Sound.FromWorld( "weapons/rust_pistol/sound/rust_pistol.shoot.sound", Transform.Position );
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

		if (ResourceBar.Instance is not null)
			ResourceBar.Instance.Ammo = AmmoCount;
	}
}
