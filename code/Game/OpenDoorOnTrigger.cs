using System.Linq;
using BrickJam.Player;
using Coroutines;
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
			var player = BrickPlayerController.Instance.Player;
			if ( PlayerFlagsComponent.Instance.HasBossKey )
			{
				OpenedDoor = true;
				Coroutine.Start( GameObject.Parent.GetComponent<BezierAnimationComponent>( false, true ).AnimateObjectCoroutine( GameObject.Parent.GetComponent<ModelComponent>( false, true ).GameObject, 2f, true ) );
				Destroy();
			}
		}
	}
}
