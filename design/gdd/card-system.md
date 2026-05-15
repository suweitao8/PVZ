# Card System GDD

> **Document Type**: Game Design Document
> **System**: Cards
> **Status**: Reverse-engineered from implementation
> **Last Updated**: 2026-05-15

---

## 1. Overview

The Card System manages all aspects of cards in Slay the Spire 2: card types, properties, deck management, card play execution, and card modification (upgrades, enchantments, transformations). Cards are the primary player action mechanism during combat, consuming energy and producing effects like damage, block, and power application.

The system uses a pile-based architecture (Draw, Hand, Discard, Exhaust, Play) with a dynamic variable system for flexible card values. Cards are defined in a class hierarchy with virtual methods for customization, supporting the game's five character classes plus colorless, status, curse, and token cards.

---

## 2. Player Fantasy

Players feel like master deckbuilders, crafting synergistic combinations from diverse card types. Each card represents a tactical decision point: aggressive attacks, defensive blocks, persistent powers, or utility skills. The upgrade system rewards repeated play by enhancing favorite cards, while the keyword system (Exhaust, Retain, Innate) adds strategic depth to when and how cards are used.

The system delivers:
- **Build diversity**: Multiple viable strategies per character
- **Progressive mastery**: Upgrades improve cards over time
- **Tactical depth**: Keywords create meaningful timing decisions
- **Visual clarity**: Card frames communicate type, rarity, and class

---

## 3. Detailed Rules

### 3.1 Card Types

| Type | Description | Typical Effect |
|------|-------------|----------------|
| **Attack** | Offensive actions | Deals damage to enemies |
| **Skill** | Utility actions | Block, draw, heal, energy manipulation |
| **Power** | Persistent effects | Applies ongoing modifier to combat |
| **Status** | Debuff cards | Added to deck during combat, harmful |
| **Curse** | Unplayable debuffs | Cannot be played, clogs deck |
| **Quest** | Special cards | Quest-related mechanics |

### 3.2 Card Rarities

| Rarity | Drop Rate (Approx.) | Notes |
|--------|---------------------|-------|
| **Basic** | Starting deck | Strike, Defend, etc. |
| **Common** | ~65% | Most frequent reward |
| **Uncommon** | ~30% | More powerful, less frequent |
| **Rare** | ~5% | Powerful effects |
| **Ancient** | Special | Ancient card pool |
| **Token** | Generated | Shivs, etc. (not in rewards) |
| **Event** | Event-specific | Only from events |

### 3.3 Card Classes (Colors)

| Class | Frame Color | Play Style |
|-------|-------------|------------|
| **Ironclad** | Red (`#D62000`) | High damage, self-damage synergies |
| **Silent** | Green (`#5EBD00`) | Poison, shivs, draw synergies |
| **Defect** | Blue | Orbs, energy manipulation |
| **Necrobinder** | Purple | Death/killing synergies |
| **Regent** | Gold | Gold/resource synergies |
| **Colorless** | Gray (`#A3A3A3`) | Universal cards |

### 3.4 Card Keywords

| Keyword | Effect | Display Position |
|---------|--------|------------------|
| **Innate** | Always in opening hand | Before description |
| **Ethereal** | Exhausts at end of turn if in hand | Before description |
| **Retain** | Keep in hand at end of turn | Before description |
| **Sly** | Plays when discarded | Before description |
| **Unplayable** | Cannot be played | Before description |
| **Exhaust** | Removed from combat after play | After description |
| **Eternal** | Cannot be removed from deck | After description |

### 3.5 Card Tags

Tags enable synergies between cards:
- **Strike**: Synergizes with Strike-count effects
- **Defend**: Synergizes with Defend-count effects
- **Shiv**: Shiv-specific synergies
- **Minion**: Minion-related effects
- **OstyAttack**: Osty-specific targeting

### 3.6 Targeting System

