using System;

namespace Sandbox;

[CustomEditor( typeof( Array ) )]
public sealed class ArrayEditor : CollectionEditor
{
	public Array Value
	{
		get => SerializedProperty.GetValue<Array>();
		set => SerializedProperty.SetValue( value );
	}

	protected override Type ElementType => SerializedProperty.PropertyType.GetElementType();
	protected override int Length => Value.Length;

	public ArrayEditor( SerializedProperty property ) : base( property )
	{
		Value ??= Array.CreateInstance( ElementType, 0 );

		RecreateContent();
	}

	protected override void OnSizeChanged( int newSize )
	{
		var clampedSize = Math.Clamp( newSize, 0, Array.MaxLength );
		if ( newSize != clampedSize )
			LengthEditor.Value = clampedSize;

		ResizeArray( clampedSize );
		RecreateContent();
	}

	private void ResizeArray( int newSize )
	{
		var oldArray = Value;
		var newArray = Array.CreateInstance( ElementType, newSize );
		var copyLength = Math.Min( oldArray.Length, newArray.Length );

		Array.Copy( oldArray, newArray, copyLength );
		Value = newArray;
	}

	protected override void AddNewElement()
	{
		OnSizeChanged( Value.Length + 1 );
		RecreateContent();
	}

	protected override void RemoveElement( int index )
	{
		var array = Value;
		for ( var i = index + 1; i < array.Length; i++ )
			array.SetValue( array.GetValue( i ), i - 1 );

		ResizeArray( array.Length - 1 );
		RecreateContent();
	}

	protected override object GetElementAt( int index ) => Value.GetValue( index );
	protected override void SetElementAt( object value, int index ) => Value.SetValue( value, index );
}
