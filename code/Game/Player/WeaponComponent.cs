using System.Linq;
using Sandbox;
using BaseWeapon = BrickJam.Game.Weapon.BaseWeapon;

namespace BrickJam.Player;

public class WeaponComponent : BaseComponent
{
	public BaseWeapon ActiveWeapon { get; set; }
	private GameObject _player;
	private GameObject _body;

	public static WeaponComponent Instance { get; set; }

	public override void OnStart()
	{
		base.OnStart();

		Instance = this;

		_player = Scene.GetAllObjects( true ).FirstOrDefault( p => p.Name == "player" );
		_body = _player?.GetComponent<BrickPlayerController>().Body;
		// Equip( new KnifeWeapon( true, "Knife" ) );
	}

	public void Equip( BaseWeapon weapon )
	{
		if ( ActiveWeapon is not null )
		{
			Holster();
		}

		ActiveWeapon = weapon;
		ActiveWeapon?.SetActive( _body );
	}

	public void Holster()
	{
		ActiveWeapon?.SetInactive();
		ActiveWeapon = null;
	}

	public override void Update()
	{
		var citizenModel = _body.GetComponent<AnimatedModelComponent>().SceneObject;
		var helper = new CitizenAnimationHelperScene( citizenModel );

		var ctrl = _player?.GetComponent<BrickPlayerController>();
		if ( ctrl is null || ctrl.IsDead )
			return;

		var charCtrl = _player?.GetComponent<CharacterController>();
		if ( charCtrl is null || charCtrl.IsOnGround == false )
			return;

		if ( ActiveWeapon is null )
			helper.HoldType = CitizenAnimationHelperScene.HoldTypes.None;

		ActiveWeapon?.OnIdle( helper );

		if ( Input.Pressed( "attack1" ) )
		{
			ActiveWeapon?.PrimaryFire( ctrl.Eye.Transform.Position, ctrl.Eye.Transform.Rotation.Forward );
			ActiveWeapon?.OnPrimaryPressed( helper );
		}
		if ( Input.Pressed( "attack2" ) )
		{
			ActiveWeapon?.SecondaryFire( ctrl.Eye.Transform.Position, ctrl.Eye.Transform.Rotation.Forward );
			ActiveWeapon?.OnSecondaryPressed( helper );
		}

		if ( Input.Down( "attack1" ) )
		{
			ActiveWeapon?.OnPrimaryHeld( helper );
		}
		if ( Input.Down( "attack2" ) )
		{
			ActiveWeapon?.OnSecondaryHeld( helper );
		}
	}
}
