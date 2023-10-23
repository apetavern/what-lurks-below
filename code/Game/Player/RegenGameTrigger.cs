using Sandbox;
using System.Linq;

namespace BrickJam.Game;

public sealed class RegenGameTrigger : BaseComponent
{
	public override void OnStart()
	{
		GetComponent<CameraTriggerComponent>().OnTriggered += RegenMap;
	}

	public void RegenMap()
	{
		Stats.FloorsCompleted++;
		Scene.GetAllObjects( true ).Where( X => X.GetComponent<MapGeneratorComponent>() != null ).FirstOrDefault().GetComponent<MapGeneratorComponent>().RegenMap();
	}
}
