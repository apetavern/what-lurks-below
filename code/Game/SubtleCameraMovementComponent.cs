using Sandbox;

namespace BrickJam.Game;

public class SubtleCameraMovementComponent : BaseComponent
{
	public override void Update()
	{
		base.Update();
		
		// Use mouse delta to give a minor parallax effect on the camera rotation
		var camera = Scene.GetComponent<CameraComponent>( true, true );
		
	}
}
