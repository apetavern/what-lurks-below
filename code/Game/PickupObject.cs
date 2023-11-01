using BrickJam.Components;
using BrickJam.Map;

namespace BrickJam;

public class PickupObject : GameObject
{
	protected ColliderBoxComponent c_BoxCollider;
	protected ItemPickup c_ItemPickup;

	public InventoryReference Item { get; set; }

	internal PickupObject( bool enabled, string name, Vector3 position, InventoryReference item ) : base( enabled, name )
	{
		SetParent( Scene );

		Transform.Position = position;
		Item = item;

		c_BoxCollider = AddComponent<ColliderBoxComponent>( false );
		c_ItemPickup = AddComponent<ItemPickup>( false );

		c_BoxCollider.Scale = new Vector3( 50f );
		c_BoxCollider.IsTrigger = true;
		Tags.Add( "pickup" );
		Tags.Add( "trigger" );
		c_BoxCollider.Enabled = true;

		c_ItemPickup.Item = item;
		c_ItemPickup.Enabled = true;

		MapGeneratorComponent.Instance.Pickups.Add( this );

		if ( GetComponent<WorldPanel>() is null )
		{
			var wp = AddComponent<WorldPanel>();
			wp.LookAtCamera = true;
			wp.RenderScale = 1.8f;
			wp.PanelSize = new Vector2( 1200, 500 );
		}

		if ( GetComponent<PickupHint>() is null )
		{
			var hint = AddComponent<PickupHint>();

			if ( GetComponent<ItemPickup>() is not ItemPickup itemPickup )
				return;

			if ( itemPickup.Item is null )
				return;

			hint?.SetItem( itemPickup );
		}
	}
}
