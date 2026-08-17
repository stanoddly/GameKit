using GameKit.DependencyInjection;
using GameKit.Pencuil;
using PencuilInstance = GameKit.Pencuil.Pencuil;

namespace GameKit.Tests;

public sealed class PencuilTests
{
    [Test]
    public void ViewRegistry_TracksViewsByScope()
    {
        ViewScope rightScope = new(1);
        PencuilViewRegistry registry = new();
        registry.Add(new TestView("left"));
        registry.Add(new TestView("right", rightScope));

        Assert.Multiple(() =>
        {
            Assert.That(registry.ConsumeChanged(default), Is.True);
            Assert.That(registry.ConsumeChanged(rightScope), Is.True);
            Assert.That(ViewNames(registry, default), Is.EqualTo(new[] { "left" }));
            Assert.That(ViewNames(registry, rightScope), Is.EqualTo(new[] { "right" }));
        });
    }

    [Test]
    public void ViewRegistry_ChangeInAnotherScopeDoesNotInvalidateScope()
    {
        ViewScope rightScope = new(1);
        PencuilViewRegistry registry = new();

        registry.Add(new TestView("right", rightScope));

        Assert.Multiple(() =>
        {
            Assert.That(registry.ConsumeChanged(default), Is.False);
            Assert.That(registry.ConsumeChanged(rightScope), Is.True);
        });
    }

    [Test]
    public void ViewRegistry_RemovesDisposedChildProviderView()
    {
        ServiceCollection rootServices = new();
        PencuilViewRegistry.AddPencuilViewRegistry(rootServices);
        using ServiceProvider root = rootServices.BuildServiceProvider();
        PencuilViewRegistry registry = root.GetRequiredService<PencuilViewRegistry>();
        ServiceCollection childServices = root.CreateServiceCollection();
        childServices.AddSingleton<IPencuilView>(new TestView("child"));
        ServiceProvider child = childServices.BuildServiceProvider();
        Assert.That(ViewNames(registry, default), Is.EqualTo(new[] { "child" }));
        Assert.That(registry.ConsumeChanged(default), Is.True);

        child.Dispose();

        Assert.Multiple(() =>
        {
            Assert.That(registry.ConsumeChanged(default), Is.True);
            Assert.That(ViewNames(registry, default), Is.Empty);
        });
    }

    [Test]
    public void ViewRegistry_AddAndRemoveTrackViewOnce()
    {
        PencuilViewRegistry registry = new();
        TestView view = new("component");

        registry.Add(view);
        registry.Add(view);

        Assert.That(ViewNames(registry, default), Is.EqualTo(new[] { "component" }));

        registry.Remove(view);

        Assert.That(ViewNames(registry, default), Is.Empty);
    }

    [Test]
    public void GetRequired_DuplicateScope_Throws()
    {
        ServiceCollection services = new();
        services.AddRegistry<PencuilInstance>();
        services.AddSingleton(new PencuilInstance(default, null!));
        services.AddSingleton(new PencuilInstance(default, null!));
        using ServiceProvider provider = services.BuildServiceProvider();
        ServiceRegistry<PencuilInstance> pencuils =
            provider.GetRequiredService<ServiceRegistry<PencuilInstance>>();

        Assert.Throws<InvalidOperationException>(() => PencuilInstance.GetRequired(pencuils, default));
    }

    [Test]
    public void GetRequired_MissingScope_Throws()
    {
        ServiceCollection services = new();
        services.AddRegistry<PencuilInstance>();
        using ServiceProvider provider = services.BuildServiceProvider();
        ServiceRegistry<PencuilInstance> pencuils =
            provider.GetRequiredService<ServiceRegistry<PencuilInstance>>();

        Assert.Throws<InvalidOperationException>(() => PencuilInstance.GetRequired(pencuils, default));
    }

    private static string[] ViewNames(PencuilViewRegistry registry, ViewScope viewScope)
    {
        List<IPencuilView> views = new();
        registry.CopyViews(viewScope, views);
        string[] names = new string[views.Count];
        for (int i = 0; i < views.Count; i++)
        {
            names[i] = ((TestView)views[i]).Name;
        }

        return names;
    }

    private sealed class TestView : IPencuilView
    {
        private readonly ViewScope _viewScope;

        public string Name { get; }

        ViewScope IViewScoped.ViewScope => _viewScope;

        public TestView(string name, ViewScope viewScope = default)
        {
            Name = name;
            _viewScope = viewScope;
        }

        public bool ConsumeDirty() => false;

        public void Build(Pencil pencil)
        {
        }
    }
}
