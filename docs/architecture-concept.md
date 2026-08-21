# Game architecture: MVP + CQS + Events

How to apply this architecture across genres. Most apparent mismatches come from
confusing the public contract with the internal representation.

The key words **MUST**, **MUST NOT**, **SHOULD**, **SHOULD NOT**, and **MAY** are
used as defined in RFC 2119.

## Layers

**Model–View–Presenter** separates simulation, rendering, and orchestration:

- **Model** — game rules and state. MUST run without a screen. MAY depend on
  math, pathfinding, DI, and framework lifecycle (tick, dispose); MUST NOT depend
  on rendering.
- **View** — rendering, animation, input adapters. Touches GPU/pixels/screen
  coordinates. MAY use command/query results handed to it, but MUST NOT invoke
  a command or query, and MUST NOT subscribe to Model events.
- **Presenter** — the glue. Commands and queries MUST be invoked only by the
  Presenter. It subscribes to Model events and View input, coordinates responses,
  and feeds query results to the View. It MUST contain no game rules and no
  rendering.

**CQS at the Model boundary:**

- **Command** — a requested mutation, usually user/AI intent ("this unit wants to
  move there"). Each command type MUST have exactly one handler. A handler returns
  `CommandResult.Success` when the command is accepted and its requested
  postcondition holds. This includes an accepted idempotent no-op when the
  postcondition already holds. It returns `CommandResult.FromError` for an expected
  domain rejection and MUST NOT apply the requested state change in that case.
  Invalid program state and infrastructure failures use exceptions. The result is
  an acceptance outcome, not model data — reads are queries.
- **Query** — a requested read. MUST NOT have side effects. Its result MUST be a
  **boundary data object (BDO)**: a behaviourless, recursively read-only data
  contract whose type ends with `Bdo`. Even a scalar result is wrapped in a named
  BDO so the Model boundary remains explicit.
- **Event** — notification of a discrete occurrence. MUST be raised by domain
  objects and consumed by Presenters.

BDO is Pixely-specific terminology for data in the Model boundary contract. It
is independent of which boundary operation carries the data and its direction.

## Boundary contract vs. internal representation

These are independent:

- The **boundary contract** is commands / queries / events — the vocabulary the
  outside world uses to talk to the Model.
- The **internal representation** is how the Model stores and steps state. It MAY
  be data-oriented and MAY run a tight `Step(dt)` over packed arrays.

The internals MUST NOT be forced through the command/query machinery. A command
handler MAY be a thin "record this intent" that a later simulation step acts on.
When a genre seems to break the architecture ("I can't dispatch a handler per
unit per frame"), the cause is usually pushing internals through the boundary.
Commands are intent at the edge; the simulation loop underneath is whatever is
fastest.

## Decision framework

Ask in order:

1. **What are the player's discrete intents?** → **Commands** (move, build, buy,
   cast, end-turn). Few types; friendly to queueing, replay, lockstep
   determinism.
2. **What does the View read every frame?** → **Queries**, invoked by the
   Presenter. Anything continuously changing (positions, resource counts, health
   bars) is a query result the Presenter fetches each frame and hands to the
   View. It MUST NOT be pushed per-frame as events, and the View MUST NOT invoke
   the query itself.
3. **What discrete things must other systems react to?** → **Events** (died,
   built, unlocked, came-under-attack, entered-vision). Bounded in volume by
   construction.
4. **What advances simulation time?** → A non-command **`Step(dt)`** on the
   Model, called by the game loop (the host), not by a Presenter — advancing
   time is not glue. A Presenter is just one consumer of the Model; an AI actor
   is another, symmetric to it. `Step(dt)` is the one core operation that is
   neither command, query, nor event. Turn-based games hide it inside end-turn;
   real-time games call it every frame.
5. **How should the Model store state?** → Whatever the simulation needs. The
   boundary does not dictate this.

## Rules

- **Discrete vs. continuous state.** Discrete facts SHOULD live in the Model;
  continuous per-frame floats SHOULD be View-side interpolation. When the
  continuous value *is* simulation truth (an RTS unit's world position drives
  collision and range checks), it MUST be Model state the Presenter queries and
  the View reads, not View smoothing.
- **No event-per-frame.** High-frequency continuous change SHOULD NOT be
  published as events; expose it as a query. Only discrete transitions
  (`MovementStarted` / `MovementStopped`) SHOULD be published; in between, the
  Presenter re-queries each frame and feeds the View the fresh snapshot.
- **BDOs are role-neutral boundary data.** A BDO MUST belong to at least one
  command, query input, query output, or event data graph. It MAY be shared
  between graphs when they use the same data contract. For example, a settings
  query MAY return `SettingsBdo`, which a save-settings command MAY accept. Its
  name SHOULD describe the represented data rather than one use of it. A BDO
  MUST be a behaviourless record and MUST be recursively read-only to consumers.
- **Query results are temporary.** A BDO instance returned by a query is a
  snapshot, valid only until the next `Step(dt)` or handled command. Consumers
  MUST NOT cache it across frames and MUST NOT expect it to update in place —
  the Model may be out-of-process (e.g. a server) in the future, so BDOs are
  plain data, not live handles into Model memory. Hot-path queries MAY return
  pooled or reused buffers that are only valid for the current frame. This
  lifetime belongs to the query result; a BDO carried by a command or event MUST
  remain valid for that message's lifetime.
- **Commands aren't the simulation.** A `SimulationTickCommand` that mutates
  thousands of entities SHOULD be `Step(dt)` instead. Commands are intent;
  stepping is not.
- **Event stream.** For non-trivial event volume, a pull-based event stream
  SHOULD be used: a ring buffer with per-consumer cursors lets multiple consumers
  (render, audio, AI) drain at their own pace and tolerates bursts. Naive C#
  events do not.
- **Caller-assigned identity.** A command that creates an entity SHOULD take the
  new entity's identity as input (a client-generated id, e.g. a GUID) rather than
  returning it. The handler stays intent-only, and the caller can reference the
  entity in follow-up commands and queries without a round trip — which keeps
  scripted, AI, and LLM-driven command sequences straightforward and preserves
  replay / lockstep determinism.

## Examples

**Turn-based tactics.** Commands: move, attack, end-turn. Queries: movement
range, visibility. Events: unit moved/removed, turn started, became-visible.
`Step(dt)` is trivial — the sim advances on end-turn. Discrete tile positions in
the Model; View interpolates the visual slide.

**Idler / incremental.** Discrete tick (per 100 ms / second) is the `Step(dt)`.
Few commands (buy, prestige). Resource counts change continuously → query them
for display, don't event them. Events only for milestones (unlock, prestige,
offline-progress applied). Works, but the ceremony is heavy for how simple an
idler is.

