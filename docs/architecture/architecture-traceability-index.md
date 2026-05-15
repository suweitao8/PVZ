# Architecture Traceability Index

> **Last Updated**: 2026-05-15
> **Engine**: Godot 4.5.1 ✅
> **Review**: architecture-review-2026-05-15.md

---

## Coverage Summary

| Metric | Count | Percentage |
|--------|-------|------------|
| **Total Requirements** | 28 | 100% |
| **✅ Covered** | 22 | 79% |
| **⚠️ Partial** | 2 | 7% |
| **❌ Gaps** | 4 | 14% |

---

## ADRs Indexed

| ADR | Title | Status | Domain | Dependencies |
|-----|-------|--------|--------|--------------|
| [ADR-0001](adr-0001-csharp-primary-language.md) | C# as Primary Language | Accepted | Scripting | Foundation |
| [ADR-0002](adr-0002-gameaction-async-queue.md) | GameAction Async Queue | Accepted | Core | ADR-0001 |
| [ADR-0003](adr-0003-megaspine-binding-pattern.md) | MegaSpine Binding Pattern | Accepted | Animation | ADR-0001 |
| [ADR-0004](adr-0004-combatmanager-singleton.md) | CombatManager Singleton | Accepted | Core | ADR-0001, ADR-0002 |
| [ADR-0005](adr-0005-fmod-audio-integration.md) | FMOD Audio Integration | Accepted | Audio | ADR-0001 |
| [ADR-0006](adr-0006-hook-system.md) | Hook System for Game Event Modification | Accepted | Core | ADR-0001, ADR-0002 |
| [ADR-0007](adr-0007-ui-rendering.md) | UI Rendering System | Accepted | UI | ADR-0001 |
| [ADR-0008](adr-0008-save-persistence.md) | Save/Persistence System | Accepted | Persistence | ADR-0001, ADR-0004 |

---

## Dependency Graph

```
ADR-0001 (Foundation)
    │
    ├──→ ADR-0002 ──→ ADR-0004 ──→ ADR-0008
    │                     │
    │                     └──→ ADR-0006
    │
    ├──→ ADR-0003
    │
    ├──→ ADR-0005
    │
    └──→ ADR-0007
```

**Order**: ADR-0001 → (ADR-0002, ADR-0003, ADR-0005, ADR-0007) → ADR-0004 → (ADR-0006, ADR-0008)

---

## Full Traceability Matrix

| TR-ID | GDD | System | Requirement | ADR Coverage | Status |
|-------|-----|--------|-------------|--------------|--------|
| TR-core-001 | combat.md | Core | CombatManager singleton for combat state | ADR-0004 | ✅ |
| TR-core-002 | combat.md | Core | GameAction async queue for sequencing | ADR-0002 | ✅ |
| TR-core-003 | combat.md | Core | Hook system for damage/block modification | ADR-0006 | ✅ |
| TR-core-004 | card.md | Core | Card pile management (Draw/Hand/Discard) | — | ⚠️ |
| TR-core-005 | enemy.md | Core | Monster state machine for AI | — | ⚠️ |
| TR-core-006 | power.md | Core | Power hook system for modifiers | ADR-0006 | ✅ |
| TR-lang-001 | *all* | Scripting | C# as primary language | ADR-0001 | ✅ |
| TR-lang-002 | *all* | Scripting | Async/await support | ADR-0001, ADR-0002 | ✅ |
| TR-anim-001 | enemy.md | Animation | Creature animation system | ADR-0003 | ✅ |
| TR-anim-002 | combat.md | Animation | VFX for damage/block effects | ADR-0003 | ✅ |
| TR-audio-001 | combat.md | Audio | SFX for combat events | ADR-0005 | ✅ |
| TR-audio-002 | *all* | Audio | Dynamic audio via FMOD | ADR-0005 | ✅ |
| TR-ui-001 | card.md | UI | Card rendering and tooltips | ADR-0007 | ✅ |
| TR-ui-002 | enemy.md | UI | Intent display system | ADR-0007 | ✅ |
| TR-ui-003 | run-map.md | UI | Map screen and path selection | — | ❌ |
| TR-save-001 | run-map.md | Persistence | Run state persistence | ADR-0008 | ✅ |
| TR-rng-001 | run-map.md | Core | Deterministic RNG system | ADR-0008 | ✅ |
| TR-multi-001 | combat.md | Multiplayer | Multiplayer HP scaling | ADR-0004 | ✅ |
| TR-multi-002 | combat.md | Multiplayer | Player readiness tracking | ADR-0004 | ✅ |
| TR-relic-001 | relic.md | Core | Relic hook integration | ADR-0006 | ✅ |
| TR-potion-001 | potion.md | Core | Potion slot management | — | ⚠️ |
| TR-event-001 | run-map.md | Core | Event system for rooms | — | ⚠️ |
| TR-shop-001 | run-map.md | Core | Shop/merchant system | — | ⚠️ |
| TR-deck-001 | card.md | Core | Deck building and management | — | ⚠️ |
| TR-reward-001 | combat.md | Core | Reward system (gold, cards, relics) | — | ⚠️ |
| TR-ascension-001 | enemy.md | Core | Ascension difficulty scaling | — | ⚠️ |
| TR-neow-001 | run-map.md | Core | Neow starting bonus system | — | ⚠️ |
| TR-history-001 | combat.md | Core | Combat history for replay | ADR-0002, ADR-0004 | ✅ |

---

## Known Gaps

### Missing ADRs (Required)

| TR-ID | System | Suggested ADR | Priority |
|-------|--------|---------------|----------|
| TR-ui-003 | Map Screen | `/architecture-decision map-screen` | HIGH |

### Partial Coverage (Implicit)

These systems are implemented but lack dedicated ADR:

- Card Pile Management (TR-core-004)
- Monster AI State Machine (TR-core-005)
- Event System (TR-event-001)
- Shop System (TR-shop-001)

---

## Superseded Requirements

None — no requirements have been superseded.

---

## Review History

| Date | Verdict | Notes |
|------|---------|-------|
| 2026-05-15 | CONCERNS | Initial review — 5 ADRs, 7 GDDs; 28 TRs extracted; 64% covered |
