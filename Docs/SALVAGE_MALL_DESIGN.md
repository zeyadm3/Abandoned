# Salvage mall zone design

The 0.15.5 authoring implementation now includes original salvage model recipes, static dressing, registered loot definitions/prefabs, dedicated inventory markers, both Nordic Habitat floors and mall worksite atmosphere. The descriptions below preserve the level-design intent; placement quantities are starting art budgets, not promises that every listed object will appear as physical loot in every run. The existing shell, front-only extraction and approved district remain the basis of the build. See the [asset breakdown](SALVAGE_ASSET_BREAKDOWN_0.15.5.md) for individual model/material/prefab specifications and the [Unity setup guide](SALVAGE_MALL_UNITY_SETUP.md) for the actual builder hierarchy, authoring menu, item tagging, lighting, collision and navigation. The user will perform tests and visual QA after construction.

## Existing constraints and coordinate system

The current 0.15 layout has a 56 × 48 m rectangular footprint, fourteen by twelve 4 m structural tiles, ground floor at Unity Y = 0, upper retail floor at Y = 4, and an internal basement at Y = −4. X increases east; Z increases toward the rear. The front facade is Z = 0. Its two adjoining opening panels make one continuous 8 m entrance, X = 24–32. The existing front payout truck has its rear at (36, 0, −11); the arrival van is at (19, 0, −15). Keep this frontage, two-storey envelope, food court, internal service core and surrounding district intact. Every recovered item leaves through the same front opening and goes into the existing truck cargo bay.

Sources: `Game/Assets/_Project/Editor/Mall/MallLayout.cs` lines 10–11, 59–67, 93–104; `MallLayout.Plan.cs` lines 9–24, 26–64. `CLAUDE.md` records the approved replacement and no-tests/user-review policy. Older 0.13 floorplan renders in Docs are superseded by this geometry.

## Tenant allocation

Reassign the existing store footprints rather than add new building wings or exterior loading doors. Nordic Habitat occupies two vertically aligned floors in the front-right bay: the ground `Jewelry` footprint becomes its heavy furniture showroom, and the upper `Offices` footprint becomes its office furniture and sample department. Move the closed offices into the former upper `Furniture` footprint on the west side. Heavy furniture stays on the ground near extraction so cargo handling and forklift tire marks remain credible.

| Zone | Proposed tenant | Existing footprint | Proposed bounds in metres | Existing doorway centre |
| --- | --- | --- | --- | --- |
| A | Apex Circuit, with a faded earlier “Volt & Byte” logo on one counter panel | Ground `DeadCircuit` | X 0–24, Z 0–16; exclude X 16–24, Z 12–16 where court takes precedence | (24, 0, 6) |
| B, ground | Nordic Habitat — heavy furniture showroom | Ground `Jewelry`, rethemed as furniture | X 32–56, Z 0–16, Y = 0; exclude X 32–40, Z 12–16 where court takes precedence | (32, 0, 6) |
| B, upper | Nordic Habitat — office furniture and samples | Upper `Offices`, rethemed as furniture | X 32–56, Z 0–16, Y = 4; exclude X 32–40, Z 12–16 where gallery takes precedence | (32, 4, 6) |
| C | Titan Tools & Industrial | Ground `Security`, rethemed as hardware | X 40–56, Z 16–32 | (40, 0, 22) |
| D | Vogue Thread | Ground `Threadbare` | X 0–16, Z 16–32 | (16, 0, 22) |
| E | Solutions Inc. Clearance Division staging hub | Ground `GrandAtrium` and entry promenade | Court X 16–40, Z 12–36; entry X 24–32, Z 0–12 | Main front opening (28, 0, 0) |
| Closed offices | Vacated mall administration | Upper `Furniture`, rethemed as closed offices | X 0–16, Z 16–32, Y = 4 | (16, 4, 22) |

Bounds are half-open at tile boundaries. A has 352 m² after its 32 m² court notch; B has 352 m² on each floor, 704 m² total. C, D and the relocated closed offices each have 256 m². The 16 × 16 m upper atrium opening spans X 20–36, Z 16–32, with the existing 4 m-deep bridge at Z 24–28. Never author an upstairs pallet onto this void. Preserve other upper tenants as secondary mall dressing and preserve the rear food court X 0–44, Z 36–48.

