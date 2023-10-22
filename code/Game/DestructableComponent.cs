using System;
using System.Collections.Generic;
using BrickJam;
using BrickJam.Player;
using Sandbox;

public sealed class DestructableComponent : BaseComponent
{
	private List<InventoryItem> PotentialDrops { get; set; } = new();
	
	public override void OnStart()
	{
		// Destroy on death
		HealthComponent healthComponent = GameObject.GetComponent<HealthComponent>();
		healthComponent.OnDeath += OnDeath;
		healthComponent.OnDamage += OnDamaged;

		var invItems = ResourceLibrary.GetAll<InventoryItem>();
		PotentialDrops.AddRange( invItems );
	}

	public void OnDeath()
	{
		Sound.FromWorld( "barrel_break", Transform.Position );
		Log.Info( PotentialDrops[Random.Shared.Next(0, PotentialDrops.Count - 1)]  );
		GameObject.Destroy();
	}

	public void OnDamaged()
	{
		Log.Info( "Barrel damage" );
	}
}
