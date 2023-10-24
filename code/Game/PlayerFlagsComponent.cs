namespace BrickJam.Components;

public class PlayerFlagsComponent : BaseComponent
{
	public bool KilledFirstEnemy { get; set; } = false;
	public bool InBossSequence { get; set; } = false;
	public bool HasShotgun { get; set; } = false;
	public bool HasBossKey { get; set; } = false;
}
