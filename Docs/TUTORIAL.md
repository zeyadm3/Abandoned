# The training job: 14 Larch Court (tutorial)

**Status (2026-10-20): written in full, never compiled, built, run or tested.** 2026-10-21: the user built and played
it ("it's just letters in the top corner") and asked for real guidance; see **Guidance** below (written, not compiled
or run; the voices were generated). The user asked for everything to be
built with no checking ("build it all, don't stop to commit or check your work or verify, I'll do all the testing").
Nothing was committed. The scene does not exist yet: it is made by **Tools > Abandoned > Maps > Tutorial House >
Rebuild Tutorial House** (which also writes the lesson data). Until then `BuildScriptTests`/`ProjectSetupVerifier` will report
`Assets/_Project/Scenes/Tutorial.unity` missing, because the scene is now in `BuildScenes.All`.

Plan agreed with the user before building: `~/.claude/plans/need-you-to-plan-lexical-fern.md` (the plan file of the
2026-10-20 session).

## What the user asked for, and the answers

| Question | Answer |
|---|---|
| What is it | An interactive prologue that walks the player through the whole game, in its own map. |
| The map | A one-story house in an ordinary neighbourhood, Michigan, 3 PM, December. |
| Rooms | The user's floor plan (image, 2026-10-20): three bedrooms, kitchen, dining, living room. |
| Bathrooms | **Two, as drawn** (the user chose the drawing over the brief's three): the hall bath between the two small bedrooms, and the master's ensuite, reachable only through the master bedroom. |
| Size | **1.4x the drawing** (the plan's ~3 x 3 m rooms are too tight for Heavy loot, the trolley and shared carries). |
| Defaults the plan proposed (not overruled) | A basement under the west wing for the collapse lesson; no garage; one scripted monster (the Blind One); nothing banked; suggested to new players, never forced. |

## The house

`Editor/Tutorial/TutorialHouseLayout.cs` holds every coordinate (metres, +X east, +Z north, house floor y = 0, the
yard 12 cm lower); `TutorialHouseFittings.cs` holds where the furniture that carries salvage stands, shared by the
rooms and the salvage spots.

**Redone 2026-10-21** (the user, after playing it: "it's so tight and you get stuck everywhere you walk", "the arrows on
the floor are not accurate"). The house is about 1.6x the first build (27 x 17.5 m): every doorway is 1.8 m wide
and 2.3 m tall, the hallway is 2.4 m wide, ceilings are 3 m, interior doorways are open cased frames (no hinged leaf
standing out into a room), the front door's leaves lie flat against the wall, and the furniture is sparse and
against the walls, with 2 m or more along the hauling routes. The cause of the bad arrows: the NavMesh agent is 0.5 m in
radius, so the old 1-1.1 m doors left no walkable gap; the route stopped short and the arrows were drawn straight through
walls. Now the doors pass the NavMesh, furniture, crates, closets and the yard's trees and fences are baked as "Not
Walkable" (the agent climbs 0.75 m, so a bed top used to count as floor), and the arrows never draw a straight stretch
through a wall.

- **West wing (8 x 17.5 m):** Bedroom B (south, with the rotten patch), the hall bath and the 2.4 m hallway (the weak
  floor) in the middle, Bedroom A (north) with the grandfather clock on its west wall. Each bedroom has a closet on its
  east wall. The hallway opens into the kitchen on its east side.
- **The band (18.5 x 7.5 m):** kitchen (counters on the north and west walls, an island well clear), dining (the table
  in the middle, the sideboard on the north wall), living room (sofa on the north wall, the TV on the
  south wall, the front door in the east wall). The route from the hallway to the front door along the band's south half
  is 4 m wide.
- **The suite:** the laundry alcove off the dining room (bypass doors 2.9 m wide), the ensuite (only through the
  master), the master bedroom (bed on the east wall, the hiding closet on the north wall).
- **The patio** between the wings, with the cellar stairs (bulkhead) at its west side. The **patio slider** is now well
  east of the stairs and opens on its **east half** (the sliding pane goes behind the fixed west pane): the user asked
  for it to slide the other way, as it used to open right in front of the stairs.
- **Basement** under the west wing and a strip under the patio and kitchen (2.4 m down): furnace and breaker on the
  west wall, water heater, shelving, a workbench in the strip. Kept empty under the rotten patch (where you land) and
  under the hallway.
- **Two teaching floors** (`Editor/Tutorial/TutorialHouseStructure.cs`): the **hallway** is weak timber (about 112 kg
  capacity: a person alone is fine, a person and the grandfather clock overload it); it creaks and cracks but its health
  never falls below 20 % (`StructuralSection.neverBelow`), so it never gives way. The **middle of the south bedroom** falls when the lesson arms it and someone stands on it, 2.4 m into
  the basement (under the 2.5 m safe-fall height). Every other floor can never collapse.
- **The yard** (`TutorialNeighbourhood`, the "Yard" group, in the NavMesh): the driveway to Larch Court with the truck
  on it and the kit crates by its ramp, a board fence round the back yard, trees, the mailbox and bins, a snowman, the
  crew's van at the kerb. The crew spawns on the front lawn by the street.
- **The town** (`TutorialTown`, the "Neighbourhood" group, outside the NavMesh): the user asked for "more homes and
  roads ... just have them there so the player feels there is a world", and players could walk off the old map. A
  grid of plowed streets (Larch Court in front, others every 110 m east-west and 160 m north-south), out to about
  200 m where the fog takes them, and woods beyond. The crew can walk Larch Court and the streets round its block
  (about 260 x 215 m); a board fence with an invisible wall behind it closes that area.
- **The street's houses are copies of the training house** (same day, the user: "copy paste the house and make the
  surrounding houses the same as the tutorial house with some variations ... lock everything so the player can't walk
  inside, remove the basement and basement stairs"). `TutorialStreetHouseKit` builds 14 Larch Court's outside from the
  same code (`TutorialHouseWalls.OuterWalls`, `TutorialHouseRoof.Gables`), with the front door and the patio slider
  shut, the patio whole (no bulkhead, no basement, no window wells or furnace flue) and only a dark room behind the
  blinds, and merges it into saved meshes (`Art/Tutorial/Meshes/Street/`): a near version with everything and a far
  one without laps, casings, blinds or icicles (an LODGroup per house switches at about 90 m). Each lot rolls its own
  look (`TutorialStreetHouse.Roll`): mirrored or not, one of 8 sidings, 4 roofs, 6 front doors, 3 trims, a garage
  beside the band (55 %), a gabled hood over the stoop (45 %), a chimney in one of three places or none, a wreath,
  shrubs; each has a drive and a front walk to its street. Lots are 30 m along the street. Houses the crew can reach
  (near the fence) get one solid block per wing, the garage and the stoop, so nobody gets in; the rest have no
  colliders. The copies are never static-batched (they share their meshes).
- **Corner boards** (same day): every outside corner of the house now has a trim board over the notch where the two
  siding skins met (`TutorialHouseWalls.CornerBoards`); the street copies and their garages get them from the same code.
- **Light:** 3 PM in December, the low sun in the south-west behind overcast (`TutorialHouseLighting.December3PM`). The
  power is off (the training's run terms); every room has dead ceiling fixtures (two in the big rooms) that the basement
  breaker relights (its power sector) for a couple of minutes. Breath shows in the cold (`TutorialColdBreath`).

Furniture is built from boxes and is solid. Nothing static looks like loot: the lamp, the TV, the fridge, the stove,
the washer and dryer are real salvage spawned by the lessons (rule of 0.24.9).

## The lessons

46 lessons in 9 chapters, data in `Data/Tutorial/Steps/*.asset` and the ordered `Data/Tutorial/Resources/TutorialCatalog.asset`
(defaults in `Editor/Tutorial/TutorialLessons.cs`, written once by **Build Tutorial Content**; after that the assets
are the truth: edit, reorder or add lessons without code).

0. **The driveway:** look, walk, sprint, crouch, jump, the readouts, up to the front door.
1. **The living room:** flashlight, scan, pick up the lamp, set it down, throw the cushion, pocket the cash, the four
   shared places (gear and pockets), switching hands.
2. **The kitchen:** the $25,000 vase (fragile), carry it to the truck, empty a pocket in the cargo bay, noise, the hand
   trolley from the kit crates, move the fridge (crews: lift it together), haul it to the truck.
3. **The laundry:** shut and open the sliding doors, pocket the watch on the shelf.
4. **The hallway:** load limits, fetch the grandfather clock, listen to the hallway crack, get off it, take the clock
   across in one go with nobody else on it (2026-10-22: the support jack and its two lessons were removed with the item), then the necklace in the south bedroom
   (the floor gives way), how floors fail.
5. **The basement:** get down, throw the breaker, climb out by the bulkhead.
6. **The visitor:** a Blind One comes in through the patio slider; hide in the master closet; wait until it walks out
   (the lesson sends it); crew talk, radio, pings, ghosts.
7. **The ensuite:** make room, pocket the ring (the best thing in the house), pockets ride with you.
8. **The truck:** load the clock, meet the quota, pull the lever.

The appraisal then prints as on a real job, with a training card instead of a payday (how a real job settles) and the
host's choice of **Back to HQ** or **Run the training again**.

Checks (`TutorialCheck`) and beats (`TutorialBeat`) are enums; a new kind of lesson needs one new case in
`TutorialChecks` (or a beat in `RunTraining.Beats`), everything else is data. Personal lessons (look, walk, sprint,
crouch, jump, scan, pockets, throw) are judged on each player's machine and reported; the rest are the host's view.
The crew moves through the lessons together (`TutorialConfig.AnyPlayerCompletes`).

## Guidance (2026-10-21)

The user: "more guided ... arrows that are interactive ... the audio ... a progress bar for each step". Everything below is
local and cosmetic on each machine, driven by one value the host writes (`RunTraining.Guide`).

- **The lesson card** (`TutorialObjectiveCard`), top centre under the compass: chapter and lesson number, the lesson's
  name in large stencil letters, what to do with the player's keys picked out in amber, **this lesson's own progress
  bar** with its numbers ("12 m to go", "1.4 / 2 m", "80° / 120°", "$4,200 / $6,000", "Floor load 85%", "2 / 3
  aboard"), and the **keys the lesson uses as key caps that light up while held** and stay marked once used
  (`TutorialKeyHints`). A finished lesson turns green, DONE is stamped on it and a chime plays; 0.9 s later the next
  lesson slides in.
- **The checklist** (`TutorialChecklist`), the old field note on the left: this chapter's lessons (one done before,
  the current one, three to come) with ticks, plus the strip of one block per lesson for the whole training.
- **Over the world** (`TutorialWaypoint`, `TutorialMarker`): a marker over the target (what it is, "PICK UP: VASE",
  "KITCHEN", "TRUCK", and how far) with a bobbing chevron that bobs faster as you close in; off screen or behind you it
  becomes an **arrow on the screen's edge turning toward it**. A blip when a new target appears, a ping when you reach it.
  While the visitor is in the house it has its own rust marker ("IT CAN HEAR YOU").
- **On the floor** (`TutorialFloorPath`): glowing chevrons flowing along the walkable route (the level's NavMesh, worked
  out again as you move: round furniture, through doors, down the cellar stairs, up the truck's ramp), a pulsing ring at
  the target, an outline around a room to enter, a floor to brace (amber) or a floor to get off (red).
- **HUD frames** (`TutorialHighlights`): pulsing frames with a tag around the instruments a lesson talks about (health
  and breath, compass, haul and quota, the four places, the flashlight battery, what the truck holds).
- **Voices** (`TutorialRadioVoice`): the dispatcher's radio line (when the lesson has one), then **the trainer reading
  the lesson** (`VoiceLine`, English, never a key name). A new lesson cuts off the old one. 48 trainer clips
  (`Art/Audio/TutorialNarration`, Kokoro `am_michael`, light radio filter) and 9 dispatcher clips were generated
  2026-10-21; the dispatcher script now reads Unity's folded long lines (its first run had misnamed 7 of the 9 clips).
- **Pause menu > Training**: every player can switch **Trainer's voice** and **Guide arrows** (Prefs).
- **F1**: a third debug line shows the guide (kind, target, followed object, progress, readout, the monster).

Where the arrows lead is data: each lesson's **Guide** line ("take item:lamp", "go area:porch", "use crate:hand_trolley",
"load truck", "use lever", "use door:0", "use breaker", "hide closet:0", "away section:hallway", "brace section:hallway",
"look area:porch", "none"). Empty works it out from the check, including phases: the vase first, then the truck once
someone holds it; for the quota, the nearest
unloaded training item, or the truck while someone carries one. **Highlight** names HUD pieces (vitals, compass, haul,
pockets, battery, cargo). Host side: `TutorialGuides` (target), `TutorialMeter` (progress), `TutorialGuideState`; the
struct is `TutorialGuide` (`TutorialGuideKind`, `TutorialGuideTarget`, `TutorialReadout`, append-only enums).
Personal lessons fill their bar from `TutorialWatch.Progress` on the player's own machine. Styles: `Art/UI/Resources/
TrainingHud.uss` (added to `UiThemeAssets.Layers`). The floor material: `Art/Tutorial/Resources/TutorialGuidePath.mat`
(made by Build Tutorial Content; a fallback is made at run time). The truck's ramp foot and bay come from spots the house
builder now places (`truck_ramp`, `truck_bay`) or, in a house built before that, from the truck itself.

## How it plugs in

| Piece | Where |
|---|---|
| Is this training? | `Tutorial/TutorialMode.Active` (the loaded scene is "Tutorial"); suspends achievements and stats. |
| The lessons, networked | `Tutorial/RunTraining` (+ `.Items`, `.Beats`) on the RunState prefab (`NetworkContentBuilder.RunStateExtras`), inert elsewhere. |
| Tunables | `Data/Tutorial/Resources/TutorialConfig.asset`: quota $6,000, the seed, power off, health floor 12, the visitor's timing. |
| HUD | `Tutorial/TutorialHud` runs the card, checklist, markers, floor arrows, HUD frames and voices (see Guidance), with `TutorialWatch` (personal lessons), on the session object (MenuUi). |
| Ways in | The main menu's **Training** card (Play screen; hosts, then `TutorialLaunch` drives the crew there) and the **training clipboard** on the HQ dispatch bench (`TrainingClipboard`, placed by the HQ builder and by **Add Training Clipboard to HQ**). New players get one toast at the HQ. |
| Leaving | Pause menu > Training: skip a lesson, restart the chapter (salvage put back, crew moved to the chapter's spot), leave (back to HQ). |
| Company | `CompanyService.StartTraining` / `ReturnFromTraining`: no contract, nothing owed, paid, struck or saved. `RulesApply` is false in training (the kit crates hand out gear the company may not own); leaving resets every kit and the HQ trims hands to stock. |
| Nobody dies | `NetworkPlayer.DealDamage` stops at `TutorialConfig.HealthFloor`; anyone below y = -8 is put back at the chapter spot. |
| Held still in training | Route changes, aftershocks, building decay and the total collapse, the danger level, monster waves/arrivals/drift/relocation, the Crew Director, boss requests, rush orders, the doors' random shuts, tips, the first-day guide, profile/achievement records. |
| Power | Training power is off; `PowerController.sectorsWithoutHorror` (new, set by the builder) lets the breaker relight its sector without the horror pacing. |
| Hiding | `HideSpotKind.Closet` (new, appended): a sliding-panel closet that conceals like a locker. |
| Floors | `StructuralSection.neverBelow` (new): a collapsible section that teaches the warnings but never falls. |
| Gear | `PlayerEquipment.ServerGive`, `ServerTrainingKit`, `ServerHint` (new, host). |
| Monster | `ThreatDirector.SpawnScripted(kind, at, facing)` (new): exactly where the lesson wants it. |
| Text | `Data/Text/Resources/Text/tutorial.txt` (every line in 9 languages; Claude's translations, no native review). `Tools/Text/check.py` now also reads `ChapterName`, `CrewDescription` and `RadioLine` from content assets. |
| Voice | The dispatcher's radio lines (captions, and `Art/Audio/TutorialDispatch/.../td_*.wav`) then the trainer's reading of the lesson (`Art/Audio/TutorialNarration/.../tn_*.wav`), both named by a hash of their English line. Generated 2026-10-21 with `Tools/Audio/generate_tutorial_dispatch.py` and `generate_tutorial_narration.py` (Kokoro-82M); re-run after editing a RadioLine or VoiceLine. |
| Builds | `BuildScenes.All` (and so every player build) carries the scene; it is not in `SalvageScenes` and not a board location. `BatchCommands.RebuildContent`/`RebuildHorrorContent` build it; `BatchCommands.RebuildTutorial` builds only it (and the HQ clipboard). |
| Tests | EditMode `TutorialTests`; PlayMode `TutorialPlayTests` and `LevelSmokeTests.Tutorial`; nettest scenario `training`. **None run.** |

## In Unity, in this order

1. Open the project; let it compile. Fix whatever the compiler says (nothing here was compiled). The house was redone
   on 2026-10-21: the saved `Tutorial.unity` is the old, tight one until step 2 runs again.
2. **Tools > Abandoned > Maps > Tutorial House > Rebuild Tutorial House** (writes the lessons and config, adds RunTraining to the
   RunState prefab, builds and saves `Scenes/Tutorial.unity`, bakes NavMesh and occlusion, updates Build Settings).
3. **Tools > Abandoned > Maps > Tutorial House > Add Training Clipboard to HQ** (adds the clipboard to the saved HQ without rebuilding it).
4. The voices are already generated. After editing a lesson's RadioLine or VoiceLine, run
   `python3 Tools/Audio/generate_tutorial_dispatch.py` / `generate_tutorial_narration.py` again, then
   `python3 Tools/Text/check.py` (expect 0 missing).
5. Play from the main menu: Play > Training > Start training.

## What to check first

- **The house itself (redone 2026-10-21):** walk every room and doorway with the trolley and the clock and a shared
  carry: nowhere should catch you. Walls meeting, the roof gables over the bigger wings, the stair down the bulkhead,
  the porch ramp, the patio slider opening on its east half. The floor arrows from the driveway to the porch, the
  kitchen to the truck up the ramp, down the hallway and down the bulkhead, without crossing a wall.
- **The town:** walk to the fence in every direction; nowhere to fall off or squeeze past. The occlusion bake leaves
  the town out (it would cover the whole world).
- **The street's houses:** walk up to the ones by the fence: no way in at the door, the slider, the garage or between
  the wings (the patio is open, the house shut); nothing inside but dark behind the glass; the look varies house to
  house (mirrored, paint, garage, hood, chimney); the near / far switch as you walk away is not distracting; frame
  rate looking down Larch Court. If the far switch pops too close, `TutorialStreetHouse` sets it (LOD 0.25).
- **The hallway lesson:** with the clock on the trolley the hallway should go Stressed and then Cracking, never fall.
- **The fall:** the south bedroom's floor should fall under the player into the basement without hurting them, the
  bed and nightstand falling with it; climbing out by the bulkhead.
- **The breaker:** it should light the basement (and, by sector, part of the house) with the power off.
- **The visitor:** the Blind One should spawn on the patio, come in through the open slider, wander near the dining
  room, then walk back out and vanish; while hidden in the master closet, nobody should be found.
- **Kit crates:** taking the trolley and a medkit; leaving training should leave no crate gear in hands at the HQ.
- **The flow from the HQ** (clipboard) and from the main menu (Training card), the appraisal's buttons, "Run the
  training again", the pause menu's skip/restart/leave, a two-player run (Multiplayer Play Mode).
- **Text:** the field note's wrapping in all nine languages (long German and Russian lines).
- **Guidance (2026-10-21):** in a house built before the guidance, first run **Tools > Abandoned > Tutorial > Build
  Tutorial Content** once: it gives the existing lesson assets their Guide, Highlight and VoiceLine (without that, the
  arrows still work from each lesson's check but there is no trainer voice and no HUD frames) and makes the floor
  material. Rebuilding the house is optional (it adds the truck spots; the guide works them out without). Then check:
  the floor chevrons follow a sensible route (driveway to porch, kitchen to the truck up the ramp, the hallway, down the
  bulkhead), the edge arrow turns the right way when the target is behind you, the card fits under the compass on 16:9
  and 16:10 and in German, the key caps light up, the DONE stamp and the slide-in, the voice is cut off by the next lesson
  and switches off from the pause menu, the visitor's red marker.

## Decisions taken without asking (the user can reverse any)

- The lessons are taught crew-wide (the first player to do a personal lesson moves everyone on); the host alone can
  skip, restart or leave. `TutorialConfig.AnyPlayerCompletes` switches to "everyone must".
- Training starts each player with a flashlight only (four places hold gear and pockets together; the radio would crowd
  the lessons, and in training no radio is needed to talk on it). Leaving training resets every kit.
- The ensuite's prize is a diamond ring (pocketed), not a safe or a lock: there is no lock in the game that fits a
  bathroom; the lesson is "pocket the small fortunes".
- Interior doors stand open (hinged leaves, not usable); only the laundry's bypass doors and the patio slider are
  real doors (`SlidingDoor` 0 and 1). Training starts every door open.
- The training quota is $6,000, shown on the truck, never owed. Training never pays and records no achievements, play
  stats or profile runs.
- Completing the training also marks the first-day guide as done (its lessons are all covered).
