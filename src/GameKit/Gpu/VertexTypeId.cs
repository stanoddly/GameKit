namespace GameKit.Gpu;

public readonly struct VertexTypeId : IEquatable<VertexTypeId>
{
    private readonly int _id;

    internal VertexTypeId(int id)
    {
        _id = id;
    }
    
    public bool Equals(VertexTypeId other)
    {
        return _id == other._id;
    }

    public override bool Equals(object? obj)
    {
        return obj is VertexTypeId other && Equals(other);
    }

    public override int GetHashCode()
    {
        return _id;
    }

    public static bool operator ==(VertexTypeId left, VertexTypeId right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(VertexTypeId left, VertexTypeId right)
    {
        return !left.Equals(right);
    }

    internal static int NextId = 0;
    public static readonly VertexTypeId Null = default;
}

public static class VertexTypeId<T> where T: IVertexType
{
    public static readonly VertexTypeId Value;
    public static readonly string Name;

    static VertexTypeId()
    {
        Value = new VertexTypeId(++VertexTypeId.NextId);

        Name = typeof(T).Name;
    }
}
