using Sandbox;

public sealed class GooBossSequencer : BaseComponent
{
	[Property] GameObject roomTrigger { get; set; }

	[Property] GameObject doorBlockers { get; set; }

	AnimatedModelComponent BossModel { get; set; }

	public bool StartedFight = false;

	public override void OnStart()
	{
		roomTrigger.GetComponent<CameraTriggerComponent>().OnTriggered += OnTriggered;
		foreach ( var item in doorBlockers.Children )
		{
			item.GetComponent<ModelComponent>().Enabled = false;
			item.GetComponent<ColliderBaseComponent>().Enabled = false;
		}

		BossModel = GetComponent<AnimatedModelComponent>();
		BossModel.SceneObject.SetBodyGroup( "middle", 1 );
	}

	public void OnTriggered()
	{
		StartedFight = true;
		foreach ( var item in doorBlockers.Children )
		{
			item.GetComponent<ModelComponent>( false, true ).Enabled = true;
			item.GetComponent<ColliderBaseComponent>( false, true ).Enabled = true;
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
