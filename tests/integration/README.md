# Integration tests go here — one subdirectory per system

## Purpose

Integration tests verify that multiple systems work correctly together:
- Save/Load round-trips
- Combat → UI → Audio integration
- Card → Enemy → Status effect chains
- Multiplayer state synchronization

## Guidelines

- Integration tests may use file I/O and real Godot nodes
- Prefer test fixtures over production data
- Each test should clean up after itself
- Use `tests/fixtures/` for shared test data

## Example Structure

```
tests/integration/
  combat/
    full_combat_flow_test.cs
    enemy_ai_integration_test.cs
  save_load/
    save_restore_roundtrip_test.cs
  multiplayer/
    combat_sync_test.cs
```
