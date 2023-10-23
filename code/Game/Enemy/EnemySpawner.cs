using BrickJam.Player;
using Sandbox;
using System.Linq;
using System;
using System.Collections.Generic;

namespace BrickJam.Game;

[Icon( "adjust", "red", "white" )]
[EditorHandle( "materials/gizmo/charactercontroller.png" )]
public class EnemySpawner : BaseComponent
{
	[Range( 0, 100 )]
	[Property] private float SpawnChance { get; set; } = 100f;

	[Property] private GameObject Enemy1 { get; set; }
	[Property] private GameObject Enemy2 { get; set; }
	[Property] private GameObject Enemy3 { get; set; }

	public NavGenComponent navGen { get; set; }

	public override void OnEnabled()
	{
		navGen = Scene.GetAllObjects( true ).Where( X => X.GetComponent<NavGenComponent>() != null ).FirstOrDefault().GetComponent<NavGenComponent>();
		SpawnEnemies();
	}
	public async void SpawnEnemies()
	{
		while ( !navGen.Initialized )
		{
			await GameTask.DelaySeconds( Time.Delta );
		}
		List<GameObject> enemiesToSpawn = new();
		if ( Enemy1 != null ) enemiesToSpawn.Add( Enemy1 );
		if ( Enemy2 != null ) enemiesToSpawn.Add( Enemy2 );
		if ( Enemy3 != null ) enemiesToSpawn.Add( Enemy3 );

		if ( enemiesToSpawn.Count > 0 && new Random().Float( 0f, 100f ) < SpawnChance )
		{
			var enemy = enemiesToSpawn[new Random().Int( 0, enemiesToSpawn.Count - 1 )];
			var enemyObject = SceneUtility.Instantiate( enemy, GameObject.Transform.Position, GameObject.Transform.Rotation );
			enemyObject.Transform.Position = GameObject.Transform.Position;
			enemyObject.SetParent( GameObject.Parent );
		}

		GameObject.Destroy();
	}
}
