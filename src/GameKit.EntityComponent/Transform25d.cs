using System.Numerics;

namespace GameKit.EntityComponent;

public readonly record struct PositionChangedArgs(Vector2 Value);
public readonly record struct DirectionChangedArgs(Vector2 Value);

public class Transform25d: GameComponent
{
    public Transform25d(Vector2 position, Vector2 direction)
    {
        Position = position;
        Direction = direction;
    }

    public Transform25d(Vector2 position)
    {
        Position = position;
        Direction = new Vector2(1, 0);
    }

    public Transform25d()
    {
        Position = Vector2.Zero;
        Direction = new Vector2(1, 0);
    }

    public Vector2 Position { get; private set; }
    public Vector2 Direction { get; private set; }

    public void UpdateDirection(Vector2 direction)
    {
        direction = Vector2.Normalize(direction);
        if (direction == Direction) return;

        Direction = direction;
        PublishEvent(new DirectionChangedArgs(direction));
    }

    public void UpdatePosition(Vector2 position)
    {
        if (position == Position) return;

        Position = position;
        PublishEvent(new PositionChangedArgs(position));
    }
}
