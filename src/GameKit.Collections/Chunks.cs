using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace GameKit.Collections;
[SkipLocalsInit]
public ref struct Chunks<TTarget,TSource1> 
    where TTarget : struct
{
    private Span<TTarget> _target;
    private readonly ReadOnlySpan<TSource1> _source1;
    private int _position;
    private int _currentChunkSize;

    public Chunks(Span<TTarget> target, ReadOnlySpan<TSource1> source1)
    {
        // TODO: assert target

        _target = target;
        _source1 = source1;
        _position = 0;
        _currentChunkSize = 0;
    }
    
    public Chunks(Span<byte> target, ReadOnlySpan<TSource1> source1)
    {
        // TODO: assert target

        _target = MemoryMarshal.Cast<byte, TTarget>(target);
        _source1 = source1;
        _position = 0;
        _currentChunkSize = 0;
    }

    public Chunks<TTarget, TSource1> GetEnumerator() => this;

    public bool MoveNext()
    {
        _position += _currentChunkSize;
        
        if (_position >= _source1.Length)
        {
            return false;
        }
        
        int remainingItems = _source1.Length - _position;
        _currentChunkSize = Math.Min(_target.Length, remainingItems);

        return true;
    }

    public Chunk Current
    {
        get
        {
            return new Chunk(
                _target.Slice(0, _currentChunkSize), _source1.Slice(_position, _currentChunkSize));
        }
    }
    
    public ref struct Chunk
    {
        private readonly Span<TTarget> _target;
        private readonly ReadOnlySpan<TSource1> _source1;

        public Chunk(
            Span<TTarget> target, ReadOnlySpan<TSource1> source1)
        {
            _source1 = source1;
            _target = target;
        }

        public Span<TTarget> Target => _target;
        public ReadOnlySpan<TSource1> Source => _source1;
    }
}
[SkipLocalsInit]
public ref struct Chunks<TTarget,TSource1, TSource2> 
    where TTarget : struct
{
    private Span<TTarget> _target;
    private readonly ReadOnlySpan<TSource1> _source1;
    private readonly ReadOnlySpan<TSource2> _source2;
    private int _position;
    private int _currentChunkSize;

    public Chunks(Span<TTarget> target, ReadOnlySpan<TSource1> source1, ReadOnlySpan<TSource2> source2)
    {
        // TODO: assert target
        // Validate all source spans have the same length
        ArgumentOutOfRangeException.ThrowIfNotEqual(source1.Length, source2.Length, nameof(source2));

        _target = target;
        _source1 = source1;
        _source2 = source2;
        _position = 0;
        _currentChunkSize = 0;
    }
    
    public Chunks(Span<byte> target, ReadOnlySpan<TSource1> source1, ReadOnlySpan<TSource2> source2)
    {
        // TODO: assert target
        // Validate all source spans have the same length
        ArgumentOutOfRangeException.ThrowIfNotEqual(source1.Length, source2.Length, nameof(source2));

        _target = MemoryMarshal.Cast<byte, TTarget>(target);
        _source1 = source1;
        _source2 = source2;
        _position = 0;
        _currentChunkSize = 0;
    }

    public Chunks<TTarget, TSource1, TSource2> GetEnumerator() => this;

    public bool MoveNext()
    {
        _position += _currentChunkSize;
        
        if (_position >= _source1.Length)
        {
            return false;
        }
        
        int remainingItems = _source1.Length - _position;
        _currentChunkSize = Math.Min(_target.Length, remainingItems);

        return true;
    }

    public Chunk Current
    {
        get
        {
            return new Chunk(
                _target.Slice(0, _currentChunkSize), _source1.Slice(_position, _currentChunkSize), _source2.Slice(_position, _currentChunkSize));
        }
    }
    
    public ref struct Chunk
    {
        private readonly Span<TTarget> _target;
        private readonly ReadOnlySpan<TSource1> _source1;
        private readonly ReadOnlySpan<TSource2> _source2;

        public Chunk(
            Span<TTarget> target, ReadOnlySpan<TSource1> source1, ReadOnlySpan<TSource2> source2)
        {
            _source1 = source1;
            _source2 = source2;
            _target = target;
        }

        public Span<TTarget> Target => _target;
        public ReadOnlySpan<TSource1> Source1 => _source1;
        public ReadOnlySpan<TSource2> Source2 => _source2;
    }
}
[SkipLocalsInit]
public ref struct Chunks<TTarget,TSource1, TSource2, TSource3> 
    where TTarget : struct
{
    private Span<TTarget> _target;
    private readonly ReadOnlySpan<TSource1> _source1;
    private readonly ReadOnlySpan<TSource2> _source2;
    private readonly ReadOnlySpan<TSource3> _source3;
    private int _position;
    private int _currentChunkSize;

    public Chunks(Span<TTarget> target, ReadOnlySpan<TSource1> source1, ReadOnlySpan<TSource2> source2, ReadOnlySpan<TSource3> source3)
    {
        // TODO: assert target
        // Validate all source spans have the same length
        ArgumentOutOfRangeException.ThrowIfNotEqual(source1.Length, source2.Length, nameof(source2));
        ArgumentOutOfRangeException.ThrowIfNotEqual(source1.Length, source3.Length, nameof(source3));

        _target = target;
        _source1 = source1;
        _source2 = source2;
        _source3 = source3;
        _position = 0;
        _currentChunkSize = 0;
    }
    
    public Chunks(Span<byte> target, ReadOnlySpan<TSource1> source1, ReadOnlySpan<TSource2> source2, ReadOnlySpan<TSource3> source3)
    {
        // TODO: assert target
        // Validate all source spans have the same length
        ArgumentOutOfRangeException.ThrowIfNotEqual(source1.Length, source2.Length, nameof(source2));
        ArgumentOutOfRangeException.ThrowIfNotEqual(source1.Length, source3.Length, nameof(source3));

        _target = MemoryMarshal.Cast<byte, TTarget>(target);
        _source1 = source1;
        _source2 = source2;
        _source3 = source3;
        _position = 0;
        _currentChunkSize = 0;
    }

    public Chunks<TTarget, TSource1, TSource2, TSource3> GetEnumerator() => this;

    public bool MoveNext()
    {
        _position += _currentChunkSize;
        
        if (_position >= _source1.Length)
        {
            return false;
        }
        
        int remainingItems = _source1.Length - _position;
        _currentChunkSize = Math.Min(_target.Length, remainingItems);

        return true;
    }

    public Chunk Current
    {
        get
        {
            return new Chunk(
                _target.Slice(0, _currentChunkSize), _source1.Slice(_position, _currentChunkSize), _source2.Slice(_position, _currentChunkSize), _source3.Slice(_position, _currentChunkSize));
        }
    }
    
    public ref struct Chunk
    {
        private readonly Span<TTarget> _target;
        private readonly ReadOnlySpan<TSource1> _source1;
        private readonly ReadOnlySpan<TSource2> _source2;
        private readonly ReadOnlySpan<TSource3> _source3;

        public Chunk(
            Span<TTarget> target, ReadOnlySpan<TSource1> source1, ReadOnlySpan<TSource2> source2, ReadOnlySpan<TSource3> source3)
        {
            _source1 = source1;
            _source2 = source2;
            _source3 = source3;
            _target = target;
        }

        public Span<TTarget> Target => _target;
        public ReadOnlySpan<TSource1> Source1 => _source1;
        public ReadOnlySpan<TSource2> Source2 => _source2;
        public ReadOnlySpan<TSource3> Source3 => _source3;
    }
}
[SkipLocalsInit]
public ref struct Chunks<TTarget,TSource1, TSource2, TSource3, TSource4> 
    where TTarget : struct
{
    private Span<TTarget> _target;
    private readonly ReadOnlySpan<TSource1> _source1;
    private readonly ReadOnlySpan<TSource2> _source2;
    private readonly ReadOnlySpan<TSource3> _source3;
    private readonly ReadOnlySpan<TSource4> _source4;
    private int _position;
    private int _currentChunkSize;

    public Chunks(Span<TTarget> target, ReadOnlySpan<TSource1> source1, ReadOnlySpan<TSource2> source2, ReadOnlySpan<TSource3> source3, ReadOnlySpan<TSource4> source4)
    {
        // TODO: assert target
        // Validate all source spans have the same length
        ArgumentOutOfRangeException.ThrowIfNotEqual(source1.Length, source2.Length, nameof(source2));
        ArgumentOutOfRangeException.ThrowIfNotEqual(source1.Length, source3.Length, nameof(source3));
        ArgumentOutOfRangeException.ThrowIfNotEqual(source1.Length, source4.Length, nameof(source4));

        _target = target;
        _source1 = source1;
        _source2 = source2;
        _source3 = source3;
        _source4 = source4;
        _position = 0;
        _currentChunkSize = 0;
    }
    
    public Chunks(Span<byte> target, ReadOnlySpan<TSource1> source1, ReadOnlySpan<TSource2> source2, ReadOnlySpan<TSource3> source3, ReadOnlySpan<TSource4> source4)
    {
        // TODO: assert target
        // Validate all source spans have the same length
        ArgumentOutOfRangeException.ThrowIfNotEqual(source1.Length, source2.Length, nameof(source2));
        ArgumentOutOfRangeException.ThrowIfNotEqual(source1.Length, source3.Length, nameof(source3));
        ArgumentOutOfRangeException.ThrowIfNotEqual(source1.Length, source4.Length, nameof(source4));

        _target = MemoryMarshal.Cast<byte, TTarget>(target);
        _source1 = source1;
        _source2 = source2;
        _source3 = source3;
        _source4 = source4;
        _position = 0;
        _currentChunkSize = 0;
    }

    public Chunks<TTarget, TSource1, TSource2, TSource3, TSource4> GetEnumerator() => this;

    public bool MoveNext()
    {
        _position += _currentChunkSize;
        
        if (_position >= _source1.Length)
        {
            return false;
        }
        
        int remainingItems = _source1.Length - _position;
        _currentChunkSize = Math.Min(_target.Length, remainingItems);

        return true;
    }

    public Chunk Current
    {
        get
        {
            return new Chunk(
                _target.Slice(0, _currentChunkSize), _source1.Slice(_position, _currentChunkSize), _source2.Slice(_position, _currentChunkSize), _source3.Slice(_position, _currentChunkSize), _source4.Slice(_position, _currentChunkSize));
        }
    }
    
    public ref struct Chunk
    {
        private readonly Span<TTarget> _target;
        private readonly ReadOnlySpan<TSource1> _source1;
        private readonly ReadOnlySpan<TSource2> _source2;
        private readonly ReadOnlySpan<TSource3> _source3;
        private readonly ReadOnlySpan<TSource4> _source4;

        public Chunk(
            Span<TTarget> target, ReadOnlySpan<TSource1> source1, ReadOnlySpan<TSource2> source2, ReadOnlySpan<TSource3> source3, ReadOnlySpan<TSource4> source4)
        {
            _source1 = source1;
            _source2 = source2;
            _source3 = source3;
            _source4 = source4;
            _target = target;
        }

        public Span<TTarget> Target => _target;
        public ReadOnlySpan<TSource1> Source1 => _source1;
        public ReadOnlySpan<TSource2> Source2 => _source2;
        public ReadOnlySpan<TSource3> Source3 => _source3;
        public ReadOnlySpan<TSource4> Source4 => _source4;
    }
}
[SkipLocalsInit]
public ref struct Chunks<TTarget,TSource1, TSource2, TSource3, TSource4, TSource5> 
    where TTarget : struct
{
    private Span<TTarget> _target;
    private readonly ReadOnlySpan<TSource1> _source1;
    private readonly ReadOnlySpan<TSource2> _source2;
    private readonly ReadOnlySpan<TSource3> _source3;
    private readonly ReadOnlySpan<TSource4> _source4;
    private readonly ReadOnlySpan<TSource5> _source5;
    private int _position;
    private int _currentChunkSize;

    public Chunks(Span<TTarget> target, ReadOnlySpan<TSource1> source1, ReadOnlySpan<TSource2> source2, ReadOnlySpan<TSource3> source3, ReadOnlySpan<TSource4> source4, ReadOnlySpan<TSource5> source5)
    {
        // TODO: assert target
        // Validate all source spans have the same length
        ArgumentOutOfRangeException.ThrowIfNotEqual(source1.Length, source2.Length, nameof(source2));
        ArgumentOutOfRangeException.ThrowIfNotEqual(source1.Length, source3.Length, nameof(source3));
        ArgumentOutOfRangeException.ThrowIfNotEqual(source1.Length, source4.Length, nameof(source4));
        ArgumentOutOfRangeException.ThrowIfNotEqual(source1.Length, source5.Length, nameof(source5));

        _target = target;
        _source1 = source1;
        _source2 = source2;
        _source3 = source3;
        _source4 = source4;
        _source5 = source5;
        _position = 0;
        _currentChunkSize = 0;
    }
    
    public Chunks(Span<byte> target, ReadOnlySpan<TSource1> source1, ReadOnlySpan<TSource2> source2, ReadOnlySpan<TSource3> source3, ReadOnlySpan<TSource4> source4, ReadOnlySpan<TSource5> source5)
    {
        // TODO: assert target
        // Validate all source spans have the same length
        ArgumentOutOfRangeException.ThrowIfNotEqual(source1.Length, source2.Length, nameof(source2));
        ArgumentOutOfRangeException.ThrowIfNotEqual(source1.Length, source3.Length, nameof(source3));
        ArgumentOutOfRangeException.ThrowIfNotEqual(source1.Length, source4.Length, nameof(source4));
        ArgumentOutOfRangeException.ThrowIfNotEqual(source1.Length, source5.Length, nameof(source5));

        _target = MemoryMarshal.Cast<byte, TTarget>(target);
        _source1 = source1;
        _source2 = source2;
        _source3 = source3;
        _source4 = source4;
        _source5 = source5;
        _position = 0;
        _currentChunkSize = 0;
    }

    public Chunks<TTarget, TSource1, TSource2, TSource3, TSource4, TSource5> GetEnumerator() => this;

    public bool MoveNext()
    {
        _position += _currentChunkSize;
        
        if (_position >= _source1.Length)
        {
            return false;
        }
        
        int remainingItems = _source1.Length - _position;
        _currentChunkSize = Math.Min(_target.Length, remainingItems);

        return true;
    }

    public Chunk Current
    {
        get
        {
            return new Chunk(
                _target.Slice(0, _currentChunkSize), _source1.Slice(_position, _currentChunkSize), _source2.Slice(_position, _currentChunkSize), _source3.Slice(_position, _currentChunkSize), _source4.Slice(_position, _currentChunkSize), _source5.Slice(_position, _currentChunkSize));
        }
    }
    
    public ref struct Chunk
    {
        private readonly Span<TTarget> _target;
        private readonly ReadOnlySpan<TSource1> _source1;
        private readonly ReadOnlySpan<TSource2> _source2;
        private readonly ReadOnlySpan<TSource3> _source3;
        private readonly ReadOnlySpan<TSource4> _source4;
        private readonly ReadOnlySpan<TSource5> _source5;

        public Chunk(
            Span<TTarget> target, ReadOnlySpan<TSource1> source1, ReadOnlySpan<TSource2> source2, ReadOnlySpan<TSource3> source3, ReadOnlySpan<TSource4> source4, ReadOnlySpan<TSource5> source5)
        {
            _source1 = source1;
            _source2 = source2;
            _source3 = source3;
            _source4 = source4;
            _source5 = source5;
            _target = target;
        }

        public Span<TTarget> Target => _target;
        public ReadOnlySpan<TSource1> Source1 => _source1;
        public ReadOnlySpan<TSource2> Source2 => _source2;
        public ReadOnlySpan<TSource3> Source3 => _source3;
        public ReadOnlySpan<TSource4> Source4 => _source4;
        public ReadOnlySpan<TSource5> Source5 => _source5;
    }
}
