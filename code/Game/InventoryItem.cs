using System;
using System.Collections.Generic;
using System.Linq;
using BrickJam.Game;
using BrickJam.Game.UI;
using BrickJam.Game.Weapon;
using BrickJam.Player;
using Sandbox;
using Sandbox.UI;

namespace BrickJam;

[GameResource("Inventory Item Asset", "bjitem", "Describes an item")]
public class InventoryItem : GameResource
{
	public string Name { get; set; }
	public float Weight { get; set; }
	public int Length { get; set; }
	public int Height { get; set; }
	
	public string ExamineText { get; set; }

	public InvCoord Position { get; set; }
	public int Quantity { get; set; }
	
	[ResourceType( "png" )]
	public string ImagePath { get; set; }
	
	[ResourceType("vmdl")]
	public Model GroundModel { get; set; }
	
	public float GroundScale { get; set; }
	
	public bool Stackable { get; set; }
	
	public List<InventoryAction> InventoryActions { get; set; }

	public void DoAction( Scene scene, InventoryAction action )
	{
		if ( action is InventoryAction.Examine )
		{
			MessagePanel.Instance.AddMessage( ExamineText );
		}
		else if ( action is InventoryAction.Drop )
		{
			// make all items droppable without requiring a unique pickup
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
		}
		else if ( action is InventoryAction.Use )
		{
			// manually check item type and hardcode behaviour
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
			weaponComponent?.Equip(weapon);
	}
}

public enum InventoryAction
{
	Use,
	Equip,
	Drop,
	Examine,
};