| Target Type | Selection | Description |
|-------------|-----------|-------------|
| `None` | Automatic | No target needed |
| `Self` | Automatic | Targets player |
| `AnyEnemy` | Manual | Player selects single enemy |
| `AllEnemies` | Automatic | Targets all enemies |
| `RandomEnemy` | Automatic | Random single enemy |
| `AnyAlly` | Manual | Select single ally |
| `AllAllies` | Automatic | All allies |

### 3.7 Pile System

Cards exist in one of six piles during combat:

```
PILE FLOW DURING COMBAT:

    ┌─────────┐     Draw      ┌─────────┐
    │  DRAW   │ ─────────────→│  HAND   │
    │ (face   │                │ (max 10)│
    │  down)  │←─── Shuffle ───│         │
    └─────────┘    (if empty)  └────┬────┘
                                     │
                          Play Card  │  End Turn
                                     ↓
    ┌─────────┐              ┌─────────┐
    │ EXHAUST │←── Exhaust ──│  PLAY   │
    │         │              │ (active)│
    └─────────┘              └────┬────┘
                                  │
                         Normal   │
                         play     ↓
                           ┌─────────┐
                           │ DISCARD │
                           │         │
                           └─────────┘
```

**Pile Rules:**
- **Draw Pile**: Face-down, shuffled at combat start
- **Hand**: Active cards, max 10 (overflow → discard)
- **Discard**: Played cards, shuffled back when draw empty
- **Exhaust**: Removed from combat entirely
- **Play**: Temporary pile during card execution

### 3.8 Card Play Sequence

```
CARD PLAY FLOW:

1. SELECTION
   ├── Player selects card in hand
   ├── Validate: CanPlay() check
   │   ├── Not Unplayable keyword
   │   ├── Sufficient energy/stars
   │   ├── Valid target exists
   │   └── Hook.ShouldPlay() returns true
   └── Capture X-cost value if applicable

2. ENQUEUE
   ├── Move card to Play pile
   ├── Trigger OnEnqueuePlayVfx
   └── Record in CombatHistory

3. RESOURCE SPEND
   ├── Deduct energy: Energy -= cost
   ├── Deduct stars: Stars -= starCost
   └── Hook.AfterResourcesSpent

4. EXECUTE
   ├── Hook.BeforeCardPlayed
   ├── card.OnPlay(target)
   │   ├── Execute card actions
   │   │   ├── DamageCmd.Attack()
   │   │   ├── CreatureCmd.GainBlock()
   │   │   ├── PowerCmd.Apply()
   │   │   └── CardPileCmd.Draw()
   │   └── Apply enchantment effects
   ├── Apply affliction effects
   └── Hook.AfterCardPlayed

5. RESULT
   ├── Determine result pile
   │   ├── Exhaust keyword → Exhaust pile
   │   ├── Power → Stay in play
   │   └── Default → Discard pile
   ├── Move to result pile
   └── History.CardPlayFinished
```

### 3.9 Card Modification

#### Upgrades
- **Trigger**: `CardCmd.Upgrade(card)`
- **Behavior**:
  - Increment `CurrentUpgradeLevel`
  - Call virtual `OnUpgrade()` method
  - Recalculate dynamic variables
  - Trigger upgrade VFX
- **Max Level**: Typically 1, some cards support multi-upgrade

#### Enchantments
- **Purpose**: Persistent positive modifications
- **Behavior**:
  - `ModifyCard()` called on enchant
  - `OnPlay()` adds extra effects
  - Can add extra card text
- **Duration**: Until combat ends

#### Afflictions
- **Purpose**: Temporary negative modifications
- **Behavior**: Applied during combat, cleared after
- **Source**: Enemy powers, status effects

#### Transformations
- **Purpose**: Replace card with different type
- **Rules**:
  - Cannot transform Eternal cards
  - Preserves pile position
  - Uses same rarity by default

---

## 4. Formulas

### 4.1 Energy Cost

