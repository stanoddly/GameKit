using System.Numerics;

namespace Pixely.Gpu;

public interface ICamera
{
    public Matrix4x4 ProjectionMatrix { get; }
    public Matrix4x4 ViewMatrix { get; }
}