namespace GameKit.Componentize;

public abstract class Behavior<TSelf>: GameComponent
    where TSelf: Behavior<TSelf>
{
    protected TSelf ReplaceState(TSelf gameState)
    {
        AttachSibling(gameState);
        return gameState;
    }
}
