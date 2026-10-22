# Saint Brigid Hospital — departmental expansion, 2026-10-23

Implementation only. No compile (including dotnet), Unity invocation, game run, tests, verification, screenshots,
scene generation, navigation bake or occlusion bake was executed by this agent. Claude CLI owns execution/review.
The existing entry point remains `Abandoned.Hospital.HospitalBuilder.Build` and its namespace is unchanged.

## Layout and carry decisions

A 56 × 48 m county hospital, with clinical floors at y=0/4 and a service basement at y=-4. Layout is now data in
HospitalLayout, using the station's four-metre plan/floor/wall/facade helpers, read-only. The station paths were not
edited. Floors, door openings, stairs, materials and room dressing therefore share one plan.

| Floor | Distinct spaces | Character and route |
|---|---|---|
| Ground | Reception, triage, ambulance bay | Public benches and screened desk; monitored triage bays with gathered curtains; open vehicle door, supply counter and lockers |
| Ground | Emergency, radiology/MRI, pharmacy, supplies | Four treatment bays; a bolted ring magnet and patient table with control desk; stock behind a cage with a 3.4 m opening; storage racks |
| Ground | Chapel, cafeteria, kitchen, staff changing | Pews and altar; tables/chairs and vending; commercial cooking/service fittings; usable lockers and wash area |
| Upper | Two wards over reception/triage, two operating theatres | Bed rows with headwalls; weak slabs over the lobby; fixed operating tables, articulated lamp booms, scrub sinks and instrument counters |
| Upper | ICU, nurses' station, sterile room, recovery | Monitored beds; working desks; sterile stock; recovery beds |
| Upper | Laboratory, records, staff rest, consultation | Small independent rooms between two cross halls, each with its own loot and fittings |
| Upper | Isolation, paediatrics, administration, equipment store | Clinical beds and dividers; coloured wall panels; office desks; medical storage |
| Basement | Morgue, boiler room, autopsy, stores | Mortuary drawers, examination table/sink, paired boilers/gauges/manifold, racks; cross corridor back to the morgue stair |

Three-metre permanent clinical stairs connect both ends of the upper floor. A third permanent stair goes from the
west circulation strip to the basement. Slab holes match the full eight-metre runs; their low ends are filled so
players cannot enter a headroom trap. Flights have guard colliders and bidirectional NavMesh links.

Main haul route: department → protected cross hall → main spine → ambulance bay → south loading apron. The ambulance
vehicle opening is 6 m; departmental haul doors are 3.2 m. The rear escape and both clinical stairs give alternatives.
The pharmacy cage never relies on power or a closed gate to release a refrigerator. No collection truck is authored.

## Structure and fair collapse

Upper-room slabs can fall; Ward A above reception and Theatre A have weaker authored health/capacity. Selected ER
slabs above the morgue also fall, making the basement matter to collapse. Basement slabs sit on earth and stay solid.
All protected cross halls, the central spine, ambulance carry lane, basement cross corridor and basement spine have
SectionKeepClear; their keys are removed from Collapsible as well. Protection therefore covers both the route slab
and rubble buildup. Stair flights are permanent and carry SectionKeepClear.

Furniture on failing slabs uses SectionProp. Ground/basement ceiling fixtures bind to the slab above, so the light
below a lost ward/ER tile goes with it. Walls, permanent shell and roof are marked OccluderStatic by the station
helpers. Floor slabs are never marked static. The builder saves, calls OcclusionBake.Bake, then saves again; only the
reviewer will execute this. Existing LevelSolidity and the second NavMesh bake, including Modifier(), are retained.

## Loot, hiding and pressure

Thirty-seven map-local LootDefinitions preserve the first nineteen definitions and add eighteen medical/service
items. New additions include a thermometer, bone saw, two-crew rescue stretcher, three-crew antique bed frame,
mortuary scale/specimen case/examination lamp/pathologist's log, pressure gauge, copper exchanger, valve/tool chest,
altar cross/hymnal/communion case and cafeteria urn/mixer/cutlery. All map tags remain prefixed `hospital_`.
The existing mobile X-ray and anaesthetic workstation are the two jackpot definitions; the standard planner picks
the run's 1–2 prizes. They have separate reserved bays in radiology and Theatre A. Ordinary ward points now admit
Huge loot, making the heavy bed frame actually eligible. Every one of the 31 rooms has two authored ordinary points.

Loot bays and large-item turning space are reserved before dressing; the station placement helper leaves out any
fitting that conflicts with loot, a doorway, a stair or another fitting. Inspector tuning on existing definitions
and generated models/prefabs remains preserved. New large frame/stretcher, cross, mixer and exchanger/urn silhouettes
are authored separately from old category models. The network catalog, run-state clone and registration stay local.

Hiding candidates include five staff lockers, ward/isolation closets, theatre/office cabinets and three basement
hiding places, plus reception/ER/radiology/pharmacy/chapel spots. Only supported HideSpotKind values are used;
under-bed hiding was not invented. Existing host-authoritative HidingLocker handles them. Fifteen monster markers
cover ground circulation, upper halls and basement approaches, avoiding the arrival apron and occupied fitting bays.

