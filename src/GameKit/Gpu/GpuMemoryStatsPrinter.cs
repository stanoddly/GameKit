namespace GameKit.Gpu;

public class GpuMemoryStatsPrinter : IStartable, IDisposable
{
    private readonly IGpuDevice _gpuDevice;
    private readonly TimeSpan _interval = TimeSpan.FromSeconds(5);
    private readonly CancellationTokenSource _cts = new();
    private readonly Thread _thread;
    private bool _disposed;

    public GpuMemoryStatsPrinter(IGpuDevice gpuDevice)
    {
        _gpuDevice = gpuDevice;
        _thread = new Thread(Run)
        {
            Name = "GpuMemoryStatsPrinter",
            IsBackground = true
        };
    }

    public void Start()
    {
        _thread.Start();
    }

    private void Run()
    {
        CancellationToken token = _cts.Token;

        while (!token.IsCancellationRequested)
        {
            GpuMemoryStats stats = _gpuDevice.MemoryStats;
            Console.WriteLine(
                $"[GPU Memory] Textures: {stats.TextureCount} ({FormatBytes(stats.TextureBytes)}) | " +
                $"VertexBuffers: {stats.VertexBufferCount} ({FormatBytes(stats.VertexBufferBytes)}) | " +
                $"StorageBuffers: {stats.StorageBufferCount} ({FormatBytes(stats.StorageBufferBytes)}) | " +
                $"Total: {FormatBytes(stats.TotalBytes)}");

            token.WaitHandle.WaitOne(_interval);
        }
    }

    private static string FormatBytes(long bytes)
    {
        return bytes switch
        {
            >= 1024 * 1024 * 1024 => $"{bytes / (1024.0 * 1024.0 * 1024.0):F2} GB",
            >= 1024 * 1024 => $"{bytes / (1024.0 * 1024.0):F2} MB",
            >= 1024 => $"{bytes / 1024.0:F2} KB",
            _ => $"{bytes} B"
        };
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _cts.Cancel();
        _thread.Join(TimeSpan.FromSeconds(1));
        _cts.Dispose();
    }
}
