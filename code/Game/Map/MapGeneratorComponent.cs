using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Immutable;
using BrickJam.Player;
using BrickJam.Components;
using Coroutines.Stallers;
using Coroutines;

namespace BrickJam.Map;

[Title( "Map Generator" )]
[Category( "World" )]
[Icon( "map", "red", "white" )]
public partial class MapGeneratorComponent : SingletonComponent<MapGeneratorComponent>
{
	private const string BossRoom = "prefabs/rooms/sewer_room_05.object";

	protected override bool NonProxyOnly => false;

	[Property] private string[] Rooms { get; set; }
	[Property] private string[] Hallways { get; set; }

	public ImmutableArray<GameObject> Containers { get; private set; } = ImmutableArray<GameObject>.Empty;
	public List<PickupObject> Pickups { get; private set; } = new();

	[Property] public GameObject GeneratedMapParent { get; set; }

	[Property] private int RoomCount { get; set; } = 10;
	[Property] private float SnapGridSize { get; set; } = 700f;
	[Property] private bool IsBossSequence { get; set; }

	[Property] private GameObject Breakables1 { get; set; }
	[Property] private GameObject Breakables2 { get; set; }
	[Property] private GameObject Breakables3 { get; set; }

	private ImmutableArray<RoomChunkComponent> spawnedRooms = ImmutableArray<RoomChunkComponent>.Empty;
	private ImmutableArray<GameObject> hallwayChunks = ImmutableArray<GameObject>.Empty;

	private GameObject player;
	private bool correctedRooms;

	public override void OnStart()
	{
		if ( IsProxy )
		{
			return;
		}
		player = BrickPlayerController.Instance.Player;

		var spawnedRoomsBuilder = ImmutableArray.CreateBuilder<RoomChunkComponent>();
		spawnedRoomsBuilder.Add( SpawnPrefabFromPath( Rooms[0], Transform.Position, Transform.Rotation ).GetComponent<RoomChunkComponent>( false ) );

		if ( IsBossSequence )
		{
			Vector3 PlacePoint = new Vector3( 0, -2048f, 0 );

			spawnedRoomsBuilder.Add( SpawnPrefabFromPath( BossRoom, Transform.Position + PlacePoint, Transform.Rotation ).GetComponent<RoomChunkComponent>( false ) );
		}
		else
		{
			for ( int i = 0; i < RoomCount; i++ )
			{
				Vector2 SpawnPoint = Game.Random.VectorInCircle( 800 );

				Vector3 PlacePoint = new Vector3( SpawnPoint.x, SpawnPoint.y, 0 );

				spawnedRoomsBuilder.Add( SpawnPrefabFromPath( Rooms[Game.Random.Int( 0, Rooms.Length - 1 )], Transform.Position + PlacePoint, Transform.Rotation ).GetComponent<RoomChunkComponent>( false ) );
			}

			//spawn bossroom
			Vector3 BossPlacePoint = new Vector3( 0, -2048f, 0 );

			spawnedRoomsBuilder.Add( SpawnPrefabFromPath( BossRoom, Transform.Position + BossPlacePoint, Transform.Rotation ).GetComponent<RoomChunkComponent>( false ) );
		}

		spawnedRoomsBuilder.Capacity = spawnedRoomsBuilder.Count;
		spawnedRooms = spawnedRoomsBuilder.MoveToImmutable();
	}

