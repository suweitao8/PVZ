# HUD Design

> **Status**: Complete
> **Author**: user + ux-designer
> **Last Updated**: 2026-05-15
> **Template**: HUD Design

---

## HUD Philosophy

**Information-dense with contextual layers.** The combat HUD presents all decision-relevant data at a glance — energy, hand cards, player/enemy HP, block, intents, powers, pile counts, gold, and floor. Players constantly calculate damage, block, and card synergies, so withholding information would hurt tactical depth. Secondary details (relic effects, potion descriptions, deck contents) are available on demand through hover/tap interactions. The HUD never obstructs the play area. During map navigation, HUD reduces to minimal run state (gold, HP, floor, act).

---

## Information Architecture

### Full Information Inventory

| Item | Source System | Description |
|------|--------------|-------------|
| Energy (current/max) | Combat | Resource for playing cards |
| Player HP / Max HP | Combat, Run | Health state |
| Player Block | Combat | Damage mitigation |
| Hand cards | Card | Cards available to play (max 10) |
| Draw pile count | Card | Cards remaining to draw |
| Discard pile count | Card | Cards in discard |
| Exhaust pile count | Card | Cards removed from combat |
| Enemy HP bars | Enemy | All enemy health |
| Enemy Block | Enemy | Enemy damage mitigation |
| Enemy Intents | Enemy | Telegraphed moves |
| Enemy Powers | Power | Powers on enemies |
| Player Powers | Power | Powers on player |
| Relics | Relic | All owned relics |
| Relic counters | Relic | Counter values for tracking relics |
| Potion slots | Potion | 3 slots (expandable) |
| Gold | Run | Currency |
| Floor / Act | Run | Progression indicator |
| Round number | Combat | Current combat round |
| Turn indicator | Combat | Player/Enemy turn |
| Deck size | Card | Total cards in deck |

### Categorization

| Category | Items | Rationale |
|----------|-------|-----------|
| **Must Show** | Energy, Player HP/Max HP, Player Block, Hand cards, Enemy HP bars, Enemy Block, Enemy Intents, Player Powers, Enemy Powers, Draw pile count, Discard pile count, Exhaust pile count, Relics, Potions, Gold, Floor/Act, Deck size, Round number, Turn indicator | All decision-relevant data needed for tactical depth in a deckbuilder |
| **Contextual** | — | All core information is Must Show per information-dense philosophy |
| **On Demand** | Relic details (hover), Potion details (hover), Deck contents (click), Draw pile contents (click), Discard pile contents (click), Exhaust pile contents (click) | Secondary information available when player actively seeks it |
| **Hidden** | Combat history | Debug/internal only, not player-facing during normal play |

---

## Layout Zones

```
┌─────────────────────────────────────────────────────────────────────────────┐
│ TOP BAR (~120px)                                                             │
│ ┌─────────┐ ┌──────────────────────────────────────────┐ ┌───────────────┐ │
│ │ Floor   │ │              ENEMIES                      │ │ Gold    Deck  │ │
│ │ Act #   │ │  [Enemy 1] [Enemy 2] [Enemy 3]           │ │ ###     ##    │ │
│ └─────────┘ └──────────────────────────────────────────┘ └───────────────┘ │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│                         PLAY AREA (flexible)                                │
│                    (Enemies, VFX, Player sprite)                            │
│                                                                             │
├─────────────────────────────────────────────────────────────────────────────┤
│ RELIC BAR (~60px)                                                           │
│ ┌─────────────────────────────────────────────────────────────────────────┐ │
│ │ [R][R][R][R][R][R][R][R][R][R][R][R][R][R][R]...                         │ │
│ └─────────────────────────────────────────────────────────────────────────┘ │
├─────────────────────────────────────────────────────────────────────────────┤
│ BOTTOM AREA (~280px)                                                        │
│ ┌─────────┐ ┌───────────────────────────────────┐ ┌─────────────────────┐ │
│ │ ENERGY  │ │           HAND (fan layout)        │ │ PILE COUNTS         │ │
│ │  (80px) │ │  [Card][Card][Card][Card][Card]   │ │ Draw: ## Disc: ##   │ │
│ │   3/3   │ │  [Card][Card][Card][Card][Card]   │ │ Exhaust: ##         │ │
│ ├─────────┤ └───────────────────────────────────┘ ├─────────────────────┤ │
│ │ PLAYER  │                                      │ POTIONS             │ │
│ │ HP ###  │                                      │ [P][P][P]           │ │
│ │ Block # │                                      │                     │ │
│ └─────────┘                                      ├─────────────────────┤ │
│                                                  │ Round: # Turn: P    │ │
│                                                  └─────────────────────┘ │
└─────────────────────────────────────────────────────────────────────────────┘
```

