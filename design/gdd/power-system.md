# Power System GDD

> **Document Type**: Game Design Document
> **System**: Powers
> **Status**: Reverse-engineered from implementation
> **Last Updated**: 2026-05-15

---

## 1. Overview

The Power System manages persistent effects attached to creatures in Slay the Spire 2. Powers can be buffs (beneficial) or debuffs (harmful), and modify game behavior through a comprehensive hook system. Powers stack by incrementing their amount counter, and duration-based powers tick down at specific turn phases.

The system supports permanent powers (Strength, Dexterity), duration-based debuffs (Weak, Vulnerable, Frail), trigger-based powers (Thorns, Poison), and complex instanced powers (Afterimage).

---

## 2. Player Fantasy

Powers create dynamic combat states where both players and enemies evolve throughout the fight. A player might stack Strength for devastating attacks, apply Poison for passive damage, or use defensive powers like Intangible to survive lethal blows. The stacking system creates satisfying "snowball" moments where effects compound.

The system delivers:
- **Build expression**: Powers enable specific archetypes (Strength stacker, Poison build, etc.)
- **Tactical timing**: Duration powers require timing awareness
- **Counter-play**: Artifact blocks debuffs, cleanse effects exist
- **Visual feedback**: Power icons clearly communicate state

---

## 3. Detailed Rules

### 3.1 Power Types

| Type | Display | Behavior |
|------|---------|----------|
| **Buff** | Green/Cream amount | Beneficial effect |
| **Debuff** | Red amount | Harmful effect |
| **None** | Special | Typeless (rare) |

**Type Flipping**: Powers with `AllowNegative=true` flip type when amount goes negative.

### 3.2 Stack Types

| Stack Type | Behavior | Examples |
|------------|----------|----------|
| **Counter** | Amount increments/decrements | Strength, Weak, Poison |
| **Single** | Only one instance allowed | Barricade, Corruption |
| **Instanced** | Multiple independent instances | Afterimage |

### 3.3 Power Properties

| Property | Purpose |
|----------|---------|
| `Amount` | Current stack count/value |
| `Owner` | Creature holding this power |
| `Applier` | Who applied the power |
| `Target` | Target reference for cross-creature powers |
| `SkipNextDurationTick` | Skip one duration decrement |
| `AllowNegative` | Allow negative amounts |
| `IsInstanced` | Multiple instances allowed |
| `IsVisible` | Show in UI |

### 3.4 Hook Categories

Powers modify game state through virtual hook methods:

#### Damage Modification
| Hook | Phase | Example |
|------|-------|---------|
| `ModifyDamageAdditive` | Add ± to damage | Strength +N |
| `ModifyDamageMultiplicative` | Multiply damage | Vulnerable ×1.5 |
| `ModifyDamageCap` | Set damage ceiling | Intangible cap=1 |

#### Block Modification
| Hook | Phase | Example |
|------|-------|---------|
| `ModifyBlockAdditive` | Add ± to block | Dexterity +N |
| `ModifyBlockMultiplicative` | Multiply block | Frail ×0.75 |

#### Turn-Based Triggers
| Hook | Timing | Example |
|------|--------|---------|
| `BeforeSideTurnStart` | Before turn | Plating gains block |
| `AfterSideTurnStart` | After turn starts | Poison deals damage |
| `BeforeTurnEnd` | Before turn ends | Ritual gains Strength |
| `AfterTurnEnd` | After turn ends | Duration tick |

#### Event Triggers
| Hook | Trigger | Example |
|------|---------|---------|
| `AfterCardPlayed` | Card resolved | Afterimage gains block |
| `AfterCardExhausted` | Card exhausted | Feel No Pain gains block |
| `AfterDamageReceived` | Damage taken | Thorns retaliates |
| `ShouldClearBlock` | Block would clear | Barricade prevents |

### 3.5 Power Application Flow

```
POWER APPLICATION:

1. Validate
   ├── Combat not ending
   └── Target can receive powers

2. Check Existing
   ├── Instanced → Create new instance
   ├── Exists → Modify amount
   └── Not exists → Create new

3. Apply Amount
   ├── BeforePowerAmountChanged hooks
   ├── ModifyPowerAmountGiven (applier)
   ├── ModifyPowerAmountReceived (target)
   │   └── Artifact blocks debuffs here
   └── Set new amount

4. Finalize
   ├── Log to combat history
   ├── Visual effects
   ├── Set SkipNextDurationTick (debuffs on players)
   └── AfterPowerAmountChanged hooks
```

