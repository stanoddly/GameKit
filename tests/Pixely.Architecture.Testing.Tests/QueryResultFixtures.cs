using System.Collections.Immutable;
using Pixely.Architecture;

namespace Pixely.Architecture.Testing.Tests.QueryResultFixtures;

// Clean: result is a record exposing only init/get members over immutable and read-only collection types.
internal record RangeQuery(int Origin);

internal sealed record GoodBdo(IReadOnlyList<int> Tiles, ImmutableArray<string> Names, int Count);

internal sealed class GoodBdoQueryHandler : IQueryHandler<RangeQuery, GoodBdo>
{
    internal GoodBdoQueryHandler()
    {
    }

    public GoodBdo Handle(RangeQuery query) => new([], [], 0);
}

// Clean: a domain-entity-style result that stays mutable internally but is readonly to external consumers.
internal sealed class InternalSetterBdo
{
    public int Health { get; internal set; }
}

internal record EntityQuery(int Id);

internal sealed class InternalSetterQueryHandler : IQueryHandler<EntityQuery, InternalSetterBdo>
{
    internal InternalSetterQueryHandler()
    {
    }

    public InternalSetterBdo Handle(EntityQuery query) => new();
}

// Violation: public setter.
internal sealed class PublicSetterBdo
{
    public int Value { get; set; }
}

internal record PublicSetterQuery(int X);

internal sealed class PublicSetterQueryHandler : IQueryHandler<PublicSetterQuery, PublicSetterBdo>
{
    internal PublicSetterQueryHandler()
    {
    }

    public PublicSetterBdo Handle(PublicSetterQuery query) => new();
}

// Violation: exposes a mutable List<T>.
internal sealed record ListBdo(List<int> Values);

internal record ListQuery(int X);

internal sealed class ListQueryHandler : IQueryHandler<ListQuery, ListBdo>
{
    internal ListQueryHandler()
    {
    }

    public ListBdo Handle(ListQuery query) => new([]);
}

// Violation: exposes an array.
internal sealed record ArrayBdo(int[] Values);

internal record ArrayQuery(int X);

internal sealed class ArrayQueryHandler : IQueryHandler<ArrayQuery, ArrayBdo>
{
    internal ArrayQueryHandler()
    {
    }

    public ArrayBdo Handle(ArrayQuery query) => new([]);
}

// Violation: recursive — a readonly wrapper around a mutable nested type.
internal sealed class MutableInner
{
    public int X { get; set; }
}

internal sealed record NestedBdo(MutableInner Inner);

internal record NestedQuery(int X);

internal sealed class NestedQueryHandler : IQueryHandler<NestedQuery, NestedBdo>
{
    internal NestedQueryHandler()
    {
    }

    public NestedBdo Handle(NestedQuery query) => new(new MutableInner());
}
