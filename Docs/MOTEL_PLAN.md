# Last Light Motor Lodge — roadside expansion, 2026-10-23

Implementation only. This agent did not compile (including dotnet), invoke Unity, run the game, run tests or
engine verification, generate scenes, bake navigation/occlusion, or take screenshots. Claude CLI owns execution/review.
The entry point remains `Abandoned.Motel.MotelBuilder.Build`; its namespace and menu registration are unchanged.

**Current user direction:** preserve the Motel's setting and surrounding area. This pass changes the playable lodge
and its existing amenity interiors only. MotelSite, the site rectangle, van pose, ground, roads, sky/nature and
perimeter are unchanged. No additional surrounding buildings or terrain are authored.

## Layout and character

The lodge retains sixteen 7 × 12 m guest rooms on two floors at y=0/4, reached from the courtyard and upper gallery.
The central service block now has a real upstairs manager's flat instead of six missing floor cells and a low roof.
A detached diner and maintenance shed extend the site to the east. No sky, trees, grass or horizon geometry is added;
Art > Upgrade New Maps remains responsible for the reviewed art/lighting chain afterwards.

| Space | Recognisable fittings and use |
|---|---|
| Guest rooms | Four variants, offset between floors: paired twins, queen/workspace, queen/child cot, queen/sofa; made/bare beds, pillows, headboards, fitted vanities, mirrors, AC grilles and bolted landscape pictures |
| Bathrooms | Separate rear rooms with tub, shower riser, basin, toilet/cistern and towel rail; a clear central loot/door approach |
| Front office and back room | Reception counter, empty twelve-hook key board, proper back-room partition/doorway and a concealing steel cabinet |
| Laundry | Bolted three-machine bank and coin details, service exit, staff locker and a separate commercial-washer prize bay |
| Manager's flat | Kitchen/worktop, sofa, bedroom partition, bed and wardrobe, with office and guest salvage points |
| Ice/vending alcoves | Two paired dispenser/vending banks under hoods, separate from the gallery carry lane |
| Diner | West-side booths and tables, long counter/stools, kitchen partition, fixed range, prep table and stocked shelves; dining roof largely lost, with exposed ribs and a cross beam carrying the lights |
| Maintenance shed | Long workbench, pegboard/brackets, stock rack, locker and steel cabinet, with room to turn a three-person transformer |
| Pool and roadside apron | Recessed drained basin, ramp, railings, three failing west deck sections, sign pylon, parking stripes, wheel stops and independent yard lamps |

The main building remains 64 m wide and z=12..28. Diner floors occupy x=80..104, z=0..20; the maintenance shed is
x=76..88, z=28..40. Paved connectors and the enlarged rear service yard join all entrances without a new truck.
The diner and shed are permanent detached structures outside the lodge's final-collapse envelope; the dining room
is exposed to the sky. They provide extra salvage routes and monster space, not an artificial monster exclusion zone.

## Interior level-design pass (2026-10-23)

Four connecting room pairs create short interior escape loops: ground 102↔103 and 106↔107, upper 201↔202 and
207↔208. Actual gaps replace their party walls, with 2.2 m openings, timber jambs and removed-leaf hinge plates;
there is no solid wall behind a decorative doorway. These are permanent open connections, not new lock mechanics.
A crew avoiding a monster on the gallery can move through the neighbouring room, while accepting the risk of its
rear upper slabs. The original front doors, protected inner gallery and both stairs remain the primary haul routes.
The Huge suite prize can still leave by its original door. The connections do not rely on any power state.

Guest variants gain fixed bedside cupboards and powered reading sconces, fitted writing desks with repositioned
chairs, stacked curtains, bathroom taps, shower heads/rods, gathered shower curtains and tile/skirting bands. New
fittings stay away from entry wardrobes, original loot points and the connecting-door approaches. The reading-light
fixtures use emissive lenses without extra shadow lamps and follow their floor support. No static replica of a
salvage clock, radio, telephone or other loose loot is placed as scenery.

