# Autonomous build, 0.16.0 to 0.24.6 (started 2026-10-10)

User brief (2026-10-10): build every roadmap version from 0.16.0 to 0.24.6, including the 0.22 UI revamp,
without stopping and without testing; make Mac + Windows builds at the end of each version (zipped into
`~/Documents/Abandoned-builds/dev/`). Blockers go to ntfy.sh (topic in the session log). Everything below
compiles and was authored in batch mode; **nothing was played**. Each version is one commit.

How a version is made: runtime code under Scripts/<Area>/, tunables as a ScriptableObject in a Resources
folder (code defaults until the asset exists), an editor step `Editor/Releases/Release<version>.cs` with a
menu item under Tools/Abandoned/... and a batch entry `AuthorAndExit`, run by
`Tools/unity.sh exec Abandoned.EditorTools.Release<version>.AuthorAndExit`. Steps add to the saved scenes
(they never regenerate the mall or HQ). Mall placements are found by `Editor/Mall/MallSites` (free walls and
floors, checked like the 0.16.0 lockers). Run-long networked state rides on the RunState prefab
(`NetworkContentBuilder.RunStateExtras`, append only). Sounds added from 0.16.1 are synthesised
(`Audio/Synth/ExtraPatches` dispatches them). One-time tips for new usables: `UI/Hints/IHintSource`.

## 0.16.1 breaker rooms
One breaker cabinet per horror power sector (6), on a bare wall inside the sector, back-of-house first.
E throws it (host-checked, `RunBreakers` on RunState): that sector's ceiling lights come back for 120 s
through danger blackouts and power-off contracts alike; the throw is heard 40 m away and the panel hums
(noise 0.4 every 6 s) while lit, then 45 s cool-down. Tunables: Data/Extraction/Resources/BreakerConfig.
Menu: Tools/Abandoned/Mall/Add Breaker Rooms (0.16.1).

## 0.16.2 seeded route changes
Locked stores (RunShutters) and doors shut at the start (RunDoors) already differed per job. New: `RunRoutes`
(RunState) sends 2–5 collapsible floor sections, at least 9 m apart, down 3 s into each job through the
normal structure path (synced, rubble mounds, NavMesh carving), and with 40 % chance one of the two
escalators (never both). Stairs, protected routes and the ground floor can't collapse, so stores and the
truck stay reachable. Tunables: Data/Extraction/Resources/RouteConfig.

