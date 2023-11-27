using BrickJam.Player;
using System;
using Sandbox;
using System.Linq;
using Coroutines.Stallers;
using Coroutines;

namespace BrickJam.Weapons;

public class KnifeWeapon : BaseWeapon
{
	public static InventoryItem KnifeItem => GetInventoryItem();

	public override Model Model { get; set; } = Model.Load( "models/items/knife/knife.vmdl" );
	public override CitizenAnimationHelperScene.HoldTypes HoldType => CitizenAnimationHelperScene.HoldTypes.HoldItem;
	public override CitizenAnimationHelperScene.Hand Handedness => CitizenAnimationHelperScene.Hand.Right;
	public override CitizenAnimationHelperScene.Hand AlternateHandedness => CitizenAnimationHelperScene.Hand.Right;
	public override bool CanAimFocus => true;
	public override int AmmoCount
	{
		get => base.AmmoCount;
		set
		{
			base.AmmoCount = value;
		}
	}
	public override int MaxAmmo => -1;
	public override float Damage => 20f;
	public override float ReloadTime => 0.6f;

	public override float TraceLength => 20f;

	private CoroutineBucket KnifeCoroutines { get; } = new();

	internal KnifeWeapon( bool enabled, string name ) : base( enabled, name )
	{
		defaultAmmoCount = MaxAmmo;
	}

	private static InventoryItem GetInventoryItem()
	{
		return ResourceLibrary.GetAll<InventoryItem>().FirstOrDefault( i => i.Name == "Knife" );
	}

	public override void OnHolster()
	{
		KnifeCoroutines.StopAll();
	}

	public override void OnIdle( ref CitizenAnimationHelperScene helper )
	{
		helper.HoldType = HoldType;
		helper.Handedness = Handedness;
	}

	public override void PrimaryFire( Vector3 position, Vector3 direction )
	{
		KnifeCoroutines.Start( TryHitCoroutine );
	}

	public CoroutineMethod TryHitCoroutine()
	{
		bool HitSomething = false;

		float AnimationTime = 1f;
		float CurrentTime = 0f;
		while ( CurrentTime < AnimationTime && !HitSomething )
		{
			var muzzle = c_AnimatedModel.GetAttachmentTransform( "muzzle" );

			var tr = Physics.Trace.Ray( muzzle.Position, muzzle.Position + muzzle.Rotation.Forward * TraceLength )
				.WithAnyTags( "solid", "enemy" )
				.Size( 5f )
				.Run();

			if ( tr.Hit && tr.Body.GameObject is GameObject hitObject )
			{
				HealthComponent hitHealth = hitObject.Parent?.GetComponent<HealthComponent>() ?? null;

				LastHitBbox = hitObject.GetBounds();

				if ( hitHealth == null )
				{
					hitHealth = hitObject.GetComponent<HealthComponent>();
				}

				if ( hitHealth != null )
				{
					var damage = Damage + new Random().Int( -1, 1 );

					if ( hitHealth.Health > 0 )
					{
						CreateDamageToast( hitObject, tr.HitPosition, (damage - 0.5f).CeilToInt() );
					}

					hitHealth.Damage( damage );
				}

				HitSomething = true;
			}

			CurrentTime += Time.Delta;
			yield return new WaitForNextFrame();
		}
	}

	public override void OnPrimaryPressed( ref CitizenAnimationHelperScene helper )
	{
		if ( AmmoCount > 0 || AmmoCount == -1 )
		{
			BrickPlayerController.Instance.TriggerAttack();
			//helper.SetAnimParameter( "holdtype_attack", 2f );//Heavy attack
			//helper.TriggerAttack();
			//Sound.FromWorld( "weapons/rust_pistol/sound/rust_pistol.shoot.sound", Transform.Position );
		}
		else
		{
			helper.TriggerReload();
		}
	}

	public override void OnSecondaryPressed( ref CitizenAnimationHelperScene helper )
	{

	}

	public override void OnSecondaryHeld( ref CitizenAnimationHelperScene helper )
	{
		if ( CanAimFocus )
			helper.Handedness = AlternateHandedness;
	}

	protected override void Update()
	{
		base.Update();
	}
}
