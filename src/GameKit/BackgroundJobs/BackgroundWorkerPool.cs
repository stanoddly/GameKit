using GameKit.Gpu;

namespace GameKit.BackgroundJobs;

/// <summary>
/// Manages a pool of background worker threads for processing messages off the main thread.
/// Each worker has access to an <see cref="Gpu.ICopyPass"/> for GPU memory transfers.
/// Implements <see cref="IStartable"/> so workers start automatically before the game loop.
/// </summary>
public class BackgroundWorkerPool : IStartable, IDisposable
{
    private readonly BackgroundWorkHub _hub;
    private readonly List<HandlerRegistration> _registrations = [];
    private readonly IGpuDevice _gpuDevice;
    private readonly int _workerCount;
    private Thread[]? _workers;
    private CancellationTokenSource? _shutdownCts;
    private bool _disposed;

    internal BackgroundWorkerPool(BackgroundWorkHub hub, IGpuDevice gpuDevice)
        : this(hub, gpuDevice, Math.Max(1, Environment.ProcessorCount - 1))
    {
    }

    internal BackgroundWorkerPool(BackgroundWorkHub hub, IGpuDevice gpuDevice, int workerCount)
    {
        _hub = hub;
        _gpuDevice = gpuDevice;
        _workerCount = workerCount;
    }

    public void RegisterHandler<TMessage>(Func<BackgroundWorkHandler<TMessage>> factory)
        where TMessage : class
    {
        _registrations.Add(new HandlerRegistration<TMessage>(factory));
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
            BackgroundWorker worker = new BackgroundWorker(
                _hub,
                _registrations,
                _shutdownCts.Token,
                _gpuDevice);

            _workers[i] = new Thread(worker.Run)
            {
                Name = $"BackgroundWorker-{i}",
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
