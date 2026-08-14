using GameKit.App;
using GameKit.DependencyInjection;
using GameKit.Pencuil;

namespace GameKit.Tests;

public class StageManagerTests
{
    [Test]
    public void Load_WithNullConfigure_Throws()
    {
        ServiceProvider root = BuildRootProvider(new ViewRegistry());
        StageManager stageManager = new(root);

        Assert.Throws<ArgumentNullException>(() => stageManager.Load(null!));
    }

    [Test]
    public void Load_DoesNotApplyImmediately()
    {
        ViewRegistry viewRegistry = new();
        ServiceProvider root = BuildRootProvider(viewRegistry);
        StageManager stageManager = new(root);

        stageManager.Load(services =>
        {
            services.AddSingleton<IView>(new TestView("stage"));
        });

        Assert.That(viewRegistry.Views.Length, Is.EqualTo(0));
    }

    [Test]
    public void Load_AppliesOnPendingTransition()
    {
        ViewRegistry viewRegistry = new();
        ServiceProvider root = BuildRootProvider(viewRegistry);
        StageManager stageManager = new(root);

        stageManager.Load(services =>
        {
            services.AddSingleton<IView>(new TestView("stage"));
        });
        stageManager.ApplyPendingTransition();

        Assert.That(ViewNames(viewRegistry), Is.EqualTo(new[] { "stage" }));
    }

    [Test]
    public void Load_MultipleBeforePendingTransition_LastWins()
    {
        ViewRegistry viewRegistry = new();
        ServiceProvider root = BuildRootProvider(viewRegistry);
        StageManager stageManager = new(root);

        stageManager.Load(services =>
        {
            services.AddSingleton<IView>(new TestView("first"));
        });
        stageManager.Load(services =>
        {
            services.AddSingleton<IView>(new TestView("second"));
        });
        stageManager.ApplyPendingTransition();

        Assert.That(ViewNames(viewRegistry), Is.EqualTo(new[] { "second" }));
    }

    [Test]
    public void Load_DisposesPreviousStageOnPendingTransition()
    {
        ViewRegistry viewRegistry = new();
        ServiceProvider root = BuildRootProvider(viewRegistry);
        StageManager stageManager = new(root);

        stageManager.Load(services =>
        {
            services.AddSingleton<IView>(new TestView("first"));
        });
        stageManager.ApplyPendingTransition();

        stageManager.Load(services =>
        {
            services.AddSingleton<IView>(new TestView("second"));
        });
        stageManager.ApplyPendingTransition();

        Assert.That(ViewNames(viewRegistry), Is.EqualTo(new[] { "second" }));
    }

    [Test]
    public void ApplyPendingTransition_WithNoPending_DoesNothing()
    {
        ViewRegistry viewRegistry = new();
        ServiceProvider root = BuildRootProvider(viewRegistry);
        StageManager stageManager = new(root);

        stageManager.Load(services =>
        {
            services.AddSingleton<IView>(new TestView("stage"));
        });
        stageManager.ApplyPendingTransition();

        stageManager.ApplyPendingTransition();

        Assert.That(ViewNames(viewRegistry), Is.EqualTo(new[] { "stage" }));
    }

    [Test]
    public void Dispose_DisposesActiveStage()
    {
        ViewRegistry viewRegistry = new();
        ServiceProvider root = BuildRootProvider(viewRegistry);
        StageManager stageManager = new(root);

        stageManager.Load(services =>
        {
            services.AddSingleton<IView>(new TestView("stage"));
        });
        stageManager.ApplyPendingTransition();

        stageManager.Dispose();

        Assert.That(viewRegistry.Views.Length, Is.EqualTo(0));
    }

    [Test]
    public void Dispose_ClearsPendingLoad()
    {
        ViewRegistry viewRegistry = new();
        ServiceProvider root = BuildRootProvider(viewRegistry);
        StageManager stageManager = new(root);

        stageManager.Load(services =>
        {
            services.AddSingleton<IView>(new TestView("stage"));
        });

        stageManager.Dispose();
        stageManager.ApplyPendingTransition();

        Assert.That(viewRegistry.Views.Length, Is.EqualTo(0));
    }

    [Test]
    public void Load_RegistersStageServicesViaParentCallbacksOnPendingTransition()
    {
        ViewRegistry viewRegistry = new();
        ServiceProvider root = BuildRootProvider(viewRegistry);
        StageManager stageManager = new(root);

        stageManager.Load(services =>
        {
            services.AddSingleton<IView>(new TestView("stage"));
        });
        stageManager.ApplyPendingTransition();

        Assert.That(ViewNames(viewRegistry), Is.EqualTo(new[] { "stage" }));
    }

    [Test]
    public void Load_StageServicesCanResolveRootServices()
    {
        ServiceCollection rootCollection = new();
        rootCollection.AddSingleton(new WindowConfig { Title = "test" });
        ServiceProvider root = rootCollection.BuildServiceProvider();
        StageManager stageManager = new(root);

        WindowConfig? resolved = null;
        stageManager.Load(services =>
        {
            services.AddSingleton<IView>(sp =>
            {
                resolved = sp.GetRequiredService<WindowConfig>();
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
        ServiceProvider root = BuildRootProvider(new ViewRegistry());
        StageManager stageManager = new(root);

        DisposableService disposable = new();
        stageManager.Load(services =>
        {
            services.AddSingleton(disposable);
        });
        stageManager.ApplyPendingTransition();

        stageManager.Load(services =>
        {
            services.AddSingleton<IView>(new TestView("next"));
        });
        stageManager.ApplyPendingTransition();

        Assert.That(disposable.IsDisposed, Is.True);
    }

    private static ServiceProvider BuildRootProvider(ViewRegistry viewRegistry)
    {
        ServiceCollection rootCollection = new();
        rootCollection.AddSingleton(viewRegistry);
        rootCollection.OnActivated((instance, _) =>
        {
            if (instance is IView view)
            {
                viewRegistry.Add(view);
            }
        });
        rootCollection.OnDisposing((instance, _) =>
        {
            if (instance is IView view)
            {
                viewRegistry.Remove(view);
            }
        });
        return rootCollection.BuildServiceProvider();
    }

    private static string[] ViewNames(ViewRegistry viewRegistry)
    {
        ReadOnlySpan<IView> views = viewRegistry.Views;
        string[] names = new string[views.Length];
        for (int i = 0; i < views.Length; i++)
        {
            names[i] = ((TestView)views[i]).Name;
        }
        return names;
    }

    private sealed class TestView : IView
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
}
