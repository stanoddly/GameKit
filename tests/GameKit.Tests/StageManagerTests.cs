using GameKit.App;
using GameKit.DependencyInjection;
using GameKit.Pencuil;

namespace GameKit.Tests;

public class StageManagerTests
{
    [Test]
    public void Load_WithNullConfigure_Throws()
    {
        ServiceProvider root = BuildRootProvider(new PencuilViewRegistry(new ViewScope(0)));
        StageManager stageManager = new(root);

        Assert.Throws<ArgumentNullException>(() => stageManager.Load(null!));
    }

    [Test]
    public void Load_DoesNotApplyImmediately()
    {
        PencuilViewRegistry viewRegistry = new(new ViewScope(0));
        ServiceProvider root = BuildRootProvider(viewRegistry);
        StageManager stageManager = new(root);

        stageManager.Load(services =>
        {
            services.AddSingleton<IPencuilView>(new TestView("stage"));
        });

        Assert.That(viewRegistry.Views.Length, Is.EqualTo(0));
    }

    [Test]
    public void Load_AppliesOnPendingTransition()
    {
        PencuilViewRegistry viewRegistry = new(new ViewScope(0));
        ServiceProvider root = BuildRootProvider(viewRegistry);
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
        PencuilViewRegistry viewRegistry = new(new ViewScope(0));
        ServiceProvider root = BuildRootProvider(viewRegistry);
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
        PencuilViewRegistry viewRegistry = new(new ViewScope(0));
        ServiceProvider root = BuildRootProvider(viewRegistry);
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
        PencuilViewRegistry viewRegistry = new(new ViewScope(0));
        ServiceProvider root = BuildRootProvider(viewRegistry);
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
        PencuilViewRegistry viewRegistry = new(new ViewScope(0));
        ServiceProvider root = BuildRootProvider(viewRegistry);
        StageManager stageManager = new(root);

        stageManager.Load(services =>
        {
            services.AddSingleton<IPencuilView>(new TestView("stage"));
        });
        stageManager.ApplyPendingTransition();

        stageManager.Dispose();

        Assert.That(viewRegistry.Views.Length, Is.EqualTo(0));
    }

    [Test]
    public void Dispose_ClearsPendingLoad()
    {
        PencuilViewRegistry viewRegistry = new(new ViewScope(0));
        ServiceProvider root = BuildRootProvider(viewRegistry);
        StageManager stageManager = new(root);

        stageManager.Load(services =>
        {
            services.AddSingleton<IPencuilView>(new TestView("stage"));
        });

        stageManager.Dispose();
        stageManager.ApplyPendingTransition();

        Assert.That(viewRegistry.Views.Length, Is.EqualTo(0));
    }

    [Test]
    public void Load_RegistersStageServicesViaParentCallbacksOnPendingTransition()
    {
        PencuilViewRegistry viewRegistry = new(new ViewScope(0));
        ServiceProvider root = BuildRootProvider(viewRegistry);
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
        ServiceProvider root = BuildRootProvider(new PencuilViewRegistry(new ViewScope(0)));
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

    private static ServiceProvider BuildRootProvider(PencuilViewRegistry viewRegistry)
    {
        ServiceCollection rootCollection = new();
        rootCollection.AddSingleton(viewRegistry);
        rootCollection.OnActivated((instance, _) =>
        {
            if (instance is IPencuilView view)
            {
                viewRegistry.Add(view);
            }
        });
        rootCollection.OnDisposing((instance, _) =>
        {
            if (instance is IPencuilView view)
            {
                viewRegistry.Remove(view);
            }
        });
        return rootCollection.BuildServiceProvider();
    }

    private static string[] ViewNames(PencuilViewRegistry viewRegistry)
    {
        ReadOnlySpan<IPencuilView> views = viewRegistry.Views;
        string[] names = new string[views.Length];
        for (int i = 0; i < views.Length; i++)
        {
            names[i] = ((TestView)views[i]).Name;
        }
        return names;
    }

    private sealed class TestView : IPencuilView
    {
        public string Name { get; }
        public ViewScope ViewScope => new(0);

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
