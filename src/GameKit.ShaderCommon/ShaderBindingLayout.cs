using System.Drawing;
using System.Runtime.InteropServices;

namespace GameKit.ShaderCommon;

public readonly record struct ShaderBindingLayout(
    ShaderBindingCounts BindingCounts,
    ShaderUniformSlotSizes UniformSlotSizes)
{
    public int NumSamplers() => BindingCounts.NumSamplers;
    public int NumStorageTextures() => BindingCounts.NumStorageTextures;
    public int NumStorageBuffers() => BindingCounts.NumStorageBuffers;
        
    public int NumUniformBuffers()
    {
        if (UniformSlotSizes.Slot3 != 0)
        {
            return 4;
        }
        if (UniformSlotSizes.Slot2 != 0)
        {
            return 3;
        }
        
        if (UniformSlotSizes.Slot1 != 0)
        {
            return 2;
        }
        
        if (UniformSlotSizes.Slot0 != 0)
        {
            return 1;
        }

        return 0;
    }
}

public record struct ShaderBindingCounts(byte NumSamplers, byte NumStorageTextures, byte NumStorageBuffers, byte NumReadWriteStorageTextures = 0, byte NumReadWriteStorageBuffers = 0);

public record struct ShaderUniformSlotSizes(byte Slot0, byte Slot1, byte Slot2, byte Slot3);

/// <summary>
/// Exception thrown when shader binding layout validation fails.
/// </summary>
public class ShaderBindingLayoutValidationException(string message) : Exception(message);

public static class ShaderBindingLayoutValidator
{
    /// <summary>
    /// Validates that expected binding counts do not exceed real/actual binding counts.
    /// </summary>
    /// <param name="expectedCounts">The expected shader binding counts</param>
    /// <param name="realCounts">The actual/real shader binding counts from the shader</param>
    /// <exception cref="ShaderBindingLayoutValidationException">Thrown when expected counts exceed real counts</exception>
    public static void ValidateBindingCounts(ShaderBindingCounts expectedCounts, ShaderBindingCounts realCounts)
    {
        if (expectedCounts.NumSamplers > realCounts.NumSamplers)
            throw new ShaderBindingLayoutValidationException(
                $"Expected samplers ({expectedCounts.NumSamplers}) exceeds real samplers ({realCounts.NumSamplers})");

        if (expectedCounts.NumStorageTextures > realCounts.NumStorageTextures)
            throw new ShaderBindingLayoutValidationException(
                $"Expected storage textures ({expectedCounts.NumStorageTextures}) exceeds real storage textures ({realCounts.NumStorageTextures})");

        if (expectedCounts.NumStorageBuffers > realCounts.NumStorageBuffers)
            throw new ShaderBindingLayoutValidationException(
                $"Expected storage buffers ({expectedCounts.NumStorageBuffers}) exceeds real storage buffers ({realCounts.NumStorageBuffers})");
    }

    /// <summary>
    /// Validates uniform slot sizes by comparing expected sizes against real/actual sizes.
    /// Real slot sizes must match expected sizes exactly, unless the expected size is 0 (in which case real can be any value).
    /// </summary>
    /// <param name="expectedSizes">The expected uniform slot sizes</param>
    /// <param name="realSizes">The actual/real uniform slot sizes from the shader</param>
    /// <exception cref="ShaderBindingLayoutValidationException">Thrown when slot sizes don't match the validation rules</exception>
    public static void ValidateUniformSlotSizes(ShaderUniformSlotSizes expectedSizes, ShaderUniformSlotSizes realSizes)
    {
        ValidateSlot(0, expectedSizes.Slot0, realSizes.Slot0);
        ValidateSlot(1, expectedSizes.Slot1, realSizes.Slot1);
        ValidateSlot(2, expectedSizes.Slot2, realSizes.Slot2);
        ValidateSlot(3, expectedSizes.Slot3, realSizes.Slot3);
    }

    private static void ValidateSlot(int slotIndex, byte expectedSize, byte realSize)
    {
        // If expected is 0, real can be any value, otherwise it must match
        if (expectedSize != 0 && expectedSize != realSize)
        {
            throw new ShaderBindingLayoutValidationException(
                $"Slot{slotIndex} size mismatch: expected {expectedSize} but got {realSize}");
        }
    }
}

