using Sandbox;

namespace BrickJam.UI;

public sealed class RootPanelComponent : BaseComponent
{
	private BrickJamHud _rootPanel;

	public override void OnStart()
	{
		_rootPanel = new BrickJamHud();
		_rootPanel.RenderedManually = true;

		Camera.Main.OnRenderOverlay += RenderPanel;
	}

	public override void OnDisabled()
	{
		Camera.Main.OnRenderOverlay -= RenderPanel;
		_rootPanel.Delete();
		_rootPanel = null;
	}

	private void RenderPanel()
	{
		_rootPanel.RenderManual();
	}
}
