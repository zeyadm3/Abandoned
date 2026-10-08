# ABANDONED — Build Progress

Living log for autonomous work. Read CLAUDE.md, then this file. Detailed per-task notes live in
`Docs/progress/M<N>.md` (read only when a task needs them); implementation-level decisions in
`Docs/progress/DECISIONS.md`; task prompts in Docs/PLAYBOOK.md.

## 0.13.4b.EEA — exclusive early access build for friends (2026-10-08)

A shareable Mac + Windows build for friends to pass around. Every launch shows an EXCLUSIVE EARLY ACCESS box over
the title screen ("very early access - gameplay work in progress", feedback/feature requests to
zeyad.m.a.2005@gmail.com, with Email / Copy email / OK). The menu footer carries the same tag, one company memo
names the address, and LaunchConfig's Feedback link is a mailto, so the menu's Feedback button appears.
Pipeline: compile, rebuild-horror, commit, build (user-requested); zips copied to the Desktop.
Follow-up (c40757c): an EXCLUSIVE EARLY ACCESS watermark sits bottom-right on every screen (EarlyAccessWatermark on
MenuUi's document, kept on top, also in F10 clip mode; only when the version contains "EEA"). Rebuilt with compile +
build; the Desktop zips were replaced and the 0ecc0af ones moved to Abandoned-builds/dev/superseded/. Not run in play.

## 0.13.4b — monster songs only on your floor while it approaches; no Collector tip (2026-10-08)

Written, not compiled or run. A monster's song now plays only while it is on the listener's floor (feet within
2.5 m height) and the gap is shrinking; it stops 2 s after either ends. The one-time Collector tip card is removed.

## 0.13.4 — monster arrivals, monster songs, a decaying mall and lasting rubble (2026-10-08)

The user's brief: monsters were too rare (30 min in the mall without meeting anything), each monster should have
its own song that swells as it approaches (only the most dangerous one heard), the mall should fall apart the
longer the crew stays, and rubble should stay. **Written only: nothing was compiled, run or verified** (user rule:
they do all testing, running and verifying). Run `Tools/unity.sh compile`, then `rebuild-horror`, then `build`.

- **Arrivals** (`ThreatPacingConfig`, `ThreatPacing`, `ThreatDirector`): opening monster at 25 s, then one every
  90-150 s (seeded) for the whole job, alive cap 3 + 1 per player (max 8; the final hunter not counted). Behaviour
  untouched. A monster nobody has been within 30 m of for 60 s wanders around a random living player's area
  (Blind One, Hunter, Thing, Weight, Crawlers; `Threat.RoamHint`); one over 45 m from everyone for 120 s is replaced
  at the spawn point nearest the crew that is unseen and 14 m+ away. Stalker, Collector, Last Hunter are left alone.
- **Decay schedule** (`BuildingDecayConfig`, `BuildingDecay`, added beside `DangerDirector`): run clock from the
  truck's arrival. 0-3 min nothing; 3-10 a section every 50 s; 10-17 every 30 s; 17-25 every 20 s; 25-40 every
  10 s. Each pick cracks (5 s of groaning) then fails through the normal path (sag, sync, cascades, NavMesh carve);
  70 % of picks come from within 30 m of the crew. Support-jacked sections are skipped. Protected route sections
  (`canCollapse` off) never fall. 2 min before 40:00 a dispatch warning plays.
- **Total collapse at 40:00**: alarm, rumble, shaking and a radio countdown; the HUD reads `COLLAPSE Ns`. Every
  collapsible section comes down over 54 s, top floor first and far from the truck first. At 60 s anyone still
  inside the building (HorrorConfig bounds, truck excluded) dies ("The mall came down on you."). If that leaves
  nobody alive, the job is lost: haul 0, nothing recovered (`RunState.Crushed`); the company save is untouched.
  Anyone outside lives and can drive off. `RunNetState.CollapseAt` carries the countdown to every machine.
- **Rubble stays** (`DebrisLifetime`, `DebrisSpawner`, `RubbleMound`, `StructureSimulation.Rubble`): chunks
  tumble, then settle kinematic and stay until the next run's restore (resting budget 2500 chunks, then newer
  loose chunks clear; mounds always stay). Each fallen section also drops a **solid mound** (1.3-1.8 m tall,
  80 % of its footprint, seeded, identical on every machine, not networked): it blocks players, loot and monsters
  (NavMeshObstacle carve), lifts a player standing there onto it and sets buried resting loot on top. It never
  lands on stairs or `SectionKeepClear` sections (protected tiles and turn landings, added by MallPopulator), and
  drops again if the floor under it falls. `StructuralSection.SurfaceBounds` now remembers where a fallen section was
  (disabled colliders report empty bounds).
- **Monster songs** (`ThreatMusic`, `ThreatMusicSynth`, `ThreatDefinition.MusicClip/HierarchyOverride`): eight
  original synthesised loops. Hierarchy = one-touch killers first, then damage / attack gap x chase speed:
  Last Hunter > Hunter > Stalker > Blind One > Thing > Crawlers > Collector > Weight. A monster counts only while it
  is on the listener's floor (feet within 2.5 m height) and actually closing in; its song starts when it could reach
  the listener within 15 s at its top danger-scaled speed (straight-line, so at least 15 s), swells to full at 2 s,
  and stops 2 s after it stops approaching or leaves the floor. The Collector tip card is gone (no theft hint).
  Only the highest-ranked counting monster plays (1.5 s crossfade). Local per machine, Music volume slider.
- **F1**: threats alive/cap/next arrival/drifting, drift gizmos; decay interval/next/cracking/down or the total
  collapse countdown, gizmos on cracking sections; music leader, rank, ETA, volume.

Test first (user):

1. Stay 5-10 min in the mall: monsters keep arriving (F1 THREATS line) and find the crew; a song starts well before
   one reaches you and only the top-ranked monster's song plays when two approach.
2. From 3:00 watch sections crack then fall on the schedule; rubble chunks and solid mounds stay; walk into a mound
   (blocked), stand where one lands (lifted on top), check the escape route never gets a mound.
3. Temporarily set BuildingDecayConfig > Total Collapse Minute to 2 (Data/Extraction/Resources) to reach the end: countdown, everything falls, dying inside loses the job,
   standing outside survives. Check frame rate late in a long job (many mounds and resting chunks).

## 0.13.2 — fitted mall remnants, restrained signs and more working lights (2026-10-08)

Responds to the user's feedback that the abandoned mall still feels too empty, the scattered text looks strange,
and the map needs more lighting and detailed assets.

- **Art:** fourteen original Blender fixture modules with manufactured detail and LOD0/1/2/cull, sharing the
  existing Meridian PBR textures: benches, bins, blank ad lightboxes, clothes rails/hangers, retail gondolas,
  checkout counters, food booths/service stations, cinema screen/ticket counter, service cart, stock cages,
  ceiling ducts and shuttered kiosk. The kit now contains 97 FBX models; no new textures/downloads/packages.
