namespace GameKit.RenderOrchestration;

internal class WindowTypeId : StaticTypeIdMap<WindowTypeId>;

internal class WindowTypeId<TRenderContext> : StaticTypeIdMap<WindowTypeId, TRenderContext>
    where TRenderContext : IRenderContext;
