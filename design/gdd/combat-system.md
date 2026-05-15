# Combat System GDD

> **Document Type**: Game Design Document
> **System**: Combat
> **Status**: Reverse-engineered from implementation
> **Last Updated**: 2026-05-15

---

## 1. Overview

The Combat System is the core gameplay loop of Slay the Spire 2. Players engage in turn-based battles against enemy encounters, managing energy, playing cards from their hand, and coordinating damage and block to survive. The system supports both single-player and multiplayer modes with HP scaling for balance.

Combat flows through discrete phases: combat setup, player turn (play cards, spend energy), enemy turn (enemies execute moves), and round resolution. All modifications flow through a centralized hook system enabling clean separation between core mechanics and power/relic effects.

---

## 2. Player Fantasy

Players feel like tactical deckbuilding masters, carefully managing limited resources (energy, cards) to survive increasingly challenging encounters. Each turn presents meaningful decisions: when to play defensively (gain block), when to attack aggressively, and when to hold cards for future turns.

The system rewards:
- **Strategic planning**: Reading enemy intents and preparing counters
- **Resource management**: Optimizing energy expenditure each turn
- **Risk assessment**: Balancing offense vs defense
- **Deck synergy**: Leveraging card combinations and power synergies

---

## 3. Detailed Rules

### 3.1 Combat Initialization

1. **Combat Setup**: When entering a combat room:
   - Create creatures for all enemies based on encounter definition
   - Create creatures for all players with HP scaling for multiplayer
   - Assign unique combat IDs to all creatures
   - Fire `BeforeCombatStart` hooks (relics, powers trigger)

2. **Combat Start Banner**: Display encounter name/intro

3. **Round 1 Begins**: Start player turn

### 3.2 Turn Order

Combat progresses in rounds, each containing a player turn and enemy turn:

```
ROUND STRUCTURE:
┌─────────────────────────────────────┐
│  PLAYER TURN                        │
│  1. BeforeTurnStart (all creatures) │
│  2. Reset Energy                    │
│  3. Draw 5 cards                    │
│  4. Play Phase (player acts)        │
│  5. End Turn                        │
│     - Phase 1: Turn-end effects     │
│     - Phase 2: Hand flush to discard│
└─────────────────────────────────────┘
                 ↓
┌─────────────────────────────────────┐
│  ENEMY TURN                         │
│  1. Each enemy performs move        │
│  2. End-of-turn cleanup for players │
│  3. Check win/lose conditions       │
└─────────────────────────────────────┘
                 ↓
        (Round number increments)
```

### 3.3 Player Turn

#### Turn Start Sequence
1. **BeforeTurnStart** called on all creatures (player and enemy)
2. **Reset Energy**: Energy = MaxEnergy (default 3)
3. **BeforeHandDraw** hooks fire
4. **Draw Cards**: Base 5 cards (modified by hooks)
   - Round 1: Innate cards placed on top of draw pile
   - Hand limit: 10 cards (overflow sent to discard)
5. **AfterPlayerTurnStart** hooks fire
6. **Play Phase** begins - player can act

#### Play Phase Actions
- **Play Card**: Spend energy/stars, execute card effects
- **Use Potion**: Consume potion for effect
- **End Turn**: Pass to enemy turn

#### End Turn Sequence
1. **Player signals ready**: `SetReadyToEndTurn()`
2. **Wait for action queue**: All queued actions complete
3. **Phase One**:
   - `BeforeTurnEnd` hooks
   - Orb queue ticks
   - Ethereal cards exhaust
   - End-of-turn card effects
4. **Phase Two**:
   - `AboutToSwitchToEnemyTurn` event
   - Flush hand to discard (unless Retain keyword)
   - `AfterTurnEnd` hooks
5. **Check extra turns** (powers/relics may grant)
6. **Switch sides** to enemy

### 3.4 Enemy Turn

1. **Each enemy in order**:
   - Perform intent visualization
   - Execute `Monster.PerformMove()`
   - Check win condition after each

2. **End of enemy turn**:
   - `BeforeTurnEnd` hooks
   - End-of-turn cleanup for players
   - `AfterTurnEnd` hooks

3. **Switch sides** to player, increment round

