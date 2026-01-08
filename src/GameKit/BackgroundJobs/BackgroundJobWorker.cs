using System.Collections.Concurrent;
using GameKit.Gpu;

namespace GameKit.BackgroundJobs;

internal class BackgroundJobWorker
{
    private readonly ConcurrentQueue<BackgroundJob>[] _priorityQueues;
    private readonly ConcurrentQueue<BackgroundJobResult> _resultQueue;
    private readonly List<ProcessorRegistration> _registrations;
    private readonly CancellationToken _cancellationToken;
    private readonly IGpuDevice _gpuDevice;

    private List<ProcessorWrapper?> _processors = null!;

    public BackgroundJobWorker(
        ConcurrentQueue<BackgroundJob>[] priorityQueues,
        ConcurrentQueue<BackgroundJobResult> resultQueue,
        List<ProcessorRegistration> registrations,
        CancellationToken cancellationToken,
        IGpuDevice gpuDevice)
    {
        _priorityQueues = priorityQueues;
        _resultQueue = resultQueue;
        _registrations = registrations;
        _cancellationToken = cancellationToken;
        _gpuDevice = gpuDevice;
    }

    public void Run()
    {
        _processors = CreateProcessorLookup();

        while (!_cancellationToken.IsCancellationRequested)
        {
            if (TryDequeueJob(out BackgroundJob job))
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

        object? result;
        using (ICopyPass copyPass = commandBuffer.CreateCopyPass())
        {
            result = processor.Process(job.Task, copyPass);
        }

        using GpuFence fence = commandBuffer.SubmitAndAcquireFence();
        _gpuDevice.WaitForFences([fence]);

        if (result != null)
        {
            _resultQueue.Enqueue(new BackgroundJobResult(processor.ResultTypeId, result));
        }
    }

    private bool TryDequeueJob(out BackgroundJob job)
    {
        foreach (ConcurrentQueue<BackgroundJob> queue in _priorityQueues)
        {
            if (queue.TryDequeue(out job))
            {
                return true;
            }
        }

        job = default;
        return false;
    }
}
