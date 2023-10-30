using BrickJam.Components;

namespace BrickJam.Player;

public class PlayerFlagsComponent : SingletonComponent<PlayerFlagsComponent>
{
	public bool KilledFirstEnemy { get; set; } = false;
	public bool InBossSequence { get; set; } = false;
	public bool HasShotgun { get; set; } = false;
	public bool HasBossKey { get; set; } = false;
}
