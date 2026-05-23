using GameKit.DependencyInjection;

namespace GameKit.App;

internal sealed class StageManager : IStageManager, IDisposable
{
    private readonly ServiceProvider _rootProvider;
    private ServiceProvider? _stageProvider;
    private Action<ServiceCollection>? _pendingLoad;
    private bool _pendingUnload;

    public StageManager(ServiceProvider rootProvider)
    {
        _rootProvider = rootProvider;
    }

    public void Load(Action<ServiceCollection> configure)
    {
        _pendingLoad = configure;
        _pendingUnload = false;
    }

    public void Unload()
    {
        _pendingLoad = null;
        _pendingUnload = true;
    }

    internal void ApplyPendingTransition()
    {
        if (_pendingLoad != null)
        {
            Action<ServiceCollection> configure = _pendingLoad;
            ClearPendingTransition();
            ReplaceStage(configure);
        }
        else if (_pendingUnload)
        {
            ClearPendingTransition();
            UnloadActiveStage();
        }
    }

    public void Dispose()
    {
        ClearPendingTransition();
        UnloadActiveStage();
    }

    private void ReplaceStage(Action<ServiceCollection> configure)
    {
        UnloadActiveStage();
        ServiceCollection collection = new();
        configure(collection);
        _stageProvider = collection.BuildServiceProvider(_rootProvider);
    }

    private void UnloadActiveStage()
    {
        if (_stageProvider != null)
        {
            _stageProvider.Dispose();
            _stageProvider = null;
        }
    }

    private void ClearPendingTransition()
    {
        _pendingLoad = null;
        _pendingUnload = false;
    }
}
