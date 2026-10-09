# HQ Hub Overhaul — 0.14.5b

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
| Company stock | Warehouse rear | Tall loaded pallet racks, stock aisles, reclaimed parts and metal/e-waste skips |
| Dispatch office | East wing front | Central metal desk, rotary telephone, leather field guide, voicemail and logistics board |
| Crew facilities | East wing middle | Wardrobe lockers, lunch seating, time clock, water station and PPE |
| Maintenance bay | East wing rear | Workbench, tools, charging batteries, supply terminal and inventory rack |

The main warehouse has a continuous haul route alongside the stock aisles. A separate service spine
connects dispatch, crew facilities and the workshop through wide framed openings. Painted lane edges
and equipment placement establish the work sequence: receive, weigh, sort, store, repair and dispatch.
The yard includes a loading canopy, fleet parking, separate pedestrian approach and a waste collection pad.

At the user's request, 0.14.5b removes all environmental lettering: signs, floor words, prop labels,
book titles, clock labels, the physical scale readout and handwritten sentences on the corkboard.
Color blocks, papers, schematics and arrows remain. Functional menus and proximity prompts retain text.

## Functional stations

The staff entrance behind the spawn area has a locked steel double door that physically closes the
opening. Outside, the company occupies an industrial estate with vacant warehouse and office blocks,
closed loading shutters, roads, utilities, trees and a distant wooded horizon. These surroundings keep
views through glazing grounded in a larger world. They add no environmental lettering.

- **Dispatch:** the rotary telephone opens the existing host-controlled contract selection and plays
  original condition briefings. Exact quota, threat, power, stability and time-window details remain
  in its readable transcript. The boss's answering machine stays on the same desk.
- **Field guide:** the worn leather logbook beside the phone opens a book-style monster manual. Its
  index and page controls use the existing encounter unlocks and survival prose.
- **Depot:** interacting with the industrial scale opens haul weight/value, quota and settlement
  receipts. Loose physical HQ salvage can be unloaded through the host; an already settled truck
  haul is never credited twice.
- **Maintenance:** the supply terminal and inventory rack retain gear and upgrade functions. Crew
  wardrobe lockers have their own room instead of sharing the loading floor.

Dispatch has filing cabinets, shift paperwork and visitor seating. The logistics corkboard carries
mall plans and wiring sketches. Company stock is organized into actual aisles;
clutter and recovery equipment occupy designated work cells.

## Atmosphere and authoring

Suspended high-bay fluorescents light the warehouse. The operations rooms have lower fittings and
warm task pendants. Two secondary tubes retain faulty-fixture behavior; work surfaces stay steady.
Rooflight shafts and localized dust follow the taller building. Air effects remain explicitly
walk-through; existing reduced-flashing and runtime shadow-budget behavior still applies.

Use **Tools → Abandoned → HQ → Rebuild Salvage Hub (0.14.5b)** to regenerate and save
`Assets/_Project/Scenes/HQ.unity`. The shared floor plan lives in `Editor/HQ/HqLayout.cs`; architecture,
operational stations, dressing and lighting use its anchors. The dedicated batch authoring entry point
is `Abandoned.EditorTools.HqBuilder.AuthorSalvageHub`; it skips validation and collider audits.
`HqWorldText.Remove` removes generated TextMesh objects after dressing so future rebuilds omit lettering.

To add the staff door and estate surroundings to an existing HQ while retaining hand-edited interior
placements and materials, use **Tools → Abandoned → HQ → Add Staff Door and Exterior**. The batch
entry point is `Abandoned.EditorTools.HqExteriorAuthoring.AuthorExterior`; it edits the saved HQ in place.

Original dispatch WAV clips are included in the shared SoundBank. The reproducible local speech
authoring script is `Tools/Audio/generate_hq_dispatch.py`.

The user will handle testing and verification. No test suites, gameplay checks, network tests,
QA captures, audits or verification commands were run for this layout revision.
