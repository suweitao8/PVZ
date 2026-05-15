# Relic System GDD

> **Document Type**: Game Design Document
> **System**: Relics
> **Status**: Reverse-engineered from implementation
> **Last Updated**: 2026-05-15

---

## 1. Overview

The Relic System provides permanent upgrades that persist throughout a run in Slay the Spire 2. Relics are obtained from combat rewards, shops, events, and Neow bonuses. Each relic implements one or more hooks to modify game behavior—providing passive bonuses, triggered effects, or resource modifications.

Relics are organized by rarity tiers (Common, Uncommon, Rare, Boss, etc.) and by pools (Shared, Character-specific, Event-exclusive). The system supports stackable relics (Circlet), counter-based tracking, and state persistence across saves.

---

## 2. Player Fantasy

Players build a collection of powerful artifacts that fundamentally change how their deck functions. Each relic represents a meaningful choice—do you take the immediate power spike or save gold for shops? Relics create build-defining synergies and reward strategic acquisition timing.

The system delivers:
- **Build customization**: Relics enable specific strategies
- **Progression feel**: Collection grows throughout run
- **Risk/reward**: Elite fights offer powerful relics
- **Discovery**: Rare/Event relics offer unique effects

---

## 3. Detailed Rules

### 3.1 Relic Tiers

| Tier | Drop Rate | Gold Cost | Notes |
|------|-----------|-----------|-------|
| **Starter** | Character start | N/A (unbuyable) | Starting relic per character |
| **Common** | 50% | 200g | Most frequent |
| **Uncommon** | 33% | 250g | Less frequent |
| **Rare** | 17% | 300g | Powerful effects |
| **Shop** | Shop only | 225g | Shop-exclusive |
| **Event** | Events only | N/A (unbuyable) | Event-exclusive |
| **Ancient** | Boss/Neow | N/A (unbuyable) | Most powerful |

### 3.2 Relic Pools

| Pool | Size | Description |
|------|------|-------------|
| **Shared** | 118 relics | Available to all characters |
| **Character** | ~8 each | Character-specific synergies |
| **Event** | 132 relics | Event-exclusive |
| **Fallback** | 1 (Circlet) | Stackable for excess rewards |

### 3.3 Character Starter Relics

| Character | Starter Relic | Effect |
|-----------|---------------|--------|
| **Ironclad** | Burning Blood | Heal 6 HP after combat |
| **Silent** | Ring of the Snake | Draw 2 extra cards turn 1 |
| **Defect** | Cracked Core | Channel 1 Lightning orb turn 1 |
| **Necrobinder** | (character-specific) | Death synergies |
| **Regent** | (character-specific) | Gold synergies |

### 3.4 Acquisition Sources

```
ACQUISITION FLOW:

┌─────────────────┐
│ Combat Rewards  │ ← Elite/Boss victories
└────────┬────────┘
         │
┌────────▼────────┐
│ Shop Purchase   │ ← Merchant with price variance
└────────┬────────┘
         │
┌────────▼────────┐
│ Event Choices   │ ← Event-exclusive relics
└────────┬────────┘
         │
┌────────▼────────┐
│ Neow Bonus      │ │ Starting bonus selection
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│ RelicCmd.Obtain │
│ 1. Add to player│
│ 2. Remove from  │
│    grab bags    │
│ 3. Trigger UI   │
│ 4. Call         │
│    AfterObtained│
└─────────────────┘
```

### 3.5 Hook Integration

Relics intercept game events through the hook system:

#### Combat Lifecycle Hooks
| Hook | Timing |
|------|--------|
| `BeforeCombatStart` | Before combat begins |
| `BeforeCombatStartLate` | After other pre-combat |
| `AfterCombatEnd` | After combat finishes |
| `AfterCombatVictory` | On combat win |

#### Turn Hooks
| Hook | Timing |
|------|--------|
| `BeforeSideTurnStart` | Before any turn |
| `AfterSideTurnStart` | After turn starts |
| `BeforeTurnEnd` | Before turn ends |
| `AfterTurnEnd` | After turn ends |

#### Card Hooks
| Hook | Timing |
|------|--------|
| `BeforeCardPlayed` | Before card executes |
| `AfterCardPlayed` | After card executes |
| `AfterCardDrawn` | When card drawn |
| `AfterCardDiscarded` | When card discarded |
| `AfterCardExhausted` | When card exhausted |

#### Damage/Block Hooks
| Hook | Timing |
|------|--------|
| `BeforeDamageReceived` | Before damage applied |
| `AfterDamageReceived` | After damage applied |
| `BeforeBlockGained` | Before block gained |
| `AfterBlockGained` | After block gained |