### Zone Specifications

| Zone | Position | Height | Contents |
|------|----------|--------|----------|
| **Top Bar** | Top edge, full width | ~120px | Floor/Act indicator, Enemy row (HP bars, block, intents, powers), Gold counter, Deck size |
| **Play Area** | Center | Flexible (remaining) | Enemy sprites, Player sprite, Combat VFX, Card animations |
| **Relic Bar** | Above hand, full width | ~60px | Horizontal scrollable list of relic icons with counters |
| **Hand Area** | Bottom center | ~200px | Player hand cards in fan layout, max 10 visible |
| **Energy Orb** | Bottom left | ~80px diameter | Circular energy display (current/max) |
| **Player Stats** | Bottom left, below energy | ~80px | HP bar, Max HP number, Block amount |
| **Pile Counts** | Bottom right | ~60px | Draw pile count, Discard pile count, Exhaust pile count |
| **Potion Slots** | Bottom right, below piles | ~80px | 3 potion slot icons (expandable to 5) |
| **Turn Info** | Bottom right corner | ~30px | Round number, Turn indicator (Player/Enemy) |

### Zone Behaviors

- **Top Bar**: Fixed position, never scrolls
- **Play Area**: Receives all combat VFX, never obstructed by HUD
- **Relic Bar**: Horizontal scroll if relics exceed visible width (wrap on mobile)
- **Hand Area**: Cards fan outward from center, selected card lifts for preview
- **Energy Orb**: Pulses when energy changes, glows when full
- **Player Stats**: Block number appears/disappears based on block value
- **Pile Counts**: Click to open pile viewer modal
- **Potion Slots**: Empty slots show placeholder, click to use potion

---

## HUD Elements

### Top Bar Elements

#### Floor/Act Indicator
- **Visual Form**: Text display "Floor X - Act Y"
- **Content**: Current floor number (1-15 per act), Act number (1-3)
- **Update Behavior**: Updates on room transition
- **Animation**: None (static text)

#### Gold Counter
- **Visual Form**: Gold coin icon + number
- **Content**: Current gold amount
- **Update Behavior**: Real-time on gold gain/spend
- **Animation**: Number slam on gain, coin bounce on spend

#### Deck Size
- **Visual Form**: Card stack icon + number
- **Content**: Total cards in master deck
- **Update Behavior**: Updates on card add/remove
- **Animation**: None
- **Interaction**: Click to open deck viewer modal

#### Enemy Display (per enemy)
- **HP Bar**: Horizontal bar, green fill, red background, damage preview on hover
- **HP Number**: Current HP / Max HP below bar
- **Block Shield**: Shield icon with number, appears when block > 0
- **Intent Icon**: Icon indicating move type (sword, shield, buff, etc.)
- **Intent Number**: Damage amount on attack intents
- **Power Icons**: Row of power icons with amounts below enemy
- **Update Behavior**: HP real-time, block real-time, intent updates at turn start
- **Animation**: HP bar smooth drain, block shield slam in, intent shake on change

### Play Area Elements

#### Enemy Sprites
- **Visual Form**: Animated character sprite (Spine/MegaSpine)
- **Update Behavior**: Idle animation, attack animation on action
- **Animation**: Hit flash on damage, death animation on defeat

#### Player Sprite
- **Visual Form**: Animated character sprite
- **Update Behavior**: Idle animation, action animations on card play
- **Animation**: Hit flash on damage, block pose when blocking

### Relic Bar Elements

#### Relic Icon (per relic)
- **Visual Form**: Square icon (~48px), rarity-colored border
- **Counter**: Number badge on counter-based relics (e.g., Shuriken: 2/3)
- **Used-Up State**: Grayscale filter when depleted
- **Update Behavior**: Counter updates on trigger
- **Animation**: Flash on trigger, pulse on obtain
- **Interaction**: Hover for tooltip with name and effect

### Hand Area Elements