## 0.16.3 freight lift
An open goods platform in the atrium's north-west void (tile 5,7, x 20–24 z 28–32), ground floor to the upper
gallery, entered from the west on both levels. Buttons: one at each landing (call) and one on the deck (send).
`RunLift` (RunState) holds the trip (from, to, start time, snap time); every machine places the platform from
it and the server clock, so it is in the same place everywhere. Players ride it through the new
`Core.MovingPlatform` (PlayerMotor adds the platform's frame move when grounded on it); loot rides the deck.
Needs power: the mains in its sector (1) or that sector's breaker. 14 s a trip, motor noise every 1.5 s.
Overloaded (gameplay weight on the deck above 450 kg when sent) it gives way halfway, free-falls to the
bottom, hurts riders 35 and stays broken for the job. Gates at both landings are solid (and carve the
NavMesh) unless the platform is parked there; a cage closes the ground level's other three sides.
Switched off (not deleted) to make room: the planter, bin and bench in that tile, one industrial dumpster
prop, and balustrade `Rail_1_4_7_(1, 0)`. Watch on the test: riding jitter for remote players (they follow
their owner's NetworkTransform), and whether the upper landing step (0.15 m gap) feels right.
Tunables: Data/Extraction/Resources/LiftConfig. Menu: Tools/Abandoned/Mall/Add Freight Lift (0.16.3).

## 0.16.4 vents
Six ducts, each a pair of floor-level grilles (found by MallSites): Basement↔Titan Tools, Cinema↔Apex
Circuit, Closed Offices↔rear promenade, Nordic Habitat↔Toy Archive, Titan Tools↔Toy Archive, Toy
Archive↔atrium. Crouch + E with nothing in your hands (pockets are fine) crawls through: the host writes
`NetworkPlayer.VentBase + duct` into the hiding value, so the player counts as hidden and **no** monster
follows them in (`Threat.HiddenFromMe`), whatever it saw. The duct is not modelled: the owner's player is
parked 60 m under the exit for the crawl (black screen with a countdown), then stands at the far grille.
Crawl time = grille distance / 1.4 m/s (at least 4 s). The duct rattles (noise 0.3 every 2 s at its middle).
Tunables: Data/Extraction/Resources/VentConfig. Menu: Tools/Abandoned/Mall/Add Vents (0.16.4).

## 0.16.5 store alarms
An alarm box on a wall inside each of the 8 stores, nearest its shopfront. `RunAlarms` (RunState) wires
each store with 35 % chance per job (by shutter index, seeded), arming 5 s into the job. The first living
player inside a wired store trips it once: an electric bell rings for 30 s (noise 1.0 every 2.5 s), the lamp
flashes. Bolt cutters in a hand slot + E at the box disarm it, before or while it rings. Armed boxes blink
red every 2 s; a disarmed or unwired box is dark. Tunables: Data/Extraction/Resources/AlarmConfig.
Menu: Tools/Abandoned/Mall/Add Store Alarms (0.16.5).

## 0.16.6 vault drill
The mall has no jewellery store (zone "Jewelry" is Nordic Habitat's furniture floor) and its ground-floor
stores had no bare wall big enough, so the safe stands in the **Cinema** (upper floor, west wall, x 0.15
z 14): a box-office safe. E sets a core drill on it: 90 s of drilling, noise 1.0 every 2 s, and every 12 s a
40 % chance it jams until someone presses E again (`RunVault` on RunState). Through, the door swings open on
4–6 of the most valuable one-hand items in the catalogue at 1.6× value, spawned by the host through the new
`NetworkLootSpawner.SpawnExtra` (cleared with the rest at the next re-roll). Tunables:
Data/Extraction/Resources/VaultConfig. Menu: Tools/Abandoned/Mall/Add Vault Safe (0.16.6).

## 0.16.7 security office
A monitor desk in the upper floor's closed offices and eight wall cameras (one per space where possible,
10 m apart). E at the desk: the local screen becomes one camera's feed in CRT green with its name and a
clock; A/D or click switches, E/jump/pause gets up. The viewer stands at the desk with controls off.
Only the watched camera renders, only on the watcher's machine (640×360, no shadows or post); nothing is
networked. Needs power in the desk's sector or its breaker. Labels are screen UI, not world text.
Menu: Tools/Abandoned/Mall/Add Security Office (0.16.7).

## 0.17.0 sledgehammer
New gear (catalog index after the battery; $3,000, company level 2; tool, never used up). Use swings at
what you aim at: 5,200 kg·m/s into a collapsible floor section (the crowbar is 1,500), 1.3 s apart, noise
0.9. A sound floor breaks in a few blows through its normal Cracking warning; protected routes and stairs
can't collapse and say so. Gear added from here on: `EquipmentKind` appends, `PlayerEquipment.Tools.cs`
(host uses), `ToolConfig` (Data/Equipment/Resources), `GearPrefabs` (prefabs gear spawns, registered with
NGO by `RegisterNetworkPrefabs`), icons drawn by UiGearIconBuilder. Menu: Tools/Abandoned/Content/Sledgehammer (0.17.0).

## 0.17.1 crash mat
New gear ($1,800, level 3, single use): use lays a 2.2 × 2.2 m pad flat on the floor 1.6 m ahead (a host-
spawned static network object that lasts the job). A soft zone 0.9 m deep above it (`Core.ImpactCushion`)
keeps 12 % of the speed of any loot impact in it, so dropped loot survives, and cuts a player's fall height
to 1.4 % (height goes with speed squared). Applied in `LootItem.ApplyImpact` and the host's landing damage.
Prefab: Prefabs/Equipment/CrashMat (GearPrefabs). Menu: Tools/Abandoned/Content/Crash Mat (0.17.1).

## 0.17.2 floor types
`StructuralSection.FloorKind` (Concrete, Glass, Timber, Tile) scales its capacity and health
(`FloorKindConfig`, Data/Structure/Resources: glass 0.45 / 0.5, timber 0.75 / 0.8, tile 0.9 / 1.0). Set on the
saved mall: the upper gallery's bridge is glass (drawn with the salvage glass material), the dining terrace
is tile, the cinema and toy archive upstairs are timber; the rest stays concrete. Non-concrete floors give
one warning of their own when they start Cracking (glass ticks and spiders, tiles pop, boards split) and
fail with their own sound in place of the concrete snap. All are Structure-bus floor sounds that walls never
hide; none is a creak. Menu: Tools/Abandoned/Mall/Set Floor Types (0.17.2).

## 0.17.3 noisy ground
24 patches (seeded, 6 m apart, on free floor found by MallSites): broken glass mostly in stores, puddles mostly
in the food court and basement, loose tiles anywhere. Every machine plays a crunch / clack / splash per 0.9 m
walked through a patch for any player (from synced positions); the host turns each step into monster noise
(glass 0.55, tiles 0.4, puddle 0.3; crouching 15 % of that). Art is flat and walk-through, merged per patch,
bound to its floor section. Tunables: Data/Extraction/Resources/GroundConfig. Menu: Tools/Abandoned/Mall/Add
Noisy Ground (0.17.3).

## 0.17.4 aftershocks
`RunAftershocks` (RunState): from 4 min into a job, every 4–7 min (seeded), an 8 s warning (a deep rumble
twice, light flicker, the radio line "TREMOR / brace yourselves"), then the shock: a heavy thud and big camera
shake for everyone, and 4–8 random collapsible sections lose 35 % health (never below 15 %) through
`StructureSimulation.AgeRandomSections`, so they crack and warn but only weight brings them down.
Tunables: Data/Extraction/Resources/AftershockConfig. Menu: Tools/Abandoned/Content/Aftershocks (0.17.4).

## 0.17.5 live water
Three 3.4 m flooded areas in the basement (the low ground). Every flood and every 0.17.3 puddle is water,
and water is live while its power sector has power (`Extraction.SectorPower.Live`: its breaker is thrown, or
the mains are on and the danger hasn't cut the sector; the lift and the security desk now use the same
check). Live water buzzes and spits (heard by everyone nearby); standing in it zaps each 0.5 s and the
host deals 22 HP/s ("Electrocuted in live water"). So a thrown breaker lights a sector and arms its water.
Tunables: GroundConfig. Menu: Tools/Abandoned/Mall/Add Flooded Areas (0.17.5).
(Changed in the 0.17.6 commit: only the flooded areas are live, not the small 0.17.3 puddles; with the mains
on most jobs, live puddles everywhere would have made the whole building a hazard from the first second.)

## 0.17.6 spray chalk
New gear ($400, level 1, never runs out). Hold use: every 0.06 s the owner sends its eye ray, the host
raycasts it (3.5 m) and adds a dot to `RunChalk` (RunState; a NetworkList, newest 800 kept, cleared each run).
Every machine draws all dots as one mesh of 11 cm quads in the sprayer's crew-seat colour (orange, green,
blue, pink) with an unlit Particles material (Art/Greybox/ChalkMarks.mat), so marks read in the dark. A
hiss plays as dots appear. Marks on a section that falls stay where they were (known gap).
Menu: Tools/Abandoned/Content/Spray Chalk (0.17.6).

## 0.17.7 glow sticks and flares
Two consumables: glow stick ($150, level 1: dim green point light, range 6 m, 10 min, silent) and flare ($350,
level 2: red, range 14 m, 90 s, flickers, hisses, noise 0.2 every 3 s). Use throws one like the noise maker
(host-simulated, NetworkTransform). No shadows, so the 0.14.0 shadow atlas is untouched; the light reaches
every storey layer (`LevelStoreys.LightEverything`). Prefabs: Prefabs/Equipment/GlowStick, Flare (GearPrefabs).
Menu: Tools/Abandoned/Content/Glow Sticks and Flares (0.17.7).

## 0.18.0 boss requests
New money plumbing for all of 0.18: `RunState.Settling` (host, as the truck leaves, not for a wiped crew) lets
each bonus add pay and an appraisal line from what is aboard (bay, held, and the pockets of those who made
it); `RunResults.Extra` carries the pay and `CompanyLedger.Apply` adds it on top of the payout, quota met or
not (it never counts toward quota). Lines go at the top of the appraisal's stats. `RunGoals` lines show under
the haul on the scan HUD. Boss request (`RunRequests` on RunState): 6 s in, the boss names one of the Huge
(else Heavy, else two-hand) items actually spawned this job, seeded; a toast says "Bring me the X". Any
unshattered item of that kind aboard pays max($1,500, 80 % of its value). Tunables: BonusConfig.

## 0.18.1 rush orders
`RunRush` (RunState): 5 min in, then every 6–9 min after the last one ends, dispatch calls a rush on one kind of
item still out in the building (seeded): toast, radio line, and a HUD goal with a countdown. For 3 min the
host notes each item of that kind that lands in the truck bay; those still aboard and whole at departure pay
×2 (the extra value on top, through `RunState.Settling`). Tunables: BonusConfig.

## 0.18.2 loot quirks
`LootDefinition.Quirks` (flags: Rattles, Leaks, Rings, TopHeavy); `Loot.LootQuirks` (added at run time by
LootItem for any quirky definition, so no prefab changes) does the rest on every machine: rattles while
moving faster than 0.6 m/s (noise 0.25), leaks while carried (drips; the host drains ~0.2 % of full value per
second via `LootItem.Drain`), rings for 4 s at a seeded moment in most 75 s windows (computed from the item
seed and server clock, so every machine agrees; noise 0.6), top-heavy (centre of mass raised to 35 % of its
height, so it tips). 27 existing items given quirks (clocks/phones/radios ring, fridges/washers/bathtubs leak,
tool chests/cartons rattle, lamps/statues/racks top-heavy). Menu: Tools/Abandoned/Content/Loot Quirks (0.18.2).

## 0.18.3 loot sets
`Loot.LootSet` assets (Data/Loot/Sets) listed in `LootSetCatalog` (Data/Loot/Resources): Home Office, Kitchen
Counter, Gallery Wall, Jewellery Box, Living Room, Checkout Lane, $1,400–3,000 each. `RunSets` (RunState) pays
each set with one whole item of every member aboard at departure, and the scan HUD shows the set closest to
done in the bay ("SET: Kitchen Counter 2/4"). A new set is a new asset. Menu: Tools/Abandoned/Content/Loot Sets (0.18.3).

## 0.18.4 shopping carts
The cart is loot data: `shopping_cart` (Utility: worth nothing, never planned; Heavy, 18 kg; `Wheeled`; Rattles
quirk). `LootDefinition.Wheeled` lets anyone drag it alone without a trolley (PlayerCarrier.Holds and
SharedCarryable). Its prefab (generated, then the box collider swapped for a basket: floor, four sides, a
stand) is host-simulated while dragged, as is anything resting in its basket, so small loot rides along and
rattles; in the bay, what's in the basket counts like anything else there. `RunCarts` (RunState) stands one
cart at each of 5 `CartBay` markers on the ground floor's concourse and food court 2 s into each job (through
`NetworkLootSpawner.SpawnExtra`, so a re-roll clears them). Menu: Tools/Abandoned/Mall/Add Shopping Carts (0.18.4).
Watch on the test: whether loot stays in the basket at drag speed, and how the drag hold point feels.