	public override void Update()
	{
		if ( IsProxy )
		{
			correctedRooms = true;
			return;
		}
		if ( correctedRooms || spawnedRooms.IsEmpty )
			return;

		int overlaps = 0;
		foreach ( var room in spawnedRooms )
		{
			BBox checkBox1 = new BBox( room.GameObject.GetBounds().Center, SnapGridSize );

			foreach ( var room2 in spawnedRooms )
			{
				if ( room == room2 || room == spawnedRooms[0] || room == null ) continue;

				BBox checkBox2 = new BBox( room2.GameObject.GetBounds().Center, SnapGridSize );
				if ( !checkBox1.Overlaps( checkBox2 ) )
					continue;

				overlaps++;
				room.Transform.Position += (room.Transform.Position - room2.Transform.Position).WithZ( 0 ) * (1f / 90f) * 4f;

				Vector2 SpawnPoint = Game.Random.VectorInCircle( 128f );

				Vector3 OffsetPoint = new Vector3( SpawnPoint.x, SpawnPoint.y, 0 ) * (1f / 90f);

				room.Transform.Position += OffsetPoint;

				room.Transform.Position = room.Transform.Position.Clamp( -3000, 3000 );

				checkBox1 = new BBox( room.GameObject.GetBounds().Center, SnapGridSize );
			}
		}

		if ( overlaps != 0 )
			return;

		correctedRooms = true;
		foreach ( var room in spawnedRooms )
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
		Coroutine.Start( CreateHallwaysCoroutine );
	}

	public CoroutineMethod RegenMapCoroutine()
	{
		var map = GeneratedMapParent.Children;
		for ( int i = 0; i < map.Count; i++ )
		{
			map[i].Destroy();
		}

		spawnedRooms = ImmutableArray<RoomChunkComponent>.Empty;
		correctedRooms = false;

		yield return new WaitForSeconds( 0.1f );

		var navgen = NavGenComponent.Instance;
		navgen.GenerationPlane.Enabled = true;
		navgen.GenerateMesh();
		navgen.Initialized = false;

		player.Transform.Position = player.GetComponent<BrickPlayerController>().startpos;

		OnStart();
	}

	private void GenerateRoomConnections()
	{
		var random = new Random( (int)Time.Now );

		foreach ( var room in spawnedRooms )
		{
			var position = room.Transform.Position;
			var gridX = MathF.Floor( position.x / SnapGridSize );
			var gridY = MathF.Floor( position.y / SnapGridSize );

			float closestDistance = float.MaxValue;

			RoomChunkComponent closestNeighbor = null;

			// Check for neighboring grid points and create connections
			foreach ( var neighbor in spawnedRooms )
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
			}
		}

		List<RoomChunkComponent> unconnectedRooms = new List<RoomChunkComponent>( spawnedRooms );

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

