using System.Collections;
using System.Collections.Immutable;

namespace GameKit.DependencyInjection.Generator;

readonly struct EquatableArray<T>(ImmutableArray<T> array) : IEquatable<EquatableArray<T>>, IEnumerable<T>
    where T : IEquatable<T>
{
    public ImmutableArray<T> Array { get; } = array;

    public int Length => Array.Length;

    public T this[int index] => Array[index];

    public bool Equals(EquatableArray<T> other)
    {
        return Array.AsSpan().SequenceEqual(other.Array.AsSpan());
    }

    public override bool Equals(object? obj) => obj is EquatableArray<T> other && Equals(other);

    public override int GetHashCode()
    {
        int hash = 0;
        foreach (T item in Array)
        {
            hash = hash * 31 + item.GetHashCode();
        }
        return hash;
    }

    public ImmutableArray<T>.Enumerator GetEnumerator() => Array.GetEnumerator();

    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        return ((IEnumerable<T>)Array).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Array).GetEnumerator();
}
