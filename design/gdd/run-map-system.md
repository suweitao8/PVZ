# Run & Map System GDD

> **Document Type**: Game Design Document
> **System**: Run Progression & Map
> **Status**: Reverse-engineered from implementation
> **Last Updated**: 2026-05-15

---

## 1. Overview

The Run & Map System governs the roguelike progression in Slay the Spire 2. Each run consists of 3 acts with procedurally generated maps. Players navigate branching paths through various room types (Combat, Elite, Shop, Rest, Event, Boss), building their deck and collecting relics until victory or death.

The map uses a grid-based structure with 7 columns and act-defined rows. Paths branch and merge, offering strategic route selection. The system supports deterministic RNG for replayability and multiplayer synchronization.

---

## 2. Player Fantasy

Players feel like strategic navigators, choosing paths that match their current build state. Low HP? Take the rest site route. Need relics? Risk the elite path. Strong deck? Rush the boss. Each run offers a unique journey through the spire, with meaningful choices at every branch.

The system delivers:
- **Strategic routing**: Path selection matters
- **Risk/reward**: Elite paths offer better rewards
- **Adaptive planning**: Route based on deck state
- **Replayability**: Procedural generation ensures variety

---

## 3. Detailed Rules

### 3.1 Run Structure

```
RUN OVERVIEW:

┌─────────────────────────────────────────────────────┐
│                    ACT 1                            │
│  Overgrowth - Jungle/Nature Theme                   │
│  ┌───┐                                              │
│  │ N │ ← Neow (Starting Ancient)                    │
│  └─┬─┘                                              │
│    │     ┌───┐  ┌───┐  ┌───┐                        │
│    ├─────│ ? │──│ ? │──│ ? │ ... (15 rooms)         │
│    │     └───┘  └───┘  └───┘                        │
│    │         Branching paths                         │
│    │     ┌───┐                                       │
│    └─────│ R │ ← Rest Site (before boss)            │
│          └─┬─┘                                       │
│            │                                         │
│          ┌─▼─┐                                       │
│          │ B │ ← Boss                                │
│          └───┘                                       │
└─────────────────────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────┐
│                    ACT 2                            │
│  Hive - Insect/Bug Theme (14 rooms)                 │
└─────────────────────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────┐
│                    ACT 3                            │
│  Glory - Construct/Machine Theme (13 rooms)         │
│  Victory on Boss defeat                             │
└─────────────────────────────────────────────────────┘
```

### 3.2 Act Properties

| Property | Act 1 | Act 2 | Act 3 |
|----------|-------|-------|-------|
| **Name** | Overgrowth | Hive | Glory |
| **Theme** | Jungle/Nature | Insect/Bug | Construct |
| **Base Rooms** | 15 | 14 | 13 |
| **Weak Encounters** | 3 | 2 | 2 |
| **Rest Sites** | 6-7 | 6-7 | 5-7 |
| **Ancients** | 1 (Neow) | 3 | 3 |

### 3.3 Map Generation

```
MAP GENERATION ALGORITHM:

1. CREATE GRID
   - Width: 7 columns (0-6)
   - Height: BaseRooms + 1 (for boss)

2. GENERATE PATHS
   - Create 7 random starting points in row 1
   - For each start, generate path forward:
     - Can go: left-forward, straight, right-forward
     - No crossing paths allowed
     - Continue until boss row

3. CONNECT BOSS
   - Connect all end-row nodes to Boss node

4. ASSIGN ROOM TYPES
   - Fixed positions (Rest before boss, etc.)
   - Random distribution for others
   - No same-type adjacent

5. POST-PROCESS
   - Center grid
   - Spread adjacent points
   - Straighten paths
```

### 3.4 Room Types

| Type | Description | Rewards |
|------|-------------|---------|
| **Monster** | Normal combat | Gold, card, potion chance |
| **Elite** | Harder combat | Gold, card, relic, potion |
| **Boss** | Act boss | Gold, relic, next act |
| **Shop** | Merchant | Buy cards/relics/potions |
| **Treasure** | Chest room | Free relic |
| **Rest Site** | Rest options | Heal, Smith, other |
| **Event** | Random event | Variable |
| **Ancient** | Ancient event | Neow, special rewards |
| **Unknown** | Mystery | Monster or Event |

### 3.5 Room Distribution

| Room Type | Count | Notes |
|-----------|-------|-------|
| **Rest Sites** | 5-7 | Act-dependent |
| **Shops** | 3 | Fixed |
| **Elites** | 5 (×1.6 if Swarming) | Ascension modifier |
| **Unknown** | 9-14 | Act-dependent |
| **Treasure** | 1 | Fixed position |
| **Monsters** | Remaining | Fills rest |

### 3.6 Placement Rules

**Fixed Positions:**
- Row before Boss → Rest Site
- 7 rows before Boss → Treasure
- First combat row → Monster
- Boss row → Boss
- Start → Ancient

**Restrictions:**
- Rest Site/Elite: Not in first 5 rows
- Rest Site: Not in last 3 rows
- No same-type adjacent (parent/child/sibling)

### 3.7 Neow System

Neow offers starting bonuses at Act 1 start:

**Option Categories:**

| Category | Examples | Trade-off |
|----------|----------|-----------|
| **Positive Relics** | Golden Pearl, Pomander | Pure benefit |
| **Curse-Attached** | Cursed Pearl, Large Capsule | Benefit + curse |
| **Conditional** | Nutritious Oyster, Lava Rock | Requires condition |

