using System.Diagnostics.CodeAnalysis;
using GameKit.App;
using GameKit.DependencyInjection;
using GameKit.Gpu;
using GameKit.RenderOrchestration;

namespace GameKit.Tests;

public class DefaultRenderManagerTests
{
    [Test]
    public void Execute_WithNoRenderPhases_DoesNotThrow()
    {
        GameKitAppBuilder builder = CreateBuilder(new List<string>());
        ServiceProvider provider = builder.BuildServiceProvider();
        IRenderManager renderManager = provider.GetRequiredService<IRenderManager>();

        Assert.DoesNotThrow(renderManager.Execute);
    }

    [Test]
    public void ChildProviderRenderPhase_IsRenderedAfterChildBuild()
    {
        List<string> calls = new();
        GameKitAppBuilder builder = CreateBuilder(calls);
        ServiceProvider parent = builder.BuildServiceProvider();
        IRenderManager renderManager = parent.GetRequiredService<IRenderManager>();

        ServiceCollection childCollection = new();
        childCollection.AddSingleton<IRenderPhase<TestRenderContext>>(new TestRenderPhase("child", calls));
        using ServiceProvider child = childCollection.BuildServiceProvider(parent);

        renderManager.Execute();

        Assert.That(calls, Is.EqualTo(new[] { "child" }));
    }

    [Test]
    public void ChildProviderRenderPhase_IsRemovedWhenChildProviderIsDisposed()
    {
        List<string> calls = new();
        GameKitAppBuilder builder = CreateBuilder(calls);
        ServiceProvider parent = builder.BuildServiceProvider();
        IRenderManager renderManager = parent.GetRequiredService<IRenderManager>();

        ServiceCollection childCollection = new();
        childCollection.AddSingleton<IRenderPhase<TestRenderContext>>(new TestRenderPhase("child", calls));
        ServiceProvider child = childCollection.BuildServiceProvider(parent);

        child.Dispose();
        renderManager.Execute();

        Assert.That(calls, Is.Empty);
    }

    [Test]
    public void DynamicRenderPhases_AreRenderedInOrder()
    {
        List<string> calls = new();
        GameKitAppBuilder builder = CreateBuilder(calls);
        builder.AddSingleton<IRenderPhase<TestRenderContext>>(new TestRenderPhase("root", calls, 10));
        ServiceProvider parent = builder.BuildServiceProvider();
        IRenderManager renderManager = parent.GetRequiredService<IRenderManager>();

        ServiceCollection childCollection = new();
        childCollection.AddSingleton<IRenderPhase<TestRenderContext>>(new TestRenderPhase("child", calls, 5));
        using ServiceProvider child = childCollection.BuildServiceProvider(parent);

        renderManager.Execute();

        Assert.That(calls, Is.EqualTo(new[] { "child", "root" }));
    }

    [Test]
    public void ChildProviderDisposeDuringRender_DoesNotSkipRemainingRootRenderPhases()
    {
        List<string> calls = new();
        GameKitAppBuilder builder = CreateBuilder(calls);
        builder.AddSingleton<IRenderPhase<TestRenderContext>>(new TestRenderPhase("root", calls, 10));
        ServiceProvider parent = builder.BuildServiceProvider();
        IRenderManager renderManager = parent.GetRequiredService<IRenderManager>();

        ServiceProvider? child = null;
        ServiceCollection childCollection = new();
        childCollection.AddSingleton<IRenderPhase<TestRenderContext>>(new DisposingRenderPhase("child", calls, () => child!, 0));
        child = childCollection.BuildServiceProvider(parent);

        renderManager.Execute();

        Assert.That(calls, Is.EqualTo(new[] { "child", "root" }));
    }

    private static GameKitAppBuilder CreateBuilder(List<string> calls)
    {
        GameKitAppBuilder builder = new();
        builder.UseDefaultRenderManager<TestRenderContext>();
        builder.AddSingleton<IRenderContextProvider<TestRenderContext>>(new TestRenderContextProvider());
        builder.AddSingleton(new GpuMemorySystem(null!));
        builder.AddSingleton(calls);
        return builder;
    }

    private sealed class TestRenderContextProvider : IRenderContextProvider<TestRenderContext>
    {
        public bool TryProvide([NotNullWhen(true)] out TestRenderContext? renderContext)
        {
            renderContext = new TestRenderContext();
            return true;
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

    private class TestRenderPhase : IRenderPhase<TestRenderContext>
    {
        private readonly string _name;
        private readonly List<string> _calls;

        public TestRenderPhase(string name, List<string> calls, int order = 0)
        {
            _name = name;
            _calls = calls;
            Order = order;
        }

        public int Order { get; }

        public virtual void Render(TestRenderContext renderContext)
        {
            _calls.Add(_name);
        }
    }

    private sealed class DisposingRenderPhase : TestRenderPhase
    {
        private readonly Func<ServiceProvider> _provider;

        public DisposingRenderPhase(
            string name,
            List<string> calls,
            Func<ServiceProvider> provider,
            int order)
            : base(name, calls, order)
        {
            _provider = provider;
        }

        public override void Render(TestRenderContext renderContext)
        {
            base.Render(renderContext);
            _provider().Dispose();
        }
    }
}
