using Sandbox;

namespace BrickJam.Player;

[Icon( "monitor_heart", "red", "white" )]
public class HealthComponent : BaseComponent
{
	public float Health { get; set; }

	[Property] public float InitialHealth { get; set; }
	[Property] public bool IsPlayer { get; set; } = false;

	public bool IsDead = false;

	public delegate void DeathEvent();
	public event DeathEvent OnDeath;

	public delegate void DamageEvent();
	public event DamageEvent OnDamage;

	public override void OnStart()
	{
		base.OnStart();

		Health = InitialHealth;
	}

	public override void Update()
	{
		base.Update();

		Health = Health.Clamp( 0, 100 );

		if ( IsPlayer && HealthHud.Instance is not null )
			HealthHud.Instance.Health = Health;
	}

	public void Damage( float amount )
	{
		if ( IsDead ) return;

		Health -= amount;

		OnDamage?.Invoke();

		if ( Health <= 0f )
		{
			IsDead = true;
			OnDeath?.Invoke();
			Health = 0f;
		}

		//Log.Info( $"Health: {Health}" );
	}
}
