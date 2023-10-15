using Sandbox;
using System.Collections.Generic;

public sealed class RoomChunkComponent : BaseComponent
{
	[Property] GameObject CameraTrigger { get; set; }
	public List<RoomChunkComponent> ConnectedRooms { get; private set; } = new List<RoomChunkComponent>(); // List to store connected rooms

	Color ThisColor;

	public override void OnAwake()
	{
		base.OnAwake();
		ThisColor = new Color( Game.Random.Float(), Game.Random.Float(), Game.Random.Float() ).WithAlpha( 1 );
	}

	public override void DrawGizmos()
	{
		foreach ( var item in ConnectedRooms )
		{
			Gizmo.Draw.Color = ThisColor;

			Gizmo.Draw.Line( Vector3.Zero, item.Transform.Position - Transform.Position );
		}
	}

	public void SetupCollision()
	{
		CameraTrigger.GetComponent<CameraTriggerComponent>( false ).RecalcBounds();
		GetComponent<ModelCollider>( false ).Enabled = true;
		Log.Info( "Collision fixed!" );
	}

	public bool connected;

	// Method to add a connection to another room
	public void AddConnection( RoomChunkComponent otherRoom )
	{
		if ( !connected )
		{
			if ( !ConnectedRooms.Contains( otherRoom ) && !otherRoom.ConnectedRooms.Contains( this ) )
			{
				ConnectedRooms.Add( otherRoom );
			}
		}
	}

	public override void Update()
	{
		base.Update();
	}
}
