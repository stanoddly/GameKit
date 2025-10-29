namespace GameKit.App;

public interface IGameKitApp : IDisposable
{
    IServiceProvider ServiceProvider { get; }
    
    TService GetMandatoryService<TService>();
    
    int Run();
}