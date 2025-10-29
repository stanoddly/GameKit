using System.Numerics;

namespace GameKit.Gpu;

public interface ICamera
{
    public Matrix4x4 ProjectionMatrix { get; }
    public Matrix4x4 ViewMatrix { get; }
}