#### Hand Card (per card)
- **Visual Form**: Card frame (~180x250px), type-colored border, art, cost, name, description
- **Energy Cost**: Top-left corner, circular badge
- **Keywords**: Bold text in description
- **Unplayable State**: Darkened, red X on cost
- **Update Behavior**: Cost updates on modifiers
- **Animation**: Fan layout adjusts on add/remove, selected card lifts and enlarges, hover shows full preview
- **Interaction**: Click to select, drag to target, right-click for full preview

### Energy Orb
- **Visual Form**: Circular orb (~80px diameter), energy-colored fill
- **Content**: Current energy / Max energy
- **Update Behavior**: Real-time on energy spend/gain
- **Animation**: Pulse on change, glow when full, drain animation on spend

### Player Stats Elements

#### HP Bar
- **Visual Form**: Horizontal bar, red fill, dark red background
- **Content**: Current HP / Max HP
- **Damage Preview**: Gray segment showing incoming damage on hover
- **Update Behavior**: Real-time on damage/heal
- **Animation**: Smooth drain on damage, smooth fill on heal, shake on low HP

#### Block Display
- **Visual Form**: Shield icon with number
- **Content**: Current block amount
- **Update Behavior**: Real-time on block gain/damage
- **Animation**: Slam in on gain, shrink on damage, disappear at 0

#### Power Icons (player)
- **Visual Form**: Row of power icons with amounts
- **Content**: Power icon + stack count
- **Update Behavior**: Real-time on power apply/remove
- **Animation**: Flash on apply, fade out on remove
- **Interaction**: Hover for tooltip

### Pile Count Elements

#### Draw Pile
- **Visual Form**: Face-down card stack icon + count number
- **Content**: Cards in draw pile
- **Update Behavior**: Updates on draw/shuffle
- **Animation**: Shuffle animation when discard shuffled in
- **Interaction**: Click to view draw pile contents (top X cards)

#### Discard Pile
- **Visual Form**: Face-up card stack icon + count number
- **Content**: Cards in discard pile
- **Update Behavior**: Updates on discard
- **Animation**: None
- **Interaction**: Click to view discard pile contents

#### Exhaust Pile
- **Visual Form**: Exhausted card icon + count number
- **Content**: Cards exhausted this combat
- **Update Behavior**: Updates on exhaust
- **Animation**: None
- **Interaction**: Click to view exhaust pile contents

### Potion Slot Elements

#### Potion Slot (per slot)
- **Visual Form**: Circular slot (~60px diameter)
- **Filled**: Potion icon with rarity-colored glow
- **Empty**: Darkened placeholder
- **Update Behavior**: Updates on obtain/use
- **Animation**: Potion slam in on obtain, throw animation on use
- **Interaction**: Click to use (targeted potions require target selection), hover for tooltip

### Turn Info Elements

#### Round Number
- **Visual Form**: Small text "Round X"
- **Content**: Current round number
- **Update Behavior**: Increments at round start
- **Animation**: None

#### Turn Indicator
- **Visual Form**: Text or icon indicating current turn
- **Content**: "Your Turn" or "Enemy Turn"
- **Update Behavior**: Changes on turn switch
- **Animation**: Flash on turn change

---

## Dynamic Behaviors

### HUD Density by Game State

| Game State | HUD Density | Visible Elements |
|------------|-------------|------------------|
| **Combat** | Full | All elements visible |
| **Map Navigation** | Minimal | Gold, HP/Max HP, Floor/Act only |
| **Shop/Event** | Minimal | Gold only |
| **Menu/Pause** | Hidden | None (menu overlays) |
| **Reward Screen** | Minimal | Gold, HP only |
| **Deck View** | Hidden | Deck viewer modal replaces HUD |

### Combat HUD Transitions

#### Combat Start
1. Top bar fades in with enemy info
2. Play area shows enemies and player
3. Relic bar slides up from bottom
4. Hand deals 5 cards with fan animation
5. Energy orb fills with glow
6. Turn indicator shows "Your Turn"

#### Turn Transition (Player → Enemy)
1. Turn indicator flashes "Enemy Turn"
2. Hand cards dim (non-interactive)
3. Enemy intents highlight
4. Energy orb drains to 0

#### Turn Transition (Enemy → Player)
1. Turn indicator flashes "Your Turn"
2. Hand cards brighten (interactive)
3. Energy orb refills
4. Draw pile count decrements, hand adds cards