The office back-room side partition now stops before the laundry doorway, leaving a clear turning apron instead
of splitting that doorway into two narrow approaches. Receipt slots, overhead laundry services and a diner
extractor improve the existing service interiors. Sixteen exposed joist units follow the individual failing outer
gallery slabs; a subtle floor inlay distinguishes the protected inner lane. These additions are within the map,
without changing the courtyard/parking/site construction.

MotelSolids fits furniture collision to visible parts rather than broad boxes through empty chair/table/shelf
space. Continuous rail safety planes and closed hiding-container occupancy shells carry explicit intentional
collision reasons. Pool-deck skirts now have matching visible aprons, section-bound independently of the scaled
slab mesh; losing a deck removes both the skirt collision and its visual. This addresses the reviewer's invisible
collision findings by changing the shapes and documenting the genuinely intentional shells; no audit was run here.

At rebuild time the shared HospitalRegistration editor helper merges this map's own 38 local loot entries and
Motel location into the shared catalogs, preserving other maps' entries. That also addresses the reviewer’s
missing job/loot registration notes. The scene, namespace, catalog/resource paths, existing 72 ordinary markers,
three prize candidates, 23 hiding spots and 20 threat starts remain stable. No new player-facing wording or
third-party asset is introduced in this pass.

## Structural choices and carry routes

The lodge has 64 ground slabs and 80 upper pieces: sixteen gallery cells are split into two 4 × 2 m halves.
The outer half at z=12..14 can peel away with its own railing. The inner half at z=14..16 stays permanent and carries
SectionKeepClear. The next row, room thresholds at z=16..20, is protected too. Thirty-two rear upper room/manager
slabs can fall; the manager bedroom now fails over the laundry. The two 3.2 m end stairs and four landings stay solid
and protected. Their stepped solid undersides prevent players or dropped loot entering a low-headroom trap.
This gives 48 failing upper sections, with a permanent route between every entrance and either stair.

Three 4 × 2 m west pool-deck sections can collapse into the basin, independently of the lodge. The west rim is open
at deck height so the deck loot is reachable from the courtyard. East guards follow their own section. Low skirts
close the space underneath while the deck exists, avoiding a low-headroom trap; those colliders disappear with it.
The recessed pool floor is y=-.8, and its 2.4 m north ramp/NavMesh link remains an exit after the deck fails.
Thirty diner and nine shed floor modules are permanent. All slabs stay out of baked static occlusion.

Main carry route: guest room → 2.2 m doorway → protected inner gallery → either end stair → open courtyard → van.
The Heavy minibar also has this route. The Huge suite stereo fits the doorway and can align lengthwise in the inner
lane; the outer gallery is the optional wider, risky route. Laundry haul can leave through the rear 2.4 m service
door and either side drive, avoiding the office. Diner and maintenance Huge salvage uses 4 m / 3.2 m doors and the
wide east yard to the south apron. The pool and ice banks sit outside the striped central carry lane.

Fitted furniture, bathroom groups, wardrobes, loose gallery guards and ground ceiling panels use SectionProp.
Ground fixtures hang from upper support sections. Upper roof fixtures remain attached to the permanent roof.
Walls/roof/shell and permanent internal partitions are marked static, then MotelOcclusion clears every floor and
SectionProp subtree, including the ceiling panels under failing rooms. Dynamic translated signs are excluded too.
The builder saves, calls OcclusionBake.Bake, then saves again. Those operations are authored, not executed here.
The reviewer's LevelSolidity pass, second NavMesh bake and Modifier() reuse helper are preserved.

## Loot, hiding and monster pressure