- **Dressing:** contextual wall runs and shop fixtures, more food-court seating/service fittings, cinema seating
  and screen, concourse furniture and basement storage. Placements read actual layout/support/loot data and
  measured model footprints. Decorative objects remain collider-free and follow their supporting section;
  protected routes, doors, escalator/stair approaches, jackpot tiles and loot centres remain clear.
- **Text cleanup:** removed automatic store-door labels, ambient slogans/notices, the gallery warning panel,
  repeated food-stall labels, crew-van lettering and the large collection instruction billboard. A restrained
  set of mounted mall/cinema/food/service/exit/collection identification remains; HUD and haul board still work.
- **Lighting:** more actual working fixtures throughout stores, food areas and service spaces, plus roof-hung
  atrium luminaires. Powered fixtures are brighter; dead/faulty fixtures remain as deliberate exceptions.
  The existing battery flashlight, power-off runs, light sectors, final red phase and collapse hooks remain.
- **Authoring census:** 298 added fitted modules (basement 9, ground 85, first 110, top 94), including all fourteen
  new asset types. Sparse legacy store displays were replaced by the fitted runs. Ceiling lights increased
  120 → 354 plus four roof-hung atrium fixtures; fixture intensity 8.4 → 18, local range 7.5 m, atrium 42 / 12 m,
  dead/faulty defaults 10% / 28%. Four fixture shadow lights plus moon remain. TextMesh labels decreased 58 → 16.
- **Checks:** compile and `rebuild-horror` passed: 11 flights / 0 problems, Mall + HQ invisible colliders 0
  (0 intentional), 306 materials / 0 problems. Blender geometry inventory: 97 models / 0 problems.
  No gameplay, EditMode/PlayMode, network or screenshot tests run.

Test first:

1. Walk the shops, food court and cinema with power on; check the added fitted assets and simpler signage.
2. Carry/trolley heavy loot through shop doors, permanent haul ramps and the truck approach; check dressing
   leaves carrying space and never catches a player or the load.
3. Try power off and a collapse near new fixtures/dressing, then record atrium FPS/batches with four flashlights.

## 0.13.1 — visibility, crew arrival and collection bay (2026-10-08)

Responds to the user's dark-mall feedback and request for parking, an arrival vehicle and a clear place to haul
and cash out salvage. The 0.13.0 shaped mall architecture and gameplay layout remain the basis of this update.

- **Flashlight:** 26 m range (was 12), intensity 28 (was 6), 68-degree outer / 40.8-degree inner cone, neutral white.
  Config, generated player prefab, runtime initialisation and art renders use the same settings. Battery stays
  420 seconds with the existing 8 seconds/second truck recharge. One-time config migration preserves later tuning.
- **Mall visibility:** powered fixtures 8.4 intensity (was 2.1), 9.5 m range, moon 0.22, brighter ambient fill,
  reduced exterior/interior fog (0.010 / 0.018), reduced vignette and +0.2 EV exposure. Dead/faulty lamps,
  collapse attachment, power-off runs and the existing horror-sector rules remain authored as before.
- **Arrival:** 40 marked parking bays, a pedestrian crossing and entrance arrows, neglected islands, an original
  branded crew van beside the four spawn positions, three earlier vehicles and five independent street lamps.
  The existing credited CC0 Car Kit is reused. Ground has visible colliders; dressing stays non-blocking.
- **Collection:** the actual extraction truck has a signed 4.4 m ramp/apron approach, painted cargo boundaries,
  haul directions and clear lever/payment instructions. The existing 2.6 × 5 m cargo bay counts goods;
  payment occurs on departure. Leaving items on the outside apron does not count as cargo. No economy changes.
- **Review:** 24 Editor art stills and regenerated level maps in `Docs/renders/0.13.1/`, including dedicated arrival
  and collection views with power on/off. FPS, visible batches and four-player host performance need the user's
  in-game profile. See `PERFORMANCE.md` for the imported texture budget and scene census.
- **Checks:** compile and final `rebuild-horror` passed: 11 flights / 0 problems, Mall + HQ invisible colliders
  0 (0 intentional), 306 materials / 0 problems. Art capture confirmed saved scene bytes unchanged. Imported
  texture GPU storage is 127.98 MiB (256.04 MiB Editor/native); five added parking lights cast no shadows.
  No EditMode/PlayMode, gameplay, network or screenshot tests run. TestMap and HQ are unchanged; unrelated
  regenerated assets were restored. Existing low-poly vehicles remain stylistically simple in this environment.
- **Builds:** shareable Mac and Windows passed from clean commit **d95ced2**. Archives are
  `~/Documents/Abandoned-builds/dev/Abandoned-0.13.1-d95ced2-Mac.zip` (**92.1 MiB**) and
  `Abandoned-0.13.1-d95ced2-Windows.zip` (**82.5 MiB**). Build reports: Mac 291 MB / 14 seconds;
  Windows 271 MB / 10 seconds. Both stamps agree on version, clean commit and UTC build time
  (2026-10-08T15:23:09Z); platform/size fields differ as expected. No players launched or push performed.

Test first:

1. Spawn by the crew van, follow the crossing into the mall and compare flashlight reach with power on/off.
2. Follow the east-drive collection signs; drag/carry a large item up the ramp, place it fully inside the truck,
   check the haul board and pull the lever to cash out after departure. Check the board ignores apron staging.
3. Check full-danger/final-phase lighting and record atrium FPS/batches with four crew flashlights.

## 0.13.0 — Meridian Mall rebuild (2026-10-08)

Built the approved realistic mall design with the user's shaped exterior requirement: recessed entry court,
stepped anchor wings, clipped skylit atrium, three shopping levels and a flooded service basement within the
56 × 48 m envelope. Parking spawns face the entrance; the loading truck and dock align on the east side.
The fountain, escalators, piano bridge, food court, Final Reel cinema and pre-collapsed gallery are landmarks.

- **Original art:** 83 Blender FBX exports, 25 PBR sets / 75 maps, plus an original 512 px overcast sky.
  Hero maps are 2048, standard maps 1024, small dressing 512; packed URP metallic/AO/smoothness masks,
  normal-map imports, mipmaps and desktop compression. Credits, stable paths and export inventory are recorded.
  Floor finishes have authored crack/sag stages and matching fracture siblings; large kit pieces have LOD0/1/2/cull.
- **Gameplay authoring:** 432 structural sections, 11 flights, permanent 3 m service stairs and 4 m haul ramps,
  protected turn landings and exit approaches, authored rope descents, 7 shutters, exact existing loot tags and
  featured prizes, basement threat coverage, updated truck/spawns, horror sectors and atmosphere bounds.
  NavMesh and never-breaking occlusion are baked by the builder. Floors carry no static flags.
- **Presentation:** collider-free fixed fixtures/dressing, leaked skylight growth and puddles, scavenger corner,
  rust/waterlines, exposed gallery rebar, fitted world-text signs, 120 ceiling lamps plus the marquee fixture,
  4 fixture shadow lights plus the moon, 3 realtime reflection probes, area ambience and surface audio mappings.
