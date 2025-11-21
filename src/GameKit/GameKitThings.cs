namespace GameKit;

public class GameKitInitializationException : Exception
{
    public GameKitInitializationException()
    {
    }

    public GameKitInitializationException(string message)
        : base(message)
    {
    }

    public GameKitInitializationException(string message, Exception inner)
        : base(message, inner)
    {
    }
}