Ground plan, front at the bottom:

```text
REAR Z48      FOOD COURT / DINING                  INTERNAL SERVICE CORE
              X0–44, Z36–48                        X44–56, Z32–48
Z32           D: VOGUE THREAD     E: COURT          C: TITAN TOOLS
              X0–16, Z16–32       + FOUNTAIN        X40–56, Z16–32
Z16           A: APEX CIRCUIT     E: ENTRY          B: NORDIC HABITAT
              X0–24, Z0–16       X24–32            X32–56, Z0–16
FRONT Z0                         8 M OPENING
                                 ↓ front court → PAYOUT TRUCK
```

The drawing is schematic: court notches remove the inner rear corner of A and B. Exact bounds above are authoritative.

On the upper floor, B sits directly above ground B in the same front-right bay. Its entrance faces the existing entry gallery at (32, 4, 6). The former west-side furniture bay, X 0–16, Z 16–32, becomes the closed offices. This is a tenant swap within existing rooms; no new floor slab, store opening or vertical circulation geometry is implied.

## Routes, pacing and cargo flow

Arrival should reveal the staging lights, dry fountain and two facing clearance routes in one view. A supplies quick light-value pickups, B introduces large awkward carries, C provides useful dense metal, and D offers light but bulky bundles. This yields different choices without treating every store as the same pile of random scrap.

Keep the ground ring around the inner court continuous: west X 16–20, east X 36–40, south Z 12–16 and north Z 32–36. Preserve the centre lanes of permanent haul tiles at a minimum 2.4 m clear width, the full 8 m front opening and existing wide storefront mouths. Store aisles at Z 4–8 connect the A/B doors; aisles at Z 20–24 connect C/D. Reserve a 4 m ground-floor forklift aisle where each store's footprint permits it, retaining the 2.4 m protected minimum through existing constrained frontage and carry-route sections. Give conference tables generous turn pockets at door approaches.

The fast loop is front entry → A or B → staging → front truck. The deeper loop is entry → west/east ring → D/C → north ring → food court → return via the other side of the court. Do not invent doors between adjacent stores: their existing court-facing doors provide circulation. High-value side bays tempt players off the permanent carry lane into structurally vulnerable display tiles.

Both floors of B use the mall's existing vertical circulation. Scout the upper showroom via the escalators at X = 22 and 34, from Z = 16 to 24, ground to Y = 4, then follow the upper gallery ring south to the entry gallery and the B door at (32, 4, 6). Use matching department signs at the two store doors and the gallery approach so this reads as one two-storey retailer. The current plan adds no in-store stair: the adjacent concourse routes already connect both floors.

Recovered upper B items leave through (32, 4, 6), move north along the entry gallery to the south gallery at Z = 14, turn east into the gallery lane X 36–40, then travel north to Z = 34 and east along the rear gallery into the service core. Descend the two 4 m-wide switchback ramps, then follow the east ground ring to the front promenade. The ground-to-upper ramp A runs (49.65, 0, 36) → (49.65, 2, 44), the half-storey landing joins X 49.65 to 53.85 at Z = 44, and ramp B runs (53.85, 2, 44) → (53.85, 4, 36). Descend B then A. Each segment rises 2 m over an 8 m horizontal run (25% grade); upper recovery uses manual dollies/shared carrying on this route. Forklifts and their tire tracks stay on the ground floor. The permanent stair begins at (45.55, 0, 36), with the internal basement stair at the same X/Z and Y = −4.

Keep at least 2 m of approach free at both ends of each flight. Gate demolition side bays with short fence returns parallel to travel; never span the single exit or create a required crawl obstacle. The E staging area is a temporary sorting location. Payment still occurs only through the existing truck and extraction flow.

## Zone A: Apex Circuit / Volt & Byte

Make the concourse-facing display almost empty and the deep western stock space relatively intact. Three retail display islands should be broken at different stages: one intact case with missing merchandise, one smashed case with broad glass decals, and one stripped metal frame. Two wall rack runs retain bent brackets and cable loops. Keep hanging cables above 2.2 m on routes; drop a few low loops in a closed side bay. Use a partial-height mesh cage and disassembled shelving to imply a backroom inside the existing footprint, avoiding an invented inaccessible structural room.

