using System.Runtime.CompilerServices;
using GameKit.DependencyInjection;
using GameKit.Gpu;
using GameKit.RenderOrchestration;

namespace GameKit.Tests;

public class WindowRenderCoordinatorTests
{
    private const string SecondaryWindowName = "inventory";

    [Test]
    public void Constructor_ClaimsWindowName()
    {
        FakeWindowRegistry windows = new();
        using WindowRenderCoordinator<TestRenderContext> coordinator =
            CreateCoordinator(windows, SecondaryWindowName);

        Assert.That(windows.ClaimedWindowNames, Contains.Item(SecondaryWindowName));
    }

    [Test]
    public void Constructor_WhenWindowNameIsAlreadyClaimed_Throws()
    {
        FakeWindowRegistry windows = new();
        using WindowRenderCoordinator<TestRenderContext> coordinator =
            CreateCoordinator(windows, SecondaryWindowName);

        Assert.Throws<InvalidOperationException>(() =>
            CreateCoordinator(windows, SecondaryWindowName));
    }

    [Test]
    public void Constructor_WithDifferentContextTypeForClaimedWindow_Throws()
    {
        FakeWindowRegistry windows = new();
        using WindowRenderCoordinator<TestRenderContext> coordinator =
            CreateCoordinator(windows, SecondaryWindowName);

        Assert.Throws<InvalidOperationException>(() =>
            CreateDerivedCoordinator(windows, SecondaryWindowName));
    }

    [Test]
    public void Execute_WithoutOpenWindow_DoesNotCreateContext()
    {
        FakeWindowRegistry windows = new();
        bool contextCreated = false;
        using WindowRenderCoordinator<TestRenderContext> coordinator = CreateCoordinator(
            windows,
            SecondaryWindowName,
            () => contextCreated = true);

        coordinator.Execute();

        Assert.That(contextCreated, Is.False);
    }

    [Test]
    public void Dispose_ReleasesWindowNameAndDestroysOpenSecondaryWindow()
    {
        FakeWindowRegistry windows = new();
        WindowRenderCoordinator<TestRenderContext> coordinator =
            CreateCoordinator(windows, SecondaryWindowName);
        windows.OpenWindow(SecondaryWindowName);

        coordinator.Dispose();

        Assert.Multiple(() =>
        {
            Assert.That(windows.ClaimedWindowNames, Does.Not.Contain(SecondaryWindowName));
            Assert.That(windows.DestroyedWindowNames, Is.EqualTo(new[] { SecondaryWindowName }));
        });
    }

    [Test]
    public void Dispose_LeavesPrimaryWindowOpen()
    {
        FakeWindowRegistry windows = new();
        WindowRenderCoordinator<TestRenderContext> coordinator =
            CreateCoordinator(windows, WindowManager.PrimaryWindowName);

        coordinator.Dispose();

        Assert.Multiple(() =>
        {
            Assert.That(windows.ClaimedWindowNames, Does.Not.Contain(WindowManager.PrimaryWindowName));
            Assert.That(windows.DestroyedWindowNames, Is.Empty);
            Assert.That(windows.HasOpenWindow(WindowManager.PrimaryWindowName), Is.True);
        });
    }

    [Test]
    public void Dispose_AllowsWindowNameToBeClaimedAgain()
    {
        FakeWindowRegistry windows = new();
        WindowRenderCoordinator<TestRenderContext> first =
            CreateCoordinator(windows, SecondaryWindowName);
        first.Dispose();

        using WindowRenderCoordinator<TestRenderContext> second =
            CreateCoordinator(windows, SecondaryWindowName);

        Assert.That(windows.ClaimedWindowNames, Contains.Item(SecondaryWindowName));
    }

    private static WindowRenderCoordinator<TestRenderContext> CreateCoordinator(
        FakeWindowRegistry windows,
        string windowName,
        Action? onContextCreated = null)
    {
        ServiceCollection services = new();
        services.AddRegistry<IRenderer<TestRenderContext>>();
        ServiceProvider provider = services.BuildServiceProvider();
        return new WindowRenderCoordinator<TestRenderContext>(
            windows,
            windowName,
            null!,
            new GpuMemorySystem(null!),
            provider.GetRequiredService<ServiceRegistry<IRenderer<TestRenderContext>>>(),
            (_, _, _) =>
            {
                onContextCreated?.Invoke();
                return new TestRenderContext();
            });
    }

    private static WindowRenderCoordinator<DerivedTestRenderContext> CreateDerivedCoordinator(
        FakeWindowRegistry windows,
        string windowName)
    {
        ServiceCollection services = new();
        services.AddRegistry<IRenderer<DerivedTestRenderContext>>();
        ServiceProvider provider = services.BuildServiceProvider();
        return new WindowRenderCoordinator<DerivedTestRenderContext>(
            windows,
            windowName,
            null!,
            new GpuMemorySystem(null!),
            provider.GetRequiredService<ServiceRegistry<IRenderer<DerivedTestRenderContext>>>(),
            static (_, _, _) => new DerivedTestRenderContext());
    }

    private sealed class FakeWindowRegistry : IWindowRegistry
    {
        private readonly Dictionary<string, Window> _windows = new(StringComparer.Ordinal);

        public FakeWindowRegistry()
        {
            OpenWindow(WindowManager.PrimaryWindowName);
        }

        public HashSet<string> ClaimedWindowNames { get; } = new(StringComparer.Ordinal);
        public List<string> DestroyedWindowNames { get; } = new();

        public void ClaimWindow(string name)
        {
            if (!ClaimedWindowNames.Add(name))
            {
                throw new InvalidOperationException();
            }
        }

        public void ReleaseWindow(string name)
        {
            if (!ClaimedWindowNames.Remove(name) || name == WindowManager.PrimaryWindowName)
            {
                return;
            }

            if (_windows.Remove(name))
            {
                DestroyedWindowNames.Add(name);
            }
        }

        public bool TryGetWindow(string name, out Window window)
        {
            return _windows.TryGetValue(name, out window!);
        }

        public void OpenWindow(string name)
        {
            Window window = (Window)RuntimeHelpers.GetUninitializedObject(typeof(Window));
            _windows.Add(name, window);
        }

        public bool HasOpenWindow(string name)
        {
            return _windows.ContainsKey(name);
        }
    }

    private class TestRenderContext : IRenderContext
    {
        public CommandBuffer CommandBuffer => null!;
        public Texture ColorTarget => null!;

        public void Dispose()
        {
        }
    }

    private sealed class DerivedTestRenderContext : TestRenderContext
    {
    }
}
