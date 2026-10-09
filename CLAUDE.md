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
  cascades onto sections below. Exception (2026-10-08): each fallen section's deterministic solid
  rubble mound (RubbleMound, Default layer) blocks players, loot and monsters; it is built locally from
  the collapse seed on every machine, never networked, and never deals damage.
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

0.14.0 (2026-10-09): the sound system rebuild, the solid-objects pass and the shadow rework compile and
`Tools/unity.sh rebuild-horror` finishes clean (the user asked for that run; the standing rule is still not to run
anything unasked). Not play-tested beyond the user's first listen, after which the base volume was halved.

Milestones 0–10 are built (autonomous build merged into main on 2026-10-07; the separate
autobuild-2 worktree is gone, everything lives in this one folder). Next: the user's full test pass.
Batch Unity runs (Tools/unity.sh) need the editor closed. See Docs/PROGRESS.md for the current task.

## Decisions log
- 2026-10-10 (user, plan only): **Early Access is 0.25.0**, not 0.20.0, to allow more time and features.
  0.20, 0.21, 0.23 and 0.24 are numbered in Docs/ROADMAP_0.16-0.25.md and approved by the user. 0.25.0 stays fixes only.
- 2026-10-10 (user, **0.22 total UI revamp**, plan only): the user's friend redoes the whole UI from a reference
  the user has in mind (not yet shared; first planned as a paid freelancer). Ten steps, 0.22.0–0.22.9, in
  Docs/ROADMAP_0.16-0.25.md. Do not start a UI redesign ourselves; until 0.22, new features get thin UI on
  the current kit.
- 2026-10-09 (user, **0.15.0 complete mall replacement**, user tests): the initial mall redesign was
  rejected. Rebuild the entire mall and parking from scratch, keeping the approved surrounding roads
  and buildings intact. Two-storey rectangular shopping centre, straight promenade, glass-fronted
  fitted tenants, atrium/escalators/upper gallery, rear food court and internal basement/service core.
  Exactly one continuous eight-metre front entrance/exit; no side loading door, rope/window escape or
  other exterior opening. The functional payout/cargo truck sits beside the arrival van in the front
  court, ramp toward the mall. Add an enclosing wooded mountain horizon beyond the district. Retain
  HQ/shared asset tuning, structural gameplay and the existing payout system; reauthor loot/spawns,
  network IDs, navigation and culling for the new plan. Menu: Tools/Abandoned/Mall/Rebuild Mall and
  Parking (0.15.0). No tests/audits/QA; the user will review after saving. Version 0.15.0.EEA.
