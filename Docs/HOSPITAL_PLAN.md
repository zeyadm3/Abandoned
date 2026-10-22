# Saint Brigid Hospital — 2026-10-23

Implementation only. The user prohibited compilation, testing, verification, Unity, scene generation and screenshots.
Claude CLI owns execution/review. Scene target: `Game/Assets/_Project/Scenes/Hospital.unity`.

## Plan and authoring contract

A 48 × 40 m, two-storey abandoned county hospital. Four-metre structural modules and a cross-shaped,
8 m wide protected circulation spine connect reception, emergency, pharmacy, imaging, wards, theatre,
laboratory and records. Both outside stair towers are permanent, 3.2 m wide, and reach independent exits.
Rooms have 2.4 m clear openings, no decorative door leaf blocking the carry route. The front entrance is 8 m.
The upper clinical rooms decay and collapse; floors below receive the shared deterministic rubble.
Permanent spine/stairs carry SectionKeepClear so rubble cannot cut both extraction routes.

The road connection is the south edge of the forecourt, z=-28; loading marker at (24,0,-14), facing south.
World streaming owns the moving cargo van. No second truck or static cargo system is generated in this POI.
Four crew markers are fallback positions beside the apron, outside the interior spawn exclusion zone.
All POI objects, including the shared plumbing's scene objects, are grouped under `Hospital`, except NGO's
NetworkManager and session bootstrap, which must remain roots for the existing bootstrap lifecycle.
World code must move the full POI root before activation and transform bounds/config positions consistently;
scene-local bounds and HorrorConfig coordinates are authored at origin. The world agent owns that integration.

## Implementation sequence

1. Hospital layout, structural floors, protected stairs and accessible openings.
2. Original hospital fittings, scanned material reuse and metre-scale UV meshes in Art/Hospital.
3. Map-local LootDefinitions, models, network prefabs, catalog and spawn tags.
4. Hiding cabinets, breakers, fixtures, local ambience, threats and host run plumbing.
5. Shared menu/build/rebuild registrations as the final edit, then map-only commit on main.

## Decisions for CLAUDE.md (do not edit that file here)

- Hospital is now an authored salvage POI, superseding its old post-Early-Access exclusion.
- POIs provide a carry apron; the world supplies the only collection van.
- Hospital loot and run prefab are map-local; no global catalog regeneration or other map edits.
- Scene assets are generated only by Claude CLI using the map rebuild entry point.

## Authored content

`Abandoned.Hospital.HospitalBuilder.Build` (menu **Maps > Hospital > Rebuild Saint Brigid Hospital**) authors:

- 240 four-metre slabs, 40 protected upper spine slabs and 80 collapsible upper room slabs; ground slabs are
  non-collapsible. Two protected stair flights plus four landings. The perimeter shell and roof are permanent.
- Reception benches and fitted desk, pharmacy storage, emergency/ward beds with gas outlets, operating table,
  laboratory counters, records cabinets, rooftop air handlers, entrance canopy, parking and independent yard lights.
- 48 ordinary spawn markers and four alternative jackpot markers, on eight Hospital-only tag families.
  Nineteen LootDefinitions: stethoscope, otoscope, defibrillator, suction pump, apothecary case, balance, mortar,
  vaccine refrigerator, call bell, admissions ledger, clock, infusion controller, resuscitator, instrument case,
  microscope, slide cabinet, research archive, mobile X-ray and anaesthetic workstation. The last two are jackpots.
  The shared seeded planner chooses how many prizes actually appear; four markers do not mean four prizes.
- Ten threat starts: four ground starts and six upper starts;
  every start is beyond the entrance threshold, with upper pressure reachable by both stairs.
- Eight concealing linen cabinets (HidingLocker/Closet), ids 80–87, all section-bound and outside loot reservation bays.
- Thirty ceiling fixtures, deterministically healthy/faulty/dead, six breaker panels with distinct sector indices.
  Two healthy cross-hall fixtures cast shadows; the other fixtures and yard lights do not.
  Ground ceiling panels/lamps follow their supporting upper slab; bolted room furniture follows its floor.
