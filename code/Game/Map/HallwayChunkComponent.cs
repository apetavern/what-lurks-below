using Sandbox;

namespace BrickJam.Map;

public sealed class HallwayChunkComponent : BaseComponent
{
	[Property] public GameObject Front { get; set; }
	[Property] public GameObject Back { get; set; }
	[Property] public GameObject Left { get; set; }
	[Property] public GameObject Right { get; set; }

	public void CheckSides()
	{
		Transform.Rotation = Rotation.Identity;

		var trFront = Physics.Trace.Ray(
			Transform.Position + Vector3.Forward * 128f + Vector3.Up * 10f,
			Transform.Position + Vector3.Forward * 128f + Vector3.Down * 128f
		).Run();

		var trBack = Physics.Trace.Ray(
			Transform.Position + Vector3.Backward * 128f + Vector3.Up * 10f,
			Transform.Position + Vector3.Backward * 128f + Vector3.Down * 128f
		).Run();

		var trLeft = Physics.Trace.Ray(
			Transform.Position + Vector3.Left * 128f + Vector3.Up * 10f,
			Transform.Position + Vector3.Left * 128f + Vector3.Down * 128f
		).Run();

		var trRight = Physics.Trace.Ray(
			Transform.Position + Vector3.Right * 128f + Vector3.Up * 10f,
			Transform.Position + Vector3.Right * 128f + Vector3.Down * 128f
		).Run();

		if ( trFront.Hit && trFront.Body.GameObject != null && (trFront.Body.GameObject as GameObject).GetComponent<RoomChunkComponent>( false ) == null )
		{
			Front.Destroy();
		}

		if ( trBack.Hit && trBack.Body.GameObject != null && (trBack.Body.GameObject as GameObject).GetComponent<RoomChunkComponent>( false ) == null )
		{
			Back.Destroy();
		}

		if ( trLeft.Hit && trLeft.Body.GameObject != null && (trLeft.Body.GameObject as GameObject).GetComponent<RoomChunkComponent>( false ) == null )
		{
			Left.Destroy();
		}

		if ( trRight.Hit && trRight.Body.GameObject != null && (trRight.Body.GameObject as GameObject).GetComponent<RoomChunkComponent>( false ) == null )
		{
			Right.Destroy();
		}

		foreach ( var item in GetComponents<Collider>( false, true ) )
		{
			item.OnPhysicsChanged();
		}

	}
}
