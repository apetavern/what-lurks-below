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

	public InvCoord Position { get; set; }
	
	[ResourceType( "png" )]
	public string ImagePath { get; set; }
	
	public List<InventoryAction> InventoryActions { get; set; }
}

public enum InventoryAction
{
	Use,
	Equip,
	Drop,
	Examine,
};
