using System;

namespace BrickJam.Components;

public abstract class SingletonComponent<T> : BaseComponent where T : SingletonComponent<T>
{
	public static T Instance { get; private set; }

	protected virtual bool ThrowOnDuplicate => false;

	public SingletonComponent()
	{
		if ( Instance is not null && ThrowOnDuplicate )
			throw new InvalidOperationException( $"An instance of the {typeof( T ).Name} component already exists" );

		Instance = (T)this;
	}
}
