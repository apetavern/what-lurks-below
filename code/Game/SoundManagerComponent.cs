using System;
using Sandbox;

namespace BrickJam.Game;

public class SoundManagerComponent : BaseComponent
{
	private TimeUntil TimeUntilWaterDrip { get; set; } = 1.0f;

	public override void OnStart()
	{
		base.OnStart();

		TimeUntilWaterDrip = 1.0f;
	}

	public override void Update()
	{
		if ( TimeUntilWaterDrip < 0f )
		{
			Log.Info("water drip"  );
			Sound.FromScreen( "water_drip" );
			TimeUntilWaterDrip = Random.Shared.Float( 1.6f, 2.4f );
		}
	}
}
