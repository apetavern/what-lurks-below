using System.Collections.Generic;

namespace BrickJam.Util;

public static class BezierCurveGenerator
{
	public static List<Vector3> GenerateBezierCurve( Vector3 startPoint, Vector3 endPoint, Vector3 controlPoint1, Vector3 controlPoint2, int segments )
	{
		List<Vector3> points = new List<Vector3>();

		for ( int i = 0; i <= segments; i++ )
		{
			float t = i / (float)segments;
			Vector3 point = CalculateBezierPoint( t, startPoint, endPoint, controlPoint1, controlPoint2 );
			points.Add( point );
		}

		return points;
	}

	private static Vector3 CalculateBezierPoint( float t, Vector3 p0, Vector3 p3, Vector3 p1, Vector3 p2 )
	{
		float u = 1 - t;
		float tt = t * t;
		float uu = u * u;
		float uuu = uu * u;
		float ttt = tt * t;

		Vector3 p = uuu * p0; // (1-t)^3 * P0
		p += 3 * uu * t * p1; // 3 * (1-t)^2 * t * P1
		p += 3 * u * tt * p2; // 3 * (1-t) * t^2 * P2
		p += ttt * p3; // t^3 * P3

		return p;
	}
}
