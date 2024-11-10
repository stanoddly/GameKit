using System.Collections.Immutable;
using System.Numerics;

namespace GameKit.Gpu;

public readonly struct Shape<TVertexType> where TVertexType : unmanaged, IVertexType
{
    internal readonly ImmutableArray<TVertexType> VertexTypes;

    public Shape(ImmutableArray<TVertexType> vertexTypes)
    {
        if (vertexTypes.IsDefaultOrEmpty)
        {
            throw new ArgumentException($"'{nameof(vertexTypes)}' cannot be empty or default.");
        }
        VertexTypes = vertexTypes;
    }

    public static implicit operator ReadOnlySpan<TVertexType>(Shape<TVertexType> shape)
    {
        return shape.VertexTypes.AsSpan();
    }
    
    public static implicit operator Shape<TVertexType>(ImmutableArray<TVertexType> immutableArray)
    {
        return new Shape<TVertexType>(immutableArray);
    }
}

public static class Shapes
{
    public static Shape<TVertexType> Reshape<TVertexType>(this Shape<TVertexType> shape, float size, Vector3 offset = default) where TVertexType : unmanaged, IVertexType, IPositionable
    {
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if (size == 1.0f && offset == default)
        {
            return shape;
        }

        ReadOnlySpan<TVertexType> span = shape;
        var builder = ImmutableArray.CreateBuilder<TVertexType>();

        foreach (TVertexType positionTextureNormalVertex in span)
        {
            builder.Add(positionTextureNormalVertex with { Position = positionTextureNormalVertex.Position * size + offset });
        }
        
        return new Shape<TVertexType>(builder.ToImmutable());
    }
    
    public static Shape<TTargetVertexType> Reshape<TSourceVertexType, TTargetVertexType>(this Shape<TSourceVertexType> shape, Func<ImmutableArray<TSourceVertexType>, ImmutableArray<TTargetVertexType>> reshapeFunction)
        where TSourceVertexType : unmanaged, IVertexType
        where TTargetVertexType : unmanaged, IVertexType
    {
        var result = reshapeFunction(shape.VertexTypes);
        
        return new Shape<TTargetVertexType>(result);
    }
}

