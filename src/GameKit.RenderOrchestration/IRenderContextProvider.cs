using System.Diagnostics.CodeAnalysis;

namespace GameKit.RenderOrchestration;

public interface IRenderContextProvider<TRenderContext> where TRenderContext: IDisposable
{
    public bool TryProvide([NotNullWhen(true)] out TRenderContext? renderContext);
}