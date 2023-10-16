using Sandbox;
using Sandbox.Diagnostics;
using Sandbox.Internal;

[Title( "Particle Renderer" )]
[Category( "Effects" )]
[Icon( "sprinkler", "red", "white" )]
[EditorHandle( "materials/gizmo/particles.png" )]
public class ParticleComponent : BaseComponent, BaseComponent.ExecuteInEditor
{
	ParticleSystem _particles;

	[Property]
	public ParticleSystem Particles
	{
		get => _particles;
		set
		{
			if ( value != null && _particles == value )
				return;

			_particles = value;
			CreateSceneParticles();
		}
	}

	[Property] public bool RunOnStart { get; set; }

	SceneParticles _sceneParticles;
	public SceneParticles SceneParticles => _sceneParticles;

	public override void DrawGizmos()
	{
		if ( Particles is null )
			return;
	}

	public override void OnStart()
	{
		if ( !RunOnStart )
			return;

		_sceneParticles?.Delete();
	}

	public override void OnEnabled()
	{
		Assert.NotNull( Scene );

		if ( Particles is not null )
			CreateSceneParticles();
	}

	private void CreateSceneParticles()
	{
		_sceneParticles?.Delete();
		_sceneParticles = new SceneParticles( Scene.SceneWorld, Particles );
		_sceneParticles.RenderParticles = true;
		_sceneParticles.Transform = Transform.World;
	}

	public override void OnDisabled()
	{
		_sceneParticles?.Delete();
		_sceneParticles = null;
	}

	public override void Update()
	{
		base.Update();

		if ( !_sceneParticles.IsValid() )
			return;

		_sceneParticles?.SetControlPoint( 0, Transform.World );
		_sceneParticles?.Simulate( Time.Delta );
	}

	protected override void OnPreRender()
	{
		if ( !_sceneParticles.IsValid() )
			return;

		_sceneParticles.Transform = Transform.World;
	}
}
