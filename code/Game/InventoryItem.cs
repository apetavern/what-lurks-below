using System;
using System.Collections.Generic;
using BrickJam.Game;
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
	
	[ResourceType( "png" )]
	public string ImagePath { get; set; }
	
	public List<InventoryAction> InventoryActions { get; set; }

	public void DoAction( InventoryAction action )
	{
		if ( action is InventoryAction.Examine )
		{
			Log.Info( ExamineText );
		}
		else if ( action is InventoryAction.Drop )
		{
			// make all items droppable without requiring a unique pickup
		}
		else if ( action is InventoryAction.Equip )
		{
			// manually check item type and hardcode correlation to weapon class
			// then equip
		}
		else if ( action is InventoryAction.Use )
		{
			// manually check item type and hardcode behaviour
		}
	}
}

public enum InventoryAction
{
	Use,
	Equip,
	Drop,
	Examine,
};
