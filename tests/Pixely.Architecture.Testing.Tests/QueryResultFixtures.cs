using System.Collections.Immutable;
using Pixely.Architecture;

namespace Pixely.Architecture.Testing.Tests.QueryResultFixtures;

// Clean: result is a record exposing only init/get members over immutable and read-only collection types.
internal record RangeQuery(int Origin);

internal sealed record GoodQdo(IReadOnlyList<int> Tiles, ImmutableArray<string> Names, int Count);

internal sealed class GoodQdoQueryHandler : IQueryHandler<RangeQuery, GoodQdo>
{
    internal GoodQdoQueryHandler()
    {
    }

    public GoodQdo Handle(RangeQuery query) => new([], [], 0);
}

// Clean: a domain-entity-style result that stays mutable internally but is readonly to external consumers.
internal sealed class InternalSetterQdo
{
    public int Health { get; internal set; }
}

internal record EntityQuery(int Id);

internal sealed class InternalSetterQueryHandler : IQueryHandler<EntityQuery, InternalSetterQdo>
{
    internal InternalSetterQueryHandler()
    {
    }

    public InternalSetterQdo Handle(EntityQuery query) => new();
}

// Violation: public setter.
internal sealed class PublicSetterQdo
{
    public int Value { get; set; }
}

internal record PublicSetterQuery(int X);

internal sealed class PublicSetterQueryHandler : IQueryHandler<PublicSetterQuery, PublicSetterQdo>
{
    internal PublicSetterQueryHandler()
    {
    }

    public PublicSetterQdo Handle(PublicSetterQuery query) => new();
}

// Violation: exposes a mutable List<T>.
internal sealed record ListQdo(List<int> Values);

internal record ListQuery(int X);

internal sealed class ListQueryHandler : IQueryHandler<ListQuery, ListQdo>
{
    internal ListQueryHandler()
    {
    }

    public ListQdo Handle(ListQuery query) => new([]);
}

// Violation: exposes an array.
internal sealed record ArrayQdo(int[] Values);

internal record ArrayQuery(int X);

internal sealed class ArrayQueryHandler : IQueryHandler<ArrayQuery, ArrayQdo>
{
    internal ArrayQueryHandler()
    {
    }

    public ArrayQdo Handle(ArrayQuery query) => new([]);
}

// Violation: recursive — a readonly wrapper around a mutable nested type.
internal sealed class MutableInner
{
    public int X { get; set; }
}

internal sealed record NestedQdo(MutableInner Inner);

internal record NestedQuery(int X);

internal sealed class NestedQueryHandler : IQueryHandler<NestedQuery, NestedQdo>
{
    internal NestedQueryHandler()
    {
    }

    public NestedQdo Handle(NestedQuery query) => new(new MutableInner());
}
