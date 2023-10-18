using System;
using BrickJam.Game;
using BrickJam.Player;
using Sandbox;

namespace BrickJam;

[GameResource("Inventory Item Asset", "bjitem", "Describes an item")]
public class InventoryItem : GameResource
{
	public string Name { get; set; }
	public float Weight { get; set; }
	public int Length { get; set; }
	public int Height { get; set; }

	public InvCoord Position { get; set; }

	// public InventoryItem( string name, float weight, int length, int height )
	// {
	// 	Name = name;
	// 	Weight = weight;
	// 	Length = length;
	// 	Height = height;
	// }
	//
	// public static InventoryItem FromPickupType( ItemPickup.PickupType pickupType )
	// {
	// 	return pickupType switch
	// 	{
	// 		ItemPickup.PickupType.Ammo => new InventoryItem( "Ammo", 0.1f, 1, 1 ),
	// 		ItemPickup.PickupType.Health => new InventoryItem( "Health", 0.0f, 1, 1 ),
	// 		ItemPickup.PickupType.Key => new InventoryItem( "Key", 1.2f, 1, 1 ),
	// 		_ => new InventoryItem()
	// 	};
	// }
}
