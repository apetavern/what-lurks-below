using BrickJam.Components;
using BrickJam.Map;
using Coroutines;

namespace BrickJam;

public sealed class RegenGameTrigger : BaseComponent
{
	public override void OnStart()
	{
		GetComponent<CameraTriggerComponent>().OnTriggered += RegenMap;
	}

	public void RegenMap()
	{
		Stats.FloorsCompleted++;
		Coroutine.Start( MapGeneratorComponent.Instance.RegenMapCoroutine );
	}
}
