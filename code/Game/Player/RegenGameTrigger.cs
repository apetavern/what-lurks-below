using BrickJam.Components;
using BrickJam.Map;
using System.Linq;

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
		MapGeneratorComponent.Instance.RegenMap();
	}
}