- **Validation:** compile and `rebuild-horror` passed: 11 flights / 0 problems, Mall + HQ invisible colliders
  0 (0 intentional), 298 materials / 0 problems. No gameplay, EditMode/PlayMode, network or screenshot tests run.
  TestMap and HQ content remain unchanged; unrelated regenerated assets were restored before committing.
- **Review artifacts:** `Docs/renders/0.13.0/` contains 20 Editor art stills, including power on/off in eight spaces,
  the atrium with four flashlight beams and three floor damage closeups. The renderer verifies the saved scene
  bytes unchanged. `floorplans/` contains SVG/ASCII maps exported from actual ownership and section data.
- **Performance evidence:** material/sky textures use **127.81 MiB** calculated imported GPU storage with mipmaps
  (**255.70 MiB** Editor/native allocation), well below the 1.5 GB material-map budget. The asset census includes
  inactive damage/LOD variants. Batch UnityStats returned all zero, so visible batches/SetPass/FPS are unavailable.
  Realtime reflection targets/transient buffers and four-player host simulation still require an in-game profile;
  the 60 FPS / ~2,500 batch target is **not yet verified**. See the render folder's `PERFORMANCE.md`.
- **Builds passed:** version 0.13.0 from clean commit **385f5f4**, both stamped with the same UTC time.
  `~/Documents/Abandoned-builds/dev/Abandoned-0.13.0-385f5f4-Mac.zip` (**91.8 MiB**) and
  `Abandoned-0.13.0-385f5f4-Windows.zip` (**82.3 MiB**). Build reports: Mac 290 MB / 143 s,
  Windows 270 MB / 129 s. Archive BUILD.txt stamps confirmed; non-shipping debug folders excluded.
  Unity's unrelated connect-settings churn was restored after the build. No push.
- **Known review gaps:** original procedural materials still have visible repetition; existing low-poly loot,
  vehicle, characters and threats retain their prior style and need review in the new setting. Art stills do not
  spawn run loot/AI or reproduce flicker timing. Multiplayer carrying, fallback use, collapse pacing, sector changes,
  darkness/readability and loading cost are left for the user's play pass.

Test first:

1. Spawn outside, enter the recessed court, navigate by the fountain/cinema/food court and reach the truck.
2. Carry/trolley the piano and generator through all haul-ramp turns, wide doors, dock apron and truck ramp.
3. Collapse bridge/escalators/gallery approaches; check crack/sag readability, matching debris, permanent service route
   and every rope descent (especially the middle east connector), then return to extraction.
4. Try power on/off, Sealed shutters and director slams near crew/loot; check fallback stores stay open, falling lamps
   stop lighting, two sectors remain lit at maximum danger and the final phase turns red.
5. Check loot/AI routes with 1–4 crew, then record **Editor F1 or Development build/profiler** FPS, batches, main-thread
   time and GC in the atrium with four flashlights. Shareable builds intentionally disable development controls.

## 0.12.7 (2026-10-08)

From the user's pause-menu screenshot: you work for a company you hate; it isn't yours. The pause menu's COMPANY
section (rename, "Start a new company") is gone, as are those host methods; the Play screen's slot picker is "Save"
(level, money, jobs); menu copy says crew, not company owner. The crew panel is one scrolling body under the title
(note, crew rows, join hint, HOST RULES: THE CREW MAY), 600 px tall, so the rows have room and nothing spills out of
the panel. Not tested (user tests); compile and builds only. Builds **0.12.7**, commit **8facd66** (clean):
`~/Documents/Abandoned-builds/dev/Abandoned-0.12.7-8facd66-Mac.zip` (54 MB) and `...-Windows.zip` (44 MB).

## 0.12.6 — QA fix pass (2026-10-08)

Fixes everything Docs/QA_REPORT_2026-10-08.md asks for (bugs B-01..B-36, balance D-01..D-10, performance P-01..P-08,
code health C-01..C-05, accessibility O-01). Not tested (user tests everything): compile, `rebuild-horror` (5 flights /
0 problems, Mall + HQ invisible colliders 0, 270 materials / 0) and builds only. Builds **0.12.6**, commit **de86d60**
(clean): `~/Documents/Abandoned-builds/dev/Abandoned-0.12.6-de86d60-Mac.zip` (53 MB) and `...-Windows.zip` (45 MB).
Commits: 037b562 (bugs), 4bff364 (balance/perf/health), de86d60 (regenerated content).

- **Critical:** a shutter never slams with a living player in the store or near its doorway, or loot in the doorway;
  from inside, E heaves a locked shutter up by hand (6 s, loud); the slam has its own sound. F1 overlay, K ragdoll and
  the structure keys work only in the editor and Development builds (`Core.DevTools`); F9 free camera in player
  builds only at the HQ or as a ghost; debug overlays are switched off in player builds.
- **High:** the truck shelters you only once it's honking to leave, or all job with the Armored Bay (was: always, so the
  upgrade did nothing). Level travel loads async, shows LOADING % / WAITING FOR <names>, and drops a machine that hasn't
  loaded after 90 s. A crash or lost connection voids the job (no penalty, no strike); quitting from the pause menu or
  closing the window mid-job still counts as failed (the button says so). Real names: Steam name, else Settings >
  Gameplay > Your name, else "Player 1-4" by crew seat. Host crew rules in the pause menu (crew may spend / start the
  van / pull the lever; default: lever only), Kick (press twice). Lever below quota needs a second pull within 4 s;
  pulling it while the truck honks (more than 2 s left) stops the departure. Settings > Accessibility: Reduce flashing
  lights, Fewer jump scares, heartbeat volume; subtitles on by default.
- **Medium/low:** ragdolling mid-fall keeps the fall height; E works with loot in hand (levers, shutters, notes, and
  pocket-sized loot); Tab + wheel picks which pocket to drop; others see a corpse kept on the real body (medkit works
  there); a dead player's pockets spread in a ring; monsters spawn 14 m+ from and out of sight of players; the Stalker
  breathes (no creak); chat is byte-safe and markup-proof; Sealed jobs say TOOLS: bolt cutters or a crowbar; loot held
  by someone aboard counts; a wipe pays 50 % of the bay, no bonus; truck materials are assets (no Shader.Find); a throw
  during a pending release is queued; rubble/players don't block aiming; save slots (Play screen); per-player voice volume remembered by Steam id; recovery falls back to the
  last floor stood on; touching loot swung into you knocks you down; authored shadows kept; jumping blocked while
  hauling; video settings cached and flushed; Chat (T), Free camera (F9), Hide HUD (F10) and the new Give gear (G) are
  rebindable.
