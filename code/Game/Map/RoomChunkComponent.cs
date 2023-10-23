using BrickJam.Game;
using Sandbox;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

public sealed class RoomChunkComponent : BaseComponent
{
	//[Property] GameObject CameraTrigger { get; set; }
	public List<RoomChunkComponent> ConnectedRooms { get; private set; } = new List<RoomChunkComponent>(); // List to store connected rooms

	Color ThisColor;

	public List<RoomDoorDefinition> Doors { get; set; }

	public List<List<Vector3>> PathPoints { get; set; } = new List<List<Vector3>>();

	public override void OnAwake()
	{
		base.OnAwake();
		ThisColor = new Color( Game.Random.Float(), Game.Random.Float(), Game.Random.Float() ).WithAlpha( 1 );

		Doors = GetComponents<RoomDoorDefinition>( false, true ).ToList();

	}

	public void ClearEnemiesAndItems()
	{
		var enemies = GameObject.GetComponents<EnemyController>( false, true );
		for ( int i = 0; i < enemies.Count(); i++ )
		{
			enemies.ElementAt( i ).GameObject.Destroy();
		}

		var items = GameObject.GetComponents<ItemPickup>( false, true );
		for ( int i = 0; i < items.Count(); i++ )
		{
			items.ElementAt( i ).GameObject.Destroy();
		}
	}

	public override void DrawGizmos()
	{
		Gizmo.Draw.Color = ThisColor;
		//foreach ( var item in ConnectedRooms )
		//{


		//Gizmo.Draw.Line( Vector3.Zero, item.Transform.Position - Transform.Position );
		//}

		if ( PathPoints.Count > 0 )
		{
			for ( int i = 0; i < PathPoints.Count; i++ )
			{
				for ( int j = 1; j < PathPoints[i].Count; j++ )
				{
					var localstart = PathPoints[i][j - 1] - Transform.Position;
					var localend = PathPoints[i][j] - Transform.Position;
					Gizmo.Draw.Line( localstart, localend );
				}
			}
		}
	}

	private RoomDoorDefinition ChooseClosestDoorForConnection( RoomChunkComponent room, Vector3 targetPosition )
	{
		float closestDistance = float.MaxValue;
		RoomDoorDefinition closestDoor = null;

		foreach ( RoomDoorDefinition door in room.Doors )
		{
			float distance = Vector3.DistanceBetween( door.Transform.Position, targetPosition );
			if ( distance < closestDistance )
			{
				closestDistance = distance;
				closestDoor = door;
			}
		}

		return closestDoor;
	}

	public async void ConnectRooms( RoomDoorDefinition doorPosition1, RoomDoorDefinition doorPosition2 )
	{
		// Calculate control points for the Bézier curve
		Vector3 controlPoint1 = doorPosition1.Transform.Position - doorPosition1.Transform.Rotation.Right * 64;//64
		Vector3 controlPoint2 = doorPosition2.Transform.Position - doorPosition2.Transform.Rotation.Right * 64;//64

		doorPosition1.OpenDoor();
		doorPosition2.OpenDoor();

		// Create a Bezier curve using the positions and control points

		NavGenComponent navgen = Scene.GetAllObjects( true ).Where( X => X.GetComponent<NavGenComponent>() != null ).FirstOrDefault().GetComponent<NavGenComponent>();

		var path = await navgen.GeneratePath( controlPoint1, controlPoint2 );

		if ( path.Count == 0 )
		{
			//Log.Info( "No navgen data" );

			//Log.Info( "Falling back to bezier curves" );

			List<Vector3> curvePoints = CreateBezierCurve( doorPosition1.Transform.Position, controlPoint1, controlPoint2, doorPosition2.Transform.Position, 128f );

			PathPoints.Add( EnforcePoints( curvePoints, 128f ) );
		}
		else
		{
			var PathResult = path;

			ProcessPath( PathResult );
		}
	}

