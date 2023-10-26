namespace BrickJam;

/// <summary>
/// Used as a tag for all containers that can drop items.
/// </summary>
// FIXME: Changes to a prefabs tags don't propagate to prefabs that contain the prefab. Use tags once that is sorted.
[Title( "ItemContainer" )]
[Category( "Map" )]
[Icon( "colorize", "red", "white" )]
[EditorHandle( "materials/gizmo/items.png" )]
public sealed class ItemContainer : BaseComponent
{
}
