namespace GameKit.Collections;

public interface IHandle<TSelf>
{
    uint Index { get; init; }
    uint Version { get; init; }
    static abstract TSelf Null { get; }
    bool IsNull();
}

public readonly struct Handle64<TType>: IHandle<Handle64<TType>>, IEquatable<Handle64<TType>>
{
    private readonly uint _index;
    private readonly uint _version;

    public uint Index
    {
        get => unchecked(_index - 1);
        init => _index = unchecked(value + 1);
    }

    public uint Version
    {
        get => _version;
        init => _version = value;
    }

    public static Handle64<TType> Null { get; } = default;

    public bool IsNull()
    {
        return _index == 0;
    }

    public bool Equals(Handle64<TType> other)
    {
        return _index == other._index && _version == other._version;
    }

    public override bool Equals(object? obj)
    {
        return obj is Handle64<TType> other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_index, _version);
    }
    
    public override string ToString() => IsNull() 
        ? $"Handle<{typeof(TType).Name}>.Null" 
        : $"Handle<{typeof(TType).Name}>{{ Index={Index}, Version={Version} }}";

    public static implicit operator bool(Handle64<TType> handle)
    {
        return handle._index != 0;
    }
}

public readonly struct Handle: IEquatable<Handle>
{
    private readonly int _index;

    public static Handle Null { get; } = default;

    private Handle(int num)
    {
        _index = num;
    }
    
    public bool IsNull()
    {
        return _index == 0;
    }

    public bool Equals(Handle other)
    {
        return _index == other._index;
    }

    public override bool Equals(object? obj)
    {
        return obj is Handle other && Equals(other);
    }

    public override int GetHashCode()
    {
        return _index.GetHashCode();
    }

    public static implicit operator int(Handle handle)
    {
        return unchecked(handle._index - 1);
    }
    
    public static implicit operator nuint(Handle handle)
    {
        return unchecked((nuint)(handle._index - 1));
    }
    
    public static explicit operator Handle(int number)
    {
        return unchecked(new Handle(number + 1));
    }
    
    public override string ToString() => IsNull() 
        ? $"{nameof(Handle)}.Null" 
        : $"{nameof(Handle)}({(nuint)this})";
    
    public static implicit operator bool(Handle handle)
    {
        return handle._index != 0;
    }
}

public class HandleNullException : Exception
{
    public HandleNullException()
    {
    }

    public HandleNullException(string? message) : base(message)
    {
    }

    public HandleNullException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}

public class HandleNotFoundException : Exception
{
    public HandleNotFoundException()
    {
    }

    public HandleNotFoundException(string? message) : base(message)
    {
    }

    public HandleNotFoundException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}