- 2026-10-09 (user: fix the console's "Reduced additional punctual light shadows resolution ... 17/18 shadow maps";
  compiles, the user's walk-through check is still open): the shadow atlas is budgeted in shadow MAPS, not lights. Every light asks for a
  1024 px map (URP's High tier), the 4096 atlas holds 16, a spot is one map and a point light six, so the old
  6 lights with 2 points (16) plus the flashlight, plus any spot still fading out, overflowed and URP halved them all.
  `ShadowDirector` now keeps its lights inside the atlas less the maps of lights it doesn't manage (counted each
  second; never fewer than `ShadowBudgetConfig.ReservedMaps` = 1, the player's flashlight). The 2 / 4 / 6 counts are
  upper limits: with two point lights casting, three spots fit beside them, not four. A new light keeps its place but
  only starts once the lights fading out have let go of their maps, so a full atlas swaps one after another instead
  of cross-fading. This replaces "one still fading out holds its place" (0.14.0), which covered point lights only.
- 2026-10-09 (user, **wardrobe room lighting save**, user tests): preserve the user's manual lighting
  angle change in the crew/wardrobe room: outer spotlight cone 164.3437 degrees, inner cone 88 degrees.
  Saved in the current HQ scene and the crew-room authoring default. Keep the current interior,
  staff door and estate surroundings; do not regenerate HQ or run tests/verification.
- 2026-10-09 (user, **HQ staff entrance and exterior**, user tests): close the staff doorway behind
  the player spawns with a solid locked steel door. Surround the company yard with vacant industrial
  buildings, streets, utilities, trees and a distant wooded horizon so views outside feel situated.
  Apply additions to the existing HQ scene, preserving the user's interior placements and asset edits;
  do not regenerate the interior. Keep environmental lettering absent. No tests or verification.
- 2026-10-09 (user, **0.14.5c asset edits**, user tests): preserve the user's edited assets and scene
  as present on disk in 0.14.5c.EEA, carrying forward the 0.14.5b physical HQ lettering removal and
  functional prompts/menus. Details of the user's asset edits were not specified. Do not regenerate
  the HQ or run tests/verification for this save; the user will test afterward.
- 2026-10-09 (user, **0.14.5b HQ lettering cleanup**, user tests): remove all physical words and numbers
  from the salvage HQ, including signs, floor stencils, prop labels, book lettering, clock labels,
  scale readout and corkboard handwriting. Preserve the approved company layout, clutter and diagrams.
  `HqWorldText.Remove` strips world TextMesh objects after dressing; the depot keeps receipts in its
  interaction menu. Functional UI and proximity interaction prompts remain. No tests or verification.
- 2026-10-09 (user, **0.14.5 layout replacement**, user tests): the first clutter pass still looked like a shed.
  Replace the old 24 × 16 m / 4 m-high three-room HQ with a 36 × 28 m company building: warehouse eaves
  6.4 m and pitched ridge 8.2 m, plus a lower 3.8 m operations wing. Receiving/van/depot at the front,
  sorting island/handling machinery in the middle, tall numbered stock aisles behind; glazed dispatch,
  separate crew/locker/break room and maintenance in the east wing. Paved loading yard, pedestrian
  staff entrance, steel portal frames, rooflights and company facade branding establish the business.
  This supersedes preservation of the original HQ envelope and room split. Functional systems and
  original garage-door placement remain. `Editor/HQ/HqLayout` is the common plan. No tests or verification.
- 2026-10-09 (user, **0.14.5 HQ Hub Overhaul**, user tests): the HQ is a cluttered utilitarian salvage warehouse.
  Dispatch contracts come from a rotary telephone on the central steel bench, with condition briefing audio;
  the adjacent worn leather field guide replaces the office computer bestiary and keeps encounter unlocks.
  The workshop groups inventory/upgrades at a maintenance bench, tool pegboard and charging station;
  processing has an industrial scale and reinforced chute; logistics has mall plans and hazard notes.
  Crates, retired electronics, copper pipe, rusted shelving, oil drums and labeled bins dress the perimeter.
  Dim tubes, exposed bulbs and localized dust/shafts use existing reduced-flashing/shadow behavior.
  Recovered truck haul remains settled once at extraction; the HQ receipt/unloading cannot pay it twice.
  Author via Tools/Abandoned/HQ/Rebuild Salvage Hub (0.14.5), which skips audits. No tests or verification
  authorized for this task; the user will do those after the redesign. Original dispatch speech was
  requested as part of this feature, authored locally without a new package or purchased asset.
- 2026-10-09 (user, **0.14.0**: sound rebuild, solid objects, shadow rework; compiled and rebuilt clean, barely played):
  - **Sound** is one system. Every sound is a `SoundId` described once in `Audio/SoundCatalog` (bus, priority, range,
    caption, how walls treat it); tuning and recordings stay in the SoundBank; `GameAudio` is the only way to play one
    (`Play`, `PlayUi`, `PlayDelayed`, `Loop`). `AudioMix` turns the volume settings into one gain per bus
    (Effects, Footsteps, Structure, Threats, Ambience, Music, Ui, Vitals); nothing else multiplies `AudioLevels` by hand.
    Tunables: Data/Audio/Resources/AudioConfig, including `BaseLevel` (0.5: the user found the first pass far too loud;
    it sits under every bus, not under crew voices). HorrorAudio, PlaceholderAudio, AmbienceSynth, MusicSynth,
    PlayerVitalAudio, WaterSurfaceAudio and ThreatMusicSynth are gone.
  - Sound sources (user's choice): synthesised in code (`Audio/Synth/*`, rendered on a worker thread by `SynthLibrary`)
    plus the three Kenney CC0 packs already in the project, which stay for impacts, footsteps, menu buttons and things
    held in the hand. No new audio assets without asking. Creak/groan/snap play the recording AND a synth layer.
  - Walls and floors dull and quieten sound (`AudioOcclusion`), except the building's warnings and the horn (never) and
    monsters (dulled, never much quieter). The listener's room (`ListenerSpace`) is measured with rays and drives a live
    reverb; it also owns the low-health muffle. 2D sounds, music and crew voices bypass both.
  - All music is played by `Audio.MusicPlayer`; `Threats.ThreatMusic` only chooses the song and its level. Jobs still
    have no music of their own (the departure theme stays off, as since 0.13.0).
  - Kept exactly: low health is the heartbeat alone, each monster's own song with the 15 s warning, captions never give
    direction, stings wait for HQ, a creak only ever means the floor, crew speech ducks ambience and music only.
  - **Collision**: everything with volume that a player can reach is solid. `Editor/SolidObjectsPass` runs in the Mall
    and HQ builds after dressing and before the NavMesh bake; it reads each model's mesh and puts boxes only where the
    model has geometry at body height (a canopy = its columns). Stay walk-through: lower than 0.3 m, thinner than 8 cm,
    higher than 2.1 m above the floor, soft vegetation (Overgrowth, LeafScatter), loose collapse chunks (still cosmetic).
    This replaces "flat litter, decals, ceiling pieces and signs stay walk-through" (2026-10-09) with those measured rules.
    An object that would stand in a stair, doorway, exit, spawn, the truck's approach or the middle lane of a carry-route
    tile (`MallRouteClearance`) is left walk-through, tagged `Core.WalkThroughObject` and reported, never made solid.
    New rebuild lines to keep at 0: `[MallValidation] ... walk-through objects`, `reachability ... cut off` (warnings,
    never fatal). Heavy/Huge loot uses speculative continuous collision.
  - **Shadows**: the lit ceiling lights nearest the camera cast, fading as you move (`Structure.ShadowDirector`):
    2 / 4 / 6 on the Shadows setting Low / Medium / High (`Core.ShadowBudgetConfig`), at most 2 of them point lights
    (six shadow maps each; one still fading out holds its place so the atlas never overflows).
    This replaces the four fixed shadow fixtures and supersedes "no additional shadow lights" (0.13.2). Your own
    flashlight and the moon always cast; crewmates' beams still don't. Builders set every fixture to no shadows.
  - Each mall storey is its own rendering layer (`Structure.LevelStoreys`, `Editor/Mall/MeridianLightLayers`): fixed
    geometry is tagged by storey, a ceiling fixture lights only its own storey plus everything on the Default layer
    (anything that moves), and every other light must reach all layers. A light created at run time must call
    `LevelStoreys.LightEverything` (ShadowDirector also fixes them within a second).
  - `Editor/ShadowCasterPass` only ever switches casting off (fixture housings, flat surfaces, see-through materials).
    Local-light shadow atlas 4096; ambient occlusion slightly stronger (RenderPipelineSetup).
  - Four-player host performance with six shadowed lights is unmeasured; the Shadows setting is the lever.
- 2026-10-09 (user, after playing 0.13.5d; written, untested): low-health feedback comes sooner, for urgency. The
  thresholds in PlayerHealthConfig are now: bar leans toward red from 80, orange-red from 60, crimson + steady heartbeat
  at 50 and below, pulsing blood red + faster heartbeat + muffle + vignette + corner blood at 35 and below, fastest
  heartbeat + closing vignette + heavy dripping blood at 20 and below (the spec's first numbers were 50/40/30/20/10).
  Low health is the heartbeat ALONE: the labored-breath clip is deleted (it read as a horse snorting) and the ordinary
  fear breath is silent from the critical threshold down. Do not add breathing back at low health.
- 2026-10-09 (user, dev tool; written, untested): holding Shift + Control + Option/Alt + Command/Windows together hurts
  the local player 5 HP at once and every 0.5 s while held (NetworkPlayer.DevHurt; Core.DevTools only, so never in a
  shareable build; the host applies it and a shareable host ignores a dev client's request).
- 2026-10-09 (user: "add fall damage"; written, untested): fall damage already existed but was free up to 4 m, exactly
  one mall storey, and quadratic above it. Now SafeFallHeight 2.5 m (asset edited by hand) and a linear curve
  (PlayerHealthConfig.FallDamageCurve = 1; 2 restores the old shape), lethal still at 12 m: one storey ~16 HP, two ~58,
  three kills; carried weight still multiplies it (+1 % per kg, up to x2.5). Knock-down height (PlayerRagdollConfig) unchanged.
- 2026-10-09 (user spec, vitals and low-health feedback; written, untested, user tests): every number is in
  PlayerHealthConfig (new fields default on the existing asset). The vitals bar grades by whole HP: electric blue to 41
  (leaning toward red from 50), into orange-red 40-31, crimson 30-21, dark blood red pulsing with the heartbeat at 20
  and below (PlayerHealthConfig.HealthColor). Thresholds (TraumaTier): <=30 a steady ~75 BPM heartbeat; <=20 ~105 BPM
  louder, a low-pass on the AudioListener (world only: 2D pool sounds, the player's own pulse/breath and crew voices
  bypass listener effects), dark-red vignette + chromatic aberration creeping in, corner blood splatter; <=10 ~145 BPM
  at full volume, a labored-breath clip, the vignette closing and swelling per beat, heavy layered dripping blood,
  slight blur + lens warp. One envelope (PlayerPulse.Beat) drives everything that pulses; Reduce flashing stills the
  visuals. Code: PlayerHealthEffects(.Trauma), UI/TraumaOverlay, UI/BloodSplatter (procedural textures, no art pack).
  The old LowHealthThreshold vignette/blur (from 35 HP) is gone; it now only feeds the fear heartbeat above 30 HP.
- 2026-10-09 (user, 0.13.5b; written, untested):
  - The board's THREAT level drives the job's monsters. Contract.ThreatLevel rides in RunTerms (ThreatShift, so a run
    with no contract is MEDIUM); per-level tunables live in ThreatPacingConfig (gap x1.5/1/0.7, opening delay
    x1.8/1/0.7, alive cap -1/0/+1, +1 opening monster on HIGH, non-lethal damage x0.75/1/1.2 via
    Threat.ContractDamageScale; LOW never opens with a one-touch killer). Modifiers' ExtraThreats now count in the label.
  - Guided first job: FirstDayGuide (session object, local, once per player, follows the Tips setting, off in batch
    mode) walks board > van > flashlight > scan > pick up > truck > floors > noise > quota > lever. While it shows, the
    HqBoard/HqVan/RunGoal/PickUp tips stay quiet. A company's first job (Runs == 0) delays the first monster to 150 s.
  - Stalker: follow speed 2.4 -> 3.4 (StalkerConfig.asset edited by hand; damage unchanged). Its prey and dread are
    replicated (sixteenths); breathing quickens with dread, a scream marks the rush and a knock marks the freeze, the
    prey's own heartbeat rises with it, and the compass shows a red mark at its bearing from 35 % dread.
- 2026-10-09 (user): the salvage company in the fiction is **Abandoned Salvaging** (CompanySave.DefaultName), named after
  the game. Never name anything in the fiction after the user or any real person. Old saves carrying "Zeyad Salvage Co."
  are renamed on load. The studio/publisher "Zeyad Games" (credits, Player Settings, save folder) is a separate thing
  and is unchanged until the user says otherwise.
- 2026-10-09 (user, tester feedback pass; written, untested): fitted mall furniture placed through
  MeridianDressingClearance is SOLID (one box fitted to each floor-standing model, as MallProps), replacing 0.13.2's
  "collider-free" for those pieces only; flat litter, decals, ceiling pieces and signs stay walk-through, and threat
  spawn tiles (MallPopulator.ThreatSpawnCells) stay clear. Takes effect on the next rebuild-horror (the NavMesh bakes
  around the boxes). HUD: health is ten always-visible blocks (UiKit.Segments); a compass tape (CompassHud, world
  +Z = north) sits top centre during jobs, with the scan's haul read-out moved under it. Default UI scale is 1.25x (GameSettings.DefaultUiScale; the existing
  slider, Settings > Gameplay > Interface, still runs 0.75x-1.5x and a saved value wins). Bestiary = the HQ office
  desk computer (SecurityDesk in HqDressingBuilder carries ThreatFilesTerminal; E opens ThreatFilesScreen, a CRT
  ScreenPanel on the session object). A file opens when a monster first comes within 16 m of the local player
  (ThreatFilesTracker; local Prefs like tips, names-only toast); texts live in Threats/ThreatFiles.cs and must follow
  behaviour changes. Still open from the same feedback: tutorial, THREAT label vs monsters, Stalker feedback.
- 2026-10-08 (user-approved 0.13.0 Meridian rebuild):
  - Environment art becomes grounded and realistic, replacing the earlier one-low-poly-style rule
    for the mall. Characters, threats and loot keep their current look; flag clashes for the user.
  - Approved plan: Docs/MERIDIAN_MALL_0.13.0_PLAN.md. The user's follow-up requires a stepped,
    recessed exterior and varied mall interior, not a large square/rectangular box; parking spawns
    frame the entrance, anchor frontage and skylight as a recognisable shopping mall.
  - Preserve the roughly 56 x 48 m envelope, but carve entry courts/corner setbacks and shape
    the atrium/circulation; keep modular 4 m sections and 4 m stories for the runtime contracts.
  - Separate Meridian Blender kit/materials preserve shared HQ, wardrobe and legacy art exactly.
    Rebuild via compile/rebuild-horror/build only; user tests. Editor art still renders are approved.
  - Protect service landing slabs, haul/escape connections and loading approach as well as stairs;
    permanent authored descent exits reuse existing SafeDescent, with no new rope inventory system.
  - Final authoring uses 3 m service stair clear width and 4 m switchback haul ramps, with permanent
    half-story turn landings; middle east rope access includes the west connector slab.
  - Original Meridian PBR maps, fractured mesh siblings, damage variants, realtime fixtures and overcast
    sky are independent of legacy HQ art. Occlusion bakes only permanent geometry; floor slabs are never static.
  - Editor art captures show ordinary open shutters and power variants without running gameplay or saving
    the scene. Performance counters from batch art rendering are unavailable; user measures Editor F1 or
    a Development build/profiler, because shareable builds keep development controls disabled.
  - No new packages, paid assets, TestMap changes or pushes. Commit each checked stage. Four-player
    host performance stays pending the user's run; report Editor/asset figures honestly.
- 2026-10-08 (user, pacing pass; written, untested): monsters keep arriving all job (ThreatPacingConfig: 25 s opening,
  then every 90-150 s, cap 3 + crew, max 8) and idle ones drift toward / are relocated near the crew; behaviour unchanged.
  The building decays on a fixed clock (BuildingDecayConfig: none to 3 min, then every 50/30/20/10 s from 3/10/17/25 min)
  and at 40 min the whole building collapses over 60 s; anyone inside dies, and if nobody survives the job is lost (haul 0,
  save untouched); outside = safe. Rubble stays until the next run: loose chunks settle (cosmetic) and each fallen section
  leaves a SOLID mound that blocks players/loot/monsters (user: rubble must be big and block) - this replaces "debris never
  blocks" for mounds only; mounds are deterministic and local (not networked), never on stairs or SectionKeepClear route
  sections. Each monster has its own song; only the highest in the hierarchy (one-touch kill > damage rate x chase speed)
  of the monsters approaching on your floor plays, starting at least 15 s before it can reach you. No Collector tip. Never test/verify/run anything
  (no compile/rebuild/build either) unless the user asks.
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
- 2026-10-08 (0.12.6, QA fix pass from Docs/QA_REPORT_2026-10-08.md): dev keys only in editor/Development builds
  (Core.DevTools). The truck shelters only while honking, or always with the Armored Bay. Crashed jobs are voided;
  deliberate mid-job exits still count. Host crew rules (CompanyService.CrewRules, default: crew may pull the lever only)
  and Kick. Player names: Steam > Settings name > "Player <seat+1>". Save slots 1-3 (slot 1 = company.json).
  0.12.7 (user): the players work for a company they hate; it isn't theirs. No renaming it, no "your company" wording.
  HorrorConfig is per level (scene list, positions, sectors). Runtime scripts stay under 300 lines via partial classes.
- 2026-10-08 (0.13.1, user visibility/parking follow-up): stronger 26 m / intensity 28 / 68-degree flashlight,
  brighter powered mall fixtures and reduced fog; keep existing battery duration/recharge and horror power sectors.
  Author a marked parking lot, crew arrival van beside the player spawns, independent street lamps and a clear
  collection approach. The existing truck remains the cargo-counting and extraction system: goods must be inside
  its cargo bay, and payment occurs on departure. The mall-only ramp/apron are 4.4 m wide; cargo dimensions and
  HQ remain unchanged. Visibility settings migrate once, then preserve future inspector tuning.
- 2026-10-08 (0.13.2, user density/signage/lighting follow-up): the mall needs substantially more fitted remnants
  of its former use: contextual store displays, counters, seating, service equipment and concourse furniture.
  Add original detailed LOD assets with existing Meridian PBR maps, collider-free and bound to their supporting
  structural section. Keep doors, loot sites, flight approaches and the permanent haul route clear. Remove
  scattered notice text, slogans, duplicate labels and the collection instruction billboard; retain a small set
  of physically mounted identification/exit/collection signs. Increase visible working fixtures and powered
  lighting while retaining power-off, collapse and horror-sector behaviour; no additional shadow lights.
