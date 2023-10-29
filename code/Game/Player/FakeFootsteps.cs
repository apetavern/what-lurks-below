using Sandbox;

namespace BrickJam.Player;

public sealed class FakeFootsteps : BaseComponent
{
	[Property] public string footstepSound { get; set; }

	public enum FootState
	{
		Rising,
		Falling
	}

	[Property] public float RaisedFootHeight { get; set; } = 1.5f;
	[Property] public float LoweredFootHeight { get; set; } = 1f;

	private AnimatedModelComponent AnimationComponent { get; set; }
	[Property] public FootState LeftFootState { get; set; }
	[Property] public FootState RightFootState { get; set; }

	public override void Update()
	{
		base.Update();

		if ( LeftFootState == FootState.Falling && GetFootHeight( 0 ) <= LoweredFootHeight )
		{
			DoFootstep( 0 );
			LeftFootState = FootState.Rising;
		}
		if ( RightFootState == FootState.Falling && GetFootHeight( 1 ) <= LoweredFootHeight )
		{
			DoFootstep( 1 );
			RightFootState = FootState.Rising;
		}
		if ( LeftFootState == FootState.Rising && GetFootHeight( 0 ) >= RaisedFootHeight )
		{
			LeftFootState = FootState.Falling;
		}
		if ( RightFootState == FootState.Rising && GetFootHeight( 1 ) >= RaisedFootHeight )
		{
			RightFootState = FootState.Falling;
		}
	}

	private float GetFootHeight( int foot )
	{
		var bone = foot == 0 ? "ball_L" : "ball_R";
		AnimationComponent ??= GetComponent<AnimatedModelComponent>();
		var boneTx = AnimationComponent.SceneObject.GetBoneWorldTransform( bone );
		var footHeight = GameObject.Transform.World.PointToLocal( boneTx.Position ).z;
		return footHeight;
	}

	private void DoFootstep( int foot )
	{
		// Log.Info( $"Footstep for {(foot == 0 ? "left" : "right")} foot." );
		Sound.FromWorld( footstepSound, Transform.Position );
	}
}
