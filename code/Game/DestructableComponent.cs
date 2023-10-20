using BrickJam.Player;
using Sandbox;

public sealed class DestructableComponent : BaseComponent
{
	public override void OnStart()
	{
		// Destroy on death
		HealthComponent healthComponent = GameObject.GetComponent<HealthComponent>();
		healthComponent.OnDeath += OnDeath;
		healthComponent.OnDamage += OnDamaged;
	}

	public void OnDeath()
	{
		GameObject.Destroy();
	}

	public void OnDamaged()
	{
		Log.Info( "Barrel damage" );
	}
}
