using System.Runtime.CompilerServices;
using Pixely.Collections;

namespace Pixely.Gpu;

public class CommandBufferValidationContext
{
    private FastListStruct<byte> _vertexUniformValidationSlots = new();
    private FastListStruct<byte> _fragmentUniformValidationSlots = new();
    
    public void AssignFragmentUniformDataSlot<TType>(uint slot)
    {
        _fragmentUniformValidationSlots.ResizeFill((int)slot, 0);
        _fragmentUniformValidationSlots[slot] = (byte)Unsafe.SizeOf<TType>();
    }
    
    public void AssignVertexUniformDataSlot<TType>(uint slot)
    {
        _vertexUniformValidationSlots.ResizeFill((int)slot, 0);
        _vertexUniformValidationSlots[slot] = (byte)Unsafe.SizeOf<TType>();
    }
}

public class RenderPassValidationContext
{
    private GraphicsPipeline? _currentGraphicsPipeline = null;
    private Type? _currentVertexType = null;

    public void AssignVertexType<TVertexType>() where TVertexType: unmanaged, IVertexType
    {
        _currentVertexType = typeof(TVertexType);
    }

    public void AssignGraphicsPipeline(GraphicsPipeline graphicsPipeline)
    {
        _currentGraphicsPipeline = graphicsPipeline;
    }

    public void Validate()
    {
        
    }
}