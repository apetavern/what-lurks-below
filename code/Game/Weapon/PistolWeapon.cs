using System.Linq;
using BrickJam.Player;
using Sandbox;

namespace BrickJam.Weapons;

public class PistolWeapon : BaseWeapon
{
	public static InventoryItem PistolItem => GetInventoryItem();

	public override Model Model { get; set; } = Model.Load( "models/weapons/usp/usp.vmdl" );
	public override CitizenAnimationHelperScene.HoldTypes HoldType => CitizenAnimationHelperScene.HoldTypes.Pistol;
	public override CitizenAnimationHelperScene.Hand Handedness => CitizenAnimationHelperScene.Hand.Left;
	public override CitizenAnimationHelperScene.Hand AlternateHandedness => CitizenAnimationHelperScene.Hand.Both;
	public override bool CanAimFocus => true;
	public override int AmmoCount
	{
		get => Inventory.Instance.PistolAmmoCount;
		set
		{
			if ( Inventory.Instance.PistolAmmoItem is not null )
				Inventory.Instance.PistolAmmoItem.Quantity = value;
		}
	}
	public override int MaxAmmo => 15;
	public override float Damage => 15f;
	public override float ReloadTime => 0.6f;

	internal PistolWeapon( bool enabled, string name ) : base( enabled, name )
	{
		defaultAmmoCount = MaxAmmo;
	}

	private static InventoryItem GetInventoryItem()
	{
		return ResourceLibrary.GetAll<InventoryItem>().FirstOrDefault( i => i.Name == "Pistol" );
	}

	public ParticleSystem muzzleflash;

	public override void OnIdle( ref CitizenAnimationHelperScene helper )
	{
		helper.HoldType = HoldType;
		helper.Handedness = Handedness;
	}

	public override void OnPrimaryPressed( ref CitizenAnimationHelperScene helper )
	{
		if ( CurrentClip > 0 )
		{
			BrickPlayerController.Instance.TriggerAttack();
			var muzzletr = GetComponent<AnimatedModelComponent>().GetAttachmentTransform( "muzzle" );//.SceneObject.GetBoneWorldTransform( "hold_R" );

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
		if ( CurrentClip == 0 )
		{
			Sound.FromWorld( "sounds/player/gun_empty.sound", Transform.Position );
		}
		else if ( AmmoCount > 0 )
		{
			helper.TriggerReload();
		}
	}

	public override void OnSecondaryPressed( ref CitizenAnimationHelperScene helper )
	{
		//Log.Info( "Secondary Pressed - Pistol" );
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
			ResourceBar.Instance.Ammo = CurrentClip;
	}
}
