using Sandbox;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

public sealed class NavGenComponent : BaseComponent
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

	public async Task<List<NavigationPath.Segment>> GeneratePath( Vector3 point1, Vector3 point2 )
	{
		if ( mesh == null )
		{
			await GameTask.DelayRealtimeSeconds( Time.Delta * 2f );
			GenerateMesh();
			await GameTask.DelayRealtimeSeconds( Time.Delta * 2f );
		}

		var path = new NavigationPath( mesh );

		//Log.Info( point1 + " " + point2 );

		path.StartPoint = point1;

		path.EndPoint = point2;

		path.Build();

		int waitasec = 0;
		while ( path.Segments.Count == 0 && waitasec < 3 )
		{
			waitasec++;
			//Log.Info( "waiting for path gen" );
			await GameTask.Delay( 1000 );
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
			await GameTask.Delay( 500 );
		}

		//Log.Info( "Path took " + path.GenerationMilliseconds + "ms" );
		//Log.Info( "Path has " + path.Segments.Count + " segments" );

		return path.Segments;
	}

}