Place 5–7 PC towers along one stripped workbench and in two wheeled cages, 3 motherboards and 4 loose circuit boards in antistatic trays, 2 CRT monitors low on a trolley, 3 LED monitors on damaged brackets/countertops, 2 server racks in the western stock recess, 2 copper spools below the workbench and 2 industrial battery blocks on a pallet. Representative backroom anchor is X 2–8, Z 9–14; display anchors occupy X 10–20, Z 1–3 and Z 9–11. Keep Z 4–8's protected aisle clear.

Tech is the visual signature; copper is Material and the industrial batteries are Tech. A single rear cage door reads `HOLD - RECOVER HIGH VALUE`; the abandoned till still holds receipts and one inventory scanner plugged into a dead socket. A severed ceiling grid panel directly above the packed cage explains why demolition preceded recovery.

## Zone B: Nordic Habitat — two-storey furniture store

Treat the aligned ground and upper bays as one tenant with the same Nordic Habitat fascia, colour palette, glazed display rhythm and clearance labels on both floors. Align the storefront signs vertically and use a shared graphic band on the front glazing to carry the retailer's identity across the two storeys. Preserve the structural slab between floors. Ground signage reads `NORDIC HABITAT / FURNITURE`; upper signage reads `NORDIC HABITAT / OFFICE & SAMPLES`, with simple directions to the existing concourse escalators.

On the ground floor, arrange three room-set islands with waist-high partitions around a broad line from the store door toward the eastern stock edge. Use 2 half-disassembled flat-pack desks, 3 tipped executive chairs, 2 shrink-wrap rolls and one heavy-duty dolly abandoned with a skewed load. A 4 m ground-floor forklift loading aisle and a 5.5 × 5.5 m cargo turning pocket distinguish the heavy-haul department; tire tracks should curve from this aisle toward the concourse, never through solid counters.

Place 3 steel filing cabinets against the east wall, 4 ergonomic chairs around one partly stripped office set, 1 heavy oak conference table in a deep side bay, 3 modular shelving units in the stock perimeter and 3 fabric rolls on a fabric sample stand. Keep one wrapped table awaiting removal near the frontage but outside the entrance approach. Place the oak table parallel to its planned exit orientation so players can read its handles and packing path.

A plastic sheet hung across one dismantled partition catches fine dust; behind it, removed drywall reveals steel studs and intact inventory pressed against them. The tag `ZONE B - CLEAR FOR DEMO` belongs on that dismantled partition's side-bay gate, while a smaller `HOLD - RECOVER HIGH VALUE` tag hangs on the table. Their adjacency sells rushed, conflicting clearance instructions.

The upper floor is a complete office furniture and sample department, with 2 partly dismantled desk displays, 4 light ergonomic chairs, 2 stripped modular shelving units and 3 fabric/sample rolls placed along the perimeter. A counter holds material swatches and abandoned order sheets; a hand dolly and half-packed sample crates show recovery working toward the gallery door. Keep the existing Z 4–8 aisle and the approach to (32, 4, 6) clear. Use the same showroom partition language as downstairs, with lighter furniture, shallow sample stands and no conference-table jackpot or loaded filing-cabinet bank. Ground-floor heavy pieces supply the large-haul challenge, while upstairs remains credible to recover along the permanent service ramps. Do not add upstairs forklift paths or marks.

In the relocated west upper offices, reuse the existing footprint and doorway at (16, 4, 22) for a closed administration suite. Draw blinds across the frontage and dress it with vacated desks, boxed files and a locked-looking entrance treatment. It is secondary abandonment dressing, with no Nordic Habitat signage, sample stock or required B haul route through it. Any later functional closure must respect the existing door and circulation setup.

## Zone C: Titan Tools & Industrial

Keep the entry line open, with the protected store aisle at Z 20–24. Concrete, grated service panels, yellow/black markings and two chain-link crib sections replace glossy retail fixtures. Cage doors stand open into recesses, with no moving door across the carry lane. Anchor the cribs along X 49–55, Z 25–31; reserve X 41–44 for the visible entrance side display and keep the centre lane open.

