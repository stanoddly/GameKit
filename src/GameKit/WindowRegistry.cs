using GameKit.RenderOrchestration;

namespace GameKit;

internal sealed class WindowRegistry
{
    private Window?[] _windowsByRenderContextTypeId = Array.Empty<Window>();
    private readonly List<(uint Id, Window Window)> _windowsBySdlId = new();

    internal bool TryGetWindow<TRenderContext>(out Window<TRenderContext> window)
        where TRenderContext : IRenderContext
    {
        int typeId = WindowTypeId<TRenderContext>.Id;
        if ((uint)typeId < (uint)_windowsByRenderContextTypeId.Length &&
            _windowsByRenderContextTypeId[typeId] is Window<TRenderContext> registeredWindow)
        {
            window = registeredWindow;
            return true;
        }

        window = null!;
        return false;
    }

    internal bool TryGetWindow(uint sdlWindowId, out Window window)
    {
        foreach ((uint id, Window registeredWindow) in _windowsBySdlId)
        {
            if (id == sdlWindowId)
            {
                window = registeredWindow;
                return true;
            }
        }

        window = null!;
        return false;
    }

    internal void Register(Window window)
    {
        int typeId = window.RenderContextTypeId;
        EnsureWindowCapacity(typeId);

        Window? registeredWindow = _windowsByRenderContextTypeId[typeId];
        if (ReferenceEquals(registeredWindow, window))
        {
            return;
        }

        if (registeredWindow != null)
        {
            throw new InvalidOperationException(
                $"A window for {window.GetType().Name} is already registered.");
        }

        foreach ((uint id, Window _) in _windowsBySdlId)
        {
            if (id == window.Id)
            {
                throw new InvalidOperationException(
                    $"SDL window ID {window.Id} is already registered.");
            }
        }

        _windowsByRenderContextTypeId[typeId] = window;
        _windowsBySdlId.Add((window.Id, window));
    }

    internal void Unregister(Window window)
    {
        int typeId = window.RenderContextTypeId;
        if ((uint)typeId >= (uint)_windowsByRenderContextTypeId.Length ||
            !ReferenceEquals(_windowsByRenderContextTypeId[typeId], window))
        {
            return;
        }

        _windowsByRenderContextTypeId[typeId] = null;
        for (int i = 0; i < _windowsBySdlId.Count; i++)
        {
            if (ReferenceEquals(_windowsBySdlId[i].Window, window))
            {
                _windowsBySdlId.RemoveAt(i);
                return;
            }
        }
    }

    private void EnsureWindowCapacity(int renderContextTypeId)
    {
        if (renderContextTypeId >= _windowsByRenderContextTypeId.Length)
        {
            Array.Resize(ref _windowsByRenderContextTypeId, renderContextTypeId + 1);
        }
    }
}