**Selection Rules:**
- 2-3 options offered randomly
- Player picks 1
- Positive + curse options mutually exclusive

### 3.8 Run State Persistence

**Saved Data:**
- Players (deck, relics, HP, gold, potions)
- Current act and map position
- Visited coordinates history
- Events seen (avoid repetition)
- RNG state (deterministic)
- Ascension level
- Run modifiers

---

## 4. Formulas

### 4.1 Floor Calculation

```
actFloor = currentRow + 1  // 1-based within act
totalFloor = sum(allPreviousActs.rooms) + actFloor
```

### 4.2 Elite Count (Swarming Ascension)

```
baseElites = 5
if (hasSwarmingAscension):
    eliteCount = round(baseElites × 1.6)  // ~8 elites
```

### 4.3 Rest Site Count

```
restSites = act switch {
    Act1: random(6, 7),
    Act2: random(6, 7),
    Act3: random(5, 7)
}
```

### 4.4 Unknown Count

```
unknown = act switch {
    Act1: random(10, 14),
    Act2: random(9, 13),
    Act3: random(9, 13)
}
```

---

## 5. Edge Cases

### 5.1 Path Crossings

- **Issue**: Diagonal paths crossing is invalid
- **Resolution**: Generation algorithm prevents crossings
- **Fallback**: Re-generate if invalid configuration

### 5.2 All Paths Visited

- **Issue**: Player visited all nodes in act
- **Resolution**: Should not happen (single path per run)
- **Design**: Only one path traversable per act

### 5.3 Boss Discovery Order

- **Issue**: New players need guided boss progression
- **Resolution**: First-time players see bosses in fixed order
- **After**: Random selection from pool

### 5.4 Double Boss Ascension

- **Trigger**: AscensionLevel.DoubleBoss active
- **Resolution**: Second boss after first
- **Victory**: Must defeat both

### 5.5 Mid-Combat Save

- **Trigger**: Save during combat
- **Resolution**: Pre-finished room state saved
- **Recovery**: Resume combat state exactly

### 5.6 Multiplayer Map Voting

- **Issue**: Multiple players need to agree on path
- **Resolution**: Voting system for branch selection
- **Tie**: Host decides or random

---

## 6. Dependencies

### Internal Dependencies
| System | Dependency Type | Description |
|--------|----------------|-------------|
| Combat System | Required | Combat rooms |
| Event System | Required | Event rooms |
| Shop System | Required | Shop rooms |
| RNG System | Required | Deterministic generation |
| Save System | Required | Run persistence |

### External Dependencies
| System | Dependency Type | Description |
|--------|----------------|-------------|
| UI System | Required | Map display, path selection |
| Audio System | Required | Map/room sounds |
| VFX System | Required | Map transitions |

---

## 7. Tuning Knobs

### Act Configuration
| Parameter | Act 1 | Act 2 | Act 3 |
|-----------|-------|-------|-------|
| Base rooms | 15 | 14 | 13 |
| Weak encounters | 3 | 2 | 2 |
| Events | 13 | 10 | 7 |

### Room Distribution
| Room Type | Base Count | Modifier |
|-----------|------------|----------|
| Rest Sites | 5-7 | Act-dependent |
| Shops | 3 | Fixed |
| Elites | 5 | ×1.6 Swarming |
| Treasure | 1 | Fixed |

### Placement Rules
| Room Type | Min Row | Max Row |
|-----------|---------|---------|
| Elite | 5 | Boss-1 |
| Rest Site | 5 | Boss-3 |

---

## 8. Acceptance Criteria

### Map Generation
- [ ] Maps generate with correct room counts
- [ ] Paths don't cross
- [ ] All paths reach boss
- [ ] Fixed positions correct
- [ ] No same-type adjacent

### Room Placement
- [ ] Rest site before boss
- [ ] Treasure at correct position
- [ ] Elites not too early
- [ ] Rest sites not too late

### Run Progression
- [ ] Act transitions work correctly
- [ ] Boss defeat advances act
- [ ] Act 3 boss = victory
- [ ] Double boss works correctly

### Persistence
- [ ] Run saves correctly
- [ ] Mid-combat save works
- [ ] Run resumes correctly
- [ ] RNG state preserved

---

## Appendix: Room Type Hierarchy

```
RoomModel (abstract)
│
├── CombatRoom
│   ├── Normal combat (Monster)
│   ├── Elite combat
│   └── Boss combat
│
├── MerchantRoom
│   └── Shop screen
│
├── TreasureRoom
│   └── Chest with relic
│
├── RestSiteRoom
│   └── Rest options (Heal, Smith, etc.)
│
├── EventRoom
│   └── Random event
│
├── AncientRoom
│   └── Ancient event (Neow, etc.)
│
└── MapRoom
    └── Virtual room for map screen
```

---

## Appendix: Neow Options Reference

| Option | Effect | Category |
|--------|--------|----------|
| **Golden Pearl** | +100 Gold | Positive |
| **Pomander** | +1 Strength | Positive |
| **Arcane Scroll** | +1 Energy | Positive |
| **Booming Conch** | +1 Dexterity | Positive |
| **New Leaf** | +10 Max HP | Positive |
| **Cursed Pearl** | +150 Gold, add Curse | Curse-Attached |
| **Large Capsule** | +2 Card rewards, add Curse | Curse-Attached |
| **Precarious Shears** | Remove 2 cards, add Curse | Curse-Attached |
| **Nutritious Oyster** | Heal 30% if below 50% HP | Conditional |
| **Lava Rock** | +2 Strength if no attacks | Conditional |
