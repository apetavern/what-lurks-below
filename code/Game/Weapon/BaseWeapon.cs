using Sandbox;

namespace BrickJam.Game.Weapon;

public class BaseWeapon : GameObject
{
	public virtual Model Model { get; set; }

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

	public virtual void OnPrimaryPressed() { }
	
	public virtual void OnSecondaryPressed() { }
}
