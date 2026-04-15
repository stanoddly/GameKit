using GameKit.DependencyInjection;

namespace GameKit.App;

public interface IGameKitApp : IDisposable
{
    ServiceProvider ServiceProvider { get; }
    T GetService<T>() where T : class;
    int Run();
}
