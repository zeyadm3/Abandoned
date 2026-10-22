# Last Light Motor Lodge — 2026-10-23

Implementation only. The user prohibited compilation, testing, verification, Unity, scene generation and screenshots.
Claude CLI owns execution/review. Scene target: `Game/Assets/_Project/Scenes/Motel.unity`.

## Plan and authoring contract

A roadside motor lodge: two parallel-to-the-road room wings flank an eight-metre reception/laundry block.
Sixteen guest rooms (101–108, 201–208) each have a private bathroom. Rooms are 7 × 12 m with a 2.2 m entrance
and a 2.2 m bathroom opening. A four-metre exterior gallery runs the full 64 m frontage on both storeys.
Two independent protected 3.2 m stairs reach the upper gallery at the ends. No elevator dependency.

South of the frontage: a courtyard with a drained, railed pool and a broad central carry lane to the van apron.
The loading pose is (32,0,-12), facing south; the road mouth is the south edge at z=-24.
The main building bounds cover z=12..28, keeping the courtyard and van apron outside the collapse envelope.
The world supplies the only cargo van; the location authors no duplicate collection truck.

## Build order

1. Module floors, guest-room partitions, service block, galleries, stairs, roofs and courtyard.
2. Original furnished rooms, bathrooms, office/laundry fittings, scanned surfaces and wayfinding.
3. Motel-local loot definitions, distinct salvage models, prefabs/catalog and room/service spawn tags.
4. Hiding wardrobes, power circuits, room/gallery lamps, sound and threat starts; shared host run plumbing.
5. Final shared menu/build/rebuild additions and map-only commit on main.

## Decisions for CLAUDE.md (do not edit that file here)

- Motel is a new distance-streamed salvage POI, separate from the proposed skyscraper restaurant.
- Guest-room interiors can fall; both exterior galleries and both stairs are permanent keep-clear carry routes.
- The unroofed courtyard is outside the building-collapse envelope.
- Map-local data/art/network resources do not rewrite global content catalogs or other maps.

## Authored content

`Abandoned.Motel.MotelBuilder.Build` (menu **Maps > Motel > Rebuild Last Light Motor Lodge**) authors:

- 122 four-metre slabs: 64 ground slabs and 58 upper slabs. Thirty upper slabs (gallery and room thresholds)
  are protected; the other 28 upper guest-room slabs can collapse. The upper floor omits the six cells above
  the central service block; that block has its own lower roof. Two permanent stair flights and four landings
  carry SectionKeepClear, as do both protected gallery/threshold rows.
- Sixteen furnished guest rooms with bed variants, fitted vanities, wall ventilation, opaque window bands,
  numbered entrances, bathroom partitions, tub and sink fittings. An office counter, empty key board,
  fixed laundry bank, rooftop vents, gallery railings, road pylon, marked parking and independent yard lamps.
  Fixed fittings have a different silhouette from loose salvage. No non-interactive duplicate loot models.
- Fifty-six ordinary loot markers (48 guest-room/bathroom, four office, four laundry), and three alternative
  jackpot markers (two end suites and the laundry). The standard seeded planner controls actual prize count.
  Six Motel-only tag families: guest, bath, office, laundry, suite_prize and laundry_prize, each prefixed `motel_`.
- Twenty-two LootDefinitions: room key, travel clock, suitcase, tube television, valve radio, film camera,
  minibar, coat, perfume, silver brush, vanity mirror, toiletry case, cash register, key cabinet, guest book,
  counter bell, linen bale, iron, sewing machine, water heater, suite stereo and commercial coin washer.
  The stereo and washer are Huge jackpots; their widths fit the authored doorways and the existing cargo bay.
- Twelve threat starts (six ground and six upper room thresholds), reachable from the gallery/stairs and away
  from crew arrival at the apron. The existing host ThreatDirector and catalog provide the monster roster.
