using System;
using System.Collections.Generic;
using System.Linq;
using BrickJam.Map;
using BrickJam.Player;
using Sandbox;

namespace BrickJam.Components;

public sealed class DestructableComponent : BaseComponent
{
	public static List<InventoryItem> PotentialDrops { get; set; } = new();
	public static List<float> PotentialDropWeights { get; set; } = new();

	[Property] public GameObject BreakParticle { get; set; }

	public override void OnStart()
	{
		Network.Spawn();
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

		PotentialDrops.Clear();
		PotentialDropWeights.Clear();
		var invItems = ResourceLibrary.GetAll<InventoryItem>().Where( i => i.SpawnWeight > 0f ).ToList();
		foreach ( var item in invItems )
		{
			PotentialDrops.Add( item );
			PotentialDropWeights.Add( item.SpawnWeight );
		}
	}

	public void OnDeath()
	{
		Sound.FromWorld( "barrel_break", Transform.Position );
		var drop = Game.Random.Float( 0f, 1f );
		if ( drop > 0.55f )
		{
			var item = GetRandomItem();
			//Log.Info( item );
			_ = new PickupObject( true, $"Pickup {item.Asset.Name}", Transform.Position, item );
		}

		BreakParticle.SetParent( MapGeneratorComponent.Instance.GeneratedMapParent );
		BreakParticle.Enabled = true;

		GameObject.Destroy();
	}

	public static InventoryReference GetRandomItem()
	{
		var itemResource = GetRandomFromArray( PotentialDrops, PotentialDropWeights );
		//Log.Info( itemResource );
		var item = itemResource.ToReference();
		var name = item.Asset.Name.ToLower();

		//Don't know why but these random values need +1 to be in range, might be an off by one error somewhere
		if ( name == "pistol ammo" )
		{
			item.Quantity = Game.Random.Next( 4, 8 ) + 1;
		}
		if ( name == "shotgun ammo" )
		{
			item.Quantity = Game.Random.Next( 2, 4 ) + 1;
		}
		else if ( name.Contains( "ammo" ) )
		{
			item.Quantity = Game.Random.Next( 2, 8 ) + 1;
		}

		return item;
	}

	static T GetRandomFromArray<T>( List<T> items, List<float> weights )
	{
		if ( items.Count != weights.Count )
		{
			Log.Error( "Items and weights must be the same length" );
			return default;
		}

		float totalWeight = 0;
		foreach ( var weight in weights )
		{
			totalWeight += MathF.Abs( weight );
		}

		float randomValue = Game.Random.Float( 0f, totalWeight );
		float cumulativeWeight = 0;

		for ( int i = 0; i < items.Count; i++ )
		{
			cumulativeWeight += weights[i];
			if ( randomValue < cumulativeWeight )
			{
				return items[i];
			}
		}

		return default;
	}
}
