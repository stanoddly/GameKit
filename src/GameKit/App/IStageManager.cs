using GameKit.DependencyInjection;

namespace GameKit.App;

/// <summary>
/// Schedules transitions between application stages.
/// </summary>
/// <remarks>
/// A stage is a scoped composition of services built as a child of the root application provider.
/// Stage services can register views, update systems, render phases, and other disposable objects.
/// At most one stage is active at a time. Loading a stage replaces the active stage.
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
    /// use the last requested stage. When the load is applied, GameKit disposes the previous stage
    /// provider and the services owned by it before building the new stage.
    /// </remarks>
    void Load(Action<ServiceCollection> configure);
}
