using Sandbox;

namespace BrickJam.Game.Weapon;

public class PistolWeapon : BaseWeapon
{
	public override Model Model { get; set; } = Model.Load( "weapons/rust_pistol/rust_pistol.vmdl" );
	public override CitizenAnimationHelperScene.HoldTypes HoldType => CitizenAnimationHelperScene.HoldTypes.Pistol;
	public override CitizenAnimationHelperScene.Hand Handedness => CitizenAnimationHelperScene.Hand.Left;
	public override CitizenAnimationHelperScene.Hand AlternateHandedness => CitizenAnimationHelperScene.Hand.Both;
	public override bool CanAimFocus => true;

	public override float Damage => 6f;

	internal PistolWeapon( bool enabled, string name, Scene scene ) : base( enabled, name, scene )
	{
	}

	public override void OnIdle( CitizenAnimationHelperScene helper )
	{
		helper.HoldType = HoldType;
		helper.Handedness = Handedness;
	}

	public override void OnPrimaryPressed( CitizenAnimationHelperScene helper )
	{
		helper.TriggerAttack();
		Sound.FromWorld( "weapons/rust_pistol/sound/rust_pistol.shoot.sound", Transform.Position );
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
}
