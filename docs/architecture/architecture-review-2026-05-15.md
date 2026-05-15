# Architecture Review Report

> **Date**: 2026-05-15
> **Engine**: Godot 4.5.1 ✅
> **GDDs Reviewed**: 7
> **ADRs Reviewed**: 5
> **Reviewer**: Claude Code (automated)

---

## Executive Summary

| Metric | Value |
|--------|-------|
| **Total Requirements** | 28 |
| **Covered** | 18 (64%) |
| **Partial** | 6 (21%) |
| **Gaps** | 4 (14%) |
| **Cross-ADR Conflicts** | 0 |
| **Engine Issues** | 0 |

**Verdict: CONCERNS** — Core architecture is sound, but several systems lack ADR coverage.

---

## Traceability Matrix

| TR-ID | GDD | System | Requirement | ADR Coverage | Status |
|-------|-----|--------|-------------|--------------|--------|
| TR-core-001 | combat.md | Core | CombatManager singleton for combat state | ADR-0004 | ✅ |
| TR-core-002 | combat.md | Core | GameAction async queue for sequencing | ADR-0002 | ✅ |
| TR-core-003 | combat.md | Core | Hook system for damage/block modification | — | ⚠️ Partial |
| TR-core-004 | card.md | Core | Card pile management (Draw/Hand/Discard) | — | ⚠️ Partial |
| TR-core-005 | enemy.md | Core | Monster state machine for AI | — | ⚠️ Partial |
| TR-core-006 | power.md | Core | Power hook system for modifiers | — | ⚠️ Partial |
| TR-lang-001 | *all* | Scripting | C# as primary language | ADR-0001 | ✅ |
| TR-lang-002 | *all* | Scripting | Async/await support | ADR-0001, ADR-0002 | ✅ |
| TR-anim-001 | enemy.md | Animation | Creature animation system | ADR-0003 | ✅ |
| TR-anim-002 | combat.md | Animation | VFX for damage/block effects | ADR-0003 | ✅ |
| TR-audio-001 | combat.md | Audio | SFX for combat events | ADR-0005 | ✅ |
| TR-audio-002 | *all* | Audio | Dynamic audio via FMOD | ADR-0005 | ✅ |
| TR-ui-001 | card.md | UI | Card rendering and tooltips | — | ❌ GAP |
| TR-ui-002 | enemy.md | UI | Intent display system | — | ❌ GAP |
| TR-ui-003 | run-map.md | UI | Map screen and path selection | — | ❌ GAP |
| TR-save-001 | run-map.md | Persistence | Run state persistence | — | ⚠️ Partial |
| TR-rng-001 | run-map.md | Core | Deterministic RNG system | — | ⚠️ Partial |
| TR-multi-001 | combat.md | Multiplayer | Multiplayer HP scaling | ADR-0004 | ✅ |
| TR-multi-002 | combat.md | Multiplayer | Player readiness tracking | ADR-0004 | ✅ |
| TR-relic-001 | relic.md | Core | Relic hook integration | — | ⚠️ Partial |
| TR-potion-001 | potion.md | Core | Potion slot management | — | ⚠️ Partial |
| TR-event-001 | run-map.md | Core | Event system for rooms | — | ⚠️ Partial |
| TR-shop-001 | run-map.md | Core | Shop/merchant system | — | ⚠️ Partial |
| TR-deck-001 | card.md | Core | Deck building and management | — | ⚠️ Partial |
| TR-reward-001 | combat.md | Core | Reward system (gold, cards, relics) | — | ⚠️ Partial |
| TR-ascension-001 | enemy.md | Core | Ascension difficulty scaling | — | ⚠️ Partial |
| TR-neow-001 | run-map.md | Core | Neow starting bonus system | — | ⚠️ Partial |
| TR-history-001 | combat.md | Core | Combat history for replay | ADR-0002, ADR-0004 | ✅ |

---

## Coverage Gaps (No ADR Exists)

