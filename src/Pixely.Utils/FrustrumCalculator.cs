using System.Numerics;

namespace Pixely.Utils;

public readonly record struct FrustumBounds(float MinX, float MinY, float MinZ, float MaxX, float MaxY, float MaxZ);

public class FrustumCalculator
{
    private static readonly Vector4[] NdcCorners =
    [
        new(-1, -1, -1, 1),
        new( 1, -1, -1, 1),
        new(-1,  1, -1, 1),
        new( 1,  1, -1, 1),
        new(-1, -1,  1, 1),
        new( 1, -1,  1, 1),
        new(-1,  1,  1, 1),
        new( 1,  1,  1, 1)
    ];

    public static bool DetermineFrustumBounds(Matrix4x4 viewProjectionMatrix, out FrustumBounds result)
    {
        if (!Matrix4x4.Invert(viewProjectionMatrix, out Matrix4x4 invViewProj))
        {
            result = default;
            return false;
        }
        Span<Vector3> corners = stackalloc Vector3[8];
        
        for(int i = 0; i < 8; i++)
        {
            Vector4 worldPos = Vector4.Transform(NdcCorners[i], invViewProj);
            corners[i] = new Vector3(worldPos.X / worldPos.W, worldPos.Y / worldPos.W, worldPos.Z / worldPos.W);
        }

        float minX = float.MaxValue;
        float maxX = float.MinValue;
        float minY = float.MaxValue;
        float maxY = float.MinValue;
        float minZ = float.MaxValue;
        float maxZ = float.MinValue;

        foreach (Vector3 worldCorner in corners)
        {
            minX = MathF.Min(minX, worldCorner.X);
            maxX = MathF.Max(maxX, worldCorner.X);
            minY = MathF.Min(minY, worldCorner.Y);
            maxY = MathF.Max(maxY, worldCorner.Y);
            minZ = MathF.Min(minZ, worldCorner.Z);
            maxZ = MathF.Max(maxZ, worldCorner.Z);
        }

        result = new FrustumBounds(minX, minY, minZ, maxX, maxY, maxZ);
        return true;
    }
}