#### Modifier Hooks
| Hook | Effect |
|------|--------|
| `ModifyDamageAdditive` | ± Damage |
| `ModifyDamageMultiplicative` | × Damage |
| `ModifyBlockAdditive` | ± Block |
| `ModifyBlockMultiplicative` | × Block |
| `ModifyHandDraw` | ± Cards drawn |
| `ModifyMaxEnergy` | ± Max energy |

### 3.6 Effect Patterns

#### Pattern A: On-Obtain Effect
Triggers immediately when obtained:
```
AfterObtained() → Apply effect once
```
Examples: Potion Belt (+2 potion slots), Calling Bell (curse + relics)

#### Pattern B: Combat Start Effect
Triggers at combat start:
```
BeforeCombatStart() → Flash() → Apply effect
```
Examples: Anchor (gain block), Cracked Core (channel orb)

#### Pattern C: Counter/State Tracking
Counts events, triggers at threshold:
```
AfterCardPlayed() → Counter++
If Counter % N == 0 → Trigger effect
```
Examples: Shuriken (3 attacks → +1 Strength), Ornamental Fan (3 attacks → block)

#### Pattern D: Value Modifier
Passive stat modification:
```
ModifyDamageAdditive() → +N damage
ModifyBlockAdditive() → +N block
```
Examples: Vajra (+1 Strength), Odd Mushroom (damage modification)

#### Pattern E: Conditional Prevention
Blocks specific actions:
```
ShouldGainGold() → false (prevents gold)
ShouldDie() → false (prevents death)
```
Examples: Ectoplasm (no gold), Fossilized Helix (death prevention)

### 3.7 Stacking Rules

- **Default**: Relics cannot stack
- **Stackable**: Only Circlet (fallback relic)
- **Stack behavior**: `StackCount++` on duplicate acquisition

### 3.8 Tradeability

Relics can be traded if:
```
IsTradable = !IsUsedUp
             AND !HasUponPickupEffect
             AND !IsMelted
             AND !SpawnsPets
             AND Rarity ∉ {Starter, Ancient, Event}
```

---

## 4. Formulas

### 4.1 Rarity Roll

```
roll = random(0, 1)

if (roll < 0.50):      → Common (50%)
elif (roll < 0.83):    → Uncommon (33%)
else:                  → Rare (17%)
```

### 4.2 Shop Price

```
baseCost = rarity switch {
    Common:   200g
    Uncommon: 250g
    Rare:     300g
    Shop:     225g
    Ancient:  999g (unbuyable)
    Starter:  999g (unbuyable)
    Event:    999g (unbuyable)
}

finalPrice = round(baseCost × random(0.85, 1.15))
```

### 4.3 Dynamic Variables

Relics use typed variables for clarity:

| Var Type | Purpose | Example |
|----------|---------|---------|
| `HealVar` | Healing amount | 6 HP |
| `BlockVar` | Block amount | 10 block |
| `EnergyVar` | Energy amount | 1 energy |
| `CardsVar` | Card count | 3 cards |
| `PowerVar<T>` | Power application | +1 Strength |
| `DynamicVar` | Custom named | "Turns" = 3 |

---

## 5. Edge Cases

### 5.1 Obtaining Duplicate Relic

- **Stackable (Circlet)**: Increment `StackCount`
- **Non-stackable**: Should not happen (removed from pool)
- **Fallback**: Circlet given when pool exhausted

### 5.2 Relic Pool Exhausted

- **Trigger**: All relics in tier already owned
- **Resolution**: Grant Circlet (stackable fallback)

### 5.3 Used-Up Relics

- **Trigger**: One-time use relics depleted
- **Resolution**: `IsUsedUp = true`, grays out in UI
- **Examples**: Toy Box, Spinning Wheel

### 5.4 Death Prevention Timing

- **Trigger**: Player would die
- **Resolution**: Check `ShouldDie()` hooks, find preventer
- **Follow-up**: `AfterPreventingDeath()` fires

### 5.5 Melted/Wax Relics

- **Wax**: Temporary relic state (Toy Box mechanic)
- **Melted**: Destroyed/consumed state
- **Effect**: No longer active

### 5.6 Multiplayer Relic Ownership

- **Ownership**: Each player has separate relic list
- **Shared Effects**: Some relics affect all players
- **Hooks**: Check `Owner` before applying

---

## 6. Dependencies

### Internal Dependencies
| System | Dependency Type | Description |
|--------|----------------|-------------|
| Hook System | Required | All relic effects via hooks |
| Combat System | Required | Combat triggers |
| Card System | Required | Card-related hooks |
| Damage System | Required | Damage modification hooks |
| Player System | Required | Ownership, state |

