using Sandbox;

namespace BrickJam.Components;

[Title( "Follow Attachment" )]
[Category( "Render" )]
[Icon( "videocam", "red", "white" )]
public sealed class FollowAttachmentComponent : BaseComponent
{
	[Property] public GameObject FollowObject { get; set; }

	[Property] public string AttachmentName { get; set; }

	protected override void OnPreRender()
	{
		if ( FollowObject != null )
		{
			var attachment = FollowObject.GetComponent<AnimatedModelComponent>().GetAttachmentTransform( AttachmentName );

			Transform.Position = attachment.Position;
			Transform.Rotation = attachment.Rotation;
		}
	}
}
