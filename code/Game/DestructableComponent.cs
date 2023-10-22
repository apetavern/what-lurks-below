using System;
using System.Collections.Generic;
using System.Linq;
using BrickJam;
using BrickJam.Game;
using BrickJam.Player;
using Sandbox;

public sealed class DestructableComponent : BaseComponent
{
	private List<InventoryItem> PotentialDrops { get; set; } = new();

	public override void OnStart()
	{
		if ( Game.Random.Float() > 0.3f )
		{
			GetComponent<ModelComponent>().SceneObject.SetMaterialGroup( "clean" );
		}
		else
		{
			GetComponent<ModelComponent>().SceneObject.SetMaterialGroup( "painted" );
		}
		// Destroy on death
		HealthComponent healthComponent = GameObject.GetComponent<HealthComponent>();
		healthComponent.OnDeath += OnDeath;
		healthComponent.OnDamage += OnDamaged;

		var invItems = ResourceLibrary.GetAll<InventoryItem>().Where( i => i.InRandomDrops );
		PotentialDrops.AddRange( invItems );
	}

	public void OnDeath()
	{
		Sound.FromWorld( "barrel_break", Transform.Position );
		var item = GetRandomItem();
		var pickup = new GameObject( false, $"Pickup {item.Name}" );
		var itemPickup = pickup.AddComponent<ItemPickup>( false );
		var collider = pickup.AddComponent<ColliderBoxComponent>();
		collider.IsTrigger = true;
		collider.Scale = new Vector3( 50 );
		collider.Tags += "pickup ";
		itemPickup.Item = item;
		pickup.Transform.Position = Transform.Position;
		itemPickup.Enabled = true;
		pickup.Enabled = true;

		// var pickup = new ItemPickup { Item = item, Transform = { Position = Transform.Position } };
		GameObject.Destroy();
	}

	private InventoryItem GetRandomItem()
	{
		var item = PotentialDrops[Random.Shared.Next( 0, PotentialDrops.Count - 1 )];
		if ( item.Name.Contains( "Ammo" ) )
		{
			item.Quantity = Random.Shared.Next( 1, 4 );
		}

		return item;
	}

	public void OnDamaged()
	{
		Log.Info( "Barrel damage" );
	}
}
