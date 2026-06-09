# Architecture testing

`GameKit.Architecture.Testing` turns the boundary claims in [architecture-concept.md](architecture-concept.md)
into reflection checks a game runs as ordinary unit tests. Roles are discovered through the
`GameKit.Architecture` contracts (`ICommandHandler<>`, `IQueryHandler<,>`, `DomainMessage`), not
name suffixes.

Both entry points are framework-agnostic: they return an `ArchitectureReport` (a `Violations`
list, `IsValid`, and a formatted `ToString()`), so you assert with whatever test framework you use.

```csharp
ArchitectureReport report = CqsConventions.Check(typeof(GameModule).Assembly);
Assert.That(report.Violations, Is.Empty, report.ToString());
```

## CqsConventions

`CqsConventions.Check(params Assembly[])` enforces the per-type CQS conventions:

- **Commands and query inputs are behaviourless records** — a record with no custom methods.
- **Handler naming** — types implementing `ICommandHandler<>` / `IQueryHandler<,>` end with
  `CommandHandler` / `QueryHandler`.
- **Handlers are internal with no public constructors** — callers go through the dispatcher, and DI
  constructs them via an internal constructor.
- **Command handlers don't depend on other command handlers** — shared behaviour belongs in a domain
  service, not handler chaining.
- **Query results are recursively readonly** — see below.

`CommandDispatcher`, `DomainEventPump`, and similar infrastructure are not discovered as handlers
(they don't implement the handler interfaces), so they need no exclusion.

### Readonly query results

The result type (`TResult` of `IQueryHandler<TQuery, TResult>`) must be readonly from the consumer's
side, checked recursively. A type passes when either:

- it is a **known-immutable type** — primitives, `enum`, `string`, `decimal`, `Guid`,
  `DateTime`/`DateTimeOffset`/`TimeSpan`/`DateOnly`/`TimeOnly`, the read-only collection interfaces
  (`IReadOnlyList<>`, `IReadOnlyCollection<>`, `IReadOnlyDictionary<,>`, `IReadOnlySet<>`),
  `Immutable*`, and `ValueTuple` (element/argument types are still recursed); or
- **every member is non-externally-mutable and every member type is itself readonly**:
  - properties are get-only, `init`, or have a **non-public** setter (a public `set` fails),
  - fields are `readonly`, `const`, or **non-public**,
  - arrays (`T[]`) fail — expose `IReadOnlyList<T>` / `ImmutableArray<T>` instead,
  - compiler-generated record backing fields are ignored (judged via their property).

The non-public-setter rule is the escape hatch for live read-only handles: a domain entity may keep
`internal set` properties (the Model mutates it) while remaining a valid result the View only reads.

## ModelBoundary

`ModelBoundary.Check(assembly, configure)` enforces the central claim — *the boundary contract is
commands / queries / events*:

- **InternalsVisibleTo whitelist** — the Model exposes internals only to assemblies you allow.
- **Reachability** — every public type must be reachable from the CQS surface (commands, queries,
  events, and declared surface seeds). A public type nothing on the surface references is a leak.

All policy is caller-supplied through the options:

```csharp
ArchitectureReport report = ModelBoundary.Check(typeof(GameModule).Assembly, options => options
    .AllowInternalsTo("Game.Editor")                                  // InternalsVisibleTo targets
    .TreatAsSurface(type => type.Name.EndsWith("Module"))             // extra surface roots (DI modules)
    .TreatAsSurface(type => typeof(IMarkerRoot).IsAssignableFrom(type))
    .Exclude(typeof(SomeIntentionalPublicType)));                     // exempt from the leak check
```

Reachability seeds from handler `Handle` signatures (including internal handlers), so query result
types are reachable through the contract rather than only through incidental references.

## Typical usage

```csharp
[Test]
public void CqsConventions_AreHeld()
{
    ArchitectureReport report = CqsConventions.Check(
        typeof(GameModule).Assembly, typeof(EditorModule).Assembly);
    Assert.That(report.Violations, Is.Empty, report.ToString());
}

[Test]
public void Model_ExposesOnlyItsCqsSurface()
{
    ArchitectureReport report = ModelBoundary.Check(typeof(GameModule).Assembly, options => options
        .AllowInternalsTo("Game.Editor")
        .TreatAsSurface(type => type.Name.EndsWith("Module")));
    Assert.That(report.Violations, Is.Empty, report.ToString());
}
```
