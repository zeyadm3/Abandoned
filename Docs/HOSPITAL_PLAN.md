# Saint Brigid Hospital — Medical City rebuild, 2026-10-23

The former rectangular two-storey precinct-like hospital is replaced. This is authored source only: no Unity,
compilation, game, tests, screenshots or engine verification were run by this agent. Claude CLI owns execution.
The entry point stays `Abandoned.Hospital.HospitalBuilder.Build` and scene/id stay Hospital/hospital.

## Architectural references and intent

The user's references are Burjeel Medical City in Mohamed Bin Zayed City and Cleveland Clinic Abu Dhabi.
This is an original, compact salvage-game interpretation, not a surveyed replica or a copy of their branding.

- [Burjeel's consultants, STH](https://sthcons.com/Healthcare/details/burjeel-medical-city): a medical campus of
  distinct departments, rather than a single institutional box.
- [Burjeel interior designers, AJi](https://ajigroup.com/intl-en/projects/burjeel%20medical%20city): warm earth/blue/gold
  finishes, geometric patterns, arches and columns. Applied here through pale stone, champagne metal, fluted gallery
  columns, pointed portal frames and dry oasis basins.
- [Cleveland's architect, HDR](https://www.hdrinc.com/portfolio/cleveland-clinic-abu-dhabi): separate architectural
  forms for healthcare components, stacked masses, diamond glazing and sea/desert colours. Applied here as a low
  clinical podium, two stepped patient wings, recessed courtyards, an elevated connecting gallery and geometric fins.

No reference photographs, logos or third-party models are imported. All new architecture is original geometry using
existing credited project materials. Upgrade New Maps owns weathering, exterior nature, sky and lighting remaster.

## Footprint, massing and rooms

The 56 × 48 m bounding rectangle is no longer the building shape. The west podium occupies x=0..20,z=8..48;
the public gallery x=20..36,z=0..48; the east emergency finger x=36..56,z=0..16; and the eastern clinical finger
x=36..56,z=28..48. The missing middle creates a real courtyard open to the east. The west front is recessed too.
The existing own-ground rectangle remains unchanged; these new inset courts have their own ground colliders.

The public gallery is an eight-metre-wide, two-storey atrium, with skylight ribs, patterned portals, waiting pockets,
fluted columns and coloured floor inlays. Four-metre permanent side lanes flank its void. A curved projecting
entrance portico and tall glazed entrance establish a different silhouette from the old flat facade. Public and
ambulance doors are separate; the ambulance finger has its own lower canopy and medical cross.

Levels are basement -4, public/emergency 0, clinical 4, inpatient 8 and upper inpatient 12. Upper floors step back:
the west wing shrinks at its south and west edges, the east wing steps inward, and the two are joined by narrow
permanent bridges at z=36..40. Each actual wing edge gets a facade; there is no enclosing rectangular wrapper.
Broad glazing, deep stone fins, opaque spandrels and champagne diamond mullions replace rows of tiny windows.
Clear public glazing is excluded from baked occlusion. Patient glazing remains opaque for privacy and readable mass.

| Level | Rooms |
|---|---|
| 0 | Reception, pharmacy cage, triage, ER, MRI/radiology, supplies, ambulance bay, changing, cafeteria, chapel, kitchen, service store |
| 4 | Ward A, nurses' station, ICU, consultation, laboratory, records, recovery, Theatre A, sterile store, Theatre B |
| 8 | Isolation, Ward B, family lounge, rehabilitation, equipment store |
| 12 | Paediatrics, staff rest, patient suite, administration, day care |
| -4 | Morgue, boiler room, autopsy, basement stores |

Every room is dressed for its function. Clinical beds/headwalls, monitored triage/ER, MRI ring and patient table,
operating tables/booms, scrub sinks, stock cage, ward rows, family seating, laboratory benches, therapy bars,
children's mosaics, mortuary drawers and boiler manifolds supply the identity. Loot, door and stair reservations
precede furniture placement. Small-room loot is moved away from the triage bed and the upper west flight.

## Routes and collapse

Eight protected flights connect the floors: public west, emergency east, rear clinical east, ward west/east,
upper west/east, and morgue. Flights retain the shared solid-under-stair construction, guards and NavMesh links.
Their upper-floor openings match their runs; canonical open edges keep the landings unobstructed.

The main haul route runs from departments to the permanent gallery side lanes, then the ambulance bay and van.
The public entrance is a second broad ground exit. West/east stair routes and cross-gallery bridges form the
vertical loop. The tiny upper atrium bridge at z=20..24 is weak; permanent bridges at z=8..12 and z=36..40 remain.
Wards and selected clinical room slabs can fail. ER slabs over the morgue give a meaningful ground-floor drop.
The protected side galleries, clinical cross halls, upper connecting bridges, stair approaches and loading path
cannot collapse and have SectionKeepClear. The courtyard and van apron remain outside the building envelope.

All 36 rooms get two ordinary points, plus the radiology and Theatre A jackpot bays. The existing 37 definitions
retain their IDs, sizes and local tags. Ward points admit ordinary Huge bed frames. The standard planner retains
the two medical jackpot definitions and its 1–2 prize policy. Fifteen threat starts span all clinical levels and
basement approaches, off the arrival apron. Existing hiding supports lockers, cabinets and closets across levels.

## Collision, lighting, sound and content

HospitalSolids replaces fitted furniture's broad footprint blockers with the actual visible part meshes. Hiding
containers retain a deliberate closed occupancy shell; stair/atrium guards explicitly document their continuous
safety collision. Floor sections are never marked static. Furniture and failing gallery guards bind through
SectionProp; fixtures below failing slabs follow their actual ceiling support.

HospitalLighting uses the actual ceiling above each cell, including four clinical heights and the tall atrium,
instead of placing all roof lamps at one height. Every room receives a fixture. Six breakers cover front/rear
geographical circuits. Fifteen independent battery route lamps cover the side galleries, tower connections and
basement. Existing monitor/PA/gurney/drip sounds are rewired to the new departments. F1 keeps their markers.

The reviewer's DefaultGameObjects scene/sun, Mood, LevelAtmosphere, single LevelAmbience, LevelSolidity, second
NavMesh bake and Modifier() helper remain. Permanent shell/partitions/roofs receive occlusion flags; the builder
saves, bakes occlusion and saves again. No engine execution was performed here. The standalone review camera is
framed on the new building; the world adapter continues to hide the standalone camera.

HospitalRegistration merges only this map's local loot entries and contract location into the shared runtime
catalogs when the builder runs. It reads current catalogs then, preserves all other locations/items, and replaces
only entries matching this map's prefix/id. This addresses the reviewer's missing job/catalog notes without editing
shared catalog YAML in this task. Inspector tuning on existing map definitions/models/prefabs remains preserved.
The new location description has English plus all nine translations in hospital.txt; existing loot names remain.

## Open-world contract

The scene stays at origin. World terrain/roads move under its anchor. No root-moving script or extra truck.

- Own ground: **Rect(-12,-28,80,88)**, x=-12..68,z=-28..60; outer edge top **y=0**.
- Road mouth: south z=-28, ambulance approach x=44.
- **Hospital/VanDock (44,0,-14), forward (0,0,-1)** remains unchanged.
- New building envelope: x=0..56,z=0..48,y=-4..16. The entire own-ground terrain hole must remain, including basement.
- Courtyard paving and the recessed public forecourt sit inside the existing rectangle. No new roads, horizon, sky,
  trees or outside settlement are authored. All new visible mass is part of the playable hospital.

## Decisions CLAUDE.md should record

The Hospital is an original Burjeel/Cleveland-inspired medical city: irregular podium, tall public atrium and two
stepped patient wings, four clinical levels plus basement. Protected side galleries and eight flights support
salvage routes. Keep both world parking pose and ground rectangle. Public glazing never occludes. Owned builders
merge their own content into shared catalogs at rebuild time. This task does not compile or run Unity.

## What to check first

For Claude CLI only; not performed by this agent:

1. Rebuild Hospital then Upgrade New Maps. Inspect the crescent portico, stepped towers, real east courtyard,
   broad glazing, geometric fins, upper bridges and tall gallery; the old square shell must be absent.
2. Walk all eight stairs, particularly west 4→8 and 8→12, both east switchback flights, and the morgue opening.
3. Reach all 36 rooms and both sides of the atrium; review clear wide doorways, four clinical ceiling heights,
   triage/ER fitting omissions, tower bed placements and wall/ceiling finishes.
4. Carry the X-ray, theatre workstation and bed frame through the stair/side-gallery/ambulance routes.
5. Collapse the weak atrium bridge, a ward and ER-over-morgue slab; review guard/prop/fixture disappearance,
   permanent alternatives, rubble clearance, nav carving, saved occlusion and restoration.
6. Review furniture mesh collision, intentional guard/hiding shells, all six circuits, battery lamps, sounds,
   local translations and the merged job/loot catalog entries.
7. Review host/clients and world streaming at the unchanged VanDock and ground rectangle. No execution result
   is claimed for this implementation.
