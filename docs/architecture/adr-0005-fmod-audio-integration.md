# ADR-0005: FMOD Audio Middleware Integration

## Status

Accepted

## Date

2026-05-15

## Last Verified

2026-05-15

## Decision Makers

MegaCrit Studio

## Summary

Audio is handled through FMOD middleware rather than Godot's built-in audio system. FMOD provides advanced features (real-time parameter control, interactive music, snapshot mixing, bank-based loading) essential for a dynamic roguelike audio experience. A GDScript autoload (`FmodManager.gd`) bridges the FMOD GDExtension to C# game code via proxy classes.

## Engine Compatibility

| Field | Value |
|-------|-------|
| **Engine** | Godot 4.5 |
| **Domain** | Audio |
| **Knowledge Risk** | MEDIUM — FMOD GDExtension API near cutoff |
| **References Consulted** | `addons/fmod/plugin.cfg` — FMOD Godot 4.x integration |
| **Post-Cutoff APIs Used** | FMOD 2.02.x GDExtension for Godot 4.x |
| **Verification Required** | FMOD banks load correctly, parameters update in real-time |

## ADR Dependencies

| Field | Value |
|-------|-------|
| **Depends On** | ADR-0001 (C# primary — requires GDScript bridge) |
| **Enables** | Dynamic music, combat audio, UI sound design |
| **Blocks** | None |
| **Ordering Note** | Audio system — independent of game logic |

## Context

### Problem Statement

A roguelike deckbuilder needs dynamic audio:
- Music that adapts to combat intensity
- Sound effects that vary based on damage type/target
- UI sounds that respect player settings
- Snapshot-based mixing for menus/pauses
- Efficient memory via bank-based loading

Godot's built-in audio is sufficient for simple games but lacks:
- Real-time parameter control (RTPCs)
- Interactive music transitions
- Snapshot mixing
- Professional audio tooling (FMOD Studio)

### Current State

FMOD integration via `addons/fmod/`:
- `FmodManager.gd` — GDScript autoload that initializes FMOD
- `fmod.gdextension` — GDExtension binding to FMOD runtime
- Banks stored in `banks/desktop/` and `banks/mobile/`
- C# proxies: `src/Core/Audio/FmodSfx.cs`, `src/gdscript/audio_manager_proxy.gd`

### Constraints

- FMOD GDExtension is GDScript-first (no C# API)
- Must use GDScript autoload to access FMOD from C#
- FMOD Studio project is external (separate from Godot project)
- Bank loading must be async for large banks

### Requirements

- Play one-shot and looping sounds
- Set real-time parameters (RTPCs) for dynamic audio
- Transition music based on game state
- Apply snapshots for menu/pause
- Load/unload banks dynamically
- Respect player audio settings (volume, mute)

## Decision

Use FMOD with a GDScript bridge pattern:
1. `FmodManager.gd` autoload initializes FMOD and provides GDScript API
2. `audio_manager_proxy.gd` wraps FmodManager for C# access
3. `FmodSfx.cs` provides C# types for sound definitions
4. Banks are loaded at startup (desktop) or streamed (mobile)

### Architecture

```
┌────────────────────────────────────────────────────────────┐
│                     C# Game Code                            │
│  ┌─────────────────┐  ┌──────────────────────────────┐     │
│  │ CombatManager   │  │ FmodSfx (sound definitions)  │     │
│  │ (triggers SFX)  │  │ - DamageSfxType enum         │     │
│  └────────┬────────┘  └──────────────┬───────────────┘     │
│           │                          │                      │
│           │ calls                    │ references           │
│           ▼                          ▼                      │
│  ┌─────────────────────────────────────────────────────┐   │
│  │        AudioManagerProxy (C# → GDScript bridge)     │   │
│  │  - PlayOneShot(soundPath)                           │   │
│  │  - SetParameter(paramName, value)                   │   │
│  │  - PlayMusic(musicPath)                             │   │
│  └──────────────────────┬──────────────────────────────┘   │
└─────────────────────────┼──────────────────────────────────┘
                          │ calls GDScript methods
                          ▼
┌────────────────────────────────────────────────────────────┐
│              FmodManager.gd (GDScript Autoload)            │
│  - FMOD init/shutdown                                      │
│  - Bank loading/unloading                                  │
│  - Event playback, parameter setting                       │
│  - Snapshot application                                    │
└──────────────────────────┬─────────────────────────────────┘
                           │ calls GDExtension
                           ▼
┌────────────────────────────────────────────────────────────┐
│              FMOD GDExtension (C++ Runtime)                │
│              (fmod.gdextension)                            │
└──────────────────────────┬─────────────────────────────────┘
                           │
                           ▼
┌────────────────────────────────────────────────────────────┐
│              FMOD Core & Studio Runtime                    │
│              (FMOD 2.02.x)                                 │
└────────────────────────────────────────────────────────────┘
```

### Key Interfaces

```gdscript
# FmodManager.gd (simplified)
extends Node

func play_one_shot(event_path: String) -> void:
    FMOD.play_one_shot(event_path)

func set_parameter(event_path: String, param_name: String, value: float) -> void:
    var event = FMOD.get_event(event_path)
    event.set_parameter_by_name(param_name, value)

func load_bank(bank_path: String) -> void:
    FMOD.load_bank(bank_path)
```

```csharp
// AudioManagerProxy (C# → GDScript bridge)
public class AudioManagerProxy : Node
{
    private GodotObject _fmodManager;

    public void PlayOneShot(string eventPath)
    {
        _fmodManager.Call("play_one_shot", eventPath);
    }

    public void SetParameter(string eventPath, string paramName, float value)
    {
        _fmodManager.Call("set_parameter", eventPath, paramName, value);
    }
}

// FmodSfx.cs — Sound definitions
public enum DamageSfxType
{
    Normal,
    Fire,
    Poison,
    // ...
}

public static class FmodSfx
{
    public static string GetDamageEventPath(DamageSfxType type) => type switch
    {
        DamageSfxType.Fire => "event:/combat/damage_fire",
        DamageSfxType.Poison => "event:/combat/damage_poison",
        _ => "event:/combat/damage_normal"
    };
}
```

### Implementation Guidelines

1. All FMOD calls go through `FmodManager.gd` or `AudioManagerProxy`
2. Never call FMOD GDExtension directly from C#
3. Define sound paths in `FmodSfx.cs` constants, not inline strings
4. Pre-load essential banks at startup
5. Use FMOD snapshots for menu/pause mixing

## Alternatives Considered

### Alternative 1: Godot Built-in Audio Only

- **Description**: Use Godot's AudioStreamPlayer, no middleware
- **Pros**: Simpler, no external dependency, better Godot integration
- **Cons**: No real-time parameters, no interactive music, limited mixing
- **Estimated Effort**: Lower initially, higher for advanced features
- **Rejection Reason**: Dynamic audio requirements need FMOD features

### Alternative 2: Wwise Instead of FMOD

- **Description**: Use Wwise audio middleware
- **Pros**: Similar features to FMOD, good authoring tools
- **Cons**: Smaller community, less Godot support, licensing differences
- **Estimated Effort**: Similar
- **Rejection Reason**: FMOD has better Godot ecosystem support

### Alternative 3: Direct FMOD C# Integration

- **Description**: Use FMOD C# API directly, bypass GDExtension
- **Pros**: No GDScript bridge, cleaner C# API
- **Cons**: Must manage FMOD lifecycle manually, conflicts with GDExtension
- **Estimated Effort**: Higher
- **Rejection Reason**: GDExtension is already integrated and working

## Consequences

### Positive

- Professional audio features (RTPCs, snapshots, interactive music)
- Audio designers can work in FMOD Studio independently
- Efficient bank-based memory management
- Industry-standard tooling and workflow

### Negative

- External dependency on FMOD runtime and Studio
- GDScript bridge adds complexity layer
- FMOD licensing for commercial projects
- Team must learn FMOD Studio

### Neutral

- FMOD Studio project is separate from Godot project (version control separate)

## Risks

| Risk | Probability | Impact | Mitigation |
|------|------------|--------|-----------|
| FMOD GDExtension breaks on Godot update | Medium | High | Pin Godot version, test FMOD on upgrade |
| FMOD bank loading delays audio | Low | Medium | Pre-load essential banks at startup |
| GDScript bridge performance overhead | Low | Low | Negligible vs. audio processing |
| FMOD licensing cost increase | Low | Medium | Monitor FMOD pricing, evaluate alternatives |

## Performance Implications

| Metric | Before | Expected After | Budget |
|--------|--------|---------------|--------|
| CPU (audio mixing) | N/A | FMOD handles efficiently | <5% CPU |
| Memory (banks) | N/A | ~50-200MB loaded banks | <300MB |
| Load time (banks) | N/A | ~1-3s for all banks | <5s |

## Migration Plan

Already implemented. FMOD integration is complete with banks in `banks/`.

**Rollback plan**: Could replace with Godot audio but would lose dynamic features

## Validation Criteria

- [x] FMOD initializes correctly via `FmodManager.gd` autoload
- [x] Banks load from `banks/desktop/` and `banks/mobile/`
- [x] One-shot sounds play correctly
- [x] Parameters update in real-time
- [x] Music transitions work
- [x] Snapshots apply for menu/pause

## GDD Requirements Addressed

Foundational — no GDD requirement. Enables: Combat Audio System, UI Audio, Music System

## Related

- ADR-0001: C# as Primary Language (requires GDScript bridge)
- `addons/fmod/` — FMOD GDExtension
- `src/Core/Audio/` — C# audio types
- `src/gdscript/audio_manager_proxy.gd` — Bridge to C#
- `banks/` — FMOD bank files
