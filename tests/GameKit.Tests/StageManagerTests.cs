using GameKit.App;
using GameKit.DependencyInjection;
using GameKit.Pencuil;

namespace GameKit.Tests;

public class StageManagerTests
{
    [Test]
    public void Load_WithNullConfigure_Throws()
    {
        ServiceProvider root = BuildRootProvider(out _);
        StageManager stageManager = new(root);

        Assert.Throws<ArgumentNullException>(() => stageManager.Load(null!));
    }

    [Test]
    public void Load_DoesNotApplyImmediately()
    {
        ServiceProvider root = BuildRootProvider(out ServiceRegistry<IPencuilView> viewRegistry);
        StageManager stageManager = new(root);

        stageManager.Load(services =>
        {
            services.AddSingleton<IPencuilView>(new TestView("stage"));
        });

        Assert.That(ViewNames(viewRegistry), Is.Empty);
    }

    [Test]
    public void Load_AppliesOnPendingTransition()
    {
        ServiceProvider root = BuildRootProvider(out ServiceRegistry<IPencuilView> viewRegistry);
        StageManager stageManager = new(root);

        stageManager.Load(services =>
        {
            services.AddSingleton<IPencuilView>(new TestView("stage"));
        });
        stageManager.ApplyPendingTransition();

        Assert.That(ViewNames(viewRegistry), Is.EqualTo(new[] { "stage" }));
    }

    [Test]
    public void Load_MultipleBeforePendingTransition_LastWins()
    {
        ServiceProvider root = BuildRootProvider(out ServiceRegistry<IPencuilView> viewRegistry);
        StageManager stageManager = new(root);

        stageManager.Load(services =>
        {
            services.AddSingleton<IPencuilView>(new TestView("first"));
        });
        stageManager.Load(services =>
        {
            services.AddSingleton<IPencuilView>(new TestView("second"));
        });
        stageManager.ApplyPendingTransition();

        Assert.That(ViewNames(viewRegistry), Is.EqualTo(new[] { "second" }));
    }

    [Test]
    public void Load_DisposesPreviousStageOnPendingTransition()
    {
        ServiceProvider root = BuildRootProvider(out ServiceRegistry<IPencuilView> viewRegistry);
        StageManager stageManager = new(root);

        stageManager.Load(services =>
        {
            services.AddSingleton<IPencuilView>(new TestView("first"));
        });
        stageManager.ApplyPendingTransition();

        stageManager.Load(services =>
        {
            services.AddSingleton<IPencuilView>(new TestView("second"));
        });
        stageManager.ApplyPendingTransition();

        Assert.That(ViewNames(viewRegistry), Is.EqualTo(new[] { "second" }));
    }

    [Test]
    public void ApplyPendingTransition_WithNoPending_DoesNothing()
    {
        ServiceProvider root = BuildRootProvider(out ServiceRegistry<IPencuilView> viewRegistry);
        StageManager stageManager = new(root);

        stageManager.Load(services =>
        {
            services.AddSingleton<IPencuilView>(new TestView("stage"));
        });
        stageManager.ApplyPendingTransition();

        stageManager.ApplyPendingTransition();

        Assert.That(ViewNames(viewRegistry), Is.EqualTo(new[] { "stage" }));
    }

    [Test]
    public void Dispose_DisposesActiveStage()
    {
        ServiceProvider root = BuildRootProvider(out ServiceRegistry<IPencuilView> viewRegistry);
        StageManager stageManager = new(root);

        stageManager.Load(services =>
        {
            services.AddSingleton<IPencuilView>(new TestView("stage"));
        });
        stageManager.ApplyPendingTransition();

        stageManager.Dispose();

        Assert.That(ViewNames(viewRegistry), Is.Empty);
    }

    [Test]
    public void Dispose_ClearsPendingLoad()
    {
        ServiceProvider root = BuildRootProvider(out ServiceRegistry<IPencuilView> viewRegistry);
        StageManager stageManager = new(root);

        stageManager.Load(services =>
        {
            services.AddSingleton<IPencuilView>(new TestView("stage"));
        });

        stageManager.Dispose();
        stageManager.ApplyPendingTransition();

        Assert.That(ViewNames(viewRegistry), Is.Empty);
    }

    [Test]
    public void Load_RegistersStageServicesViaParentCallbacksOnPendingTransition()
    {
        ServiceProvider root = BuildRootProvider(out ServiceRegistry<IPencuilView> viewRegistry);
        StageManager stageManager = new(root);

        stageManager.Load(services =>
        {
            services.AddSingleton<IPencuilView>(new TestView("stage"));
        });
        stageManager.ApplyPendingTransition();

        Assert.That(ViewNames(viewRegistry), Is.EqualTo(new[] { "stage" }));
    }

    [Test]
    public void Load_StageServicesCanResolveRootServices()
    {
        ServiceCollection rootCollection = new();
        rootCollection.AddSingleton(new TestConfig("test"));
        ServiceProvider root = rootCollection.BuildServiceProvider();
        StageManager stageManager = new(root);

        TestConfig? resolved = null;
        stageManager.Load(services =>
        {
            services.AddSingleton<IPencuilView>(sp =>
            {
                resolved = sp.GetRequiredService<TestConfig>();
                return new TestView("stage");
            });
        });
        stageManager.ApplyPendingTransition();

        Assert.That(resolved, Is.Not.Null);
        Assert.That(resolved!.Title, Is.EqualTo("test"));
    }

    [Test]
    public void Load_DisposesPreviousStageOwnedDisposables()
    {
        ServiceProvider root = BuildRootProvider(out _);
        StageManager stageManager = new(root);

        DisposableService disposable = new();
        stageManager.Load(services =>
        {
            services.AddSingleton(disposable);
        });
        stageManager.ApplyPendingTransition();

        stageManager.Load(services =>
        {
            services.AddSingleton<IPencuilView>(new TestView("next"));
        });
        stageManager.ApplyPendingTransition();

        Assert.That(disposable.IsDisposed, Is.True);
    }

    private static ServiceProvider BuildRootProvider(
        out ServiceRegistry<IPencuilView> viewRegistry)
    {
        ServiceCollection rootCollection = new();
        rootCollection.AddRegistry<IPencuilView>();
        ServiceProvider provider = rootCollection.BuildServiceProvider();
        viewRegistry = provider.GetRequiredService<ServiceRegistry<IPencuilView>>();
        return provider;
    }

    private static string[] ViewNames(ServiceRegistry<IPencuilView> viewRegistry)
    {
        List<string> names = new();
        foreach (IPencuilView view in viewRegistry)
        {
            names.Add(((TestView)view).Name);
        }
        return names.ToArray();
    }

    private sealed class TestView : IPencuilView
    {
        public string Name { get; }
        public TestView(string name)
        {
            Name = name;
        }

        public bool ConsumeDirty() => false;

        public void Build(Pencil pencil) { }
    }

    private sealed class DisposableService : IDisposable
    {
        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            IsDisposed = true;
        }
    }

    private sealed record TestConfig(string Title);
}
