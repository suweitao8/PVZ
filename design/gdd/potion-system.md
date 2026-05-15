# Potion System GDD

> **Document Type**: Game Design Document
> **System**: Potions
> **Status**: Reverse-engineered from implementation
> **Last Updated**: 2026-05-15

---

## 1. Overview

The Potion System provides consumable items that offer immediate effects in Slay the Spire 2. Players have limited potion slots (base 3) and can use potions during combat or, for some potions, outside combat. Potions are obtained from combat rewards, shops, and events, with rarity tiers determining drop rates and prices.

The system supports combat-only potions, anytime potions, and automatic potions (like Fairy in a Bottle that triggers on death). Potions can target enemies, the player, or all enemies.

---

## 2. Player Fantasy

Potions provide emergency options and strategic flexibility. Players decide when to use limited resources—save the Fairy for a dangerous fight, or use it now to preserve HP? Potions create clutch moments where a well-timed Ghost in a Jar or Entropic Brew turns a losing fight into a victory.

The system delivers:
- **Emergency tools**: Panic buttons for tough situations
- **Resource management**: Limited slots create meaningful choices
- **Build support**: Potions complement specific strategies
- **Discovery**: Rare potions offer unique effects

---

## 3. Detailed Rules

### 3.1 Potion Tiers

| Rarity | Drop Rate | Shop Price |
|--------|-----------|------------|
| **Common** | 65% | 50g |
| **Uncommon** | 25% | 75g |
| **Rare** | 10% | 100g |
| **Event** | N/A | Event-only |
| **Token** | N/A | Special |

### 3.2 Usage Categories

| Category | Description | Examples |
|----------|-------------|----------|
| **CombatOnly** | Only during combat | Fire Potion, Attack Potion |
| **AnyTime** | Combat or out of combat | Blood Potion, Fruit Juice |
| **Automatic** | Triggers on condition | Fairy in a Bottle |

### 3.3 Target Types

| Target | Description |
|--------|-------------|
| `Self` | Targets using player |
| `AnyPlayer` | Any player (multiplayer) |
| `AnyEnemy` | Single enemy |
| `AllEnemies` | All enemies |
| `TargetedNoCreature` | Special (e.g., Merchant) |

### 3.4 Potion Slots

- **Base Slots**: 3
- **Can Increase**: Potion Belt relic (+2 slots)
- **Can Decrease**: Tight Belt ascension
- **Overflow**: Cannot obtain if no empty slots

### 3.5 Acquisition Sources

```
POTION ACQUISITION:

┌─────────────────┐
│ Combat Rewards  │ ← 40-50% chance, +25% for Elite
└────────┬────────┘
         │
┌────────▼────────┐
│ Shop Purchase   │ ← 50-100g ±5% variance
└────────┬────────┘
         │
┌────────▼────────┐
│ Events          │ ← Event-exclusive potions
└────────┬────────┘
         │
┌────────▼────────┐
│ Starting        │ ← Character starting potions
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│ Add to empty    │
│ potion slot     │
└─────────────────┘
```

### 3.6 Potion Reward Odds

```
baseOdds = 0.40 (40%)
targetOdds = 0.50 (50%)
eliteBonus = 0.25 (+25% for Elite)

// Dynamic adjustment
if (noPotionLastFight):
    currentOdds += 0.10
else:
    currentOdds -= 0.10

currentOdds = clamp(currentOdds, baseOdds, targetOdds)
```

### 3.7 Potion Usage Flow

```
POTION USAGE:

1. Selection
   ├── Player clicks potion slot
   ├── Validate: CanUse() check
   └── If targeted, select target

2. Enqueue
   ├── Remove from slot
   └── Queue UsePotionAction

3. Execute
   ├── BeforePotionUsed hook
   ├── Play throw VFX
   ├── OnUse() effect
   ├── Log to combat history
   └── AfterPotionUsed hook
```

### 3.8 Potion Pools

| Pool | Size | Description |
|------|------|-------------|
| **Shared** | 45 | Available to all |
| **Character** | Variable | Character-specific |
| **Event** | 2 | Foul, Glowwater |
| **Token** | 1 | PotionShapedRock |

---

## 4. Formulas

### 4.1 Rarity Roll

```
roll = random(0, 1)

if (roll <= 0.10):      → Rare (10%)
elif (roll <= 0.35):    → Uncommon (25%)
else:                   → Common (65%)
```

### 4.2 Shop Price

```
basePrice = rarity switch {
    Common:   50g
    Uncommon: 75g
    Rare:     100g
}

finalPrice = round(basePrice × random(0.95, 1.05))
```

### 4.3 Potion Reward Chance

```
currentOdds = baseOdds + adjustment
if (isElite): currentOdds += eliteBonus
hasPotion = random(0, 1) < currentOdds
```

---

## 5. Common Potions Reference

### 5.1 Common Potions (Combat-Only)

| Potion | Effect | Target |
|--------|--------|--------|
| **Fire Potion** | Deal 20 damage | Enemy |
| **Block potion** | Gain 12 Block | Self |
| **Strength Potion** | Gain 2 Strength | Self |
| **Energy Potion** | Gain 2 Energy | Self |
| **Swift Potion** | Draw 3 cards | Self |
| **Weak Potion** | Apply 3 Weak | Enemy |
| **Vulnerable Potion** | Apply 3 Vulnerable | Enemy |
| **Poison Potion** | Apply 6 Poison | Enemy |
| **Attack Potion** | Choose 1 of 3 Attacks | Self |
| **Skill Potion** | Choose 1 of 3 Skills | Self |
| **Power Potion** | Choose 1 of 3 Powers | Self |

