using System;
using System.Diagnostics.CodeAnalysis;

namespace Sandbox;

public abstract class CollectionEditor : ControlWidget
{
	[MemberNotNullWhen( true, nameof( LengthEditor ) )]
	protected virtual bool IsFixedSize
	{
		get
		{
			if ( !SerializedProperty.TryGetAttribute<CollectionSizingAttribute>( out var collectionSizingAttribute ) )
				return false;

			return collectionSizingAttribute.CollectionSizeMode == CollectionSizeMode.Fixed;
		}
	}

	protected abstract Type ElementType { get; }
	protected abstract int Length { get; }

	protected Layout Content { get; private set; }
	protected IntProperty LengthEditor { get; private set; }

	protected CollectionEditor( SerializedProperty property ) : base( property )
	{
		SetSizeMode( SizeMode.Default, SizeMode.Default );
		Layout = Layout.Column();
		Layout.Spacing = 2;
	}

	protected virtual void RecreateContent()
	{
		Content?.Clear( true );
		Content = Layout.AddColumn();

		if ( IsFixedSize )
		{
			// Fixed size editor.
			LengthEditor = Content.Add( new IntProperty( "Size", this ) );
			LengthEditor.Value = Length;
			LengthEditor.OnValueEdited += () => OnSizeChanged( LengthEditor.Value );
		}

		// Element editors.
		var elementType = ElementType;
		for ( var i = 0; i < Length; i++ )
		{
			Content.AddSpacingCell( 2 );

			var layout = IsFixedSize ? Content : Content.AddRow();
			var index = i;

			if ( !IsFixedSize )
				layout.Add( new IconButton( "remove", () => RemoveElement( index ), this ) );

			var elementEditor = CanEditAttribute.CreateEditorFor( elementType );
			if ( elementEditor is null )
			{
				Log.Error( $"{elementType} has no editor" );
				return;
			}

			layout.Add( elementEditor );
			elementEditor.Bind( "Value" ).From( () => GetElementAt( index ), obj => SetElementAt( obj, index ) );
		}

		if ( !IsFixedSize )
			Content.Add( new IconButton( "add", AddNewElement, this ) );
	}

	protected virtual void AddNewElement()
	{
	}

	protected virtual void RemoveElement( int index )
	{
	}

	protected virtual void OnSizeChanged( int newSize )
	{
	}

	protected abstract object GetElementAt( int index );
	protected abstract void SetElementAt( object value, int index );
}
