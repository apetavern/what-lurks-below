using Sandbox;
using System.Linq;

namespace BrickJam.Game;

public class EnemyController : BaseComponent
{
    [Property] public float Speed { get; set; } = 1f;
    [Property] public float Friction { get; set; } = 0.5f;
    [Property] public float AggroRange { get; set; } = 1f;

    [Property] public GameObject Body { get; set; }

    public bool IsAggro = false;

    GameObject Player;
    CharacterController _characterController;
    BBox _bounds;

    public override void OnEnabled()
    {
        base.OnStart();

        Player = Scene.GetAllObjects( true ).FirstOrDefault( x => x.Name == "player" );
    }

    public override void Update()
    {
        base.Update();

        Vector3 myPosition = Transform.Position;
        Vector3 playerPosition = Player.Transform.Position;
        IsAggro = playerPosition.Distance( myPosition ) <= AggroRange;

        _characterController ??= GameObject.GetComponent<CharacterController>();

        if ( IsAggro )
        {
            // Move towards player when in range
            Vector3 direction = (playerPosition - myPosition).Normal;
            Vector3 wishVelocity = (direction * Speed).WithZ( 0 );
            _characterController.Accelerate( wishVelocity );
            _characterController.ApplyFriction( Friction );
        }
        else
        {
            // Slow down when out of range
            _characterController.ApplyFriction( Friction / 3f );
        }

        _characterController.Move();

        // Rotate body towards velocity
        if ( _characterController.Velocity.LengthSquared > 0.1f )
        {
            Rotation targetRotation = Rotation.LookAt( _characterController.Velocity.WithZ( 0 ), Vector3.Up );
            Body.Transform.Rotation = Rotation.Lerp( Body.Transform.Rotation, targetRotation, Time.Delta * 10f );
        }
    }

    public override void DrawGizmos()
    {
        base.DrawGizmos();

        if ( !Gizmo.IsSelected ) return;

        // Draw red sphere for aggro range
        Gizmo.Draw.Color = Color.Red;
        Gizmo.Draw.LineSphere( new() { Center = Vector3.Zero, Radius = AggroRange }, 8 );
    }
}