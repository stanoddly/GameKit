using GameKit.DependencyInjection;

namespace GameKit.App;

/// <summary>
/// Schedules transitions between application stages.
/// </summary>
/// <remarks>
/// A stage is a scoped composition of services built as a child of the root application provider.
/// Stage services can register views, update systems, render phases, and other disposable objects.
/// Only one stage is active at a time.
/// </remarks>
public interface IStageManager
{
    /// <summary>
    /// Schedules a stage to become active at the next stage transition point.
    /// </summary>
    /// <param name="configure">Configures the services owned by the next stage.</param>
    /// <remarks>
    /// This method does not build the stage immediately, so it is safe to call while views, update
    /// systems, or render phases are being iterated. GameKit applies pending stage transitions at the
    /// beginning of a frame, after frame timing starts and before events, updates, or rendering. If
    /// this method is called before <see cref="IGameKitApp.Run"/>, the stage is applied during the first
    /// loop iteration before the first rendered frame. Multiple calls before the next transition point
    /// use the last requested stage. Calling this method after <see cref="Unload"/> but before that
    /// transition point cancels the pending unload.
    /// </remarks>
    void Load(Action<ServiceCollection> configure);

    /// <summary>
    /// Schedules the active stage to be unloaded at the next stage transition point.
    /// </summary>
    /// <remarks>
    /// This method does not dispose the active stage immediately, so it is safe to call while views,
    /// update systems, or render phases are being iterated. GameKit applies pending stage transitions
    /// at the beginning of a frame, after frame timing starts and before events, updates, or rendering.
    /// Calling this method after <see cref="Load"/> but before that transition point cancels the pending
    /// load. When the unload is applied, GameKit disposes the active stage provider and the services
    /// owned by it.
    /// </remarks>
    void Unload();
}