Place 2 hydraulic jacks low beside lifting stands, 2 portable generators (one sale inventory, one clearly crew-owned work unit), 3 angle grinders on a half-cleared tool bench, 2 tool chests, 2 tied bundles of scaffolding pipes and 3 safety harnesses hanging from cage hooks. Copper/conduit offcuts can enter Material bins, but tool inventory stays Hardware. Generators used by the crew should have a distinct visual marking and no loot outline so salvage power sources are not accidentally presented as sale stock.

A dust-covered teardown trolley with one loose grinder remains at a bench; removed conduit points toward the staging power hub. The heaviest pile should sit outside the protected aisle, with nothing protruding into ramp approaches. Keep a compact forklift service kit near the cage to connect this store's former use with the clearance team's activity.

## Zone D: Vogue Thread

Use 4 stripped mannequins along the storefront and stock wall, 4 garment racks pushed into two perimeter banks, and one overturned cash-wrap counter in a side pocket. Aim for a 4 m-wide former shop aisle where fittings permit it, with a pair of tire tracks and displaced racks reading as a newly repurposed forklift path. Tie the operation to the same central staging hub through marks and material labels.

Place 4 compressed textile bundles on two pallets, 2 security-tag detachers on/under the fallen cash wrap, 1 floor safe partly exposed where a changing-room partition was removed, and 2 commercial laundry bins against the stock edge. Textile bundles are Material; safe and detachers are Hardware. A partly filled bin still containing untagged garments shows that tenant pack-out stopped mid-task.

Demolition is concentrated near the stock wall: 2–3 m of missing drywall, exposed framing and a severed low-voltage conduit. Put demolition netting on the side recess rather than across the aisle. A discarded tenant trolley with folded clothing sits next to industrial clearance crates to distinguish the two interrupted operations.

## Zone E: concourse and staging

Retain the existing dry fountain at (28, 0, 29) as a legible retail landmark, with shallow scrap piles inside its basin. Use the central pad approximately X 25–31, Z 19–23 for a compact pallet sorting island between the escalator approaches; final placement must respect the actual prop bounds and reserved spawn footprints. The present kiosk at (28, 0, 17.8) can be dressed as the company's check-in/sorting table or repositioned within this pad at authoring time if its geometry conflicts. Do not fill the ring or the front promenade with stacks.

Start with 6 heavy wooden shipping pallets (3 loaded, 3 nested), 2 compact industrial dumpsters against the basin's unused outer edge or an approved side pocket, 2 hydraulic pallet jacks, 5 labeled cargo crates, 2 mobile floodlight masts and 1 visually crew-owned generator. Align pallet fork openings with the outgoing haul direction. Keep loaded stack height below 1.4 m along court sightlines. Scrap in the fountain is debris/material dressing until explicitly tagged as recoverable; a basin does not automatically make all its contents loot.

Board or visually fracture two skylight panes, retaining a sealed backing and the roof's physical closure so this creates no additional exterior route. Distribute glass decals beneath their projection and put a heavy canvas dust sheet over the threatened sorting stack. Aim the mobile lights across sheet folds and fine airborne dust, with their bases outside travel paths. Crates use `TECH / TEST`, `METAL / WEIGH`, `TEXTILES / BALE` and `HOLD / FRAGILE` as clear classification labels. Add two stacks of three plastic classification crates, one at the tech sorting edge and one by the textile stack. Put company identification on one operations board and a few owned equipment decals, preserving the mall's legibility without repeating slogans. A localized generator hum, occasional pallet wheel rattle and intermittent zip-tie/cable flutter make the active hub audible before it becomes visible; feed these through the existing sound system when implementation is authorized.

## Environmental storytelling placement rules

