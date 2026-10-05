# ABANDONED — Project Rules for Claude

## What this is
A 1–4 player co-op first-person salvage horror-comedy for Steam, built by a solo developer.
Players carry physical loot out of collapsing abandoned buildings while avoiding monsters.
The headline feature is the STRUCTURAL SYSTEM: floors, stairs and walkways have load limits,
crack under weight and impacts, and collapse. Full design: Docs/GDD.md.
How we work: Docs/PLAYBOOK.md.

## Folder layout
- This folder (repo root): CLAUDE.md, Docs/, .gitignore, .gitattributes
- Game/ is the Unity project (Assets/, Packages/, ProjectSettings/ live inside Game/)
- All paths below are relative to Game/ unless they start with Docs/

## Locked tech stack — do not substitute
- Unity 6.3 LTS, URP, C#
- Netcode for GameObjects (NGO), host/client. Do NOT use Mirror, FishNet, Photon or Fusion.
- Transports: Unity Transport (local testing) and Facepunch Transport (Steam),
  switchable via a TransportMode enum on NetworkBootstrap.
- Steam: Facepunch.Steamworks (NOT Steamworks.NET). App ID 480 during development.
- Voice: Dissonance + Dissonance for NGO (Milestone 4+).
- AI: Unity AI Navigation (NavMesh).
- Input: Unity Input System. Do NOT use the legacy Input Manager.
- No new packages or Asset Store assets without asking me first.

## Architecture rules
- Host is authoritative for: loot value and damage, structural damage and collapse,
  monster AI, money, quota, extraction, save data.
- Clients own their movement and input, and request pickups/drops/interactions via RPC.
  The host validates, then applies.
- Loot items and StructuralSections are host-controlled NetworkObjects.
- Collapse debris is NOT networked: the host broadcasts "section X collapsed" plus a seed,
  and every client plays the same pre-fractured break locally.
- Monsters run only on the host; clients see synced position/animation state.
- Data-driven content: loot items, threats, contracts, modifiers and equipment are
  ScriptableObject definitions. Adding a new loot item must not require new code.

## Code style
- One class per file. File name = class name.
- Namespaces: Abandoned.<Area> (Core, Player, Interaction, Loot, Structure, Networking,
  Threats, Extraction, Company, Contracts, Equipment, Voice, Audio, UI).
- Folders: Assets/_Project/Scripts/<Area>/, Assets/_Project/Data/<Area>/,
  Assets/_Project/Prefabs/, Assets/_Project/Scenes/, Assets/_Project/Editor/
- [SerializeField] private fields, not public fields.
- Tunables (speeds, capacities, damage, timers) live in ScriptableObject configs.
- Small components over giant scripts. Nothing over ~300 lines without a reason.
- Comment WHY, not WHAT.

## Working rules
- ONE task at a time, small enough to test in Play mode in under 5 minutes.
- For new systems, explain the plan first (files, classes, data, networking).
- After changes, give me numbered Unity Editor steps. If it's more than ~5 clicks,
  write an Editor script with a menu item under Tools/Abandoned/... instead.
- Never edit Game/Library/, Game/Temp/, Game/Logs/, obj/, or Game/Packages/
  (except Game/Packages/manifest.json when I ask).
- Never hand-edit .unity or .prefab YAML unless I ask; use Editor scripts.
- Don't delete or rename files outside the current task without asking.
- If something fails twice, stop and explain possible causes instead of guessing.
- Add debug visualizations (Gizmos, on-screen debug text) for every new system,
  toggleable with F1.
- Suggest a commit message when a step works. Don't push unless I ask.

## Testing
- Local co-op: Multiplayer Play Mode + Unity Transport, up to 4 players on one Mac.
- Steam: Facepunch Transport, separate machines, Steam running, App ID 480.

## Current milestone
Milestone 0 — Foundation

## Decisions log
- Stack: NGO + Facepunch + Dissonance (same combination as Lethal Company).
- Structure uses modular sections + pre-fractured mesh swaps, not real-time destruction.
- Levels are hand-built layouts with randomized contents, not procedural generation.
- Repo root is ~/Documents/Abandoned; the Unity project is the Game/ subfolder.
