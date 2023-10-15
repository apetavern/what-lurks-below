using System.Linq;
using BrickJam.Player;
using Sandbox;

namespace BrickJam.Game;

[Title( "ItemPickup" )]
[Category( "Map" )]
[Icon( "colorize", "red", "white" )]
[EditorHandle( "materials/gizmo/2dskybox.png" )]
public class ItemPickup : BaseComponent
{
	private BBox Bounds;

	private GameObject Player;

	private PlayerController Controller;

	private SceneModel _sceneModel;

	[Property] public Model Model { get; set; }

	[Property] public InventoryItem Item { get; set; }

	public override void OnEnabled()
	{
		base.OnEnabled();

		// Setup bounds
		Player = Scene.GetAllObjects( true ).FirstOrDefault( p => p.Name == "player" );
		Controller = Player?.GetComponent<PlayerController>();
		var box = new BBox();

		var scale = GetComponent<ColliderBoxComponent>( false ).Scale;
		var position = Transform.Position;

		box.Mins = position - 0.5f * scale;
		box.Maxs = position + 0.5f * scale;

		Bounds = box;

		// Setup scene model
		_sceneModel = new SceneModel( Scene.SceneWorld, Model ?? Model.Load( "models/dev/box.vmdl" ), Transform.World );
	}

	public override void OnDisabled()
	{
		base.OnDisabled();

		_sceneModel?.Delete();
		_sceneModel = null;
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

		if ( _sceneModel != null )
		{
			_sceneModel.Rotation = Rotation.From( 45f, Time.Now * 90f, 0 );

			_sceneModel.Position = Transform.Position + Vector3.Up * _sceneModel.Bounds.Size;

			_sceneModel.Update( 0.1f );
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
