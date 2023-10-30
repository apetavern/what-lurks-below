using System;
using System.Linq;
using BrickJam.Player;
using Sandbox;

namespace BrickJam.Components;

public class SoundManagerComponent : BaseComponent
{
	private TimeUntil TimeUntilWaterDrip { get; set; } = 1.0f;
	private Sound Music { get; set; }
	private Sound BossMusic { get; set; }

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

		var flags = PlayerFlagsComponent.Instance;
		if ( flags is null )
			return;

		if ( flags.InBossSequence )
		{
			if ( !BossMusic.IsPlaying )
				BossMusic = Sound.FromScreen( "boss" );
			BossMusic.SetVolume( 1f );
			Music.SetVolume( 0f );
			return;
		}
		else
		{
			if ( BossMusic.IsPlaying )
				BossMusic.Stop();
		}

		BossMusic.SetVolume( 0f );
		Music.SetVolume( 1f );
	}

	public override void OnDestroy()
	{
		base.OnDestroy();

		BossMusic.Stop();
		Music.Stop();
	}
}
