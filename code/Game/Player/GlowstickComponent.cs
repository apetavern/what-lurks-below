using Sandbox;

public sealed class GlowstickComponent : BaseComponent
{
	float brightness = 4f;

	ModelComponent model;

	public PhysicsBody _body;

	public Vector3 Velocity;

	Vector3 Gravity;

	bool grounded;

	PointLightComponent light;

	Color initialLight;

	public override void OnStart()
	{
		model = GetComponent<ModelComponent>( false, true );
		light = GetComponent<PointLightComponent>( false, true );
		initialLight = light.LightColor;
	}

	PhysicsTraceBuilder BuildTrace( Vector3 from, Vector3 to ) => BuildTrace( Scene.PhysicsWorld.Trace.Ray( from, to ).WithoutTags( "trigger" ) );
	PhysicsTraceBuilder BuildTrace( PhysicsTraceBuilder source ) => source.Size( GameObject.GetBounds() ).WithoutTags( "trigger" );

	TimeSince TimeSinceGrounded;

	bool StopPhysics;

	public override void Update()
	{
		if ( brightness > 0.1f )
		{
			brightness -= Time.Delta * 0.05f;
			model.SceneObject.Attributes.Set( "brightness", brightness );
			light.LightColor = initialLight * (brightness / 4f);
		}
		else
		{
			light.GameObject.Destroy();
		}

		if ( TimeSinceGrounded > 20f )
		{
			Destroy();
		}

		if ( StopPhysics ) return;

		grounded = BuildTrace( Transform.Position, Transform.Position ).Size( 15f ).Run().Hit;

		if ( !grounded )
		{
			TimeSinceGrounded = 0;
			Gravity = -Vector3.Up * 800f * Time.Delta;
		}
		else
		{
			Gravity = Vector3.Zero;
			if ( TimeSinceGrounded > 3f )
			{
				StopPhysics = true;
			}
		}

		var helper = new CharacterControllerHelper( BuildTrace( Transform.Position, Transform.Position ), Transform.Position, Velocity + Gravity );
		helper.Trace.Size( 15f );
		helper.Bounce = 0.3f;
		helper.TryMove( Time.Delta );

		Transform.Rotation = helper.Velocity.Length > 1f ? Rotation.LookAt( helper.Velocity ) * Rotation.FromPitch( 90 ) : Transform.Rotation;

		Transform.Position = helper.Position;
		Velocity = helper.Velocity;
	}
}
