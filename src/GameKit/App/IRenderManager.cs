namespace GameKit.App;

public interface IRenderManager
{
    void Execute();
}

public sealed class NullRenderManager: IRenderManager
{
    public void Execute()
    {
    }
}
