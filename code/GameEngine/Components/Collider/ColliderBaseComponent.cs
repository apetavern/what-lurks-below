using Sandbox;
using Sandbox.Diagnostics;

public abstract class ColliderBaseComponent : BaseComponent
{
	PhysicsShape shape;
	protected PhysicsBody ownBody;
	protected PhysicsGroup group;

	bool _isTrigger;
	[Property]
	public bool IsTrigger
	{
		get => _isTrigger;
		set
		{
			_isTrigger = value;

			if ( shape is not null )
			{
				shape.IsTrigger = _isTrigger;
			}
		}
	}

	[Property]
	public string Tags { get; set; } = "";

	public override void OnEnabled()
	{
		Assert.IsNull( ownBody );
		Assert.IsNull( shape );
		Assert.NotNull( Scene );

		PhysicsBody physicsBody = null;

		// is there a physics body?
		var body = GameObject.GetComponentInParent<PhysicsComponent>( true, true );
		if ( body is not null )
		{
			physicsBody = body.GetBody();

			//
			if ( physicsBody is null )
			{
				return;
			}
		}

		if ( physicsBody is null )
		{
			physicsBody = new PhysicsBody( Scene.PhysicsWorld );
			physicsBody.BodyType = PhysicsBodyType.Keyframed;
			physicsBody.GameObject = GameObject;
			physicsBody.Transform = Transform.World.WithScale( 1 );
			physicsBody.UseController = true;
			physicsBody.GravityEnabled = false;
			ownBody = physicsBody;
		}

		shape = CreatePhysicsShape( physicsBody );
		if ( shape is not null )
		{
			shape.AddTag( "solid" );

			shape.IsTrigger = IsTrigger;
			if ( IsTrigger )
			{
				shape.AddTag( "trigger" );
				shape.RemoveTag( "solid" );
			}

			var tags = Tags.Split( ',' );
			foreach ( var tag in tags )
			{
				string realTag = tag.Trim();
				if ( realTag.StartsWith( '-' ) )
				{
					shape.RemoveTag( realTag.Substring( 1 ) );
					continue;
				}
				shape.AddTag( realTag );
			}
		}
	}

	protected abstract PhysicsShape CreatePhysicsShape( PhysicsBody targetBody );

	public override void OnDisabled()
	{
		//shape?.Body?.RemoveShape( shape );
		shape?.Remove();
		shape = null;

		ownBody?.Remove();
		ownBody = null;

		group?.Remove();
		group = null;
	}

	protected override void OnPostPhysics()
	{
		if ( group is not null )
		{
			foreach ( var body in group.Bodies )
			{
				//	body?.Move( GameObject.WorldTransform, Time.Delta * 4.0f );
			}

			return;
		}

		ownBody?.Move( Transform.World, Time.Delta * 4.0f );
	}

	public void OnPhysicsChanged()
	{
		OnDisabled();
		OnEnabled();
	}
}
