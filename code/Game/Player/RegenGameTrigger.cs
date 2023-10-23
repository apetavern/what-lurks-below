using Sandbox;
using System.Linq;

public sealed class RegenGameTrigger : BaseComponent
{
	public override void OnStart()
	{
		GetComponent<CameraTriggerComponent>().OnTriggered += RegenMap;
	}

	public void RegenMap()
	{
		Scene.GetAllObjects( true ).Where( X => X.GetComponent<MapGeneratorComponent>() != null ).FirstOrDefault().GetComponent<MapGeneratorComponent>().RegenMap();
	}
}
