namespace GameKit.Gpu;

public class PerformanceTracker : IUpdatable, IDisposable
{
    private readonly IGpuDevice _gpuDevice;
    private readonly FrameContext _frameContext;
    private long _frameCount;
    private double _totalFrameTime;
    private double _minFrameTime = double.MaxValue;
    private double _maxFrameTime;
    private GpuMemoryStats _peakMemoryStats;

    public PerformanceTracker(IGpuDevice gpuDevice, FrameContext frameContext)
    {
        _gpuDevice = gpuDevice;
        _frameContext = frameContext;
    }

    public void Update()
    {
        double frameTime = _frameContext.TimeDelta64;

        if (frameTime > 0)
        {
            _frameCount++;
            _totalFrameTime += frameTime;

            if (frameTime < _minFrameTime)
            {
                _minFrameTime = frameTime;
            }

            if (frameTime > _maxFrameTime)
            {
                _maxFrameTime = frameTime;
            }
        }

        GpuMemoryStats stats = _gpuDevice.MemoryStats;

        if (stats.TotalBytes > _peakMemoryStats.TotalBytes)
        {
            _peakMemoryStats = stats;
        }
    }

    public void Dispose()
    {
        PrintSummary();
    }

    private void PrintSummary()
    {
        Console.WriteLine("=== Performance Summary ===");

        if (_frameCount > 0)
        {
            double averageFrameTime = _totalFrameTime / _frameCount;
            double averageFps = 1.0 / averageFrameTime;

            Console.WriteLine($"Frames: {_frameCount}");
            Console.WriteLine($"Average frame time: {averageFrameTime * 1000:F2} ms ({averageFps:F1} FPS)");
            Console.WriteLine($"Min frame time: {_minFrameTime * 1000:F2} ms ({1.0 / _minFrameTime:F1} FPS)");
            Console.WriteLine($"Max frame time: {_maxFrameTime * 1000:F2} ms ({1.0 / _maxFrameTime:F1} FPS)");
        }
        else
        {
            Console.WriteLine("No frames recorded.");
        }

        GpuMemoryStats final = _gpuDevice.MemoryStats;

        Console.WriteLine("--- GPU Memory (Final) ---");
        PrintMemoryStats(final);

        Console.WriteLine("--- GPU Memory (Peak) ---");
        PrintMemoryStats(_peakMemoryStats);

        Console.WriteLine("===========================");
    }

    private static void PrintMemoryStats(GpuMemoryStats stats)
    {
        Console.WriteLine(
            $"  Textures: {stats.TextureCount} ({FormatBytes(stats.TextureBytes)}) | " +
            $"VertexBuffers: {stats.VertexBufferCount} ({FormatBytes(stats.VertexBufferBytes)}) | " +
            $"StorageBuffers: {stats.StorageBufferCount} ({FormatBytes(stats.StorageBufferBytes)}) | " +
            $"Total: {FormatBytes(stats.TotalBytes)}");
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
}
