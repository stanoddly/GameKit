using GameKit.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GameKit.Logging;

public static class LoggingServiceCollectionExtensions
{
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

        services.AddSingleton<ILoggerFactory>(_ => LoggerFactory.Create(configure));
        return services;
    }

    public static ServiceCollection AddLogger<TCategory>(this ServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        if (services.IsRegistered<ILogger<TCategory>>())
        {
            return services;
        }

        services.AddSingleton<ILogger<TCategory>>(static serviceProvider =>
            serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<TCategory>());
        return services;
    }
}