## Power, light and sound

The plan's rooms/halls receive the station's ceiling fixtures; lower fixtures follow upper slabs. Six breakers are
spread across the front and rear wings, with numbered cabinets. HospitalHorror uses SectorCell (20,100,24): three
front geographical circuits and three rear circuits, with the existing formula handling basement membership.
Nine green battery route lamps on the upper/ground main spine and basement corridor bypass mains power. Their
HospitalEmergencyLight component still turns them off if their supporting slab falls. No shadow lights are added
by these emergency lamps.

HospitalSoundscape uses a map-owned HospitalSoundConfig: soft monitor pulses using VoicemailBeep, PA RadioStatic,
a moving sequence of LootRattle wheel sounds across two distant gurney markers, and morgue WaterDrip loops.
Powered cues follow nearby LightFixture state; drips/wheels do not. All audio uses GameAudio and existing SoundIds;
these cosmetic sounds never emit gameplay NoiseSystem events. F1 draws all sound markers and the gurney path.
The reviewer's LevelAmbience sound bed remains in the builder; the old duplicate services sound bed was removed.

## Art, text and code

Distinct room sets use original geometry and the existing station furniture primitives. Existing hospital scans,
station PBR surfaces, original procedural factory materials and the existing credited sound bank/font are reused;
there are no new downloads or third-party assets. Art > Upgrade New Maps applies the reviewed sky/scanned nature,
weathering and lighting chain afterwards. No sky, grass, trees or horizon ring is authored here.

The reviewer's DefaultGameObjects scene, Mood, LevelAtmosphere.Apply, LevelAmbience, LevelSolidity/second bake and
navigation Modifier() are preserved. All Hospital editor files were rewritten/formatted into statements and blocks
with braces on their own lines. Nested Item remains data only. New runtime files are HospitalSoundscape,
HospitalSoundConfig and HospitalEmergencyLight, named Hospital* under Scripts/Audio and Scripts/Structure, explicitly
authorized by this task; the existing HospitalNetworkContent remains necessary for runtime prefab registration.
Hospital loot/location wording is supplied in Data/Text/Resources/Text/hospital.txt in English plus all nine shipped
translation languages. The text check was not executed because verification was prohibited.

## Open-world fit (moving origin)

The Hospital scene is never translated. It loads at its authored origin with absolute configs and a baked NavMesh;
the world moves its terrain/roads around the current anchor. No root-moving script or second truck is added.

- Own ground rectangle: **x=-12..68, z=-28..60** (`Rect(-12,-28,80,88)`). Ground top at every outer edge: **y=0**.
- World road connection: **south edge, z=-28**, aimed at the ambulance approach x=44.
- Van parking/loading pose: **(44,0,-14)**, forward **(0,0,-1)**, `Hospital/VanDock`.
- Interior footprint: x=0..56, z=0..48. World terrain must be absent across the whole own-ground rectangle so it
  cannot fill the basement. The map itself has no slab of exterior ground beneath the building.
- No standalone horizon/sky geometry exists to hide. The adapter already handles the scene camera, sun and global
  atmosphere volume; those remain useful for the reviewer's standalone scene review.
- Resource prefix Hospital/Network and location id hospital remain unchanged. Register map prefabs before NGO starts.
  Existing world readiness/adapter code owns activation and persistent van cargo. Its paths were not edited.

## Decisions CLAUDE.md should record

Hospital is a three-level authored salvage location with an ambulance carry bay, not a post-Early-Access placeholder.
POIs stay at origin; world terrain moves. Runtime Hospital* scripts in the allowed areas are authorized by the user.
The world owns the sole cargo van. New-map sky/nature/remaster stays in Upgrade New Maps. All execution is reserved
for Claude CLI; no claim of a compile, bake, navigation audit or playtest is made here.

## What to check first

For Claude CLI only:

1. Compile, rebuild Hospital, and run Upgrade New Maps; inspect the station-based room shell, ceilings and practical light.
2. Reach the basement using the west morgue stair and return with the mortuary scale/exchanger; inspect the full stair
   hole, headroom, lower landing and terrain opening.
3. Carry the X-ray from radiology, workstation from Theatre A and bed frame from Ward A through the ambulance bay.
4. Collapse wards above reception and ER slabs above the morgue. Check dressing, fixtures, rubble, navigation carving,
   the protected alternative route and restoration on reroll.
5. Review ordinary loot in every room, both prize bays, several hiding kinds, all breaker circuits, battery lamps
   during mains failure, and monitor/PA/gurney/drip sounds with F1 markers.
6. Review saved occlusion with floors excluded, localization in all nine languages, and host plus three clients.
7. Stream through the world at origin, using the stated ground rectangle/edge height and van pose; review readiness,
   cargo persistence and unload cleanup. This agent performed none of these checks.
