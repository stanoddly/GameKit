using GameKit.DependencyInjection;

namespace GameKit.App;

internal sealed class UpdateLoop
{
    private readonly ServiceRegistry<IUpdatable> _updatables;

    public UpdateLoop(ServiceRegistry<IUpdatable> updatables)
    {
        _updatables = updatables;
    }

    public void Update()
    {
        IReadOnlyList<IUpdatable> updatables = _updatables.Services;
        for (int i = 0; i < updatables.Count; i++)
        {
            updatables[i].Update();
        }
    }
}
