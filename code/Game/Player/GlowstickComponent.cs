using Sandbox;

namespace BrickJam.Player;

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

	public GameObject Player;

	public override void OnStart()
	{
		model = GetComponent<ModelComponent>( false, true );
		light = GetComponent<PointLightComponent>( false, true );
		initialLight = light.LightColor;
	}

	PhysicsTraceBuilder BuildTrace( Vector3 from, Vector3 to ) => BuildTrace( Scene.PhysicsWorld.Trace.Ray( from, to ).WithoutTags( "trigger", "solid" ) );
	PhysicsTraceBuilder BuildTrace( PhysicsTraceBuilder source ) => source.Size( GameObject.GetBounds() ).WithoutTags( "trigger", "solid" );

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

		if ( StopPhysics ) return;

		Gravity = -Vector3.Up * 800f * Time.Delta;


		var helper = new CharacterControllerHelper( BuildTrace( Transform.Position, Transform.Position ), Transform.Position, Velocity + Gravity );
		helper.Bounce = 0.3f;
		helper.TryMove( Time.Delta );

		Transform.Rotation = helper.Velocity.Length > 1f ? Rotation.LookAt( helper.Velocity ) * Rotation.FromPitch( 90 ) : Transform.Rotation;

		Transform.Position = helper.Position;
		Velocity = helper.Velocity;
		var tr = Physics.Trace.Ray( Transform.Position + Vector3.Up * 15f, Transform.Position + Velocity.Normal ).WithoutTags( "trigger" ).Run();
		if ( tr.Hit )
		{
			StopPhysics = true;
			Transform.Position = tr.EndPosition + tr.Normal * 2f;
			Transform.Rotation = Rotation.LookAt( tr.Normal, Transform.Rotation.Up );
		}
	}
}