### External Dependencies
| System | Dependency Type | Description |
|--------|----------------|-------------|
| UI System | Required | Relic display, tooltips |
| Audio System | Required | Obtain/effect sounds |
| VFX System | Required | Flash effects |
| Save System | Required | State persistence |

---

## 7. Tuning Knobs

### Rarity Distribution
| Rarity | Threshold | Cost |
|--------|-----------|------|
| Common | < 0.50 | 200g |
| Uncommon | 0.50 - 0.83 | 250g |
| Rare | ≥ 0.83 | 300g |

### Price Variance
| Parameter | Value |
|-----------|-------|
| Minimum multiplier | 0.85 |
| Maximum multiplier | 1.15 |

### Dynamic Variables (Per Relic)
| Variable | Purpose | Tunable |
|----------|---------|---------|
| Heal amount | HP restored | Yes |
| Block amount | Block gained | Yes |
| Counter threshold | Triggers needed | Yes |
| Power amount | Effect magnitude | Yes |

---

## 8. Acceptance Criteria

### Acquisition
- [ ] Combat rewards offer correct rarity distribution
- [ ] Shop prices vary by ±15%
- [ ] Event relics only from events
- [ ] Starter relics start with character
- [ ] Circlet granted when pool exhausted

### Effects
- [ ] On-obtain effects trigger immediately
- [ ] Combat start effects trigger correctly
- [ ] Counter-based relics trigger at threshold
- [ ] Modifier hooks apply correct values
- [ ] Prevention hooks block correctly

### State
- [ ] Counter state persists across saves
- [ ] Stackable relics increment correctly
- [ ] Used-up relics display grayed
- [ ] Status changes (Active/Disabled) display correctly

### Pool Management
- [ ] Obtained relics removed from pool
- [ ] Character pools independent
- [ ] Event pool separate from main
- [ ] Epoch-gated relics unlock correctly

---

## Appendix: Relic Implementation Pattern

```csharp
public sealed class ExampleRelic : RelicModel
{
    // 1. Tier
    public override RelicRarity Rarity => RelicRarity.Common;
    
    // 2. Dynamic variables
    protected override IEnumerable<DynamicVar> CanonicalVars => new[] {
        new PowerVar<StrengthPower>(1m),
        new CardsVar(3)
    };
    
    // 3. Counter display
    public override bool ShowCounter => CombatManager.Instance.IsInProgress;
    public override int DisplayAmount => AttacksPlayed % DynamicVars.Cards.IntValue;
    
    // 4. State persistence
    [SavedProperty]
    public int AttacksPlayed { get; set; }
    
    // 5. Hook implementation
    public override async Task AfterCardPlayed(
        PlayerChoiceContext context, 
        CardPlay cardPlay)
    {
        if (cardPlay.Card.Type == CardType.Attack)
        {
            AttacksPlayed++;
            if (AttacksPlayed % DynamicVars.Cards.IntValue == 0)
            {
                Flash();  // Visual feedback
                await PowerCmd.Apply<StrengthPower>(
                    Owner.Creature, 
                    DynamicVars.Power.BaseValue, 
                    Owner.Creature, 
                    null);
            }
        }
    }
}
```

---

## Appendix: Relic Pool Hierarchy

```
RelicPoolModel (abstract)
│
├── SharedRelicPool
│   └── 118 relics available to all
│
├── CharacterRelicPools
│   ├── IroncladRelicPool (~8 relics)
│   ├── SilentRelicPool (~8 relics)
│   ├── DefectRelicPool
│   ├── NecrobinderRelicPool
│   └── RegentRelicPool
│
├── EventRelicPool
│   └── 132 event-exclusive relics
│
├── DeprecatedRelicPool
│   └── Removed/unused relics
│
└── FallbackRelicPool
    └── Circlet (stackable fallback)
```

---

## Appendix: Hook Categories Reference

| Category | Purpose | Examples |
|----------|---------|----------|
| **Lifecycle** | Combat start/end | Burning Blood, Anchor |
| **Turn** | Turn-based triggers | Shuriken, Kunai |
| **Card** | Card play triggers | Ink Bottle, Sundial |
| **Damage** | Damage modification | Vajra, Odd Mushroom |
| **Block** | Block modification | Paper Crane, Calipers |
| **Death** | Death prevention | Fossilized Helix |
| **Reward** | Reward modification | Busted Crown |
| **Potion** | Potion interactions | Sozu, Potion Belt |
| **Obtain** | On-acquisition | Calling Bell, Tiny House |
