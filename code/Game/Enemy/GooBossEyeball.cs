using BrickJam.Player;
using Sandbox;
using System.Linq;

public sealed class GooBossEyeball : BaseComponent
{
	public EyeballPosition pos { get; set; }
	public GooBossSequencer boss { get; set; }
	HealthComponent health { get; set; }

	public int HitsThisCycle;

	public override void OnStart()
	{
		health = GetComponent<HealthComponent>();
		health.OnDamage += OnTakeDamage;
		health.OnDeath += OnEyeballDestroyed;
		base.OnStart();
	}

	public void OnTakeDamage()
	{
		HitsThisCycle++;
		if ( boss != null )
		{
			boss.TriggerDamage( this );
		}
		else
		{
			boss.TriggerDamage( this );
		}
		if ( HitsThisCycle >= 3 )
		{
			boss.TimeSinceEyesOpened = 15f;
			HitsThisCycle = 0;
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
			boss.TriggerDamage( this, true );
		}

		GameObject.Destroy();
	}

	public override void Update()
	{
		if ( boss == null )
		{
			return;
		}

		if ( boss.EyesOpen )
		{
			Transform.Position = boss.GetComponent<AnimatedModelComponent>().GetAttachmentTransform( pos.ToString() ).Position;
		}
		else
		{
			Transform.Position = boss.Transform.Position;
		}

		if ( health.Health <= 0 )
		{
			GameObject.Destroy();
			return;
		}

		GetComponent<Collider>().OnPhysicsChanged();
	}
}
