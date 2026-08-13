using System.Runtime.CompilerServices;
using GameKit.App;
using GameKit.DependencyInjection;
using GameKit.Gpu;
using GameKit.RenderOrchestration;

namespace GameKit.Tests;

public sealed class MultiWindowContainerTests
{
    [Test]
    public void WindowOptions_StopGameOnCloseByDefault()
    {
        WindowOptions options = new();

        Assert.That(options.StopGameOnClose, Is.True);
    }

    [Test]
    public void RootWindow_WithoutStopGameOnClose_ThrowsDuringRegistration()
    {
        GameKitAppBuilder builder = new();
        WindowOptions options = new(StopGameOnClose: false);

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            builder.AddWindow(options))!;

        Assert.That(exception.Message, Does.Contain(nameof(WindowOptions.StopGameOnClose)));
    }

    [Test]
    public void WindowlessRoot_WithDefaultRenderCoordinator_ThrowsBeforeSdlInitialization()
    {
        GameKitAppBuilder builder = new GameKitAppBuilder()
            .UseDefaultRenderCoordinator<TestRenderContext>();

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => builder.Build())!;

        Assert.That(exception.Message, Does.Contain(nameof(Window)));
    }

    [Test]
    public void AddWindow_WithSecondLocalWindow_ThrowsDuringRegistration()
    {
        ServiceCollection services = new();
        services.AddWindow(new WindowOptions());

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            services.AddWindow(new WindowOptions()))!;

        Assert.That(exception.Message, Does.Contain("already registered"));
    }

    [Test]
    public void AddWindow_WithInheritedWindow_ThrowsDuringRegistration()
    {
        ServiceCollection rootServices = new();
        rootServices.AddSingleton((Window)RuntimeHelpers.GetUninitializedObject(typeof(Window)));
        using ServiceProvider rootProvider = rootServices.BuildServiceProvider();

        ServiceCollection childServices = rootProvider.CreateServiceCollection();

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            childServices.AddWindow(new WindowOptions()))!;

        Assert.That(exception.Message, Does.Contain("already registered"));
    }

    [Test]
    public void BuildServiceProvider_WithApp_UsesAndIsOwnedByAppProvider()
    {
        ServiceCollection rootServices = new();
        RootService rootService = new();
        rootServices.AddSingleton(rootService);
        ServiceProvider rootProvider = rootServices.BuildServiceProvider();
        FakeApp app = new(rootProvider);

        ServiceCollection childServices = app.CreateServiceCollection();
        DisposableChildService childService = new();
        childServices.AddSingleton(childService);
        ServiceProvider childProvider = childServices.BuildServiceProvider();

        Assert.That(childProvider.GetRequiredService<RootService>(), Is.SameAs(rootService));

        app.Dispose();

        Assert.That(childService.IsDisposed, Is.True);
    }

    private sealed class RootService
    {
    }

    private sealed class DisposableChildService : IDisposable
    {
        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            IsDisposed = true;
        }
    }

    private sealed class FakeApp : IGameKitApp
    {
        public FakeApp(ServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        public ServiceProvider ServiceProvider { get; }

        public ServiceCollection CreateServiceCollection()
        {
            return ServiceProvider.CreateServiceCollection();
        }

        public T GetRequiredService<T>() where T : class
        {
            return ServiceProvider.GetRequiredService<T>();
        }

        public int Run()
        {
            throw new NotSupportedException();
        }

        public void Dispose()
        {
            ServiceProvider.Dispose();
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
