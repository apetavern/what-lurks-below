using System.Collections.Generic;
using System.Linq;
using BrickJam.Game;
using BrickJam.Game.Weapon;
using BrickJam.Player;
using Sandbox;

namespace BrickJam;

[GameResource( "Inventory Item Asset", "bjitem", "Describes an item" )]
public class InventoryItem : GameResource
{
	public string Name { get; set; }
	public float Weight { get; set; }
	public float SpawnWeight { get; set; } = 1f;
	public int Length { get; set; }
	public int Height { get; set; }

	public string ExamineText { get; set; }
	public int Quantity { get; set; }

	[ResourceType( "png" )]
	public string ImagePath { get; set; }

	[ResourceType( "vmdl" )]
	public Model GroundModel { get; set; }

	public float GroundScale { get; set; }

	public bool Stackable { get; set; }

	public List<InventoryAction> InventoryActions { get; set; }

	public void DoAction( Scene scene, InventoryAction action, InventoryReference item )
	{
		if ( action is InventoryAction.Examine )
		{
			MessagePanel.Instance.AddMessage( ExamineText );
		}
		else if ( action is InventoryAction.Drop )
		{
			// make all items droppable without requiring a unique pickup
			Inventory.Instance.RemoveItem( item );
			var player = scene
				.GetAllObjects( true )
				.FirstOrDefault( o => o.Name == "player" );
			if ( player is null )
				return;
			_ = new PickupObject( true, $"Pickup {item.Asset.Name}", player.Transform.Position, item );
			MessagePanel.Instance.AddMessage( $"You drop your {Name}." );
		}
		else if ( action is InventoryAction.Equip )
		{
			if ( Name == "Knife" )
			{
				var knife = new KnifeWeapon( true, "Knife" );
				EquipWeapon( scene, knife );
			}
			else if ( Name == "Pistol" )
			{
				var pistol = new PistolWeapon( true, "Pistol" );
				EquipWeapon( scene, pistol );
			}
			else if ( Name == "Shotgun" )
			{
				var shotgun = new ShotgunWeapon( true, "Shotgun" );
				EquipWeapon( scene, shotgun );
			}
			else
			{
				return;
			}
			MessagePanel.Instance.AddMessage( $"You equip your {Name}." );
		}
		else if ( action is InventoryAction.Use )
		{
			// manually check item type and hardcode behaviour
			if ( Name == "Medkit" )
			{
				var player = scene
					.GetAllObjects( true )
					.FirstOrDefault( o => o.Name == "player" );
				if ( player is null )
					return;
				var healthComponent = player.GetComponent<HealthComponent>();
				healthComponent.Health += 25f;
				MessagePanel.Instance.AddMessage( "You carefully heal your wounds a bit with the medkit." );
				Inventory.Instance.RemoveItem( item );
			}
		}
	}

	void EquipWeapon( Scene scene, Game.Weapon.BaseWeapon weapon )
	{
		var player = scene
			.GetAllObjects( true )
			.FirstOrDefault( o => o.Name == "player" );
		if ( player is null )
			return;
		var weaponComponent = player.GetComponent<WeaponComponent>();
		if ( weaponComponent.ActiveWeapon?.GetType() == weapon.GetType() )
			weaponComponent?.Holster();
		else
			weaponComponent?.Equip( weapon );
	}

	public InventoryReference ToReference()
	{
		return new InventoryReference()
		{
			Id = InventoryReference.IdIndex++,
			Asset = this,
			Position = InvCoord.None,
			Quantity = 0,
		};
	}
}

public class InventoryReference
{
	public static int IdIndex { get; set; } = 0;

	public int Id { get; set; }
	public InventoryItem Asset { get; set; }
	public InvCoord Position { get; set; }
	public int Quantity { get; set; }
}

public enum InventoryAction
{
	Use,
	Equip,
	Drop,
	Examine,
};
