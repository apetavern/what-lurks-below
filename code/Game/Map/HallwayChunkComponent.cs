using Sandbox;

namespace BrickJam.Map;

public sealed class HallwayChunkComponent : BaseComponent
{
	[Property] public GameObject Front { get; set; }
	[Property] public GameObject Back { get; set; }
	[Property] public GameObject Left { get; set; }
	[Property] public GameObject Right { get; set; }

	float SpawnTime = 0f;

	bool FixedWalls;

	public override void OnStart()
	{
		base.OnStart();
		if ( IsProxy )
		{
			SpawnTime = Time.Now;
		}
	}

	public override void Update()
	{
		if ( !IsProxy )
		{
			return;
		}

		if ( !FixedWalls && SpawnTime != 0f && Time.Now - SpawnTime > 1f && NavGenComponent.Instance.Initialized )
		{
			CheckSides();
			FixedWalls = true;
		}
	}

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

		if ( trFront.Hit && trFront.Body.GameObject is GameObject goFront && goFront.GetComponent<RoomChunkComponent>( false ) is null )
		{
			Front.Destroy();
		}

		if ( trBack.Hit && trBack.Body.GameObject is GameObject goBack && goBack.GetComponent<RoomChunkComponent>( false ) is null )
		{
			Back.Destroy();
		}

		if ( trLeft.Hit && trLeft.Body.GameObject is GameObject goLeft && goLeft.GetComponent<RoomChunkComponent>( false ) is null )
		{
			Left.Destroy();
		}

		if ( trRight.Hit && trRight.Body.GameObject is GameObject goRight && goRight.GetComponent<RoomChunkComponent>( false ) is null )
		{
			Right.Destroy();
		}

		foreach ( var item in GetComponents<Collider>( false, true ) )
		{
			item.OnPhysicsChanged();
		}

	}
}
