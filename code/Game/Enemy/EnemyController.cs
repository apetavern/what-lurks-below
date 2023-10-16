using Sandbox;
using System.Linq;

namespace BrickJam.Game;

public class EnemyController : BaseComponent
{
    [Property] public float Speed { get; set; } = 1f;
    [Property] public float Friction { get; set; } = 0.5f;
    [Property] public float Health { get; set; } = 1f;
    [Property] public float AggroRange { get; set; } = 1f;

    public bool IsAggro = false;

    GameObject Player;
    CharacterController _characterController;

    public override void OnEnabled()
    {
        base.OnStart();

        Player = Player = Scene.GetAllObjects( true ).FirstOrDefault( x => x.Name == "player" );

        _characterController = GetComponent<CharacterController>( false );

    }

    public override void Update()
    {
        base.Update();

        Vector3 myPosition = Transform.Position + GameObject.GetBounds().Center;
        Vector3 playerPosition = Player.Transform.Position + Player.GetBounds().Center;
        IsAggro = playerPosition.Distance( myPosition ) <= AggroRange;

        if ( IsAggro )
        {
            Vector3 direction = (playerPosition - myPosition).Normal;
            Vector3 accel = direction * Speed;
            _characterController.Accelerate( accel );
        }
    }

    public override void DrawGizmos()
    {
        base.DrawGizmos();

        if ( !Gizmo.IsSelected ) return;

        // Draw red sphere for aggro range
        Sphere sphere = new()
        {
            Center = GameObject.GetBounds().Center,
            Radius = AggroRange,
        };
        Gizmo.Draw.Color = Color.Red;
        Gizmo.Draw.LineSphere( sphere, 8 );
    }
}