- Building interior fog, wind/room tone/fixture hum, spatial ventilation and dripping rooms, plus shared structural,
  breaker, hiding, loot and monster sounds. No custom audio bypasses GameAudio.
- A local catalog, local network prefabs, local run-state clone, HorrorConfig and LocationDefinition in owned folders.
  Navigation is baked by the builder into Art/Hospital/HospitalNavigation.asset, never a new file beside the scene.
  No camera, AudioListener, directional sun or global post-process volume is placed in the streamed POI.

Original geometry uses existing ambientCG CC0 surfaces (see the appended asset-credits entry). No new downloads,
packages or third-party models. Material reuse does not change shared scan importers or other maps' materials.
Rebuild preserves existing LootDefinition tuning, model/prefab assets and material tuning; the map layout and local
catalog membership are rebuilt deterministically. Change a definition's dimensions/model then remove only its
owned generated network prefab if regeneration is required.

## World integration contract for Claude CLI

The world agent owns POI placement/registration and activation. Scene key `Hospital`, location id `hospital`;
root `Hospital`; resource prefix `Hospital/Network`; catalog `Data/Loot/Hospital/HospitalCatalog.asset`;
location `Data/Loot/Hospital/HospitalLocation.asset`. Loading marker `Hospital/VanDock` is a pose, not cargo.
`Abandoned.Networking.HospitalNetworkContent.Register(manager)` registers all this map's network prefabs before
Host/Client starts, including when Hospital itself is not loaded. Automatic sceneLoaded registration covers the
existing scene-based bootstrap; a later-created world manager must call Register before it starts NGO. Do not
change ForceSamePrefabs mid-session. All clients require the generated map resources in their build.

Hospital's local run clone references HospitalHorror; its permitted scenes are Hospital and World. The builder uses
LocationPlumbing and retains the existing standalone run fallback. The shared runtime currently uses global Current
registries, SessionTravel.LevelReady and active-scene instantiation: the world agent must gate loaded-but-idle POI
RunDirector/StructureNetSpawner/ThreatDirector and bind the active POI before starting its job. Root translation
alone does not translate serialized LevelBuildingBounds, BuildingInteriorAtmosphere or HorrorConfig vectors; the
world placement pass must transform those, the NavMesh data and sector assignments coherently. These are explicit
handoff requirements in the world-owned paths, which this task must not edit. No global HorrorConfig/catalog is rewritten.

## Decisions for CLAUDE.md, continued

- Generated nav data belongs to the map's Art folder; POI scenes carry no second camera, sun or collection truck.
- Register map-local Resources prefabs before NGO starts; retain ForceSamePrefabs and the host authority model.
- Local run plumbing is authorable now; the world owner handles POI activation, placement and singleton scoping.

## What to check first

For Claude CLI only; none of these actions was performed in this task.

1. Compile, then run the Hospital rebuild entry point to generate Hospital.unity and the owned data/art assets.
   The second map commit also finishes Hospital material/surface tuning, cabinet ids and non-overlapping prize bays.
2. Walk entrance → both cross halls → both stairs → each room → south loading marker with Heavy/Huge salvage.
3. Exercise the Hospital-only catalog: small, Heavy and both prize types; ensure no ordinary/jackpot tag crossover.
4. Collapse an upper room slab: its furniture, ceiling below and lights disappear; the corridor/stairs remain clear
   and navigation carves the hole. Restore the run and check recovery.
5. Check all eight cabinet entrances/exits and six breakers, powered/off/night states, room sound and fixture hum.
6. Review four-player host/client loot, collapse and threat spawning, then distance-stream the POI in/out through the
   world activation path, including prefab registration, translated bounds/sectors, cargo persistence and unload cleanup.
