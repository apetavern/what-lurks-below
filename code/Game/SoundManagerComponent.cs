using System;
using System.Linq;
using Sandbox;

namespace BrickJam.Game;

public class SoundManagerComponent : BaseComponent
{
	private TimeUntil TimeUntilWaterDrip { get; set; } = 1.0f;
	private Sound Music { get; set; }
	private Sound BossMusic { get; set; }

	private PlayerFlagsComponent _flags;

	public override void OnStart()
	{
		base.OnStart();

		TimeUntilWaterDrip = 1.0f;

		Music = Sound.FromScreen( "spoop" );
		Music.SetVolume( 0f );

		_flags = Scene.GetAllObjects( true ).FirstOrDefault( o => o.Name == "player" )?
			.GetComponent<PlayerFlagsComponent>();
	}

	public override void Update()
	{
		if ( TimeUntilWaterDrip < 0f )
		{
			Sound.FromScreen( "water_drip" );
			TimeUntilWaterDrip = Random.Shared.Float( 1.6f, 2.4f );
		}
		
		if ( _flags is null )
			return;

		if ( _flags.InBossSequence )
		{
			if ( !BossMusic.IsPlaying )
				BossMusic = Sound.FromScreen( "boss" );
			BossMusic.SetVolume( 1f );
			Music.SetVolume( 0f );
			return;
		}

		BossMusic.SetVolume( 0f );
		Music.SetVolume( 1f );
	}

	public override void OnDestroy()
	{
		base.OnDestroy();

		Music.Stop();
	}
}
