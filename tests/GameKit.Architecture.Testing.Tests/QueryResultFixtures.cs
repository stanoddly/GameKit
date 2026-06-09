using System.Collections.Immutable;
using GameKit.Architecture;

namespace GameKit.Architecture.Testing.Tests.QueryResultFixtures;

// Clean: result is a record exposing only init/get members over immutable and read-only collection types.
internal record RangeQuery(int Origin);

internal sealed record GoodResult(IReadOnlyList<int> Tiles, ImmutableArray<string> Names, int Count);

internal sealed class GoodResultQueryHandler : IQueryHandler<RangeQuery, GoodResult>
{
    internal GoodResultQueryHandler()
    {
    }

    public GoodResult Handle(RangeQuery query) => new([], [], 0);
}

// Clean: a domain-entity-style result that stays mutable internally but is readonly to external consumers.
internal sealed class InternalSetterResult
{
    public int Health { get; internal set; }
}

internal record EntityQuery(int Id);

internal sealed class InternalSetterQueryHandler : IQueryHandler<EntityQuery, InternalSetterResult>
{
    internal InternalSetterQueryHandler()
    {
    }

    public InternalSetterResult Handle(EntityQuery query) => new();
}

// Violation: public setter.
internal sealed class PublicSetterResult
{
    public int Value { get; set; }
}

internal record PublicSetterQuery(int X);

internal sealed class PublicSetterQueryHandler : IQueryHandler<PublicSetterQuery, PublicSetterResult>
{
    internal PublicSetterQueryHandler()
    {
    }

    public PublicSetterResult Handle(PublicSetterQuery query) => new();
}

// Violation: exposes a mutable List<T>.
internal sealed record ListResult(List<int> Values);

internal record ListQuery(int X);

internal sealed class ListQueryHandler : IQueryHandler<ListQuery, ListResult>
{
    internal ListQueryHandler()
    {
    }

    public ListResult Handle(ListQuery query) => new([]);
}

// Violation: exposes an array.
internal sealed record ArrayResult(int[] Values);

internal record ArrayQuery(int X);

internal sealed class ArrayQueryHandler : IQueryHandler<ArrayQuery, ArrayResult>
{
    internal ArrayQueryHandler()
    {
    }

    public ArrayResult Handle(ArrayQuery query) => new([]);
}

// Violation: recursive — a readonly wrapper around a mutable nested type.
internal sealed class MutableInner
{
    public int X { get; set; }
}

internal sealed record NestedResult(MutableInner Inner);

internal record NestedQuery(int X);

internal sealed class NestedQueryHandler : IQueryHandler<NestedQuery, NestedResult>
{
    internal NestedQueryHandler()
    {
    }

    public NestedResult Handle(NestedQuery query) => new(new MutableInner());
}
