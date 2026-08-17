using GameKit.DependencyInjection;
using GameKit.Pencuil;
using PencuilInstance = GameKit.Pencuil.Pencuil;

namespace GameKit.Tests;

public sealed class PencuilTests
{
    [Test]
    public void SynchronizeViews_TracksOnlyMatchingScope()
    {
        ViewScope rightScope = new(1);
        TestView leftView = new("left");
        TestView rightView = new("right", rightScope);
        using ServiceProvider provider = BuildViewProvider(
            out ServiceRegistry<IPencuilView> views,
            leftView,
            rightView);
        PencuilInstance left = new(default, null!);
        PencuilInstance right = new(rightScope, null!);

        bool leftChanged = left.SynchronizeViews(views);
        bool rightChanged = right.SynchronizeViews(views);

        Assert.Multiple(() =>
        {
            Assert.That(leftChanged, Is.True);
            Assert.That(rightChanged, Is.True);
            Assert.That(ViewNames(left), Is.EqualTo(new[] { "left" }));
            Assert.That(ViewNames(right), Is.EqualTo(new[] { "right" }));
        });
    }

    [Test]
    public void SynchronizeViews_ChangeInAnotherScopeDoesNotInvalidateViews()
    {
        ViewScope rightScope = new(1);
        using ServiceProvider root = BuildViewProvider(
            out ServiceRegistry<IPencuilView> views);
        PencuilInstance left = new(default, null!);
        Assert.That(left.SynchronizeViews(views), Is.False);

        ServiceCollection childServices = root.CreateServiceCollection();
        childServices.AddSingleton<IPencuilView>(new TestView("right", rightScope));
        using ServiceProvider child = childServices.BuildServiceProvider();

        Assert.That(left.SynchronizeViews(views), Is.False);
        Assert.That(ViewNames(left), Is.Empty);
    }

    [Test]
    public void SynchronizeViews_RemovesDisposedChildProviderView()
    {
        using ServiceProvider root = BuildViewProvider(
            out ServiceRegistry<IPencuilView> views);
        ServiceCollection childServices = root.CreateServiceCollection();
        childServices.AddSingleton<IPencuilView>(new TestView("child"));
        ServiceProvider child = childServices.BuildServiceProvider();
        PencuilInstance pencuil = new(default, null!);
        Assert.That(pencuil.SynchronizeViews(views), Is.True);

        child.Dispose();

        Assert.That(pencuil.SynchronizeViews(views), Is.True);
        Assert.That(ViewNames(pencuil), Is.Empty);
    }

    [Test]
    public void ComponentView_IsAddedAndRemoved()
    {
        using ServiceProvider provider = BuildViewProvider(
            out ServiceRegistry<IPencuilView> views);
        PencuilInstance pencuil = new(default, null!);
        TestView view = new("component");

        pencuil.AddComponentView(view);

        Assert.That(pencuil.SynchronizeViews(views), Is.True);
        Assert.That(ViewNames(pencuil), Is.EqualTo(new[] { "component" }));

        pencuil.RemoveComponentView(view);

        Assert.That(pencuil.SynchronizeViews(views), Is.True);
        Assert.That(ViewNames(pencuil), Is.Empty);
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

    private static ServiceProvider BuildViewProvider(
        out ServiceRegistry<IPencuilView> registry,
        params IPencuilView[] views)
    {
        ServiceCollection services = new();
        services.AddRegistry<IPencuilView>();
        foreach (IPencuilView view in views)
        {
            services.AddSingleton(view);
        }

        ServiceProvider provider = services.BuildServiceProvider();
        registry = provider.GetRequiredService<ServiceRegistry<IPencuilView>>();
        return provider;
    }

    private static string[] ViewNames(PencuilInstance pencuil)
    {
        ReadOnlySpan<IPencuilView> views = pencuil.Views;
        string[] names = new string[views.Length];
        for (int i = 0; i < views.Length; i++)
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
