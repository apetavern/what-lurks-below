using System.Linq;

namespace BrickJam.Components;

public sealed class OpenDoorOnTrigger : BaseComponent
{
	bool OpenedDoor;

	public override void OnStart()
	{
		GetComponent<CameraTriggerComponent>().OnTriggered += CheckForKey;
	}

	public void CheckForKey()
	{
		if ( !OpenedDoor )
		{
			var player = Scene.GetAllObjects( true ).Where( X => X.Name == "player" ).FirstOrDefault();
			if ( player.GetComponent<PlayerFlagsComponent>().HasBossKey )
			{
				OpenedDoor = true;
				GameObject.Parent.GetComponent<BezierAnimationComponent>( false, true ).AnimateObject( GameObject.Parent.GetComponent<ModelComponent>( false, true ).GameObject, 2f, true );
				Destroy();
			}
		}
	}
}
