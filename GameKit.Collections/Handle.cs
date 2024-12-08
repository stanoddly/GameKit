using System.Numerics;
using System.Runtime.CompilerServices;

namespace GameKit.Collections;

public interface IHandle<TSelf> where TSelf: unmanaged, IHandle<TSelf>
{
    static abstract implicit operator uint(TSelf self);
    static abstract implicit operator nuint(TSelf self);
    static abstract explicit operator TSelf(uint integer);
}

/// <summary>
/// Interface for a versioned handle.  
/// </summary>
/// <typeparam name="TSelf">Type to self.</typeparam>
public interface IVersionedHandle<TSelf> where TSelf: struct, IVersionedHandle<TSelf>
{
    uint Index { get; }
    uint Version { get; }
    static abstract TSelf TombStone { get; }
    bool IsTombStone();
    TSelf WithIndex(uint index);
    TSelf WithVersion(uint version);
    TSelf RotateVersion();
}

public readonly struct VersionedHandle<TType> : IVersionedHandle<VersionedHandle<TType>>, IEquatable<VersionedHandle<TType>>
{
    private readonly uint _index;
    private readonly uint _version;

    private VersionedHandle(uint index, uint version)
    {
        _index = index;
        _version = version;
    }

    public uint Index => _index;
    public uint Version => _version;
    
    public nuint NativeIndex => _index;
    public nuint NativeVersion => _version;

    public static VersionedHandle<TType> TombStone => new VersionedHandle<TType>(uint.MaxValue, uint.MaxValue);

    public bool IsTombStone() => _index == uint.MaxValue && _version == uint.MaxValue;

    public VersionedHandle<TType> WithIndex(uint index) => new VersionedHandle<TType>(index, _version);

    public VersionedHandle<TType> WithVersion(uint version) => new VersionedHandle<TType>(_index, version);

    public VersionedHandle<TType> RotateVersion() => new VersionedHandle<TType>(_index, _version + 1);

    public override string ToString() => IsTombStone() 
        ? "Handle(TombStone)" 
        : $"Handle(Index={Index}, Version={Version})";

    public override bool Equals(object obj) => 
        obj is VersionedHandle<TType> other && _index == other._index && _version == other._version;

    public override int GetHashCode() => 
        HashCode.Combine(_index, _version);

    public static bool operator ==(VersionedHandle<TType> left, VersionedHandle<TType> right) => 
        left._index == right._index && left._version == right._version;

    public static bool operator !=(VersionedHandle<TType> left, VersionedHandle<TType> right) => 
        !(left == right);

    public bool Equals(VersionedHandle<TType> other)
    {
        return _index == other._index && _version == other._version;
    }
}

public readonly struct Handle : IVersionedHandle<Handle>, IEquatable<Handle>
{
    private readonly uint _index;
    private readonly uint _version;

    public Handle(uint index, uint version)
    {
        _index = index;
        _version = version;
    }

    public uint Index
    {
        get => _index;
        init => _index = value;
    }

    //public uint Index => _index;
    public uint Version
    {
        get => _version;
        init => _version = value;
    }
    
    public nuint NativeIndex => _index;
    public nuint NativeVersion => _version;

    public static Handle TombStone => new Handle(uint.MaxValue, uint.MaxValue);

    public bool IsTombStone() => _index == uint.MaxValue && _version == uint.MaxValue;

    public Handle WithIndex(uint index) => new Handle(index, _version);

    public Handle WithVersion(uint version) => new Handle(_index, version);

    public Handle RotateVersion() => new Handle(_index, _version + 1);

    public override string ToString() => IsTombStone() 
        ? "Handle(TombStone)" 
        : $"Handle(Index={Index}, Version={Version})";

    public override bool Equals(object? obj) => 
        obj is Handle other && _index == other._index && _version == other._version;

    public override int GetHashCode() => 
        HashCode.Combine(_index, _version);

    public static bool operator ==(Handle left, Handle right) => 
        left._index == right._index && left._version == right._version;

    public static bool operator !=(Handle left, Handle right) => 
        !(left == right);

    public bool Equals(Handle other)
    {
        return _index == other._index && _version == other._version;
    }
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

    public static implicit operator uint(Handle<TType> self) => self._id;
    
    public static implicit operator nuint(Handle<TType> self) => self._id;
    
    public static implicit operator int(Handle<TType> self) => (int)self._id;

    public static explicit operator Handle<TType>(uint id) => new(id);
    
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