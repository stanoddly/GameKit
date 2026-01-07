using System.Runtime.CompilerServices;
using GameKit.Gpu;

namespace GameKit.BackgroundJobs;

internal abstract class ProcessorWrapper
{
    public abstract int ResultTypeId { get; }
    public abstract object? Process(object task, ICopyPass copyPass);
}

internal class ProcessorWrapper<TTask, TResult> : ProcessorWrapper
    where TTask : class
    where TResult : class
{
    private readonly BackgroundTaskProcessor<TTask, TResult> _processor;

    public override int ResultTypeId => BackgroundJobTypeId<TResult>.Id;

    public ProcessorWrapper(BackgroundTaskProcessor<TTask, TResult> processor)
    {
        _processor = processor;
    }

    // Unsafe.As is safe here: jobs are dispatched with TypeId matching this processor's TaskTypeId.
    public override object? Process(object task, ICopyPass copyPass)
    {
        return _processor.Process(Unsafe.As<TTask>(task), copyPass);
    }
}

internal abstract class ProcessorRegistration
{
    public abstract int TaskTypeId { get; }
    public abstract ProcessorWrapper CreateWrapper();
}

internal class ProcessorRegistration<TTask, TResult> : ProcessorRegistration
    where TTask : class
    where TResult : class
{
    private readonly Func<BackgroundTaskProcessor<TTask, TResult>> _factory;

    public override int TaskTypeId => BackgroundJobTypeId<TTask>.Id;

    public ProcessorRegistration(Func<BackgroundTaskProcessor<TTask, TResult>> factory)
    {
        _factory = factory;
    }

    public override ProcessorWrapper CreateWrapper()
    {
        return new ProcessorWrapper<TTask, TResult>(_factory());
    }
}
