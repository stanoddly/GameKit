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

`CommandDispatcher`, `DomainEventDispatchHook`, and similar infrastructure are not discovered as handlers
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

Non-public setters are allowed so the Model can construct and fill result instances internally
(object initializers, mapping, deserialization) while consumers still cannot mutate them. This does
not make a result a live handle — a query result is a temporary snapshot, never cached across frames
(see [architecture-concept.md](architecture-concept.md)).

## ModelBoundary

`ModelBoundary.Check(assembly, configure)` checks the central claim — *the boundary contract is
commands / queries / events*:

- **InternalsVisibleTo policy** — ordered rules allow or disallow friend assemblies.
- **Reachability** — every public type must be reachable from the CQS surface (commands, queries,
  events, and declared surface seeds), or be handled by an outside-surface rule.

All policy is caller-supplied through the options:

```csharp
ArchitectureReport report = ModelBoundary.Check(typeof(GameModule).Assembly, options => options
    .AllowInternalsTo("Game.Editor", "Required for editor integration.")
    .DisallowInternalsTo(
        new Regex(@"Game\.Tests(?:\..*)?"),
        "Tests must exercise the public boundary.")
    .DisallowInternalsTo(new Regex(@".*"), "No other assembly may access Model internals.")
    .TreatAsSurface(type => type.Name.EndsWith("Module"))
    .TreatAsSurface(type => typeof(IMarkerRoot).IsAssignableFrom(type))
    .AllowOutsideSurface(typeof(SomeIntentionalPublicType), "Required by the serializer.")
    .DisallowOutsideSurface(
        new Regex(@".*"),
        "All other public types must be reachable from the boundary surface."));
```

Rules are evaluated in declaration order. A rule handles and removes every matching candidate, so the first
matching rule decides each assembly or outside-surface type. Exact `string`/`Type` overloads use exact matching;
`Regex` overloads must match the candidate's entire name. Candidates left unmatched after all rules are allowed.
Put specific decisions before a final `.*` disallow rule when the policy should be closed by default. Every rule
requires a reason, which is included in diagnostics from disallow rules.

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
        .AllowInternalsTo("Game.Editor", "Required for editor integration.")
        .DisallowInternalsTo(new Regex(@".*"), "No other assembly may access Model internals.")
        .TreatAsSurface(type => type.Name.EndsWith("Module"))
        .DisallowOutsideSurface(
            new Regex(@".*"),
            "Public types must be reachable from the boundary surface."));
    Assert.That(report.Violations, Is.Empty, report.ToString());
}
```