- **Balance:** flashlight 420 s, recharging 8 s/s in the truck; two of six light sectors keep power at max danger;
  ageing every 15 s at 8 %; solo final phase 60 s later; one plain job on every board; the Blind One only strikes
  someone it heard within 3 m in the last 2 s; the Collector keeps one nest per run that jingles; alone, the company's
  hand trolley needs no hand slot; G hands your active gear to the crewmate in front of you or takes a fallen one's.
- **Performance/code:** cached structural supports (SupportCache), re-paths at most 4/s, horror lights only on change,
  cheap truck mirror, no shadows on others' and corpses' flashlights; RunHorrorDirector, TruckSanctuary, MysteryNote and
  ServerSlam reformatted; HorrorConfig holds the level's scene list, positions and light sectors; every runtime script
  is under 300 lines (partial classes); tunables moved to configs.
- **Not done (decisions/assets for you):** a font with Arabic/Cyrillic/CJK glyphs (names/chat may show boxes),
  D-05 money needs playtest numbers, and the report's feature ideas/assets/options lists (sections 7-10) beyond the above.
- **Test first:** report section 11 is the checklist; new controls: G, Tab + wheel, lever double-pull and cancel, pause
  menu host rules + Kick, Play screen Save slot, Settings > Accessibility.

## 0.12.5 (2026-10-08)

From the user's screenshots (Desktop, file names = instructions). Not tested (user tests everything); compile,
`rebuild-horror` (5 flights / 0 problems, Mall + HQ invisible colliders 0, 268 materials / 0 problems) and builds only.
Builds **0.12.5**, commit **aec1223** (clean): `~/Documents/Abandoned-builds/dev/Abandoned-0.12.5-aec1223-Mac.zip` (53 MB)
and `Abandoned-0.12.5-aec1223-Windows.zip` (44 MB).

- **Vertical text in buttons** (Report a problem, Open logs, Play's Steam/Direct IP, gear rack Empty, settings tabs and
  On/Off): 0.12.2's MenuTactile put child elements inside buttons, and a text element with children stops measuring its
  text. Now no children: the marker is a stylesheet underline, the static a background swap.
- **Contract board:** MenuKit.Panel's inline width beat `.panel--xwide`; the board is now 1320 px. Fact rows are a fixed
  label column and one value column (warnings only recolour); the crew size is a note under the quota.
- **Settings redone:** landscape, pages down the left (Gameplay, Controls, Video, Graphics, Audio, Voice, Accessibility),
  an explanation under every option, per-page Restore defaults. New, working: invert vertical look, crouch hold/toggle,
  crosshair on/off, render scale, MSAA, mute when in the background, subtitle size.
- **Answering machine:** a modelled kit asset on the office desk (cassette, keys, handset, grille), blinking light kept.
  The user's 0.12.3 HQ hand edits now live in HqDressingBuilder, so HQ rebuilds keep them.
- **Wardrobe redone:** 25 new original hats/accessories (Blender kit, fitted to the worker) + 14 coveralls. Each is free,
  bought once with company money (owner asks, host charges via `CompanyService.TryCharge`, owner keeps it in
  PlayerProfile), or a reward for one of the 12 achievements. Landscape screen: preview (front-on, sway, drag to turn,
  selection is tried on), cards per tab, detail panel with Wear / Buy / how to earn, company funds and purchase result.
- **Test first:** every small button above; Settings (each page, Restore defaults, invert Y, crouch toggle, render
  scale/AA in a build); contract board with crew of 1-3; the answering machine (E, blinking); wardrobe (buy with enough
  and too little money, as host and as client; a reward item after its achievement; drag the preview).

## 0.12.3b (2026-10-08)

Same content as 0.12.3 (the user's HQ edits + the garage door), rebuilt with the editor closed. Builds stamped
**0.12.3b**, commit **49e090e** (clean): `~/Documents/Abandoned-builds/dev/Abandoned-0.12.3b-49e090e-Mac.zip` (53 MB)
and `Abandoned-0.12.3b-49e090e-Windows.zip` (44 MB). Not tested (user tests).

## 0.12.3 (2026-10-08)

Version bump over 0.12.2 plus the HQ garage door (4a730c5) and the user's own HQ scene edits, committed
as made in the editor: all seven ceiling tubes brightened (intensity 30-50, was 1.4-6.5), the
"ASHLINE / DISPATCH", "TAKE ONLY WHAT YOU CAN CARRY" and "ASHLINE SUPPLY" labels moved, four pillars
moved, and the "NO CREW LEFT BEHIND" label, the yard door lamp and the EXIT sign removed. The garage door
stays at its default (closing the garage opening). **These hand edits live only in Scenes/HQ.unity:
`Tools/unity.sh rebuild-horror` (or Create HQ) regenerates the HQ and would drop them.** Port them into
HqDressingBuilder/HqBuilder before the next HQ rebuild. No builds yet (the editor was open): close Unity,
then `Tools/unity.sh build`.

## Fix pass — 0.12.2 (2026-10-08)

Fixes the five problems from the 0.12.1 play test. Per the user: no gameplay, EditMode/PlayMode/nettests
or screenshot QA; compile, `Tools/unity.sh rebuild-horror` and `Tools/unity.sh build` only (all passed,
no project warnings). Builds are stamped **0.12.2**, code commit **eb11c36** (clean). Archives in
`~/Documents/Abandoned-builds/dev/`: `Abandoned-0.12.2-eb11c36-Mac.zip` (53 MB; 152 MB app) and
`Abandoned-0.12.2-eb11c36-Windows.zip` (45 MB; 133 MB player). TestMap untouched. Commits: cc2450e (text),
a4d51b5 (materials), 0edc22d (collider audit), fdd09b7 (stairs/kit), 19b65de (menus), eb11c36 (version + regen).

- **1. Stairs.** Root cause: `Tools/Blender/environment.py` mapped Unity z to Blender -Y, so all 52 kit
  models imported mirrored along Z. Every flight's steps were drawn *behind* its foot over open floor (no
  collider: you walked through them) while its ramp stood undrawn. `xyz()` fixed, lettering re-faced,
  normals made consistent, all FBX re-exported (same GUIDs); asymmetric kit props/signs now face the way
  the builders intended. MallFlights fits ramp + handrail-height rails to the model and checks slope/lip
  against PlayerMovementConfig. The basement stair surfaced under Stairs_G1; it now rises in column 12
  (`MallLayout.BasementFlight`), and the loading-bay east door opens onto floor again. Stairs remain
  StructuralSections (escalators collapse, service stairs are the never-collapse route); NavMesh is baked
  over the ramps. New rebuild check `MallFlightValidator` (player capsule sweep, ground/headroom/slope/lip,
  NavMesh foot-to-head path): 5 flights, 0 problems.
- **2. Invisible blocks.** New rebuild check `InvisibleColliderAudit` (Mall + HQ): every solid collider on a
  player layer must sit inside visible geometry; deliberate ones get `IntentionalInvisibleCollider` and are
  listed (none). First run reported exactly the flights' ramps and rails (the mirror above); now 0 / 0.
