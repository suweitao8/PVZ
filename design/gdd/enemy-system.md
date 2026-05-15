# Enemy System GDD

> **Document Type**: Game Design Document
> **System**: Enemies / Monsters
> **Status**: Reverse-engineered from implementation
> **Last Updated**: 2026-05-15

---

## 1. Overview

The Enemy System governs all monster behavior in Slay the Spire 2. Monsters use a state machine architecture to determine moves, with support for sequential patterns, weighted random selection, and conditional branching. The intent system telegraphs upcoming moves to players, enabling strategic planning.

Enemies are organized into encounters (groups of monsters) and scale with act progression and ascension levels. The system supports normal monsters, elites, and bosses with distinct reward tiers and mechanical complexity.

---

## 2. Player Fantasy

Players face unpredictable but fair challenges. The intent system creates a "read the enemy" dynamic where players must interpret upcoming threats and plan accordingly. Each enemy type has distinct personality—aggressive berserkers, defensive turtles, summoning spellcasters—creating varied tactical puzzles.

The system delivers:
- **Predictable unpredictability**: Random moves with readable patterns
- **Strategic depth**: Intent reading enables planning
- **Progressive challenge**: Scaling across acts and ascensions
- **Enemy variety**: Distinct mechanical identities per monster type

---

## 3. Detailed Rules

### 3.1 Monster Types

| Type | Gold Reward | Encounter Type | Notes |
|------|-------------|----------------|-------|
| **Normal** | 10-20g | Random rooms | Standard enemies |
| **Elite** | 35-45g | Elite rooms | Tougher, better rewards |
| **Boss** | 100g | Boss rooms | Unique mechanics |

### 3.2 Monster Stats

| Stat | Description | Generation |
|------|-------------|------------|
| **HP** | Health points | Random between MinHp and MaxHp |
| **Block** | Current block | Gained via moves, cleared each turn |
| **Powers** | Active effects | Applied via moves |
| **Strength** | Damage modifier | Via StrengthPower |

**HP Generation Rules:**
- Roll random value between `MinInitialHp` and `MaxInitialHp`
- In multi-monster encounters, ensure unique HP values (avoid duplicates)
- Scale for multiplayer: `HP × playerCount × scalingFactor`

### 3.3 State Machine Architecture

Monsters use `MonsterMoveStateMachine` to select moves:

```
STATE MACHINE FLOW:

┌─────────────────────────────────────────────────────────┐
│                   Turn Start                             │
│                         ↓                                │
│              RollMove() called                           │
│                         ↓                                │
│    ┌─────────────────────────────────────┐              │
│    │         Current State                │              │
│    │  ┌─────────────────────────────────┐│              │
│    │  │ MoveState                       ││              │
│    │  │ → Execute PerformMove()         ││              │
│    │  │ → Transition to FollowUpState   ││              │
│    │  └─────────────────────────────────┘│              │
│    │  ┌─────────────────────────────────┐│              │
│    │  │ RandomBranchState               ││              │
│    │  │ → Weighted random selection     ││              │
│    │  │ → Check repeat/cooldown rules   ││              │
│    │  └─────────────────────────────────┘│              │
│    │  ┌─────────────────────────────────┐│              │
│    │  │ ConditionalBranchState          ││              │
│    │  │ → Evaluate conditions           ││              │
│    │  │ → Select matching state         ││              │
│    │  └─────────────────────────────────┘│              │
│    └─────────────────────────────────────┘              │
│                         ↓                                │
│              Intent displayed                            │
│                         ↓                                │
│              Enemy turn executes                         │
│                         ↓                                │
│              State transition                            │
└─────────────────────────────────────────────────────────┘
```

### 3.4 State Types

#### MoveState
Simple move with optional follow-up:
```
MoveState("ATTACK", AttackMove, intent)
  .FollowUpState = buffState  → Always goes to buff next
```

#### RandomBranchState
Weighted random selection with repeat rules:
```
RandomBranchState
  ├── Move A: weight 3, CannotRepeat
  ├── Move B: weight 2, CanRepeatXTimes(2)
  └── Move C: weight 1, UseOnlyOnce
```

#### ConditionalBranchState
Condition-based branching:
```
ConditionalBranchState
  ├── If (allies > 0) → SupportMove
  └── If (allies == 0) → SoloAttackMove
```

### 3.5 Move Repeat Rules

| Rule | Behavior |
|------|----------|
| `CanRepeatForever` | No restrictions |
| `CanRepeatXTimes(N)` | Max N consecutive uses |
| `CannotRepeat` | Cannot repeat immediately |
| `UseOnlyOnce` | Can only ever be used once per combat |

### 3.6 Intent System