1. Build three layers in each store: former retail order at its perimeter; interrupted tenant removal at a cash desk or trolley; industrial clearance overwriting it with pallets, barriers and tags. Use matching damage/packing stages across a few grouped props rather than random rotations everywhere.
2. Place dust sheets on two vulnerable asset groups per department, with one corner dragged loose. Use one or two torn orange-net sections per store around demolition recesses. Fence returns should terminate against solid walls or defined side bays, never in a vague dead-end corridor.
3. Use one readable status tag per doorway/gated bay at 1.4–1.7 m height; handwriting or stencils go on door skins and mounted boards. Primary text is `HOLD - RECOVER HIGH VALUE`, `ZONE B - CLEAR FOR DEMO`, `SALVAGE FIRST` or `LIVE TEMP POWER`. The contradictory B tags are specific narrative evidence, not general wayfinding.
4. Keep zip-tied cable bundles along wall bases; where one crosses a walkway, pass it through a low cable ramp with a smooth proxy collider or use purely visual flat geometry. The cable leads visually back to the same generator hub instead of disappearing under arbitrary debris.
5. Apply dust as a coherent fall pattern under broken ceiling/wall sections. Forklift tire tracks cut through that same deposit and stop at actual loaded pallets. Leave cleaner rectangles where inventory was removed. Flat dust/glass/bag scraps should not individually collide with players.
6. Loose debris accents live on the edges of routes and in blocked demolition pockets. Tall objects and solid fencing must respect the existing `MallRouteClearance` lanes, flight approaches, player/threat/loot spawn reservations and truck approach. Use the current measured collision policy for cosmetic dressing: pieces below 0.3 m high or thinner than 0.08 m can remain walk-through, while substantial reachable volumes receive simple proxy colliders. Recoverable loot retains its pickup/physics collider regardless of those dressing thresholds. Cosmetic collapse debris remains on its existing nonblocking layer; deterministic rubble mounds retain their special solid behavior.
7. Keep all carryable high-value items reachable from at least two player stand positions where shared grips require it. Hide rewards through foreground clutter and darker side bays, while keeping silhouettes readable from a flashlight sweep. Do not force a player to crawl through a cage just to attach a carry grip.

## Dressing density and loot pads

As an initial art allocation inside each store, use approximately 55% of the floor for visible circulation/turning, 25% for readable retail remnants, 15% for packed salvage pockets and 5% for demolition accents. These are composition targets, not a change to structural load or a guarantee that every square metre is accessible. Keep a stronger gap between silhouettes near doors than in side bays. On the concourse, leave roughly 75–80% of floor space open and concentrate its dressing within the fountain basin, compact sorting pad and route-safe edge pockets.

Begin each store with 3–5 authored candidate loot pockets: 1 easy light-item pocket visible from its doorway, 1 medium inventory pocket, 1 deeper high-value pocket, and optional secondary material/tool pockets. Vary which candidates fill between runs; leave at least a third of the visually dressed shelf/display slots empty to show that extraction is already underway. Do not assign appraisal prices in this layout: value and gameplay weight remain game-tuning decisions in the existing loot definitions.

Existing loot spawn yaw may be selected independently of the marker's orientation. Therefore a spawn pad must accommodate the item's complete 360° horizontal rotation, not just the displayed long axis. For a rectangular item of length L and width W, the circular footprint diameter is `sqrt(L² + W²)`; reserve that diameter plus clearance on both sides. A proposed 3.0 × 1.2 m conference table has a 3.23 m rotation diameter; a 5.5 m square pocket leaves about 1.1 m around the sweep for grips and nearby players. Confirm actual prefab sizes and grip offsets when authoring. For small objects, use roughly 0.8–1.2 m pockets; for medium props start at 2–2.5 m; for heavy racks/tables use the measured rotation and carrier footprint. No spawn pad may consume a protected lane, door approach or flight landing, even when the marker itself lies outside it.

## Integration cautions

Tenant reassignment changes authoring meaning, not merely storefront text. A later implementation would update zone display names, spawn filters and fixture dressing together. Existing ground `Jewelry` and `Security` tags would no longer fit B/C; upper `Offices` becomes B furniture, while upper `Furniture` becomes the closed office suite. Give both B floors the same retail identity and suitable per-floor furniture spawn filters, using the project's data-driven loot definitions rather than the previous jewelry or office filters. Brand “Solutions Inc. Clearance Division” and its mall-mounted clearance/status tags come from the current request and are authorized proposed mall story dressing; no HQ branding or company-save changes are implied by this design.

Use the current `MallRouteClearance` and structural-section attachment workflow when implementation is authorized. `MeridianMallCourt.cs` supplies the fountain/kiosk anchors; `MallLayout.IsProtected` supplies the permanent haul grid. Existing geometry, baked navigation, scene network IDs and structure bindings need no modification for this planning deliverable. No Unity batch run, gameplay test, audit or QA image is requested or performed.
