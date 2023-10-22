using Sandbox;
using System;
using System.Threading;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using BrickJam.Game;

public static class Vector3Extensions
{
	public static Vector3 Clamp( this Vector3 vector, float min, float max )
	{
		float x = Math.Clamp( vector.x, min, max );
		float y = Math.Clamp( vector.y, min, max );
		float z = Math.Clamp( vector.z, min, max );

		return new Vector3( x, y, z );
	}
}

[Title( "Map Generator" )]
[Category( "World" )]
[Icon( "map", "red", "white" )]
public partial class MapGeneratorComponent : BaseComponent
{
	List<string> Rooms = new List<string>() { "prefabs/rooms/sewer_room_01.object", "prefabs/rooms/sewer_room_02.object", "prefabs/rooms/sewer_room_03.object", "prefabs/rooms/sewer_room_04.object" };

	List<string> Hallways = new List<string>() { "prefabs/hallways/hallway_01.object", "prefabs/hallways/hallway_02.object", "prefabs/hallways/hallway_03.object", "prefabs/hallways/hallway_04.object" };

	List<RoomChunkComponent> SpawnedRooms = new List<RoomChunkComponent>();

	[Property] int RoomCount { get; set; } = 10;

	[Property] float SnapGridSize { get; set; } = 700f;


	[Property] public bool IsBossSequence { get; set; }


	public GameObject Player { get; set; }

	public GameObject SpawnPrefabFromPath( string path, Vector3 position, Rotation rotation )
	{
		var prefab = ResourceLibrary.Get<PrefabFile>( path );
		var go = SceneUtility.Instantiate( prefab.Scene, position, rotation );
		go.SetParent( GameObject.Children.First() );
		return go;
	}

	public void RegenMap()
	{
		var navgen = Scene.GetAllObjects( true ).FirstOrDefault( x => x.GetComponent<NavGenComponent>() != null ).GetComponent<NavGenComponent>();
		navgen.GenerationPlane.Enabled = true;

		navgen.GenerateMesh();

		navgen.Initialized = false;

		Player.Transform.Position = Player.GetComponent<BrickPlayerController>().startpos;

		var map = GameObject.Children.First().Children;
		for ( int i = 0; i < map.Count; i++ )
		{
			map[i].Destroy();
		}

		SpawnedRooms.Clear();
		CorrectedRooms = false;

		OnStart();
	}

