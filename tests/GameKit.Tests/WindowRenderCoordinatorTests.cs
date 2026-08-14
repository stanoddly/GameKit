using System.Runtime.CompilerServices;
using GameKit.DependencyInjection;
using GameKit.Gpu;
using GameKit.RenderOrchestration;

namespace GameKit.Tests;

public class WindowRenderCoordinatorTests
{
    [Test]
    public void Execute_WithoutAttachment_DoesNotCreateContext()
    {
        FakeWindowRegistry windows = new(new WindowId(1));
        bool contextCreated = false;
        using WindowRenderCoordinator<TestRenderContext> coordinator = CreateCoordinator(
            windows,
            false,
            () => contextCreated = true);

        coordinator.Execute();

        Assert.That(contextCreated, Is.False);
    }

    [Test]
    public void Attach_WithUnknownWindow_Throws()
    {
        FakeWindowRegistry windows = new(new WindowId(1));
        using WindowRenderCoordinator<TestRenderContext> coordinator = CreateCoordinator(windows);

        Assert.Throws<ArgumentException>(() => coordinator.Attach(new WindowId(2)));
    }

    [Test]
    public void Attach_WhenAlreadyAttached_Throws()
    {
        WindowId windowId = new(2);
        FakeWindowRegistry windows = new(new WindowId(1));
        windows.Add(windowId);
        using WindowRenderCoordinator<TestRenderContext> coordinator = CreateCoordinator(windows);
        using IWindowRenderBinding binding = coordinator.Attach(windowId);

        Assert.Throws<InvalidOperationException>(() => coordinator.Attach(windowId));
    }

    [Test]
    public void BindingDispose_DestroysAttachedWindow()
    {
        WindowId windowId = new(2);
        FakeWindowRegistry windows = new(new WindowId(1));
        windows.Add(windowId);
        using WindowRenderCoordinator<TestRenderContext> coordinator = CreateCoordinator(windows);
        IWindowRenderBinding binding = coordinator.Attach(windowId);

        binding.Dispose();

        Assert.Multiple(() =>
        {
            Assert.That(binding.IsActive, Is.False);
            Assert.That(windows.DestroyedWindowIds, Is.EqualTo(new[] { windowId }));
        });
    }

    [Test]
    public void CoordinatorDispose_DestroysAttachedWindow()
    {
        WindowId windowId = new(2);
        FakeWindowRegistry windows = new(new WindowId(1));
        windows.Add(windowId);
        WindowRenderCoordinator<TestRenderContext> coordinator = CreateCoordinator(windows);
        IWindowRenderBinding binding = coordinator.Attach(windowId);

        coordinator.Dispose();

        Assert.Multiple(() =>
        {
            Assert.That(binding.IsActive, Is.False);
            Assert.That(windows.DestroyedWindowIds, Is.EqualTo(new[] { windowId }));
        });
    }

    [Test]
    public void WindowDestroyed_ImmediatelyInvalidatesBinding()
    {
        WindowId windowId = new(2);
        FakeWindowRegistry windows = new(new WindowId(1));
        windows.Add(windowId);
        using WindowRenderCoordinator<TestRenderContext> coordinator = CreateCoordinator(windows);
        IWindowRenderBinding binding = coordinator.Attach(windowId);

        windows.DestroyExternally(windowId);

        Assert.That(binding.IsActive, Is.False);
    }

    [Test]
    public void PrimaryAttachment_IsNotOwnedByCoordinator()
    {
        WindowId primaryWindowId = new(1);
        FakeWindowRegistry windows = new(primaryWindowId);
        WindowRenderCoordinator<TestRenderContext> coordinator = CreateCoordinator(windows, true);

        coordinator.Dispose();

        Assert.That(windows.DestroyedWindowIds, Is.Empty);
    }

    private static WindowRenderCoordinator<TestRenderContext> CreateCoordinator(
        FakeWindowRegistry windows,
        bool attachPrimaryWindow = false,
        Action? onContextCreated = null)
    {
        ServiceCollection services = new();
        services.AddRegistry<IRenderer<TestRenderContext>>();
        ServiceProvider provider = services.BuildServiceProvider();
        return new WindowRenderCoordinator<TestRenderContext>(
            windows,
            null!,
            new GpuMemorySystem(null!),
            provider.GetRequiredService<ServiceRegistry<IRenderer<TestRenderContext>>>(),
            (_, _, _) =>
            {
                onContextCreated?.Invoke();
                return new TestRenderContext();
            },
            attachPrimaryWindow);
    }

    private sealed class FakeWindowRegistry : IWindowRegistry
    {
        private readonly Dictionary<WindowId, Window> _windows = new();

        public FakeWindowRegistry(WindowId primaryWindowId)
        {
            PrimaryWindowId = primaryWindowId;
            Add(primaryWindowId);
        }

        public WindowId PrimaryWindowId { get; }
        public event Action<WindowId>? WindowDestroyed;
        public List<WindowId> DestroyedWindowIds { get; } = new();

        public void Add(WindowId windowId)
        {
            Window window = (Window)RuntimeHelpers.GetUninitializedObject(typeof(Window));
            _windows.Add(windowId, window);
        }

        public bool TryGetWindow(WindowId windowId, out Window window)
        {
            return _windows.TryGetValue(windowId, out window!);
        }

        public void DestroyWindow(WindowId windowId)
        {
            if (!_windows.Remove(windowId))
            {
                return;
            }

            DestroyedWindowIds.Add(windowId);
            WindowDestroyed?.Invoke(windowId);
        }

        public void DestroyExternally(WindowId windowId)
        {
            _windows.Remove(windowId);
            WindowDestroyed?.Invoke(windowId);
        }
    }

    private sealed class TestRenderContext : IRenderContext
    {
        public CommandBuffer CommandBuffer => null!;
        public Texture ColorTarget => null!;

        public void Dispose()
        {
        }
    }
}
