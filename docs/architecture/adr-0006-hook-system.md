# ADR-0006: Hook System for Game Event Modification

## Status

Accepted

## Date

2026-05-15

## Last Verified

2026-05-15

## Decision Makers

MegaCrit Studio

## Summary

All game modifications (damage, block, card effects, turn triggers) flow through a centralized Hook system. The `Hook` static class dispatches events to all registered listeners (powers, relics, cards, potions, orbs), which override virtual methods on `AbstractModel` to modify values, trigger effects, or prevent actions. Hooks execute in deterministic phases (VeryEarly → Early → Normal → Late) enabling predictable interaction ordering.

## Engine Compatibility

| Field | Value |
|-------|-------|
| **Engine** | Godot 4.5 |
| **Domain** | Core |
| **Knowledge Risk** | LOW — standard C# patterns |
| **References Consulted** | None needed |
| **Post-Cutoff APIs Used** | None |
| **Verification Required** | Hook execution order consistency |

## ADR Dependencies

| Field | Value |
|-------|-------|
| **Depends On** | ADR-0001 (C# primary), ADR-0002 (GameAction queue for async hooks) |
| **Enables** | All gameplay systems (Powers, Relics, Cards, Potions) |
| **Blocks** | None |
| **Ordering Note** | Core system — required before Powers/Relics implementation |

## Context

### Problem Statement

A card game with complex interactions needs a way to:
1. Modify values dynamically (damage, block, energy, etc.)
2. Trigger effects on game events (card played, turn end, damage dealt)
3. Prevent actions conditionally (death prevention, targeting blocks)
4. Maintain deterministic execution order for multiplayer sync
5. Support both synchronous value modification and async event triggers

### Current State

`src/Core/Hooks/Hook.cs` defines the `Hook` static class with 60+ hook methods. `AbstractModel` provides virtual implementations for all hooks. Models that can receive hooks: `PowerModel`, `RelicModel`, `CardModel`, `PotionModel`, `OrbModel`, `MonsterModel`, `EnchantmentModel`, `AfflictionModel`, `ModifierModel`.

### Constraints

- Hook execution must be deterministic for multiplayer
- Some hooks need to short-circuit (predicate hooks)
- Some hooks need async execution (VFX, animations)
- Order of execution matters (phases)
- Thousands of hooks may fire per combat

### Requirements

- Centralized dispatch point for all modifications
- Phase-based ordering (VeryEarly, Early, Normal, Late)
- Support for additive, multiplicative, and cap modifiers
- Short-circuit capability for predicate hooks
- Async support for event hooks

## Decision

Implement a Hook system with these components:

### Architecture

```
┌─────────────────────────────────────────────────────────────────────┐
│                     Game Code (Combat, Cards, etc.)                  │
│                                                                      │
│  DamageCmd.Attack() ──→ Hook.ModifyDamage() ──→ Apply damage        │
│  CardCmd.Play() ──────→ Hook.AfterCardPlayed() ──→ VFX + triggers   │
│  CombatManager ───────→ Hook.BeforeTurnEnd() ──→ Turn effects       │
└─────────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────────┐
│                    Hook (static dispatcher)                          │
│                                                                      │
│  foreach (AbstractModel item in combatState.IterateHookListeners()) │
│  {                                                                   │
│      // Phase-based execution                                       │
│      await item.BeforeTurnEndVeryEarly();                           │
│  }                                                                   │
│  foreach (AbstractModel item in ...)                                │
│      await item.BeforeTurnEndEarly();                               │
│  foreach (AbstractModel item in ...)                                │
│      await item.BeforeTurnEnd();                                    │
└─────────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────────┐
│                 Hook Listeners (AbstractModel subclasses)            │
│                                                                      │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐  ┌───────────┐  │
│  │ PowerModel  │  │ RelicModel  │  │ CardModel   │  │ OrbModel  │  │
│  │ (Strength)  │  │ (BurningBlood)│ │ (in combat)│  │ (Lightning)│  │
│  └─────────────┘  └─────────────┘  └─────────────┘  └───────────┘  │
└─────────────────────────────────────────────────────────────────────┘
```

### Hook Categories

#### Modifier Hooks (Return modified value)
```csharp
// Additive: Sum all contributions
public virtual decimal ModifyDamageAdditive(...) => 0m;

// Multiplicative: Chain multiply
public virtual decimal ModifyDamageMultiplicative(...) => 1m;

// Cap: Take minimum
public virtual decimal ModifyDamageCap(...) => decimal.MaxValue;
```

#### Event Hooks (Return Task, async)
```csharp
// Fire and await
public virtual Task AfterCardPlayed(PlayerChoiceContext ctx, CardPlay play) => Task.CompletedTask;
public virtual Task AfterTurnEnd(PlayerChoiceContext ctx, CombatSide side) => Task.CompletedTask;
```

#### Predicate Hooks (Return bool, short-circuit)
```csharp
// First false wins
public virtual bool ShouldDie(Creature creature) => true;
public virtual bool ShouldClearBlock(CombatState state, Creature creature) => true;
```

#### Try-Modify Hooks (Return bool success + out parameter)
```csharp
// Allows modification with confirmation
public virtual bool TryModifyEnergyCostInCombat(CardModel card, int original, out decimal modified)
{
    modified = original;
    return false;
}
```

### Execution Phases

| Phase | Suffix | Order | Use Case |
|-------|--------|-------|----------|
| VeryEarly | `VeryEarly` | 1 | Buffer checks, pre-processing |
| Early | `Early` | 2 | Pre-normal processing |
| Normal | (none) | 3 | Default processing |
| Late | `Late` | 4 | Post-processing, final modifiers |

### Listener Enumeration Order

From `CombatState.IterateHookListeners()`:
1. Powers on all creatures
2. Relics (non-melted)
3. Potions (non-empty slots)
4. Orbs in orb queue
5. Cards in combat piles
6. Enchantments/Afflictions on cards
7. Run-level Modifiers

### Key Interfaces

```csharp
// Hook dispatcher (simplified)
public static class Hook
{
    public static decimal ModifyDamage(IRunState runState, CombatState combatState,
        Creature? target, Creature? dealer, decimal amount, ValueProp props,
        CardModel? cardSource, ModifyDamageHookType hookType, ...)
    {
        // Phase 1: Additive modifiers
        foreach (var item in runState.IterateHookListeners(combatState))
            amount += item.ModifyDamageAdditive(...);
        
        // Phase 2: Multiplicative modifiers
        foreach (var item in runState.IterateHookListeners(combatState))
            amount *= item.ModifyDamageMultiplicative(...);
        
        // Phase 3: Cap
        decimal cap = decimal.MaxValue;
        foreach (var item in runState.IterateHookListeners(combatState))
            cap = Math.Min(cap, item.ModifyDamageCap(...));
        
        return Math.Min(amount, cap);
    }
    
    public static async Task AfterCardPlayed(PlayerChoiceContext ctx, CardPlay play)
    {
        foreach (var item in ctx.CombatState.IterateHookListeners())
        {
            await item.AfterCardPlayed(ctx, play);
            item.InvokeExecutionFinished(); // Signal for VFX
        }
    }
}

// AbstractModel base class
public abstract class AbstractModel
{
    // Modifier hooks (default implementations)
    public virtual decimal ModifyDamageAdditive(...) => 0m;
    public virtual decimal ModifyDamageMultiplicative(...) => 1m;
    public virtual decimal ModifyDamageCap(...) => decimal.MaxValue;
    public virtual decimal ModifyBlockAdditive(...) => 0m;
    public virtual decimal ModifyBlockMultiplicative(...) => 1m;
    
    // Event hooks
    public virtual Task BeforeCombatStart() => Task.CompletedTask;
    public virtual Task AfterCardPlayed(...) => Task.CompletedTask;
    public virtual Task AfterTurnEnd(...) => Task.CompletedTask;
    
    // Predicate hooks
    public virtual bool ShouldDie(Creature creature) => true;
    public virtual bool ShouldClearBlock(...) => true;
    
    // Try-modify hooks
    public virtual bool TryModifyEnergyCostInCombat(...) => false;
}
```

## Alternatives Considered

### Alternative 1: Event Bus / C# Events

- **Description**: Use C# events (`event Action<T>`) for pub/sub
- **Pros**: Built-in, familiar pattern
- **Cons**: No execution order control, no short-circuit, harder to debug
- **Rejection Reason**: Deterministic ordering critical for multiplayer

### Alternative 2: Godot Signals

- **Description**: Use Godot's signal system for all hooks
- **Pros**: Native Godot integration
- **Cons**: No return values, no ordering, no short-circuit, must be on Node
- **Rejection Reason**: Cannot support modifier/predicate patterns

### Alternative 3: Dependency Injection

- **Description**: Inject hook processors into each system
- **Pros**: Testable, explicit dependencies
- **Cons**: Complex, verbose, performance overhead
- **Rejection Reason**: Static dispatch is simpler and faster for hot path

## Consequences

### Positive

- Single source of truth for all game modifications
- Deterministic execution order for multiplayer sync
- Clean separation: models declare hooks via virtual overrides
- Short-circuit capability for predicate hooks
- Phase system enables priority ordering without explicit priority numbers

### Negative

- All models inherit from AbstractModel (coupling)
- Hook method explosion (60+ methods)
- Debugging hook chains requires iteration tracking
- Async hooks add complexity

### Neutral

- Hook system is foundational — all gameplay systems depend on it

## Risks

| Risk | Probability | Impact | Mitigation |
|------|------------|--------|-----------|
| Hook order bugs cause subtle gameplay differences | Medium | High | Log hook execution in debug mode |
| Performance degradation with many listeners | Low | Medium | Profile combat with max powers/relics |
| Async hook deadlock | Low | High | Timeout + watchdog in GameAction queue |

## Performance Implications

| Metric | Budget | Expected |
|--------|--------|----------|
| Hook invocation overhead | < 0.1ms per hook | ~0.01ms |
| Listener enumeration | < 0.5ms | ~0.1ms (50 listeners) |
| Total combat hooks per turn | < 5ms | ~1-2ms |

## Migration Plan

Already implemented. Core system in use by all gameplay code.

## Validation Criteria

- [x] Hooks execute in deterministic phase order
- [x] Modifier hooks chain values correctly
- [x] Predicate hooks short-circuit on false
- [x] Async hooks await correctly
- [x] Listener enumeration includes all model types

## GDD Requirements Addressed

- TR-core-003: Hook system for damage/block modification
- Enables: Power System, Relic System, Card modification effects

## Related

- ADR-0001: C# as Primary Language
- ADR-0002: GameAction Async Queue System
- `src/Core/Hooks/Hook.cs` — Dispatcher implementation
- `src/Core/Models/AbstractModel.cs` — Virtual hook definitions
- `src/Core/Combat/CombatState.cs` — IterateHookListeners()
