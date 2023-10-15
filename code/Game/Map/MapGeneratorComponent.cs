using Sandbox;
using System.Collections.Generic;

[Title( "Map Generator" )]
[Category( "World" )]
[Icon( "map", "red", "white" )]
public sealed class MapGeneratorComponent : BaseComponent
{
	List<GameObject> gameObjects = new List<GameObject>() { };

	public override void OnStart()
	{
		base.OnStart();
	}

	public override void Update()
	{

	}
}
