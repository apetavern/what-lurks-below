using Sandbox;

public sealed class GooBossSequencer : BaseComponent
{
	[Property] GameObject roomTrigger { get; set; }

	[Property] GameObject doorBlockers { get; set; }

	public bool StartedFight = false;

	public override void OnStart()
	{
		roomTrigger.GetComponent<CameraTriggerComponent>().OnTriggered += OnTriggered;
		foreach ( var item in doorBlockers.Children )
		{
			item.GetComponent<ModelComponent>().Enabled = false;
			item.GetComponent<ColliderBaseComponent>().Enabled = false;
		}
	}

	public void OnTriggered()
	{
		StartedFight = true;
		foreach ( var item in doorBlockers.Children )
		{
			item.GetComponent<ModelComponent>().Enabled = true;
			item.GetComponent<ColliderBaseComponent>().Enabled = true;
		}

	}

	public override void Update()
	{
		if ( !StartedFight )
		{
			return;
		}
	}
}
