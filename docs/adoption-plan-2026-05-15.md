# Adoption Plan

> **Generated**: 2026-05-15
> **Project phase**: Production
> **Engine**: Godot 4.5 + C#
> **Template version**: v1.0+

Work through these steps in order. Check off each item as you complete it.
Re-run `/adopt` anytime to check remaining gaps.

---

## ✅ Step 0: Template Infrastructure (COMPLETED)

The following infrastructure has been installed:

- [x] `.claude/agents/` — 49 agent definitions
- [x] `.claude/docs/` — Documentation and templates
- [x] `.claude/hooks/` — Automation hooks
- [x] `.claude/rules/` — Rules files
- [x] `.claude/skills/` — 69 skills (including your existing spine-exporter)
- [x] `.claude/settings.json` — Settings configuration
- [x] `.claude/statusline.sh` — Status line script
- [x] `CLAUDE.md` — Master configuration (customized for your project)
- [x] `docs/engine-reference/godot/VERSION.md` — Engine reference docs
- [x] `design/`, `docs/architecture/`, `production/`, `tests/`, `tools/`, `prototypes/` — Directory structure
- [x] `production/stage.txt` — Project stage (Production)
- [x] `production/review-mode.txt` — Review mode (lean)
- [x] `.claude/docs/technical-preferences.md` — Configured for Godot 4.5 + C#

---

## Step 1: Create Initial Architecture Documentation

Since this is a Production-phase project with existing code, document the current architecture:

### 1a. Create Architecture Decision Records (ADRs)

For key architectural decisions already made in your codebase:

- [ ] `/architecture-decision` — Document the C# primary language decision
- [ ] `/architecture-decision` — Document the FMOD audio integration
- [ ] `/architecture-decision` — Document the Spine animation system
- [ ] `/architecture-decision` — Document the combat system architecture

**Time**: 1-2 hours per ADR

### 1b. Create Game Design Documents (GDDs)

For existing game systems:

- [ ] `/design-system` — Document the combat mechanics
- [ ] `/design-system` — Document the card system
- [ ] `/design-system` — Document the enemy/monster system
- [ ] `/design-system` — Document the progression system

**Time**: 2-4 hours per GDD

---

## Step 2: Bootstrap Infrastructure

### 2a. Register existing requirements (creates tr-registry.yaml)

Run `/architecture-review` — even if ADRs already exist, this run bootstraps
the TR registry from your existing GDDs and ADRs.

**Time**: 1 session (review can be long for large codebases)
- [ ] tr-registry.yaml created

### 2b. Create control manifest

Run `/create-control-manifest`

**Time**: 30 min
- [ ] docs/architecture/control-manifest.md created

### 2c. Create sprint tracking file

Run `/sprint-plan update`

**Time**: 5 min (if sprint plan already exists as markdown)
- [ ] production/sprint-status.yaml created

---

## Step 3: Verify Setup

Run `/project-stage-detect` to verify all infrastructure is correctly configured.

**Time**: 5 min
- [ ] All checks pass

---

## What to Expect

Your existing codebase continues to work unchanged. The template provides:

1. **49 specialized agents** — Each owns a specific domain (combat, UI, audio, etc.)
2. **69 skills** — Workflows for design, implementation, testing, and release
3. **Hooks** — Automated validation on commits, pushes, and file changes
4. **Documentation templates** — Structured formats for GDDs, ADRs, stories

## Next Steps

1. **Document existing architecture** — Use `/architecture-decision` to capture decisions already made
2. **Document game systems** — Use `/design-system` to formalize existing mechanics
3. **Create stories for new features** — Use `/create-stories` when adding new functionality
4. **Track progress** — Use `/sprint-status` to see current work status

---

## Re-run

Run `/adopt` again after completing Step 2 to verify all infrastructure is resolved.
The new run will reflect the current state of the project.
