using BrickJam.Game.UI;
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

		if ( IsPlayer && Vitals.Instance is not null )
			Vitals.Instance.Health = $"{Health:F0}";
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

		Log.Info( $"Health: {Health}" );
	}
}