#### Combat End (Victory)
1. All enemies play defeat animation
2. "VICTORY" banner appears
3. HUD fades except gold (updates on reward)
4. Transition to reward screen

#### Combat End (Defeat)
1. Player sprite plays defeat animation
2. "DEFEAT" banner appears
3. HUD fades completely
4. Transition to run summary

### In-Combat Dynamic Behaviors

| Trigger | Behavior |
|---------|----------|
| HP drops below 25% | HP bar pulses red, warning sound |
| Block reaches 0 | Block shield fades out |
| Hand reaches 10 cards | "Hand Full" indicator, overflow animation |
| Draw pile empty | Shuffle animation, discard count moves to draw |
| Energy reaches 0 | Energy orb dims |
| Power applied | Power icon slams in with flash |
| Relic triggers | Relic icon flashes, tooltip shows effect |
| Potion obtained | Empty slot fills with potion slam |
| Enemy dies | Enemy fades, remaining enemies reposition |

---

## Platform & Input Variants

### Platform Adaptations

| Platform | Resolution | HUD Scaling | Touch Targets | Notes |
|----------|------------|-------------|---------------|-------|
| **PC (Windows/Linux)** | 1920x1080 base | 1.0x | N/A (mouse) | Full HUD density |
| **PC (4K)** | 3840x2160 | 1.5x | N/A | Larger elements, same layout |
| **Mobile (Phone)** | 1080x1920 (portrait) | 0.8x | Min 44px | Relic bar wraps, hand smaller |
| **Mobile (Tablet)** | 2048x1536 | 1.0x | Min 44px | Same as PC, touch-friendly |

### Input Method Adaptations

#### Keyboard/Mouse (Primary)
- **Card Selection**: Click to select, click target to play
- **Card Preview**: Hover for tooltip, right-click for full preview
- **Potion Use**: Click potion, click target (if targeted)
- **Pile View**: Click pile icon to open modal
- **Relic/Potion Info**: Hover for tooltip
- **End Turn**: Click "End Turn" button or press Space/E

#### Gamepad (Full Support)
- **Navigation**: D-pad/Left stick moves focus through elements
- **Focus Order**: Hand cards → Potions → End Turn → Pile counts → Relics → Deck
- **Card Selection**: A button to select, A on target to play
- **Card Preview**: Hold X for full preview
- **Potion Use**: Navigate to potion, A to use, D-pad to select target
- **Pile View**: Navigate to pile, A to open modal
- **End Turn**: Start button or navigate to End Turn button + A
- **Focus Indicator**: Glowing border on focused element

#### Touch (Mobile)
- **Card Selection**: Tap to select, tap target to play
- **Card Preview**: Long-press for full preview
- **Potion Use**: Tap potion, tap target (if targeted)
- **Pile View**: Tap pile icon to open modal
- **Relic/Potion Info**: Tap for tooltip popup
- **End Turn**: Tap "End Turn" button
- **Scroll**: Relic bar horizontal scroll with swipe
- **Hand Scroll**: Swipe left/right if hand exceeds screen width

### Mobile-Specific Layout Changes

```
PORTRAIT MOBILE LAYOUT:

┌─────────────────────────┐
│ TOP BAR (compact)       │
│ Floor Act | Enemies     │
│ Gold | Deck             │
├─────────────────────────┤
│                         │
│    PLAY AREA            │
│                         │
├─────────────────────────┤
│ RELICS (wrap to 2 rows) │
├─────────────────────────┤
│ ENERGY | HAND | PILES   │
│ PLAYER |       | POTIONS│
│        |       | TURN   │
└─────────────────────────┘
```

- **Relic Bar**: Wraps to 2 rows if needed
- **Hand**: Smaller cards (~140x200px), scrollable if > 5 visible
- **Energy/Player**: Compact vertical stack
- **Potions/Piles**: Combined right column

---

## Accessibility

> ⚠️ **Note**: Accessibility tier not yet defined for this project. This section provides baseline WCAG-AA considerations. Run `/gate-check` to see whether this blocks any phase gates.

### Visual Accessibility

