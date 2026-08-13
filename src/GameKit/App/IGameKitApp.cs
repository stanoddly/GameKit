using System.Diagnostics.CodeAnalysis;
using GameKit.DependencyInjection;

namespace GameKit.App;

public interface IGameKitApp : IDisposable
{
    ServiceProvider ServiceProvider { get; }
    ServiceCollection CreateServiceCollection();
    T GetRequiredService<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] T>() where T : class;
    int Run();
}
