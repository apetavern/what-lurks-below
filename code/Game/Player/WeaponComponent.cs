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
		ActiveWeapon = weapon;
		ActiveWeapon?.SetActive( _body );
	}

	public void Holster( BaseWeapon weapon )
	{
		ActiveWeapon?.SetInactive();
		ActiveWeapon = null;
	}

	public override void OnEnabled()
	{
		base.OnEnabled();
		
		
	}

	public override void Update()
	{
		if ( Input.Pressed( "attack1" ) )
		{
			ActiveWeapon?.OnPrimaryPressed();
		}
		else if ( Input.Pressed( "attack2" ) )
		{
			ActiveWeapon?.OnSecondaryPressed();
		}
	}
}