| Requirement | Implementation |
|-------------|----------------|
| **Text Contrast** | Minimum 4.5:1 contrast ratio for all text elements (WCAG-AA) |
| **Icon Contrast** | Minimum 3:1 contrast ratio for icons and UI elements |
| **Color Independence** | No information conveyed by color alone |
| **HP/Block Indicators** | Numbers displayed alongside bars, not just color fills |
| **Power Types** | Icons differentiated by shape, not just color (buff = upward arrow, debuff = downward arrow) |
| **Intent Types** | Each intent has unique icon shape, not just color |

### Cognitive Accessibility

| Requirement | Implementation |
|-------------|----------------|
| **Consistent Layout** | HUD elements never move position during combat |
| **Clear Hierarchy** | Must Show elements are larger and more prominent |
| **Information Grouping** | Related elements grouped in zones (player stats together, enemy info together) |
| **Preview on Hover** | Card effects previewable before committing |
| **Undo Prevention** | Clear confirmation for irreversible actions (End Turn, use limited potion) |

### Motor Accessibility

| Requirement | Implementation |
|-------------|----------------|
| **Keyboard Navigation** | Full keyboard-only path through all interactive elements |
| **Gamepad Navigation** | Full gamepad support with logical focus order |
| **Touch Targets** | Minimum 44px touch targets on mobile |
| **No Quick-Time Events** | All actions are turn-based, no time pressure |
| **No Precision Required** | Click/tap anywhere on card to select |

### Auditory Accessibility

| Requirement | Implementation |
|-------------|----------------|
| **Visual Feedback** | All audio cues have corresponding visual feedback |
| **Subtitle Options** | Combat sound effects need not be subtitled (informational only) |

### Screen Reader Considerations

| Element | Screen Reader Announcement |
|---------|---------------------------|
| **Card** | "[Card Name]. Cost: [X] energy. Type: [Attack/Skill/Power]. Effect: [description]. Target: [Self/Enemy/All Enemies]" |
| **Energy Orb** | "Energy: [X] of [Y]" |
| **HP Bar** | "Health: [X] of [Y]. [percentage]% remaining." |
| **Block** | "Block: [X]" (only when block > 0) |
| **Enemy** | "[Enemy Name]. Health: [X] of [Y]. Intent: [Attack for X damage / Buff / Debuff]. Powers: [list]" |
| **Relic** | "[Relic Name]. [Effect description]. Counter: [X] of [Y]" (if counter-based) |
| **Potion** | "[Potion Name]. [Effect description]. Target: [Self/Enemy]" |
| **Pile Count** | "Draw pile: [X] cards. Discard pile: [X] cards. Exhaust pile: [X] cards." |
| **Turn Indicator** | "Your turn" / "Enemy turn" |

### Reduced Motion

| Element | Reduced Motion Alternative |
|---------|---------------------------|
| **Card Fan Animation** | Instant position without animation |
| **HP Bar Drain** | Instant value change |
| **Energy Orb Pulse** | Static display |
| **Intent Shake** | No shake, just highlight |
| **VFX** | Disable combat VFX option |

### Open Accessibility Questions

- [ ] Define accessibility tier (WCAG-A, AA, or AAA)
- [ ] Determine colorblind simulation testing requirements
- [ ] Define screen reader testing scope (full combat flow?)
- [ ] Consider dyslexia-friendly font option

---

## Open Questions

1. **Player journey map not yet created** — designing without emotional context for player state during combat. Consider creating `design/player-journey.md` to establish player context.
2. **Accessibility tier not defined** — consider WCAG-AA as a baseline. Run `/gate-check` to see whether this blocks any phase gates.
3. **Interaction pattern library not created** — HUD interaction patterns (card hover, tooltip, target selection) should be formalized in `design/ux/interaction-patterns.md`.
4. **Game concept document missing** — no `design/gdd/game-concept.md` to validate HUD philosophy against.
5. **Multiplayer HUD layout** — how does the HUD change for 2-player co-op? Does each player see their own hand separately? Is there a shared energy pool or separate?
6. **Orb display** — the Defect character uses an orb system. How are orbs displayed in the HUD? (Not covered in current GDDs, likely needs a dedicated element)
7. **End Turn button** — exact placement and styling TBD. Should it be prominent (always visible) or contextual (only during player turn)?
8. **Card tooltip content depth** — how much information should the hover tooltip show? Full card text + current modified values? Or just the card name?
9. **Relic bar scrolling** — if a player collects 20+ relics, how does the bar handle overflow? Scroll, wrap, or paginate?