## 0.18.5 moving blanket
New consumable ($600, level 2): look at a loot item (2.6 m) and use. The host sets a new `NetworkLoot.Wrapped`
flag (NetworkLoot.Blanket.cs); every machine drapes a quilt box over it and sets `LootItem.Cushion` to 0.4, so
its impacts keep 40 % of their speed for the rest of the job (with a crash mat too, the two multiply).
Menu: Tools/Abandoned/Content/Moving Blanket (0.18.5).

## 0.18.6 overtime
`RunOvertime` (RunState): the server time the haul first meets quota is kept (toast "QUOTA MET... overtime");
from then the multiplier on everything over quota climbs 0.1 a minute up to ×2, shown on the scan HUD, and
paid at departure through `RunState.Settling` on the haul at the moment of leaving. The building's decay
clock keeps running, so staying is the risk. Tunables: BonusConfig.

## 0.18.7 loot catalogue and trophy shelf
`CompanySave.catalogue` lists every kind of loot the company has sold (`RunResults.Item.Id` is new); the host
adds to it at payday (not for a wiped crew) and mirrors it on `CompanyService` (CompanyService.Catalogue.cs).
At the HQ (added to the saved scene by `HqSites`, never regenerated): a lectern with the catalogue ledger (E:
`UI/LootCatalogueScreen`, in the field guide's style: found kinds with picture and price range, jackpots
starred, the rest blank) and a three-board trophy shelf with 12 places that shows a small copy of every
jackpot sold, best first (`Company.TrophyShelf`). New screens from here on live on `UI/ExtraScreens`, created
at start-up (no session-prefab change). Menu: Tools/Abandoned/HQ/Add Loot Catalogue and Trophy Shelf (0.18.7).