- **3. Textures.** New `MaterialAudit` at the end of rebuild-horror over Mall, HQ, every prefab and our
  .mat assets: Kenney FBX materials remapped to editable URP Lit copies (Art/Generated/ThirdPartyMaterials),
  non-URP shaders converted, untextured materials given original grain maps (soft sprite for particles), the
  light shaft's unsaveable built-in white texture replaced. Report: 257 materials, 0 problems.
- **4. Mirrored text.** TextMesh signs/labels/crew numbers used the font's GUI/Text material (double-sided,
  no depth test: readable mirrored from behind, even through walls). New `Abandoned/WorldText` shader
  (depth tested, drawn only from the reading side) via `WorldTextMaterial` + runtime `WorldTextBinding`;
  kit lettering (Chalk) and notices (Paper) are back-face culled.
- **5. Menus.** Title: eye-level HQ view (`MenuVantage`) drifting through dust; rare random beats (a tube
  dies, a figure at the far end gone on the next flicker, a groan + shudder); worn, fringed, glitching logo;
  grain, scanlines, vignette, VHS band; static cut / flicker-in / CRT-off transitions. Buttons: hover and
  focus jitter, glow, scratched marker, static; press-in, flash, glitch; Host/Join/Start/Leave/Quit jolt the
  screen. Pause fades in with a flicker; a runtime URP volume (`MenuPostEffects`) darkens, desaturates and
  blurs the running game. Tunables in `Data/UI/Resources/MenuEffectsConfig`; Settings > Video and
  Accessibility > "Reduce menu effects". Original procedural textures, credited in Docs/ASSET_CREDITS.md.
- **Test first:** walk up/down all four flights and the basement stair; walk the atrium escalator area and
  loading bay for invisible walls; look for flat grey/white/pink surfaces (Kenney loot, HQ); view signs,
  notices and crew numbers from behind; main menu (backdrop beats need ~15-30 s), hover/keys/gamepad on
  buttons, Host/Join; Esc in a game (blur, flicker-in), then "Reduce menu effects". MallTests' stair test
  coordinates were updated to the current layout but not run.

## Horror repair pass — 0.12.1 (2026-10-08)

Implemented following the user's 0.12.0 play pass. Compilation passed with no project warnings;
`Tools/unity.sh rebuild-horror` passed with no scene generation errors. Final Mac universal and
Windows x64 shareable Mono builds succeeded, stamped version **0.12.1**, code commit **2a2b4d8**.
Archives in `~/Documents/Abandoned-builds/dev/`: `Abandoned-0.12.1-2a2b4d8-Mac.zip`
(53 MB compressed, 150 MB app) and `Abandoned-0.12.1-2a2b4d8-Windows.zip`
(45 MB compressed, 131 MB player). No gameplay, EditMode/PlayMode/nettests, screenshots or QA passes;
the user tests the completed builds. TestMap and the pre-existing material edits remain unchanged.

- **Interface:** corrected panel bounds, scrolling and icon layout; removed the yellow arrow shown
  while the ESC menu was open. Existing screens and interaction flows remain available.
- **Custom art / enclosure:** placement preserves imported FBX units, axes and root transforms.
  Repaired solid doorway headers/jambs, continuous opaque roofs and complete backed ceiling grids;
  ceiling dressing follows its supporting structural section. Corrected floor thickness, prop
  collision bounds, doorway clearance and note placement. Texture references remap to the original
  procedural maps; emission settings now survive subsequent content generation.
- **Threats:** corrected animation looping and phase transitions.
- **Lighting / atmosphere:** actual light output and diffuser glow now share the same authored
  flicker, including power loss and alarm changes. Darker indoor fog and sheltered spatial ambience
  reinforce entering an enclosed building; the arrival blackout wave now affects the intended hall
  lights in sequence instead of leaving inconsistent fixture states.

## Full horror redesign — 0.12.0 (2026-10-08)

User direction: fully autonomous authoring; no gameplay, EditMode/PlayMode/nettests, screenshots,
QA passes or new tests. Only compilation, required content generation and final player builds.
TestMap remains untouched. Six implementation commits follow the requested order; no push.

1. **Mall / arrival / escalation.** Meridian is a new 56 × 48 m, three-storey layout with a tall
   atrium, east service passage, dark fictional storefronts, food court, cinema, security office,
   quarantine rooms and a ruined gallery. A flooded lower level and parking garage lie beneath the
   east wing; ground slabs there can collapse. Fixed service stairs, loading exit and rope windows
   remain escape routes. Custom Blender kit replaces modular visuals while logical structural load,
   pre-fractured local debris, NavMesh bake/carving, seeded loot and shutters remain active.
   The truck parks facing the front entrance. Arrival beats run after the first player enters: groan,
   travelling light failure, distant scream, corridor silhouette and overhead footsteps within 46s.
   Host-synced cosmetic scares cause no damage. Danger rises every 110s, ages structure faster,
   progressively kills lighting sectors, brings audio closer and slams store shutters while preserving
   authored escape routes. Final phase begins 90s after the extraction window: siren, red fixtures,
   huge roar and an eight-second warning before the final hunter moves. Runs and departure have no
   music. Warm truck lamps, nearby engine idle, closing doors and a receding rear-view image mark escape.
2. **Existing monsters.** Eight Blender rigs include Idle/Walk/Chase/Attack/Special takes and original
   grime. Blind One stays sound-only (65 base damage); Stalker punishes isolation and looking away
   (85); Collector steals unattended loot without damaging players; Hunter retains its 450kg load
   and contact kill. Host-written motion state drives replicated animation with signature sounds.
3. **New monsters / final phase.** Crawlers unlock at danger 1, flee a usable flashlight and strike
   for 8 damage while dislodging carried loot. Weight unlocks at 3: audible ceiling movement winds up
   before damaging structural sections. Thing unlocks at 4: sustained flashlight inspection provokes
   it; switching the beam off breaks fixation. Last Hunter is final-only, breaks routes and kills on
   contact; truck occupants and players who have escaped the building are sheltered. New silhouettes
   arrive before duplicates; regular threats cap at eight. Danger improves speed/hearing/sight/memory
   and shortens attack cooldowns; damage increases up to 1.4×.
4. **HQ.** Dim Ashline salvage depot rebuilt with original modular walls, concrete tiles, exposed
   roof services, abandoned belongings, damp patches and missing notices. Stable warm light by the
   van/terminal contrasts with cold faulty overheads. Board, van, terminal, rack, wardrobe, voicemail
   and all four spawn positions remain connected to their existing flows.
5. **Health / falls / lifeline.** 100 host-owned HP, one host damage API, death at zero, no regeneration,
   reset on each run. Medkits restore 65HP or revive a downed teammate at 50HP. Fall curve is quadratic
   from 4m to lethal at 12m; carried weight increases damage up to 2.5×. Owner landing reports are
   bounded, ground-validated and rate-limited; collapse-body landings feed the same path. Damage vignette,
   low-health depth blur, heartbeat/breathing and local persistent corpses with switched-on floor beams.
   Flashlights reach 12m, flicker near threats, and have 180s of host-owned battery, with no automatic
   recharge. A buyable 500-credit single-use replacement cell is appended to the gear catalog.
