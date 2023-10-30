using BrickJam.Util;
using Coroutines.Stallers;
using Sandbox;
using System.Collections.Generic;

namespace BrickJam.Components;

public sealed class BezierAnimationComponent : BaseComponent, BaseComponent.ExecuteInEditor
{
	[Property] GameObject Point1 { get; set; }
	[Property] GameObject Point2 { get; set; }

	[Property] int segments { get; set; }

	private List<Vector3> bezierPoints;

	public override void DrawGizmos()
	{
		if ( Point1 == null || Point2 == null )
			return;

		Vector3 startPoint = Point1.Transform.Position;
		Vector3 endPoint = Point2.Transform.Position;

		bezierPoints = BezierCurveGenerator.GenerateBezierCurve( startPoint, endPoint, startPoint + Point1.Transform.Rotation.Forward * Point1.Transform.Scale.x, endPoint + Point2.Transform.Rotation.Forward * Point2.Transform.Scale.x, segments );

		// Draw the Bezier curve using Gizmos (you may need to implement a Gizmos drawing method).
		// For simplicity, you can use Debug.DrawLines to visualize the curve.
		for ( int i = 1; i < bezierPoints.Count; i++ )
		{
			Gizmo.Draw.Color = Color.Red;
			Gizmo.Draw.Line( bezierPoints[i - 1], bezierPoints[i] );
			Gizmo.Draw.SolidBox( new BBox( bezierPoints[i], 1f ) );
		}
	}

	public CoroutineMethod AnimateObjectCoroutine( GameObject animatable, float time = 2f, bool LerpToPointAngles = false )
	{
		Vector3 startPoint = Point1.Transform.Position;
		Vector3 endPoint = Point2.Transform.Position;

		bezierPoints = BezierCurveGenerator.GenerateBezierCurve( startPoint, endPoint,
			startPoint + Point1.Transform.Rotation.Forward * Point1.Transform.Scale.x,
			endPoint + Point2.Transform.Rotation.Forward * Point2.Transform.Scale.x, segments );

		if ( bezierPoints.Count < 2 )
		{
			yield break;
		}

		float totalTime = time; // Total time for the animation (adjust as needed).
		float currentTime = 0.0f;
		float rotationSpeed = 25.0f; // Adjust the rotation speed as needed.

		float realtime = 0f;

		int currentIndex = 0;

		while ( currentIndex < bezierPoints.Count - 1 )
		{
			realtime += Time.Delta;
			float t = currentTime / (totalTime / segments);
			Vector3 currentPosition = Vector3.Lerp( bezierPoints[currentIndex], bezierPoints[currentIndex + 1], t );
			animatable.Transform.Position = currentPosition;

			if ( !LerpToPointAngles )
			{
				// Calculate the rotation to look at the next point.
				Vector3 nextPosition = bezierPoints[currentIndex + 1];
				Rotation lookRotation = Rotation.LookAt( nextPosition - currentPosition, Vector3.Up );

				// Smoothly interpolate the rotation.
				Rotation currentRotation = animatable.Transform.Rotation;
				animatable.Transform.Rotation = Rotation.Slerp( currentRotation, lookRotation, rotationSpeed * Time.Delta );

			}
			else
			{
				float t2 = realtime / totalTime;

				animatable.Transform.Rotation = Rotation.Slerp( Point1.Transform.Rotation * Rotation.FromYaw( 180f ), Point2.Transform.Rotation, t2 );
			}

			yield return new WaitForNextFrame();

			currentTime += Time.Delta;

			if ( currentTime >= totalTime / segments )
			{
				currentTime = 0f;
				currentIndex++;
			}
		}

		// Ensure the final position is exactly at the end of the curve.
		animatable.Transform.Position = bezierPoints[bezierPoints.Count - 1];
	}
}
