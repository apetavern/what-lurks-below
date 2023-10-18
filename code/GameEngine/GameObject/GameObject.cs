﻿using Sandbox;
using Sandbox.Diagnostics;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;


public partial class GameObject
{
	public Scene Scene { get; private set; }

	public GameTransform Transform { get; private set; }

	[Property]
	public string Name { get; set; } = "Untitled Object";


	bool _enabled = true;

	/// <summary>
	/// Is this gameobject enabled?
	/// </summary>
	[Property]
	public bool Enabled
	{
		get => _enabled;
		set
		{
			if ( _enabled == value )
				return;

			_enabled = value;

			SceneUtility.ActivateGameObject( this );
		}
	}

	internal GameObject( bool enabled, string name )
	{
		Transform = new GameTransform( this );
		_enabled = enabled;
		Scene = this as Scene ?? GameManager.ActiveScene;
		Id = Guid.NewGuid();
		Name = name;
	}

	public override string ToString()
	{
		return $"GameObject:{Name}";
	}

	public List<BaseComponent> Components = new List<BaseComponent>();

	GameObject _parent;

	public GameObject Parent
	{
		get => _parent;
		set
		{
			if ( _parent == value ) return;

			if ( value is not null && value.IsAncestor( this ) )
			{
				Log.Warning( $"Illegal parentage" );
				return;
			}

			var oldParent = _parent;
			oldParent?.RemoveChild( this );

			_parent = value;

			if ( _parent is not null )
			{
				Assert.True( Scene == _parent.Scene, "Can't parent to a gameobject in a different scene" );
				_parent.Children.Add( this );
			}
		}
	}

	public List<GameObject> Children { get; } = new List<GameObject>();

	/// <summary>
	/// Is this gameobject active. For it to be active, it needs to be enabled, all of its ancestors
	/// need to be enabled, and it needs to be in a scene.
	/// </summary>
	public bool Active => Enabled && Scene is not null && (Parent?.Active ?? true);


	internal void PostPhysics()
	{
		//Gizmo.Draw.LineSphere( new Sphere( WorldTransform.Position, 3 ) );

		ForEachComponent( "PostPhysics", true, c => c.PostPhysics() );
		ForEachChild( "PostPhysics", true, c => c.PostPhysics() );
	}

	internal void PreRender()
	{
		ForEachComponent( "PreRender", true, c => c.PreRender() );
		ForEachChild( "PreRender", true, c => c.PreRender() );
	}

	internal void ForEachChild( string name, bool activeOnly, Action<GameObject> action )
	{
		for ( int i = Children.Count - 1; i >= 0; i-- )
		{
			var c = Children[i];

			if ( c is null )
			{
				Children.RemoveAt( i );
				continue;
			}

			if ( activeOnly && !c.Active )
				continue;

			try
			{
				action( c );
			}
			catch ( System.Exception e )
			{
				Log.Warning( e, $"Exception when calling {name} on {c}: {e.Message}" );
			}
		}
	}

	internal virtual void Tick()
	{
		if ( !Enabled )
			return;

		ForEachComponent( "Update", true, c => c.InternalUpdate() );
		ForEachChild( "Tick", true, x => x.Tick() );
	}


	/// <summary>
	/// Should be called whenever we change anything that we suspect might
	/// cause the active status to change on us, or our components. Don't call
	/// this directly. Only call it via SceneUtility.ActivateGameObject( this );
	/// </summary>
	internal void UpdateEnabledStatus()
	{
		ForEachComponent( "UpdateEnabledStatus", false, c =>
		{
			c.GameObject = this;
			c.UpdateEnabledStatus();
		} );

		ForEachChild( "UpdateEnabledStatus", true, c => c.UpdateEnabledStatus() );
	}

	public bool IsDescendant( GameObject o )
	{
		return o.IsAncestor( this );
	}

	public bool IsAncestor( GameObject o )
	{
		if ( o == this ) return true;

		if ( Parent is not null )
		{
			return Parent.IsAncestor( o );
		}

		return false;
	}

	public void AddSibling( GameObject go, bool before, bool keepWorldPosition = true )
	{
		if ( this is Scene ) throw new InvalidOperationException( "Can't add a sibling to a scene!" );

		go.SetParent( Parent, keepWorldPosition );

		go.Parent.Children.Remove( go );
		var targetIndex = go.Parent.Children.IndexOf( this );
		if ( !before ) targetIndex++;
		go.Parent.Children.Insert( targetIndex, go );
	}

	public void SetParent( GameObject value, bool keepWorldPosition = true )
	{
		if ( this is Scene ) throw new InvalidOperationException( "Can't set the parent of a scene!" );

		if ( Parent == value ) return;

		if ( keepWorldPosition )
		{
			var wp = Transform.World;
			Parent = value;
			Transform.World = wp;
		}
		else
		{
			Parent = value;
		}
	}

	IEnumerable<GameObject> GetSiblings()
	{
		if ( Parent is not null )
		{
			return Parent.Children.Where( x => x != this );
		}

		return Enumerable.Empty<GameObject>();
	}

	// todo - this should be internal
	public void MakeNameUnique()
	{
		var names = GetSiblings().Select( x => x.Name ).ToHashSet();

		if ( !names.Contains( Name ) )
			return;

		var targetName = Name;

		// todo regex (number)

		if ( targetName.Contains( '(' ) )
		{
			targetName = targetName.Substring( 0, targetName.LastIndexOf( '(' ) ).Trim();
		}

		for ( int i = 1; i < 500; i++ )
		{
			var indexedName = $"{targetName} ({i})";

			if ( !names.Contains( indexedName ) )
			{
				Name = indexedName;
				return;
			}
		}
	}

	public IEnumerable<GameObject> GetAllObjects( bool enabled )
	{
		if ( enabled && !Enabled )
			yield break;

		yield return this;

		foreach ( var child in Children.OfType<GameObject>().SelectMany( x => x.GetAllObjects( enabled ) ).ToArray() )
		{
			yield return child;
		}
	}

	public virtual void EditLog( string name, object source )
	{
		if ( Parent == null ) return;

		Parent.EditLog( name, source );
	}

	/// <summary>
	/// This is slow, and somewhat innacurate. Don't call it every frame!
	/// </summary>
	public BBox GetBounds()
	{
		var renderers = GetComponents<ModelComponent>( true, true );

		var animatedrenderers = GetComponents<AnimatedModelComponent>( true, true );

		IEnumerable<BBox> boxes;

		if ( renderers.Count() > 0 )
		{
			boxes = renderers.Select( x => x.Bounds );
		}
		else if ( animatedrenderers.Count() > 0 )
		{
			boxes = animatedrenderers.Select( x => x.Bounds );
		}
		else
		{
			boxes = Enumerable.Repeat( new BBox( Transform.Position, 1f ), 1 );
		}



		return BBox.FromBoxes( boxes );
	}

	/// <summary>
	/// Get the GameObject after us,
	/// </summary>
	public GameObject GetNextSibling( bool enabledOnly )
	{
		if ( Parent is null ) return null;
		var myIndex = Parent.Children.IndexOf( this );
		if ( myIndex < 0 ) return null;

		for ( int i = myIndex + 1; i < Parent.Children.Count; i++ )
		{
			if ( Parent.Children[i] is null ) continue;
			if ( enabledOnly && !Parent.Children[i].Enabled ) continue;
			return Parent.Children[i];
		}

		return null;
	}
}
