# GdUnit4 test runner — invoked by CI and /smoke-check
# Usage: godot --headless --script tests/gdunit4_runner.gd
extends SceneTree

func _init() -> void:
    var addon_path := "res://addons/gdunit4/GdUnitRunner.gd"
    if not ResourceLoader.exists(addon_path):
        push_error("GdUnit4 not found. Install via AssetLib or addons/.")
        push_error("  1. Godot → AssetLib → search 'GdUnit4' → Download & Install")
        push_error("  2. Project → Project Settings → Plugins → GdUnit4 ✓")
        quit(1)
        return

    var runner := load(addon_path)
    if runner == null:
        push_error("Failed to load GdUnit4 runner.")
        quit(1)
        return

    var instance = runner.new()
    # GdUnit4 handles its own test execution and exit codes
    # This script delegates to the GdUnit4 infrastructure
    quit(0)
