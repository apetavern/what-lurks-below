using BrickJam.Player;
using Sandbox;
using System.Linq;

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
	private PickupResource _pickupResource;

	public enum PickupType
	{
		Ammo,
		Health,
		Item
	}

	[Property] public PickupType Type { get; set; } = PickupType.Ammo;
	[Property] public InventoryItem Item { get; set; }

	public override void OnEnabled()
	{
		base.OnEnabled();

		var pickupResources = ResourceLibrary.GetAll<PickupResource>();

		PickupResource pickup;
		if ( Type is PickupType.Item )
			pickup = pickupResources.FirstOrDefault( p =>
				p.PickupType == PickupType.Item && p.Item.Name == Item.Name );
		else
			pickup = pickupResources.FirstOrDefault( p => p.PickupType == Type );
		// var pickup = pickupResources.FirstOrDefault( p => p.PickupType == Type );
		//
		if ( pickup is null )
			return;

		_pickupResource = pickup;

		// Setup bounds
		Player = Scene.GetAllObjects( true ).FirstOrDefault( p => p.Name == "player" );
		Controller = Player?.GetComponent<BrickPlayerController>();

		var scale = GetComponent<ColliderBoxComponent>( false ).Scale;
		var position = Transform.Position;

		// Setup scene model
		_sceneModel = new SceneModel( Scene.SceneWorld, _pickupResource.Model ?? Model.Load( "models/dev/box.vmdl" ), Transform.World );
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
			_sceneModel.Transform = _sceneModel.Transform.WithScale( _pickupResource.Scale );

			_sceneModel.Update( 0.1f );
		}

		base.Update();
	}

	public void Triggered()
	{
		switch ( Type )
		{
			case PickupType.Ammo:
				break;
			case PickupType.Health:
				TriggerHealth();
				break;
			case PickupType.Item:
				TriggerItem();
				break;
		}
	}

	void TriggerHealth()
	{
		var pHealth = Player.GetComponent<HealthComponent>();
		pHealth.Health += 25f;

		Sound.FromScreen( "item_pickup" );
		Destroy();
	}

	void TriggerItem()
	{
		var inv = Player.GetComponent<Inventory>();
		if ( inv is null )
			return;

		var (hasSpace, invCoord) = inv.HasFreeSpace( _pickupResource.Item.Length, _pickupResource.Item.Height );
		if ( !hasSpace )
		{
			Log.Warning( $"Not enough space for {_pickupResource.Item.Name} in inventory." );
			return;
		}

		var added = inv.PlaceItem( _pickupResource.Item, invCoord );
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
