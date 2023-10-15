using Sandbox;

public sealed class RoomChunkComponent : BaseComponent
{
	[Property] GameObject CameraTrigger { get; set; }

	public void SetupCollision()
	{
		CameraTrigger.GetComponent<CameraTriggerComponent>( false ).RecalcBounds();
		GetComponent<ModelCollider>( false ).Enabled = true;
		Log.Info( "Collision fixed!" );
	}
}
