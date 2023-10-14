using Sandbox;
using System.Linq;

[Title( "CameraTrigger" )]
[Category( "Render" )]
[Icon( "videocam", "red", "white" )]
[EditorHandle( "materials/gizmo/camera.png" )]
public sealed class CameraTriggerComponent : BaseComponent
{
	BBox Bounds;

	GameObject Player;

	PlayerController Controller;

	[Property] GameObject CameraPoint { get; set; }

	public override void OnEnabled()
	{
		base.OnEnabled();
		Player = Scene.GetAllObjects( true ).Where( X => X.Name == "player" ).FirstOrDefault();
		Controller = Player.GetComponent<PlayerController>();

		var box = new BBox();
		var scale = GetComponent<ColliderBoxComponent>( false ).Scale;
		var position = Transform.Position; // Assuming you have a reference to the object's transform

		// Calculate the minimum and maximum points of the bounding box
		box.Mins = position - 0.5f * scale;
		box.Maxs = position + 0.5f * scale;

		Bounds = box;
	}

	public override void Update()
	{
		if ( Player is not null )
		{
			if ( Bounds.Contains( Player.Transform.Position + Vector3.Up * 32f ) )
			{
				Triggered();
			}
		}
		base.Update();
	}

	public void Triggered()
	{
		Controller.Camera.Transform.Position = CameraPoint.Transform.Position;
		Controller.Camera.Transform.Rotation = CameraPoint.Transform.Rotation;
	}
}