Intent types telegraph upcoming moves:

| Intent | Icon | Meaning |
|--------|------|---------|
| **Attack** | Sword | Will deal damage |
| **Multi-Attack** | Sword × N | Multiple hits |
| **DeathBlow** | Skull | Lethal damage incoming |
| **Buff** | Buff icon | Will self-buff |
| **Debuff** | Debuff icon | Will debuff player |
| **Defend** | Shield | Will gain block |
| **Summon** | Summon icon | Will summon allies |
| **Heal** | Heal icon | Will heal |
| **Sleep** | Zzz | Skipping turn |
| **Stun** | Stun icon | Stunned, no action |
| **Status** | Card icon | Will add status card |
| **Hidden** | None | Unknown intent |
| **Escape** | Run icon | Will flee |

**Attack Intent Scaling:**
- Icon size scales with damage amount
- Damage number displayed on intent

### 3.7 Encounter System

Encounters define monster groups:

```
ENCOUNTER STRUCTURE:

EncounterModel
├── RoomType (Monster/Elite/Boss)
├── IsWeak flag (early encounter)
├── Slots[] (named positions)
└── GenerateMonsters() → [(Monster, Slot)]
```

**Encounter Tags:**
Prevents similar encounters back-to-back:
- Slimes, Thieves, Workers, Crawler, Mushroom, Knights, etc.

### 3.8 Act Progression

Each act defines encounter pools:

| Pool | Distribution | Notes |
|------|--------------|-------|
| **Weak** | First ~3 floors | Easier encounters |
| **Regular** | Standard floors | Normal encounters |
| **Elite** | Elite rooms | Tougher fights |
| **Boss** | Boss room | End of act |

### 3.9 Monster Powers

Common monster powers:

| Power | Effect | Example |
|-------|--------|---------|
| **Strength** | +Damage to attacks | Many monsters |
| **Ritual** | +Strength at turn end | Cultists |
| **CurlUp** | Block on first hit | LouseProgenitor |
| **Frail** | Player block reduced | Various |
| **Vulnerable** | Player takes +50% damage | Various |
| **Weak** | Player deals -25% damage | Various |

---

## 4. Formulas

### 4.1 HP Generation

```
// Single monster
hp = random(MinInitialHp, MaxInitialHp)

// Multi-monster (ensure unique)
hp = random(MinInitialHp, MaxInitialHp)
while (hp in existingHps):
    hp = random(MinInitialHp, MaxInitialHp)

// Multiplayer scaling
hp = hp × playerCount × multiplayerScalingFactor
```

### 4.2 Damage Calculation

```
// Intent damage preview
intentDamage = baseDamage + strength
intentDamage = intentDamage × damageMultiplier
intentDamage = max(0, intentDamage)

// Actual damage (same as combat system)
finalDamage = Hook.ModifyDamage(baseDamage, ...)
```

### 4.3 Random Branch Weight

```
effectiveWeight = baseWeight

// UseOnlyOnce check
if (repeatType == UseOnlyOnce AND state in stateLog):
    effectiveWeight = 0

// CannotRepeat check
if (repeatType == CannotRepeat AND lastState == thisState):
    effectiveWeight = 0

// CanRepeatXTimes check
if (repeatType == CanRepeatXTimes(N) AND consecutiveCount >= N):
    effectiveWeight = 0

// Cooldown check
if (cooldown > 0 AND thisState in recentMoves[0:cooldown]):
    effectiveWeight = 0

// Dynamic weight function
effectiveWeight = effectiveWeight × GetWeight()
```

### 4.4 Gold Reward

```
// Normal monster
gold = random(10, 20)

// Elite
gold = random(35, 45)

// Boss
gold = 100

// Poverty ascension
gold = gold × 0.75
```

---

## 5. Edge Cases

### 5.1 Unique HP Enforcement

- **Trigger**: Multi-monster encounter
- **Issue**: Two monsters with same HP is confusing
- **Resolution**: Re-roll until unique values

### 5.2 All Moves Exhausted

- **Trigger**: UseOnlyOnce on all moves, all used
- **Resolution**: State machine should have fallback or loop
- **Design**: Always include at least one repeatable move

### 5.3 Stunned Monsters

- **Trigger**: Stun power applied
- **Resolution**: Skip turn, show Stun intent
- **Recovery**: Stun typically lasts 1 turn

### 5.4 Monster Death Mid-Encounter

- **Trigger**: Monster killed before other monsters act
- **Resolution**: Remove from encounter, continue with remaining
- **Hooks**: Trigger death-related effects

### 5.5 Summoned Monsters

- **Trigger**: Monster uses summon move
- **Resolution**: Add new monsters to encounter
- **Turn Order**: Summoned monsters act in current enemy turn

