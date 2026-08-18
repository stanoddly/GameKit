using Pixely.Gpu;

namespace Pixely.RenderOrchestration;

public interface IRenderContext : IDisposable
{
    CommandBuffer CommandBuffer { get; }
    Texture ColorTarget { get; }
}
