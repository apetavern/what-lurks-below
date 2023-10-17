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
		Equip( new PistolWeapon( true, "Pistol", Scene ) );
	}

	public void Equip( BaseWeapon weapon )
	{
		if ( weapon.GetType() == ActiveWeapon?.GetType() )
		{
			Holster( ActiveWeapon );
			return;
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
		var citizenModel = _body.GetComponent<AnimatedModelComponent>().SceneModel;
		var helper = new CitizenAnimationHelperScene( citizenModel );

		if ( ActiveWeapon is null )
			helper.HoldType = CitizenAnimationHelperScene.HoldTypes.None;

		ActiveWeapon?.OnIdle( helper );

		if ( Input.Pressed( "attack1" ) )
		{
			var player = _player?.GetComponent<PlayerController>();
			ActiveWeapon?.PrimaryFire( player.Eye.Transform.Position, player.Eye.Transform.Rotation.Forward );
			ActiveWeapon?.OnPrimaryPressed( helper );
		}
		if ( Input.Pressed( "attack2" ) )
		{
			var player = _player?.GetComponent<PlayerController>();
			ActiveWeapon?.SecondaryFire( player.Eye.Transform.Position, player.Eye.Transform.Rotation.Forward );
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