### 3.6 Duration Tick System

```
DURATION TICK FLOW:

At turn end:
├── Check SkipNextDurationTick
│   ├── true → Clear flag, no decrement
│   └── false → Decrement amount by 1
├── Check removal condition
│   └── Amount = 0 (or < 1 if !AllowNegative)
└── Remove if condition met
```

**Skip Logic**: Debuffs applied to players skip the first duration tick, ensuring they last the full intended duration.

### 3.7 Common Powers

#### Stat Modifiers
| Power | Type | Effect |
|-------|------|--------|
| **Strength** | Buff | +N damage on attacks |
| **Dexterity** | Buff | +N block from cards |
| **Focus** | Buff | +N orb output |

#### Duration Debuffs
| Power | Type | Effect | Duration |
|-------|------|--------|----------|
| **Weak** | Debuff | ×0.75 outgoing damage | Ticks on enemy turn end |
| **Vulnerable** | Debuff | ×1.5 incoming damage | Ticks on enemy turn end |
| **Frail** | Debuff | ×0.75 block gained | Ticks on enemy turn end |
| **Intangible** | Buff | Cap damage at 1 | Ticks on enemy turn end |

#### Trigger-Based Powers
| Power | Type | Trigger | Effect |
|-------|------|---------|--------|
| **Poison** | Debuff | Turn start | Unblockable damage, decrement |
| **Thorns** | Buff | When attacked | Reflect damage |
| **Regen** | Buff | Turn end | Heal HP |
| **Ritual** | Buff | Turn end | Gain Strength |

#### Special Powers
| Power | Type | Effect |
|-------|------|--------|
| **Artifact** | Buff | Block N debuffs |
| **Buffer** | Buff | Prevent N HP loss |
| **Barricade** | Buff | Block never clears |
| **Corruption** | Buff | Skills cost 0, Skills exhaust |

---

## 4. Formulas

### 4.1 Damage Modification

```
// Additive phase
damage += sum(ModifyDamageAdditive for all powers)

// Multiplicative phase
damage *= product(ModifyDamageMultiplicative for all powers)

// Cap phase
damage = min(damage, min(ModifyDamageCap for all powers))
```

### 4.2 Block Modification

```
// Additive phase
block += sum(ModifyBlockAdditive for all powers)

// Multiplicative phase
block *= product(ModifyBlockMultiplicative for all powers)
```

### 4.3 Poison Damage

```
poisonDamage = amount
target.HP -= poisonDamage
amount -= 1  // Decrement after damage
if (amount <= 0): remove power
```

### 4.4 Thorns Reflection

```
reflectedDamage = thornsAmount
await Damage(dealer, reflectedDamage)
thornsAmount -= 1
```

### 4.5 Artifact Debuff Blocking

```
if (incomingPower.Type == Debuff AND artifact.Amount > 0):
    artifact.Amount -= 1
    blockPowerApplication()
```

---

## 5. Edge Cases

### 5.1 Negative Amounts

- **AllowNegative=true**: Amount can go negative, type may flip
- **AllowNegative=false**: Amount floored at 0, power removed

### 5.2 Zero Amount Removal

- **Counter powers**: Remove when Amount = 0
- **Single powers**: Never removed by amount

### 5.3 Instanced Powers

- **Multiple instances**: Each tracks independently
- **Example**: Afterimage has separate block amounts per card

### 5.4 Artifact Blocking

- **Timing**: Blocks during `ModifyPowerAmountReceived`
- **Order**: Artifact decrements, debuff blocked
- **Multiple debuffs**: Each blocked separately

### 5.5 Death and Powers

- **Default**: Powers removed when owner dies
- **Exception**: `ShouldPowerBeRemovedAfterOwnerDeath() = false`

### 5.6 Skip Duration Tick

- **When set**: Debuff applied to player
- **Effect**: First duration tick skipped
- **Purpose**: Full intended duration

---

## 6. Dependencies

### Internal Dependencies
| System | Dependency Type | Description |
|--------|----------------|-------------|
| Combat System | Required | Combat state, turn flow |
| Damage System | Required | Damage modification hooks |
| Block System | Required | Block modification hooks |
| Hook System | Required | All power effects via hooks |
| Card System | Required | Card-related triggers |

