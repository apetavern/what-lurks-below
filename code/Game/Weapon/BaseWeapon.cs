using Sandbox;

namespace BrickJam.Game.Weapon;

public class BaseWeapon : GameObject
{
	public virtual Model Model { get; set; }
	public virtual CitizenAnimationHelperScene.HoldTypes HoldType => CitizenAnimationHelperScene.HoldTypes.None;
	public virtual CitizenAnimationHelperScene.Hand Handedness => CitizenAnimationHelperScene.Hand.Left;
	public virtual CitizenAnimationHelperScene.Hand AlternateHandedness => CitizenAnimationHelperScene.Hand.Left;
	public virtual bool CanAimFocus => false;
	

	protected AnimatedModelComponent c_AnimatedModel;

	internal BaseWeapon( bool enabled, string name, Scene scene ) : base( enabled, name, scene )
	{
		c_AnimatedModel = AddComponent<AnimatedModelComponent>( false );
	}

	public void SetActive( GameObject parent )
	{
		c_AnimatedModel.GameObject.Parent = parent;
		c_AnimatedModel.Model = Model;
		c_AnimatedModel.Enabled = true;
	}

	public void SetInactive()
	{
		c_AnimatedModel.Enabled = false;
	}
	
	public virtual void OnIdle( CitizenAnimationHelperScene helper ) { }

	public virtual void OnPrimaryPressed( CitizenAnimationHelperScene helper ) { }
	
	public virtual void OnPrimaryHeld ( CitizenAnimationHelperScene helper ) { }
	
	public virtual void OnSecondaryPressed( CitizenAnimationHelperScene helper ) { }
	
	public virtual void OnSecondaryHeld( CitizenAnimationHelperScene helper ) { }
}