## 0.19.0 ping marker
New Gameplay action `Ping` (middle mouse, rebindable) in AbandonedInput.inputactions, read by name in
PlayerInputReader (`PingPressed`). `UI/PingView` (on ExtraScreens) sends the local camera ray to `RunPings`
(RunState); the host raycasts 60 m and marks LOOT (follows the item), MONSTER! (follows it), WEAK FLOOR (a
collapsible section past Stable) or HERE, for 12 s, max 3 per player (a 4th replaces the oldest). Every machine
draws a diamond with the label and distance in the pinger's seat colour, fading out; a blip plays.

## 0.19.1 replacement worker
An agency radio beside the truck's ignition (`HireRadio`). With a crewmate dead, a living player there presses E:
`RunHires` (RunState) respawns the crewmate who has been dead longest in the truck bay (`ServerRespawn`), up
to 3 hires a job, and adds a fee of max($1,200, 12 % of the haul at that moment). Fees come off the payout at
departure as negative lines through `RunState.Settling` (CompanyLedger now takes a negative extra). Respawn,
not join. Tunables: BonusConfig. Menu: Tools/Abandoned/Mall/Add Agency Radio (0.19.1).

## 0.19.2 ghost knock, flicker and nudge
While spectating, a dead player's E knocks (a soft triple rap on wood, deliberately unlike the Stalker's
knock), F makes every light within 8 m of where the ghost camera looks stutter for 1.6 s
(`LightFixture.DisturbHere`), jump nudges the nearest loose small item (pocket/one-hand, not held) within 1.6 m.
`RunGhosts` (RunState) checks the sender is dead, the spot is within 30 m of a living player, and 3.5 s between
acts. No monster noise from any of it. `UI/GhostActions` (ExtraScreens) reads the keys and shows the help line.

