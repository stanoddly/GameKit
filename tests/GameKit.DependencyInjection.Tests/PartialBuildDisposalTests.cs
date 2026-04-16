using GameKit.DependencyInjection;

namespace GameKit.DependencyInjection.Tests;

public class PartialBuildDisposalTests
{
    [Test]
    public void Dispose_AfterBuildThrows_DisposesPartiallyCreatedServices()
    {
        ServiceProvider? capturedProvider = null;
        ServiceA? capturedServiceA = null;

        ServiceCollection collection = new();
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
        Assert.That(capturedServiceA!.Disposed, Is.False);

        capturedProvider!.Dispose();

        Assert.That(capturedServiceA.Disposed, Is.True);
    }

    private class ServiceA : IDisposable
    {
        public bool Disposed { get; private set; }

        public void Dispose()
        {
            Disposed = true;
        }
    }

    private class ServiceB
    {
    }
}