**Local RTS.** Orders (`Move` / `Attack` / `Build`) are a canonical command fit
and lockstep-friendly. The Model runs a data-oriented `Step(dt)` over packed
arrays for movement/collision/targeting — the dispatcher never enters the hot
loop. Continuous positions are Model state, exposed by query; the Presenter wires
that result to the View each frame. Events are discrete transitions only. Two
adjustments versus turn-based: an explicit per-frame `Step(dt)`, and accepting
continuous position as Model truth.

| Concern               | Turn-based | Idler        | Local RTS                    |
|-----------------------|------------|--------------|------------------------------|
| MVP separation        | Fine       | Fine         | Fine                         |
| CQS for player intent | Fine       | Fine (light) | Excellent fit (order queue)  |
| Internal stepping     | On-turn    | Cheap ticks  | Data-oriented `Step(dt)`     |
| Event stream          | Discrete   | Discrete     | Discrete transitions only    |
| Continuous state      | View-side  | Query        | Model state, read via query  |
| Render-by-query       | Some       | Required     | Required                     |

The pattern generalizes: keep commands as intent at the boundary, let the Model's
internals be as data-oriented as the simulation needs, push only discrete events,
pull continuous state by query. Where a genre seems not to fit, check first
whether you're forcing internals through the boundary or treating
simulation-truth floats as View state.
