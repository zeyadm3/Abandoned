# Skills from Git

Skills installed for Claude Code on this Mac, with where each one came from.
Last checked: 2026-10-28.

CLAUDE.md tells Claude to read this file at the start of every session and to use these skills to help develop
ABANDONED. Add every new skill here when it is installed.

## Using them on this game

| Pipeline step | Use |
|---|---|
| Finding code (which file, what calls what) | **Graphify** first: `venv/bin/graphify query "<question>"`, `path "<A>" "<B>"`, `explain "<concept>"`; then read only the lines it points to |
| A new system or feature | **superpowers:brainstorming**, then the plan CLAUDE.md asks for (files, classes, data, networking) |
| A multi-step task | **superpowers:writing-plans**, then **executing-plans** (or **subagent-driven-development** for independent parts) |
| Writing the code | **ponytail**: smallest change that works, no unrequested abstractions; CLAUDE.md's code style still applies |
| A bug (console error, broken behaviour, failed batch run) | **superpowers:systematic-debugging**: root cause before any fix (and CLAUDE.md's "fails twice, stop and explain") |
| Anything in the Unity Editor (scenes, prefabs, menus, console, tests) | **unity-mcp-skill** (MCP for Unity) |
| Remake art (Blender, imports, builders) | **abandoned-remake** (project skill) |
| Reviewing a change before a commit | **ponytail-review** or **superpowers:requesting-code-review** |
| Answering review feedback | **superpowers:receiving-code-review** |
| Checking the whole codebase for bloat or risk | **ponytail-audit**; **ponytail-debt** for deferred `shortcut:` comments |
| Saying something is done | **superpowers:verification-before-completion**, but only run the checks the user allows (see below) |
| After changing code | `venv/bin/graphify update .` to keep the graph current |
| A new skill for this project | **superpowers:writing-skills** |

**Where this project's rules win over a skill** (from CLAUDE.md and the user's standing instructions):
- Testing: don't compile, run tests, play or verify unless the user asks; the TDD and verification skills apply
  only when they do. When nothing was run, say so.
- Git: commit only to `main`, only when the user asks, never push unless asked. No worktrees or feature
  branches, so skip **using-git-worktrees** and the branch steps of **finishing-a-development-branch**.
- Locked tech stack and "no new packages without asking": a skill's suggestion to add a library doesn't override it.
- Unity: never hand-edit .unity/.prefab YAML; Tools menu items follow Docs/TOOL_MENU.md.

## From GitHub

### Superpowers (plugin, v7.0.1)
- Source: https://github.com/obra/superpowers (marketplace: https://github.com/obra/superpowers-marketplace)
- Installed: 2026-10-22, commit `2ce8aa15d05bd57213e6f746f953aafb7ded7a8b`
- Location: `~/.claude-account2/plugins/cache/superpowers-marketplace/superpowers/7.0.1/skills/`

| Skill | What it does |
|---|---|
| using-superpowers | Starts every conversation; makes Claude check for and use skills first |
| brainstorming | Before any creative work: features, components, plans |
| writing-plans | Turns a spec into a multi-step implementation plan |
| executing-plans | Runs a written plan inline, step by step |
| subagent-driven-development | Runs a plan's independent tasks with subagents |
| dispatching-parallel-agents | Splits 2+ independent tasks across parallel agents |
| test-driven-development | Write the test first, then the code |
| systematic-debugging | Find the root cause of a bug before fixing it |
| verification-before-completion | Run the checks before claiming something works |
| requesting-code-review | Asks for a review when a feature is done |
| receiving-code-review | Checks review feedback on its merits before acting on it |
| using-git-worktrees | Isolated git worktrees for feature work |
| finishing-a-development-branch | Wrapping up and merging a finished branch |
| writing-skills | Creating and testing new skills |
| diagnosing-superpowers | Works out why a superpowers session went wrong |

### MCP for Unity skill (unity-mcp-skill / unity-mcp-orchestrator)
- Source: https://github.com/CoplayDev/unity-mcp (Unity package `com.coplaydev.unity-mcp` in Game/Packages/manifest.json)
- Installed: 2026-10-22, written by the package's skill sync (marker `.unity-mcp-skill-sync`)
- Location: `~/.claude-account2/skills/unity-mcp-skill/` and `~/.claude/skills/unity-mcp-skill/`
- What it does: best practices, tool schemas and workflows for driving the Unity Editor through the MCP tools

## Not from GitHub (for reference)

### Project skill (written for this repo)
- abandoned-remake: `.claude/skills/abandoned-remake/`. Conventions for the 2026 remake (Blender MCP + Unity MCP).

### Synced from the claude.ai account (Anthropic)
- Location: `~/.claude-account2/skills/synced/...` and `~/.claude/skills/synced/...`
- Skills: built-in-browser, chrome-browser, computer-use, deep-research, doc-coauthoring, docs, docx,
  google-workspace, humanizer, import-memory, mcp-builder, morning, pdf, pptx, skill-creator,
  web-artifacts-builder, xlsx
- Engineering plugin (synced): architecture, code-review, debug, deploy-checklist, documentation,
  incident-response, standup, system-design, tech-debt, testing-strategy

### Available but not installed
The official marketplace catalog (https://github.com/anthropics/claude-plugins-official) is downloaded to
`~/.claude-account2/plugins/marketplaces/claude-plugins-official/` and `~/.claude/plugins/marketplaces/`,
but none of its plugins is installed or enabled (frontend-design, plugin-dev, skill-creator, hookify,
mcp-server-dev, playground, claude-md-improver and others).

## Added later

### Ponytail (plugin, v5.3.2), added 2026-10-28
- Source: https://github.com/DietrichGebert/ponytail (marketplace `ponytail`, installed as `ponytail@ponytail`)
- Commit: `09c434ab73ca43761c5d8b0c5d341926c2fba710`
- Location: `~/.claude-account2/plugins/cache/ponytail/ponytail/5.3.2/skills/`
- What it is: "lazy senior dev mode", the smallest change that fully solves the task (YAGNI, stdlib first, no
  unrequested abstractions). It also comes with a hook that writes per-task stats to `~/.config/ponytail/band/`.

| Skill | What it does |
|---|---|
| ponytail | Smallest change that solves the task, short replies (levels: lite, full, ultra) |
| ponytail-audit | One-shot whole-repo audit: bugs, security, load, untested risk, what to delete |
| ponytail-review | Review of one change: logic, safety, load, tests, speed, needless lines |
| ponytail-debt | Lists every `shortcut:` comment as a debt ledger |
| ponytail-gain | Shows ponytail's measured savings from its benchmark |
| ponytail-help | Quick reference for levels, skills and commands |

### Graphify (Python tool `graphifyy` v0.9.97), added 2026-10-28
- Source: https://github.com/Graphify-Labs/graphify (site: https://graphify.com)
- Installed with pip into `venv/` in the repo root (`venv/bin/graphify`, `venv/bin/graphify-mcp`); not on the
  PATH and not a Claude plugin, so no Claude skill folder was created.
- What it does: turns the repo into a queryable knowledge graph (`graphify query`, `path`, `explain`,
  `update .`). The graph is in `graphify-out/` (graph.json is about 35 MB, plus graph.html).
- It also changed the project: a "## graphify" section at the end of `CLAUDE.md`, and `.claude/settings.json`
  with PreToolUse hooks (`graphify hook-guard search` / `read`) that run before searches and file reads.
- Git: `venv/`, `graphify-out/` and `.claude/settings.json` are untracked and not ignored, so commit or
  ignore them on purpose.
