using Sandbox;

namespace BrickJam.Components;

public class MenuSoundComponent : BaseComponent
{
	private Sound _sound;

	public override void OnStart()
	{
		base.OnStart();

		_sound = Sound.FromScreen( "city_ambience" );
	}

	public override void OnDestroy()
	{
		base.OnDestroy();

		_sound.SetVolume( 0 );
	}
}
