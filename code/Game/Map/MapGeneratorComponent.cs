using Sandbox;
using System.Collections.Generic;

[Title( "Map Generator" )]
[Category( "World" )]
[Icon( "map", "red", "white" )]
public sealed class MapGeneratorComponent : BaseComponent
{
	List<string> Rooms = new List<string>() { "prefabs/rooms/room_01.object" };

	public override void OnStart()
	{
		var room1 = ResourceLibrary.Get<PrefabFile>( Rooms[0] );
		SceneUtility.Instantiate( room1.Scene, Transform.Position, Transform.Rotation );

		base.OnStart();
	}

	public override void Update()
	{

	}
}
