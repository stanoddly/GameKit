using System.Diagnostics.CodeAnalysis;

namespace GameKit.DependencyInjection;

/// <summary>
/// Invoked after a singleton service is constructed (or, for pre-constructed instances, when the provider is built).
/// The callback fires for every activation regardless of how the service was registered —
/// constructor-injection, delegate factory, typed factory, or pre-constructed instance.
/// </summary>
/// <param name="instance">The newly activated service instance.</param>
/// <param name="type">
/// The concrete implementation type of the instance. Carries
/// <see cref="DynamicallyAccessedMemberTypes.Interfaces"/> so the trimmer preserves interface
/// metadata when the type flows from a <c>typeof(T)</c> expression in generator-emitted code.
/// For pre-constructed instances and non-intercepted typed-factory registrations the annotation
/// is preserved on a best-effort basis via the stored <see cref="Type"/> value.
/// </param>
/// <param name="provider">The <see cref="ServiceProvider"/> that owns the instance.</param>
public delegate void ServiceActivationCallback(
    object instance,
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] Type type,
    ServiceProvider provider);

/// <summary>
/// Invoked during <see cref="ServiceProvider.Dispose"/> for each singleton service, immediately before
/// that service's own <see cref="IDisposable.Dispose"/> call (if the service implements <see cref="IDisposable"/>).
/// Services are visited in reverse creation order, matching the order used for <see cref="IDisposable"/> disposal.
/// </summary>
/// <param name="instance">The service instance being disposed.</param>
/// <param name="type">
/// The concrete implementation type of the instance. Carries
/// <see cref="DynamicallyAccessedMemberTypes.Interfaces"/> so the trimmer preserves interface
/// metadata when the type flows from a <c>typeof(T)</c> expression in generator-emitted code.
/// </param>
/// <param name="provider">The <see cref="ServiceProvider"/> being disposed.</param>
public delegate void ServiceDisposalCallback(
    object instance,
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] Type type,
    ServiceProvider provider);
