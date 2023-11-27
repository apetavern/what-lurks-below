using System.Collections.Generic;
using System.Linq;
using BrickJam.Components;
using BrickJam.Weapons;
using Sandbox;

namespace BrickJam.Player;

[Category( "Player" )]
public class Inventory : SingletonComponent<Inventory>
{
	private List<InventoryReference> _items = new();

	public List<InventoryReference> Get => _items;

	private GameObject _player;
	private int ItemCount => _items.Count;
	[Property] public int Slots { get; set; }

	public int SlotsX { get; set; } = 6;
	public int SlotsY { get; set; } = 3;

	private bool[,] _inventorySlots;

	public int PistolAmmoCount => GetPistolAmmoCount();
	public int ShotgunAmmoCount => GetShotgunAmmoCount();
	public InventoryReference PistolAmmoItem => _items.FirstOrDefault( i => i.Asset.Name == "Pistol Ammo" );
	public InventoryReference ShotgunAmmoItem => _items.FirstOrDefault( i => i.Asset.Name == "Shotgun Ammo" );

	private WeaponComponent weaponComponent;

	private int GetPistolAmmoCount()
	{
		var pistolAmmo = PistolAmmoItem;
		if ( pistolAmmo is null )
		{
			return 0;
		}

		return pistolAmmo.Quantity;
	}

	private int GetShotgunAmmoCount()
	{
		var shotgunAmmo = ShotgunAmmoItem;
		if ( shotgunAmmo is null )
		{
			return 0;
		}

		return shotgunAmmo.Quantity;
	}

	public bool ItemInInventory( InventoryItem item )
	{
		return _items.Any( i => i.Asset.Name == item.Name );
	}

	public bool ItemInInventory( string itemName )
	{
		return _items.Any( i => i.Asset.Name == itemName );
	}

	public (bool, InvCoord) HasFreeSpace( int length, int height )
	{
		for ( var i = 0; i < _inventorySlots.GetLength( 1 ); i++ )
		{
			for ( var j = 0; j < _inventorySlots.GetLength( 0 ); j++ )
			{
				var currentPos = new InvCoord( j, i );
				var valid = CheckPositionValid( currentPos, length, height );
				if ( valid )
					return (true, currentPos);
			}
		}

		return (false, InvCoord.None);
	}

	bool CheckPositionValid( InvCoord pos, int length, int height )
	{
		if ( pos.X < 0 || pos.X + length > _inventorySlots.GetLength( 0 ) )
			return false;
		if ( pos.Y < 0 || pos.Y + height > _inventorySlots.GetLength( 1 ) )
			return false;

		for ( var i = pos.X; i < pos.X + length; i++ )
		{
			for ( var j = pos.Y; j < pos.Y + height; j++ )
			{
				if ( _inventorySlots[i, j] )
					return false;
			}
		}

		return true;
	}

	void Rent( InvCoord pos, int length, int height )
	{
		for ( var i = pos.X; i < pos.X + length; i++ )
		{
			for ( var j = pos.Y; j < pos.Y + height; j++ )
			{
				_inventorySlots[i, j] = true;
			}
		}
	}

	void Free( InvCoord pos, int length, int height )
	{
		for ( var i = pos.X; i < pos.X + length; i++ )
		{
			for ( var j = pos.Y; j < pos.Y + height; j++ )
			{
				_inventorySlots[i, j] = false;
			}
		}
	}

	public void UpdateExistingItem( InventoryReference item )
	{
		if ( item.Asset.Stackable )
		{
			var existingItem = _items.FirstOrDefault( i => i.Asset.Name == item.Asset.Name );
			if ( existingItem is not null )
			{
				existingItem.Quantity += item.Quantity;
			}
		}
	}

	public bool PlaceItem( InventoryReference item, InvCoord pos )
	{
		if ( _items.Contains( item ) )
		{
			_items.Remove( item );
			Free( item.Position, item.Asset.Length, item.Asset.Height );
		}

		var positionValid = CheckPositionValid( pos, item.Asset.Length, item.Asset.Height );
		if ( !positionValid )
		{
			_items.Add( item );
			Rent( item.Position, item.Asset.Length, item.Asset.Height );
			return false;
		}

		item.Position = pos;

		_items.Add( item );
		Rent( pos, item.Asset.Length, item.Asset.Height );
		return true;
	}

	public void RemoveItem( InventoryReference item )
	{
		Free( item.Position, item.Asset.Length, item.Asset.Height );

		_items.Remove( item );

		if ( !ItemInInventory( "Key" ) )
		{
			PlayerFlagsComponent.Instance.HasBossKey = false;
		}
	}

	public override void OnStart()
	{
		_inventorySlots = new bool[SlotsX, SlotsY];

		_player = GameObject;
		weaponComponent = _player.GetComponent<WeaponComponent>();

		PlaceItem( KnifeWeapon.KnifeItem.ToReference(), new InvCoord( 0, 0 ) );

		if ( !IsProxy )
		{
			GameHud.Instance.SetInventory( this );
		}
	}

	[Broadcast]
	public void Equip( string weaponname )
	{
		if ( weaponname == "Knife" )
		{
			weaponComponent.Equip( new KnifeWeapon( true, "Knife" ) );
		}

		if ( weaponname == "Pistol" )
		{
			weaponComponent.Equip( new PistolWeapon( true, "Pistol" ) );
		}

		if ( weaponname == "Shotgun" )
		{
			weaponComponent.Equip( new ShotgunWeapon( true, "Shotgun" ) );
		}
	}

	public override void Update()
	{
		if ( IsProxy )
		{
			return;
		}

		if ( Input.Pressed( "slot1" ) )
		{
			if ( _items.All( i => i.Asset.Name != "Knife" ) )
				return;
			//weaponComponent.Equip( new KnifeWeapon( true, "Knife" ) );
			Equip( "Knife" );
		}

		if ( Input.Pressed( "slot2" ) )
		{
			if ( _items.All( i => i.Asset.Name != "Pistol" ) )
				return;
			//weaponComponent.Equip( new PistolWeapon( true, "Pistol" ) );
			Equip( "Pistol" );
		}

		if ( Input.Pressed( "slot3" ) )
		{
			if ( _items.All( i => i.Asset.Name != "Shotgun" ) )
				return;
			//weaponComponent.Equip( new ShotgunWeapon( true, "Shotgun" ) );
			Equip( "Shotgun" );
		}
	}
}

public struct InvCoord
{
	public int X { get; set; }
	public int Y { get; set; }

	public InvCoord( int x, int y )
	{
		X = x;
		Y = y;
	}

	public static InvCoord None => new InvCoord( -1, 1 );

	public override string ToString()
	{
		return $"({X}, {Y})";
	}
}
