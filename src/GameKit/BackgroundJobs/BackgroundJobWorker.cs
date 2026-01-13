using GameKit.Gpu;

namespace GameKit.BackgroundJobs;

internal class BackgroundJobWorker
{
    private readonly BackgroundJobQueues _queues;
    private readonly List<ProcessorRegistration> _registrations;
    private readonly CancellationToken _cancellationToken;
    private readonly IGpuDevice _gpuDevice;

    private List<ProcessorWrapper?> _processors = null!;

    public BackgroundJobWorker(
        BackgroundJobQueues queues,
        List<ProcessorRegistration> registrations,
        CancellationToken cancellationToken,
        IGpuDevice gpuDevice)
    {
        _queues = queues;
        _registrations = registrations;
        _cancellationToken = cancellationToken;
        _gpuDevice = gpuDevice;
    }

    public void Run()
    {
        _processors = CreateProcessorLookup();

        while (!_cancellationToken.IsCancellationRequested)
        {
            if (_queues.TryDequeueJob(out BackgroundJob job))
            {
                ProcessJob(job);
            }
            else
            {
                Thread.Sleep(1);
            }
        }
    }

    private List<ProcessorWrapper?> CreateProcessorLookup()
    {
        List<ProcessorWrapper?> processors = [null];

        foreach (ProcessorRegistration registration in _registrations)
        {
            while (processors.Count <= registration.TaskTypeId)
            {
                processors.Add(null);
            }

            processors[registration.TaskTypeId] = registration.CreateWrapper();
        }

        return processors;
    }

    private void ProcessJob(BackgroundJob job)
    {
        ProcessorWrapper? processor = job.TypeId < _processors.Count ? _processors[job.TypeId] : null;
        if (processor == null)
        {
            return;
        }

        using CommandBuffer commandBuffer = _gpuDevice.AcquireCommandBuffer();

        using (ICopyPass copyPass = commandBuffer.CreateCopyPass())
        {
            BackgroundJobContext context = new(copyPass, _queues);
            processor.Process(job.Task, context);
        }

        using GpuFence fence = commandBuffer.SubmitAndAcquireFence();
        _gpuDevice.WaitForFences([fence]);
    }
}
