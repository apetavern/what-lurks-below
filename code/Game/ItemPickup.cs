using BrickJam.Player;
using Sandbox;
using System.Linq;
using BrickJam.Game.UI;

namespace BrickJam.Game;

[Title( "ItemPickup" )]
[Category( "Map" )]
[Icon( "colorize", "red", "white" )]
[EditorHandle( "materials/gizmo/items.png" )]
public class ItemPickup : BaseComponent
{
	private GameObject Player;
	private BrickPlayerController Controller;
	private SceneModel _sceneModel;

	[Property] public InventoryItem Item { get; set; }

	public override void OnEnabled()
	{
		base.OnEnabled();

		// Setup bounds
		Player = Scene.GetAllObjects( true ).FirstOrDefault( p => p.Name == "player" );
		Controller = Player?.GetComponent<BrickPlayerController>();

		var scale = GetComponent<ColliderBoxComponent>( false ).Scale;
		var position = Transform.Position;

		// Setup scene model
		_sceneModel = new SceneModel( Scene.SceneWorld, Item.GroundModel ?? Model.Load( "models/dev/box.vmdl" ), Transform.World );
	}

	public override void OnDisabled()
	{
		base.OnDisabled();

		_sceneModel?.Delete();
		_sceneModel = null;
	}

	public override void Update()
	{
		if ( _sceneModel != null )
		{
			_sceneModel.Rotation = Rotation.From( 0, Time.Now * 90f, 0 );
			_sceneModel.Position = Transform.Position + Vector3.Up * _sceneModel.Bounds.Size;
			_sceneModel.Transform = _sceneModel.Transform.WithScale( Item.GroundScale );

			_sceneModel.Update( 0.1f );
		}

		base.Update();
	}

	public void Triggered()
	{
		TriggerItem();
	}

	void TriggerItem()
	{
		var inv = Player.GetComponent<Inventory>();
		if ( inv is null )
			return;

		var (hasSpace, invCoord) = inv.HasFreeSpace( Item.Length, Item.Height );
		if ( !hasSpace )
		{
			MessagePanel.Instance.AddMessage( $"Not enough space for {Item.Name} in inventory." );
			return;
		}

		var added = inv.PlaceItem( Item, invCoord );
		if ( !added )
			return;

		Sound.FromScreen( "item_pickup" );
		Destroy();
	}

	public override void OnDestroy()
	{
		base.OnDestroy();

		// Remove the world panel immediately if we have one.
		GetComponent<WorldPanel>()?.Destroy();
	}
}
