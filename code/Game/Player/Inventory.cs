using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace BrickJam.Player;

[Category("Player")]
public class Inventory : BaseComponent
{
	private List<InventoryItem> _items = new();
	
	public List<InventoryItem> Get => _items;

	private int ItemCount => _items.Count;
	[Property] public int Slots { get; set; }

	public InventoryItem this[ int key ]
	{
		get => _items[key];
	}

	public bool AddItem( InventoryItem item )
	{
		if ( ItemCount >= Slots )
			return false;
		
		Log.Info(item?.Name);
		
		_items.Add( item );
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
}
