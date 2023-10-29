using Coroutines;
using Coroutines.Stallers;
using Sandbox;

namespace BrickJam.Components;

public sealed class GameStartComponent : BaseComponent
{
	[Property] public GameObject bezier { get; set; }

	[Property] public GameObject blackFade { get; set; }

	public bool Started;

	public override void Update()
	{
		if ( !Started && Input.Pressed( "Jump" ) )
		{
			Coroutines.Coroutine.Start( StartGame );
			Started = true;
		}
	}

	public CoroutineMethod StartGame()
	{
		var bezierAnimation = bezier.GetComponent<BezierAnimationComponent>( false );
		var animateCoroutine = Coroutine.Start( bezierAnimation.AnimateObject, GameObject, 2f, false );
		yield return new WaitForCoroutine( animateCoroutine, ExecutionStrategy.Frame );

		var model = blackFade.GetComponent<ModelComponent>();
		Stats.Reset();

		while ( model.Tint.a < 0.99f )
		{
			var col = model.Tint;
			col.a += Time.Delta * 2f;
			model.Tint = col;
			yield return new WaitForNextFrame();
		}
		model.Tint = Color.White.WithAlpha( 1f );

		yield return new WaitForNextFrame();
		Scene.LoadFromFile( "scenes/devmap.scene" );
	}
}