6. **Interface.** Muted worn paperwork, grime and occasional restrained CRT flicker across existing
   UI Toolkit screens. Health, stamina and flashlight battery display when relevant; carrying prompts
   and inventory stay accessible. A subtle danger indicator and readable dispatch messages carry pressure.

Final build entry (2026-10-08): compilation and `Tools/unity.sh rebuild-horror` succeeded;
Mac universal and Windows x64 shareable Mono builds succeeded. Matching clean build stamp: `9e78ca2`.
Archives: `~/Documents/Abandoned-builds/dev/Abandoned-0.12.0-9e78ca2-Mac.zip` (53 MB) and
`Abandoned-0.12.0-9e78ca2-Windows.zip` (45 MB). Unity's generated cloud-settings change was restored;
Windows was rebuilt to remove its transient dirty stamp. No gameplay/tests/screenshots/QA were run.
The six stage commits are fd9a0c7, e49185c, 0d10746, 33921c9, 4be655b and 9e78ca2. Nothing pushed.

## Feel and presentation pass (2026-10-07)

Version 0.11.0, implemented and built 2026-10-08 (local time). Direction and user playtest checklist: Docs/POLISH.md.
The user explicitly requested: "build eveything and ill test after". Build the agreed pass now;
compile/content regeneration/builds only, no EditMode/PlayMode/nettests/gameplay/QA screenshots.
Existing/free resources and original code-built low-poly art; no new packages or paid art budget.
Scope: crew/monster visuals and restrained animation, mall decay/lighting, rotation/gentle placement,
carry sprint/flashlight rules, camera comfort, scan-only exploration values, Failing-only HUD cue,
quiet run audio and travel/departure music, plus original art for 22 previously placeholder collectibles.
Compile and full content regeneration passed. Mac/Windows full and demo builds passed; zips are in
~/Documents/Abandoned-builds/dev/ as Abandoned-0.11.0-400ff52-dirty-{Mac,Windows,MacDemo,WindowsDemo}.zip.
No runtime/gameplay/tests/QA screenshots were run. Five pre-existing material edits were restored
exactly after regeneration; user settings/untracked files are preserved. User plays the finished
builds and sets tuning priorities. Suggested commit: `Polish salvage handling and abandoned mall presentation`.

## Current state (2026-10-06, autonomous build M3–M10 on branch `autobuild-2`)
- M0–M2 done and hand-tested. M3–M10 built autonomously on `autobuild-2`
  (merged into `main` 2026-10-07; one folder again, no worktree). Don't wait for plan approval; record decisions,
  put human-only items under "Needs you". Push `autobuild-2` and tags freely (user OK'd).
- **M3 done** (tag `milestone-3`; review fixed, see Docs/progress/M3.md). 3.1–3.5 (+fixes) done: Facepunch fork + SteamBootstrap, NetworkBootstrap + networked player,
  builds + multi-process nettest, networked loot, shared carrying. M3.6 networked structure done (`nettest collapse`). M3.7 Steam lobby/invites done (fake-Steam tested; real Steam needs you). M3.8 robustness done (`nettest robust`).
- Company "Zeyad Games", bundle id `com.zeyadgames.abandoned` (user decision 2026-10-06).
- Last verified (2026-10-07, after M10 + TestMap rename): compile clean; verify ALL PASS; EditMode 221/221; PlayMode 253/254
  (MallTests entrance blocked by the M10.4 shutters: fixed, MallTests 9/9); all eleven nettests 4/4.
  (M8 tag: all eleven nettests 4/4, loot on a rerun);
  M7 tag: all ten nettests 4/4 (basic/loot/sharedcarry/collapse/robust/voice/run/travel/company/perf).
- Steam safety: Steam never initialises in batch mode or test runs unless Unity gets `-steam`. Never
  launch Steam from automation. `spike/facepunch-transport` is local only; never merge it.

- **M4 done** (tag `milestone-4`, review fixed): 4.1 proximity voice, 4.2 wall muffling + radio, 4.3 voice as monster noise (`nettest voice`; see Docs/progress/M4.md).
- **M5 done** (tag `milestone-5`, review fixed): greybox mall, seeded loot, runs + truck + appraisal, Blind One, danger (Docs/progress/M5.md).
- **M6 done** (tag `milestone-6`, review fixed): persistent session + travel, HQ, save, contracts, payday/debt/bankruptcy, shop + gear, Stalker + Collector, ghosts (Docs/progress/M6.md).

- **M7 done** (tag `milestone-7`, review fixed; Docs/progress/M7.md): 7.1 Kenney CC0 art (loot models, store
  props, vehicles), 7.2 realtime lighting (fixtures that fall with their ceiling and follow power, fog,
  post, dust, skylight shaft, flashlight shadows), 7.3 audio (CC0 sound bank, pooled playback, ambience,
  subtitles feed), 7.4 menus (main/pause/crew/settings/keys/credits), 7.5 cosmetics (coveralls + hats,
  HQ wardrobe), 7.6 performance (F1 perf line, nettest perf, occlusion, shadow budget).
- **M8 done** (tag `milestone-8`, review fixed; Docs/progress/M8.md): UI Toolkit HUD + HQ/appraisal screens,
  first-time tips + How to play, Demo build flavour (`Tools/unity.sh build-demo`: 5 jobs, own save, end
  screen with wishlist), floor warnings + truck pop-ups, the bridge piano jackpot, `nettest soak`, F10 clip mode.

- **M10 done** (tag `milestone-10`; Docs/progress/M10.md): Early Access content + launch kit (see Next). UNTESTED.
- **M9 done** (tag `milestone-9`, review fixed; Docs/progress/M9.md): the Hunter, 8 modifiers, 57 loot items,
  crowbar/backpack/support jack, 12 achievements, Steam Cloud (Auto-Cloud). Review fixes untested (user brief).

## Next
M7 done (tag `milestone-7`; builds in ~/Documents/Abandoned-builds/dev/).
-> M8 demo (Docs/progress/M8.md; version 0.8.0): 8.1 HUD pass done (in-run HUD, HQ + appraisal
screens on UI Toolkit), 8.2 onboarding tips + How to play done, 8.3 demo flavour done (`Tools/unity.sh build-demo`), 8.4 floor warnings + truck pop-ups done.
8.5 bridge piano jackpot, 8.6 `nettest soak`, 8.7 F10 clip mode, review fixed.
M8 done (tag `milestone-8`; `build` + `build-demo` zips in ~/Documents/Abandoned-builds/dev/).
-> M9 content (Docs/progress/M9.md; version 0.9.0; mall-only per the decisions log): 9.1 the Hunter done,
9.2 modifiers (8) done, 9.3 loot table (57 items) done, 9.4 gear (crowbar, backpack, support jack) done, 9.5 achievements (12, local + Steam hand-off) done,
9.6 Steam Cloud (Auto-Cloud; Docs/STEAM_SETUP.md) done. M9 review fixed (compile clean; per the user's 2026-10-07
brief no tests/nettests were run from here on), tag `milestone-9`, `build` + `build-demo`.
-> M10 Early Access (Docs/progress/M10.md; version 0.10.0): 10.1 truck upgrades, 10.2 flatbed trolley, 10.3 rope & pulley,
10.4 locked stores + Sealed/Hot Property/Condemned, 10.5 motion detector + night vision, 10.6 cosmetics (15 hats, accessories),
10.7 the boss's voicemails, 10.8 music, 10.9 balance (Docs/BALANCE.md), 10.10 bug sweep, 10.11 launch kit (F9 free camera,
links, build-release, Docs/STORE_PAGE.md, TRAILER.md, LAUNCH.md). All done (compile clean, untested); tag `milestone-10`,
`build` + `build-demo`. **All milestones (M0-M10) are built.** Next: the user's full test pass, then Docs/LAUNCH.md. **User brief 2026-10-07: finish every milestone,
do NOT test (no EditMode/PlayMode/nettest/screenshots); the user tests everything once the game is complete.**
Compile + rebuild + builds only; everything from M9-review onward is untested.

