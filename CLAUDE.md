# ABANDONED — Project Rules for Claude

## What this is
A 1–4 player co-op first-person salvage horror-comedy for Steam, built by a solo developer.
Players carry physical loot out of collapsing abandoned buildings while avoiding monsters.
The headline feature is the STRUCTURAL SYSTEM: floors, stairs and walkways have load limits,
crack under weight and impacts, and collapse. Full design: Docs/GDD.md.
How we work: Docs/PLAYBOOK.md.

## Folder layout
- This folder (repo root): CLAUDE.md, Docs/, Tools/ (batch-mode scripts), .gitignore, .gitattributes
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
  (Player Settings > Active Input Handling = "Input System Package (New)" only.)
- Camera: Cinemachine (camera feel: head bob, shake, landing dip).
- No new packages or Asset Store assets without asking me first.

## Architecture rules
- Host is authoritative for: loot value and damage, structural damage and collapse,
  monster AI, money, quota, extraction, save data.
- Clients own their movement and input, and request pickups/drops/interactions via RPC.
  The host validates, then applies.
- Loot items and StructuralSections are NetworkObjects. The host always owns loot value
  and damage, and structural state.
- Loot physics ownership: while ONE player carries an item, that carrier owns its physics
  (ownership transferred on pickup, returned to host on drop/throw). For shared carrying
  (2+ players), the host simulates the item from the carriers' input forces.
  Resting or thrown-and-settled loot is host-simulated.
- Structural load is LOGICAL, not physics contacts: each load source (player + carried
  item, resting item, share of a shared carry) does a downward check for its supporting
  section(s) and splits its gameplay weight across them.
- Gameplay weight (load, stamina, speed) is separate from Rigidbody mass, which is
  clamped to a sane range to keep PhysX stable. Both live in LootDefinition.
- Collapse state is a networked state change; the host flips the section's collider
  off at that moment so client-owned players fall at the same time for everyone.
- Collapse debris is NOT networked: the host broadcasts "section X collapsed" plus a seed,
  and every client plays the same pre-fractured break locally.
- Debris is cosmetic only: its own physics layer, never collides with players or loot,
  never deals gameplay damage. The host calculates all structural damage, including
  cascades onto sections below.
- Death ragdolls are local only (not networked).
- Players join only at HQ between runs. No mid-run joining or reconnect.
- Monsters run only on the host; clients see synced position/animation state.
- Data-driven content: loot items, threats, contracts, modifiers and equipment are
  ScriptableObject definitions. Adding a new loot item must not require new code.

## Code style
- One class per file. File name = class name.
- Namespaces: Abandoned.<Area> (Core, Player, Interaction, Loot, Structure, Networking,
  Threats, Extraction, Company, Contracts, Equipment, Voice, Audio, UI).
- Folders: Assets/_Project/Scripts/<Area>/, Assets/_Project/Data/<Area>/,
  Assets/_Project/Prefabs/, Assets/_Project/Scenes/, Assets/_Project/Editor/,
  Assets/_Project/Art/ (materials, meshes, textures; greybox materials in Art/Greybox/)
- Tests: Assets/_Project/Tests/EditMode/ and Assets/_Project/Tests/PlayMode/
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
- Batch-mode verification (Unity closed): Tools/unity.sh all. See Docs/PROGRESS.md.
- Local co-op: Multiplayer Play Mode + Unity Transport, up to 4 players on one Mac.
- Steam: Facepunch Transport, separate machines, Steam running, App ID 480.
- No Windows PC here. A friend with his own Steam account is the Windows tester and
  second Steam account. Builds are made on the Mac (Mono backend) and shared as a
  single zipped folder.

## Current milestone
Milestone 2 — The Weight (built and verified in batch; awaiting hands-on feel check before M3)

## Decisions log
- Stack: NGO + Facepunch + Dissonance (same combination as Lethal Company).
- Structure uses modular sections + pre-fractured mesh swaps, not real-time destruction.
- Levels are hand-built layouts with randomized contents, not procedural generation.
- Repo root is ~/Documents/Abandoned; the Unity project is the Game/ subfolder.
- 2026-10-06 (pre-production review):
  - Cinemachine added to the locked stack for camera feel. Visual Scripting removed.
  - Unity template leftovers (TutorialInfo/, Readme.asset) get removed; our own Input
    Actions live under _Project/, and the template InputSystem_Actions is deleted once
    ours works. SampleScene stays for now.
  - Facepunch Transport source chosen at M3, after a 1-hour Facepunch Transport + NGO 2.13
    compatibility spike at the end of M1.
  - Dissonance bought at M4, only after a spike confirms it works with NGO 2.x.
  - No mid-run joining: players join at HQ between runs.
  - Solo: hand trolley available from the start; trolley + ramp lets a solo player move
    Huge items slowly. Jackpots are possible solo, just hard.
  - M2 structure code is host-authoritative from day one; the solo player acts as host.
  - Carried-loot physics: single carrier owns physics, host owns value/damage; shared
    carry is host-simulated from carrier input forces.
  - Structural load uses the logical downward-check model, not physics contacts.
  - Gameplay weight is separate from clamped Rigidbody mass.
  - Debris is cosmetic only; host computes all structural damage including cascades.
  - Host flips collapsed-section colliders via a networked state change.
  - NavMesh-after-collapse approach decided at M5. Recommendation: per-section authored
    navmesh with carving/toggling + NavMeshLinks for holes and drop routes, no runtime
    rebakes.
  - Every level has authored fallback extraction routes (rope points, windows) instead of
    runtime connectivity checks.
  - Cut for launch: dragging a body back to the truck to revive. Death ragdolls stay local.
  - Hospital and Hotel are post-Early-Access candidates; the mall carries Early Access.
  - Every StructuralSection gets a per-section "can collapse" flag (M2). It's off for
    TestBuilding ground-floor tiles (nothing below them). It's per-section, not a
    "ground floor never collapses" rule, because real levels may have basements.
