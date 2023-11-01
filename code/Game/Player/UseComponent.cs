using BrickJam.Components;
using BrickJam.Map;
using Sandbox;
using System.Linq;

namespace BrickJam.Player;

[Category( "Player" )]
public class UseComponent : BaseComponent
{
	[Property, Range( 0.1f, 3f ), Description( "How long before the player can use something again" )]
	public float UseCooldown { get; set; } = 1;

	[Property, Range( 30, 80 ), Description( "How close does the player need to be to use an item" )]
	public float UseReach { get; set; } = 40;

	TimeSince _timeSinceLastUsed { get; set; }

	public override void Update()
	{
		base.Update();

		if ( Input.Pressed( "use" ) && _timeSinceLastUsed > UseCooldown )
		{
			_timeSinceLastUsed = 0;

			var pickupsInRange = MapGeneratorComponent.Instance.Pickups
				.Where( x => Vector3.DistanceBetween( Transform.Position, x.Transform.Position ) < UseReach )
				.Select( x => x.GetComponent<ItemPickup>() );

			foreach ( var itemPickup in pickupsInRange )
			{
				itemPickup.Triggered();
			};
		}
	}
}
