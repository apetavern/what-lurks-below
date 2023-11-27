using Sandbox;
using System;
using System.Collections.Generic;
using BrickJam.Map;
using Coroutines;
using Coroutines.Stallers;

namespace BrickJam;

[Icon( "adjust", "red", "white" )]
[EditorHandle( "materials/gizmo/charactercontroller.png" )]
public class EnemySpawner : BaseComponent
{
	[Range( 0, 100 )]
	[Property] private float SpawnChance { get; set; } = 100f;

	[Property] private GameObject Enemy1 { get; set; }
	[Property] private GameObject Enemy2 { get; set; }
	[Property] private GameObject Enemy3 { get; set; }

	public override void OnEnabled()
	{
		Coroutine.Start( SpawnEnemiesCoroutine );
	}

	private CoroutineMethod SpawnEnemiesCoroutine()
	{
		if ( !IsProxy )
		{
			while ( !NavGenComponent.Instance.Initialized )
				yield return new WaitForNextFrame();

			List<GameObject> enemiesToSpawn = new();
			if ( Enemy1 != null ) enemiesToSpawn.Add( Enemy1 );
			if ( Enemy2 != null ) enemiesToSpawn.Add( Enemy2 );
			if ( Enemy3 != null ) enemiesToSpawn.Add( Enemy3 );

			if ( enemiesToSpawn.Count > 0 && new Random().Float( 0f, 100f ) < SpawnChance )
			{
				var enemy = enemiesToSpawn[new Random().Int( 0, enemiesToSpawn.Count - 1 )];
				var enemyObject = SceneUtility.Instantiate( enemy, GameObject.Transform.Position, GameObject.Transform.Rotation );

				enemyObject.SetParent( GameObject.Parent );

				enemyObject.Network.Spawn();

				enemyObject.Transform.Position = GameObject.Transform.Position;

			}

			GameObject.Destroy();
		}
	}
}