The map-local catalog now defines 38 items: the original 22 plus coffee percolator, Heavy griddle, ordinary Huge
chest freezer, cutlery roll, milkshake mixer, syrup crate, rolling tool chest, compressor, ordinary Huge neon
transformer, pressure gauge, pool pump, whistle, rotary phone, payroll ledger, postcards and silver locket.
Pocket, OneHand, TwoHand, Heavy/two-crew and Huge/three-crew classes all have eligible map spawn points.
New freezer, griddle, transformer, pump/compressor, percolator and mixer models have distinct original silhouettes.
The phone Rings; suitcase and syrup crate Rattle, using existing LootQuirk behavior and host-owned loot plumbing.

There are 72 ordinary points: 48 in guest rooms/bathrooms, four office, four laundry, three manager, two alcove,
two pool, six diner and three maintenance points. Each room/amenity offers salvage. Three separate prize candidates
serve the two end suites and laundry, while the original suite stereo/coin washer remain the two jackpot definitions;
the shared seeded planner chooses the run's actual 1–2 prizes. Ordinary diner points and one maintenance point
admit Huge items, so the freezer/transformer can actually appear without becoming extra jackpot definitions.
Nine tag families are map-owned: `motel_guest`, `bath`, `office`, `laundry`, `diner`, `maintenance`, `pool`,
`suite_prize`, `laundry_prize`, with the `motel_` prefix applied to every family.

Twenty-three supported hiding spots use HidingLocker: sixteen guest closets/wardrobes plus office cabinet,
laundry locker, manager wardrobe, alcove cabinet, diner cabinet and two shed spots. Cabinet doors, wooden closet
louvers and staff locker finishes differ; under-bed hiding is not invented. Guest IDs remain 96–111; amenities use
112, 114–117 and 120–121. Exit markers face into open room space rather than a wall or a prize footprint.
Twenty authored threat markers span both lodge floors, manager/office, upper gallery, courtyard/pool, diner and shed.
Existing host ThreatDirector/catalog chooses the roster; markers avoid the crew's arrival strip.

## Power, practical lights and sound

Room and bathroom fixtures cover all sixteen guest rooms, both galleries, office/back room, laundry, manager living
and bedroom, diner/kitchen, shed and both alcoves. The neon pylon has its own powered LightFixture. Only the existing
two designated main fixtures cast shadows. Six numbered breaker panels are spread across front and rear wings,
with SectorCell (24,100,24), keeping each upstairs wing on the circuit beneath it. The geographic formula also groups
the annex circuits. Eight independent green battery lamps mark the inner gallery and annex entrances, in addition
to the four original yard lamps. MotelEmergencyLight makes a battery lamp obey its support if that support is lost.

MotelSoundscape reads MotelSoundConfig and uses existing SoundIds through GameAudio: HumLoop at the neon transformer,
VentDrone at the ice dispensers, intermittent RadioStatic from behind room entrances, WindLoop along the upper
gallery, and WaterDrip in bathrooms/pool. The electrical cues follow nearby LightFixture power; a lost TV-room floor
silences that set. Wind and drips continue with mains off. F1 draws each sound position in a different colour.
These are local cosmetic cues, not AI hearing events or network authorities. Loops belong to map children and are
silenced on disable. The reviewer's builder LevelAmbience bed is retained; the old duplicate services bed and
individual HorrorAreaAmbience drip components are removed to avoid layering the same ambience repeatedly.

## Code, art and text

All Motel editor files now use statements on separate lines and braces on their own lines. New helpers split guest
props, amenities, annex, fitted furniture, large salvage models, sound wiring and occlusion preparation. Runtime
MotelSoundConfig, MotelSoundscape, MotelEmergencyLight and MotelWorldSign are named Motel* in the allowed Scripts
areas; the existing MotelNetworkContent remains necessary to register owned network resources before connection.
The reviewer's DefaultGameObjects sun, Mood, LevelAtmosphere.Apply, LevelAmbience and navigation additions remain.

All geometry and new salvage models are original work. Materials/audio/fonts reuse already credited ambientCG,
project sound-bank and shipped font assets; no new downloads, packages or third-party assets are introduced.
Mounted word signs select the existing Noto font for the active script and resize to fit. Motel loot, location and
sign wording is in Data/Text/Resources/Text/motel.txt in English plus all nine shipped translation languages.
The text checker was not run because verification is prohibited. Existing tuned definitions/models/prefabs remain
preserved; new definitions and generated content are created when Claude CLI runs the builder.

