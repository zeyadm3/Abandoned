# HQ Hub Overhaul — 0.14.5

The headquarters is a working Abandoned Salvaging warehouse: dirty steel furniture, sorted scrap,
stacked crates, retired electronics and maintenance supplies. The existing 24 × 16 m building,
garage door placement, four crew spawns and van remain the basis of the layout.

## Stations

- **Dispatch workbench:** a heavy steel bench in the middle of the office. The rotary telephone opens
  the existing host-controlled contract selection and briefing. Original spoken dispatch warnings
  accompany a readable transcript; the selected contract supplies its exact quota, threat, power,
  stability and time window. The boss's answering machine sits at the back of the same bench.
- **Field guide:** a scuffed leather logbook immediately beside the telephone. Interact opens a
  paper-and-leather monster manual with an index and previous/next page controls. It uses the existing
  encounter unlocks and threat descriptions; unread subjects remain unknown.
- **Scrap and quota depot:** a rusted platform scale and reinforced receiving chute against the
  garage's north wall. The scale displays the company's receipt; the depot screen includes current
  quota, the previous settled haul and its weight/count where available. Legitimate loose HQ salvage
  can be unloaded at the chute through the host. Haul already credited on return is never paid twice.
- **Maintenance and gear:** a steel workbench with the supply terminal, tool pegboard, spare
  flashlights, battery chargers, tool cases and the inventory rack together in the workshop.
- **Logistics corkboard:** taped mall floor plans, wiring sketches, handwritten instructions and
  hazard notices along the office wall, including reminders about noise, sight and overloaded floors.

## Warehouse dressing and atmosphere

Wall stock includes wooden crate stacks with bracing, rusted storage shelves, decommissioned electronic
racks, copper pipe bundles, oil drums and labeled plastic bins. Clutter stays at the room edges and
leaves the dispatch approach, doorways, van and spawn area open by authored placement.

Cool fluorescent shop lights and exposed warm bulbs light the working stations. Two secondary tubes
use the existing faulty-fixture behavior, while the main task lights remain steady. Localized particle
systems add dust in the light pools and faint shafts. Atmospheric meshes are explicitly walk-through.
The existing reduced-flashing setting and runtime shadow budget continue to apply.

## Authoring

In Unity, use **Tools → Abandoned → HQ → Rebuild Salvage Hub (0.14.5)**. This generates and saves
`Assets/_Project/Scenes/HQ.unity`, creates the HQ-owned art assets and installs the dispatch clips in
the existing sound bank. It skips the collider validation/audit passes. The usual Create HQ and
whole-project content builders also include the redesign.

Batch authoring entry point: `Abandoned.EditorTools.HqBuilder.AuthorSalvageHub`. This is generation
only, with no tests, screenshots or validation passes. Do not use `Tools/unity.sh all` for authoring
this change: that command explicitly performs verification and tests.

The original dispatch speech is authored locally with the installed macOS speech voice; the checked-in
WAV clips play through GameAudio on every supported platform. Regenerate those clips with the dedicated
`Tools/Audio/generate_hq_dispatch.py` authoring script if the dispatch wording changes.

## Handoff

The dedicated authoring command completed and saved the redesigned HQ scene and generated assets.

The user requested that tests and verification be left to them. No test suites, gameplay sessions,
network tests, QA captures, audits or verification commands were run for this redesign. The first-day
guide and HQ hints now direct players to the telephone, adjacent book and maintenance bench.
