namespace Pixely.Architecture;

/// <summary>
/// Runs once after a top-level command batch is handled — inside the dispatch call, before it returns — in
/// registration order. Re-entrant commands dispatched by a handler share the batch and do not trigger it again.
/// </summary>
public interface ICommandDispatchHook
{
    void OnBatchCompleted();
}