## Open-world fit (moving origin)

The Motel scene never moves. It loads at the authored origin; the world moves land/roads beneath the active anchor.
The existing absolute configs/NavMesh stay there. No script moves the root and no second cargo truck is authored.

- Own ground rectangle: **x=-8..112, z=-24..44** (`Rect(-8,-24,120,68)`). Every outer ground edge meets **y=0**.
- Road connection: **south edge z=-24**, across the van apron, aligned with the central lane at x=32.
- Van parking/loading pose: **(32,0,-12)**, forward **(0,0,-1)**, `Motel/VanDock`.
- Lodge collapse bounds remain x=-4..68, z=12..28, y=0..8. Courtyard, pool, apron, diner and shed are outside.
- Omit world terrain across the complete own-ground rectangle; it must not fill the pool's real depression.
- No standalone horizon/sky geometry exists to hide. The adapter handles standalone camera/sun/global atmosphere.
- Location id `motel`, scene `Motel`, resource prefix `Motel/Network` and the owned catalog/run clone remain stable.
  World readiness/activation and persistent cargo stay with Claude CLI's world adapter; its paths were not touched.

## Pre-rebuild source review (2026-10-23)

The builder, menu/rebuild registrations, runtime dependencies and map-owned script metadata are present. The catalog
contains 38 authored definitions across every carry class; all of their names have English plus nine translated
sheet entries. Shared structure configs/fractured tile, run/structure network prefabs and loot/carry configs are
present on disk. Unity folder metadata is included. This was a source/file review only; no compilation or engine
execution was performed, and generated scene/data/art content still comes from the rebuild.

## Decisions CLAUDE.md should record

Last Light is a lodge plus diner, manager flat and maintenance compound. Upper gallery outer halves and the pool
deck can fail; the inner gallery, thresholds and two end stairs are protected. Ordinary Huge diner/shed salvage
is separate from the two jackpot definitions. POIs stay at origin; use the expanded ground rectangle above.
Connecting guest-room loops and detailed fitted interiors are part of the lodge; keep the surrounding site frozen.
Runtime Motel* scripts in the allowed areas are user-authorized. Upgrade New Maps owns sky/nature/remaster.
All execution/review belongs to Claude CLI; no compilation, scene generation, bake or gameplay result is claimed.

## What to check first

For Claude CLI only:

1. Compile and rebuild Motel, then run Upgrade New Maps. Check the four connecting pairs, entry/loot clearances,
   writing desks/sconces/bath details, office-to-laundry turn, fitted collision and unchanged exterior site.
   Inspect all four guest variants, manager floor, diner,
   shed, fitted prop density, supported hiding kinds and practical light placement.
2. Carry the suite stereo through a room door and down both end stairs, the washer out the rear laundry door,
   and the ordinary Huge freezer/transformer through their annex doors to the van.
3. Collapse the outer gallery while using the inner lane, then rear room/manager slabs; review ceiling/fixture,
   furniture/guard hiding, nav carving, protected rubble clearance and restoration.
4. Reach pool-deck loot from the west, collapse a deck tile, and leave the basin by the ramp without a terrain
   plane or surviving skirt trapping anyone below.
5. Review 72 ordinary points, all nine tags and three prize candidates, including eligible ordinary Huge loot,
   turn space, hiding exits, twenty threat starts and the central carry lane.
6. Switch off mains, restore each breaker, and review battery routes, neon/ice/TV silence, persistent wind/drips,
   reduced flashing, F1 sound markers, translated signs/fonts and all nine text languages.
7. Review saved occlusion through lost slabs, then host plus three clients and world stream/unload behavior at
   origin using the expanded ground rectangle/edge height and van pose. This agent performed none of these checks.
