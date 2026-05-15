# ADR-0003: MegaSpine Binding Pattern

## Status

Accepted

## Date

2026-05-15

## Last Verified

2026-05-15

## Decision Makers

MegaCrit Studio

## Summary

Spine animation integration uses a C# wrapper pattern (MegaSpineBinding) that wraps GDExtension Spine objects with type-safe C# interfaces. Each Spine type (Skeleton, Animation, Bone, Skin, etc.) has a corresponding binding class that validates the wrapped object, whitelists callable methods, and provides a safe C# API surface over the untyped GDExtension boundary.

## Engine Compatibility

| Field | Value |
|-------|-------|
| **Engine** | Godot 4.5 |
| **Domain** | Animation |
| **Knowledge Risk** | MEDIUM — Spine GDExtension API near cutoff |
| **References Consulted** | `docs/engine-reference/godot/VERSION.md` |
| **Post-Cutoff APIs Used** | None for binding layer; Spine runtime version must be verified |
| **Verification Required** | Spine GDExtension loads correctly in Godot 4.5, signal names match |

## ADR Dependencies

| Field | Value |
|-------|-------|
| **Depends On** | ADR-0001 (C# primary — binding classes are C#) |
| **Enables** | Creature animation system, VFX animation system |
| **Blocks** | None |
| **Ordering Note** | Must exist before creature rendering implementation |

## Context

### Problem Statement

Spine 2D animation runs as a GDExtension (C++ plugin) in Godot. GDExtension objects appear as untyped `GodotObject` in C#. Calling methods by string name is error-prone and provides no compile-time safety. We need a type-safe way to interact with Spine from C# game code.

### Current State

`src/Core/Bindings/MegaSpine/` contains 12 binding classes:
- `MegaSpineBinding` (abstract base)
- `MegaSkeleton`, `MegaAnimation`, `MegaAnimationState`
- `MegaBone`, `MegaBoneData`, `MegaSkin`
- `MegaEvent`, `MegaEventData`
- `MegaSlotNode`, `MegaSprite`
- `MegaTrackEntry`
- `MegaSkeletonDataResource`

Each binding wraps a `GodotObject` and validates:
1. The object's class name matches expected Spine class
2. All declared methods exist on the object
3. All declared signals exist on the object

### Constraints

- Spine GDExtension provides no C# API — only GDScript/class_name access
- Method calls must go through `GodotObject.Call(string, Variant[])`
- Signal connections use string-based `Connect(string, Callable)`
- Spine objects cannot be created from C# — only obtained from scene tree

### Requirements

- Type-safe Spine API from C#
- Runtime validation that Spine methods/signals exist
- Clear error messages when Spine API changes
- No performance overhead beyond the GDExtension call boundary

## Decision

Use the MegaSpineBinding wrapper pattern:
1. Abstract base class `MegaSpineBinding` holds the `GodotObject` reference
2. Subclasses declare `SpineClassName`, `SpineMethods`, `SpineSignals`
3. Constructor validates the bound object matches expectations
4. Protected `Call()` method checks method whitelist before invoking
5. Protected `Connect()`/`Disconnect()` wrap signal operations

### Architecture

```
┌──────────────────────────────────────────────────┐
│                   C# Game Code                    │
│          (MegaCrit.Sts2.Core.Entities)            │
└──────────────┬───────────────────────────────────┘
               │ uses type-safe API
               ▼
┌──────────────────────────────────────────────────┐
│            MegaSpine Bindings                     │
│  ┌──────────────┐ ┌───────────────┐              │
│  │ MegaSkeleton │ │ MegaAnimation │ ...          │
│  └──────┬───────┘ └───────┬───────┘              │
│         │                 │                       │
│  ┌──────┴─────────────────┴───────┐              │
│  │   MegaSpineBinding (abstract)  │              │
│  │  - ValidateBoundObject()       │              │
│  │  - Call(method, args)          │              │
│  │  - Connect(signal, callable)   │              │
│  └──────────────┬─────────────────┘              │
└─────────────────┼────────────────────────────────┘
                  │ GodotObject.Call(string, Variant[])
                  ▼
┌──────────────────────────────────────────────────┐
│        Spine GDExtension (C++ Runtime)            │
│        (spine_godot runtime objects)              │
└──────────────────────────────────────────────────┘
```

### Key Interfaces

```csharp
public abstract class MegaSpineBinding
{
    public GodotObject BoundObject { get; }
    protected abstract string SpineClassName { get; }
    protected virtual IEnumerable<string> SpineMethods => Array.Empty<string>();
    protected virtual IEnumerable<string> SpineSignals => Array.Empty<string>();

    // Safe method call with whitelist check
    protected Variant Call(string methodName, params Variant[] args);

    // Signal operations
    protected Error Connect(string signalName, Callable callable);
    protected void Disconnect(string signalName, Callable callable);
}

// Example concrete binding
public class MegaSkeleton : MegaSpineBinding
{
    protected override string SpineClassName => "SpineSkeleton";
    protected override IEnumerable<string> SpineMethods =>
        new[] { "set_animation", "get_animation_state", ... };
}
```

### Implementation Guidelines

1. One binding class per Spine runtime type
2. Add every used method/signal to the whitelist — unlisted calls throw
3. Use `CallNullable()` for methods that may return null
4. Validate in constructor — fail fast on Spine API mismatch
5. Document Spine version compatibility in each binding

## Alternatives Considered

### Alternative 1: Direct GodotObject.Call()

- **Description**: Call Spine methods directly via `node.Call("method")` everywhere
- **Pros**: No wrapper overhead, simpler initially
- **Cons**: No type safety, no validation, string typos crash at runtime
- **Estimated Effort**: Lower initially, higher long-term
- **Rejection Reason**: Error-prone at scale (12+ Spine types, 50+ methods)

### Alternative 2: Generate C# bindings from Spine API

- **Description**: Auto-generate C# wrappers from Spine runtime headers
- **Pros**: Always in sync with Spine API, zero manual maintenance
- **Cons**: Complex build step, generated code is hard to debug
- **Estimated Effort**: Higher initial setup
- **Rejection Reason**: Over-engineering for current needs; manual bindings are manageable

## Consequences

### Positive

- Compile-time type safety for all Spine interactions
- Clear error messages when Spine GDExtension API changes
- Self-documenting: `SpineMethods`/`SpineSignals` list the used API surface
- Easy to audit Spine API usage across the codebase

### Negative

- Every new Spine method/signal requires updating the binding whitelist
- Small overhead per call (whitelist check)
- Binding classes must be kept in sync with Spine runtime version

### Neutral

- Binding pattern is reusable for other GDExtensions

## Risks

| Risk | Probability | Impact | Mitigation |
|------|------------|--------|-----------|
| Spine GDExtension API changes between versions | Medium | High | Constructor validation fails loudly |
| Missing method in whitelist causes runtime crash | Medium | Medium | Add integration test per binding |
| Performance overhead from whitelist check | Low | Low | Negligible vs. GDExtension call cost |

## Performance Implications

| Metric | Before | Expected After | Budget |
|--------|--------|---------------|--------|
| CPU (per Spine call) | N/A | +0.001ms (whitelist check) | <0.1ms per call |
| Memory (per binding) | N/A | ~200 bytes | Negligible |

## Migration Plan

Already implemented. 12 binding classes exist in `src/Core/Bindings/MegaSpine/`.

**Rollback plan**: Not applicable (core animation system)

## Validation Criteria

- [x] All 12 binding classes validate their Spine objects
- [x] Method whitelist prevents calling undeclared methods
- [x] Spine animations play correctly through binding API
- [x] Constructor validation catches API mismatches

## GDD Requirements Addressed

Foundational — no GDD requirement. Enables: Creature Animation System, VFX Animation System

## Related

- ADR-0001: C# as Primary Language (binding classes are C#)
- `src/Core/Bindings/MegaSpine/` — Implementation directory
- `src/Core/Animation/CreatureAnimator.cs` — Primary consumer
