using Sandbox;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

public sealed class NavGenComponent : BaseComponent
{
	[Property] public GameObject GenerationPlane { get; set; }

	NavigationMesh mesh { get; set; }

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
			foreach ( var node in mesh.Nodes )
			{
				Gizmo.Draw.Color = Color.Blue.WithAlpha( 0.5f );
				Gizmo.Draw.LineTriangle( new Triangle( node.Value.Vertices[0], node.Value.Vertices[1], node.Value.Vertices[2] ) );
			}
		}
	}

	public async Task<List<NavigationPath.Segment>> GeneratePath( Vector3 point1, Vector3 point2 )
	{
		if ( mesh == null )
			GenerateMesh();
		await GameTask.Delay( 10 );

		var path = new NavigationPath( mesh );

		Log.Info( point1 + " " + point2 );

		path.StartPoint = point1;

		path.EndPoint = point2;

		path.Build();

		int waitasec = 0;
		while ( path.Segments.Count == 0 && waitasec < 3 )
		{
			waitasec++;
			Log.Info( "waiting for path gen" );
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
			Log.Info( "waiting for path gen" );
			await GameTask.Delay( 500 );
		}

		Log.Info( "Path took " + path.GenerationMilliseconds + "ms" );
		Log.Info( "Path has " + path.Segments.Count + " segments" );

		return path.Segments;
	}

}
