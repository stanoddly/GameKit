using GameKit.DependencyInjection;
using GameKit.Logging;
using Microsoft.Extensions.Logging;

namespace GameKit.Logging.Tests;

public class LoggingRegistrationTests
{
    [Test]
    public void AddZLogger_RegistersOwnedLoggerFactoryAndCategoryLogger()
    {
        ServiceCollection services = new();
        services.AddZLogger(static logging => logging.SetMinimumLevel(LogLevel.Trace));
        services.AddLogger<LoggingRegistrationTests>();

        using ServiceProvider serviceProvider = services.BuildServiceProvider();

        ILoggerFactory loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        ILogger<LoggingRegistrationTests> first = serviceProvider.GetRequiredService<ILogger<LoggingRegistrationTests>>();
        ILogger<LoggingRegistrationTests> second = serviceProvider.GetRequiredService<ILogger<LoggingRegistrationTests>>();

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
    public void AddLogger_BeforeLoggerFactory_ResolvesAfterFactoryRegistration()
    {
        ServiceCollection services = new();
        services.AddLogger<LoggingRegistrationTests>();
        services.AddZLogger(static _ => { });

        using ServiceProvider serviceProvider = services.BuildServiceProvider();

        Assert.That(serviceProvider.GetRequiredService<ILogger<LoggingRegistrationTests>>(), Is.Not.Null);
    }

    [Test]
    public void AddLogger_InChildProvider_UsesParentLoggerFactory()
    {
        ServiceCollection rootServices = new();
        rootServices.AddZLogger(static _ => { });
        using ServiceProvider rootProvider = rootServices.BuildServiceProvider();

        ServiceCollection childServices = new();
        childServices.AddLogger<LoggingRegistrationTests>();
        using ServiceProvider childProvider = childServices.BuildServiceProvider(rootProvider);

        Assert.That(childProvider.GetRequiredService<ILogger<LoggingRegistrationTests>>(), Is.Not.Null);
    }

    [Test]
    public void AddLogger_WhenCategoryIsAlreadyRegistered_IsIdempotent()
    {
        ServiceCollection services = new();
        services.AddZLogger(static _ => { });

        services.AddLogger<LoggingRegistrationTests>();
        services.AddLogger<LoggingRegistrationTests>();

        using ServiceProvider serviceProvider = services.BuildServiceProvider();

        Assert.That(
            serviceProvider.GetServices<ILogger<LoggingRegistrationTests>>().Count(),
            Is.EqualTo(1));
    }
}
