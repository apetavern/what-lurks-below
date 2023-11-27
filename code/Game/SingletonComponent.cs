using System;

namespace BrickJam.Components;

public abstract class SingletonComponent<T> : BaseComponent where T : SingletonComponent<T>
{
	public static T Instance { get; set; }

	protected virtual bool ThrowOnDuplicate => false;

	protected virtual bool NonProxyOnly => true;

	public SingletonComponent()
	{

	}

	public override void OnAwake()
	{
		if ( Instance is not null && ThrowOnDuplicate )
			throw new InvalidOperationException( $"An instance of the {typeof( T ).Name} component already exists" );

		if ( NonProxyOnly && !IsProxy )
		{
			Instance = (T)this;
		}
		else if ( !NonProxyOnly )
		{
			Instance = (T)this;
		}
	}
}
