# ADR-0002: GameAction Async Queue System

## Status

Accepted

## Date

2026-05-15

## Last Verified

2026-05-15

## Decision Makers

MegaCrit Studio

## Summary

All game actions (card plays, enemy moves, damage calculations) flow through a centralized async GameAction queue system. Each action is a discrete object with lifecycle states (Waiting, Executing, Paused, Completed, Cancelled), enabling deterministic sequencing, replay support, multiplayer synchronization, and the ability to pause for player choices mid-execution.

## Engine Compatibility

| Field | Value |
|-------|-------|
| **Engine** | Godot 4.5 |
| **Domain** | Core / Scripting |
| **Knowledge Risk** | LOW — C# async/await is standard .NET |
| **References Consulted** | None needed — standard .NET patterns |
| **Post-Cutoff APIs Used** | None |
| **Verification Required** | Task completion on main thread, async exception handling |

## ADR Dependencies

| Field | Value |
|-------|-------|
| **Depends On** | ADR-0001 (C# primary language — requires async/await) |
| **Enables** | ADR-0004 (CombatManager), Multiplayer synchronization, Replay system |
| **Blocks** | None |
| **Ordering Note** | Must exist before CombatManager implementation |

## Context

### Problem Statement

In a card game with complex action chains (play card → trigger effects → deal damage → apply status → trigger on-damage → etc.), sequencing must be deterministic, interruptible, and replayable for multiplayer and debugging.

### Current State

The `GameAction` abstract base class (`src/Core/GameActions/GameAction.cs`) defines the action lifecycle:
- Actions are enqueued with unique IDs
- Actions execute asynchronously via `ExecuteAction()`
- Actions can pause for player choice (`_pauseForPlayerChoiceTaskSource`)
- Actions track state: None → WaitingForExecution → Executing → Completed/Cancelled

### Constraints

- Actions must complete in order (queue semantics)
- Some actions require player input mid-execution
- Actions must be serializable for multiplayer and replay
- Godot runs on main thread; async must yield back correctly

### Requirements

- Deterministic action ordering
- Ability to pause action for player choice, then resume
- Event hooks before/after each action phase
- Support for action cancellation
- Async execution without blocking main thread

## Decision

Implement a GameAction queue where:
1. Each action is a class extending `GameAction`
2. Actions are enqueued with auto-incrementing IDs
3. `Execute()` manages the state machine transition
4. Actions can call `PauseForPlayerChoice()` which awaits a TaskCompletionSource
5. Events fire at each lifecycle transition for UI/hooks

### Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    GameAction Queue                          │
│                                                              │
│  ┌─────────┐    ┌──────────┐    ┌───────────────────────┐   │
│  │ Enqueue │ →  │ Waiting  │ →  │ Executing             │   │
│  │ (ID++)  │    │          │    │  ├─ ExecuteAction()   │   │
│  └─────────┘    └──────────┘    │  ├─ PauseForChoice()  │   │
│                                 │  └─ Resume()          │   │
│                                 └───────────────────────┘   │
│                                          │                   │
│                    ┌─────────────────────┼───────────────┐   │
│                    ▼                     ▼               ▼   │
│              ┌──────────┐         ┌───────────┐    ┌───────┐ │
│              │ Completed│         │ Cancelled │    │Paused │ │
│              └──────────┘         └───────────┘    └───────┘ │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
              ┌─────────────────────────────┐
              │ Events: BeforeExecuted,     │
              │ BeforePaused, BeforeResumed,│
              │ AfterFinished               │
              └─────────────────────────────┘
```

### Key Interfaces

```csharp
public abstract class GameAction
{
    public GameActionState State { get; private set; }
    public abstract GameActionType ActionType { get; }
    public uint? Id { get; private set; }

    // Lifecycle events
    public event Action<GameAction>? BeforeExecuted;
    public event Action<GameAction>? BeforePausedForPlayerChoice;
    public event Action<GameAction>? BeforeResumedAfterPlayerChoice;
    public event Action<GameAction>? AfterFinished;

    // Called by queue
    public async Task Execute();

    // Implemented by concrete actions
    protected abstract Task ExecuteAction();

    // Pause for player input
    protected Task PauseForPlayerChoice();

    // Resume after player input
    public void ResumeAfterPlayerChoice();
}

public enum GameActionState
{
    None,
    WaitingForExecution,
    Executing,
    PausedForPlayerChoice,
    ReadyToResumeExecuting,
    Completed,
    Cancelled
}
```

### Implementation Guidelines

1. Every game action (PlayCard, DealDamage, ApplyStatus, etc.) extends `GameAction`
2. Use `TaskCompletionSource` for pause/resume mechanics
3. Never block the main thread — always use `await`
4. Hook into events for UI updates and VFX triggers
5. Log action state transitions for debugging/replay

## Alternatives Considered

### Alternative 1: Synchronous Execution

- **Description**: Execute actions synchronously in a single frame
- **Pros**: Simpler code, no async complexity
- **Cons**: Cannot pause for player choice, blocks main thread, no animation timing
- **Estimated Effort**: Lower initial, but limits features
- **Rejection Reason**: Cannot support player choice mid-action (essential for card targeting)

### Alternative 2: Coroutine-based (Godot native)

- **Description**: Use Godot coroutines (`await ToSignal`) instead of Tasks
- **Pros**: More Godot-idiomatic
- **Cons**: No return values, harder to compose, no Task.WhenAll support
- **Estimated Effort**: Similar
- **Rejection Reason**: C# Tasks are more powerful and testable

### Alternative 3: Event Bus Only

- **Description**: Pure event-driven without explicit queue
- **Pros**: Maximum flexibility
- **Cons**: Ordering non-deterministic, hard to debug, no replay
- **Estimated Effort**: Higher (no structure)
- **Rejection Reason**: Determinism is critical for card game

## Consequences

### Positive

- Deterministic action ordering
- Natural pause points for player targeting/UI
- Events enable UI/VFX hooks without coupling
- Serializable actions enable replay and multiplayer sync
- Async execution keeps UI responsive

### Negative

- Added complexity: every action must be a GameAction class
- Async debugging is harder than synchronous
- Potential for deadlocks if TaskCompletionSource never completes

### Neutral

- All game logic flows through this system

## Risks

| Risk | Probability | Impact | Mitigation |
|------|------------|--------|-----------|
| Deadlock from uncompleted TaskCompletionSource | Medium | High | Add timeout + watchdog in AutoSlay |
| Exception in async action crashes game | Medium | High | Wrap ExecuteAction in try-catch, log, set Cancelled |
| Action queue grows unbounded | Low | Medium | Enforce max queue depth, fail gracefully |

## Performance Implications

| Metric | Before | Expected After | Budget |
|--------|--------|---------------|--------|
| CPU (per action) | N/A | ~0.1-1ms (async overhead) | <5ms per action |
| Memory (per action) | N/A | ~1-5KB (action object) | <100KB total queue |
| Latency (pause→resume) | N/A | <1ms | <16.67ms |

## Migration Plan

Already implemented. Concrete action types exist in `src/Core/GameActions/` and `src/Core/Entities/Actions/`.

**Rollback plan**: Not applicable (core system, 3286 files depend on it)

## Validation Criteria

- [x] Actions execute in enqueue order
- [x] Actions can pause and resume for player choice
- [x] Events fire at correct lifecycle points
- [x] Async exceptions are caught and logged
- [x] CombatManager uses GameAction queue for all combat actions

## GDD Requirements Addressed

Foundational — no GDD requirement. Enables: Combat System (all card plays, enemy moves, damage calculations)

## Related

- ADR-0001: C# as Primary Language (enables async/await)
- ADR-0004: CombatManager Singleton (uses GameAction queue)
- `src/Core/GameActions/GameAction.cs` — Base implementation
- `src/Core/Combat/CombatManager.cs` — Primary consumer
