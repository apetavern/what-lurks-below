using System.Linq;
using BrickJam.Player;
using Sandbox;

namespace BrickJam.Game;

[Title("ItemPickup")]
[Category("Map")]
[Icon("colorize", "red", "white")]
public class ItemPickup : BaseComponent
{
	private BBox Bounds;

	private GameObject Player;

	private PlayerController Controller;

	[Property] public InventoryItem Item { get; set; }

	public override void OnEnabled()
	{
		base.OnEnabled();
		Player = Scene.GetAllObjects( true ).FirstOrDefault( p => p.Name == "player" );
		Controller = Player?.GetComponent<PlayerController>();
		var box = new BBox();
		
		var scale = GetComponent<ColliderBoxComponent>().Scale;
		var position = Transform.Position;

		// Calculate the minimum and maximum points of the bounding box
		box.Mins = position - 0.5f * scale;
		box.Maxs = position + 0.5f * scale;

		Bounds = box;
	}

	public override void Update()
	{
		if ( Player is not null )
		{
			if ( Bounds.Contains( Player.Transform.Position ) )
			{
				Triggered();
			}
		}
		
		base.Update();
	}

	void Triggered()
	{
		var inv = Player.GetComponent<Inventory>();
		if ( inv is null )
			return;

		inv.AddItem( Item );
		Destroy();
	}
}
