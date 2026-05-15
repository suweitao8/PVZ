# Technical Preferences

<!-- Populated by /setup-engine. Updated as the user makes decisions throughout development. -->
<!-- All agents reference this file for project-specific standards and conventions. -->

## Engine & Language

- **Engine**: Godot 4.5
- **Language**: C# (primary), GDScript (secondary for tooling/testing)
- **Rendering**: Forward+ (D3D12 on Windows)
- **Physics**: Dummy (disabled - no physics simulation needed)

## Input & Platform

<!-- Written by /setup-engine. Read by /ux-design, /ux-review, /test-setup, /team-ui, and /dev-story -->
<!-- to scope interaction specs, test helpers, and implementation to the correct input methods. -->

- **Target Platforms**: PC (Windows, Linux), Mobile
- **Input Methods**: Keyboard/Mouse, Gamepad, Touch (Mixed)
- **Primary Input**: Keyboard/Mouse
- **Gamepad Support**: Full
- **Touch Support**: Full (Mobile)
- **Platform Notes**: 1920x1080 base resolution, canvas_items stretch mode

## Naming Conventions

- **Classes**: PascalCase (C# standard)
- **Variables**: camelCase for local/private, PascalCase for public properties
- **Signals/Events**: PascalCase (e.g., `OnCardPlayed`, `OnDamageDealt`)
- **Files**: PascalCase for C# classes, snake_case for GDScript
- **Scenes/Prefabs**: PascalCase.tscn
- **Constants**: PascalCase or ALL_CAPS for compile-time constants

## Performance Budgets

- **Target Framerate**: 60 FPS (PC), 30 FPS (Mobile)
- **Frame Budget**: 16.67ms (PC), 33.33ms (Mobile)
- **Draw Calls**: [To be profiled]
- **Memory Ceiling**: [To be profiled]

## Testing

- **Framework**: GDUnit4 (GDScript), NUnit (C#)
- **Minimum Coverage**: [To be configured]
- **Required Tests**: Balance formulas, gameplay systems, combat mechanics

## Forbidden Patterns

<!-- Add patterns that should never appear in this project's codebase -->
- [None configured yet — add as architectural decisions are made]

## Allowed Libraries / Addons

<!-- Add approved third-party dependencies here -->
- **FMOD** (audio middleware) - `addons/fmod/`
- **Sentry** (crash reporting) - `addons/sentry/`
- **Spine** (skeletal animation) - via MegaSpine binding
- **MegaText** (text rendering) - `addons/mega_text/`
- **Atlas Generator** - `addons/atlas_generator/`
- **Dev Tools** - `addons/dev_tools/`

## Architecture Decisions Log

<!-- Quick reference linking to full ADRs in docs/architecture/ -->

| ADR | Title | Status |
|-----|-------|--------|
| [ADR-0001](../docs/architecture/adr-0001-csharp-primary-language.md) | C# as Primary Language with GDScript for Tooling | Accepted |
| [ADR-0002](../docs/architecture/adr-0002-gameaction-async-queue.md) | GameAction Async Queue System | Accepted |
| [ADR-0003](../docs/architecture/adr-0003-megaspine-binding-pattern.md) | MegaSpine Binding Pattern | Accepted |
| [ADR-0004](../docs/architecture/adr-0004-combatmanager-singleton.md) | CombatManager Singleton Pattern | Accepted |
| [ADR-0005](../docs/architecture/adr-0005-fmod-audio-integration.md) | FMOD Audio Middleware Integration | Accepted |
| [ADR-0006](../docs/architecture/adr-0006-hook-system.md) | Hook System for Game Event Modification | Accepted |
| [ADR-0007](../docs/architecture/adr-0007-ui-rendering.md) | UI Rendering System | Accepted |
| [ADR-0008](../docs/architecture/adr-0008-save-persistence.md) | Save/Persistence System | Accepted |

> Use `/architecture-decision` to create new ADRs

## Engine Specialists

<!-- Written by /setup-engine when engine is configured. -->
<!-- Read by /code-review, /architecture-decision, /architecture-review, and team skills -->
<!-- to know which specialist to spawn for engine-specific validation. -->

- **Primary**: godot-specialist
- **Language/Code Specialist**: godot-csharp-specialist
- **Shader Specialist**: godot-shader-specialist
- **UI Specialist**: godot-specialist (UI in C#/tscn)
- **Additional Specialists**: godot-gdscript-specialist (for GDScript tooling)
- **Routing Notes**: C# is primary language. GDScript used only for editor plugins and testing scripts.

### File Extension Routing

<!-- Skills use this table to select the right specialist per file type. -->
<!-- If a row says [TO BE CONFIGURED], fall back to Primary for that file type. -->

| File Extension / Type | Specialist to Spawn |
|-----------------------|---------------------|
| Game code (primary language) | godot-csharp-specialist |
| Shader / material files | godot-shader-specialist |
| UI / screen files | godot-specialist |
| Scene / prefab / level files | godot-specialist |
| Native extension / plugin files | godot-gdextension-specialist |
| GDScript files | godot-gdscript-specialist |
| General architecture review | Primary |
