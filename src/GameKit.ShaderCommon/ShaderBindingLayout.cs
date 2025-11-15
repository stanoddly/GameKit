using System.Drawing;
using System.Runtime.InteropServices;

namespace GameKit.ShaderCommon;

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 8)]
public readonly record struct ShaderBindingLayout(ShaderBindingCounts BindingCounts, ShaderUniformSlotSizes UniformSlotSizes);

public record struct ShaderBindingCounts(byte NumSamplers, byte NumStorageTextures, byte NumStorageBuffers);

public record struct ShaderUniformSlotSizes(byte Slot0, byte Slot1, byte Slot2, byte Slot3);


public static class ShaderBindingLayoutExtension
{
    extension(ShaderBindingLayout layout)
    {
        public int NumSamplers => layout.BindingCounts.NumSamplers;
        public int NumStorageTextures => layout.BindingCounts.NumStorageTextures;
        public int NumStorageBuffers => layout.BindingCounts.NumStorageBuffers;
        
        public int NumUniformBuffers
        {
            get
            {
                if (layout.UniformSlotSizes.Slot3 != 0)
                {
                    return 4;
                }
                if (layout.UniformSlotSizes.Slot2 != 0)
                {
                    return 3;
                }
        
                if (layout.UniformSlotSizes.Slot1 != 0)
                {
                    return 2;
                }
        
                if (layout.UniformSlotSizes.Slot0 != 0)
                {
                    return 1;
                }

                return 0;
            }
        }
    }
}
