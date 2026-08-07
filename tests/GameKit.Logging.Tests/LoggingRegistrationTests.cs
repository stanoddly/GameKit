using GameKit.DependencyInjection;
using GameKit.Logging;
using Microsoft.Extensions.Logging;

namespace GameKit.Logging.Tests;

public class LoggingRegistrationTests
{
    [Test]
    public void AddZLogger_RegistersOwnedLoggerFactoryAndApplicationLogger()
    {
        ServiceCollection services = new();
        services.AddZLogger(static logging => logging.SetMinimumLevel(LogLevel.Trace));

        using ServiceProvider serviceProvider = services.BuildServiceProvider();

        ILoggerFactory loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        ILogger first = serviceProvider.GetRequiredService<ILogger>();
        ILogger second = serviceProvider.GetRequiredService<ILogger>();

        Assert.Multiple(() =>
        {
            Assert.That(loggerFactory, Is.Not.Null);
            Assert.That(second, Is.SameAs(first));
        });
    }

    [Test]
    public void AddZLogger_WhenFactoryIsAlreadyRegistered_Throws()
    {
        ServiceCollection services = new();
        services.AddZLogger(static _ => { });

        InvalidOperationException? exception = Assert.Throws<InvalidOperationException>(
            () => services.AddZLogger(static _ => { }));

        Assert.That(exception!.Message, Does.Contain(nameof(ILoggerFactory)));
    }

    [Test]
    public void AddZLogger_WhenApplicationLoggerIsAlreadyRegistered_Throws()
    {
        ServiceCollection services = new();
        using ILoggerFactory loggerFactory = LoggerFactory.Create(static _ => { });
        services.AddSingleton(loggerFactory.CreateLogger("Existing"));

        InvalidOperationException? exception = Assert.Throws<InvalidOperationException>(
            () => services.AddZLogger(static _ => { }));

        Assert.That(exception!.Message, Does.Contain(nameof(ILogger)));
    }

    [Test]
    public void ApplicationLogger_IsAvailableFromChildProvider()
    {
        ServiceCollection rootServices = new();
        rootServices.AddZLogger(static _ => { });
        using ServiceProvider rootProvider = rootServices.BuildServiceProvider();

        ServiceCollection childServices = new();
        using ServiceProvider childProvider = childServices.BuildServiceProvider(rootProvider);

        Assert.That(
            childProvider.GetRequiredService<ILogger>(),
            Is.SameAs(rootProvider.GetRequiredService<ILogger>()));
    }
}
