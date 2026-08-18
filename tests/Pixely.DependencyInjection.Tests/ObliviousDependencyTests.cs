#nullable disable

using Pixely.DependencyInjection;

namespace Pixely.DependencyInjection.Tests;

public sealed class ObliviousDependencyConsumer
{
    public ObliviousDependencyConsumer(SimpleService service)
    {
        Service = service;
    }

    public SimpleService Service { get; }
}

public sealed class ObliviousDependencyTests
{
    [Test]
    public void SingletonConstructor_MissingObliviousDependency_Throws()
    {
        ServiceCollection collection = new();
        collection.AddSingleton<ObliviousDependencyConsumer>();

        Assert.Throws<InvalidOperationException>(() => collection.BuildServiceProvider());
    }
}
