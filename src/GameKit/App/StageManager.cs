using GameKit.DependencyInjection;

namespace GameKit.App;

internal sealed class StageManager : IStageManager, IDisposable
{
    private readonly ServiceProvider _rootProvider;
    private ServiceProvider? _stageProvider;
    private Action<ServiceCollection>? _pendingLoad;

    public StageManager(ServiceProvider rootProvider)
    {
        _rootProvider = rootProvider;
    }

    public void Load(Action<ServiceCollection> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        _pendingLoad = configure;
    }

    internal void ApplyPendingTransition()
    {
        if (_pendingLoad != null)
        {
            Action<ServiceCollection> configure = _pendingLoad;
            _pendingLoad = null;
            ReplaceStage(configure);
        }
    }

    public void Dispose()
    {
        _pendingLoad = null;
        UnloadActiveStage();
    }

    private void ReplaceStage(Action<ServiceCollection> configure)
    {
        UnloadActiveStage();
        ServiceCollection collection = _rootProvider.CreateServiceCollection();
        configure(collection);
        _stageProvider = collection.BuildServiceProvider();
    }

    private void UnloadActiveStage()
    {
        if (_stageProvider != null)
        {
            _stageProvider.Dispose();
            _stageProvider = null;
        }
    }

}
