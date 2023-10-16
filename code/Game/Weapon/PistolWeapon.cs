using Sandbox;

namespace BrickJam.Game.Weapon;

public class PistolWeapon : BaseWeapon
{
	public override Model Model { get; set; } = Model.Load( "weapons/rust_pistol/rust_pistol.vmdl" );
	
	internal PistolWeapon(bool enabled, string name, Scene scene) : base(enabled, name, scene)
	{
	}

	public override void OnPrimaryPressed()
	{
		Log.Info( "Primary Pressed - Pistol" );
	}

	public override void OnSecondaryPressed()
	{
		Log.Info( "Secondary Pressed - Pistol" );
	}
}
