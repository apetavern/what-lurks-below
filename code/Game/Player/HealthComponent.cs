using BrickJam.Game.UI;
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

	public override void Update()
	{
		base.Update();

		if ( Vitals.Instance is not null )
			Vitals.Instance.Health = $"{Health:F0}";
	}
}