## 0.19.3 post-run blame screen
`RunResults.Blame` (new, serialized) holds one line per player; `RunStats.Blame` (wired as
`RunState.BlameSource` by RunDirector) gives each the first title that fits: Employee of the Month (most value
they last held that is aboard), Demolition Specialist (most broken), Gravity Tester (longest fall), else
Deceased (unpaid), Missing, presumed lazy, or Present. The appraisal shows them as "WHO DID WHAT" above the
employee awards. No editor step.

## 0.19.4 host difficulty presets
`Core.Difficulty` Relaxed / Standard / Brutal in `CompanySave.difficulty`, mirrored by CompanyService; a
three-way switch on an HQ wall near dispatch (only the host can turn it). `DifficultyConfig`
(Data/Core/Resources) scales quota ×0.8/1/1.25 and building decay ×0.7/1/1.4 (RunDirector's terms), monster
damage ×0.7/1/1.3 and the gap between arrivals ×1.4/1/0.75 (ThreatDirector). Known gap: the contract board
still prints the unscaled quota; the HUD, truck and payday use the scaled one.
Menu: Tools/Abandoned/HQ/Add Difficulty Switch (0.19.4).

## 0.19.5 emotes
Three new rebindable Gameplay actions, EmotePoint / EmoteWave / EmotePanic on 3 / 4 / 5. The owner writes the
emote and its start time into two owner-written NetworkVariables on NetworkPlayer (NetworkPlayer.Emote.cs);
every machine's `WorkerPresentation` lays a 2.6 s procedural pose over the gait (point: right arm forward;
wave: right arm up and waving; panic: both arms flailing, torso twisting), only with empty hands. Your own
body is hidden in first person, so you see crewmates' emotes, not yours. No editor step.

