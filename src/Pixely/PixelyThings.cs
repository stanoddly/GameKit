namespace Pixely;

public class PixelyInitializationException : Exception
{
    public PixelyInitializationException()
    {
    }

    public PixelyInitializationException(string message)
        : base(message)
    {
    }

    public PixelyInitializationException(string message, Exception inner)
        : base(message, inner)
    {
    }
}