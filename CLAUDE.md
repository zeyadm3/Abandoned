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
- Voice: our own proximity voice on Steam's voice API (Facepunch.Steamworks) sent over NGO, behind an
  IVoiceService interface so Dissonance can replace it later (Dissonance can't be bought now).
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
- Multi-process network test: Tools/unity.sh nettest [scenario] (1 host + 3 headless clients of a
  dev Mac build over 127.0.0.1). Builds: Tools/unity.sh build (Mac + Windows zips).
- Local co-op: Multiplayer Play Mode + Unity Transport, up to 4 players on one Mac.
- Steam: Facepunch Transport, separate machines, Steam running, App ID 480.
- No Windows PC here. A friend with his own Steam account is the Windows tester and
  second Steam account. Builds are made on the Mac (Mono backend) and shared as a
  single zipped folder.

## Current milestone
Feel/presentation pass: the user's 2026-10-07 answers are captured in Docs/POLISH.md.
Latest instruction: **build everything first; the user will test afterward**. Compile, regenerate
content and make builds; no gameplay, EditMode/PlayMode/nettest or QA screenshots during this pass.
Use existing/free assets and original code-built low-poly art. New paid packs/packages are not authorized.

Milestones 0–10 are built (autonomous build merged into main on 2026-10-07; the separate
autobuild-2 worktree is gone, everything lives in this one folder). Next: the user's full test pass.
Batch Unity runs (Tools/unity.sh) need the editor closed. See Docs/PROGRESS.md for the current task.

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
    TestMap (then TestBuilding) ground-floor tiles (nothing below them). It's per-section, not a
    "ground floor never collapses" rule, because real levels may have basements.
- 2026-10-06 (autonomous build M3–M10, user brief):
  - Voice: free, built on Steam's voice API + NGO behind IVoiceService (Dissonance later if bought).
    For local Unity Transport testing a raw-microphone backend stands in for Steam voice.
  - Embedded Facepunch Transport fork with Facepunch.Steamworks 2.5.2 + the three spike patches
    (pre-approved). Steam features are written fully but marked "needs real Steam test".
  - Art/audio: free CC0 assets only (Kenney, Quaternius, Poly Haven and similar), one low-poly style,
    every asset recorded in Docs/ASSET_CREDITS.md. No other new packages without asking.
  - Each milestone is tagged milestone-N and ships zipped Mac + Windows (Mono) builds.
  - M3.1: the fork lives in Game/Packages/com.community.netcode.transport.facepunch (2.0.0-abandoned.2; .2 = disconnect reasons survive the close).
    Abandoned.Networking.SteamBootstrap is the ONLY owner of Steam's lifetime (Init/RunCallbacks/
    Shutdown); the transport never inits or shuts Steam down. App ID lives in Data/Networking/
    NetworkConfig.asset (Game/steam_appid.txt kept in sync by the rebuild). Steam never starts in
    batch mode/tests unless Unity is run with -steam; -nosteam turns it off anywhere. Tests use a
    fake ISteamClient. Linux editor isn't supported by the plugin settings (mac + Windows editors).
  - M3.2: scenes have no placed player; NGO spawns one per connection at PlayerSpawnPoint slots the host
    assigns (host = slot 0). Solo = hosting alone: the editor auto-hosts on Play (NetworkConfig
    AutoHostInEditor; never in MPPM virtual players or with -client/-connect); builds have a "Host (or
    play solo)" button and accept -host / -connect ip:port / -transport unity|steam. Player movement is
    an owner-authoritative NetworkTransform; grounded/sprint/crouch/ragdoll state is an owner-written
    NetworkVariable; remote ragdolls show a lying capsule, never a physics ragdoll. NGO scene
    management is off for now (each instance loads its own scene) until the HQ/run flow (M5).
  - M3.3: version = Player Settings > Version, 0.<milestone>.<patch> (set from BuildScript.Version; bump it when
    a milestone starts - M4's builds still said 0.3.0); builds stamp the commit into
    Data/Core/Resources/BuildInfo (reset after the build). Build scenes come only from
    Editor/Build/BuildScenes.All. Shareable builds are Mono, universal Mac + Win64, with steam_appid.txt
    beside (never inside) the executable; Release builds leave it out. Every networked feature gets a
    nettest scenario (Scripts/Networking/NetTest, registry in NetTestScenarios).
  - M3.4: loot prefabs are NetworkObjects (owner-authoritative NetworkTransform, no NetworkRigidbody;
    Grabbable makes non-simulating copies kinematic). NetworkLoot holds host-written NetworkVariables
    for value (full/current/condition/shattered) and hold state (free/held/pocketed + holder's player
    object id); every machine mirrors the hold onto its own copies. All interactions go through
    NetworkInteractionHandler -> LootServerActions (same PickupRules, host's view); non-networked items
    fall back to the single-player handler. Pocketed items stay host-owned (no physics). Carriers report
    collisions by RPC, filtered by ImpactReportFilter (owner or just-released owner, rate, plausibility,
    speed clamp; tunables in LootNetConfig). Scene-placed loot must be saved as in-scene placed
    (Editor/NetworkObjectIds stamps it after the scene is in Build Settings), or clients double it.
  - M3.4-fix: hits against a copy this machine only follows are judged where they happen. A follower
    copy's velocity is estimated from its network motion (Grabbable.Velocity / IVelocitySource), so a
    player's owner is knocked down by host-simulated loot too. A client-carried item that strikes
    host-simulated loot names it in its impact report; the host applies the same impact and a
    mass-scaled push (LootStrikes, LootNetConfig.StruckPushTransfer).
  - M3.5: Heavy/Huge loot carries SharedCarryable + NetworkSharedCarry (added by the loot prefab
    generator). Crew sizes in Data/Interaction/SharedCarryConfig (Heavy 2, Huge 3) with a per-definition
    RequiredCarriers/CarryPoints override; handles are generated from the definition's Size. The host
    owns WHO holds which handle (SharedCarryState NetworkVariable: holder player ids + grip offsets in the item's
    yaw frame, so grips turn with the item; mirrored on every machine) and always simulates a shared item; it never hands its physics to a client.
    Each carrier streams its desired hold point (feet + grip offset) by unreliable RPC every tick; the
    host clamps it (MaxTargetDeviation) and falls back to its own view when stale. Solo Heavy drag now
    runs through the same path (one carrier = under-crewed = dragged). The carrier's own motor is
    tethered to its handle and capped at the slowest carrier's speed (PlayerMotor.SetTether/MaxSpeed).
  - M3.5-fix: an under-crewed item nobody may drag alone (Huge) only moves within
    SharedCarryConfig.NudgeRadius (0.75 m) of where the under-crewed hold began; the budget belongs to
    the item (regrabbing doesn't refill it) and resets only after a full-crew lift or a big move.
  - M3.6: structure sync is ONE host-spawned NetworkObject per level (Prefabs/Network/StructureNet,
    spawned by StructureNetSpawner next to the StructureSimulation), not one per section: a NetworkList
    of SectionNetState (stage, health /255, load in 5 kg steps, collapse seed, Failing start in server
    time, generation byte; index = section id) + StructureNetGlobals (stability, seed, generation,
    section count, layout hash). NGO sends only changed entries, so a ~300-section mall is a ~6 KB
    initial sync and ~20 B per change. Clients set StructureSimulation.SetMirror(true): no load, damage,
    timers, pre-damage or cascades run there; ApplyReplicated flips colliders off the moment a collapse
    entry arrives and plays the same seeded pre-fractured break (debris local, Debris layer). Re-roll =
    generation bump: clients restore everything, then take entries of the new generation only.
    Sections that were already down when a client joins are applied quietly (no break/ragdoll).
    Structure gameplay noise (creaks/collapse for monsters) stays host-only; creak/groan/snap/crash
    sounds play on every machine from the replicated stage.
  - Company "Zeyad Games", bundle id com.zeyadgames.abandoned (BuildScript.ApplyPlayerSettings; they fix the
    save-data folder, so they don't change once saves exist).
  - M4.1: voice = IVoiceCapture + IVoiceCodec (Steam voice in Steam sessions, Unity Microphone + mu-law over
    Unity Transport, tone in tests) relayed by NetworkVoice on the Player (owner -> host -> others,
    unreliable, sender-checked). Playback is our own distance gain (full <1.5 m, 0 at 25 m) on a jitter
    buffer; settings (PTT/open mic/volume/mute) in PlayerPrefs until the settings menu.
  - M5: the mall is a data-driven greybox (MallLayout + builders); loot spawns from a seeded planner
    (SpawnTags per zone, 1-2 jackpots); runs are host RunState (haul/quota/window/danger) with the truck's
    cargo bay; the next run happens in place (re-roll + respawn, no reload); NavMesh baked once, collapses
    carve it (SectionNavCarver), exterior/roof excluded; threats are host-only NetworkObjects.
  - M6.0: one persistent session (the first scene's NetworkBootstrap + NetworkManager live in
    DontDestroyOnLoad; later levels' copies remove themselves); SessionTravel loads levels on every
    machine without NGO scene management and gates level spawns until all are in; players and session
    objects are DDOL. Levels must spawn their network objects dynamically.
  - M6.1-6.7: the HQ is the first build scene; CompanyService (session-long) saves company.json on the
    host (never in tests/batch); contracts are data (ContractModifier assets) rolled from a board seed;
    payday/debt/bankruptcy per GDD 13; joins only at the HQ. Company rules (gear stock, trolley for solo
    Heavy drags, radios) apply only in games started at the HQ. Gear = EquipmentDefinition assets in an
    ordered catalog; 2 hand slots per player. Ghosts orbit living teammates and aren't heard by the living.
    Threat roster per level (Blind One, Stalker, Collector), weighted by run seed.
  - M7.1-7.3: art/audio are Kenney CC0 packs (furniture, car, impact/interface/RPG sounds, fonts) under
    Art/ThirdParty/Kenney/<Pack>/, listed in Docs/ASSET_CREDITS.md (the credits screen is generated from it).
    Loot models live on LootDefinition.Model (the Size box stays the collider). Lighting is realtime only
    (no bake: power-off runs and collapses): ceiling LightFixtures hang from the slab above (SectionProp) and
    follow power; fog + a global volume per level (LevelAtmosphere). All sound goes through GameAudio +
    SoundBank (data); cues without clips play the synthesised signatures.
  - M7.4: menus are UI Toolkit built in C# (MenuUi on the session object; no package). Player settings,
    bindings and the cosmetic profile go through Core.Prefs (PlayerPrefs, memory in batch mode).
  - M7.5: cosmetics are CosmeticDefinition data in an append-only catalog (byte index on the wire), the
    owner writes its outfit; unlocks are local (PlayerProfile), not the host's company save.
  - M7.6: occlusion baked for the mall, but only never-breaking geometry occludes (floors carry no static
    flags). Sun shadows 2 cascades / 35 m (RenderPipelineSetup in the rebuild). `nettest perf` and the F1 perf
    line are the performance probes.
  - M8: every player-facing screen is UI Toolkit on MenuUi's document: the HUD on HudLayer (hidden under
    menus and in F10 clip mode), world-opened screens as ScreenPanels beside it. Tips via HintDirector
    (once per player, Prefs). The demo is a build flavour (BuildFlavor.Demo stamped in BuildInfo; Core.Demo,
    DemoConfig in Data/Core/Resources, its own company_demo.json). Failed nettest logs: Game/Logs/nettest-failed/.
  - M9: the mall carries Early Access (no Hospital/Hotel). The Hunter (4th threat, a 450 kg load source), 8 modifiers,
    57 loot items, crowbar/backpack/support jack, 12 achievements (local stats, Steam hand-off), Steam Cloud = Auto-Cloud.
- 2026-10-07 (user brief): finish every milestone WITHOUT testing (no EditMode/PlayMode/nettest/screenshots); the user
  tests the finished game. Compile + rebuild + builds only; everything from the M9 review on is untested.
  - M10: truck upgrades (TruckUpgradeCatalog bits in CompanyNetState; owned ids in CompanySave.unlocks), flatbed
    trolley (short crews drag Huge), rope & pulley (Core.SafeDescent columns), locked stores (RollerShutter in the
    level + RunShutters on the RunState prefab; Sealed modifier), motion detector + night vision (owner-local), an
    accessory cosmetic slot, the boss's voicemails (CompanyMessages), synthesised music (MusicPlayer, never in batch),
    balance (running costs per job, gear x5-8, GDD 13 unlock levels, crew-size quota scale; Docs/BALANCE.md), F9 free
    camera, LaunchConfig links, `Tools/unity.sh build-release` (refuses App ID 480). Version 0.10.0.
- 2026-10-07 (user): the old TestBuilding scene is renamed **TestMap** (Scenes/TestMap.unity, TestMapBuilder). It's a
  feature-testing map for us, not a level: it's never in a job and never in a shared build (BuildScenes.ForPlayer
  keeps it only in Dev builds, for nettests). The mall is the game's map.
- 2026-10-07 (user-approved UI overhaul, R.E.P.O./Lethal Company style): work in autobuild-2; steps (1) theme+kit,
  (2) HUD + loot scan + prompts, (3) death/ghost + truck banner + toasts, (4) pause + crew + per-player volume,
  (5) settings tabs + Video + mic picker + UI Scale, (6) main menu + Play flow, (7) appraisal receipt, (8) HQ
  terminal (clickable amber CRT) / board / wardrobe, (9) travel screen, (10) text chat (T). Commit + push each step;
  render UI screenshots (UiShotsTests, 16:9 / 16:10 / ultrawide) and fix layout before moving on. Fonts/icons CC0 or
  OFL only, in Docs/ASSET_CREDITS.md; never the stencil font for small text. Panel scales by height (match 1).
  Built: HUD = RunHud / PlayerHud / LootTags (scan = the Scan action, Q) / InteractionHud key caps; world tags on
  HudLayer.World (placed from the viewport); ToastFeed, TravelScreen, ChatView (+ NetworkChat on the player) on the
  session object; death card + death cam in GhostSpectator (Threat.DeathLine -> NetworkPlayer.DeathCause); TruckDisplay
  (UITK panel into a RenderTexture on a quad); VideoSettings/VideoApplier (runtime URP asset copy, never the project's);
  UI Scale = a runtime copy of the menu PanelSettings; WardrobePreview = an offstage mannequin + camera into a texture.
  Steps 4-10 untested (user brief: no tests until they test everything).
- 2026-10-08 (0.12.2): Tools/Blender/environment.py maps Unity (x,y,z) -> Blender (x,z,y); the old (x,-z,y)
  imported every kit model mirrored along Z (flights drew behind their ramps). rebuild-horror now logs
  `[MallValidation]` (flights walkable + NavMesh, invisible colliders in Mall/HQ) and `[MaterialAudit]`;
  keep both at 0. World text uses Abandoned/WorldText via WorldTextMaterial, never a font's GUI material.
  Menu effect tunables: Data/UI/Resources/MenuEffectsConfig; "Reduce menu effects" in Settings.
- 2026-10-08 (0.12.3/0.12.5): the user's HQ hand edits (tube intensities 30-50, moved labels/pillars, removed the NO
  CREW LEFT BEHIND label, yard door lamp and EXIT sign) were ported into HqDressingBuilder in 0.12.5. If the user
  hand-edits Scenes/HQ.unity again, port those edits before any HQ rebuild. The garage door's pose is kept via HqPlacements.
- 2026-10-08 (0.12.5): wardrobe items are free, bought once with company money (PlayerCosmetics.RequestBuy -> host
  CompanyService.TryCharge -> owner's PlayerProfile), or achievement rewards. Models: Environment/Cosmetics (Blender kit).
  MenuKit buttons must never get child elements (a text element with children stops measuring its text).