## 0.19.6 HQ break-room toys
A basketball and darts are zero-value utility loot (one-hand, never planned): carried and thrown like
anything else. In the saved HQ's crew wing: a wall hoop (rim 3.05 m) whose net trigger scores a ball dropping
through it, and a dartboard (bull 50, rings 25/15/10/5; a scoring dart sticks for 3 s) over a crate holding
three darts. `Company.HqToys` (host) stands the toys at their places, puts back any that are gone every 5 s,
and remembers who last held each; scores go to everyone as a toast with a jingle (`CompanyService.Toys`).
Menu: Tools/Abandoned/HQ/Add Break-Room Toys (0.19.6).

## 0.19.7 arachnophobia and streamer modes
Settings > Accessibility: "Arachnophobia mode" (the Crawlers' renderers are hidden on this machine and six
bobbing grit lumps scurry in their place; behaviour unchanged; `Threats.ArachnophobiaSwap`, added by
CrawlerSwarm) and a PRIVACY section with "Streamer mode" (`NetworkPlayer.DisplayName` reads "Crewmate N" for
everyone but you, the pause menu hides your LAN port, the join field on the Play screen is masked). New
settings live in `Core/GameSettings.Extra.cs` (GameSettings is now partial). No editor step.

## 0.19.8 controller, first tier
A Gamepad control scheme and pad bindings in AbandonedInput.inputactions (Xbox names): left stick move, right
stick look (new `LookPad` action; the reader turns tilt into a turn rate, curved, `GameSettings.PadLookSpeed`
900), A jump, B crouch, X interact, Y scan, RT use, LT drop/throw, RB talk, LB radio, L3 sprint, R3 ping, d-pad
up flashlight, left/right hand slots, down inventory, Menu pause, View give gear (hold View: rotate item).
Keyboard-only on a pad for now: chat, photo camera, hide HUD, emotes. `Core.PadInput` notes the device used
last; HUD prompts (`InputBindings.Display`) then show pad buttons as text ("X", "RT"). The eight screens that
closed on a hard-coded Escape now use `PadInput.BackPressed` (Esc or pad B; the menu also takes Menu).
Menu navigation with a pad is 0.23.0. Glyph images were not added (text names; a glyph pack needs approval).

## 0.19.9 text table and font coverage
`Core.TextTable` reads player text from Resources/Text/<language>.json (Data/Text/Resources/Text/en.json),
key → text, with "{key:Interact}" filled in with the player's own key; code keeps its English as the
fallback, so nothing ever shows a raw key. Moved so far: all 28 first-time tips (`hint.<HintId>`); tips added
later read from code until re-exported. Not yet moved: menus, HUD, prompts, toasts (planned with 0.22's UI).
Fonts: Noto Sans (Latin/Greek/Cyrillic), Noto Sans Arabic, Noto Sans SC (Chinese + kana) and Noto Sans KR
(OFL, downloaded from google/fonts, credited in Docs/ASSET_CREDITS.md) as dynamic font assets
(Data/UI/Fonts), set as global fallbacks on the menu panel's new PanelText settings: names and chat in those
scripts now draw. Arabic letters may not join/flow right-to-left without UI Toolkit's advanced text generator
(check on the test). Builds grow ~30 MB (the CJK fonts).

## 0.19.10 more achievements and a stats page
Ten more achievements (22 in all), appended to the catalog with Steam names ACH_<ID>: Let There Be Light
(breaker), Safecracker (vault), Going Up (10 lift rides), Ductwork (5 vents), Matching Set, Teacher's Pet (3 boss
requests), Express Delivery (rush order), Under New Management (come back as a new hire), Look Over There
(100 pings), Workaholic (overtime). Counted on each player's own machine (`UI/AchievementWatcher`, from what
they did and the departure's bonus lines). The achievements screen lists every stat under STATS.
Steam's partner site needs the 10 new API names (0.24.6).

