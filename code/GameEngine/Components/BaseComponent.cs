﻿using Sandbox;
using System;
using System.Collections.Generic;

public abstract partial class BaseComponent
{
	public Scene Scene => GameObject.Scene;
	public GameTransform Transform => GameObject.Transform;

	public GameObject GameObject { get; internal set; }

	public GameObjectFlags Flags { get; set; } = GameObjectFlags.None;

	/// <summary>
	/// Internal functions to be called when the object wakes up
	/// </summary>
	Action onAwake;

	bool _enabledState;
	bool _enabled = false;

	public bool Enabled
	{
		get => _enabled;

		set
		{
			if ( _enabled == value ) return;

			_enabled = value;

			SceneUtility.ActivateComponent( this );
		}
	}

	public bool Active
	{
		get => _enabledState;
	}

	public string Name { get; set; }

	public virtual void DrawGizmos() { }

	public virtual void OnEnabled() { }

	public virtual void OnDisabled() { } 
	public virtual void OnDestroyInternal() 
	{
		if ( _enabledState )
		{
			_enabledState = false;
			OnDisabled();
		}

		GameObject = null;
	} 

	protected virtual void OnPostPhysics() { }
	internal void PostPhysics() 
	{
		OnPostPhysics();
	}

	protected virtual void OnPreRender() { }
	internal virtual void PreRender() 
	{
		OnPreRender();
	}

	internal virtual void InternalUpdate() 
	{
		if ( !Enabled ) return;

		Update();
	}

	internal void UpdateEnabledStatus()
	{
		var state = _enabled && Scene is not null && GameObject is not null && GameObject.Active;
		if ( state  == _enabledState ) return;

		_enabledState = state;

		if ( _enabledState )
		{
			onAwake?.Invoke();
			onAwake = null;

			ExceptionWrap( "OnEnabled", OnEnabled );
		}
		else
		{
			ExceptionWrap( "OnDisabled", OnDisabled );
		}
	}

	public void Destroy()
	{
		Enabled = false;
		GameObject.Components.Remove( this );
	}

	public virtual void Reset()
	{

	}

	void ExceptionWrap( string name, Action a )
	{
		try
		{
			a();
		}
		catch (System.Exception e ) 
		{
			Log.Error( e, $"Exception when calling '{name}' on {this}" );
		}
	}

	public virtual void Update()
	{

	}

	public virtual void FixedUpdate()
	{

	}

	public virtual void EditorUpdate()
	{

	}

	/// <summary>
	/// Called when something on the component has been edited
	/// </summary>
	public void EditLog( string name, object source, Action undo )
	{
		GameObject.EditLog( name, source, undo );
	}
	
	/// <inheritdoc cref="GameObject.GetComponent{T}(bool, bool)"/>
	public T GetComponent<T>( bool enabledOnly = true, bool deep = false ) => GameObject.GetComponent<T>( enabledOnly, deep );

	/// <inheritdoc cref="GameObject.GetComponents{T}(bool, bool)"/>
	public IEnumerable<T> GetComponents<T>( bool enabledOnly = true, bool deep = false ) => GameObject.GetComponents<T>( enabledOnly, deep );
}