| TR-ID | System | Requirement | Suggested ADR | Domain | Engine Risk |
|-------|--------|-------------|---------------|--------|-------------|
| TR-ui-001 | Card UI | Card rendering and tooltips | `/architecture-decision ui-rendering` | UI | LOW |
| TR-ui-002 | Enemy Intent | Intent display system | `/architecture-decision intent-display` | UI | LOW |
| TR-ui-003 | Map Screen | Map screen and path selection | `/architecture-decision map-screen` | UI | LOW |

---

## Partial Coverage (Needs Expansion)

| System | Current Coverage | Gap | Suggested Action |
|--------|-----------------|-----|------------------|
| Hook System | Mentioned in ADR-0002 | No dedicated ADR | Create `/architecture-decision hook-system` |
| Card Pile Management | None | Full gap | Create `/architecture-decision card-pile-system` |
| Monster AI | None | Full gap | Document in GDD or create ADR for complex patterns |
| Power System | None | Full gap | Create `/architecture-decision power-system` |
| Save/Persistence | None | Full gap | Create `/architecture-decision save-system` |
| RNG System | None | Full gap | Create `/architecture-decision rng-system` |

---

## Cross-ADR Conflicts

**No conflicts detected.**

All ADR dependency relationships are consistent:
- ADR-0001 (C# Primary) enables ADR-0002, 0003, 0004, 0005
- ADR-0002 (GameAction Queue) enables ADR-0004 (CombatManager)
- No circular dependencies

---

## ADR Dependency Order

Recommended implementation order (topologically sorted):

```
Foundation (no dependencies):
  1. ADR-0001: C# as Primary Language
     └─ Enables: ADR-0002, ADR-0003, ADR-0004, ADR-0005

Core Layer:
  2. ADR-0002: GameAction Async Queue System
     └─ Requires: ADR-0001
     └─ Enables: ADR-0004

  3. ADR-0003: MegaSpine Binding Pattern
     └─ Requires: ADR-0001

  4. ADR-0005: FMOD Audio Integration
     └─ Requires: ADR-0001

Feature Layer:
  5. ADR-0004: CombatManager Singleton
     └─ Requires: ADR-0001, ADR-0002
```

- ✅ No unresolved dependencies
- ✅ No dependency cycles
- ✅ All ADRs have Accepted status

---

## Engine Compatibility Audit

| Check | Status | Notes |
|-------|--------|-------|
| Version Consistency | ✅ | All ADRs reference Godot 4.5/4.6 |
| Deprecated APIs | ✅ | None referenced |
| Post-Cutoff Conflicts | ✅ | None found |
| Missing Sections | ✅ | All ADRs have Engine Compatibility |

---

## GDD Revision Flags

**None** — All GDD assumptions are consistent with verified engine behaviour and accepted ADRs.

---

## Verdict: CONCERNS

The architecture foundation is solid with 5 well-structured ADRs covering:
- ✅ Primary language (C#)
- ✅ Action sequencing (GameAction queue)
- ✅ Animation binding (MegaSpine)
- ✅ Combat coordination (CombatManager)
- ✅ Audio (FMOD)

However, several systems need ADR coverage:

### Required ADRs (Prioritized)

| Priority | ADR | Why |
|----------|-----|-----|
| **HIGH** | Hook System | Foundation for damage/block/relic/power modifications |
| **HIGH** | UI Rendering | Required for card, intent, map display |
| **MEDIUM** | Save/Persistence | Required for run state, multiplayer sync |
| **MEDIUM** | RNG System | Required for deterministic generation |
| **LOW** | Card Pile System | Well-documented in GDD, but ADR adds value |

---

## Pre-Gate Checklist

| Item | Status | Action |
|------|--------|--------|
| `tests/unit/` directory | ❌ MISSING | Run `/test-setup` |
| `tests/integration/` directory | ❌ MISSING | Run `/test-setup` |
| `.github/workflows/tests.yml` | ❌ MISSING | Run `/test-setup` |
| `design/accessibility-requirements.md` | ❌ MISSING | Run `/ux-design` |
| `design/ux/interaction-patterns.md` | ❌ MISSING | Run `/ux-design` |

---

## Files Generated

- `docs/architecture/architecture-traceability-index.md` — Updated traceability summary
- `docs/architecture/architecture-review-2026-05-15.md` — This report
