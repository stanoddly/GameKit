using GameKit.DependencyInjection;

namespace GameKit.DependencyInjection.Tests;

public interface IServiceCallbackContract;

public sealed class CallbackConcreteService;

public sealed class CallbackImplementationService : IServiceCallbackContract;

public sealed class CallbackFactoryService;

public sealed class CallbackInstanceService;

public sealed class CallbackFirstService;

public sealed class CallbackSecondService;

public sealed class CallbackDisposableService : IDisposable
{
    private readonly List<string> _events;

    public CallbackDisposableService(List<string> events)
    {
        _events = events;
    }

    public bool Disposed { get; private set; }

    public void Dispose()
    {
        _events.Add("dispose");
        Disposed = true;
    }
}

public sealed class ServiceCallbackTests
{
    [Test]
    public void OnActivated_FiresOncePerSingleton_WithInstanceAndConcreteType()
    {
        List<(object Instance, Type Type)> activations = new();
        CallbackInstanceService instanceService = new();

        ServiceCollection collection = new();
        collection.OnActivated((instance, type) => activations.Add((instance, type)));
        collection.AddSingleton<CallbackConcreteService>();
        collection.AddSingleton<IServiceCallbackContract, CallbackImplementationService>();
        collection.AddSingleton<CallbackFactoryService>(static sp => new CallbackFactoryService());
        collection.AddSingleton(instanceService);

        ServiceProvider provider = collection.BuildServiceProvider();

        Assert.That(activations, Has.Count.EqualTo(4));
        Assert.That(activations[0].Instance, Is.SameAs(provider.GetRequiredService<CallbackConcreteService>()));
        Assert.That(activations[0].Type, Is.EqualTo(typeof(CallbackConcreteService)));
        Assert.That(activations[1].Instance, Is.SameAs(provider.GetRequiredService<IServiceCallbackContract>()));
        Assert.That(activations[1].Type, Is.EqualTo(typeof(CallbackImplementationService)));
        Assert.That(activations[2].Instance, Is.SameAs(provider.GetRequiredService<CallbackFactoryService>()));
        Assert.That(activations[2].Type, Is.EqualTo(typeof(CallbackFactoryService)));
        Assert.That(activations[3].Instance, Is.SameAs(instanceService));
        Assert.That(activations[3].Type, Is.EqualTo(typeof(CallbackInstanceService)));
    }

    [Test]
    public void OnActivated_MultipleCallbacks_FireInRegistrationOrder()
    {
        List<string> calls = new();

        ServiceCollection collection = new();
        collection.OnActivated((instance, type) => calls.Add("first"));
        collection.OnActivated((instance, type) => calls.Add("second"));
        collection.AddSingleton<CallbackConcreteService>();

        collection.BuildServiceProvider();

        Assert.That(calls, Is.EqualTo(new[] { "first", "second" }));
    }

    [Test]
    public void OnActivated_FiresDuringBuild_EvenBeforeExplicitResolve()
    {
        List<Type> activations = new();

        ServiceCollection collection = new();
        collection.OnActivated((instance, type) => activations.Add(type));
        collection.AddSingleton<CallbackConcreteService>();

        ServiceProvider provider = collection.BuildServiceProvider();

        Assert.That(activations, Is.EqualTo(new[] { typeof(CallbackConcreteService) }));
        Assert.That(provider.GetRequiredService<CallbackConcreteService>(), Is.Not.Null);
        Assert.That(activations, Has.Count.EqualTo(1));
    }

    [Test]
    public void OnActivated_ReceivesOwningProvider()
    {
        ServiceProvider? callbackProvider = null;

        ServiceCollection collection = new();
        collection.OnActivated((instance, type, provider) => callbackProvider = provider);
        collection.AddSingleton<CallbackConcreteService>();

        ServiceProvider provider = collection.BuildServiceProvider();

        Assert.That(callbackProvider, Is.SameAs(provider));
    }

    [Test]
    public void OnDisposing_FiresBeforeOwnDispose()
    {
        List<string> events = new();

        ServiceCollection collection = new();
        collection.AddSingleton<List<string>>(events);
        collection.AddSingleton<CallbackDisposableService>();
        collection.OnDisposing((instance, type) =>
        {
            if (instance is CallbackDisposableService disposableService)
            {
                events.Add(disposableService.Disposed ? "callback-after-dispose" : "callback-before-dispose");
            }
        });

        ServiceProvider provider = collection.BuildServiceProvider();
        CallbackDisposableService service = provider.GetRequiredService<CallbackDisposableService>();

        provider.Dispose();

        Assert.That(service.Disposed, Is.True);
        Assert.That(events, Is.EqualTo(new[] { "callback-before-dispose", "dispose" }));
    }

    [Test]
    public void OnDisposing_FiresInReverseConstructionOrder()
    {
        List<string> calls = new();

        ServiceCollection collection = new();
        collection.AddSingleton<CallbackFirstService>();
        collection.AddSingleton<CallbackSecondService>();
        collection.OnDisposing((instance, type) => calls.Add(type.Name));

        collection.BuildServiceProvider().Dispose();

        Assert.That(calls, Is.EqualTo(new[] { nameof(CallbackSecondService), nameof(CallbackFirstService) }));
    }
}
