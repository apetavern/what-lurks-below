using BrickJam.Player;
using Sandbox;

public sealed class GooBossEyeball : BaseComponent
{
	public EyeballPosition pos { get; set; }
	public GooBossSequencer boss { get; set; }
	HealthComponent health { get; set; }
	public override void OnStart()
	{
		health = GetComponent<HealthComponent>();
		health.OnDamage += OnTakeDamage;
		health.OnDeath += OnEyeballDestroyed;
		base.OnStart();
	}

	public void OnTakeDamage()
	{
		if ( boss != null )
		{
			boss.TriggerDamage( this );
		}
		else
		{
			boss = GameObject.Parent.GetComponent<GooBossSequencer>();
			boss.TriggerDamage( this );
		}
	}

	public void OnEyeballDestroyed()
	{
		if ( boss != null )
		{
			boss.TriggerDamage( this, true );
		}
		else
		{
			boss = GameObject.Parent.GetComponent<GooBossSequencer>();
			boss.TriggerDamage( this );
		}
	}

	public override void Update()
	{
		if ( boss == null )
		{
			boss = GameObject.Parent.GetComponent<GooBossSequencer>();
		}
		Transform.Position = boss.GetComponent<AnimatedModelComponent>().GetAttachmentTransform( pos.ToString() ).Position;
	}
}
