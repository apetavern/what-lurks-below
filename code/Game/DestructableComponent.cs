using System;
using System.Collections.Generic;
using System.Linq;
using BrickJam;
using BrickJam.Game;
using BrickJam.Player;
using Sandbox;

public sealed class DestructableComponent : BaseComponent
{
	public static List<InventoryItem> PotentialDrops { get; set; } = new();

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
		var drop = Random.Shared.Float( 0f, 1f );
		if ( drop > 0.5f )
		{
			var item = GetRandomItem();
			_ = new PickupObject( true, $"Pickup {item.Name}", Transform.Position, item );
		}
		GameObject.Destroy();
	}

	public static InventoryItem GetRandomItem()
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
		// Log.Info( "Barrel damage" );
	}
}
