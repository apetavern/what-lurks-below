using Sandbox;
using Sandbox.Diagnostics;
using static Sandbox.VisibilityBoxEntity;

[Title( "Animated Model Renderer" )]
[Category( "Rendering" )]
[Icon( "visibility", "red", "white" )]
[Alias( "ModelComponentMate" )]
public class AnimatedModelComponent : BaseComponent, BaseComponent.ExecuteInEditor
{
	Model _model;

	public BBox Bounds
	{
		get
		{
			if ( _sceneModel is not null )
			{
				return _sceneModel.Bounds;
			}

			return new BBox( Transform.Position, 16 );
		}
	}

	[Property]
	public Model Model
	{
		get => _model;
		set
		{
			if ( _model == value ) return;
			_model = value;

			if ( _sceneModel is not null )
			{
				_sceneModel.Model = _model;
			}
		}
	}


	Color _tint = Color.White;

	[Property]
	public Color Tint
	{
		get => _tint;
		set
		{
			if ( _tint == value ) return;

			_tint = value;

			if ( _sceneModel is not null )
			{
				_sceneModel.ColorTint = Tint;
			}
		}
	}

	Material _material;
	[Property]
	public Material MaterialOverride
	{
		get => _material;
		set
		{
			if ( _material == value ) return;
			_material = value;

			if ( _sceneModel is not null )
			{
				_sceneModel.SetMaterialOverride( _material );
			}
		}
	}

	bool _castShadows = true;
	[Property]
	public bool ShouldCastShadows
	{
		get => _castShadows;
		set
		{
			if ( _castShadows == value ) return;
			_castShadows = value;

			if ( _sceneModel is not null )
			{
				_sceneModel.Flags.CastShadows = _castShadows;
			}
		}
	}

	public string TestString { get; set; }

	SceneModel _sceneModel;
	public SceneModel SceneModel => _sceneModel;

	public override void DrawGizmos()
	{
		if ( Model is null )
			return;

		if ( GameObject.Parent.GetComponent<AnimatedModelComponent>() == null )
		{
			Gizmo.Hitbox.Model( Model );
		}
		else
		{
			Gizmo.Hitbox.BBox( new BBox( Model.Bounds.Mins - Transform.LocalPosition, Model.Bounds.Maxs - Transform.LocalPosition ) );
		}

		Gizmo.Draw.Color = Color.White.WithAlpha( 0.1f );

		if ( Gizmo.IsSelected )
		{
			Gizmo.Draw.Color = Color.White.WithAlpha( 0.9f );
			Gizmo.Draw.LineBBox( new BBox( Model.Bounds.Mins - Transform.LocalPosition, Model.Bounds.Maxs - Transform.LocalPosition ) );
		}

		if ( Gizmo.IsHovered )
		{
			Gizmo.Draw.Color = Color.White.WithAlpha( 0.4f );
			Gizmo.Draw.LineBBox( new BBox( Model.Bounds.Mins - Transform.LocalPosition, Model.Bounds.Maxs - Transform.LocalPosition ) );
		}




	}

	public override void OnEnabled()
	{
		Assert.True( _sceneModel == null );
		Assert.NotNull( Scene );

		var parent = GameObject.Parent.GetComponent<AnimatedModelComponent>();

		_sceneModel = new SceneModel( Scene.SceneWorld, Model ?? Model.Load( "models/dev/box.vmdl" ), parent != null ? parent.SceneModel.Transform : Transform.World );

		_sceneModel.Transform = parent != null ? GameObject.Parent.GetComponent<AnimatedModelComponent>().SceneModel.Transform : Transform.World;
		_sceneModel.SetMaterialOverride( MaterialOverride );
		_sceneModel.ColorTint = Tint;
		_sceneModel.Flags.CastShadows = _castShadows;

		if ( parent is not null )
		{
			parent.SceneModel.AddChild( GameObject.Name, SceneModel );
		}
	}

	public override void OnDisabled()
	{
		_sceneModel?.Delete();
		_sceneModel = null;
	}

	protected override void OnPreRender()
	{
		if ( !_sceneModel.IsValid() )
			return;

		_sceneModel.Transform = Transform.World;

		var parent = GameObject.Parent.GetComponent<AnimatedModelComponent>();

		if ( parent is not null )
		{
			_sceneModel.Transform = parent.SceneModel.Transform;

			parent.SceneModel.AddChild( GameObject.Name, SceneModel );
			Transform.Position = SceneModel.Bounds.Center;

			var newbounds = parent.SceneModel.Bounds;
			newbounds.AddPoint( parent.SceneModel.Transform.Position + Vector3.Up * 64 );
			SceneModel.Bounds = newbounds;
		}
	}

	public Transform GetAttachmentTransform( string attachmentName )
	{
		return SceneModel.GetAttachment( attachmentName ).Value;
	}

	public Transform GetBoneTransform( string attachmentName )
	{
		return SceneModel.GetBoneWorldTransform( attachmentName );
	}

	public override void Update()
	{
		base.Update();
		_sceneModel.Update( Time.Delta );
	}
}