### External Dependencies
| System | Dependency Type | Description |
|--------|----------------|-------------|
| UI System | Required | Power icon display |
| VFX System | Required | Power application effects |
| Audio System | Required | Power sounds |

---

## 7. Tuning Knobs

### Duration Powers (Per Power)
| Power | Initial Duration | Tick Timing |
|-------|------------------|-------------|
| Weak | Variable | Enemy turn end |
| Vulnerable | Variable | Enemy turn end |
| Frail | Variable | Enemy turn end |
| Intangible | Variable | Enemy turn end |

### Multiplier Values (Per Power)
| Power | Multiplier | Notes |
|-------|------------|-------|
| Weak | 0.75 | -25% damage |
| Vulnerable | 1.5 | +50% damage |
| Frail | 0.75 | -25% block |

### Trigger Amounts (Per Power)
| Power | Base Amount | Scaling |
|-------|-------------|---------|
| Poison | Variable | Can stack |
| Thorns | Variable | Decrement on trigger |
| Regen | Variable | Can stack |

---

## 8. Acceptance Criteria

### Power Application
- [ ] Powers apply with correct initial amount
- [ ] Stacking increments existing power amount
- [ ] Single powers don't stack
- [ ] Instanced powers create separate instances

### Power Effects
- [ ] Additive modifiers apply correctly
- [ ] Multiplicative modifiers apply correctly
- [ ] Damage caps enforced correctly
- [ ] Trigger powers fire at correct times

### Duration System
- [ ] Duration ticks at correct phase
- [ ] Skip flag prevents first tick
- [ ] Powers remove when duration expires
- [ ] Artifact blocks debuffs correctly

### Edge Cases
- [ ] Negative amounts handled correctly
- [ ] Powers removed on owner death (unless exception)
- [ ] Multiple powers interact correctly

---

## Appendix: Power Implementation Pattern

```csharp
public sealed class ExamplePower : PowerModel
{
    // 1. Type and Stack
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    
    // 2. Allow negative
    public override bool AllowNegative => false;
    
    // 3. Hook implementation
    public override decimal ModifyDamageAdditive(
        Creature? target, decimal amount, 
        ValueProp props, Creature? dealer, 
        CardModel? cardSource)
    {
        if (Owner != dealer) return 0m;
        if (!props.IsPoweredAttack()) return 0m;
        return Amount;  // Add strength to damage
    }
    
    // 4. Duration tick (for duration powers)
    public override async Task AfterTurnEnd(
        PlayerChoiceContext ctx, CombatSide side)
    {
        if (side == CombatSide.Enemy)
            await PowerCmd.TickDownDuration(this);
    }
}
```

---

## Appendix: Common Powers Reference

| Power | Type | Stack | Hook Used | Duration |
|-------|------|-------|-----------|----------|
| Strength | Buff | Counter | ModifyDamageAdditive | Permanent |
| Dexterity | Buff | Counter | ModifyBlockAdditive | Permanent |
| Weak | Debuff | Counter | ModifyDamageMultiplicative (×0.75) | Duration |
| Vulnerable | Debuff | Counter | ModifyDamageMultiplicative (×1.5) | Duration |
| Frail | Debuff | Counter | ModifyBlockMultiplicative (×0.75) | Duration |
| Poison | Debuff | Counter | AfterSideTurnStart (damage) | Self-decrement |
| Artifact | Buff | Counter | TryModifyPowerAmountReceived | Until consumed |
| Intangible | Buff | Counter | ModifyDamageCap (1) | Duration |
| Buffer | Buff | Counter | ModifyHpLostAfterOsty (0) | Until consumed |
| Barricade | Buff | Single | ShouldClearBlock (false) | Permanent |
| Thorns | Buff | Counter | AfterDamageGiven (reflect) | Duration |
| Regen | Buff | Counter | AfterTurnEnd (heal) | Duration |
| Ritual | Buff | Counter | AfterTurnEnd (+Strength) | Permanent |
| Focus | Buff | Counter | ModifyOrbValue | Permanent |
| Corruption | Buff | Single | ModifyEnergyCostInCombat, ModifyCardPlayResultPileType | Permanent |
