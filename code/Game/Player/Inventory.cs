using System.Collections.Generic;
using System.Linq;

namespace BrickJam.Player;

[Category("Player")]
public class Inventory : BaseComponent
{
	private List<InventoryItem> _items = new();
	
	public List<InventoryItem> Get => _items;

	public InventoryItem this[ int key ]
	{
		get => _items[key];
	}

	public void AddItem( InventoryItem item )
	{
		_items.Add( item );
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
