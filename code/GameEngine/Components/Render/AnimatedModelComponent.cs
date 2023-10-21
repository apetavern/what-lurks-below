﻿using Sandbox;
using Sandbox.Diagnostics;
using System;

[Title( "Animated Model Renderer" )]
[Category( "Rendering" )]
[Icon( "sports_martial_arts" )]
public class AnimatedModelComponent : BaseComponent, BaseComponent.ExecuteInEditor
{
	Model _model;

	public BBox Bounds
	{
		get
		{
			if ( _sceneObject is not null )
			{
				return _sceneObject.Bounds;
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

			if ( _sceneObject is not null )
			{
				_sceneObject.Model = _model;
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

			if ( _sceneObject is not null )
			{
				_sceneObject.ColorTint = Tint;
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

			if ( _sceneObject is not null )
			{
				_sceneObject.SetMaterialOverride( _material );
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

			if ( _sceneObject is not null )
			{
				_sceneObject.Flags.CastShadows = _castShadows;
			}
		}
	}

	public string TestString { get; set; }

	SceneModel _sceneObject;
	public SceneModel SceneObject => _sceneObject;


	public override void DrawGizmos()
	{
		if ( Model is null )
			return;

		Gizmo.Hitbox.Model( Model );

		Gizmo.Draw.Color = Color.White.WithAlpha( 0.1f );

		if ( Gizmo.IsSelected )
		{
			Gizmo.Draw.Color = Color.White.WithAlpha( 0.9f );
			Gizmo.Draw.LineBBox( Model.Bounds );
		}

		if ( Gizmo.IsHovered )
		{
			Gizmo.Draw.Color = Color.White.WithAlpha( 0.4f );
			Gizmo.Draw.LineBBox( Model.Bounds );
		}
	}

	public override void OnEnabled()
	{
		Assert.True( _sceneObject == null );
		Assert.NotNull( Scene );

		var model = Model ?? Model.Load( "models/dev/box.vmdl" );

		_sceneObject = new SceneModel( Scene.SceneWorld, model, Transform.World );
		_sceneObject.SetMaterialOverride( MaterialOverride );
		_sceneObject.ColorTint = Tint;
		_sceneObject.Flags.CastShadows = _castShadows;
	}

	public override void OnDisabled()
	{
		_sceneObject?.Delete();
		_sceneObject = null;
	}

	protected override void OnPreRender()
	{
		if ( !_sceneObject.IsValid() )
			return;

		_sceneObject.Transform = Transform.World;
		_sceneObject.Update( Time.Delta );
	}

	public void Set( string v, Vector3 value ) => _sceneObject.SetAnimParameter( v, value );
	public void Set( string v, int value ) => _sceneObject.SetAnimParameter( v, value );
	public void Set( string v, float value ) => _sceneObject.SetAnimParameter( v, value );
	public void Set( string v, bool value ) => _sceneObject.SetAnimParameter( v, value );
	//	public void Set( string v, Enum value ) => _sceneObject.SetAnimParameter( v, value );

	public bool GetBool( string v ) => _sceneObject.GetBool( v );
	public int GetInt( string v ) => _sceneObject.GetInt( v );
	public float GetFloat( string v ) => _sceneObject.GetFloat( v );
	public Vector3 GetVector( string v ) => _sceneObject.GetVector3( v );
	public Rotation GetRotation( string v ) => _sceneObject.GetRotation( v );

	/// <summary>
	/// Converts value to vector local to this entity's eyepos and passes it to SetAnimVector
	/// </summary>
	public void SetLookDirection( string name, Vector3 eyeDirectionWorld )
	{
		var delta = eyeDirectionWorld * Transform.Rotation.Inverse;
		Set( name, delta );
	}

	public Transform GetAttachmentTransform( string attachmentName )
	{
		return SceneObject.GetAttachment( attachmentName ).Value;
	}

	public Transform GetBoneTransform( string attachmentName )
	{
		return SceneObject.GetBoneWorldTransform( attachmentName );
	}
}
