
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

    public static readonly VertexTypeId Null = new(-1);
}

public class VertexTypeIdMap : TypeIdMap<VertexTypeIdMap>;

public class VertexTypeId<T> : TypeIdMap<VertexTypeIdMap, T> where T : IVertexType
{
    public static readonly VertexTypeId Value = new(Id);
}
