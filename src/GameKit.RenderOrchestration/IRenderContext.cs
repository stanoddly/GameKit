using GameKit.Gpu;

namespace GameKit.RenderOrchestration;

public interface IRenderContext : IDisposable
{
    CommandBuffer CommandBuffer { get; }
}
