# Meridian Mall — 0.13.0 approval plan

Date: 2026-10-08. Status: **approved; implementation started**.

Approved follow-up: avoid a big square/rectangle. The 56 × 48 m figure is the bounding envelope,
not a filled rectangular shell. Carve front corner setbacks, a recessed entry court and an offset
service frontage; shape the atrium with clipped corners and stagger storefront/ceiling bays.
Angle parking spawns toward the entrance canopy, anchor sign and skylight, with landscape islands
and a visible sidewalk guiding the approach. This preserves hauling/escape space while changing
the silhouette and first-person experience both outside and inside.

This document records the approved planning deliverable. The project rules,
required design documents and entire specified mall, art and runtime pipeline have been read.

## Concept

Meridian opened in 1988 as a regional mall whose owners believed polished stone, brass and a
large indoor fountain could make shopping feel like a holiday. A modest 1990s cinema conversion
and discount electronics annex followed; the mall closed in 2009. Its in-world abandonment is
about fifteen years, rather than a claim about the current real-world calendar. The rebuild keeps
the 56 × 48 m building, three 4 m stories and east service basement, but replaces its architecture,
materials and dressing. A leaking skylight links the story across all three levels: dead planters
have regrown, the fountain is an algae basin, the escalator skirts are rust-streaked, and the top
gallery has already fallen through a damaged wing. Players navigate by the fountain, cinema
marquee, food-court fascia and broken escalator. The public mall offers fast, weak shortcuts; the
painted-block service circulation offers a longer, dependable way to carry the prize out.

Mood:

- Ivory terrazzo and tarnished brass; faded sage, tobacco brown, concrete grey and burgundy carpet.
- Cold fluorescent islands, warm amber service lamps and a dim blue-grey sky above the atrium.
- Deep but readable shadows, grazing flashlight highlights on cracks, grime concentrated at edges.
- Empty commercial optimism: DEAD CIRCUIT, THREADBARE, TOY ARCHIVE, RECLINE & FALL,
  LAST CHANCE JEWELS, FOOD FOR THOUGHT and FINAL REEL.
- Dry notices: “FOUNTAIN CLOSED FOR LIQUIDATION”, “PLEASE REPORT NEW FLOORS BELOW YOU”
  and “CUSTOMER SATISFACTION SURVIVED THE BUSINESS”. No real brands or offensive graffiti.

