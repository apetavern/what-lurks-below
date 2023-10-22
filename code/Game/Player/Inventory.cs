using System.Collections.Generic;
using System.Linq;
using BrickJam.Game.UI;
using BrickJam.Game.Weapon;
using Sandbox;

namespace BrickJam.Player;

[Category( "Player" )]
public class Inventory : BaseComponent
{
	private List<InventoryItem> _items = new();

	public List<InventoryItem> Get => _items;

	private GameObject _player;
	private int ItemCount => _items.Count;
	[Property] public int Slots { get; set; }

	public int SlotsX { get; set; } = 6;
	public int SlotsY { get; set; } = 3;

	private bool[,] _inventorySlots;

	public int PistolAmmoCount => GetPistolAmmoCount();
	public int ShotgunAmmoCount => GetShotgunAmmoCount();
	public InventoryItem PistolAmmoItem => _items.FirstOrDefault( i => i.Name == "Pistol Ammo" );	
	public InventoryItem ShotgunAmmoItem =>_items.FirstOrDefault( i => i.Name == "Shotgun Ammo" );
	
	public static Inventory Instance { get; set; }

	public InventoryItem this[int key]
	{
		get => _items[key];
	}

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

	public bool AddItem( InventoryItem item )
	{
		if ( ItemCount >= Slots )
			return false;

		Log.Info( item?.Name );

		_items.Add( item );
		return true;
	}

	public bool ItemInInventory( InventoryItem item )
	{
		return _items.Any( i => i.Name == item.Name );
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

	public void UpdateExistingItem( InventoryItem item )
	{
		if ( item.Stackable )
		{
			var existingItem = _items.FirstOrDefault( i => i.Name == item.Name );
			if ( existingItem is not null )
			{
				existingItem.Quantity += item.Quantity;
			}
		}
	}

	public bool PlaceItem( InventoryItem item, InvCoord pos )
	{
		if ( _items.Contains( item ) )
		{
			_items.Remove( item );
			Free( item.Position, item.Length, item.Height );
		}

		var positionValid = CheckPositionValid( pos, item.Length, item.Height );
		if ( !positionValid )
		{
			_items.Add( item );
			Rent( item.Position, item.Length, item.Height );
			return false;
		}

		item.Position = pos;

		_items.Add( item );
		Rent( pos, item.Length, item.Height );
		return true;
	}

	public void RemoveItem( string itemName )
	{
		_items = _items.Where( i => i.Name != itemName ).ToList();
	}

	public InventoryItem GetItem( string itemName )
	{
		return _items.FirstOrDefault( i => i.Name == itemName );
	}

	public override void OnStart()
	{
		Instance = this;
		
		_inventorySlots = new bool[SlotsX, SlotsY];
		_player = Scene.GetAllObjects( true ).FirstOrDefault( p => p.Name == "player" );

		PlaceItem( KnifeWeapon.KnifeItem, new InvCoord( 0, 0 ) );
		PlaceItem( PistolWeapon.PistolItem, new InvCoord( 1, 0 ) );

		InventoryHud.Instance.SetInventory( this );
	}

	public override void Update()
	{
		var c_PlayerWeapon = _player?.GetComponent<WeaponComponent>();
		if ( c_PlayerWeapon is null )
			return;

		if ( Input.Pressed( "slot1" ) )
		{
			/* Future: Check if we have a pistol in our inventory.
			If so, we need to instantiate it with info from the Pistol InventoryItem. 
			If not, we do not equip the pistol at all. */
			c_PlayerWeapon.Equip( new KnifeWeapon( true, "Knife" ) );
		}

		if ( Input.Pressed( "slot2" ) )
		{
			/* Future: Check if we have a pistol in our inventory.
			If so, we need to instantiate it with info from the Pistol InventoryItem. 
			If not, we do not equip the pistol at all. */
			c_PlayerWeapon.Equip( new PistolWeapon( true, "pistol" ) );
		}

		if ( Input.Pressed( "slot3" ) )
		{
			/* Future: Check if we have a pistol in our inventory.
			If so, we need to instantiate it with info from the Pistol InventoryItem. 
			If not, we do not equip the pistol at all. */
			c_PlayerWeapon.Equip( new ShotgunWeapon( true, "Shotgun" ) );
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