- Sixteen concealing guest wardrobes, ids 96–111 (separate from Hospital's 80–87), each using HidingLocker/Closet.
  Fifty-two fixtures with deterministic faulty/dead variants, including room, bath, gallery, office and laundry;
  two healthy gallery fixtures cast shadows. Six numbered breaker cabinets mount beside the office entrance.
- Wind/room tone/powered hum, sixteen bathroom drip areas, pool drip and laundry duct ambience. Existing
  LightFixture/HidingLocker/StructurePresentation and the shared sound bank supply the functional sounds.
- A drained pool with a real recessed floor, walls, rails, a 2.4 m exit ramp and a bidirectional NavMesh link.
  It lies off the eight-metre carry lane; there is no death volume in the courtyard.
- Local catalog, network prefabs, run-state clone, HorrorConfig and LocationDefinition in owned folders.
  Navigation is authored into Art/Motel/MotelNavigation.asset. No camera, listener, sun, global volume or static
  collection truck is added to the POI.

All geometry and salvage models are original project work. Materials reuse existing ambientCG CC0 scans,
referenced without changing shared textures/importers; audio and the sign font reuse the existing credited library.
No new packages or external downloads. Floor and fitting meshes have metre-scale UVs. Existing definition,
model/prefab and material tuning is preserved on rebuild; layout and catalog membership are regenerated.
After changing a definition's model/dimensions, remove only its owned generated network prefab to regenerate it.

## World integration contract for Claude CLI

Scene key `Motel`, location id `motel`, root `Motel`, resource prefix `Motel/Network`;
catalog `Data/Loot/Motel/MotelCatalog.asset`; location `Data/Loot/Motel/MotelLocation.asset`;
loading pose `Motel/VanDock` at (32,0,-12), facing south. The road mouth spans the south apron at z=-24.
The map footprint, including the pool, needs the world terrain collider omitted or lowered beneath it; the map's
own courtyard colliders leave a pool opening deliberately. Do not lay a world ground plane across the pool.

`Abandoned.Networking.MotelNetworkContent.Register(manager)` registers the map's Resources prefabs before
Host/Client starts, even while Motel is not loaded. Automatic sceneLoaded registration covers the existing
scene bootstrap; a manager created later must call Register before starting NGO. Keep ForceSamePrefabs enabled.
The builder also provides an owned NetworkPrefabsList on its standalone manager. Register both map libraries in
all participants before connection, rather than waiting until one client streams in a map.

The world owner supplies POI positions, contract catalog registration, distance streaming and activation. As with
Hospital, shared runtime currently assumes one active level: gate loaded-but-idle RunDirector, StructureNetSpawner,
ThreatDirector and global Current registries until the crew activates that POI. Reparented map content sits under
Motel; NetworkManager and Network bootstrap stay roots for the existing persistent-session lifecycle. Translation
must include serialized LevelBuildingBounds, BuildingInteriorAtmosphere, HorrorConfig vectors, NavMesh placement
and power-sector assignments. The local run clone allows scenes Motel/World. The world-owned layer must bind the
active POI before spawning its run; avoid a second cargo truck, premature run, or duplicated singleton director.
These world-owned changes are intentionally left to Claude CLI's concurrent open-world task.

## Decisions for CLAUDE.md, continued

- Map rebuild entry points are `Abandoned.Hospital.HospitalBuilder.Build` and `Abandoned.Motel.MotelBuilder.Build`.
- Own local catalogs and resource-prefab registration allow both maps without rewriting the global content library.
- The shared world is responsible for placement, activation and singleton scoping; scene files are generated by
  Claude CLI, never hand-authored YAML. No compile, scene generation, bake, audit, gameplay or screenshot ran here.

## What to check first

For Claude CLI only; none of these actions was performed in this task.

1. Compile and run the Motel rebuild entry point to generate Motel.unity and its owned data/art assets.
2. Carry the Huge stereo from room 208 through its 2.2 m door, along the gallery, down either 3.2 m stair and
   through the courtyard to the van; carry the washer through the laundry's rear exit and around either end.
3. Exercise all six loot tags and prize candidates; confirm ordinary items and prizes do not overlap and room
   props cannot be mistaken for grabbable salvage. Check the 56 ordinary points and planner's chosen prize count.
4. Collapse a rear guest-room slab and restore it: furniture, ceiling below and lights follow it; the gallery,
   thresholds and both stairs remain passable and clear of rubble; navigation carves and restores the opening.
5. Enter/exit all sixteen wardrobes, restore each breaker with mains off, and review day/night light pools,
   reduced-flashing behavior, bathroom/pool drips, laundry ventilation and powered hum.
6. Walk out of the recessed pool by its ramp and traverse the side/rear drives without crossing hidden ground.
7. Review host plus three clients, then distance-stream both POIs through the world activation path: prefab
   registration before connect, world terrain openings, translated bounds/nav/power sectors, persistent van cargo,
   job activation only on arrival and cleanup on unload. The courtyard and van apron must stay outside total collapse.
