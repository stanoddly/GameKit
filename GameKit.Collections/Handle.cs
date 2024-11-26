using System.Numerics;
using System.Runtime.CompilerServices;

namespace GameKit.Collections;

public interface IHandle<TSelf> where TSelf: unmanaged, IHandle<TSelf>
{
    static abstract explicit operator uint(TSelf self);
    static abstract explicit operator TSelf(uint integer);
}

public readonly struct Handle<TType>: IHandle<Handle<TType>>, IEqualityOperators<Handle<TType>, Handle<TType>, bool>, IEquatable<Handle<TType>>
{
    private readonly uint _id;

    private Handle(uint id) => _id = id;
    
    public bool IsNull() => _id == 0;
    
    public bool Equals(Handle<TType> other)
    {
        return _id == other._id;
    }

    public override bool Equals(object? obj)
    {
        return obj is Handle<TType> other && Equals(other);
    }

    public override int GetHashCode()
    {
        return (int)_id;
    }

    public static explicit operator uint(Handle<TType> self) => self._id;

    public static explicit operator Handle<TType>(uint id) => new(id);

    public static explicit operator int(Handle<TType> self) => (int)self._id;
    
    public static explicit operator Handle<TType>(int id) => new((uint)id);
    
    public static implicit operator bool(Handle<TType> self) => self._id != 0;

    public static readonly Handle<TType> Null = new Handle<TType>(0);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ThrowIfNull(string message)
    {
        if (_id == 0)
        {
            throw new HandleNullException(message);
        }
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ThrowIfNull()
    {
        if (_id == 0)
        {
            throw new HandleNullException();
        }
    }

    public static bool operator ==(Handle<TType> left, Handle<TType> right)
    {
        return left._id == right._id;
    }

    public static bool operator !=(Handle<TType> left, Handle<TType> right)
    {
        return left._id != right._id;
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