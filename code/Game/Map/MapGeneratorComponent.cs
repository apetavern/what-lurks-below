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

	List<GameObject> SpawnedRooms = new List<GameObject>();

	[Property] int RoomCount { get; set; } = 10;

	[Property] float SnapGridSize { get; set; } = 700f;

	public override void OnStart()
	{
		var room1 = ResourceLibrary.Get<PrefabFile>( Rooms[0] );
		SpawnedRooms.Add( SceneUtility.Instantiate( room1.Scene, Transform.Position, Transform.Rotation ) );

		for ( int i = 0; i < RoomCount; i++ )
		{
			var room = ResourceLibrary.Get<PrefabFile>( Rooms[0] );

			Vector2 SpawnPoint = Game.Random.VectorInCircle( 800 );

			Vector3 PlacePoint = new Vector3( SpawnPoint.x, SpawnPoint.y, 0 );

			SpawnedRooms.Add( SceneUtility.Instantiate( room.Scene, Transform.Position + PlacePoint, Transform.Rotation ) );
		}

		//MoveRooms();

		base.OnStart();
	}

	public async void MoveRooms()
	{
		foreach ( var room in SpawnedRooms )
		{
			foreach ( var room2 in SpawnedRooms )
			{
				if ( room.GetBounds().Overlaps( room2.GetBounds() ) )
				{
					room.Transform.Position -= room.Transform.Position - room2.Transform.Position;
				}
			}
		}
	}

	bool CorrectedRooms;

	public override void Update()
	{
		if ( !CorrectedRooms )
		{
			int overlaps = 0;
			foreach ( var room in SpawnedRooms )
			{
				foreach ( var room2 in SpawnedRooms )
				{
					if ( room.GetBounds().Overlaps( room2.GetBounds() ) )
					{
						overlaps++;
						room.Transform.Position += (room.Transform.Position - room2.Transform.Position) * 0.3f;
					}
				}
			}
			if ( overlaps == 11 )
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
			}
		}
	}
}