Architectural reference is material and spatial logic, not a copy of another mall. Historic England's
[Shopping Building record](https://historicengland.org.uk/listing/the-list/list-entry/1393882?section=official-list-entry)
documents a late twentieth-century retail vocabulary of glazed arcades, stone finishes, anchor
frontages, stainless storefront surrounds, integrated planters and separated servicing. Meridian
uses those principles with its own compact three-level layout and fictional identity. Reference
photographs and other website artwork will not be imported as game assets.

## Floor plans

North is +Z, east is +X; the entrance faces south. Floors are at Y = −4, 0, 4 and 8 m;
the roof is at 12 m. These maps show adjacency and routes, not wall thickness or every room.
The metric schedule below governs the actual builder.

Legend: `P` permanent escape/haul circulation; `S` never-collapse service stair;
`R` never-collapse haul ramp; `E01/E12` collapsible escalators; `W` weak sections;
`X` permanent gallery wound; `J` featured prize candidate; `F` permanent rope descent;
`D` exterior door. Arrows mark circulation connections. Upper atrium centres are void.

Ground — Y = 0:

```text
                                     NORTH +Z
    ┌───────────────────────────────────────┬────────┬───────────────┐
    │ FOOD FOR THOUGHT: counters + kitchens  │        │               │
    ├──────── P north haul passage ─────────┤ P east │  S + R core   │
    │ THREADBARE       │   north concourse   │ spine  │   −1/0/1/2    │
    │ camp at rear     │ ┌─────────────────┐ │        ├───────D───────┤
 D──┤ P rear aisle     │ │ fountain / pool │ │   ←────┤ loading apron ├──TRUCK
    ├──────────────────┤ │      E01 ↑      │ │        │ dock leveller│  ramp faces
    │ DEAD CIRCUIT     │ └─────────────────┘ │        ├───────────────┤  the mall
    │ anchor frontage  │ directory / foyer │        │ security      │
    │ P rear aisle     │                   │        │ jewelry       │
    ├──────── P south link ───D D───────────┴────────┴───────────────┤
    └───────────────────────────────────────────────────────────────┘
                         entrance shutters
                 PARKING: four spawns face north ↑
```

Middle — Y = 4:

```text
                                     NORTH +Z
 F──┌─────────────────┬─────────────────────┬────────┬───────────────┐
    │ RECLINE & FALL  │ food-court terrace  │        │               │
    │ furniture       ├── P north link ─────┤ P east │  S + R core   │
    │ J: Heavy sofa   │  4 m balcony ring   │ spine  │               │
    │ P rear aisle    │ ┌─────────────────┐ │        ├───────────────┤
    │ dry carry lane  │ │   ATRIUM VOID   │ │        │ stock /       │
    │                 │ ├── W J W bridge ┤─┤        │ receiving     │
    │                 │ │ E01 head       │ │        ├───────────────┤
    │                 │ │       E12 ↑    │ │        │ DEAD CIRCUIT  │
    │                 │ └─────────────────┘ │        │ annex         ├──F
    │                 │ TOY ARCHIVE        │        │               │
    ├─────────────────┴── P south link ────┴────────┴───────────────┤
    └───────────────────────────────────────────────────────────────┘
```

Top — Y = 8:

```text
                                     NORTH +Z
    ┌─────────────────┬──────F──────────────┬────────┬───────────────┐
    │ FINAL REEL      │ STILL LIFE GALLERY  │        │               │
    │ cinema / foyer  │ J statue │ X X X   │ P east │  S + R core   │
    │ projection room │ W W W    │ wound   │ spine  │               │
    │ P rear aisle    ├── P north link ─────┤        ├───────────────┤
 F──┤ carpet landmark │  4 m balcony ring   │        │ mall offices  │
    │                 │ ┌─────────────────┐ │        │ records       │
    │                 │ │   ATRIUM VOID   │ │        │               │
    │                 │ │    skylight     │ │        │               │
    │                 │ │   E12 head      │ │        │               │
    │                 │ └─────────────────┘ │        │               │
    │                 │ LAST CHANCE JEWELS │        │               │
    ├─────────────────┴── P south link ────┴────────┴───────────────┤
    └───────────────────────────────────────────────────────────────┘
```

Basement — Y = −4; only the east service footprint is excavated:

```text
          x = 40 m                              x = 56 m
 z=48 m   ┌────────────────┬──────────────────────────┐
          │ P service lane │ S + R core to ground     │
          │                │ permanently dry landings│
          │                │                          │
 z=32 m   ├────────────────┼──────────────────────────┤
          │ P dry carry    │ J stock generator        │
          │ lane           │ shallow flood / cages    │
 z=24 m   └────────────────┴──────────────────────────┘
             pipes, waterline, screened threat pocket
```

Metric schedule:

| Element | Proposed dimensions / location |
|---|---|
| Building / section grid | X 0..56, Z 0..48; 14 × 12 cells of 4 m; 4 m story spacing |
| Atrium / public balcony | Atrium X 20..36, Z 16..32; ring X 16..40, Z 12..36 |
| East service spine | X 40..44, with a continuous 4 m clear walking/carry envelope |
| Service core | X 44..56, Z 32..48; separate stair and switchback haul-ramp bays |
| Service ramps | Two 8 m runs per story, each rising 2 m; 1:4 grade; 4 m clear lanes |
| Service stair | 3 m clear pedestrian flight; permanent foot/head landings and connectors |
| Core landings | South Z 32..36 and north Z 44..48; 4 m deep; visible ramp underside |
| Permanent horizontal routes | West rear carry aisle and north/south links, connected to the east spine; reserve clear lanes before store fit-out |
| North links / gallery access | Ground/middle link Z 36..40; top link Z 32..36. Top gallery weak entry is Z 36..40, so its protected link does not bypass the weak approach |
| Basement | X 40..56, Z 24..48, floor −4; shallow visible flood excludes its dry escape lane |
| Loading door / truck | Door centre (56, 0, 34); proposed truck rear (62, 0, 34), forward +X; existing 2 m truck ramp ends at X 60 |
| Exterior cargo approach | Level 4 m clear apron X 56..60 aligned with the dock; depressed dock dressing sits outside this route |
| Player spawns | Four slots near (28, 0.05, −8), facing +Z; no truck blocking the entrance |
| Authored descent exits | Middle west furniture, middle east annex, top west cinema, top north gallery; permanent ropes to visible solid exterior landings |

Clear dimensions mean clearance between solid faces. Jambs, handrails and wall thickness remain
outside those envelopes. The core fits approximately 3 m stair + 8 m ramp clear width + 1 m total
for boundaries; its detailed rail/turn envelopes are an explicit layout-stage validation item.
The 8 m wide switchback landing is reserved for the piano turn; validate its 4 m depth against
rotated loot, occupied handles and player capsules before accepting the greybox. Intermediate
switchback landings are at half-story height; service-spine doors connect only to full-story landings.
The truck retains its existing 2.6 m cargo width; the final approach is a deliberate narrower loading
stage with a clear handoff apron, checked against existing item and carry-handle dimensions. No
cargo upgrade or loot-size changes are proposed.

## Zones, story and loot

All existing tags stay spelled exactly as below. Display names are authored on the zone and reused
by storefront signs, directories, mystery notices and documentation. No new loot items are added.

| Level / zone | Story and architectural treatment | Existing loot tags / prize |
|---|---|---|
| Ground: Grand Atrium | Leaking roof, algae fountain, rooted planters, tarnished divider strips; directory overlooks escalators | `concourse` |
| Ground: Dead Circuit | Former anchor frontage subdivided into electronics; scavenged displays, empty cable trays and a dry stock aisle | `electronics` |
| Ground: Threadbare | Stripped racks and sun-faded window lettering; one sheltered explorer camp with mattress, scorch and original graffiti | `clothing` |
| Ground: Food for Thought | Fixed food counters, grease behind kitchens and plumbing stains; broad central approach | `food` |
| Ground: Last Chance Jewels | Broken display glazing and empty fixed case shells, retained bulkhead sign | `jewelry` |
| Ground: Security | CCTV brackets with screens removed, wired-glass partitions and paperwork | `office` |
| All levels: Service / loading | Painted block, conduit, drainage, dock bumpers and leveller; protected carrying envelope | `stock` |
| Middle: Recline & Fall | Furniture anchor with dry delivery aisle and warped display platforms below one leak | `furniture`; existing Heavy Designer Sofa prize |
| Middle: Toy Archive | Bleached poster walls and empty fixed display structures; dressing does not imitate spawned toys | `toys` |
| Middle: Food Terrace | View down to the fountain; damaged fascia and removed tables outside the carry route | `food`, `walkway` as appropriate |
| Middle: Dead Circuit Annex | Later fit-out: cheap carpet, stripped cable containment, east emergency descent | `electronics` |
| Middle: Stock / receiving | Dustier than public areas, intact access to ramp core | `stock` |
| Middle: Atrium bridge | 4 m clear transverse shortcut; two weak central sections, obvious seams and a bridge-specific damaged finish | `walkway`; Grand Piano prize |
| Top: Final Reel | Burgundy carpet, marquee, cinema foyer, auditorium seating strips and projection booth; west emergency descent | `cinema` |
| Top: Last Chance clearance room | Discounted jewelry/storage identity replaces the generic quarantine frontage | `jewelry` |
| Top: Still Life Gallery | Stained roof edge, visible historic wound, exposed slab/rebar and statue alcove beyond a weak approach | `gallery`; Marble Statue prize |
| Top: Mall offices / records | Painted partitions, dusty carpet, original admin notices; stripped fittings distinguish decoration from loot | `office` |
| Upper balcony rings | Brass section boundaries, glass balustrades, steel rails and strong void-edge silhouette | `walkway` |
| Basement: stock / plant storage | Failed drainage, painted waterline, corroded cage frames, shallow flood and dry haul lane | `stock`, `electronics`; Military Generator prize |

Four featured prize sites are authored, with **1–2 selected per run**, rather than four guaranteed
high-value items. At least one selection is an existing `Jackpot=true` statue, piano or generator;
the Heavy sofa can occupy only the optional second featured slot. This retains 1–2 actual jackpots
per run without changing the sofa's definition or jackpot achievement semantics. Gallery eligibility
is explicitly statue-only; the existing gallery tag currently
also admits a piano. The upstairs furniture candidate is an existing **Heavy** sofa, not a new or
reclassified Huge item. Add an optional definition filter/featured-point authoring option to the
existing spawn point/planner so those choices are explicit and legacy points retain their behavior.
Keep normal tagged scatter and other heavy spots separate from the selected featured sites.

## Routes, collapse and safety

- Every floor, balcony, bridge, ramp segment, stair/escalator flight and landing remains an authored
  `StructuralSection`. Pivots sit on the walking surface; visible geometry is under `Visual`; solid
  colliders fit visible solids. Stable IDs come from deterministic names/positions.
- The protected route includes **slabs and connections**, not just flights: rear store carry aisles,
  cross links, service spine, core landings, basement lane and loading approach are noncollapsing.
  Shops retain an accessible protected threshold/carry aisle; no decorative object or shutter occupies it.
- The public upper ring, two escalators, bridge, upper store floor fields and ordinary ground slabs
  over the real basement can collapse. Ground over solid soil and the basement bottom cannot.
- The bridge and leak-damaged inner balcony bays give the fastest route to the east haul corridor.
  The permanent outer route costs distance, so crews trade time against structural risk.
- Gallery approach sections have authored lower capacity/health, plus readable cracks and staining.
  The statue pocket has a permanent emergency ledge/descent. Access still crosses weak structure;
  evacuation does not depend on that structure surviving.
- The gallery wound is an authored void with nonblocking rubble/exposed reinforcement, not a
  section collapsed at run start that would reappear on reset. Debris visible below tells the story.
- The broken escalator landmark uses torn skirts and damaged but navigable treads; its structural
  flight can subsequently collapse. It is not a second hidden missing route.
- Permanent rope windows reuse `SafeDescent` with a thin authored-fixture adapter and the existing
  host capsule/Rigidbody loot-lowering behavior as well as player descent registration; unregister
  on disable. They require no purchased gear and reset with the map. Descent is one-way; climbing
  or a new rope gameplay system is outside scope. Openings have near-flush thresholds and at least
  2.3 m clear headroom, rather than the current high window sill. Visible solid ledges, rope columns
  covering the threshold and exterior landing clearance are authored together.
- Single-door stores get `RollerShutter` and precise room bounds. Any store carrying a fallback exit
  or permanent through-route is omitted from shutter placement. Front entrance shutters remain;
  the protected loading exit is independent. Unique shutter indices stay below the existing 32-bit limit.
- Bake the intact interior NavMesh once; collapsing sections retain carving. Flight data gains its
  own length, rise, width and kind because the old validator assumes every flight is 8 m × 4 m.
  Keep slope/lip/headroom/path checks and add deterministic carry-envelope/turn-clearance checks.
- Screened threat points cover all levels, including basement, with approximately 12–16 candidate
  positions. Their locations respect NavMesh and existing director distance/visibility preferences.
  No threat behavior or networking rewrite is proposed.

The implementation keeps the current network architecture: **one dynamically spawned structure
sync object per level**, not a NetworkObject per section. RunState, loot and threats remain dynamic.
Stamp genuinely scene-placed NetworkObjects after save/build-scene registration.

## Original modular kit and materials

Use a new `Tools/Blender/meridian.py` entry point with small helper files for architecture, UVs,
textures, fracture and export. Preserve the legacy `environment.py` outputs and `ENV_*` materials:
HQ, wardrobe, garage door and horror scare assets share them.

New exports: `Game/Assets/_Project/Art/Custom/Environment/Meridian/`.
New generated Unity materials/prefabs: mall-specific paths and names such as `MALL_*`.
A checked-in manifest records model dimensions, pivots, LODs, material roles, damage variants and
fracture mappings. Names and filenames remain stable; Unity-created `.meta` files stay checked in
and are never deleted during re-export. Unity (X,Y,Z) maps to Blender (X,Z,Y), preserving 0.12.2's correction.

| Kit family | Required pieces | Texture size and source |
|---|---|---|
| Structural floors | Terrazzo, tile, carpet, concrete, wooden display/walkway and bridge finishes; stressed/cracked/sagged variants and pre-fractured chunks | Terrazzo / hero tile 2048; others 1024; original work plus CC0 terrazzo |
| Walls and storefronts | Plaster/dado, block wall, corners, bulkheads, glazed/boarded/shuttered modules, display windows, doors and shutter housings | Hero wall/storefront trim sheets 2048; service surfaces 1024; original |
| Flights / core | Escalator tread ribbons, skirts, rubber handrails, stair flights, smooth haul ramps, landings and rail modules; damaged/fractured flight sets | Escalator hero trim 2048; stairs/ramps 1024; original |
| Atrium architecture | Columns, glass balustrades, bridge trims, actual skylight aperture/frame/panes, fountain basin and integrated planters | Shared 2048 hero trims; basin/planters 1024; original |
| Ceilings / servicing | Coffers, drop grids, missing/sagging panels, slab undersides, ducts, pipes and conduit | Ceiling atlas 2048; servicing 1024; original |
| Destination fittings | Directory/sign frames, kiosks, fixed food counters, cinema doors/marquee/seating strips, gallery slab edges and rebar | 1024, minor fittings 512; original |
| Exterior | Anchor facade, canopy, entrance doors, dock leveller/bumpers, boarded openings, asphalt and parking markings | Hero facade shares 2048 wall/storefront set; asphalt/concrete 1024; original plus CC0 concrete |
| Decay / small dressing | Leak trails, mould, algae, edge grime, rust, scorch, graffiti, leaves, broken glazing and bird deposits | 512 per prop/decal set; original |

Proposed downloaded materials only:

- [ambientCG Terrazzo001](https://ambientcg.com/view?id=Terrazzo001), author ambientCG/Lennart Demes,
  2K maps for atrium terrazzo. Its [published licence](https://docs.ambientcg.com/license/) is CC0.
- [Poly Haven Concrete Wall 006](https://polyhaven.com/a/concrete_wall_006), photography Charlotte
  Baglioni, processing Dario Barresi, 1K maps for exposed cores/service concrete; the asset page
  identifies CC0 and the [site licence](https://polyhaven.com/license) confirms it.

No third-party models are planned. Existing credited Kenney vehicles and loot stay in place unless
their appearance clashes; flag those clashes in the render notes instead of silently rebuilding them.
Only 2K/1K source maps are downloaded. Every imported map and derived mask gets a file-level credit
with author, licence, asset URL, use and path under `Art/ThirdParty/<Source>/<Pack>/`; original maps
stay under the new Meridian art folder. The downloaded material budget can be satisfied with original
procedural replacements if either source is unavailable; record the final actual inventory.

Materials use URP Lit with base colour, tangent normal and packed linear mask. For the installed
URP Lit shader, pack **R metallic, G occlusion, B unused, A smoothness**; bind the mask to both
metallic-gloss and occlusion slots and enable the corresponding keywords. Convert roughness to
smoothness and use the correct normal convention. Base colour is sRGB; masks are linear; normals
import as normal maps. Use metre-based tiling UVs and authored trim UVs rather than indiscriminate
smart projection. Set Desktop Max Size explicitly (2048/1024/512), compressed BC7/DXT5 where appropriate,
compressed normal maps, mipmaps and non-readable runtime textures. No 4K source or imported textures.
World text continues to use `WorldTextMaterial` and `Abandoned/WorldText`, including directories,
store names and notices; never a font GUI material. Existing text/particle shader exceptions remain.

Use geometry bevels and PBR roughness detail for realism; do not rely on noisy colour maps. The
provisional cap is eight 2048 PBR triplets, twelve 1024 triplets and eight 512 triplets, with reuse
through trim sheets. Signs/decals use atlases. Bound crack meshes follow their section; projector
decals are used where they reduce cost. URP currently has no configured decal renderer feature:
any feature setup is through Editor APIs, uses installed URP, and must preserve HQ's appearance.

## Damage, decay and collision treatment

Per-floor presentation data binds authored crack/sag stages and matching fracture finishes. Cracks
remain readable on dark carpet, wet terrazzo, ceramic tile, concrete and bridge surfaces. All crack
and sag variants are visual only; the root collider stays authoritative until collapse. Preserve the
existing replicated sounds, warnings and dust timing, including the roughly two-second failing phase.

Extend `StructureSceneSetup`/`SectionPresentation` through optional mall-specific data, keeping the
legacy defaults for other levels. Preserve authored chunk core/trim/rebar materials in `DebrisSpawner`
instead of overwriting them with the first intact material. Escalators receive real pre-fractured
pieces. Debris remains local, cosmetic, on Debris layer and within the existing live-chunk/lifetime caps.
Restore logic must select the correct stable stage and LOD, rather than enabling every renderer.

Section-supported ceiling panels, bridge rails, fixtures and decoration follow their support and
carry no static flags. Include lamp/ceiling/rail fragments in the local collapse presentation so they
visibly fall while losing power; there is no gameplay collision or damage from these fragments.
Persistent walls and columns remain outside section hierarchies, avoiding accidental collider shutdown.

Decay is driven by authored causes in layout data:

| Cause | Placement rule |
|---|---|
| Skylight leaks | Named leak origins project onto roof frames, ceiling stains, wall streaks, floor pools and adjacent warped/rusted fittings |
| Failed plumbing | Food-court back walls and basement pipe joints, with coherent waterlines and drain paths |
| Daylight | Bleaching near glazing, growth in skylit planter pockets, bird deposits beneath roof perches |
| Wind / entry | Leaves and small fragments collect behind broken doors and sheltered floor edges |
| Scavengers | Removed screens and cables, smashed display fronts, coherent camp in one sheltered Threadbare corner |
| Historic failure | Gallery hole, matching material rubble below, bent reinforcement and warning notices at its approach |
| Time | Edge/corner dirt, protected dust, streaks beneath metal fittings and peeling paint on damp block |

Small dressing has no solid collision. Solid architecture uses fitted boxes or authored mesh
colliders only where geometry is actually solid: never a box spanning a fountain hollow, doorway,
broken pane or rail gap. Interactive notices use the existing query layer without snagging movement.
Water is shallow, passable presentation; no swimming, drowning or new flood modifier mechanics.

## Lighting, atmosphere and sound

- Realtime only. No baked lightmaps or APV dependency. Retain the existing Forward+ and SRP batching.
- Cut a real atrium roof aperture; the current opaque deck covering the skylight is removed by the
  builder. Use subdued overcast/moonlit sky through dirty panes and open patches.
- Broad, mostly unshadowed fluorescent pools, faulty tubes and warm service lamps; roughly 70–90
  powered fixtures is a starting art budget, including basement. Tune range/intensity from stills.
- Reserve at most four local environment shadow casters in a view, plus the existing main-light and
  flashlight policy. Preserve the current remote/corpse flashlight shadow limits. Four beams still illuminate.
- Fixtures beneath slabs bind to the actual overhead section. Top-floor fixtures bind to permanent
  roof supports; basement fixtures bind to ground slabs. Authored emergency lamps use `HorrorAlarm_*`.
- Keep six `HorrorConfig.SectorOf` sectors and two survivors at maximum danger, with `(12,4,12)`
  cells. These are currently hashed spatial sectors, not contiguous rooms; distribute fixture coverage
  so losing a sector is readable. Preserve final red phase and reduced-flashing preferences.
- Building bounds derive once from layout: centre `(28,4,24)`, size `(56,16,48)`, including basement.
  Update HorrorConfig and interior atmosphere through Editor APIs. Author arrival figure/door/roar
  positions against real new geometry; ensure arrival door effects point at the correct visible door.
- Keep the arrival wave's +Z direction unless layout data supplies its origin explicitly. Spatial
  changes do not justify changing the existing danger timings or contract balance.
- Use existing exponential fog/post-processing and restrained dust/shaft effects. The current stack
  has no true volumetric fog; no new volumetric package is proposed or implied.
- Start with 4–6 local 128/256 reflection probes, refreshed once/staggered on relevant power changes,
  with neutral fallback reflections. No per-frame full-scene probe rendering or stale bright power-on
  reflections in blackouts. Set floor/glass roughness to control expensive reflections/overdraw.
- Zone-authored ambience uses existing area bounds and vent/water/wind cues; distinguish kitchens,
  service space, cinema and basement through data and existing sound design, not a new audio system.
- Append surface enum values without renumbering existing ones where needed for Terrazzo, Tile,
  Carpet and Water; update SoundBank/fallback mappings in the same gameplay-hook step. Tag every
  walkable/hittable architectural solid, including shutter/rail/ramp surfaces. Wet floor colliders
  carry the wet tag: decorative water need not add a snagging collider to win a footstep ray.

## Performance and rebuild evidence

Targets: 60 FPS at 1080p as a four-player host; under approximately 2,500 batches; mall texture
residency below 1.5 GB. These are targets, not results already achieved.

- Shared materials, one/two slots for ordinary modules and up to three justified hero slots.
- LOD0/1/2 and cull for large destination props/fittings; simplified architecture LODs preserve
  floors, walls and void-edge silhouettes for as long as they are visible. Small detail culls earlier.
- GPU instancing on reusable compatible materials; SRP batching retained. Avoid needless material
  instances/property blocks. Static batching/occluder flags only on permanent geometry.
- Bake occlusion in the mall rebuild. The current `rebuild-horror` path does not actually invoke
  `OcclusionBake`; making the bake part of this path is an explicit deliverable.
- No breaking floors, attached rails/ceilings, shutters or decorative fall pieces act as occluders.
- Restrict transparent glass/water layers, distant decals, active dust and local shadow casters.
- A mall asset census records textures/format/mips, material slots, mesh/LOD counts, lights, probes
  and estimated residency. The proposed PBR cap is about **184 MiB** with one-byte-per-pixel compressed
  formats and full mip chains, before auxiliary maps/probes; report actual imported totals later.
- Keep `[MallValidation]` flights/invisible colliders and `[MaterialAudit]` at zero. Add a strict
  mall-specific import/PBR/LOD/collision/route audit because the existing material audit can accept
  a tiny placeholder grain map, and renderer-bound collider checks can accept hollow geometry.
- Aggregate required rebuild failures into a failed command result. Today validators mostly log;
  `Tools/unity.sh` success alone is insufficient evidence of zero findings. Capture the actual counts.

The user's no-playtesting policy takes precedence over the request for measured four-player host
performance. Editor still-render statistics, asset memory totals and estimates will be reported with
their measurement context. Actual F1 FPS/main-thread/batches/GC numbers on a mid-range Windows
four-player host remain **pending the user's test**, unless the user separately authorizes that run.
F1's current `mem` is managed heap only and will not be misreported as texture or GPU residency.

## Build sequence after approval

Each stage runs `Tools/unity.sh compile`, then `Tools/unity.sh rebuild-horror`; inspect all required
zero-problem lines before the next stage. Unity must be closed for those commands. If an operation
fails twice, stop and explain likely causes. No EditMode, PlayMode, nettest, screenshot tests,
`all`, `rebuild`, TestMap regeneration, paid assets, new packages or pushes.

1. **Layout and working greybox.** Establish canonical layout/config data, safe paths, core ramps,
   exterior exits, shutter eligibility/bounds, deterministic sections, loot/prize eligibility, threats,
   spawn/truck data, atmosphere positions, surface/ambience hooks and generalised flight validation.
   Remove the mall's dependency on TestMapBuilder constants without editing TestMapBuilder.
   Add the approved realistic-environment exception to CLAUDE.md; keep other content's current look.
2. **Architecture kit.** Generate the separate original Meridian models, LODs and fracture/stage
   assets; replace the greybox through small area builders. Check core turning clearance, visible
   collider coverage, real skylight and section-attached geometry.
3. **Materials and textures.** Import the credited 2K/1K CC0 maps, generate original trim/PBR sets,
   pack masks, set platform imports and bind materials. Introduce per-surface damage presentation
   and preserve debris materials, with legacy default behavior unchanged.
4. **Dressing and decay.** Replace random scatter with cause fields, storefront identities, cinema,
   food court, camp, gallery wound, basement water and exterior details. Keep loot/haul envelopes clear.
5. **Lighting and atmosphere.** Tune failing fluorescents, emergency/red phase, fog, dust, reflection
   probes and basement fixtures; author falling cosmetic fittings. Keep all HQ settings/placements intact.
6. **Performance pass.** Reduce slots, overdraw and detail; confirm LOD/cull setup, instancing and
   static flags; bake mall occlusion and report asset census plus available Editor measurements.
7. **Renders, builds and notes.** Bump `BuildScript.Version` to 0.13.0; final compile/rebuild counts.
   Produce paired power-on/power-off stills from an Editor render script for atrium, food court,
   cinema, Dead Circuit, service corridor, basement and parking lot at night (14 stills), plus a
   gallery/collapse-stage close view if useful, into `Docs/renders/0.13.0/`. Render through the real
   URP camera/volume using a scene opened in memory; do not persist preview lighting overrides.
   These are art stills, not test captures, and never use the generic screenshot command.

Commit logical stages with clear messages and `Co-Authored-By: Codex <noreply@openai.com>`.
Before shareable builds, commit all source/generated content and require a clean tree. Run
`Tools/unity.sh build` for universal Mac and Windows x64 Mono archives in the existing
`~/Documents/Abandoned-builds/dev/` naming convention. Reset build stamps through the existing
build code and verify the resulting tree. Final PROGRESS notes include render/build paths,
validation counts, texture statistics, measurement context, known gaps and user test priorities.
Build results/paths are only reported after the actual builds complete.

Implementation is split among small Mall layout, architecture, storefront, core, exterior,
lighting, decay, presentation/import, render and audit helpers, each roughly under 300 lines.
Tunables live in ScriptableObject data; generated scenes/prefabs/assets use Editor APIs. Keep
the existing Mall scene path and BuildScenes registration. No handwritten scene/prefab YAML.

## Scope and review gates

- HQ hand edits, shared legacy environment art, player/monster/loot models, TestMap and package
  versions are preserved. Read-only git comparison after every regeneration identifies unintended
  shared/HQ changes; any such effect must be resolved in the builder before committing.
- Narrow runtime edits are limited to feeding this layout: featured loot-point eligibility,
  authored fallback descent, optional structural presentation data, surface sound mappings and
  cosmetic section-supported fragments. No new threats, equipment, economic balance or net architecture.
- The core's carry/turn geometry, section readability in darkness and fixed rope landing clearance
  are first-stage acceptance requirements, not detail to postpone until dressing.
- A realistic environment may expose low-poly loot/vehicle/threat style clashes. Record them with
  the review renders; redesigning those assets requires another approved scope.
- Art readiness is judged from the produced stills and the user's in-game review. Compilation and
  zero audit findings do not establish gameplay quality or the 60 FPS host target.

Proposed first in-game checks for the user after delivery:

1. Carry/trolley a piano and generator through ramp turns, doors, dock apron and truck ramp; repeat with three/four crew.
2. Collapse bridge, balcony, gallery approach and both escalators; verify readable warnings, matching debris and usable permanent escapes.
3. Use every rope window without bringing rope equipment; check descent, safe exterior landings and return to the truck.
4. Run power on/off, maximum danger and final red phase on every level, including basement; check fallen lamps stop lighting.
5. Play Sealed and trigger shutter slams with crew/loot near thresholds; confirm fallback stores are never locked.
6. Review loot eligibility/placement, monster routes and hidden spawn coverage, then record Editor F1 (or Development build/profiler) performance in the atrium with four flashlights.

**Approval requested:** approve this design and the narrow map-supporting code/data changes above,
or specify changes to the layout, art direction, service haul ramps or featured-prize treatment.
