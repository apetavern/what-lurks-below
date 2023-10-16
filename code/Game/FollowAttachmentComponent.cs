using Sandbox;

[Title( "Follow Attachment" )]
[Category( "Render" )]
[Icon( "videocam", "red", "white" )]
public sealed class FollowAttachmentComponent : BaseComponent
{
	[Property] GameObject FollowObject { get; set; }

	[Property] string AttachmentName { get; set; }

	public override void Update()
	{
		var attachment = FollowObject.GetComponent<AnimatedModelComponent>().GetAttachmentTransform( AttachmentName );

		Transform.Position = attachment.Position;
		Transform.Rotation = attachment.Rotation;
	}
}
