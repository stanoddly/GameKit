namespace GameKit.Architecture;

/// <summary>
/// Runs once after a top-level command batch completes (dispatch depth 1), in registration order.
/// Re-entrant commands dispatched by a handler do not trigger it again until the outermost dispatch returns.
/// </summary>
public interface IPostDispatchHook
{
    void OnBatchCompleted();
}
