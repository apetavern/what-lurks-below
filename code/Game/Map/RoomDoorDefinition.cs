using Sandbox;

[EditorHandle( "materials/gizmo/door.png" )]
public sealed class RoomDoorDefinition : BaseComponent
{
	[Property] public GameObject ClosedMesh { get; set; }

	public bool Connected;
	public void OpenDoor()
	{
		Connected = true;
		if ( ClosedMesh != null ) ClosedMesh.Enabled = false;
	}

	public override void DrawGizmos()
	{
		base.DrawGizmos();
		var triangle = new Triangle();
		triangle.A = Vector3.Backward * 15f;
		triangle.B = Vector3.Left * 25f;
		triangle.C = Vector3.Forward * 15f;
		Gizmo.Draw.Color = Color.Red;
		Gizmo.Draw.LineTriangle( triangle );

		Gizmo.Draw.LineThickness = 1f;
		Gizmo.Draw.Line( Vector3.Backward * 6f, Vector3.Right * 25f + Vector3.Backward * 7.5f );

		Gizmo.Draw.LineThickness = 1f;
		Gizmo.Draw.Line( Vector3.Right * 25f + Vector3.Backward * 7.5f, Vector3.Right * 25f + Vector3.Forward * 7.5f );

		Gizmo.Draw.LineThickness = 1f;
		Gizmo.Draw.Line( Vector3.Forward * 6f, Vector3.Right * 25f + Vector3.Forward * 7.5f );
	}
}
