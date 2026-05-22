using GameKit.DependencyInjection;
using GameKit.Pencuil;

namespace GameKit.Tests;

public class ViewRegistryTests
{
    [Test]
    public void ChildProviderView_IsAddedAfterChildBuild()
    {
        ViewRegistry viewRegistry = new();
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
        ServiceProvider root = rootCollection.BuildServiceProvider();

        ServiceCollection childCollection = new();
        childCollection.AddSingleton<IView>(new TestView("child"));
        using ServiceProvider child = childCollection.BuildServiceProvider(root);

        Assert.That(
            viewRegistry.Views.Where(v => v != null).Cast<TestView>().Select(v => v.Name),
            Is.EqualTo(new[] { "child" }));
    }

    [Test]
    public void ChildProviderView_IsRemovedWhenChildProviderIsDisposed()
    {
        ViewRegistry viewRegistry = new();
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
        ServiceProvider root = rootCollection.BuildServiceProvider();

        ServiceCollection childCollection = new();
        childCollection.AddSingleton<IView>(new TestView("child"));
        ServiceProvider child = childCollection.BuildServiceProvider(root);

        child.Dispose();
        viewRegistry.Compact();

        Assert.That(viewRegistry.Views, Is.Empty);
    }

    [Test]
    public void RootRegisteredView_AppearsInRegistry()
    {
        ViewRegistry viewRegistry = new();
        ServiceCollection rootCollection = new();
        rootCollection.AddSingleton(viewRegistry);
        rootCollection.AddSingleton<IView>(new TestView("root"));
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
        ServiceProvider root = rootCollection.BuildServiceProvider();

        Assert.That(
            viewRegistry.Views.Where(v => v != null).Cast<TestView>().Select(v => v.Name),
            Is.EqualTo(new[] { "root" }));
    }

    [Test]
    public void MultipleChildProviders_ViewsAddedAndRemovedIndependently()
    {
        ViewRegistry viewRegistry = new();
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
        ServiceProvider root = rootCollection.BuildServiceProvider();

        ServiceCollection child1Collection = new();
        child1Collection.AddSingleton<IView>(new TestView("child1"));
        ServiceProvider child1 = child1Collection.BuildServiceProvider(root);

        ServiceCollection child2Collection = new();
        child2Collection.AddSingleton<IView>(new TestView("child2"));
        using ServiceProvider child2 = child2Collection.BuildServiceProvider(root);

        child1.Dispose();
        viewRegistry.Compact();

        Assert.That(
            viewRegistry.Views.Where(v => v != null).Cast<TestView>().Select(v => v.Name),
            Is.EqualTo(new[] { "child2" }));
    }

    [Test]
    public void DuplicateView_IsNotAddedTwice()
    {
        ViewRegistry viewRegistry = new();
        TestView view = new("test");

        viewRegistry.Add(view);
        viewRegistry.Add(view);

        Assert.That(viewRegistry.Views, Has.Count.EqualTo(1));
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
}
