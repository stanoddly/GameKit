using GameKit.Gpu;

namespace GameKit.BackgroundJobs;

/// <summary>
/// Manages a pool of background worker threads for processing jobs off the main thread.
/// Each worker has access to an <see cref="Gpu.ICopyPass"/> for GPU memory transfers.
/// Implements <see cref="IStartable"/> so workers start automatically before the game loop.
/// </summary>
public class BackgroundJobWorkerPool : IStartable, IDisposable
{
    private readonly BackgroundJobQueues _queues;
    private readonly List<ProcessorRegistration> _registrations = [];
    private readonly IGpuDevice _gpuDevice;
    private readonly int _workerCount;
    private Thread[]? _workers;
    private CancellationTokenSource? _shutdownCts;
    private bool _disposed;

    internal BackgroundJobQueues Queues => _queues;

    public BackgroundJobWorkerPool(IGpuDevice gpuDevice, int priorityLevels = 1)
        : this(gpuDevice, priorityLevels, Math.Max(1, Environment.ProcessorCount - 1))
    {
    }

    public BackgroundJobWorkerPool(IGpuDevice gpuDevice, int priorityLevels, int workerCount)
    {
        _gpuDevice = gpuDevice;
        _workerCount = workerCount;
        _queues = new BackgroundJobQueues(priorityLevels);
    }

    public void RegisterProcessor<TTask>(Func<BackgroundTaskProcessor<TTask>> factory)
        where TTask : class
    {
        _registrations.Add(new ProcessorRegistration<TTask>(factory));
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
                _queues,
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
