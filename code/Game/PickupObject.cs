using BrickJam.Game.UI;

namespace BrickJam.Game;

public class PickupObject : GameObject
{
	protected ColliderBoxComponent c_BoxCollider;
	protected ItemPickup c_ItemPickup;
	
	public InventoryItem Item { get; set; }
	
	internal PickupObject(bool enabled, string name, Vector3 position, InventoryItem item) : base(enabled, name)
	{
		SetParent( Scene );
		
		Transform.Position = position;
		Item = item;
		
		c_BoxCollider = AddComponent<ColliderBoxComponent>( false );
		c_ItemPickup = AddComponent<ItemPickup>( false );

		c_BoxCollider.Scale = new Vector3( 50f );
		c_BoxCollider.IsTrigger = true;
		c_BoxCollider.Tags = "pickup";
		c_BoxCollider.Enabled = true;

		c_ItemPickup.Item = item;
		c_ItemPickup.Enabled = true;
	}
}
