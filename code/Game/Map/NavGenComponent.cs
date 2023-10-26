using BrickJam.Components;
using Sandbox;
using System.Collections.Generic;

namespace BrickJam.Map;

public sealed class NavGenComponent : SingletonComponent<NavGenComponent>
{
	[Property] public GameObject GenerationPlane { get; set; }

	NavigationMesh mesh { get; set; }

	public bool Initialized;

	//NavigationPath path { get; set; }

	public void GenerateMesh()
	{
		mesh = new NavigationMesh();
		mesh.Generate( Scene.PhysicsWorld );

		//path = new NavigationPath( mesh );
	}

	public override void DrawGizmos()
	{
		base.DrawGizmos();
		if ( mesh != null )
		{
			Gizmo.Draw.Color = Color.Blue.WithAlpha( 0.5f );
			Gizmo.Draw.LineNavigationMesh( mesh );
		}
	}

	public List<NavigationPath.Segment> GeneratePath( Vector3 point1, Vector3 point2 )
	{
		if ( mesh == null )
		{
			GenerateMesh();
		}

		var path = new NavigationPath( mesh );

		path.StartPoint = point1;

		path.EndPoint = point2;

		path.Build();

		int waitasec = 0;
		while ( path.Segments.Count == 0 && waitasec < 3 )
		{
			waitasec++;
			//Log.Info( "waiting for path gen" );
		}

		if ( path.Segments.Count == 0 )
		{
			path.Build();
		}

		waitasec = 0;
		while ( path.Segments.Count == 0 && waitasec < 3 )
		{
			waitasec++;
			//Log.Info( "waiting for path gen" );
		}

		//Log.Info( "Path took " + path.GenerationMilliseconds + "ms" );
		//Log.Info( "Path has " + path.Segments.Count + " segments" );

		return path.Segments;
	}

}