```
// Standard cost
effectiveCost = max(0, baseCost + localModifiers + globalModifiers)

// X-cost cards
if (costsX):
    effectiveCost = capturedXValue  // Set at play time

// Cost modifiers (additive)
localModifiers = sum(all "cost +/- N" effects on this card)

// Cost modifiers (multiplicative)
effectiveCost = floor(effectiveCost * costMultiplier)
```

### 4.2 Star Cost

```
effectiveStarCost = max(0, baseStarCost + modifiers)

// Star-to-energy overflow
if (energyCost > currentEnergy AND shouldPayExcessWithStars):
    excessEnergy = energyCost - currentEnergy
    starCost += excessEnergy * 2
    energyCost = currentEnergy
```

### 4.3 Dynamic Variable Resolution

Cards use `DynamicVar` classes for flexible values:

```
// DamageVar
damage = baseDamage + strength
damage = damage * damageMultiplier
damage = max(0, damage)

// BlockVar
block = baseBlock + dexterity
block = block * blockMultiplier
block = max(0, block)

// X-cost capture
capturedX = currentEnergy  // Default X = current energy
```

### 4.4 Upgrade Roll (Card Rewards)

```
upgradeChance = baseChance + (currentActIndex * 0.25)

// Ascension Scarcity modifier
if (ascensionScarcity):
    upgradeChance = baseChance + (currentActIndex * 0.125)
```

### 4.5 Rarity Roll (Card Rewards)

```
// Standard distribution (approximate)
roll = random(0, 100)
if (roll < rareThreshold):      // ~5%
    rarity = Rare
elif (roll < uncommonThreshold): // ~35%
    rarity = Uncommon
else:
    rarity = Common

// Modified by:
// - Relics (N'loth, etc.)
// - Ascension
// - Card pool exhaustion
```

---

## 5. Edge Cases

### 5.1 Unplayable Cards

**Reasons** (flags can combine):
- `HasUnplayableKeyword`: Card has Unplayable keyword
- `BlockedByHook`: Power/relic prevents play
- `BlockedByCardLogic`: Card-specific condition not met
- `EnergyCostTooHigh`: Insufficient energy
- `StarCostTooHigh`: Insufficient stars
- `NoLivingAllies`: Requires ally but none alive

### 5.2 Hand Full During Draw

- **Limit**: 10 cards
- **Behavior**: Overflow goes to discard
- **Visual**: "Hand Full" thought bubble

### 5.3 Draw Pile Empty

- **Trigger**: Draw when draw pile is empty
- **Resolution**: Shuffle discard → draw, then draw
- **Edge**: If both empty, no draw occurs

### 5.4 X-Cost Cards

- **Capture**: X value captured at play initiation
- **Timing**: Before energy spend
- **Zero Energy**: X = 0 if no energy available

### 5.5 Exhaust vs Discard

- **Exhaust**: Card goes to exhaust pile, removed from combat
- **Discard**: Card goes to discard, can be drawn again
- **Power**: Stays in play area, not in any pile

### 5.6 Ethereal at Turn End

- **Trigger**: Turn ends, Ethereal card in hand
- **Resolution**: Exhaust immediately
- **Order**: Before normal hand flush

### 5.7 Retain at Turn End

- **Behavior**: Card stays in hand
- **Stacking**: Multiple Retain sources don't conflict
- **Loss**: Retain can be removed by effects

### 5.8 Transformation Restrictions

- **Eternal**: Cannot transform Eternal cards
- **In Combat**: Preserves current pile position
- **Out of Combat**: Transforms in master deck

---

## 6. Dependencies

### Internal Dependencies
| System | Dependency Type | Description |
|--------|----------------|-------------|
| Combat System | Required | Combat state, turn flow |
| Hook System | Required | Card modifications, triggers |
| Energy System | Required | Resource costs |
| Damage System | Required | Attack damage execution |
| Block System | Required | Block gain |
| Power System | Required | Power application |
| RNG System | Required | Random targeting, shuffling |

