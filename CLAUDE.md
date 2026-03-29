# PBSSE — Procon v2 Plugin

## Project Overview

PBSSE (PunkBuster ScreenShot Enforcer) is a C# plugin for Procon v2 that monitors PunkBuster screenshot requests and responses. It detects players who block PunkBuster screenshots and can kick, temp-ban, or permanently ban them. The legacy Procon v1 version lives on the `legacy` branch.

- **Language:** C#
- **License:** GPLv3
- **Author:** onegrizzlybeer
- **Supported games:** BF3, BF4
- **Dependencies:** None (Procon v2 runtime only)

## Architecture

| File | Responsibility |
|------|---------------|
| `src/PBSSE.cs` | Single-file plugin: all logic, settings, event handlers, PB message parsing |

## Code Style

See the master CLAUDE.md at `/home/andrew/local_projects/procon_plugins/CLAUDE.md` for shared conventions.

**Critical conventions:**
- **Use `String`, `Int32`, `Boolean`, `Double`** — NOT `string`, `int`, `bool`, `double`. The codebase uses explicit System type names everywhere.
- **Allman brace style** — opening brace on its own line
- **4 spaces** for indentation, LF line endings
- **Block-scoped namespaces** (not file-scoped)
- **`using` directives outside namespace**, System usings first

## Build & CI

- `PBSSE.csproj` at root is a **CI-only artifact** for `dotnet format`. It is NOT a real build file — Procon v2 assemblies are unavailable for compilation.
- **CI workflow** (`.github/workflows/ci.yml`): runs on push to `master` and PRs. Checks `dotnet format whitespace` and `dotnet format style --exclude-diagnostics IDE1007`.
- **Release workflow** (`.github/workflows/release.yml`): triggered by `v*` tags. Packages `.cs` files from `src/` into a zip and creates a GitHub Release.

### Running style checks locally

```bash
dotnet restore
dotnet format whitespace --verify-no-changes
dotnet format style --verify-no-changes --severity warn --exclude-diagnostics IDE1007
```

## Threading Model

The plugin is single-threaded and event-driven. All logic runs synchronously within Procon event callbacks:
- `OnPunkbusterMessage` — parses PB messages, tracks screenshot requests/successes, triggers enforcement checks
- `OnListPlayers` — housekeeping for player tracking dictionaries
- `OnLevelLoaded` — resets all tracking state for the new round
- `OnPlayerLeft` — removes departing players from all tracking dictionaries
- `OnServerInfo` — triggers periodic update checks

Counter-based timing is used instead of dedicated threads: `maincheckhelper` and `viphelper` counters increment on events and trigger actions at thresholds.

## Event Registrations

From `RegisterEvents` in `OnPluginLoaded`:
- `OnListPlayers`
- `OnPunkbusterMessage`
- `OnLevelLoaded`
- `OnReservedSlotsList`
- `OnPunkbusterPlayerInfo`
- `OnPlayerLeft`
- `OnServerInfo`

## Supported Games

- Battlefield 3 (BF3)
- Battlefield 4 (BF4)

## Branch Structure

- `master` — current development, Procon v2 only
- `legacy` — archived Procon v1 version, no longer maintained
