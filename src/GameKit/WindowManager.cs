using GameKit.DependencyInjection;

namespace GameKit;

public sealed class WindowManager
{
    private readonly ServiceProvider _applicationProvider;
    private (Window Window, ServiceProvider Provider)[] _windowOwners = [];
    private readonly List<Window> _windows = new();
    private readonly List<ServiceProvider> _pendingDisposals = new();

    internal WindowManager(ServiceProvider applicationProvider)
    {
        _applicationProvider = applicationProvider;
    }

    public IReadOnlyList<Window> Windows => _windows;

    public void DestroyWindow(Window window)
    {
        ArgumentNullException.ThrowIfNull(window);

        ServiceProvider? provider = null;
        for (int i = 0; i < _windowOwners.Length; i++)
        {
            if (ReferenceEquals(_windowOwners[i].Window, window))
            {
                provider = _windowOwners[i].Provider;
                break;
            }
        }

        if (provider == null)
        {
            throw new InvalidOperationException("The window is not attached to the application.");
        }

        if (ReferenceEquals(provider, _applicationProvider))
        {
            throw new InvalidOperationException(
                "A window owned by the application service container cannot be closed independently. Use StopGame close behavior.");
        }

        QueueDisposal(provider);
    }

    internal void Attach(Window window, ServiceProvider provider)
    {
        for (int i = 0; i < _windowOwners.Length; i++)
        {
            if (ReferenceEquals(_windowOwners[i].Window, window))
            {
                throw new InvalidOperationException("The window is already attached.");
            }
        }

        _windowOwners = [.. _windowOwners, (window, provider)];
        _windows.Add(window);
    }

    internal void Detach(Window window)
    {
        int index = -1;
        for (int i = 0; i < _windowOwners.Length; i++)
        {
            if (ReferenceEquals(_windowOwners[i].Window, window))
            {
                index = i;
                break;
            }
        }

        if (index < 0)
        {
            return;
        }

        int itemsToMove = _windowOwners.Length - index - 1;
        if (itemsToMove > 0)
        {
            Array.Copy(_windowOwners, index + 1, _windowOwners, index, itemsToMove);
        }

        Array.Resize(ref _windowOwners, _windowOwners.Length - 1);
        _windows.Remove(window);
    }

    internal void ApplyPendingDisposals()
    {
        if (_pendingDisposals.Count > 0)
        {
            ServiceProvider[] providers = _pendingDisposals.ToArray();
            _pendingDisposals.Clear();

            for (int i = 0; i < providers.Length; i++)
            {
                providers[i].Dispose();
            }
        }
    }

    internal void QueueDisposal(ServiceProvider provider)
    {
        for (int i = 0; i < _pendingDisposals.Count; i++)
        {
            if (ReferenceEquals(_pendingDisposals[i], provider))
            {
                return;
            }
        }

        _pendingDisposals.Add(provider);
    }
}