### 3.5 Damage Resolution

Damage flows through a multi-stage pipeline:

```
DAMAGE PIPELINE:
1. Base Damage Amount
        ↓
2. Hook.ModifyDamage (additive)
   - Strength, Weak, etc.
        ↓
3. Hook.ModifyDamage (multiplicative)
   - Double damage, etc.
        ↓
4. Hook.BeforeDamageReceived
        ↓
5. Block Absorption
   - blockedDamage = min(Block, damage)
   - Block -= blockedDamage
   - (Bypassed if Unblockable flag)
        ↓
6. HP Loss Calculation
   - unblockedDamage = damage - blockedDamage
        ↓
7. Hook.ModifyHpLostBeforeOsty
        ↓
8. Target Redirection (e.g., Osty pet)
        ↓
9. Hook.ModifyHpLostAfterOsty
        ↓
10. Apply HP Loss
    - creature.HP -= unblockedDamage
    - Track overkill damage
        ↓
11. Record in CombatHistory
        ↓
12. Hook.AfterDamageGiven/Received
        ↓
13. Kill check (if HP <= 0)
```

### 3.6 Block System

- **Block Cap**: Maximum 999 block
- **Block Gain**: Modified by Dexterity (additive) and multipliers
- **Block Consumption**: Damage reduces block before affecting HP
- **Block Clear**: Block clears at start of player turn (unless prevented by powers)
- **Unblockable Damage**: Bypasses block entirely

### 3.7 Death & Win/Lose Conditions

#### Player Death
- When HP reaches 0, check `ShouldDie` hooks
- Death prevention powers may trigger (e.g., Fossilized Helix)
- If all players die → Combat Lost
- Dead player auto-ends their turn

#### Enemy Death
- Remove from combat
- Check win condition

#### Win Condition
All enemies defeated → Combat Won

#### Lose Condition
All players dead → Combat Lost

---

## 4. Formulas

### 4.1 Damage Calculation

```
// Base formula
finalDamage = baseDamage

// Additive modifiers
finalDamage += strength    // Positive = more damage
finalDamage -= weak        // Weak reduces damage (typically 25% or flat)

// Multiplicative modifiers (applied after additive)
finalDamage = finalDamage * damageMultiplier

// Unpowered (ignores strength)
if (unpoweredFlag):
    finalDamage = baseDamage

// Minimum floor
finalDamage = max(0, finalDamage)
```

### 4.2 Block Calculation

```
// Block gain
finalBlock = baseBlock + dexterity
finalBlock = finalBlock * blockMultiplier
finalBlock = max(0, finalBlock)
finalBlock = min(finalBlock, 999)  // Cap
```

### 4.3 Damage vs Block

```
if (unblockableFlag):
    blockedDamage = 0
    unblockedDamage = damage
else:
    blockedDamage = min(creature.Block, damage)
    unblockedDamage = damage - blockedDamage
    creature.Block -= blockedDamage
```

### 4.4 HP Loss

```
hpLost = max(0, unblockedDamage)
overkill = max(0, unblockedDamage - creature.CurrentHp)
creature.CurrentHp -= hpLost
```

### 4.5 Multiplayer HP Scaling

```
scaledHp = baseHp * playerCount * multiplayerScalingFactor
```

### 4.6 Star-to-Energy Conversion

```
// When energy cost exceeds current energy
// Players can pay excess with stars
excessEnergyCost = energyCost - currentEnergy
starCost += excessEnergyCost * 2  // 2 stars per 1 energy
```

---

## 5. Edge Cases

### 5.1 Hand Full During Draw

- **Limit**: 10 cards maximum in hand
- **Behavior**: Overflow cards go directly to discard pile
- **Feedback**: "Hand Full" thought bubble displayed

### 5.2 Draw Pile Empty

- **Trigger**: Attempting to draw when draw pile is empty
- **Resolution**: Shuffle discard pile into draw pile, then draw
- **Notification**: "Draw pile shuffled" visual

### 5.3 Death Prevention

- **Hook**: `ShouldDie` can return a preventer (power/relic)
- **Result**: HP stays at 1 or prevented value
- **Follow-up**: `AfterPreventingDeath` hook fires