### 5.6 Escaping Monsters

- **Trigger**: Monster uses escape move
- **Resolution**: Remove from encounter (not killed)
- **Rewards**: Escaped monsters don't count for rewards

---

## 6. Dependencies

### Internal Dependencies
| System | Dependency Type | Description |
|--------|----------------|-------------|
| Combat System | Required | Combat state, turn flow |
| Damage System | Required | Attack execution |
| Power System | Required | Monster powers |
| Hook System | Required | Damage/ability modifications |
| RNG System | Required | Move selection, HP rolls |

### External Dependencies
| System | Dependency Type | Description |
|--------|----------------|-------------|
| UI System | Required | Intent display, health bars |
| Audio System | Required | Monster sounds |
| VFX System | Required | Attack animations |
| Asset System | Required | Monster sprites, animations |

---

## 7. Tuning Knobs

### Per-Monster Parameters
| Parameter | Description | Example |
|-----------|-------------|---------|
| `MinInitialHp` | Minimum HP | 51 |
| `MaxInitialHp` | Maximum HP | 53 |
| `IsWeak` | Early encounter flag | true |
| `IsHealthBarVisible` | Show HP bar | true |

### Per-Move Parameters
| Parameter | Description | Example |
|-----------|-------------|---------|
| Base damage | Attack damage | 12 |
| Repeat type | Repeat rules | CannotRepeat |
| Weight | Random selection weight | 3 |
| Cooldown | Turns before repeatable | 2 |

### Global Parameters
| Parameter | Location | Default |
|-----------|----------|---------|
| Weak encounter count | `ActModel.NumberOfWeakEncounters` | 3 |
| Multiplayer HP scale | `MultiplayerScalingModel` | Per-act |

---

## 8. Acceptance Criteria

### Monster Behavior
- [ ] HP rolls within defined range
- [ ] Moves follow state machine logic
- [ ] Intents correctly preview moves
- [ ] Repeat rules enforced correctly
- [ ] Conditional branches evaluate correctly

### Encounter System
- [ ] Encounters generate correct monster groups
- [ ] Tags prevent back-to-back similar encounters
- [ ] Slots position monsters correctly
- [ ] Scaling applies for act/ascension

### Intent Display
- [ ] Attack intents show correct damage
- [ ] Multi-attack shows hit count
- [ ] Buff/debuff icons display correctly
- [ ] Hidden intents show nothing

### Scaling
- [ ] Ascension ToughEnemies increases HP
- [ ] Ascension DeadlyEnemies increases damage
- [ ] Multiplayer scales HP correctly
- [ ] Poverty reduces gold rewards

---

## Appendix: Monster Implementation Pattern

```csharp
public sealed class ExampleMonster : MonsterModel
{
    // 1. HP range with ascension scaling
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(
        AscensionLevel.ToughEnemies, 52, 51);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(
        AscensionLevel.ToughEnemies, 54, 53);
    
    // 2. Damage values with ascension scaling
    private int AttackDamage => AscensionHelper.GetValueIfAscension(
        AscensionLevel.DeadlyEnemies, 12, 10);
    
    // 3. State machine
    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        MoveState attack = new MoveState("ATTACK", AttackMove, 
            new SingleAttackIntent(() => AttackDamage));
        MoveState buff = new MoveState("BUFF", BuffMove, 
            new BuffIntent());
        
        attack.FollowUpState = buff;
        buff.FollowUpState = attack;
        
        return new MonsterMoveStateMachine(
            new List<MonsterState> { attack, buff }, 
            attack);
    }
    
    // 4. Move implementations
    private async Task AttackMove(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(AttackDamage)
            .FromMonster(this)
            .WithAttackerAnim("Attack", 0.2f)
            .Execute(null);
    }
    
    private async Task BuffMove(IReadOnlyList<Creature> targets)
    {
        await PowerCmd.Apply<StrengthPower>(
            base.Creature, 2m, base.Creature, null);
    }
}
```

---

## Appendix: Intent Hierarchy

```
AbstractIntent (base)
├── SingleAttackIntent      - Single hit attack
├── MultiAttackIntent       - Multi-hit attack
├── DeathBlowIntent         - Lethal attack
├── BuffIntent              - Self-buff
├── DebuffIntent            - Player debuff
├── DefendIntent            - Block gain
├── SummonIntent            - Summon allies
├── HealIntent              - Self-heal
├── SleepIntent             - Skip turn
├── StunIntent              - Stunned
├── StatusIntent            - Add status card
├── CardDebuffIntent        - Card-related debuff
├── HiddenIntent            - No display
├── UnknownIntent           - Mystery
└── EscapeIntent            - Will flee
```
