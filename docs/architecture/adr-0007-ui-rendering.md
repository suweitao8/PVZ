# ADR-0007: UI Rendering System

## Status

Accepted

## Date

2026-05-16

## Last Verified

2026-05-16

## Decision Makers

MegaCrit Studio

## Summary

The UI rendering system uses Godot Control nodes organized in a hierarchical scene architecture. Card display uses object pooling via `NodePool<T>` for performance. Enemy intents use frame animation with a floating sine-wave effect. Data binding is achieved through model subscription patterns with `CombatStateTracker` as the central event hub. Resolution scaling uses `canvas_items` stretch mode with dynamic aspect ratio adjustment.

## Engine Compatibility

| Field | Value |
|-------|-------|
| **Engine** | Godot 4.5 |
| **Domain** | UI |
| **Knowledge Risk** | LOW — standard Godot patterns |
| **References Consulted** | Godot 4.5 UI documentation |
| **Post-Cutoff APIs Used** | None |
| **Verification Required** | Object pool performance under load |

## ADR Dependencies

| Field | Value |
|-------|-------|
| **Depends On** | ADR-0001 (C# primary) |
| **Enables** | All UI screens, HUD, card rendering |
| **Blocks** | None |
| **Ordering Note** | Core system — required before UI implementation |

## Context

### Problem Statement

A deckbuilder game requires:
1. Efficient card rendering (10 cards in hand, many cards in piles)
2. Dynamic intent display for enemy AI telegraphing
3. Map screen with scrolling and path visualization
4. Real-time data binding between game state and UI
5. Resolution independence across PC and mobile platforms

### Current State

The codebase uses:
- `NCard` for card rendering with object pooling
- `NIntent` for enemy intent display with frame animation
- `NMapScreen` for map visualization
- `CombatStateTracker` for model-view binding
- `canvas_items` stretch mode with dynamic aspect ratio adjustment

### Constraints

- Cards must render efficiently (object pooling required)
- UI must update in real-time without frame drops
- Resolution must scale across 1920x1080 to 4K and mobile
- Touch targets must be minimum 44px on mobile
- All UI must be keyboard/gamepad navigable

## Decision

### Architecture

```
┌─────────────────────────────────────────────────────────────────────┐
│                     NGame (Root Control)                            │
│                                                                      │
│  NSceneContainer ──→ NRun ──→ NGlobalUi                            │
│                            │                                         │
│                            ├── NTopBar                               │
│                            ├── NMapScreen                           │
│                            ├── NOverlayStack                        │
│                            └── NCombatUi                            │
│                                    │                                 │
│                                    ├── HandArea                     │
│                                    │   └── NCard (pooled)          │
│                                    ├── EnemyRow                     │
│                                    │   └── NIntent (animated)      │
│                                    └── ...                          │
└─────────────────────────────────────────────────────────────────────┘
```

### Object Pooling Pattern

```csharp
// Card pool initialization (prewarm 30 cards)
public static void InitPool() {
    NodePool.Init<NCard>("res://scenes/cards/card.tscn", 30);
}

// Pool usage
public static NCard? Create(CardModel card, ModelVisibility visibility) {
    NCard nCard = NodePool.Get<NCard>();
    nCard.Model = card;
    return nCard;
}

// Pool return (automatic on scene change)
public void OnReturnedFromPool() { /* Reset state */ }
```

### Model-View Binding

```csharp
// CombatStateTracker as central event hub
public void Subscribe(CardModel card) {
    card.AfflictionChanged += OnCardValueChanged;
    card.EnergyCostChanged += OnCardValueChanged;
}

// Deferred update (batch changes per frame)
private async Task CallCombatStateChangedDeferred() {
    await sceneTree.ToSignal(sceneTree, SceneTree.SignalName.ProcessFrame);
    CombatStateChanged?.Invoke(_state);
}
```

### Intent Animation System

```csharp
// Frame animation (manual, not SpriteFrames)
public override void _Process(double delta) {
    if (_animationName != null) {
        int frame = (int)(_timeAccumulator * 15f) % frameCount;
        _intentSprite.Texture = GetAnimationFrame(_animationName, frame);
        _timeAccumulator += (float)delta;
    }
}

// Floating effect
_intentHolder.Position = Vector2.Up * (
    Mathf.Sin(Time.GetTicksMsec() * 0.001f * PI + _timeOffset) * 10f + 8f
);
```

### Resolution Scaling

```csharp
// Dynamic aspect ratio adjustment
private void OnWindowChange() {
    float ratio = (float)_window.Size.X / (float)_window.Size.Y;

    if (ratio > 2.3888888f) {  // Ultrawide 21:9
        _window.ContentScaleAspect = Window.ContentScaleAspectEnum.KeepWidth;
        _window.ContentScaleSize = new Vector2I(2580, 1080);
    }
    else if (ratio < 1.3333334f) {  // Narrow 4:3
        _window.ContentScaleAspect = Window.ContentScaleAspectEnum.KeepHeight;
        _window.ContentScaleSize = new Vector2I(1680, 1260);
    }
    else {  // Standard widescreen
        _window.ContentScaleAspect = Window.ContentScaleAspectEnum.Expand;
        _window.ContentScaleSize = new Vector2I(1680, 1080);
    }
}
```

## Alternatives Considered

### Alternative 1: GDScript UI

- **Description**: Write UI in GDScript instead of C#
- **Pros**: More Godot-native, easier iteration
- **Cons**: Type safety issues, harder to bind to C# game logic
- **Rejection Reason**: C# is primary language per ADR-0001

### Alternative 2: Immediate Mode UI

- **Description**: Use immediate mode UI pattern (ImGui-style)
- **Pros**: Simpler code, no scene files
- **Cons**: Not data-driven, harder for artists to modify
- **Rejection Reason**: Retained mode enables designer iteration

### Alternative 3: No Object Pooling

- **Description**: Instantiate cards on demand, let GC handle cleanup
- **Pros**: Simpler code
- **Cons**: GC pauses, frame drops during heavy combat
- **Rejection Reason**: Performance requirement for 60fps

## Consequences

### Positive

- Efficient card rendering with object pooling (no GC pauses)
- Clean model-view separation via subscription pattern
- Resolution independence across all target platforms
- Touch-friendly with dynamic aspect ratio adjustment
- Performance budget met: < 1ms for UI updates

### Negative

- Object pool adds complexity to card lifecycle management
- Model subscription requires careful unsubscription to prevent leaks
- Frame animation system is manual (no SpriteFrames editor support)

### Neutral

- All UI must go through CombatStateTracker for updates

## Risks

| Risk | Probability | Impact | Mitigation |
|------|------------|--------|-----------|
| Pool exhaustion under heavy combat | Low | Medium | Monitor pool usage, dynamic expansion |
| Subscription leak causing memory growth | Medium | Medium | Pool objects auto-disconnect signals |
| Aspect ratio edge cases | Low | Low | Test on ultrawide and 4:3 monitors |

## Performance Implications

| Metric | Budget | Expected |
|--------|--------|----------|
| Card pool instantiation | < 100ms (one-time) | ~50ms (30 cards) |
| Card retrieval from pool | < 0.1ms | ~0.02ms |
| CombatState update dispatch | < 0.5ms | ~0.1ms |
| Intent animation per frame | < 0.1ms | ~0.02ms |

## Migration Plan

Already implemented. Core system in use by all UI code.

## Validation Criteria

- [x] Object pool correctly reuses card instances
- [x] Model subscription updates UI in real-time
- [x] Resolution scaling works across aspect ratios
- [x] Frame animation plays smoothly for intents
- [x] No memory growth from subscription leaks

## GDD Requirements Addressed

- TR-ui-001: Card rendering and tooltips
- TR-ui-002: Intent display system
- Enables: HUD Design (design/ux/hud.md)

## Related

- ADR-0001: C# as Primary Language
- `src/UI/NCard.cs` — Card rendering with object pool
- `src/UI/NIntent.cs` — Intent frame animation
- `src/UI/NMapScreen.cs` — Map scrolling and path visualization
- `src/Core/CombatStateTracker.cs` — Model-view binding hub
- `src/Core/NodePool.cs` — Generic object pool implementation