	public async void ProcessPath( List<NavigationPath.Segment> PathResult )
	{
		var CurrentPath = new List<Vector3>();

		for ( int i = 0; i < PathResult.Count; i++ )
		{
			var pt = PathResult[i].Position;

			pt.x = MathF.Round( pt.x / 128f ) * 128f;
			pt.y = MathF.Round( pt.y / 128f ) * 128f;

			if ( CurrentPath.Count > 0 )
			{
				Vector3 prevPoint = CurrentPath[CurrentPath.Count - 1];
				Vector3 diff = pt - prevPoint;
				if ( MathF.Abs( diff.x ) >= MathF.Abs( diff.y ) )
				{
					pt.y = prevPoint.y;
				}
				else
				{
					pt.x = prevPoint.x;
				}
			}
			else
			{
				CurrentPath.Add( pt );
			}


			if ( !CurrentPath.Contains( pt ) )
				CurrentPath.Add( pt );
		}
		//CurrentPath = navgen.result;

		PathPoints.Add( EnforcePoints( CurrentPath, 128f ) );
	}

	private List<Vector3> EnforcePoints( List<Vector3> list, float gridSize )
	{
		List<Vector3> newPoints = new List<Vector3>();

		for ( int i = 1; i < list.Count; i++ )
		{
			Vector3 point1 = list[i - 1];
			Vector3 point2 = list[i];
			float distance = Vector3.DistanceBetween( point1, point2 );
			int numSteps = MathX.CeilToInt( distance / gridSize );

			if ( numSteps > 1 )
			{
				float stepSize = 1.0f / numSteps;
				for ( int j = 1; j < numSteps; j++ )
				{
					float t = j * stepSize;
					Vector3 newPoint = Vector3.Lerp( point1, point2, t );

					newPoint.x = MathF.Round( newPoint.x / gridSize ) * gridSize;
					newPoint.y = MathF.Round( newPoint.y / gridSize ) * gridSize;

					newPoints.Add( newPoint );
				}
			}
		}

		// Add the new intermediate points to the original list
		list.AddRange( newPoints );

		return list;
	}

	private List<Vector3> CreateBezierCurve( Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float gridSize )
	{
		List<Vector3> curve = new List<Vector3>();
		int numSegments = MathX.CeilToInt( Vector3.DistanceBetween( p0, p3 ) / gridSize );

		for ( int i = 0; i <= numSegments; i++ )
		{
			float t = i / (float)numSegments;
			float u = 1 - t;

			float tt = t * t;
			float uu = u * u;
			float uuu = uu * u;
			float ttt = tt * t;

			Vector3 point = uuu * p0 + 3 * uu * t * p1 + 3 * u * tt * p2 + ttt * p3;

			point.x = MathF.Round( point.x / gridSize ) * gridSize;
			point.y = MathF.Round( point.y / gridSize ) * gridSize;

			// Snap to 90-degree angles
			if ( i > 0 )
			{
				Vector3 prevPoint = curve[curve.Count - 1];
				Vector3 diff = point - prevPoint;
				if ( MathF.Abs( diff.x ) >= MathF.Abs( diff.y ) )
				{
					point.y = prevPoint.y;
				}
				else
				{
					point.x = prevPoint.x;
				}
			}

			curve.Add( point );
		}

		return curve;
	}

	public void SetupCollision()
	{
		foreach ( var childtrigger in GameObject.Children.Where( X => X.GetComponent<CameraTriggerComponent>( false, true ) != null ) )
		{
			childtrigger.GetComponent<CameraTriggerComponent>( false, true ).RecalcBounds();
		}
		//CameraTrigger.GetComponent<CameraTriggerComponent>( false ).RecalcBounds();

		/*foreach ( var coll in GetComponents<ModelCollider>( false, true ) )
		{
			coll.OnPhysicsChanged();
		}

		foreach ( var coll in GetComponents<ModelCollider>( false, true ) )
		{
			coll.Enabled = true;
		}*/
		// Log.Info( "Collision fixed!" );
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

				RoomDoorDefinition door1 = ChooseClosestDoorForConnection( this, otherRoom.Transform.Position );
				RoomDoorDefinition door2 = ChooseClosestDoorForConnection( otherRoom, door1.Transform.Position );
				ConnectRooms( door1, door2 );
			}
		}
	}

	public override void Update()
	{
		base.Update();
	}
}