### 5.2 Uncommon Potions

| Potion | Effect | Target |
|--------|--------|--------|
| **Regen Potion** | Gain 5 Regen | Self |
| **Liquid Bronze** | Gain 3 Thorns | Self |
| **Cunning Potion** | Add 3 Shivs to hand | Self |
| **Duplicator** | Next card plays twice | Self |
| **Gambler's Brew** | Discard N, draw N | Self |

### 5.3 Rare Potions

| Potion | Effect | Target |
|--------|--------|--------|
| **Fairy in a Bottle** | Auto-heal 30% HP on death | Automatic |
| **Fruit Juice** | Gain 5 Max HP | Self |
| **Entropic Brew** | Fill slots with random potions | Self |
| **Ghost in a Jar** | Gain 1 Intangible | Self |
| **Snecko Oil** | Draw 7, randomize costs | Self |

### 5.4 Event-Only Potions

| Potion | Effect | Special |
|--------|--------|---------|
| **Foul Potion** | 12 damage to ALL, or sell to Merchant | 100g at Merchant |
| **Glowwater Potion** | Exhaust hand, draw 10 | Self |

---

## 6. Edge Cases

### 6.1 No Empty Slots

- **Trigger**: Attempting to obtain potion with full slots
- **Resolution**: "Potion slots full" message, potion lost
- **Exception**: Entropic Brew fills slots (doesn't overflow)

### 6.2 Sozu Relic

- **Effect**: Prevents potion procurement
- **Behavior**: `ShouldProcurePotion` returns false
- **Compensation**: +1 Energy from relic

### 6.3 Fairy in a Bottle Timing

- **Trigger**: Player HP reaches 0
- **Resolution**: Heal to 30% Max HP, potion consumed
- **Priority**: Before death prevention hooks

### 6.4 Foul Potion at Merchant

- **Normal Use**: Deal 12 damage to everyone (including player)
- **At Merchant**: Throw at Merchant for 100 gold
- **Special**: Only potion with Merchant interaction

### 6.5 Multiplayer Targeting

- **AnyPlayer potions**: Can target any player
- **Throw animation**: Shows throw to target player
- **Network sync**: Use synchronized across clients

---

## 7. Dependencies

### Internal Dependencies
| System | Dependency Type | Description |
|--------|----------------|-------------|
| Combat System | Required | Combat state for combat-only potions |
| Damage System | Required | Fire Potion, etc. |
| Card System | Required | Card generation potions |
| Power System | Required | Power application potions |
| Hook System | Required | Potion hooks |

### External Dependencies
| System | Dependency Type | Description |
|--------|----------------|-------------|
| UI System | Required | Potion slot display |
| VFX System | Required | Throw/splash effects |
| Audio System | Required | Potion sounds |

---

## 8. Tuning Knobs

### Slot Configuration
| Parameter | Default | Modifier |
|-----------|---------|----------|
| Base slots | 3 | Potion Belt +2 |
| Min slots | 0 | Tight Belt -1 |

### Drop Rates
| Parameter | Value |
|-----------|-------|
| Base drop chance | 40% |
| Target drop chance | 50% |
| Elite bonus | +25% |
| Adjustment per fight | ±10% |

### Prices
| Rarity | Base Price |
|--------|------------|
| Common | 50g |
| Uncommon | 75g |
| Rare | 100g |

---

## 8. Acceptance Criteria

### Acquisition
- [ ] Potions drop at correct rates
- [ ] Elite fights have bonus drop chance
- [ ] Shop prices vary by ±5%
- [ ] Event potions only from events
- [ ] Full slots prevent acquisition

### Usage
- [ ] Combat-only potions unusable outside combat
- [ ] Targeted potions require target selection
- [ ] Automatic potions trigger at correct time
- [ ] Potions consume slot on use

### Effects
- [ ] Damage potions deal correct damage
- [ ] Buff potions apply correct powers
- [ ] Card potions generate correct cards
- [ ] Fairy heals to correct percentage

---

## Appendix: Potion Implementation Pattern

```csharp
public sealed class FirePotion : PotionModel
{
    // 1. Rarity
    public override PotionRarity Rarity => PotionRarity.Common;
    
    // 2. Usage
    public override PotionUsage Usage => PotionUsage.CombatOnly;
    
    // 3. Target
    public override TargetType TargetType => TargetType.AnyEnemy;
    
    // 4. Dynamic variables
    protected override IEnumerable<DynamicVar> CanonicalVars => new[] {
        new DamageVar(20m)
    };
    
    // 5. Effect
    protected override async Task OnUse(
        PlayerChoiceContext choiceContext, 
        Creature? target)
    {
        AssertValidForTargetedPotion(target);
        await DamageCmd.Attack(DynamicVars.Damage)
            .FromPotion(this)
            .Targeting(target)
            .Execute(choiceContext);
    }
}
```

---

## Appendix: Relic Interactions

| Relic | Interaction |
|-------|-------------|
| **Potion Belt** | +2 potion slots |
| **Sozu** | Prevents potion drops, +1 Energy |
| **Cauldron** | Potion-related effects |
| **Alchemical Coffer** | Potion-related effects |
| **Petrified Toad** | Potion interactions |
