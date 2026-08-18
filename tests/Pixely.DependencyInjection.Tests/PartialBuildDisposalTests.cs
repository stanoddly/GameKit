using Pixely.DependencyInjection;

namespace Pixely.DependencyInjection.Tests;

public class PartialBuildDisposalTests
{
    [Test]
    public void BuildServiceProvider_WhenBuildThrows_DisposesPartiallyCreatedServices()
    {
        ServiceProvider? capturedProvider = null;
        ServiceA? capturedServiceA = null;
        using ServiceProvider parent = new ServiceCollection().BuildServiceProvider();

        ServiceCollection collection = parent.CreateServiceCollection();
        collection.AddSingleton<ServiceA>((ServiceProvider sp) =>
        {
            capturedProvider = sp;
            ServiceA instance = new();
            capturedServiceA = instance;
            return instance;
        });
        collection.AddSingleton<ServiceB>((ServiceProvider sp) =>
        {
            sp.GetRequiredService<ServiceA>();
            throw new InvalidOperationException("boom");
        });

        Assert.Throws<InvalidOperationException>(() => collection.BuildServiceProvider());

        Assert.That(capturedProvider, Is.Not.Null);
        Assert.That(capturedServiceA, Is.Not.Null);
        Assert.That(capturedServiceA!.Disposed, Is.True);
        Assert.Throws<ObjectDisposedException>(() => capturedProvider!.GetRequiredService<ServiceA>());

        parent.Dispose();

        Assert.That(capturedServiceA.DisposeCount, Is.EqualTo(1));
    }

    private class ServiceA : IDisposable
    {
        public bool Disposed { get; private set; }
        public int DisposeCount { get; private set; }

        public void Dispose()
        {
            Disposed = true;
            DisposeCount++;
        }
    }

    private class ServiceB
    {
    }
}
