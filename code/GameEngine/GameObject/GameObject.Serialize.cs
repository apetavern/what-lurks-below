﻿using Sandbox;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;

public partial class GameObject
{
	public class SerializeOptions
	{
	}

	/// <summary>
	/// Destroy all components and child objects
	/// </summary>
	public void Clear()
	{
		// Remove all of our components
		ForEachComponent( "DestroyComponents", true, c => c.Destroy() );

		// Remove all of our children
		foreach ( var child in Children.ToArray() )
		{
			child.DestroyRecursive();
		}
	}

	//
	// For flexibility purposes, we serialize the GameObject manually
	// into a JsonObject. I haven't benchmarked this, but I assume it's okay.
	//

	public JsonObject Serialize( SerializeOptions options = null )
	{
		if ( Flags.HasFlag( GameObjectFlags.NotSaved ) )
			return null;

		bool isPartOfPrefab = IsPrefabInstance;

		var json = new JsonObject
		{
			{ "Id", Id },
			{ "Name", Name },
			{ "Enabled", Enabled },
			{ "Position",  JsonValue.Create( Transform.LocalPosition ) },
			{ "Rotation", JsonValue.Create( Transform.LocalRotation ) },
			{ "Scale", JsonValue.Create( Transform.LocalScale ) }
		};

		if ( IsPrefabInstanceRoot )
		{
			json.Add( "__Prefab", JsonValue.Create( PrefabSource ) );

			// prefabs don't save their children
			return json;
		}

		if ( Components.Any() && !isPartOfPrefab )
		{
			var components = new JsonArray();

			ForEachComponent( "Serialize", false, component =>
			{
				var result = component.Serialize( options );
				if ( result is null ) return;

				components.Add( result );

			} );

			json.Add( "Components", components );
		}

		if ( Children.Any() )
		{
			var children = new JsonArray();

			foreach( var child in Children )
			{
				try
				{
					var result = child.Serialize( options );

					if ( result is not null )
					{
						children.Add( result );
					}
				}
				catch ( System.Exception e )
				{
					Log.Warning( e, $"Exception when serializing GameObject" );
				}
			}

			json.Add( "Children", children );
		}

		return json;
	}

	public void Deserialize( JsonObject node )
	{
		Id = node["Id"].Deserialize<Guid>();
		Name = node["Name"].ToString() ?? Name;
		Transform.LocalPosition = node["Position"].Deserialize<Vector3>();
		Transform.LocalRotation = node["Rotation"].Deserialize<Rotation>();
		Transform.LocalScale = node["Scale"].Deserialize<Vector3>();

		if ( node["__Prefab"].Deserialize<string>() is string prefabSource )
		{
			SetPrefabSource( prefabSource );

			var prefabFile = ResourceLibrary.Get<PrefabFile>( prefabSource );
			if ( prefabFile  is null )
			{
				Log.Warning( $"Unable to load prefab '{prefabSource}'" );
				return;
			}

			node = prefabFile.RootObject.Deserialize<JsonObject>(); // make a copy
			SceneUtility.MakeGameObjectsUnique( node ); // change all the guids
		}

		if ( node["Children"] is JsonArray childArray )
		{
			foreach( var child in  childArray )
			{
				if ( child is not JsonObject jso )
					return;

				var go = GameObject.Create();
				
				go.Parent = this;

				go.Deserialize( jso );
			}
		}

		if ( node["Components"] is JsonArray componentArray )
		{
			foreach( var component in componentArray )
			{
				if ( component is not JsonObject jso )
				{
					Log.Warning( $"Component entry is not an object!" );
					return;
				}

				var componentType = TypeLibrary.GetType<BaseComponent>( (string)jso["__type"] );
				if ( componentType is null )
				{
					Log.Warning( $"TypeLibrary couldn't find BaseComponent type {jso["__type"]}" );
					return;
				}
				
				var c = this.AddComponent( componentType );
				if ( c is null ) continue;
				
				c.Deserialize( jso );
			}
		}

		Enabled = (bool)(node["Enabled"] ?? Enabled);
	}

	public PrefabFile GetAsPrefab()
	{
		var a = new PrefabFile();
		a.RootObject = Serialize();
		return a;
	}
}
