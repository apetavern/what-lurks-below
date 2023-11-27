using System.Linq;
using Sandbox;
using BaseWeapon = BrickJam.Weapons.BaseWeapon;

namespace BrickJam.Player;

public class WeaponComponent : BaseComponent
{
	public BaseWeapon ActiveWeapon { get; set; }
	private GameObject _player;
	private GameObject _body;
	private AnimatedModelComponent modelComponent;
	private BrickPlayerController playerController;
	private CharacterController characterController;

	public override void OnStart()
	{
		_player = GameObject;
		playerController = _player.GetComponent<BrickPlayerController>();
		characterController = _player.GetComponent<CharacterController>();
		_body = playerController.Body;
		modelComponent = _body.GetComponent<AnimatedModelComponent>();
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
		var helper = new CitizenAnimationHelperScene( modelComponent.SceneObject );

		if ( playerController is null || playerController.IsDead )
			return;

		if ( characterController is null || characterController.IsOnGround == false )
			return;

		if ( ActiveWeapon is null )
			helper.HoldType = CitizenAnimationHelperScene.HoldTypes.None;


		ActiveWeapon?.OnIdle( ref helper );

		if ( IsProxy )
		{
			return;
		}

		if ( Input.Pressed( "attack1" ) )
		{
			ActiveWeapon?.PrimaryFire( playerController.Eye.Transform.Position, playerController.Eye.Transform.Rotation.Forward );
			ActiveWeapon?.OnPrimaryPressed( ref helper );
		}
		if ( Input.Pressed( "attack2" ) )
		{
			ActiveWeapon?.SecondaryFire( playerController.Eye.Transform.Position, playerController.Eye.Transform.Rotation.Forward );
			ActiveWeapon?.OnSecondaryPressed( ref helper );
		}

		if ( Input.Down( "attack1" ) )
		{
			ActiveWeapon?.OnPrimaryHeld( ref helper );
		}
		if ( Input.Down( "attack2" ) )
		{
			ActiveWeapon?.OnSecondaryHeld( ref helper );
		}
	}
}
