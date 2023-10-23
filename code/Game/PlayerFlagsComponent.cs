namespace BrickJam.Game;

public class PlayerFlagsComponent : BaseComponent
{
	public bool KilledFirstEnemy { get; set; } = false;
	public bool InBossSequence { get; set; } = false;
	public bool HasShotgun { get; set; } = false;
}
