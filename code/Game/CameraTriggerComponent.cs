using BrickJam.Player;
using Sandbox;
using System.Linq;

namespace BrickJam.Components;

[Title( "CameraTrigger" )]
[Category( "Render" )]
[Icon( "videocam", "red", "white" )]
[EditorHandle( "materials/gizmo/camera.png" )]
public sealed class CameraTriggerComponent : BaseComponent
{
	BBox Bounds;

	GameObject Player;

	BrickPlayerController Controller;

	[Property] public GameObject CameraPoint { get; set; }

	[Property] bool FollowPlayer { get; set; }

	public delegate void TriggeredEvent();
	public event TriggeredEvent OnTriggered;

	public override void OnEnabled()
	{
		base.OnEnabled();
		Player = Scene.GetAllObjects( true ).FirstOrDefault( x => x.Name == "player" );
		Controller = Player?.GetComponent<BrickPlayerController>( false );

		var box = new BBox();
		var scale = GetComponent<ColliderBoxComponent>( false ).Scale;
		var position = Transform.Position; // Assuming you have a reference to the object's transform

		// Calculate the minimum and maximum points of the bounding box
		box.Mins = position - 0.5f * scale;
		box.Maxs = position + 0.5f * scale;

		Bounds = box;
	}

	public void RecalcBounds()
	{
		GetComponent<ColliderBoxComponent>( false ).Enabled = true;
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
			if ( Bounds.Contains( Player.Transform.Position + Vector3.Up * 28f ) )
			{
				Triggered();
			}
			else
			{
				UnTrigger();
			}
		}
		base.Update();
	}

	bool WasTriggered;

	public void UnTrigger()
	{
		if ( WasTriggered )
		{
			WasTriggered = false;
		}
	}

	public void Triggered()
	{
		if ( !WasTriggered )
		{
			OnTriggered?.Invoke();
			WasTriggered = true;
		}

		if ( CameraPoint != null )
		{
			Controller.Camera.Transform.Position = CameraPoint.Transform.Position;

			Player.GetComponent<BrickPlayerController>().CameraControl = false;

			if ( !FollowPlayer )
			{
				Controller.Camera.Transform.Rotation = CameraPoint.Transform.Rotation;
			}
			else
			{
				Controller.Camera.Transform.Rotation = Rotation.LookAt( -(CameraPoint.Transform.Position - Player.Transform.Position) + Vector3.Up * 45f );
			}
		}
	}
}