public static class PositionTextureNormalShapes
{
    static PositionTextureNormalShapes()
    {
        ImmutableArray<PositionTextureNormalVertex> cube =
        [
            // Bottom Face (y = 0)
            new(new Vector3(-1.0f, -1.0f, -1.0f), new Vector3(0, -1, 0), new Vector2(0, 0)),
            new(new Vector3( 1.0f, -1.0f, -1.0f), new Vector3(0, -1, 0), new Vector2(1, 0)),
            new(new Vector3(-1.0f, -1.0f,  1.0f), new Vector3(0, -1, 0), new Vector2(0, 1)),
            new(new Vector3( 1.0f, -1.0f, -1.0f), new Vector3(0, -1, 0), new Vector2(1, 0)),
            new(new Vector3( 1.0f, -1.0f,  1.0f), new Vector3(0, -1, 0), new Vector2(1, 1)),
            new(new Vector3(-1.0f, -1.0f,  1.0f), new Vector3(0, -1, 0), new Vector2(0, 1)),

            // Top Face (y = 1)
            new(new Vector3(-1.0f,  1.0f, -1.0f), new Vector3(0, 1, 0), new Vector2(0, 0)),
            new(new Vector3(-1.0f,  1.0f,  1.0f), new Vector3(0, 1, 0), new Vector2(0, 1)),
            new(new Vector3( 1.0f,  1.0f, -1.0f), new Vector3(0, 1, 0), new Vector2(1, 0)),
            new(new Vector3( 1.0f,  1.0f, -1.0f), new Vector3(0, 1, 0), new Vector2(1, 0)),
            new(new Vector3(-1.0f,  1.0f,  1.0f), new Vector3(0, 1, 0), new Vector2(0, 1)),
            new(new Vector3( 1.0f,  1.0f,  1.0f), new Vector3(0, 1, 0), new Vector2(1, 1)),

            // Front Face
            new(new Vector3(-1.0f, -1.0f,  1.0f), new Vector3(0, 0, 1), new Vector2(0, 0)),
            new(new Vector3( 1.0f, -1.0f,  1.0f), new Vector3(0, 0, 1), new Vector2(1, 0)),
            new(new Vector3(-1.0f,  1.0f,  1.0f), new Vector3(0, 0, 1), new Vector2(0, 1)),
            new(new Vector3( 1.0f, -1.0f,  1.0f), new Vector3(0, 0, 1), new Vector2(1, 0)),
            new(new Vector3( 1.0f,  1.0f,  1.0f), new Vector3(0, 0, 1), new Vector2(1, 1)),
            new(new Vector3(-1.0f,  1.0f,  1.0f), new Vector3(0, 0, 1), new Vector2(0, 1)),

            // Back Face
            new(new Vector3(-1.0f, -1.0f, -1.0f), new Vector3(0, 0, -1), new Vector2(0, 1)),
            new(new Vector3(-1.0f,  1.0f, -1.0f), new Vector3(0, 0, -1), new Vector2(0, 0)),
            new(new Vector3( 1.0f, -1.0f, -1.0f), new Vector3(0, 0, -1), new Vector2(1, 1)),
            new(new Vector3( 1.0f, -1.0f, -1.0f), new Vector3(0, 0, -1), new Vector2(1, 1)),
            new(new Vector3(-1.0f,  1.0f, -1.0f), new Vector3(0, 0, -1), new Vector2(0, 0)),
            new(new Vector3( 1.0f,  1.0f, -1.0f), new Vector3(0, 0, -1), new Vector2(1, 0)),

            // Left Face
            new(new Vector3(-1.0f, -1.0f,  1.0f), new Vector3(-1, 0, 0), new Vector2(0, 1)),
            new(new Vector3(-1.0f,  1.0f, -1.0f), new Vector3(-1, 0, 0), new Vector2(1, 0)),
            new(new Vector3(-1.0f, -1.0f, -1.0f), new Vector3(-1, 0, 0), new Vector2(0, 0)),
            new(new Vector3(-1.0f, -1.0f,  1.0f), new Vector3(-1, 0, 0), new Vector2(0, 1)),
            new(new Vector3(-1.0f,  1.0f,  1.0f), new Vector3(-1, 0, 0), new Vector2(1, 1)),
            new(new Vector3(-1.0f,  1.0f, -1.0f), new Vector3(-1, 0, 0), new Vector2(1, 0)),

            // Right Face
            new(new Vector3( 1.0f, -1.0f,  1.0f), new Vector3(1, 0, 0), new Vector2(0, 1)),
            new(new Vector3( 1.0f, -1.0f, -1.0f), new Vector3(1, 0, 0), new Vector2(0, 0)),
            new(new Vector3( 1.0f,  1.0f, -1.0f), new Vector3(1, 0, 0), new Vector2(1, 0)),
            new(new Vector3( 1.0f, -1.0f,  1.0f), new Vector3(1, 0, 0), new Vector2(0, 1)),
            new(new Vector3( 1.0f,  1.0f, -1.0f), new Vector3(1, 0, 0), new Vector2(1, 0)),
            new(new Vector3( 1.0f,  1.0f,  1.0f), new Vector3(1, 0, 0), new Vector2(1, 1))
        ];
        Cube = cube;
        
        ImmutableArray<PositionTextureNormalVertex> horizontalQuad =
        [
            new(new Vector3(-1.0f, 1.0f, 0.0f), new Vector3(0, 0, 1), new Vector2(0, 0)),
            new(new Vector3(1.0f, 1.0f, 0.0f), new Vector3(0, 0, 1), new Vector2(1, 0)),
            new(new Vector3(-1.0f, -1.0f, 0.0f), new Vector3(0, 0, 1), new Vector2(0, 1)),
            new(new Vector3(1.0f, -1.0f, 0.0f), new Vector3(0, 0, 1), new Vector2(1, 1)),
        ];
        HorizontalQuad = horizontalQuad;
    
        ImmutableArray<PositionTextureNormalVertex> verticalQuad =
        [
            new(new Vector3(-1.0f, 0.0f, 1.0f), new Vector3(0, 1, 0), new Vector2(0, 0)),
            new(new Vector3(1.0f, 0.0f, 1.0f), new Vector3(0, 1, 0), new Vector2(1, 0)),
            new(new Vector3(-1.0f, 0.0f, -1.0f), new Vector3(0, 1, 0), new Vector2(0, 1)),
            new(new Vector3(1.0f, 0.0f, -1.0f), new Vector3(0, 1, 0), new Vector2(1, 1)),
        ];
        VerticalQuad = verticalQuad;
    }

    public static readonly Shape<PositionTextureNormalVertex> Cube;

    public static readonly Shape<PositionTextureNormalVertex> HorizontalQuad;

    public static readonly Shape<PositionTextureNormalVertex> VerticalQuad;
}

public static class PositionTextureShapes
{
    private static ImmutableArray<PositionTextureVertex> RecreateShape(ImmutableArray<PositionTextureNormalVertex> shape)
    {
        var builder = ImmutableArray.CreateBuilder<PositionTextureVertex>(shape.Length);
        foreach (PositionTextureNormalVertex positionTextureNormalVertex in shape)
        {
            builder.Add(new PositionTextureVertex(positionTextureNormalVertex.Position, positionTextureNormalVertex.TextureCoords));
        }
        return builder.MoveToImmutable();
    }

    static PositionTextureShapes()
    {
        Cube = PositionTextureNormalShapes.Cube.Reshape(RecreateShape);
        HorizontalQuad = PositionTextureNormalShapes.HorizontalQuad.Reshape(RecreateShape);
        VerticalQuad = PositionTextureNormalShapes.VerticalQuad.Reshape(RecreateShape);
    }

    public static readonly Shape<PositionTextureVertex> Cube;

    public static readonly Shape<PositionTextureVertex> HorizontalQuad;

    public static readonly Shape<PositionTextureVertex> VerticalQuad;
}