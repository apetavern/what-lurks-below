using Sandbox;
using Sandbox.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

[Title( "Local Map" )]
[Category( "World" )]
[Icon( "visibility", "red", "white" )]
public class LocalMapComponent : BaseComponent, BaseComponent.ExecuteInEditor
{
	[Property]
	public string MapFileName { get; set; }

	Map loadedMap;

	Task loadingTask;

	public override void OnEnabled()
	{
		base.OnEnabled();

		if ( MapFileName == "" )
			return;

		var assetName = MapFileName;

		loadedMap = new Map( assetName, new MapComponentMapLoader( this ) );

		foreach ( var body in loadedMap.PhysicsGroup.Bodies )
		{
			var go = GameObject.Create();
			go.Flags |= GameObjectFlags.NotSaved;
			go.Name = "World Physics";
			var co = go.AddComponent<ColliderMapComponent>();
			co.SetBody( body );
			go.SetParent( GameObject, true );
		}
	}

	public override void OnDisabled()
	{
		loadedMap?.Delete();
		loadedMap = null;

		foreach ( var child in GameObject.Children )
		{
			child.Destroy();
		}
	}

}

file class MapComponentMapLoader : SceneMapLoader
{
	LocalMapComponent map;

	public MapComponentMapLoader( LocalMapComponent mapComponent ) : base( mapComponent.Scene.SceneWorld, mapComponent.Scene.PhysicsWorld )
	{
		map = mapComponent;
	}

	protected override void CreateObject( ObjectEntry kv )
	{
		var go = GameObject.Create();
		go.Flags |= GameObjectFlags.NotSaved;
		go.Name = $"{kv.TypeName}";
		go.Transform.Local = kv.Transform;

		//
		// ideal situation here is that we look at the entities and create them
		// via components. Like for a spotlight we create a SpotLightComponent.
		// but that's a lot of work right now so lets just do this crazy hack
		// to allow the created SceneObjects be viewed as gameobject compoonents
		// and be turned on and off.. but nothing else.
		//

		var c = go.AddComponent<MapObjectComponent>();

		c.RecreateMapObjects += () =>
		{
			SceneObjects.Clear();
			base.CreateObject( kv );

			if ( SceneObjects.Count > 0 )
			{
				c.AddSceneObjects( SceneObjects );
			}
		};

		go.SetParent( map.GameObject, true );

		//go.Name += " (unhandled)";
	}
}
