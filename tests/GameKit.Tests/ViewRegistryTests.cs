using GameKit.DependencyInjection;
using GameKit.Pencuil;

namespace GameKit.Tests;

public class PencuilViewRegistryTests
{
    [Test]
    public void ChildProviderView_IsAddedAfterChildBuild()
    {
        PencuilViewRegistry viewRegistry = new(new ViewScope(0));
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
        ServiceProvider root = rootCollection.BuildServiceProvider();

        ServiceCollection childCollection = root.CreateServiceCollection();
        childCollection.AddSingleton<IPencuilView>(new TestView("child"));
        using ServiceProvider child = childCollection.BuildServiceProvider();

        Assert.That(ViewNames(viewRegistry), Is.EqualTo(new[] { "child" }));
    }

    [Test]
    public void ChildProviderView_IsRemovedWhenChildProviderIsDisposed()
    {
        PencuilViewRegistry viewRegistry = new(new ViewScope(0));
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
        ServiceProvider root = rootCollection.BuildServiceProvider();

        ServiceCollection childCollection = root.CreateServiceCollection();
        childCollection.AddSingleton<IPencuilView>(new TestView("child"));
        ServiceProvider child = childCollection.BuildServiceProvider();

        child.Dispose();

        Assert.That(viewRegistry.Views.Length, Is.EqualTo(0));
    }

    [Test]
    public void RootRegisteredView_AppearsInRegistry()
    {
        PencuilViewRegistry viewRegistry = new(new ViewScope(0));
        ServiceCollection rootCollection = new();
        rootCollection.AddSingleton(viewRegistry);
        rootCollection.AddSingleton<IPencuilView>(new TestView("root"));
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
        ServiceProvider root = rootCollection.BuildServiceProvider();

        Assert.That(ViewNames(viewRegistry), Is.EqualTo(new[] { "root" }));
    }

    [Test]
    public void MultipleChildProviders_ViewsAddedAndRemovedIndependently()
    {
        PencuilViewRegistry viewRegistry = new(new ViewScope(0));
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
        ServiceProvider root = rootCollection.BuildServiceProvider();

        ServiceCollection child1Collection = root.CreateServiceCollection();
        child1Collection.AddSingleton<IPencuilView>(new TestView("child1"));
        ServiceProvider child1 = child1Collection.BuildServiceProvider();

        ServiceCollection child2Collection = root.CreateServiceCollection();
        child2Collection.AddSingleton<IPencuilView>(new TestView("child2"));
        using ServiceProvider child2 = child2Collection.BuildServiceProvider();

        child1.Dispose();

        Assert.That(ViewNames(viewRegistry), Is.EqualTo(new[] { "child2" }));
    }

    [Test]
    public void DuplicateView_IsNotAddedTwice()
    {
        PencuilViewRegistry viewRegistry = new(new ViewScope(0));
        TestView view = new("test");

        viewRegistry.Add(view);
        viewRegistry.Add(view);

        Assert.That(viewRegistry.Views.Length, Is.EqualTo(1));
    }

    [Test]
    public void Add_ViewFromAnotherScope_Throws()
    {
        PencuilViewRegistry viewRegistry = new(new ViewScope(0));
        TestView view = new("test", new ViewScope(1));

        Assert.Throws<InvalidOperationException>(() => viewRegistry.Add(view));
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
        public ViewScope ViewScope { get; }

        public TestView(string name, ViewScope viewScope = default)
        {
            Name = name;
            ViewScope = viewScope;
        }

        public bool ConsumeDirty() => false;

        public void Build(Pencil pencil) { }
    }
}
