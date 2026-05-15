# Test Evidence Directory

This directory stores manual test evidence that cannot be automated:

- **Screenshots** — Visual verification of UI, animations, VFX
- **Sign-off records** — Lead approval for Visual/Feel stories
- **Playtest logs** — Notes from manual playtest sessions
- **Accessibility test records** — Keyboard navigation, screen reader tests

## Directory Structure

```
tests/evidence/
  [date]_[feature]/
    screenshot_001.png
    screenshot_002.png
    sign-off.md
```

## Sign-off Template

Create a `sign-off.md` file for Visual/Feel stories:

```markdown
# Visual/Feel Sign-off

**Feature**: [feature name]
**Story ID**: [story ID if applicable]
**Tester**: [name]
**Date**: [date]
**Build**: [build number or commit hash]

## Evidence

- [ ] Screenshot attached showing final result
- [ ] Animation/VFX plays correctly
- [ ] No visual artifacts or glitches

## Notes

[Any observations, edge cases, or follow-up items]

## Approval

- [ ] Lead sign-off obtained
- [ ] Ready for merge

**Approved by**: [name]
**Date**: [date]
```

## What Goes Where

| Evidence Type | Location |
|--------------|----------|
| Unit test output | Automated — `tests/unit/` |
| Integration test output | Automated — `tests/integration/` |
| Screenshots for UI/Visual stories | `tests/evidence/[date]_[feature]/` |
| Playtest notes | `tests/evidence/playtests/` |
| Accessibility test records | `tests/evidence/accessibility/` |