				if ( unconnectedNeighbors.Length > 0 )
				{
					// Randomly select one unconnected neighbor
					var randomIndex = random.Next( unconnectedNeighbors.Length );
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
		}
	}

	private CoroutineMethod CreateHallwaysCoroutine()
	{
		int waitframes = 0;
		foreach ( var room in spawnedRooms )
		{
			while ( room.PathPoints.Count == 0 && waitframes < 100 )
			{
				waitframes++;
				yield return new WaitForSeconds( 0.05f );
			}
			waitframes = 0;
			while ( room.PathPoints.Count == 1 && waitframes < 50 )
			{
				waitframes++;
				yield return new WaitForSeconds( 0.005f );
			}

			foreach ( var hall in room.PathPoints )
			{
				for ( int i = 0; i < hall.Count; i++ )
				{
					var tr = Physics.Trace.Ray( hall[i], hall[i] - Vector3.Up * 18f ).WithoutTags( "navgen" ).Run();
					if ( !tr.Hit && hall[i] != Vector3.Zero )
					{
						SpawnPrefabFromPath( Hallways[Game.Random.Int( 0, Hallways.Length - 1 )], hall[i], Rotation.Identity );
						continue;
					}

					if ( !tr.Hit || hall[i] == Vector3.Zero )
						continue;

					List<Vector3> direction = new List<Vector3> { Vector3.Forward, Vector3.Backward, Vector3.Left, Vector3.Right };
					for ( int dir = 0; dir < direction.Count; dir++ )
					{
						var pos = hall[i] + direction[dir] * 128f;
						tr = Physics.Trace.Ray( pos, pos - Vector3.Up * 18f ).WithoutTags( "navgen" ).Run();
						if ( tr.Hit )
							continue;

						SpawnPrefabFromPath( Hallways[Game.Random.Int( 0, Hallways.Length - 1 )], pos, Rotation.Identity );
						if ( Game.Random.Float() <= 0.95f )
							continue;

						List<GameObject> breakablesToSpawn = new();
						if ( Breakables1 != null ) breakablesToSpawn.Add( Breakables1 );
						if ( Breakables2 != null ) breakablesToSpawn.Add( Breakables2 );
						if ( Breakables3 != null ) breakablesToSpawn.Add( Breakables3 );

						if ( breakablesToSpawn.Count <= 0 )
							continue;

						var breakable = breakablesToSpawn[new Random().Int( 0, breakablesToSpawn.Count - 1 )];
						var breaky = SceneUtility.Instantiate( breakable, pos + Vector3.Random.WithZ( 0 ) * 64f, Rotation.LookAt( Vector3.Random.WithZ( 0 ) * 64f ) );
						breaky.SetParent( GeneratedMapParent );
						breaky.Network.Spawn();
					}
				}
			}
		}

		Containers = Scene.GetAllObjects( true )
			.Where( x => x.GetComponent<ItemContainer>() is not null )
			.ToImmutableArray();

		var navgen = NavGenComponent.Instance;
		navgen.GenerationPlane.Enabled = false;

		hallwayChunks = Scene.GetAllObjects( true ).Where( X => X.GetComponent<HallwayChunkComponent>() != null ).ToImmutableArray();

		foreach ( var item in hallwayChunks )
		{
			item.Transform.Rotation = Rotation.Identity;
			item.GetComponent<HallwayChunkComponent>( false ).CheckSides();
		}

		navgen.GenerateMesh();
		navgen.SetInitialize();
		navgen.Initialized = true;

		foreach ( var door in spawnedRooms[0].GetComponents<RoomDoorDefinition>( false, true ).Where( X => X.Connected ) )
		{
			SpawnPrefabFromPath( "prefabs/pieces/barrel_01.object", door.Transform.Position, door.Transform.Rotation );
		}

		spawnedRooms[0].ClearEnemiesAndItems();

		// FIXME: For some reason hallway chunks sometimes get their Z value mangled.
		foreach ( var hallwayChunk in hallwayChunks )
		{
			hallwayChunk.Transform.Position = new Vector3( hallwayChunk.Transform.Position.x, hallwayChunk.Transform.Position.y, 0 );
		}

		MakeStatic( GeneratedMapParent );
	}

	private void MakeStatic( GameObject parent )
	{
		parent.ForEachChild( "Make static", false, child =>
		{
			// Visual elements.
			if ( child.Name.StartsWith( "floor" ) ||
				child.Name.StartsWith( "wall" ) ||
				child.Name.StartsWith( "pillar" ) ||
				child.Name.StartsWith( "stairs" ) ||
				child.Name.StartsWith( "pipe" ) ||
				child.Name.StartsWith( "Ceil" ) )
				child.Static = true;

			// Should only be visual elements with no children.
			if ( child.Components.Count == 2 && child.TryGetComponent<ModelComponent>( out _ ) && child.TryGetComponent<ModelCollider>( out _ ) )
				child.Static = true;

			MakeStatic( child );
		} );
	}

	private GameObject SpawnPrefabFromPath( string path, Vector3 position, Rotation rotation )
	{
		var prefab = ResourceLibrary.Get<PrefabFile>( path );
		var go = SceneUtility.Instantiate( prefab.Scene, position, rotation );
		go.SetParent( GeneratedMapParent );
		go.Network.Spawn();
		return go;
	}

	private static ImmutableArray<RoomChunkComponent> GetUnconnectedNeighbors( RoomChunkComponent room, IEnumerable<RoomChunkComponent> unconnectedRooms )
	{
		var unconnectedNeighbors = ImmutableArray.CreateBuilder<RoomChunkComponent>();

		foreach ( var neighbor in room.GetComponent<RoomChunkComponent>().ConnectedRooms )
		{
			if ( unconnectedRooms.Contains( neighbor ) )
				unconnectedNeighbors.Add( neighbor );
		}

		unconnectedNeighbors.Capacity = unconnectedNeighbors.Count;
		return unconnectedNeighbors.MoveToImmutable();
	}
}
