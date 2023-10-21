using System.Linq;
using BrickJam.Game.Weapon;
using Sandbox;
using BaseWeapon = BrickJam.Game.Weapon.BaseWeapon;

namespace BrickJam.Player;

public class WeaponComponent : BaseComponent
{
	public BaseWeapon ActiveWeapon { get; set; }
	private GameObject _player;
	private GameObject _body;

	public override void OnStart()
	{
		base.OnStart();

		_player = Scene.GetAllObjects( true ).FirstOrDefault( p => p.Name == "player" );
		_body = _player?.GetComponent<PlayerController>().Body;
		Equip( new KnifeWeapon( true, "Knife" ) );
	}

	public void Equip( BaseWeapon weapon )
	{
		if ( weapon.GetType() == ActiveWeapon?.GetType() )
		{
			Holster( ActiveWeapon );
			return;
		}
		else
		{
			Holster( ActiveWeapon );
		}

		ActiveWeapon = weapon;
		ActiveWeapon?.SetActive( _body );
	}

	public void Holster( BaseWeapon weapon )
	{
		ActiveWeapon?.SetInactive();
		ActiveWeapon = null;
	}

	public override void Update()
	{
		var citizenModel = _body.GetComponent<AnimatedModelComponent>().SceneObject;
		var helper = new CitizenAnimationHelperScene( citizenModel );

		var ctrl = _player?.GetComponent<PlayerController>();
		if ( ctrl is null )
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
