using Sandbox;
using Sandbox.Diagnostics;
using System;

public sealed partial class AnimatedModelComponent
{
	public Transform GetAttachmentTransform( string name )
	{
		return SceneObject.GetAttachment( name ).Value;
	}
}
