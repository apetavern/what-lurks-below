using System;
using Sandbox;

namespace BrickJam.Game;

public class SoundManagerComponent : BaseComponent
{
	private TimeUntil TimeUntilWaterDrip { get; set; } = 1.0f;
	private Sound Music { get; set; }

	public override void OnStart()
	{
		base.OnStart();

		TimeUntilWaterDrip = 1.0f;

		Music = Sound.FromScreen( "spoop" );
		Music.SetVolume( 0f );
	}

	public override void Update()
	{
		if ( TimeUntilWaterDrip < 0f )
		{
			Sound.FromScreen( "water_drip" );
			TimeUntilWaterDrip = Random.Shared.Float( 1.6f, 2.4f );
		}

		Music.SetVolume( 1f );
	}

	public override void OnDestroy()
	{
		base.OnDestroy();

		Music.Stop();
	}
}
