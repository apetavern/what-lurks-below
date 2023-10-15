using Sandbox;

namespace BrickJam.Player;

public class HealthComponent : BaseComponent
{
	public float Health { get; set; }

	[Property] public float InitialHealth { get; set; }
	
	public override void OnStart()
	{
		base.OnStart();

		Health = InitialHealth;
	}
}
