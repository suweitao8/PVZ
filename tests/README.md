# Test Infrastructure

**Engine**: Godot 4.5
**Test Framework**: GdUnit4
**CI**: `.github/workflows/tests.yml`
**Setup date**: 2026-05-15

## Directory Layout

```
tests/
  unit/           # Isolated unit tests (formulas, state machines, logic)
  integration/    # Cross-system and save/load tests
  smoke/          # Critical path test list for /smoke-check gate
  evidence/       # Screenshot logs and manual test sign-off records
```

## Running Tests

### In Editor
1. Open Godot Editor
2. GdUnit4 panel appears at bottom (after plugin installation)
3. Click "Run All" or right-click a test file → "Run Tests"

### Command Line (Headless)
```bash
godot --headless --script tests/gdunit4_runner.gd
```

## Installing GdUnit4

1. Open Godot → AssetLib → search "GdUnit4" → Download & Install
2. Enable the plugin: Project → Project Settings → Plugins → GdUnit4 ✓
3. Restart the editor
4. Verify: `res://addons/gdunit4/` exists

## Test Naming

- **Files**: `[system]_[feature]_test.gd` or `[system]_[feature]_test.cs`
- **Functions**: `test_[scenario]_[expected]`
- **Example**: `combat_damage_test.cs` → `test_base_attack_returns_expected_damage()`

## Language Choice

This project uses C# as primary language. Tests should be written in C# when testing C# game logic, and GDScript when testing GDScript-specific functionality (editor plugins, tools).

### C# Test Example
```csharp
using GdUnit4;
using static GdUnit4.Assertions;

[TestSuite]
public class CombatDamageTest
{
    [TestCase]
    public void test_base_attack_returns_expected_damage()
    {
        var combat = new CombatState();
        AssertThat(combat.CalculateDamage(10, 5)).IsEqual(5);
    }
}
```

### GDScript Test Example
```gdscript
class_name CombatDamageTest
extends GdUnitTestSuite

func test_base_attack_returns_expected_damage():
    var combat = CombatState.new()
    assert_int(combat.calculate_damage(10, 5)).is_equal(5)
```

## Story Type → Test Evidence

| Story Type | Required Evidence | Location |
|---|---|---|
| Logic | Automated unit test — must pass | `tests/unit/[system]/` |
| Integration | Integration test OR playtest doc | `tests/integration/[system]/` |
| Visual/Feel | Screenshot + lead sign-off | `tests/evidence/` |
| UI | Manual walkthrough OR interaction test | `tests/evidence/` |
| Config/Data | Smoke check pass | `production/qa/smoke-*.md` |

## CI

Tests run automatically on every push to `main` and on every pull request.
A failed test suite blocks merging.

## Adding Your First Test

1. Create a directory for your system: `tests/unit/combat/`
2. Create a test file: `tests/unit/combat/damage_test.cs`
3. Write your test following the naming conventions above
4. Run tests to verify they pass
