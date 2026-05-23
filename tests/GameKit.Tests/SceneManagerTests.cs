using GameKit.App;
using GameKit.DependencyInjection;
using GameKit.Pencuil;

namespace GameKit.Tests;

public class SceneManagerTests
{
    [Test]
    public void Load_DoesNotApplyImmediately()
    {
        ViewRegistry viewRegistry = new();
        ServiceProvider root = BuildRootProvider(viewRegistry);
        SceneManager sceneManager = new(root);

        sceneManager.Load(services =>
        {
            services.AddSingleton<IView>(new TestView("scene"));
        });

        Assert.That(viewRegistry.Views.Length, Is.EqualTo(0));
    }

    [Test]
    public void Load_AppliesOnUpdate()
    {
        ViewRegistry viewRegistry = new();
        ServiceProvider root = BuildRootProvider(viewRegistry);
        SceneManager sceneManager = new(root);

        sceneManager.Load(services =>
        {
            services.AddSingleton<IView>(new TestView("scene"));
        });
        sceneManager.ApplyPendingTransition();

        Assert.That(ViewNames(viewRegistry), Is.EqualTo(new[] { "scene" }));
    }

    [Test]
    public void Load_MultipleBeforeUpdate_LastWins()
    {
        ViewRegistry viewRegistry = new();
        ServiceProvider root = BuildRootProvider(viewRegistry);
        SceneManager sceneManager = new(root);

        sceneManager.Load(services =>
        {
            services.AddSingleton<IView>(new TestView("first"));
        });
        sceneManager.Load(services =>
        {
            services.AddSingleton<IView>(new TestView("second"));
        });
        sceneManager.ApplyPendingTransition();

        Assert.That(ViewNames(viewRegistry), Is.EqualTo(new[] { "second" }));
    }

    [Test]
    public void Load_DisposesPreviousSceneOnUpdate()
    {
        ViewRegistry viewRegistry = new();
        ServiceProvider root = BuildRootProvider(viewRegistry);
        SceneManager sceneManager = new(root);

        sceneManager.LoadImmediately(services =>
        {
            services.AddSingleton<IView>(new TestView("first"));
        });

        sceneManager.Load(services =>
        {
            services.AddSingleton<IView>(new TestView("second"));
        });
        sceneManager.ApplyPendingTransition();

        Assert.That(ViewNames(viewRegistry), Is.EqualTo(new[] { "second" }));
    }

    [Test]
    public void Unload_DoesNotApplyImmediately()
    {
        ViewRegistry viewRegistry = new();
        ServiceProvider root = BuildRootProvider(viewRegistry);
        SceneManager sceneManager = new(root);

        sceneManager.LoadImmediately(services =>
        {
            services.AddSingleton<IView>(new TestView("scene"));
        });

        sceneManager.Unload();

        Assert.That(ViewNames(viewRegistry), Is.EqualTo(new[] { "scene" }));
    }

    [Test]
    public void Unload_AppliesOnUpdate()
    {
        ViewRegistry viewRegistry = new();
        ServiceProvider root = BuildRootProvider(viewRegistry);
        SceneManager sceneManager = new(root);

        sceneManager.LoadImmediately(services =>
        {
            services.AddSingleton<IView>(new TestView("scene"));
        });

        sceneManager.Unload();
        sceneManager.ApplyPendingTransition();

        Assert.That(viewRegistry.Views.Length, Is.EqualTo(0));
    }

    [Test]
    public void Unload_WhenNoSceneLoaded_DoesNotThrow()
    {
        ServiceProvider root = BuildRootProvider(new ViewRegistry());
        SceneManager sceneManager = new(root);

        sceneManager.Unload();

        Assert.DoesNotThrow(() => sceneManager.ApplyPendingTransition());
    }

    [Test]
    public void Load_CancelsPendingUnload()
    {
        ViewRegistry viewRegistry = new();
        ServiceProvider root = BuildRootProvider(viewRegistry);
        SceneManager sceneManager = new(root);

        sceneManager.LoadImmediately(services =>
        {
            services.AddSingleton<IView>(new TestView("first"));
        });

        sceneManager.Unload();
        sceneManager.Load(services =>
        {
            services.AddSingleton<IView>(new TestView("second"));
        });
        sceneManager.ApplyPendingTransition();

        Assert.That(ViewNames(viewRegistry), Is.EqualTo(new[] { "second" }));
    }

