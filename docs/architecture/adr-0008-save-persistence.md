# ADR-0008: Save/Persistence System

## Status

Accepted

## Date

2026-05-16

## Last Verified

2026-05-16

## Decision Makers

MegaCrit Studio

## Summary

Save persistence uses JSON serialization via `System.Text.Json` with source generators for AOT compatibility. Saves are stored via Godot `user://` protocol with platform-specific paths (Steam, Default, Editor). Mid-combat saves store pre-finished room state for exact combat resume. Multiplayer uses XXHash32 checksum validation to detect state divergence. RNG persistence uses seed + counter approach for deterministic replay across all RNG streams.

## Engine Compatibility

| Field | Value |
|-------|-------|
| **Engine** | Godot 4.5 |
| **Domain** | Persistence |
| **Knowledge Risk** | LOW — standard C# patterns |
| **References Consulted** | Godot FileAccess documentation |
| **Post-Cutoff APIs Used** | None |
| **Verification Required** | Save/load round-trip integrity |

## ADR Dependencies

| Field | Value |
|-------|-------|
| **Depends On** | ADR-0001 (C# primary), ADR-0004 (CombatManager for combat state) |
| **Enables** | Run persistence, multiplayer sync, cloud saves |
| **Blocks** | None |
| **Ordering Note** | Core system — required for run state persistence |

## Context

### Problem Statement

A roguelike deckbuilder requires:
1. Persistent run state across sessions (deck, relics, HP, gold, map position)
2. Mid-combat save capability for mobile/resume scenarios
3. Deterministic RNG for multiplayer synchronization and replay
4. Schema migration for backward compatibility across versions
5. Cloud save integration for Steam

### Current State

The codebase uses:
- `System.Text.Json` with `MegaCritSerializerContext` source generator
- `[SavedProperty]` attribute for marking persistable properties
- `UserDataPathProvider` for platform-specific paths
- `ChecksumTracker` for multiplayer state validation
- `RunRngSet` for multi-stream RNG with seed + counter

### Constraints

- Saves must be human-readable (JSON) for debugging
- Mid-combat saves must resume combat exactly
- Multiplayer must detect and handle state divergence
- RNG must be deterministic across all clients
- Schema migrations must be backward compatible

## Decision

### Serialization Architecture

```csharp
// Source generator for AOT-friendly serialization
[JsonSerializable(typeof(SerializableRun))]
[JsonSerializable(typeof(SerializablePlayer))]
public partial class MegaCritSerializerContext : JsonSerializerContext { }

// Serialization with options
public static string Serialize<T>(T value) {
    return JsonSerializer.Serialize(value, MegaCritSerializerContext.Default.T);
}

// Deserialization with migration
public static T? Deserialize<T>(string json) {
    json = MigrationManager.ApplyMigrations(json);
    return JsonSerializer.Deserialize(json, MegaCritSerializerContext.Default.T);
}
```

### Saved Property Pattern

```csharp
// Mark properties for persistence
[SavedProperty]
public int Counter { get; set; }

[SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
public int OptionalValue { get; set; }  // Skipped if 0

// SavedProperties container for type-erased storage
public class SavedProperties {
    public void SetInt(string key, int value);
    public int? GetInt(string key);
    // Supports: int, bool, string, int[], ModelId, SerializableCard, etc.
}
```

### Storage Paths

```
user://{platform}/{userId}/{profileDir}/{dataType}

Examples:
  Steam:     user://steam/{steamId}/profile1/saves/current_run.save
  Default:   user://default/{userId}/profile1/saves/current_run.save
  Editor:    user://editor/{userId}/profile2/saves/current_run.save
  Modded:    user://steam/{steamId}/profile1/modded/saves/current_run.save
```

### Mid-Combat Save Pattern

```csharp
// Save with pre-finished room for combat resume
public SerializableRun ToSave(AbstractRoom? preFinishedRoom) {
    return new SerializableRun {
        // ... run state ...
        PreFinishedRoom = preFinishedRoom?.ToSerializable()
    };
}

// Load restores room stack
public void LoadIntoLatestMapCoord(SerializableRoom? preFinishedRoom) {
    if (preFinishedRoom != null) {
        // Restore combat state exactly
        CombatManager.RestoreFromSave(preFinishedRoom.CombatState);
    }
}
```

### Multiplayer Checksum Validation

```csharp
// Generate checksum after each action
public uint GenerateChecksum(NetFullCombatState state) {
    _packetWriter.Reset();
    _packetWriter.Write(state);
    return XxHash32.HashToUInt32(_packetWriter.Buffer);
}

// Client sends checksum to host
public struct NetChecksumData {
    public uint id;        // Sequential ID
    public uint checksum;  // XXHash32 of serialized state
}

// Host validates and responds
if (clientChecksum != hostChecksum) {
    SendStateDivergenceMessage(fullState);
}
```

### RNG Persistence

```csharp
// Seed + counter approach
public class Rng {
    public uint Seed { get; }           // Immutable
    public int Counter { get; private set; }  // Calls made

    // Fast-forward to sync with other clients
    public void FastForwardCounter(int targetCount) {
        while (Counter < targetCount) {
            Counter++;
            _random.Next();
        }
    }
}

// Multiple independent streams
public class RunRngSet {
    public Rng UpFront { get; }              // Map generation
    public Rng Shuffle { get; }              // Deck shuffling
    public Rng CombatCardGeneration { get; } // Card rewards
    public Rng MonsterAi { get; }            // Enemy moves
    // ... 12 total streams
}

// Serializable state
public class SerializableRunRngSet {
    public string? Seed { get; set; }
    public Dictionary<RunRngType, int> Counters { get; set; }
}
```

### Schema Migration

```csharp
// Migration registration
[Migration(fromVersion: 1, toVersion: 2)]
public static string MigrateV1ToV2(string json) {
    // Apply transformation
    return updatedJson;
}

// Sequential migration chain
public static string ApplyMigrations(string json) {
    int version = ExtractVersion(json);
    while (version < CurrentVersion) {
        json = _migrations[(version, version + 1)](json);
        version++;
    }
    return json;
}
```

## Alternatives Considered

### Alternative 1: Binary Serialization

- **Description**: Use Protocol Buffers or MessagePack
- **Pros**: Smaller file size, faster serialization
- **Cons**: Not human-readable, harder to debug
- **Rejection Reason**: JSON readability critical for debugging and migration

### Alternative 2: Godot Resource Save

- **Description**: Use Godot's built-in .tres/.res resource format
- **Pros**: Native Godot integration
- **Cons**: Not cross-platform, harder to version, ties to engine version
- **Rejection Reason**: Need engine-agnostic format for cloud saves and debugging

### Alternative 3: Single RNG Stream

- **Description**: Use single RNG for all randomness
- **Pros**: Simpler implementation
- **Cons**: Actions in one system affect all others (non-deterministic ordering)
- **Rejection Reason**: Multi-stream enables deterministic replay independent of action order

## Consequences

### Positive

- Human-readable saves for debugging and manual recovery
- Deterministic RNG across all clients via seed + counter
- Exact combat resume from mid-combat saves
- Backward compatible via schema migration
- Steam cloud save integration

### Negative

- JSON files are larger than binary (~100KB for run save)
- Source generator adds build complexity
- RNG fast-forward can be slow if counter gap is large

### Neutral

- All RNG must go through RunRngSet for determinism

## Risks

| Risk | Probability | Impact | Mitigation |
|------|------------|--------|-----------|
| Schema migration failure | Low | High | Backup file before migration, partial recovery |
| RNG counter overflow | Very Low | High | Use 64-bit counter, validate on load |
| Checksum false positive | Very Low | Medium | Include full state in divergence message |
| Cloud save conflict | Medium | Medium | Last-write-wins with timestamp check |

## Performance Implications

| Metric | Budget | Expected |
|--------|--------|----------|
| Run save serialization | < 50ms | ~20ms |
| Run load + migration | < 100ms | ~50ms |
| Checksum generation | < 5ms | ~1ms |
| RNG fast-forward (1000 calls) | < 1ms | ~0.3ms |

## Migration Plan

Already implemented. Core system in use by all save/load code.

## Validation Criteria

- [x] Save/load round-trip preserves all state
- [x] Mid-combat save resumes combat exactly
- [x] RNG determinism across clients
- [x] Schema migration handles version changes
- [x] Checksum detects state divergence

## GDD Requirements Addressed

- TR-save-001: Run state persistence
- TR-rng-001: Deterministic RNG system
- Enables: Multiplayer sync, cloud saves

## Related

- ADR-0001: C# as Primary Language
- ADR-0004: CombatManager Singleton
- `src/Core/Save/SaveManager.cs` — Central save coordinator
- `src/Core/Save/JsonSerializationUtility.cs` — JSON helpers
- `src/Core/Save/SavedPropertyAttribute.cs` — Property marking
- `src/Core/Rng/RunRngSet.cs` — Multi-stream RNG
- `src/Core/Net/ChecksumTracker.cs` — Multiplayer validation
- `src/Core/Migration/MigrationManager.cs` — Schema migration
