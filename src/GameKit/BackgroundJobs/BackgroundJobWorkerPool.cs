using System.Collections.Concurrent;
using GameKit.Gpu;

namespace GameKit.BackgroundJobs;

/// <summary>
/// Manages a pool of background worker threads for processing jobs off the main thread.
/// Each worker has access to an <see cref="Gpu.ICopyPass"/> for GPU memory transfers.
/// Implements <see cref="IStartable"/> so workers start automatically before the game loop.
/// </summary>
public class BackgroundJobWorkerPool : IStartable, IDisposable
{
    private readonly ConcurrentQueue<BackgroundJob>[] _priorityQueues;
    private readonly ConcurrentQueue<BackgroundJobResult> _resultQueue = new();
    private readonly List<ProcessorRegistration> _registrations = [];
    private readonly IGpuDevice _gpuDevice;
    private readonly int _workerCount;
    private Thread[]? _workers;
    private CancellationTokenSource? _shutdownCts;
    private bool _disposed;

    public BackgroundJobWorkerPool(IGpuDevice gpuDevice, int priorityLevels = 1)
        : this(gpuDevice, priorityLevels, Math.Max(1, Environment.ProcessorCount - 1))
    {
    }

    public BackgroundJobWorkerPool(IGpuDevice gpuDevice, int priorityLevels, int workerCount)
    {
        _gpuDevice = gpuDevice;
        _workerCount = workerCount;

        _priorityQueues = new ConcurrentQueue<BackgroundJob>[priorityLevels];
        for (int i = 0; i < priorityLevels; i++)
        {
            _priorityQueues[i] = new ConcurrentQueue<BackgroundJob>();
        }
    }

    public void RegisterProcessor<TTask, TResult>(Func<BackgroundTaskProcessor<TTask, TResult>> factory)
        where TTask : class
        where TResult : class
    {
        _registrations.Add(new ProcessorRegistration<TTask, TResult>(factory));
    }

    public void Start()
    {
        if (_workers != null)
        {
            throw new InvalidOperationException("Worker pool already started");
        }

        _shutdownCts = new CancellationTokenSource();
        _workers = new Thread[_workerCount];

        for (int i = 0; i < _workerCount; i++)
        {
            BackgroundJobWorker worker = new BackgroundJobWorker(
                _priorityQueues,
                _resultQueue,
                _registrations,
                _shutdownCts.Token,
                _gpuDevice);

            _workers[i] = new Thread(worker.Run)
            {
                Name = $"BackgroundJobWorker-{i}",
                IsBackground = true
            };
            _workers[i].Start();
        }
    }

    internal void Enqueue(BackgroundJob job, int priority = 0)
    {
        int clampedPriority = Math.Clamp(priority, 0, _priorityQueues.Length - 1);
        _priorityQueues[clampedPriority].Enqueue(job);
    }

    internal bool TryDequeueResult(out BackgroundJobResult result)
    {
        return _resultQueue.TryDequeue(out result);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        if (_shutdownCts != null)
        {
            _shutdownCts.Cancel();

            if (_workers != null)
            {
                foreach (Thread worker in _workers)
                {
                    worker.Join(TimeSpan.FromSeconds(1));
                }
            }

            _shutdownCts.Dispose();
        }
    }
}
