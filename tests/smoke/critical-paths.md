# Smoke Test: Critical Paths

**Purpose**: Run these 10-15 checks in under 15 minutes before any QA hand-off.
**Run via**: `/smoke-check` (which reads this file)
**Update**: Add new entries when new core systems are implemented.

---

## Core Stability (always run)

| # | Check | Pass Criteria |
|---|-------|---------------|
| 1 | Game launches to main menu | No crash, main menu visible within 5s |
| 2 | New game starts | "New Game" button responds, game state initializes |
| 3 | Main menu navigation | All menu buttons respond, no freeze |

---

## Core Mechanics (update per sprint)

<!-- Add the primary mechanic for each sprint here as it is implemented -->

| # | Check | Pass Criteria | Status |
|---|-------|---------------|--------|
| 4 | Combat system basics | Player can play a card, enemy takes damage | 🔲 Not implemented |
| 5 | Deck management | Player can view deck, cards display correctly | 🔲 Not implemented |
| 6 | Map navigation | Player can select map nodes, transitions work | 🔲 Not implemented |

---

## Data Integrity

| # | Check | Pass Criteria | Status |
|---|-------|---------------|--------|
| 7 | Save game | Save completes without error | 🔲 Not implemented |
| 8 | Load game | Load restores correct state | 🔲 Not implemented |

---

## Performance

| # | Check | Pass Criteria |
|---|-------|---------------|
| 9 | Frame rate | Consistent 60fps on target hardware (no visible drops) |
| 10 | Memory stability | No memory growth over 5 minutes of gameplay |

---

## Audio/Visual

| # | Check | Pass Criteria |
|---|-------|---------------|
| 11 | Music plays | Background music audible from main menu |
| 12 | SFX triggers | Sound effects play on button clicks |

---

## Smoke Test Run Log

| Date | Build | Tester | Result | Notes |
|------|-------|--------|--------|-------|
| | | | | |
