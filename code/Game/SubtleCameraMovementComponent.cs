using Sandbox;

namespace BrickJam.Components;

public class SubtleCameraMovementComponent : BaseComponent
{
	private Vector3 startPosition;
	private Vector3 targetPosition;
	private Rotation startRotation;

	float yaw;
	float pitch;

	private float lerpSpeed = 2.0f; // Adjust the speed of the interpolation

	public override void OnStart()
	{
		// Get the camera component
		var camera = Scene.GetComponent<CameraComponent>( true, true );
		startPosition = camera.Transform.Position;
		targetPosition = startPosition;
		startRotation = camera.Transform.Rotation;
	}

	public override void Update()
	{
		if ( Input.Pressed( "Jump" ) )
		{
			Destroy();
		}

		// Get the camera component
		var camera = Scene.GetComponent<CameraComponent>( true, true );

		// Calculate the mouse delta
		var mouseDelta = Input.MouseDelta;

		// Adjust the camera's target position based on the mouse delta
		var cameraSpeed = 0.01f; // Adjust this value to control the speed of camera movement
		targetPosition += new Vector3( mouseDelta.x * cameraSpeed, mouseDelta.y * cameraSpeed, 0.0f );

		targetPosition = Vector3.Lerp( targetPosition, startPosition, Time.Delta );

		// Interpolate the camera's position back to the target position
		camera.Transform.Position = Vector3.Lerp( camera.Transform.Position, targetPosition, Time.Delta * lerpSpeed );

		// Adjust the camera rotation based on the mouse delta
		var rotation = camera.Transform.Rotation;

		yaw = rotation.Yaw();

		pitch = rotation.Pitch();

		yaw += mouseDelta.x * 0.005f; // Adjust the rotation based on the horizontal mouse movement
		pitch += mouseDelta.y * 0.005f; // Adjust the rotation based on the vertical mouse movement

		rotation = new Angles( pitch, yaw, rotation.Roll() ).ToRotation();

		rotation = Rotation.Slerp( rotation, startRotation, Time.Delta * 10f );

		// Set the new camera rotation
		camera.Transform.Rotation = rotation;
	}
}