## 0.19.11 rich presence
`Networking.SteamPresence` (made at start-up, off in batch mode) sets Steam rich presence every 5 s while Steam
runs: "status" (plain text) and "steam_display" #Status_Menu / #Status_HQ (crew size) / #Status_Job (haul of
quota), plus "steam_player_group" = the crew's Steam lobby so friends see the crew together. Needs the game's
own App ID: upload Tools/Steam/rich_presence_english.vdf on the partner site (Rich Presence localization).
Under App ID 480 Steam shows nothing; untested against Steam.

## 0.19.12 daily seeded job with leaderboard
The contract board gains a last offer every day: "Meridian Mall (today's daily job)" (`ContractGenerator.Daily`),
rolled from the UTC date at company level 1 with no modifier, so its seed, quota, stability, loot, route
changes and alarms are the same for every company that day (the host's difficulty preset still scales its
quota). When a daily job's truck leaves with someone alive, each player submits the crew's haul to Steam
leaderboard `daily_YYYYMMDD` (descending, numeric, best kept; `Networking.DailyLeaderboard`) and gets a toast
with the crew's global rank; the best haul is also kept locally. Needs the game's own App ID (480's
leaderboards may refuse it). Untested against Steam.

## 0.20.0 portable floodlight
New consumable ($1,400, level 2): use stands a tripod work lamp a metre ahead, facing your way (host-spawned
static network object, Prefabs/Equipment/Floodlight). A 95° spot, range 22 m, intensity 40, for 240 s; the
last 15 s it sags and stutters, then it's gone. Only the oldest standing floodlight casts shadows
(ToolConfig.FloodlightShadows = 1; one map in the atlas, which ShadowDirector already counts as a light it
doesn't manage), the rest light without shadows. Reaches every storey layer.

## 0.20.1 lockpick
New tool ($1,800, level 2): with it in a hand slot (and no bolt cutters), E on a locked shutter picks the
padlock in 14 s (ToolConfig.LockpickSeconds) with almost no noise (0.05) and a faint ticking, through
RunShutters' existing timed opening. Bolt cutters stay quicker (1.6 s, a little louder); the crowbar stays fast
and loud.

## 0.20.2 flashlight recharge at the truck
Already in the game since 0.12.6 (QA D-01: standing in the truck bay tops the battery up, FlashlightConfig.
TruckRechargePerSecond), but invisible. Now `UI/ChargeIndicator` shows "CHARGING FLASHLIGHT 64%" under the
crosshair while yours charges there, with a soft tick as it fills.

## 0.20.3 truck winch
A truck upgrade (catalog append: "Truck Winch", $18,000, company level 4, `TruckUpgradeKind.Winch`). A cable drum
at the cargo bay's open end; with the upgrade owned, E there hooks the nearest Heavy or Huge item lying within
30 m of the ramp's foot (not held, not already aboard). `RunWinch` (RunState) then reels it on the host at
1.3 m/s, gravity off, first to the ramp's foot and then up into the bay; it lets go once aboard, if anyone grabs
it, or after 5 s without progress. Everyone sees the cable (LineRenderer); the motor grinds (noise 0.5 a second).
Menu: Tools/Abandoned/Mall/Add Truck Winch (0.20.3). Watch on the test: the reel's path round obstacles.

## 0.20.4 thermal camera
New tool ($12,000, level 5). Owner-local like night vision (`Equipment.ThermalCamera`, on ExtraScreens, so no
player-prefab change): in hand, use toggles it. The view goes cold blue and dark (a global URP volume), and every
monster and crewmate within 45 m with a clear line of sight gets an orange-white heat blob over its outline,
however dark or foggy; walls still hide them. 60 s of battery, 0.3 s back per second while off.
