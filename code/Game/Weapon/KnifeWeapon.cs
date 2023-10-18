using Sandbox;

namespace BrickJam.Game.Weapon;

public class KnifeWeapon : BaseWeapon
{
	public override Model Model { get; set; } = Model.Load( "models/items/knife/knife.vmdl" );
	public override CitizenAnimationHelperScene.HoldTypes HoldType => CitizenAnimationHelperScene.HoldTypes.HoldItem;
	public override CitizenAnimationHelperScene.Hand Handedness => CitizenAnimationHelperScene.Hand.Right;
	public override CitizenAnimationHelperScene.Hand AlternateHandedness => CitizenAnimationHelperScene.Hand.Right;
	public override bool CanAimFocus => true;
	public override int AmmoCount
	{
		get => base.AmmoCount;
		set
		{
			base.AmmoCount = value;
		}
	}
	public override int MaxAmmo => -1;
	public override float Damage => 12f;
	public override float ReloadTime => 0.6f;

	public override float TraceLength => 20f;

	internal KnifeWeapon( bool enabled, string name ) : base( enabled, name )
	{
		defaultAmmoCount = MaxAmmo;
	}

	public override void OnIdle( CitizenAnimationHelperScene helper )
	{
		helper.HoldType = HoldType;
		helper.Handedness = Handedness;
	}

	public override void OnPrimaryPressed( CitizenAnimationHelperScene helper )
	{
		if ( AmmoCount > 0 || AmmoCount == -1 )
		{
			helper.SetAnimParameter( "holdtype_attack", 2f );//Heavy attack
			helper.TriggerAttack();
			//Sound.FromWorld( "weapons/rust_pistol/sound/rust_pistol.shoot.sound", Transform.Position );
		}
		else
		{
			helper.TriggerReload();
		}
	}

	public override void OnSecondaryPressed( CitizenAnimationHelperScene helper )
	{

	}

	public override void OnSecondaryHeld( CitizenAnimationHelperScene helper )
	{
		if ( CanAimFocus )
			helper.Handedness = AlternateHandedness;
	}
}
