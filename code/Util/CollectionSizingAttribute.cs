namespace Sandbox;

[AttributeUsage( AttributeTargets.Class )]
public class CollectionSizingAttribute : Attribute
{
	public CollectionSizeMode CollectionSizeMode { get; }

	public CollectionSizingAttribute( CollectionSizeMode collectionSizeMode )
	{
		CollectionSizeMode = collectionSizeMode;
	}
}
