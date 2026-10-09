# HQ Hub Overhaul — 0.14.5

Abandoned Salvaging now occupies a 36 × 28 m industrial company building: a high-bay receiving,
sorting and stock warehouse, with an attached lower operations wing. Warehouse eaves are 6.4 m;
the pitched roof reaches 8.2 m. Steel portal frames, rooflight bands, brick plinths and corrugated
cladding establish the industrial scale. The office wing has its own staff entrance and glazed
views into the receiving floor.

## Floor plan

| Area | Location | Purpose |
| --- | --- | --- |
| Receiving and vehicle dispatch | Warehouse front | Van, industrial scale, deposit chute, marked goods-in apron |
| Recovery and sorting | Warehouse middle | Sorting island, graded bins, vice, gantry, pallet jack and parked forklift |
| Company stock | Warehouse rear | Tall loaded pallet racks, numbered aisles, reclaimed parts and metal/e-waste skips |
| Dispatch office | East wing front | Central metal desk, rotary telephone, leather field guide, voicemail and logistics board |
| Crew facilities | East wing middle | Wardrobe lockers, lunch seating, time clock, water station and PPE |
| Maintenance bay | East wing rear | Workbench, tools, charging batteries, supply terminal and inventory rack |

The main warehouse has a continuous haul route alongside the stock aisles. A separate service spine
connects dispatch, crew facilities and the workshop through wide framed openings. Painted lane edges,
numbered hanging signs and department labels make the work sequence visible: receive, weigh, sort,
store, repair and dispatch. The yard includes a loading canopy, fleet parking, separate pedestrian
approach, company facade signs and a waste collection pad.

## Functional stations

- **Dispatch:** the rotary telephone opens the existing host-controlled contract selection and plays
  original condition briefings. Exact quota, threat, power, stability and time-window details remain
  in its readable transcript. The boss's answering machine stays on the same desk.
- **Field guide:** the worn leather logbook beside the phone opens a book-style monster manual. Its
  index and page controls use the existing encounter unlocks and survival prose.
- **Depot:** the industrial scale and reinforced chute show haul weight/value, quota and settlement
  receipts. Loose physical HQ salvage can be unloaded through the host; an already settled truck
  haul is never credited twice.
- **Maintenance:** the supply terminal and inventory rack retain gear and upgrade functions. Crew
  wardrobe lockers have their own room instead of sharing the loading floor.

Dispatch has filing cabinets, shift paperwork and visitor seating. The logistics corkboard carries
mall plans, wiring sketches and crew hazard notes. Company stock is organized into actual aisles;
clutter and recovery equipment occupy designated work cells.

## Atmosphere and authoring

Suspended high-bay fluorescents light the warehouse. The operations rooms have lower fittings and
warm task pendants. Two secondary tubes retain faulty-fixture behavior; work surfaces stay steady.
Rooflight shafts and localized dust follow the taller building. Air effects remain explicitly
walk-through; existing reduced-flashing and runtime shadow-budget behavior still applies.

Use **Tools → Abandoned → HQ → Rebuild Salvage Hub (0.14.5)** to regenerate and save
`Assets/_Project/Scenes/HQ.unity`. The shared floor plan lives in `Editor/HQ/HqLayout.cs`; architecture,
operational stations, dressing and lighting use its anchors. The dedicated batch authoring entry point
is `Abandoned.EditorTools.HqBuilder.AuthorSalvageHub`; it skips validation and collider audits.

Original dispatch WAV clips are included in the shared SoundBank. The reproducible local speech
authoring script is `Tools/Audio/generate_hq_dispatch.py`.

The user will handle testing and verification. No test suites, gameplay checks, network tests,
QA captures, audits or verification commands were run for this layout revision.
