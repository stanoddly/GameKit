using System.Diagnostics.CodeAnalysis;
using GameKit.DependencyInjection;
using GameKit.Gpu;

namespace GameKit.RenderOrchestration;

internal sealed class WindowRenderCoordinator<TRenderContext> :
    RenderCoordinator<TRenderContext>,
    IWindowRendering<TRenderContext>,
    IDisposable
    where TRenderContext : IRenderContext
{
    private readonly IWindowRegistry _windows;
    private readonly GpuDevice _gpuDevice;
    private readonly Func<Window, SwapchainTexture, CommandBuffer, TRenderContext> _contextFactory;
    private WindowRenderBinding? _binding;
    private bool _disposed;

    internal WindowRenderCoordinator(
        IWindowRegistry windows,
        GpuDevice gpuDevice,
        GpuMemorySystem gpuMemorySystem,
        ServiceRegistry<IRenderer<TRenderContext>> renderers,
        Func<Window, SwapchainTexture, CommandBuffer, TRenderContext> contextFactory,
        bool attachPrimaryWindow)
        : base(gpuMemorySystem, renderers)
    {
        _windows = windows;
        _gpuDevice = gpuDevice;
        _contextFactory = contextFactory;
        _windows.WindowDestroyed += OnWindowDestroyed;

        if (attachPrimaryWindow)
        {
            _binding = new WindowRenderBinding(this, windows.PrimaryWindowId, false);
        }
    }

    public IWindowRenderBinding Attach(WindowId windowId)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (_binding?.IsActive == true)
        {
            throw new InvalidOperationException(
                $"{typeof(TRenderContext).Name} rendering is already attached to a window.");
        }

        if (!_windows.TryGetWindow(windowId, out _))
        {
            throw new ArgumentException("The window does not exist.", nameof(windowId));
        }

        bool ownsWindow = windowId != _windows.PrimaryWindowId;
        _binding = new WindowRenderBinding(this, windowId, ownsWindow);
        return _binding;
    }

    protected override bool TryCreateRenderContext(
        [NotNullWhen(true)] out TRenderContext? renderContext)
    {
        WindowRenderBinding? binding = _binding;
        if (binding?.IsActive != true || !_windows.TryGetWindow(binding.WindowId, out Window window))
        {
            binding?.Invalidate();
            _binding = null;
            renderContext = default;
            return false;
        }

        CommandBuffer commandBuffer = _gpuDevice.AcquireCommandBuffer();
        if (!window.TryWaitAndAcquireSwapchainTexture(commandBuffer, out SwapchainTexture swapchainTexture))
        {
            commandBuffer.Dispose();
            renderContext = default;
            return false;
        }

        try
        {
            renderContext = _contextFactory(window, swapchainTexture, commandBuffer);
            return true;
        }
        catch
        {
            commandBuffer.Dispose();
            throw;
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _windows.WindowDestroyed -= OnWindowDestroyed;
        _binding?.Dispose();
        _binding = null;
    }

    private void Detach(WindowRenderBinding binding)
    {
        if (!ReferenceEquals(_binding, binding))
        {
            binding.Invalidate();
            return;
        }

        _binding = null;
        binding.Invalidate();
        if (binding.OwnsWindow)
        {
            _windows.DestroyWindow(binding.WindowId);
        }
    }

    private void OnWindowDestroyed(WindowId windowId)
    {
        if (_binding?.WindowId != windowId)
        {
            return;
        }

        _binding.Invalidate();
        _binding = null;
    }

    private sealed class WindowRenderBinding : IWindowRenderBinding
    {
        private WindowRenderCoordinator<TRenderContext>? _owner;

        internal WindowRenderBinding(
            WindowRenderCoordinator<TRenderContext> owner,
            WindowId windowId,
            bool ownsWindow)
        {
            _owner = owner;
            WindowId = windowId;
            OwnsWindow = ownsWindow;
        }

        public WindowId WindowId { get; }
        public bool IsActive => _owner != null;
        internal bool OwnsWindow { get; }

        public void Dispose()
        {
            _owner?.Detach(this);
        }

        internal void Invalidate()
        {
            _owner = null;
        }
    }
}
