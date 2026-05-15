# Unit tests go here — one subdirectory per system (e.g., tests/unit/combat/)

## Guidelines

- Unit tests are isolated: no file I/O, no network, no external dependencies
- Each test sets up and tears down its own state
- Test naming: `[system]_[feature]_test.[ext]`
- Use dependency injection for testability

## Example Structure

```
tests/unit/
  combat/
    damage_test.cs
    card_play_test.cs
  deck/
    shuffle_test.cs
    draw_test.cs
  map/
    node_generation_test.cs
```
