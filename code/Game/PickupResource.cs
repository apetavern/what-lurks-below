using Sandbox;

namespace BrickJam.Game;

[GameResource("Pickup Asset", "bjpck", "Describes a pickup")]
public class PickupResource : GameResource
{
	[Category( "Meta" )] public ItemPickup.PickupType PickupType { get; set; } = ItemPickup.PickupType.Ammo;
	[Category( "Meta" )] public float Scale { get; set; } = 1f;
	[Category( "Meta" )] public Model Model { get; set; }
	[Category( "Meta" )] public InventoryItem Item { get; set; }
}
