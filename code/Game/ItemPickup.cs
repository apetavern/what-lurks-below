using BrickJam.Player;
using Sandbox;
using System;
using System.Linq;
using BrickJam.Game.UI;

namespace BrickJam.Game;

[Title( "ItemPickup" )]
[Category( "Map" )]
[Icon( "colorize", "red", "white" )]
[EditorHandle( "materials/gizmo/items.png" )]
public class ItemPickup : BaseComponent
{
	public SceneModel SceneModel;
	private GameObject Player;
	private BrickPlayerController Controller;
	private float _sceneModelStartZ;
	private float _sceneModelEndZ;
	private float _sceneModelTargetZ;

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
		SceneModel = new SceneModel( Scene.SceneWorld, Item.GroundModel ?? Model.Load( "models/dev/box.vmdl" ), Transform.World );
		SceneModel.Position = Transform.Position + Vector3.Up * SceneModel.Bounds.Size;
		SceneModel.Transform = SceneModel.Transform.WithScale( Item.GroundScale );

		_sceneModelStartZ = SceneModel.Transform.Position.z;
		_sceneModelEndZ = SceneModel.Transform.Position.z + 20f;
		_sceneModelTargetZ = _sceneModelEndZ;
	}

	public override void OnDisabled()
	{
		base.OnDisabled();

		SceneModel?.Delete();
		SceneModel = null;
	}

	public override void Update()
	{
		if ( SceneModel != null )
		{
			SceneModel.Rotation = Rotation.From( 0, Time.Now * 50f, 0 );
			SceneModel.Position = Vector3.Lerp( SceneModel.Position, SceneModel.Position.WithZ( _sceneModelTargetZ ), 0.15f * Time.Delta );

			// Calculate distance & change target
			if ( Math.Abs( SceneModel.Position.z - _sceneModelTargetZ ) < 5 )
			{
				if ( Math.Abs( SceneModel.Position.z - _sceneModelEndZ ) < 5 )
					_sceneModelTargetZ = _sceneModelStartZ;

				if ( Math.Abs( SceneModel.Position.z - _sceneModelStartZ ) < 5 )
					_sceneModelTargetZ = _sceneModelEndZ;
			}

			// Arrived bottom
			if ( Math.Abs( SceneModel.Position.z - _sceneModelTargetZ ) < 5 )
			{
				_sceneModelTargetZ = _sceneModelEndZ;
			}

			SceneModel.Update( 0.1f );
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

		if ( Item.Stackable && inv.ItemInInventory( Item ) )
		{
			inv.UpdateExistingItem( Item );
		}
		else
		{
			var (hasSpace, invCoord) = inv.HasFreeSpace( Item.Length, Item.Height );
			Log.Info(hasSpace + ":" + invCoord  );
			if ( !hasSpace )
			{
				MessagePanel.Instance.AddMessage( $"Not enough space for {Item.Name} in inventory." );
				return;
			}

			var added = inv.PlaceItem( Item.ToReference(), invCoord );
			if ( !added )
				return;
		}


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
