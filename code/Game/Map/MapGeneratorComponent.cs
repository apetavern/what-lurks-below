using Sandbox;
using System;
using System.Threading;
using System.Collections.Generic;
using System.Threading.Tasks;

[Title( "Map Generator" )]
[Category( "World" )]
[Icon( "map", "red", "white" )]
public sealed class MapGeneratorComponent : BaseComponent
{
	List<string> Rooms = new List<string>() { "prefabs/rooms/room_01.object" };

	List<RoomChunkComponent> SpawnedRooms = new List<RoomChunkComponent>();

	[Property] int RoomCount { get; set; } = 10;

	[Property] float SnapGridSize { get; set; } = 700f;

	public override void OnStart()
	{
		var room1 = ResourceLibrary.Get<PrefabFile>( Rooms[0] );
		SpawnedRooms.Add( SceneUtility.Instantiate( room1.Scene, Transform.Position, Transform.Rotation ).GetComponent<RoomChunkComponent>( false ) );

		for ( int i = 0; i < RoomCount; i++ )
		{
			var room = ResourceLibrary.Get<PrefabFile>( Rooms[0] );

			Vector2 SpawnPoint = Game.Random.VectorInCircle( 800 );

			Vector3 PlacePoint = new Vector3( SpawnPoint.x, SpawnPoint.y, 0 );

			SpawnedRooms.Add( SceneUtility.Instantiate( room.Scene, Transform.Position + PlacePoint, Transform.Rotation ).GetComponent<RoomChunkComponent>( false ) );
		}

		base.OnStart();
	}

	bool CorrectedRooms;

	public async void GenerateRoomConnections()
	{
		Random random = new Random(); // Create a random number generator

		foreach ( var room in SpawnedRooms )
		{
			var position = room.Transform.Position;
			var gridX = MathF.Floor( position.x / SnapGridSize );
			var gridY = MathF.Floor( position.y / SnapGridSize );

			float closestDistance = float.MaxValue;

			RoomChunkComponent closestNeighbor = null;

			await GameTask.Delay( 10 );

			// Check for neighboring grid points and create connections
			foreach ( var neighbor in SpawnedRooms )
			{
				if ( neighbor == room || neighbor.ConnectedRooms.Contains( room ) ) continue; // Skip self and already-connected rooms

				var neighborPosition = neighbor.Transform.Position;
				var neighborGridX = MathF.Floor( neighborPosition.x / SnapGridSize );
				var neighborGridY = MathF.Floor( neighborPosition.y / SnapGridSize );

				if ( (gridX == neighborGridX && Math.Abs( gridY - neighborGridY ) == 1) ||
						(gridY == neighborGridY && Math.Abs( gridX - neighborGridX ) == 1) )
				{
					await GameTask.Delay( 10 );
					// Create a connection between the two rooms
					room.GetComponent<RoomChunkComponent>().AddConnection( neighbor.GetComponent<RoomChunkComponent>() );
				}

				// Calculate the distance between the current room and the neighbor
				float distance = Vector3.DistanceBetween( position, neighborPosition );

				if ( distance < closestDistance )
				{
					closestNeighbor = neighbor.GetComponent<RoomChunkComponent>();
					closestDistance = distance;
				}
			}

			// Randomly create a connection with the closest neighbor
			if ( closestNeighbor is not null )
			{
				await GameTask.Delay( 10 );
				room.GetComponent<RoomChunkComponent>().AddConnection( closestNeighbor );
			}
		}

		List<RoomChunkComponent> unconnectedRooms = new List<RoomChunkComponent>( SpawnedRooms );

		while ( unconnectedRooms.Count > 0 )
		{
			// Start with a random unconnected room
			var startingRoom = unconnectedRooms[random.Next( unconnectedRooms.Count )];

			List<RoomChunkComponent> connectedRooms = new List<RoomChunkComponent>();
			Stack<RoomChunkComponent> roomStack = new Stack<RoomChunkComponent>();

			roomStack.Push( startingRoom );
			connectedRooms.Add( startingRoom );
			unconnectedRooms.Remove( startingRoom );

			while ( roomStack.Count > 0 )
			{
				var currentRoom = roomStack.Pop();

				// Find unconnected neighboring rooms
				var unconnectedNeighbors = GetUnconnectedNeighbors( currentRoom, unconnectedRooms );

				if ( unconnectedNeighbors.Count > 0 )
				{
					// Randomly select one unconnected neighbor
					var randomIndex = random.Next( unconnectedNeighbors.Count );
					var randomNeighbor = unconnectedNeighbors[randomIndex];

					// Connect the current room to the selected neighbor
					currentRoom.AddConnection( randomNeighbor );

					// Mark the neighbor as connected
					connectedRooms.Add( randomNeighbor );
					unconnectedRooms.Remove( randomNeighbor );

					// Add the neighbor to the stack for further exploration
					roomStack.Push( randomNeighbor );
				}
			}

			await GameTask.Delay( 10 );
		}
	}

	private List<RoomChunkComponent> GetUnconnectedNeighbors( RoomChunkComponent room, List<RoomChunkComponent> unconnectedRooms )
	{
		List<RoomChunkComponent> unconnectedNeighbors = new List<RoomChunkComponent>();

		foreach ( var neighbor in room.GetComponent<RoomChunkComponent>().ConnectedRooms )
		{
			if ( unconnectedRooms.Contains( neighbor ) )
			{
				unconnectedNeighbors.Add( neighbor );
			}
		}

		return unconnectedNeighbors;
	}

	public override void Update()
	{
		if ( !CorrectedRooms )
		{
			int overlaps = 0;
			foreach ( var room in SpawnedRooms )
			{
				foreach ( var room2 in SpawnedRooms )
				{
					if ( room == room2 ) continue;
					if ( room.GameObject.GetBounds().Overlaps( room2.GameObject.GetBounds() ) )
					{
						overlaps++;
						room.Transform.Position += (room.Transform.Position - room2.Transform.Position) * 0.325f;
					}
				}
			}
			if ( overlaps == 0 )
			{
				CorrectedRooms = true;
				Log.Info( "Rooms corrected!" );
				foreach ( var room in SpawnedRooms )
				{
					var GridSnappedPosition = room.Transform.Position;
					GridSnappedPosition.x = MathF.Round( GridSnappedPosition.x / SnapGridSize ) * SnapGridSize;
					GridSnappedPosition.y = MathF.Round( GridSnappedPosition.y / SnapGridSize ) * SnapGridSize;
					room.Transform.Position = GridSnappedPosition;
					room.GetComponent<RoomChunkComponent>( false ).SetupCollision();
				}

				GenerateRoomConnections();
			}
		}
	}
}
