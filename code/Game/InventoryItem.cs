using System;
using BrickJam.Game;

namespace BrickJam;

public struct InventoryItem
{
	public string Name;
	public float Weight;
	public int Length;
	public int Height;

	public InventoryItem( string name, float weight, int length, int height )
	{
		Name = name;
		Weight = weight;
		Length = length;
		Height = height;
	}

	public static InventoryItem FromPickupType( ItemPickup.PickupType pickupType )
	{
		return pickupType switch
		{
			ItemPickup.PickupType.Ammo => new InventoryItem( "Ammo", 0.1f, 1, 1 ),
			ItemPickup.PickupType.Health => new InventoryItem( "Health", 0.0f, 1, 1 ),
			ItemPickup.PickupType.Key => new InventoryItem( "Key", 1.2f, 1, 1 ),
			_ => new InventoryItem()
		};
	}
}
