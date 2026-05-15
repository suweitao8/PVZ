# ADR-0004: CombatManager Singleton Pattern

## Status

Accepted

## Date

2026-05-15

## Last Verified

2026-05-15

## Decision Makers

MegaCrit Studio

## Summary

Combat state is managed by a single `CombatManager` singleton instance that owns the `CombatState`, coordinates turn flow (player turn → enemy turn → repeat), tracks combat history, and provides the primary API for all combat actions. This centralizes combat logic, enables the AutoSlay bot to observe and control combat, and provides a single source of truth for UI and multiplayer synchronization.

## Engine Compatibility

| Field | Value |
|-------|-------|
| **Engine** | Godot 4.5 |
| **Domain** | Core |
| **Knowledge Risk** | LOW — standard C# singleton pattern |
| **References Consulted** | None needed |
| **Post-Cutoff APIs Used** | None |
| **Verification Required** | Thread safety for multiplayer, state consistency |

## ADR Dependencies

| Field | Value |
|-------|-------|
| **Depends On** | ADR-0001 (C# primary), ADR-0002 (GameAction queue) |
| **Enables** | AutoSlay bot, Multiplayer combat sync, Combat UI |
| **Blocks** | None |
| **Ordering Note** | Core combat system — many systems depend on it |

## Context

### Problem Statement

Combat in a card game involves complex state (player hand, deck, discard, energy, enemy intents, status effects, turn phase). This state must be:
1. Accessible from many systems (UI, AI, multiplayer, achievements)
2. Modified only through validated actions (no direct mutation)
3. Tracked for history/replay
4. Synchronized for multiplayer

### Current State

`CombatManager` (`src/Core/Combat/CombatManager.cs`) is a singleton with:
- `Instance` static property (lazy singleton)
- `CombatState _state` — current combat state
- `CombatHistory History` — action history for replay/debug
- `CombatStateTracker StateTracker` — state change notifications
- Turn phase tracking: `IsPlayPhase`, `IsEnemyTurnStarted`, `EndingPlayerTurnPhaseOne/Two`
- Player readiness tracking for multiplayer: `_playersReadyToEndTurn`

### Constraints

- Combat state must be consistent at all times
- Multiple systems need read access to combat state
- State changes must be observable (UI updates, VFX triggers)
- AutoSlay bot needs to intercept/control combat
- Multiplayer requires synchronized state across clients

### Requirements

- Single source of truth for combat state
- Observable state changes
- History tracking for replay/debug
- Thread-safe player readiness for multiplayer
- Clear turn phase boundaries

## Decision

Use a singleton `CombatManager` that:
1. Owns the `CombatState` object (no other code holds a separate state)
2. Provides read-only access via properties
3. All state changes go through `GameAction` queue (ADR-0002)
4. Emits events on state changes via `CombatStateTracker`
5. Tracks history in `CombatHistory` for every action

### Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    CombatManager.Instance                    │
│                                                              │
│  ┌─────────────┐  ┌───────────────┐  ┌──────────────────┐   │
│  │ CombatState │  │ CombatHistory │  │ StateTracker     │   │
│  │ - hand      │  │ - entries[]   │  │ - OnStateChanged │   │
│  │ - deck      │  │               │  │                  │   │
│  │ - discard   │  │               │  │                  │   │
│  │ - enemies   │  │               │  │                  │   │
│  │ - energy    │  │               │  │                  │   │
│  └─────────────┘  └───────────────┘  └──────────────────┘   │
│         │                                    │               │
│         │ read-only                          │ events        │
│         ▼                                    ▼               │
│  ┌─────────────────────────────────────────────────────┐    │
│  │                GameAction Queue                      │    │
│  │  PlayCard → DrawCard → DealDamage → ApplyStatus     │    │
│  └─────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────┘
          │
          │ observed/controlled by
          ▼
┌─────────────────┐  ┌─────────────┐  ┌───────────────┐
│   Combat UI     │  │  AutoSlay   │  │  Multiplayer  │
│  (hand, HP...)  │  │    Bot      │  │    Sync       │
└─────────────────┘  └─────────────┘  └───────────────┘
```

### Key Interfaces

```csharp
public class CombatManager
{
    public static CombatManager Instance { get; }

    // Current state (read-only for consumers)
    private CombatState? _state;

    // Turn phase tracking
    public bool IsPlayPhase { get; }
    public bool IsEnemyTurnStarted { get; }
    public bool EndingPlayerTurnPhaseOne { get; }
    public bool EndingPlayerTurnPhaseTwo { get; }

    // Player readiness (multiplayer)
    private readonly HashSet<Player> _playersReadyToEndTurn;

    // History and tracking
    public CombatHistory History { get; }
    public CombatStateTracker StateTracker { get; }

    // Events
    public event Action<CombatState?>? PlayerActionsDisabledChanged;
}

public class CombatState
{
    public IReadOnlyList<Card> Hand { get; }
    public IReadOnlyList<Card> Deck { get; }
    public IReadOnlyList<Card> DiscardPile { get; }
    public IReadOnlyList<Enemy> Enemies { get; }
    public Player Player { get; }
    public int Energy { get; }
    // ... other state fields
}
```

### Implementation Guidelines

1. Never expose `_state` directly — always return read-only projections
2. All mutations go through GameAction queue
3. Use `CombatStateTracker` for UI subscriptions (not direct events)
4. Log all state transitions for debugging
5. Reset state cleanly when combat ends

## Alternatives Considered

### Alternative 1: Godot Node Singleton (Autoload)

- **Description**: Make CombatManager an Autoload node in Godot
- **Pros**: Native Godot pattern, accessible from GDScript
- **Cons**: Must be in scene tree, lifecycle tied to Godot, harder to test in isolation
- **Estimated Effort**: Similar
- **Rejection Reason**: C# singleton is more testable and doesn't require scene tree

### Alternative 2: Dependency Injection

- **Description**: Inject CombatManager where needed instead of singleton
- **Pros**: More testable, explicit dependencies
- **Cons**: Verbose, many systems need combat access, adds DI framework
- **Estimated Effort**: Higher
- **Rejection Reason**: Singleton is simpler for a single global combat context

### Alternative 3: State Machine per Entity

- **Description**: Each entity (player, enemies) owns its own state
- **Pros**: More modular
- **Cons**: Hard to coordinate turn flow, no single source of truth
- **Estimated Effort**: Higher
- **Rejection Reason**: Combat is inherently global — all entities interact

## Consequences

### Positive

- Single source of truth for combat state
- Easy for any system to access combat info
- History enables replay and debug
- StateTracker provides clean observer pattern
- AutoSlay can observe and control combat easily

### Negative

- Singleton is a global dependency (harder to test in isolation)
- All combat code depends on CombatManager
- Must be careful to reset state between combats

### Neutral

- This pattern is consistent with other managers (RunManager, SaveManager)

## Risks

| Risk | Probability | Impact | Mitigation |
|------|------------|--------|-----------|
| State not reset between combats | Medium | High | Explicit `ResetState()` called on combat end |
| Thread safety in multiplayer | Medium | High | Use `Lock` for `_playersReady` collections |
| Singleton becomes "god object" | Medium | Medium | Delegate to focused subsystems (StateTracker, History) |

## Performance Implications

| Metric | Before | Expected After | Budget |
|--------|--------|---------------|--------|
| CPU (state access) | N/A | O(1) property access | <0.01ms |
| Memory (combat state) | N/A | ~10-50KB per combat | <1MB |

## Migration Plan

Already implemented. CombatManager is the core of the combat system.

**Rollback plan**: Not applicable (core system)

## Validation Criteria

- [x] CombatManager.Instance provides access to all combat state
- [x] State changes only through GameAction queue
- [x] CombatHistory records all actions
- [x] StateTracker fires events on state changes
- [x] Turn phase tracking is accurate
- [x] Multiplayer player readiness tracking works

## GDD Requirements Addressed

Foundational — no GDD requirement. Enables: Combat System (all combat mechanics)

## Related

- ADR-0001: C# as Primary Language
- ADR-0002: GameAction Async Queue System
- `src/Core/Combat/CombatManager.cs` — Implementation
- `src/Core/AutoSlay/Handlers/Rooms/CombatRoomHandler.cs` — AutoSlay integration
