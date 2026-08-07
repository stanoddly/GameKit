using GameKit.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GameKit.Logging;

public static class LoggingServiceCollectionExtensions
{
    private const string ApplicationLoggerCategoryName = "Application";

    public static ServiceCollection AddZLogger(
        this ServiceCollection services,
        Action<ILoggingBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        if (services.IsRegistered<ILoggerFactory>())
        {
            throw new InvalidOperationException($"{nameof(ILoggerFactory)} is already registered.");
        }

        if (services.IsRegistered<ILogger>())
        {
            throw new InvalidOperationException($"{nameof(ILogger)} is already registered.");
        }

        services.AddSingleton<ILoggerFactory>(_ => LoggerFactory.Create(configure));
        services.AddSingleton<ILogger>(static serviceProvider =>
            serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(ApplicationLoggerCategoryName));
        return services;
    }
}