    [Test]
    public void Unload_CancelsPendingLoad()
    {
        ViewRegistry viewRegistry = new();
        ServiceProvider root = BuildRootProvider(viewRegistry);
        SceneManager sceneManager = new(root);

        sceneManager.Load(services =>
        {
            services.AddSingleton<IView>(new TestView("scene"));
        });
        sceneManager.Unload();
        sceneManager.ApplyPendingTransition();

        Assert.That(viewRegistry.Views.Length, Is.EqualTo(0));
    }

    [Test]
    public void Update_WithNoPending_DoesNothing()
    {
        ViewRegistry viewRegistry = new();
        ServiceProvider root = BuildRootProvider(viewRegistry);
        SceneManager sceneManager = new(root);

        sceneManager.LoadImmediately(services =>
        {
            services.AddSingleton<IView>(new TestView("scene"));
        });

        sceneManager.ApplyPendingTransition();

        Assert.That(ViewNames(viewRegistry), Is.EqualTo(new[] { "scene" }));
    }

    [Test]
    public void Dispose_UnloadsActiveScene()
    {
        ViewRegistry viewRegistry = new();
        ServiceProvider root = BuildRootProvider(viewRegistry);
        SceneManager sceneManager = new(root);

        sceneManager.LoadImmediately(services =>
        {
            services.AddSingleton<IView>(new TestView("scene"));
        });

        sceneManager.Dispose();

        Assert.That(viewRegistry.Views.Length, Is.EqualTo(0));
    }

    [Test]
    public void Dispose_ClearsPendingLoad()
    {
        ViewRegistry viewRegistry = new();
        ServiceProvider root = BuildRootProvider(viewRegistry);
        SceneManager sceneManager = new(root);

        sceneManager.Load(services =>
        {
            services.AddSingleton<IView>(new TestView("scene"));
        });

        sceneManager.Dispose();
        sceneManager.ApplyPendingTransition();

        Assert.That(viewRegistry.Views.Length, Is.EqualTo(0));
    }

    [Test]
    public void LoadImmediately_RegistersSceneServicesViaParentCallbacks()
    {
        ViewRegistry viewRegistry = new();
        ServiceProvider root = BuildRootProvider(viewRegistry);
        SceneManager sceneManager = new(root);

        sceneManager.LoadImmediately(services =>
        {
            services.AddSingleton<IView>(new TestView("scene"));
        });

        Assert.That(ViewNames(viewRegistry), Is.EqualTo(new[] { "scene" }));
    }

    [Test]
    public void LoadImmediately_DisposesPreviousScene()
    {
        ViewRegistry viewRegistry = new();
        ServiceProvider root = BuildRootProvider(viewRegistry);
        SceneManager sceneManager = new(root);

        sceneManager.LoadImmediately(services =>
        {
            services.AddSingleton<IView>(new TestView("first"));
        });

        sceneManager.LoadImmediately(services =>
        {
            services.AddSingleton<IView>(new TestView("second"));
        });

        Assert.That(ViewNames(viewRegistry), Is.EqualTo(new[] { "second" }));
    }

    [Test]
    public void LoadImmediately_SceneServicesCanResolveRootServices()
    {
        ServiceCollection rootCollection = new();
        rootCollection.AddSingleton(new AppConfig { Title = "test" });
        ServiceProvider root = rootCollection.BuildServiceProvider();
        SceneManager sceneManager = new(root);

        AppConfig? resolved = null;
        sceneManager.LoadImmediately(services =>
        {
            services.AddSingleton<IView>(sp =>
            {
                resolved = sp.GetRequiredService<AppConfig>();
                return new TestView("scene");
            });
        });

        Assert.That(resolved, Is.Not.Null);
        Assert.That(resolved!.Title, Is.EqualTo("test"));
    }

    [Test]
    public void UnloadImmediately_DisposesSceneOwnedDisposables()
    {
        ServiceProvider root = BuildRootProvider(new ViewRegistry());
        SceneManager sceneManager = new(root);

        DisposableService disposable = new();
        sceneManager.LoadImmediately(services =>
        {
            services.AddSingleton(disposable);
        });

        sceneManager.UnloadImmediately();

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