-> **UI overhaul** (2026-10-07, user-approved plan, R.E.P.O./Lethal Company style; all 10 steps built and pushed):
1 theme + kit (OFL fonts Barlow / Barlow Condensed / Saira Stencil One / IBM Plex Mono / VT323, Kenney CC0 icons, UiIcons,
key caps, bars, stamps, hazard tape), 2 in-run HUD (haul bar + clock, stamina, hand + pocket slots with rendered loot
pictures, value tags on what you look at / hold, the loot scan on Q, key-cap prompts), 3 death camera + YOU DIED with
the cause, ghost bar, truck-leaving banner, toasts, speaker marks over players, the haul board in the truck, 4 pause
menu + crew list with per-player voice volume/mute, 5 settings tabs (Video, mic picker + level, UI Scale), 6 main menu +
Play screen (solo / host / join), 7 appraisal receipt, 8 HQ CRT terminal + job sheets + wardrobe 3D preview, 9 travel
screen, 10 text chat (T). Steps 1-3 were screenshot-checked at 16:9 / 16:10 / ultrawide and their tests passed;
**steps 4-10 are compile-clean only (user: no tests, they test everything)**.

## Needs you (details per item in Docs/progress/M3.md)
- [ ] Real Steam test (App ID 480, both machines, Steam running): F1 shows "Steam: on <name>". Main menu ->
      Steam -> "Host a Steam lobby"; in the HQ press Esc -> "Invite friends" opens the overlay; invite the
      Windows friend. They accept (game running: joins at once; game closed: Steam launches it with
      +connect_lobby and it joins). Both Esc menus list both names under CREW; F1 shows the lobby id. Also try their
      "Join game" from your Steam friends list, a different build (clear version message), and a 5th
      person (full). With Steam quit, the menu says "Steam isn't running".
- [ ] Multiplayer Play Mode (host + 1–3 virtual players, Join 127.0.0.1:7777): spawn points, smooth
      remote movement, footsteps, ragdolls, F1 client ids/RTT; 30 Hz tick feel.
- [ ] Builds: open the Mac zip from `~/Documents/Abandoned-builds/dev/` (right-click > Open first time);
      Windows friend runs the Windows zip (SmartScreen: More info > Run anyway).
- [ ] Loot in MPPM: pickup/drop/throw/pocket replicate; two grabbers -> one wins; vase shatters for all;
      thrown safe knocks a client down; carried laptop striking resting loot damages + pushes it.
- [ ] Shared carry in MPPM: server rack 1/2 drag -> 2/2 lift; tether, crouch slows crew, sway; piano/
      statue need 3 (2 only nudge ~0.75 m); turning corners swings the rack. Judge lift/spring/tether feel.
- [ ] Structure in MPPM (host + 1–2 virtual players): stand on the weak tile upstairs (Tile_U_3_3) with
      the server rack; every screen shows the same cracks/sag, then it collapses for everyone at once and
      the players on it fall; F1 shows the same stage/health on host and clients. Re-roll (F2) restores it everywhere.
- [ ] Leaving in MPPM: a virtual player picks up the laptop and closes/stops; the host sees it drop where
      they stood. Then the host picks Esc -> "End game for everyone": every virtual player lands on the
      main menu saying "The host left the game." with an OK button, and nobody auto-hosts.
- [ ] Voice (M4.1) in MPPM or LAN (Direct IP uses the raw microphone): allow the mic when macOS asks.
      Hold V and talk: the other player hears you from your position, quieter with distance and silent
      past ~25 m; ((•)) shows over your head on their screen, "● TALKING" on yours. Esc -> Settings: Open mic,
      voices volume, mute. Judge delay (~120 ms buffer) and quality (16 kHz mu-law). Over Steam (two
      machines) the same with Steam voice. M4.2: talk from the next room (muffled + quieter); hold R
      anywhere in the building: the other player hears you band-limited with static wherever they are.
- [ ] Mall run (M5.1-5.3): Mall scene (Build profile: open Scenes/Mall, Play). Walk the mall: entrance,
      both escalators, service stairs, the atrium bridge; loot everywhere (F1 shows seed/count/value). Carry
      loot to the truck at the loading bay (east), watch "Haul $X / Quota $40,000" rise, press E on the
      yellow lever in the bay: 10 s of honking, then it leaves and the appraisal shows (items, damage, who
      made it out, funny stats); host presses "Next run": everyone is back in the parking lot with new loot
      in a restored building. Judge distances and the 15 min window. M5.5: 45 s in, the Blind One
      appears (listen for clicking): sprint and drop things near it and it hunts you; touch = death
      ("YOU DIED", body stays down); crouch-walking past it should be possible. F1 shows what it heard.
- [ ] Company loop (M6): the build opens in the HQ. Read the contract board (E), host takes a job, press E
      on the van: everyone drives to the mall on that contract's terms (Power Off = dark). Extract, see the
      payday (debt if the quota is missed), "Back to HQ". Quit and restart: money/level are kept
      (~/Library/Application Support/Zeyad Games/Abandoned/company.json). A friend can only join at the HQ.
- [ ] Look and listen (M7.1-7.3): walk the mall with sound on. Loot has Kenney models; stores have
      furniture against the walls; ceiling panels glow, a few flicker, dead ones are dark; fog, dust in
      the light, a sun shaft under the skylight; flashlight (F) casts shadows. Judge brightness (too dark /
      too bright?), footsteps and impacts per material, creak/groan/snap/crash, the wind and light hum
      (gone in a Power Off contract), grab/pocket/flashlight/lever/coin sounds. Tell me what's off.