	public override void OnStart()
	{

		Player = Scene.GetAllObjects( true ).FirstOrDefault( x => x.Name == "player" );

		SpawnedRooms.Add( SpawnPrefabFromPath( Rooms[0], Transform.Position, Transform.Rotation ).GetComponent<RoomChunkComponent>( false ) );

		SpawnedRooms[0].ClearEnemiesAndItems();

		if ( IsBossSequence )
		{
			Vector3 PlacePoint = new Vector3( 0, -2048f, 0 );

			SpawnedRooms.Add( SpawnPrefabFromPath( "prefabs/rooms/sewer_room_05.object", Transform.Position + PlacePoint, Transform.Rotation ).GetComponent<RoomChunkComponent>( false ) );
		}
		else
		{
			for ( int i = 0; i < RoomCount; i++ )
			{
				Vector2 SpawnPoint = Game.Random.VectorInCircle( 800 );

				Vector3 PlacePoint = new Vector3( SpawnPoint.x, SpawnPoint.y, 0 );

				SpawnedRooms.Add( SpawnPrefabFromPath( Rooms[Game.Random.Int( 0, Rooms.Count - 1 )], Transform.Position + PlacePoint, Transform.Rotation ).GetComponent<RoomChunkComponent>( false ) );
			}

			//spawn bossroom
			Vector3 BossPlacePoint = new Vector3( 0, -2048f, 0 );

			SpawnedRooms.Add( SpawnPrefabFromPath( "prefabs/rooms/sewer_room_05.object", Transform.Position + BossPlacePoint, Transform.Rotation ).GetComponent<RoomChunkComponent>( false ) );
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
					// Create a connection between the two rooms
					room.GetComponent<RoomChunkComponent>().AddConnection( neighbor.GetComponent<RoomChunkComponent>() );
					await GameTask.Delay( 10 );
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
				room.GetComponent<RoomChunkComponent>().AddConnection( closestNeighbor );
				await GameTask.Delay( 10 );
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
					await GameTask.Delay( 10 );
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

	List<GameObject> hallwayChunks = new List<GameObject>();

	public async void CreateHallways()
	{
		await GameTask.Delay( 7500 );
		int waitframes = 0;
		foreach ( var room in SpawnedRooms )
		{
			while ( room.PathPoints.Count == 0 && waitframes < 100 )
			{
				waitframes++;
				//Log.Info( "Waiting for hallways..." );
				await GameTask.Delay( 50 );
			}
			waitframes = 0;
			while ( room.PathPoints.Count == 1 && waitframes < 50 )
			{
				waitframes++;
				//Log.Info( "Waiting for hallways..." );
				await GameTask.Delay( 5 );
			}
			waitframes = 0;
			foreach ( var hall in room.PathPoints )
			{
				for ( int i = 0; i < hall.Count; i++ )
				{
					var tr = Physics.Trace.Ray( hall[i], hall[i] - Vector3.Up * 18f ).WithoutTags( "navgen" ).Run();
					if ( !tr.Hit && hall[i] != Vector3.Zero )
					{
						SpawnPrefabFromPath( Hallways[Game.Random.Int( 0, Hallways.Count - 1 )], hall[i], Rotation.Identity );
					}
					else if ( tr.Hit && hall[i] != Vector3.Zero )
					{
						List<Vector3> direction = new List<Vector3> { Vector3.Forward, Vector3.Backward, Vector3.Left, Vector3.Right };
						for ( int dir = 0; dir < direction.Count; dir++ )
						{
							var pos = hall[i] + direction[dir] * 128f;
							tr = Physics.Trace.Ray( pos, pos - Vector3.Up * 18f ).WithoutTags( "navgen" ).Run();
							if ( !tr.Hit )
							{
								SpawnPrefabFromPath( Hallways[Game.Random.Int( 0, Hallways.Count - 1 )], pos, Rotation.Identity );
							}
						}
					}
				}
			}
		}

		await GameTask.Delay( 200 );

		var navgen = Scene.GetAllObjects( true ).Where( X => X.GetComponent<NavGenComponent>() != null ).FirstOrDefault().GetComponent<NavGenComponent>();

		navgen.GenerationPlane.Enabled = false;

		await GameTask.Delay( 10 );

		hallwayChunks = Scene.GetAllObjects( true ).Where( X => X.GetComponent<HallwayChunkComponent>() != null ).ToList();

		foreach ( var item in hallwayChunks )
		{
			item.Transform.Rotation = Rotation.Identity;
			item.GetComponent<HallwayChunkComponent>( false ).CheckSides();
			await GameTask.Delay( 10 );
		}

		navgen.GenerateMesh();
		navgen.Initialized = true;
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
					if ( room == room2 || room == SpawnedRooms[0] || room == null ) continue;

					BBox CheckBox1 = new BBox( room.GameObject.GetBounds().Center, SnapGridSize );
					BBox CheckBox2 = new BBox( room2.GameObject.GetBounds().Center, SnapGridSize );

					if ( CheckBox1.Overlaps( CheckBox2 ) )
					{
						overlaps++;
						room.Transform.Position += (room.Transform.Position - room2.Transform.Position).WithZ( 0 ) * (1f / 90f) * 4f;

						Vector2 SpawnPoint = Game.Random.VectorInCircle( 128f );

						Vector3 OffsetPoint = new Vector3( SpawnPoint.x, SpawnPoint.y, 0 ) * (1f / 90f);

						room.Transform.Position += OffsetPoint;

						room.Transform.Position = Vector3Extensions.Clamp( room.Transform.Position, -3000f, 3000f );
					}
				}
			}
			if ( overlaps == 0 )
			{
				CorrectedRooms = true;
				foreach ( var room in SpawnedRooms )
				{
					if ( room == null ) continue;
					var GridSnappedPosition = room.Transform.Position;
					GridSnappedPosition.x = MathF.Round( GridSnappedPosition.x / SnapGridSize ) * SnapGridSize;
					GridSnappedPosition.y = MathF.Round( GridSnappedPosition.y / SnapGridSize ) * SnapGridSize;

					GridSnappedPosition.x += 64f;
					GridSnappedPosition.y += 64f;

					room.Transform.Position = GridSnappedPosition;
					room.GetComponent<RoomChunkComponent>( false ).SetupCollision();
				}

				GenerateRoomConnections();

				CreateHallways();
			}
		}

		if ( CorrectedRooms && !IsBossSequence && !Scene.GetAllObjects( true ).Where( x => x.GetComponent<EnemyController>() != null ).Any() )
		{
			RegenMap();
		}
	}
}
