using System.Runtime.CompilerServices;

namespace GameKit.BackgroundJobs;

internal abstract class ProcessorWrapper
{
    public abstract void Process(object task, IBackgroundJobContext context);
}

internal class ProcessorWrapper<TTask> : ProcessorWrapper
    where TTask : class
{
    private readonly BackgroundTaskProcessor<TTask> _processor;

    public ProcessorWrapper(BackgroundTaskProcessor<TTask> processor)
    {
        _processor = processor;
    }

    // Unsafe.As is safe here: jobs are dispatched with TypeId matching this processor's TaskTypeId.
    public override void Process(object task, IBackgroundJobContext context)
    {
        _processor.Process(Unsafe.As<TTask>(task), context);
    }
}

internal abstract class ProcessorRegistration
{
    public abstract int TaskTypeId { get; }
    public abstract ProcessorWrapper CreateWrapper();
}

internal class ProcessorRegistration<TTask> : ProcessorRegistration
    where TTask : class
{
    private readonly Func<BackgroundTaskProcessor<TTask>> _factory;

    public override int TaskTypeId => BackgroundJobTypeId<TTask>.Id;

    public ProcessorRegistration(Func<BackgroundTaskProcessor<TTask>> factory)
    {
        _factory = factory;
    }

    public override ProcessorWrapper CreateWrapper()
    {
        return new ProcessorWrapper<TTask>(_factory());
    }
}