### 5.4 Extra Turns

- **Source**: Powers/relics can grant extra player turns
- **Behavior**: Player takes another full turn before enemy turn
- **Hook**: `ShouldTakeExtraTurn` / `AfterTakingExtraTurn`

### 5.5 Block Retention

- **Default**: Block clears at turn start
- **Exception**: Powers can prevent block clear
- **Hook**: `ShouldClearBlock` returns false with preventer

### 5.6 Pending Loss State

- **Trigger**: Player dies but combat hasn't ended yet
- **Behavior**: Loss is pending, shows Game Over screen
- **Recovery**: Some effects can prevent/undo pending loss

### 5.7 Ethereal Cards at Turn End

- **Trigger**: Turn ends with Ethereal card in hand
- **Resolution**: Card exhausts immediately
- **Order**: Before hand flush to discard

---

## 6. Dependencies

### Internal Dependencies
| System | Dependency Type | Description |
|--------|----------------|-------------|
| Card System | Required | Cards are played during combat |
| Hook System | Required | All modifications flow through hooks |
| Action Queue | Required | Actions are queued and executed |
| RNG System | Required | Deterministic random for shuffling, targeting |
| Energy System | Required | Resource management for card play |

### External Dependencies
| System | Dependency Type | Description |
|--------|----------------|-------------|
| Run State | Required | Encounters, player data, act progression |
| UI System | Required | Combat screen, card rendering, animations |
| Audio System | Required | SFX for damage, block, cards |
| VFX System | Required | Hit effects, block effects, card effects |
| FMOD | Required | Audio middleware |
| Sentry | Required | Crash reporting |

---

## 7. Tuning Knobs

| Parameter | Location | Default Value | Notes |
|-----------|----------|---------------|-------|
| Base hand draw | `CombatManager.baseHandDrawCount` | 5 | Cards drawn per turn |
| Max cards in hand | `CardPile.maxCardsInHand` | 10 | Overflow to discard |
| Max block | `Creature.GainBlockInternal` | 999 | Hard cap |
| Max HP | `Creature.SetMaxHpInternal` | 999,999,999 | Hard cap |
| Default max energy | Player definition | 3 | Gained at turn start |
| Star-to-energy rate | `PlayerCombatState` | 2:1 | Stars per excess energy |
| Multiplayer scaling | `MultiplayerScalingModel` | Per-act | HP multiplier |

---

## 8. Acceptance Criteria

### Core Combat Loop
- [ ] Combat initializes with correct encounter setup
- [ ] Players draw correct number of cards at turn start
- [ ] Energy resets correctly at turn start
- [ ] Cards can be played with correct resource deduction
- [ ] Damage applies correctly (block → HP)
- [ ] Turn order follows player → enemy → round++
- [ ] Win condition triggers when all enemies defeated
- [ ] Lose condition triggers when all players dead

### Damage/Block
- [ ] Damage calculation respects all modifiers
- [ ] Block correctly absorbs damage before HP loss
- [ ] Unblockable damage bypasses block
- [ ] Block caps at maximum value
- [ ] Block clears at turn start (unless prevented)

### Edge Cases
- [ ] Hand overflow sends cards to discard
- [ ] Empty draw pile triggers shuffle from discard
- [ ] Death prevention powers work correctly
- [ ] Extra turn mechanics work correctly
- [ ] Ethereal cards exhaust at turn end

### Multiplayer
- [ ] HP scales correctly for player count
- [ ] Each player has independent hand and energy
- [ ] Turn coordination works correctly

---

## Appendix: State Tracking

### CombatState Properties
- `Allies`: Player creatures
- `Enemies`: Enemy creatures
- `RoundNumber`: Current round (starts at 1)
- `CurrentSide`: Player or Enemy
- `Encounter`: Encounter definition
- `EscapedCreatures`: Creatures that fled

### CombatHistory Entries
- `CardPlayStarted` / `CardPlayFinished`
- `DamageReceived` / `CreatureAttacked`
- `BlockGained`
- `EnergySpent`
- `MonsterPerformedMove`
- `OrbChanneled`
- `PotionUsed`
- `PowerReceived`
- `StarsModified`
- `Summoned`