### External Dependencies
| System | Dependency Type | Description |
|--------|----------------|-------------|
| UI System | Required | Card rendering, tooltips |
| Audio System | Required | Card play SFX |
| VFX System | Required | Card animations |
| Localization | Required | Card text, keywords |
| Asset System | Required | Card art, frames |

---

## 7. Tuning Knobs

| Parameter | Location | Default | Notes |
|-----------|----------|---------|-------|
| Max cards in hand | `CardPile.maxCardsInHand` | 10 | Overflow to discard |
| Max upgrade level | `CardModel.MaxUpgradeLevel` | 1 | Some cards higher |
| Upgrade chance per act | `CardFactory` | +25%/act | 12.5% with Scarcity |
| Star-to-energy rate | `PlayerCombatState` | 2:1 | Stars per excess energy |

### Rarity Thresholds (Tunable)
| Rarity | Base Threshold |
|--------|----------------|
| Rare | ~5% |
| Uncommon | ~35% |
| Common | ~65% |

---

## 8. Acceptance Criteria

### Card Properties
- [ ] Cards display correct type, rarity, and class frame
- [ ] Energy cost displays correctly with modifiers
- [ ] Keywords display in correct order
- [ ] Dynamic variables update with modifiers

### Card Play
- [ ] Cards consume correct energy when played
- [ ] Cards consume correct stars when played
- [ ] Targeting works for all target types
- [ ] Unplayable cards show correct reason
- [ ] X-cost cards capture correct value

### Pile Management
- [ ] Draw moves cards from draw to hand
- [ ] Hand overflow goes to discard
- [ ] Empty draw triggers shuffle from discard
- [ ] Exhaust cards go to exhaust pile
- [ ] Power cards stay in play area

### Card Modification
- [ ] Upgrades apply correctly
- [ ] Enchantments modify card behavior
- [ ] Afflictions apply temporary effects
- [ ] Transformations preserve pile position
- [ ] Eternal cards cannot be transformed

### Keywords
- [ ] Innate cards start in hand
- [ ] Ethereal cards exhaust at turn end
- [ ] Retain cards stay in hand at turn end
- [ ] Exhaust cards go to exhaust after play
- [ ] Unplayable cards cannot be played

---

## Appendix: Card Hierarchy

```
CardModel (abstract base)
│
├── Character Cards
│   ├── Ironclad Cards (Red)
│   │   ├── StrikeIronclad
│   │   ├── DefendIronclad
│   │   ├── Bash
│   │   └── ...
│   ├── Silent Cards (Green)
│   │   ├── StrikeSilent
│   │   ├── DefendSilent
│   │   ├── Neutralize
│   │   └── ...
│   ├── Defect Cards (Blue)
│   ├── Necrobinder Cards
│   └── Regent Cards
│
├── Colorless Cards
│   └── Universal cards
│
├── Generated Cards (Token)
│   ├── Shiv
│   └── ...
│
├── Status Cards
│   ├── Slimed
│   ├── Wound
│   └── ...
│
├── Curse Cards
│   ├── CurseOfPain
│   └── ...
│
└── Testing Cards
    └── MockCardModel variants
```

---

## Appendix: Virtual Methods for Card Definition

When creating a new card, override these properties:

```csharp
// Required
protected override int CanonicalEnergyCost { get; }
protected override CardType Type { get; }
protected override CardRarity Rarity { get; }
protected override TargetType TargetType { get; }

// Optional
protected override bool HasEnergyCostX { get; }  // Default: false
protected override int CanonicalStarCost { get; }  // Default: 0
protected override bool HasStarCostX { get; }  // Default: false
protected override IEnumerable<CardKeyword> CanonicalKeywords { get; }
protected override HashSet<CardTag> CanonicalTags { get; }
protected override IEnumerable<DynamicVar> CanonicalVars { get; }

// Behavior
protected override Task OnPlay(PlayerChoiceContext, CardPlay);
protected override void OnUpgrade();
protected override PileType GetResultPileType();  // Default: Discard
```