- [ ] Menus (M7.4): a build opens on the main menu (title over the HQ). Play; Esc opens the menu (crew,
      settings, leave). Settings: sensitivity, FOV, head bob, shake, subtitles, colourblind scanner, sound
      sliders, voice; Keys...: rebind Interact to G and check the "[G] Pick up" prompt. Judge readability
      of the all-caps Kenney font and the layout at your screen size.
- [ ] Cosmetics (M7.5): at the HQ press E on the blue lockers (garage, west wall): pick a coverall and hat;
      a friend sees them. Unlocks come from playing (a few runs unlock the cap/green; cone at 5 runs).
- [ ] Performance (M7.6): in a 4-player session (or MPPM) press F1: the bottom line shows fps, batches,
      awake bodies. Note fps in the busiest spot (concourse/atrium, flashlights on) on your Mac and on the
      Windows friend's PC, and during a big collapse. Anything under ~50 fps, tell me where.
- [ ] Demo build (M8.3): unzip `Abandoned-*-MacDemo.zip`: the menu says DEMO; play the 5 jobs (or ask me to
      lower the limit for a quick check): back at the HQ after the 5th the end screen shows; Wishlist opens
      the Steam overlay (with Steam running) or the browser. When the real App ID exists, set
      Data/Core/Resources/DemoConfig StoreAppId.
- [ ] Steam partner setup (M9.5/9.6), once the real App ID exists: follow Docs/STEAM_SETUP.md (App IDs,
      Auto-Cloud paths, the 12 achievement API names, depots), then check an achievement unlocks in the Steam
      overlay and the company save appears on a second machine. Achievements already work locally.
- [ ] Trailer capture (M8.7): F10 hides the HUD (tips and captions too) for clean shots; F10 again brings it
      back. Tell me if you want a free-flying camera for the host as well.
- [ ] LAN: host on one machine, join with its LAN IP:7777.
- [ ] **Full test pass of M9 review + M10 (nothing since M9.6 has been run)**: first `Tools/unity.sh all` (some tests were
      updated to the new economy without running them) and the nettests; then play: the shop's THE TRUCK section (bay,
      floodlights, engine, armor), flatbed with the piano, rope & pulley over a collapsed hole (loot and people come down
      slowly), locked stores (E with bolt cutters / crowbar; Sealed job), motion detector + night vision, wardrobe
      accessories, the HQ answering machine, music + Music slider, payday running costs, crew-size quota on the board,
      F9 free camera, menu links (Open logs).
- [ ] **UI overhaul test pass**: `Tools/unity.sh all` (UiShotsTests renders every screen into Game/Screenshots/UI_*; steps
      4-10 have no shots yet: add them or look in the editor) + nettests. Then play: Q scan in the mall, value tags,
      pockets, the truck board, dying (camera, card, ghost bar), Esc (crew rows, per-player volume), Settings tabs (Video:
      window/resolution in a build; UI Scale; mic picker), main menu -> Play, the appraisal receipt, the HQ terminal
      (GEAR/TRUCK pages), the job sheets, the wardrobe preview, the drive screen, T chat (incl. a ghost's line).
- [x] Clean-up: the aborted test run's `InitTestScene...unity` and `Game/Assets/Resources/` leftovers are gone
      (checked 2026-10-08, QA B-36).
- [ ] Early Access launch: Docs/LAUNCH.md (App ID, store page from Docs/STORE_PAGE.md, trailer per Docs/TRAILER.md,
      Discord + feedback links in Data/Core/Resources/LaunchConfig, `build-release`, launch discount).

## Committing
Stage `Docs`, `Tools`, `CLAUDE.md`, `Game/Assets/_Project` AND `Game/Assets/DefaultNetworkPrefabs.asset`
(NGO's prefab list lives outside _Project; M5.2-5.5 missed it) and any `Game/ProjectSettings/*.asset`
the rebuild changed on purpose (ProjectSettings, EditorBuildSettings). Leave the churn listed at the end.

## How to verify (batch mode, Unity must be closed)
```
Tools/unity.sh compile      # zero errors, no warnings from Assets/_Project
Tools/unity.sh rebuild      # regenerates generated content (Player prefab, TestMap, loot, network prefabs)
Tools/unity.sh verify       # Verify Project Setup + Content Validator
Tools/unity.sh editmode     # -> Game/TestResults/EditMode.xml
Tools/unity.sh playmode     # -> Game/TestResults/PlayMode.xml
Tools/unity.sh screenshots  # PNGs -> Game/Screenshots/ (gitignored); look at them
Tools/unity.sh all          # everything above in order
Tools/unity.sh build-dev    # dev Mac player -> Game/Builds/MacDev (used by nettest)
Tools/unity.sh build        # shareable Mac + Windows zips in ~/Documents/Abandoned-builds/dev/
Tools/unity.sh nettest [scenario] [--clients N] [--timeout S] [--rebuild|--no-build]
```
Zips: `Abandoned-<version>-<commit>-<Mac|Windows>.zip`; build them only after the task's commit.
Nettest results + logs: `Game/Logs/nettest/<host|clientN>.{json,log}`; batch logs `Game/Logs/batch/<step>.log`.
The script fails on compile errors, warnings in our code, or any exception in a log.

## Open problems
- M9.1: one `git push` failed at GitHub's LFS lock check ("does not support the Git LFS locking API");
  it went through on a later retry. If it recurs and persists, ask before touching lfs.locksverify.
- `nettest loot` right after `basic` failed twice (M3.8, M6.0): sessions ended while waiting; never
  reproduced on demand. Recurred M7.2 (`company` right after `run`): one client "Failed to connect to
  server", no stale process left, passed on rerun. Recurred M8 (`loot` 0/4 right after `basic`); basic+loot
  passed on rerun. Its logs were lost because nettest.sh kept failed runs INSIDE Game/Logs/nettest, which
  the next run wipes: failed runs now go to Game/Logs/nettest-failed/<scenario>-<time>/ (look there).  If it recurs, check host.log for the listen port
  and whether a previous run's process was still alive.
- M3.5: one full PlayMode run failed `NetworkLootHitTests.HostThrownSafeKnocksDownTheClientPlayerOnTheClient`
  (client never ragdolled within 4 s); it passed alone 3x and in two further full runs. The safe now has
  SharedCarryable (idle when nobody holds it). If it recurs, look at that test's timing first.
- One `Tools/unity.sh all` run printed no PlayMode results line; rerun passed 103/103 and a second full
  `all` passed. Possible flake; if it recurs, check Game/Logs/batch/PlayMode.log for a crash.

## Unity-generated churn left uncommitted on purpose
(PC_RPAsset.asset is committed since M7.6: the rebuild sets its shadow budget on purpose.)
DefaultVolumeProfile.asset, UniversalRenderPipelineGlobalSettings.asset (player builds
fill its runtime-settings list), probuilder Settings.json,
ProjectSettings/Packages/com.unity.multiplayer.tools/, ProjectSettings/SceneTemplateSettings.json.
