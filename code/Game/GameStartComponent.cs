using Sandbox;

namespace BrickJam.Game;

public sealed class GameStartComponent : BaseComponent
{
	[Property] public GameObject bezier { get; set; }

	[Property] public GameObject blackFade { get; set; }

	public bool Started;

	public override void Update()
	{
		if ( !Started && Input.Pressed( "Jump" ) )
		{
			StartGame();
			Started = true;
		}
	}

	public async void StartGame()
	{
		await bezier.GetComponent<BezierAnimationComponent>( false ).AnimateObject( GameObject, 2f );
		var model = blackFade.GetComponent<ModelComponent>();
		Stats.Reset();
		while ( model.Tint.a < 0.99f )
		{
			var col = model.Tint;
			col.a += Time.Delta * 2f;
			model.Tint = col;
			await GameTask.DelaySeconds( Time.Delta );
		}
		model.Tint = Color.White.WithAlpha( 1f );
		await GameTask.DelaySeconds( Time.Delta );
		Scene.LoadFromFile( "scenes/devmap.scene" );
	}
}
