# Salvage asset breakdown — 0.15.5

This is the production specification for the five-zone salvage mall asset set: **45 individual assets**. Zone B is Nordic Habitat on **both front-right floors**; its ground floor holds heavy furniture and its upper floor holds lighter office furniture and samples. Closed offices occupy the former upper west furniture bay. The mall's existing front extraction route, floors and structural circulation remain the basis of placement.

The 0.15.5 authoring catalogue supplies **35 recoverable variants and 10 support-only assets**, alongside static dressing variants for all 45 assets. This permits scene composition to use owned/static equipment while separate sale-stock copies remain recoverable. Per-entry dimensions, solver masses and budgets below are planning suggestions; the generated `LootDefinition` catalogue is authoritative for initial gameplay value, weight, carry class and clamped physics mass, and preserves later inspector tuning. The authoring manifest records those effective values and model dimensions when the assets are built.

The build uses original, code-authored low-poly meshes, reusable procedural PBR surface maps and separately authored readable labels. The estimates below are **art budgets**, not measured triangle counts. They describe the intended deliverable and future detail allowance. AI prompts are ready-to-use concept-reference prompts; an isometric image is neither a 3D mesh nor a validated PBR texture pack. No AI concepts, Unity compilation, tests, scene previews or verification are implied by this document. The user will perform those checks after the 0.15.5 build.

## Shared production conventions

- **Scale and pivots:** Unity units are metres. Upright props use a root pivot at the centre of their floor contact; flat props use the underside centre. Apply authored dimensions in the mesh, leaving prefab root scale at `(1, 1, 1)`. Keep labels, collider proxies and interaction children independent of render meshes.
- **Categories:** `Tech`, `Furniture`, `Hardware`, `Material`. Apparel textiles belong to `Material`; their display fixtures belong to `Furniture`. A support asset's category describes its visual asset family, not an automatic recoverability flag.
- **Texture packing:** Use a 1,024 px shared surface atlas or repeating 512 px surface maps for common plastic, metal, wood, fabric and dust. Base Color is sRGB; normal and packed maps are linear. For URP Lit, pack metalness into **R** and smoothness into **A** of the metallic map; pack ambient occlusion into **G** of a separate occlusion map. Roughness is an authoring input converted to `smoothness = 1 − roughness`. Do not feed an unconverted roughness map into the smoothness channel. Keep labels on their own atlas/material so wording is sharp.
- **Required PBR channels:** Every entry names Base Color, Normal, Metallic/Smoothness and AO explicitly. Normal can be a flat tangent-space map for the first low-poly pass. Painted surfaces are dielectric at the paint layer, exposed metal is metallic, and dust reduces smoothness locally. Emission is only required for powered lights, screens or indicators specified below. Opacity is only required for netting, film, glass or decals specified below.
- **LODs:** An entry marked `LOD0/1/2` calls for roughly 100%/50%/25% of its stated budget, prioritising silhouette. LOD1 removes small attachments; LOD2 collapses subdivisions and repeated internals. Tiny boards, labels and floor decals do not need an LODGroup. These are planned art targets, not checked mesh statistics or guaranteed decimation ratios.
- **Physics:** Recoverable props use the existing network-authoritative loot flow. The server/host owns spawn, value and weight. Dynamic variants use a root Rigidbody, gravity enabled, no child Rigidbodies, simple compound colliders and the project's existing collision layers. Keep the existing generator's interpolation and collision policy: `ContinuousDynamic` for Pocket/OneHand/TwoHand items and `ContinuousSpeculative` for Heavy/Huge items. Do not make every visible object a networked physics prop.
- **Mass versus gameplay weight:** The mass suggestions below are solver starting points. The project's existing definition setup clamps dynamic physics mass to **0.1–80 kg**, even when gameplay carrying weight represents a heavier object. The authored catalogue's actual values supersede those suggestions. Keep authoritative salvage weight/value in the existing item definition rather than infer them from mesh volume or Rigidbody mass.
- **Interaction sockets:** `InteractionAnchor` locates the focus/pickup cue; `OutlineAnchor` is the parent for optional outline renderers, which have no collider or Rigidbody; `CarryGrip_A` and `CarryGrip_B` locate large-item grip references; `LabelAnchor` carries the readable clearance sticker. Socket positions are relative to the asset's actual surface. Outlines are enabled by focus through the salvage interaction hook, not baked permanently into the base material. Suggested sockets do not add a second pickup system.
- **Static dressing variants:** Assets used as owned clearance equipment or mall fixtures have no loot registration, pickup trigger or active outline. Their static variant has no Rigidbody. Only substantial reachable solids receive player-blocking proxy colliders. Flat dust, glass shards, tags and tire marks never create snagging obstacles. Recoverable small loot still retains its own pickup/physics collider.
- **Concept prompts:** Each prompt below stands alone. Render one asset only, in an orthographic three-quarter isometric product view with neutral lighting, a plain background and a clear silhouette. Put exact contractor classification text on the authored label atlas; do not rely on text rendered inside an AI concept. Generated concepts are reference art and need manual interpretation into geometry and materials.

## Zone A — Apex Circuit / Volt & Byte: 8 assets

Keep sale inventory in recoverable variants and mark the crew's powered equipment distinctly. Copper is Material; industrial batteries are Tech. The display front is depleted and damaged, while the west stock recess holds larger recoverable equipment.

### A01 — PC tower (`pc_tower`)

1. **Item name & category:** Mid-tower desktop PC; `Tech`, recoverable. Approximate size 0.22 × 0.46 × 0.45 m.
2. **3D model specs:** 900–1,800 triangles at LOD0. Medium complexity: box chassis, bevelled front, rear IO strip, vent strips and a loosened side panel. Use LOD0/1/2; replace small ports and grille holes with textures at distance. Internal boards need only be visible through the opened panel.
3. **Material & texture maps:** Base Color, Normal, Metallic(R)/Smoothness(A), AO(G). Powder-coated steel, scratched black plastic front, dusty vents, exposed metal along panel edges and a cleaner rectangular area where a sticker peeled off. No emissive screen; the abandoned PC is unpowered. Separate `TECH / TEST` label and small faded retail inventory sticker.
4. **Prefab setup:** Dynamic root Rigidbody, initial mass 7 kg. Use one BoxCollider for the chassis and a second small box only if the side panel protrudes materially; no concave collider. `InteractionAnchor` above the top-front edge; `OutlineAnchor` at chassis centre; `CarryGrip_A/B` beside the upper side edges; `LabelAnchor` on the side panel. Static display variant has no Rigidbody.
5. **AI concept prompt:** “Single abandoned mid-tower desktop PC for a 3D salvage game, orthographic three-quarter isometric asset concept, slightly open steel side panel, dusty black plastic front, scratched paint, visible simple rear ports and ventilation, faded inventory sticker and an orange clearance-label rectangle, believable 0.46 metre height, clear low-poly-friendly silhouette, neutral studio lighting, plain warm grey background, no people, no surrounding shop, no readable text.”

### A02 — Motherboard (`motherboard`)

1. **Item name & category:** Desktop motherboard; `Tech`, recoverable. Approximate size 0.30 × 0.04 × 0.24 m, including heatsinks.
2. **3D model specs:** 500–1,200 triangles. Medium close-up detail in a thin board, CPU socket, four RAM slots, two heatsinks and a back-port block. Use textures for circuitry, small capacitors and pins. No LODGroup for this small pickup; distance culling can remove it after its parent display is no longer relevant.
3. **Material & texture maps:** Base Color, Normal, Metallic/Smoothness, AO. Dark green or black solder mask, pale printed traces, metallic heatsinks, dust gathered around socket rims and tarnished connector contacts. Keep circuitry largely in Base Color/Normal; do not model every trace. Use a separate antistatic tray material when dressed on a shelf.
4. **Prefab setup:** Dynamic Rigidbody, mass 0.8 kg, one shallow BoxCollider thick enough for stable contact. `InteractionAnchor` at the CPU socket, `OutlineAnchor` at board centre, `CarryGrip_A` on a long edge. A tray is a separate static support, so carrying the board does not accidentally carry the entire shelf.
5. **AI concept prompt:** “One salvaged desktop computer motherboard isolated in orthographic isometric three-quarter view, dark green PCB, silver CPU socket, four RAM slots, two simple aluminium heatsinks and rear IO block, dust along component bases, tarnished contacts, bent corner but intact silhouette, game-ready low-poly concept with circuitry shown as painted detail, neutral light, plain grey background, no text, no hands, no extra objects.”

### A03 — CRT monitor (`crt_monitor`)

1. **Item name & category:** Boxy 17-inch CRT monitor; `Tech`, recoverable. Approximate size 0.43 × 0.43 × 0.46 m.
2. **3D model specs:** 1,200–2,400 triangles. Medium complexity: broad curved housing, bevelled screen recess, stand and rear cable exit. Use LOD0/1/2; preserve the deep rear silhouette and reduce the screen curvature in later LODs. Moulded ventilation is primarily texture detail.
3. **Material & texture maps:** Base Color, Normal, Metallic/Smoothness, AO. Yellowed ABS, dark screen glass, scratched corners, a hand-cleaned patch around the handle and dust in the vent recesses. Screen is opaque and non-emissive; dark smoothness variation sells reflections without expensive transparent glass. Optional exposed wire uses the shared cable material.
4. **Prefab setup:** Dynamic Rigidbody, mass 18 kg. Compound BoxColliders for body and base; rounded housing does not require a convex mesh. Lower centre of mass slightly inside the heavy body. `InteractionAnchor` near front bezel; `OutlineAnchor` at body centre; `CarryGrip_A/B` on left and right sides; `LabelAnchor` on the upper casing.
5. **AI concept prompt:** “Single old 17-inch CRT computer monitor, deep bulky rear housing and squat swivel base, orthographic isometric game prop concept, yellowed beige plastic, black unpowered curved screen, dusty vents, scraped corners, short cut cable at rear, small orange salvage sticker shape, sturdy low-poly-friendly forms, neutral studio lighting, plain grey background, no readable text, no people, no environment.”

### A04 — LED monitor (`led_monitor`)

1. **Item name & category:** Flat LED monitor with stand; `Tech`, recoverable. Approximate size 0.57 × 0.43 × 0.18 m.
2. **3D model specs:** 600–1,400 triangles. Low-to-medium complexity: thin rectangular display, bevelled frame, vertical support and foot. LOD0/1/2; preserve the thin side profile. Make wall-bracket dressing a separate removable support rather than a large permanent part of the carryable monitor.
3. **Material & texture maps:** Base Color, Normal, Metallic/Smoothness, AO. Black plastic with fine scratches, dusty screen lower edge, exposed metal at stand scuffs and one pressure crack represented in Base Color/Normal. Screen remains opaque, dark and non-emissive. No transparency is required for the first asset pass.
4. **Prefab setup:** Dynamic Rigidbody, mass 5 kg. Two BoxColliders, one for the display and one for the base; avoid an extremely thin collider. `InteractionAnchor` at screen centre; `OutlineAnchor` follows body/stand renderers; `CarryGrip_A/B` on side bezels; `LabelAnchor` on the rear. Static wall-mounted display variant has no Rigidbody.
5. **AI concept prompt:** “One disconnected widescreen LED office monitor with a thin black bezel and simple stand, orthographic isometric three-quarter asset concept, dusty screen, faint corner pressure crack, scraped stand, visible rear cable socket and old asset-sticker patch, dead screen, clear low-poly silhouette, neutral light, plain light grey background, no readable text, no environment or people.”

### A05 — Loose circuit board (`circuit_board`)

1. **Item name & category:** Loose electronics controller board; `Tech`, recoverable. Approximate size 0.18 × 0.03 × 0.12 m.
2. **3D model specs:** 300–700 triangles. Low complexity: thin plate, one connector block, three component volumes and a small heatsink. No LODGroup. Use two geometry variants with the same material so this reads as salvaged electronics rather than repeated identical motherboards.
3. **Material & texture maps:** Base Color, Normal, Metallic/Smoothness, AO. Green solder mask, printed pale traces, copper contact edge, dark integrated circuits, chalky dust and slight solder tarnish. Keep individual solder pads and small inscriptions in the texture. Add no bright powered LEDs to abandoned boards.
4. **Prefab setup:** Dynamic Rigidbody, mass 0.2 kg. One shallow BoxCollider; position the pickup focus slightly above it. `InteractionAnchor` and `OutlineAnchor` at board centre, `CarryGrip_A` along its long edge. Static tray-fill copies have no Rigidbody and no interaction outline.
5. **AI concept prompt:** “One small loose industrial circuit board for a salvage game, orthographic three-quarter isometric object study, green PCB, simple black chips, silver solder pads, copper contact edge and one white connector, light demolition dust and tarnish, flat readable geometry with circuit traces as surface detail, neutral studio lighting, plain grey background, no text, no tray, no people.”

### A06 — Server rack (`server_rack`)

1. **Item name & category:** Half-populated server rack; `Tech`, recoverable large item. Approximate size 0.65 × 1.85 × 0.85 m.
2. **3D model specs:** 1,800–3,500 triangles. High relative complexity: four uprights, six visible equipment faces, empty slots, side rails and cable loops. Use LOD0/1/2; do not build every internal server. Collapse repeated rack internals and cable loops at LOD1/2 while retaining the open-frame silhouette.
3. **Material & texture maps:** Base Color, Normal, Metallic/Smoothness, AO. Black painted steel, brushed server fronts, dusty horizontal ledges, scuffed rack rails and a faded inventory strip. No emission on indicator lights unless a separate powered storytelling variant explicitly requires it. A `HOLD / FRAGILE` sticker sits on the upright.
4. **Prefab setup:** Dynamic loot variant, Rigidbody solver mass 80 kg; gameplay weight can be higher. Use compound BoxColliders for body and projecting base, or one broad box if the interior is not traversable. Do not put a walk-through cavity inside a movable rack. `InteractionAnchor` at chest-height front rail; `CarryGrip_A/B` on opposite lower sides; `OutlineAnchor` covers the frame and equipment; `LabelAnchor` on the front upright. Static rack variant has no Rigidbody.
5. **AI concept prompt:** “One half-populated wheeled server rack from an abruptly abandoned electronics retailer, orthographic isometric asset concept, tall black steel frame, six dusty silver equipment fronts and several empty slots, a few clipped cable loops, scraped base and orange hold-label rectangle, clear sturdy low-poly silhouette, neutral light, plain grey background, no readable text, no environment, no people.”

### A07 — Copper spool (`copper_spool`)

1. **Item name & category:** Copper cable spool; `Material`, recoverable. Approximate size 0.45 × 0.45 × 0.34 m.
2. **3D model specs:** 700–1,500 triangles. Medium complexity: two 12–16-sided flanges, central hub and a wound-cable volume. LOD0/1/2; reduce circumference segmentation and represent windings in the normal map. A short loose cable tail may be a separate render child with no collider.
3. **Material & texture maps:** Base Color, Normal, Metallic/Smoothness, AO. Warm copper with mild dark tarnish, wooden or black plastic spool flanges, dusty top arcs and rubbed bright edges. Use normal bands to imply winding instead of hundreds of separate wires. A tied paper weight label uses the label atlas.
4. **Prefab setup:** Dynamic Rigidbody, mass 22 kg. Use a short CapsuleCollider rotated along the spool axis or a low-sided convex hull; add no collision to the dangling cable tail. `InteractionAnchor` at hub front, `OutlineAnchor` around spool, `CarryGrip_A/B` at flange sides and `LabelAnchor` on one flange.
5. **AI concept prompt:** “Single medium copper cable spool standing on its circular flange, orthographic isometric salvage asset concept, tightly wound copper wire with warm tarnish and rubbed bright edges, dusty dark spool flanges, one short loose cable tail and tied blank weight tag, low-poly-friendly circular forms, neutral studio lighting, plain grey background, no readable text, no people, no surrounding clutter.”

### A08 — Industrial battery (`industrial_battery`)

1. **Item name & category:** Sealed industrial battery block; `Tech`, recoverable. Approximate size 0.52 × 0.46 × 0.30 m.
2. **3D model specs:** 650–1,300 triangles. Medium complexity: bevelled block, recessed handle, two terminal covers and moulded top ribs. LOD0/1/2; preserve terminal/handle silhouettes and simplify top ribbing at distance. Use a sealed intact case for the first pass.
3. **Material & texture maps:** Base Color, Normal, Metallic/Smoothness, AO. Matte dark polymer, dusty ribs, scratched casing, worn yellow warning panel and oxidised metal terminal hardware. No leaking fluid or glow. Put tiny warnings on the shared decal atlas and contractor `TECH / TEST` classification on its own label.
4. **Prefab setup:** Dynamic Rigidbody, mass 32 kg. One BoxCollider plus a small top-terminal proxy only if necessary; lower centre of mass for stable stacking. `InteractionAnchor` on top handle, `OutlineAnchor` at body centre, `CarryGrip_A/B` beside the recessed handles, `LabelAnchor` on the broad front face. Static pallet-load version has no Rigidbody.
5. **AI concept prompt:** “One sealed industrial battery block ready for recovery, orthographic three-quarter isometric 3D-game asset concept, heavy dark polymer case, recessed lifting handle, two covered terminals, worn yellow warning-panel shapes, scratched corners and drywall dust in top ribs, sturdy low-poly construction, neutral lighting, plain grey background, no readable text, no people, no leak or dramatic effects.”

## Zone B — Nordic Habitat, ground and upper floors: 9 assets

Ground-floor heavy pieces remain near extraction and broad turn pockets. The upper floor contains desks, lighter ergonomic chairs, stripped shelves and sample rolls. Dollies can support the existing manual service-ramp recovery route; forklift marks stay on the ground floor.

### B01 — Steel filing cabinet (`steel_filing_cabinet`)

1. **Item name & category:** Four-drawer steel filing cabinet; `Furniture`, recoverable. Approximate size 0.47 × 1.32 × 0.62 m. Ground floor.
2. **3D model specs:** 800–1,600 triangles. Medium complexity: bevelled shell, separate drawer faces, handles and shallow label holders. Use LOD0/1/2. Open drawers are an authored pose variant, not simulated drawers in the first pass; keep their silhouette within the reserved load footprint.
3. **Material & texture maps:** Base Color, Normal, Metallic/Smoothness, AO. Off-white painted steel, scratched handle surrounds, rust freckles along bottom seams, accumulated dust on top and one missing label-card rectangle. Keep large rust patches concentrated near seams rather than uniformly covering every face. Add a salvage classification sticker.
4. **Prefab setup:** Dynamic Rigidbody, mass 45 kg, compound BoxColliders for cabinet and any fixed open drawer. Do not give each drawer a Rigidbody. `InteractionAnchor` above second drawer; `OutlineAnchor` at shell centre; `CarryGrip_A/B` on opposite sides around lower chest height; `LabelAnchor` on a drawer face. Static wall-bank variant has no Rigidbody.
5. **AI concept prompt:** “Single four-drawer steel filing cabinet, orthographic isometric salvage prop concept, off-white painted metal, one drawer slightly open, simple pulls and empty label holders, rust freckles at lower seams, scratched handles and dusty top, orange clearance sticker shape, realistic office proportions and low-poly-friendly bevels, neutral studio lighting, plain grey background, no readable text or people.”

### B02 — Ergonomic chair (`ergonomic_chair`)

1. **Item name & category:** Lightweight ergonomic office chair; `Furniture`, recoverable. Approximate size 0.68 × 1.10 × 0.68 m. Both floors; upper-floor primary seating.
2. **3D model specs:** 1,400–2,800 triangles. High relative complexity in the back frame, seat, armrests, five-star base and caster silhouettes. Use LOD0/1/2; model the back as opaque shaped fabric/plastic in the first pass, avoiding real mesh weave geometry. Reduce casters and armrest subdivisions at distance.
3. **Material & texture maps:** Base Color, Normal, Metallic/Smoothness, AO. Charcoal fabric, matte polymer back, brushed metal column, scratched armrests, flattened seat nap and dust along frame edges. Fabric weave is a repeating normal detail. Optional opacity is only needed if a later art pass introduces an actual mesh-back cutout.
4. **Prefab setup:** Dynamic Rigidbody, mass 12 kg. Compound boxes for seat/back and a broad shallow base proxy; no five separate caster colliders that snag the player. `InteractionAnchor` above the seat, `OutlineAnchor` on chair centre, `CarryGrip_A/B` beneath seat sides, `LabelAnchor` on back rear. Static showroom variant has no Rigidbody.
5. **AI concept prompt:** “Single modern ergonomic office chair, orthographic three-quarter isometric game asset concept, charcoal fabric seat, shaped breathable-looking back, plain armrests and five-star base, scraped plastic, worn seat nap, dusty edges, small blank salvage sticker on rear, low-poly-friendly forms with weave as texture, neutral light, plain grey background, no readable text, no people.”

### B03 — Oak conference table (`oak_conference_table`)

1. **Item name & category:** Heavy oak conference table; `Furniture`, recoverable large item. Approximate size 3.0 × 0.76 × 1.2 m. Ground floor only.
2. **3D model specs:** 900–1,800 triangles. Medium complexity: thick bevelled top, two broad leg assemblies and cross rail. LOD0/1/2; preserve tabletop thickness and leg spacing. Use wood grain and scuffs in textures; reserve geometry for real edge chips visible in silhouette.
3. **Material & texture maps:** Base Color, Normal, Metallic/Smoothness, AO. Oak grain, rubbed satin lacquer, dusty top, dragged-edge scratches, dark water-ring mark and a partly removed shrink-wrap band. Metal brackets use exposed-metal regions. If film is modelled, use a separate low-opacity material rather than make the whole table transparent.
4. **Prefab setup:** Dynamic Rigidbody solver mass 80 kg, authoritative carrying weight configured independently. Compound BoxColliders for top and legs. Keep a broad stable support footprint. `InteractionAnchor` above a long edge; `CarryGrip_A/B` near opposite ends; `OutlineAnchor` contains all table renderers; `LabelAnchor` carries `HOLD - RECOVER HIGH VALUE`. Reserve the full rotation/carry footprint in its side bay.
5. **AI concept prompt:** “One large oak conference table awaiting mall salvage, orthographic isometric asset concept, three-metre thick rectangular tabletop with bevelled corners, two solid leg assemblies, warm oak grain, dusty lacquer, dragged-edge gouges and old water ring, short loose clear packing-film strip and orange hold-tag shape, neutral lighting, plain grey background, no readable text, no people.”

### B04 — Modular shelving (`modular_shelving`)

1. **Item name & category:** Modular metal-and-board shelving unit; `Furniture`, recoverable. Approximate size 1.20 × 1.80 × 0.42 m. Fully populated ground variant, stripped upper variant.
2. **3D model specs:** 1,000–2,000 triangles. Medium complexity: four uprights, four shelf slabs, optional cross brace and a missing shelf variant. LOD0/1/2. Render real gaps between shelves; omit unseen fasteners. Do not combine nearby loot into the shelving mesh.
3. **Material & texture maps:** Base Color, Normal, Metallic/Smoothness, AO. Painted steel uprights, veneered board shelves, rubbed edges, a dust line where stock stood, rust at feet and scattered salvage marker paint. The vacant stock rectangles should read consistently across shelves.
4. **Prefab setup:** Dynamic recoverable variant, mass 26 kg, compound BoxColliders for outer structure and selected shelves; simplify to a broad proxy if shelf gaps are too narrow to use. Static perimeter version has no Rigidbody. `InteractionAnchor` at front centre, `OutlineAnchor` at unit centre, `CarryGrip_A/B` on outer uprights and `LabelAnchor` on an upright. Loose shelf boards remain separate optional Material props.
5. **AI concept prompt:** “Single stripped modular shelving unit, orthographic isometric game prop concept, four grey metal uprights, three wood-board shelves and one missing shelf, visible cross brace, dusty empty stock rectangles, scratched edges and rusty feet, small orange salvage mark, clear open low-poly structure, neutral studio lighting, plain grey background, no readable text, no stored items or people.”

### B05 — Fabric roll (`fabric_roll`)

1. **Item name & category:** Upholstery fabric sample roll; `Material`, recoverable. Approximate size 1.15 m long, 0.25 m diameter. Both floors.
2. **3D model specs:** 400–900 triangles. Low complexity: 12–16-sided cylinder, visible cardboard core and a short rolled lip. LOD0/1/2 for grouped showroom views. The fabric tail is a fixed folded strip with no cloth simulation in the first pass.
3. **Material & texture maps:** Base Color, Normal, Metallic/Smoothness, AO. Muted blue/ochre woven fabric, cardboard core, frayed cut edge, chalky dust along the upper arc and a cleaner contact stripe. Metalness is zero except any separate buckle. Use a woven normal, not high-frequency geometry.
4. **Prefab setup:** Dynamic Rigidbody, mass 5 kg. CapsuleCollider rotated along the roll or a low-sided convex cylinder; no collider on a thin fabric tail. `InteractionAnchor` at roll midpoint, `OutlineAnchor` at centre, `CarryGrip_A/B` near each end and `LabelAnchor` on a tied paper band. Display sample-stand variant has no Rigidbody.
5. **AI concept prompt:** “One upholstery fabric roll with a visible cardboard core, orthographic three-quarter isometric salvage asset concept, muted blue woven cloth, slightly frayed cut edge, short folded tail, dust on the upper arc and blank paper stock band, believable 1.15 metre length, simple low-poly cylinder with weave shown in texture, neutral light, plain grey background, no readable text or people.”

### B06 — Flat-pack desk (`flatpack_desk`)

1. **Item name & category:** Partly disassembled flat-pack desk; `Furniture`, recoverable assembly. Approximate size 1.40 × 0.74 × 0.65 m when assembled. Both floors.
2. **3D model specs:** 800–1,600 triangles. Medium complexity: laminated top, simple side panels, metal legs and a fixed angled loose panel. LOD0/1/2. Provide assembled and disassembly-pose render variants sharing the same definition; the loose panel is part of the carried item unless deliberately registered separately.
3. **Material & texture maps:** Base Color, Normal, Metallic/Smoothness, AO. Pale laminate, raw chipboard on exposed edges, stripped screw-hole marks, scratched metal legs and dusty top. Small fasteners and missing-fixing circles belong in the texture. No emission or transparency required.
4. **Prefab setup:** Dynamic Rigidbody, mass 22 kg, compound BoxColliders matching top/legs and a substantially projecting panel. No articulated hardware in the first pass. `InteractionAnchor` above front edge, `CarryGrip_A/B` on opposite short ends, `OutlineAnchor` at assembly centre and `LabelAnchor` on the top. Static showroom pose has no Rigidbody.
5. **AI concept prompt:** “Single half-disassembled flat-pack office desk, orthographic isometric asset concept, pale laminate desktop, two simple metal legs, one side panel leaning within the desk silhouette, exposed chipboard edge and empty screw holes, dusty surface and scratched fittings, abandoned assembly mood, low-poly-friendly construction, neutral light, plain grey background, no readable text, no people or additional furniture.”

### B07 — Executive chair (`executive_chair`)

1. **Item name & category:** Padded executive office chair; `Furniture`, recoverable. Approximate size 0.72 × 1.18 × 0.76 m. Ground floor, with tipped variants as dressing.
2. **3D model specs:** 1,500–3,000 triangles. High relative complexity: segmented padded back, cushion, arm supports and caster base. LOD0/1/2. Use a few broad cushion shapes; stitching and wrinkles belong in normal detail. No cloth simulation or moving caster joints.
3. **Material & texture maps:** Base Color, Normal, Metallic/Smoothness, AO. Worn brown imitation leather, cracked armrest coating, exposed foam at one seam, dark metal base and demolition dust in padded seams. Keep tears small enough to preserve the chair silhouette and grip readability.
4. **Prefab setup:** Dynamic Rigidbody, mass 20 kg, compound seat/back boxes and smooth broad base proxy. Tipped static variants have no Rigidbody and a simple orientation-matched proxy only when blocking volume warrants it. `InteractionAnchor` at seat-front edge, `CarryGrip_A/B` beneath seat sides, `OutlineAnchor` around whole chair and `LabelAnchor` at back rear.
5. **AI concept prompt:** “One abandoned padded executive office chair, orthographic isometric game prop concept, worn brown faux leather, broad segmented back cushion, simple curved arms and caster base, cracked armrest finish, tiny exposed foam seam and white demolition dust in creases, sturdy low-poly-friendly shapes, neutral lighting, plain grey background, no readable text, no people, no extra objects.”

### B08 — Shrink-wrap roll (`shrinkwrap_roll`)

1. **Item name & category:** Commercial packing-film roll; `Material`, optional recoverable consumable. Approximate size 0.50 m long, 0.14 m diameter.
2. **3D model specs:** 350–700 triangles. Low complexity: faceted roll with cardboard core and optional short loose strip. No LODGroup for a single small item; use reduced copies when modelling bundles. The film tail is a fixed lightweight mesh, not simulated cloth.
3. **Material & texture maps:** Base Color, Normal, Metallic/Smoothness, AO; opacity only on a separate tail material. Pale translucent-looking film body can be opaque off-white with high smoothness; dirty cardboard end, dust stuck to loose film and stretched crease lines. Keep transparency local to reduce sorting issues.
4. **Prefab setup:** Dynamic recoverable roll, Rigidbody mass 1.4 kg, rotated CapsuleCollider. Static packing station variant has no Rigidbody. `InteractionAnchor` at centre, `OutlineAnchor` around roll body and `CarryGrip_A` on the cardboard core. Thin tail has no collider or outline. `LabelAnchor` can carry a stock code band.
5. **AI concept prompt:** “One commercial shrink-wrap packing-film roll, orthographic isometric asset concept, pale tightly wound clear film around a brown cardboard core, short loose crumpled tail with dust stuck in folds, simple cylinder proportions, subtle glossy highlights, low-poly-friendly studio product view, plain grey background, no readable text, no people or packing machine.”

### B09 — Heavy-duty dolly (`heavy_dolly`)

1. **Item name & category:** Industrial hand truck/dolly; `Hardware`, crew-owned support by default. Approximate size 0.55 × 1.20 × 0.55 m. Both floors, including upper manual haul staging.
2. **3D model specs:** 1,200–2,500 triangles. Medium complexity: tubular frame, toe plate, two solid wheels, rubber grips and folded straps. LOD0/1/2. Model hollow tube ends only where visible; wheel tread is normal detail. Do not add a driven vehicle controller in this asset pass.
3. **Material & texture maps:** Base Color, Normal, Metallic/Smoothness, AO. Yellow painted steel, chipped toe plate, black rubber wheels, polished hand-contact grips, accumulated drywall dust and Solutions Inc. ownership label. Straps use the fabric material with dirt at contact folds.
4. **Prefab setup:** Static parked support, no Rigidbody or loot outline; compound boxes for frame/toe plate plus smooth wheel proxies. Optional movable support variant may use one Rigidbody at 14 kg with no salvage pickup registration. `InteractionAnchor` at handle for future utility actions, `OutlineAnchor` inactive, `CarryGrip_A/B` at handle crossbar and `LabelAnchor` on frame.
5. **AI concept prompt:** “Single heavy-duty two-wheel industrial hand dolly for a mall clearance crew, orthographic isometric game asset concept, yellow tubular steel frame, scuffed toe plate, black rubber wheels, worn grip handles and folded dusty webbing strap, small contractor ownership-label shape, low-poly-friendly construction, neutral studio light, plain grey background, no readable text, no load or people.”

## Zone C — Titan Tools & Industrial: 6 assets

Tools are Hardware. Sale stock can be recovered; visibly crew-owned generators, lighting and service equipment do not present loot outlines. Cage shelves and heavy piles stay outside the protected central aisle.

### C01 — Hydraulic jack (`hydraulic_jack`)

1. **Item name & category:** Low-profile hydraulic trolley jack; `Hardware`, recoverable. Approximate size 0.70 × 0.25 × 0.32 m, with handle up to 0.8 m.
2. **3D model specs:** 1,300–2,800 triangles. High relative complexity: low frame, lifting arm, saddle, small wheels and handle. LOD0/1/2. Use fixed authored jack positions; no hydraulic mechanism simulation. Reduce small axles and handle bend segmentation at later LODs.
3. **Material & texture maps:** Base Color, Normal, Metallic/Smoothness, AO. Red painted steel, chipped saddle, oily dark hinge marks, exposed bright edges and dust on frame ledges. Surface oil is a darker smoother mask, not a transparent puddle. Retain clear yellow warning shapes and a separate salvage label.
4. **Prefab setup:** Dynamic Rigidbody, mass 24 kg, compound BoxColliders for chassis/lifting arm and a slim CapsuleCollider for a materially projecting handle. No individual wheel physics. `InteractionAnchor` above main frame, `CarryGrip_A/B` on side handles, `OutlineAnchor` at chassis centre and `LabelAnchor` on frame flank.
5. **AI concept prompt:** “One low-profile hydraulic trolley jack isolated in orthographic isometric view, red steel frame, fixed raised lifting arm and round saddle, simple wheels and upright handle, chipped red paint, greasy hinge darkening and dusty ledges, small yellow warning shapes, clear low-poly-friendly industrial asset, neutral light, plain grey background, no readable text or people.”

### C02 — Portable generator (`portable_generator`)

1. **Item name & category:** Portable cage-frame generator; `Hardware`, sale-stock recoverable variant and separate crew-owned static work variant. Approximate size 0.75 × 0.60 × 0.55 m.
2. **3D model specs:** 1,800–3,500 triangles. High relative complexity: cage rails, tank, engine block, muffler, sockets and feet. LOD0/1/2; hide engine internals and represent fins/vents with textures at distance. Work variant shares geometry and changes labels/indicator state.
3. **Material & texture maps:** Base Color, Normal, Metallic/Smoothness, AO; small optional emission for a powered status indicator only. Powder-coated frame, yellow tank, dark engine, heat discoloration around exhaust, fuel-stain rings and dusty rubber feet. Sale stock has `METAL / WEIGH`; owned equipment has a distinct contractor mark and live-power label.
4. **Prefab setup:** Sale stock: dynamic Rigidbody, mass 42 kg, compound BoxColliders for the cage and raised tank. Crew work unit: static, no Rigidbody, pickup trigger or loot outline. `InteractionAnchor` near control panel, `CarryGrip_A/B` on cage side rails, `OutlineAnchor` enabled only on sale variant and `LabelAnchor` on tank. Sound/electrical effects are separate components, not baked into mesh geometry.
5. **AI concept prompt:** “Single portable industrial generator in a rectangular protective cage, orthographic three-quarter isometric game prop concept, yellow fuel tank above a dark engine block, simple control sockets, scuffed steel rails, heat-tinted muffler and dusty rubber feet, blank contractor-label panel, believable compact low-poly forms, neutral studio lighting, plain grey background, no readable text, no cables, no people.”

### C03 — Angle grinder (`angle_grinder`)

1. **Item name & category:** Corded angle grinder; `Hardware`, recoverable. Approximate size 0.35 × 0.16 × 0.19 m.
2. **3D model specs:** 1,200–2,500 triangles. Medium-to-high small-item complexity: grip body, metal head, guard, disc and side handle. No LODGroup for a single hand tool in the first pass; optional LOD1 at about half density when many tools are visible. Keep the disc readable but omit microscopic teeth and fasteners.
3. **Material & texture maps:** Base Color, Normal, Metallic/Smoothness, AO. Scratched teal plastic, cast aluminium head, dark abrasive disc, greasy grip and drywall dust at ventilation. A short cord is a fixed cable child with no collision. Label sticker is lightly abraded.
4. **Prefab setup:** Dynamic Rigidbody, mass 2.6 kg. One CapsuleCollider along the body plus a small BoxCollider for head/disc envelope. Avoid a thin sharp disc collider. `InteractionAnchor` above body, `CarryGrip_A` along main grip, `OutlineAnchor` at tool centre and `LabelAnchor` on grip casing. No spinning disc or damage behavior is implied.
5. **AI concept prompt:** “One unplugged corded angle grinder, orthographic isometric asset concept, scratched teal grip body, cast aluminium head, stationary dark abrasive disc with guard, short side handle and clipped cable tail, grease around hand-contact areas and demolition dust in vents, clear compact low-poly-friendly shapes, neutral light, plain grey background, no text, no sparks, no people.”

### C04 — Tool chest (`tool_chest`)

1. **Item name & category:** Rolling metal tool chest; `Hardware`, recoverable large item. Approximate size 0.90 × 1.0 × 0.50 m.
2. **3D model specs:** 1,200–2,600 triangles. Medium complexity: bevelled shell, five drawer fronts, long pulls, caster blocks and side handle. LOD0/1/2. Keep drawers fixed; an open-drawer variant is a single authored assembly. Smaller tools on its top remain separate assets.
3. **Material & texture maps:** Base Color, Normal, Metallic/Smoothness, AO. Red/orange painted steel, scratched drawer pulls, worn top mat, small rust seam marks, oil smears and a broad dusty upper surface. Use normal strips for the mat texture and fasteners; salvage mark belongs on one drawer.
4. **Prefab setup:** Dynamic Rigidbody, mass 58 kg, compound boxes for shell and any fixed protruding drawer. Use a stable lower centre of mass; wheel silhouettes do not need separate physical wheels. `InteractionAnchor` at upper pull, `CarryGrip_A/B` on side handles, `OutlineAnchor` at shell centre and `LabelAnchor` on drawer face. Static bench variant has no Rigidbody.
5. **AI concept prompt:** “One rolling industrial tool chest with five fixed drawer fronts, orthographic isometric salvage game concept, red-orange painted steel, long scratched silver pulls, dark worn top mat, chunky caster silhouettes and a side handle, oil smears, dusty top and light rust at seams, blank salvage-label shape, low-poly-friendly forms, neutral light, plain grey background, no readable text or people.”

### C05 — Scaffolding pipe bundle (`scaffolding_pipes`)

1. **Item name & category:** Tied bundle of scaffolding pipes; `Material`, recoverable. Approximate size 2.0 × 0.35 × 0.35 m.
2. **3D model specs:** 800–1,800 triangles. Medium complexity: six to eight 8–12-sided pipes with visible ends and two binding straps. LOD0/1/2; collapse interior pipes to a bundle body at LOD2 while preserving outer pipe ends. Do not add every concealed tube wall.
3. **Material & texture maps:** Base Color, Normal, Metallic/Smoothness, AO. Galvanised steel, dull oxidation, rust near cut ends, bright scrape streaks, dusty upper pipes and dark textile or plastic bands. Pipeline seams and stamped marks are texture detail. Add a `METAL / WEIGH` classification label to one band.
4. **Prefab setup:** Dynamic Rigidbody, mass 50 kg. One elongated BoxCollider or one convex hull around the whole tied bundle; individual pipes remain fixed children. `InteractionAnchor` at bundle midpoint, `CarryGrip_A/B` near opposite ends, `OutlineAnchor` at centre and `LabelAnchor` on band. Static scrap-pile copy has no Rigidbody.
5. **AI concept prompt:** “One tied bundle of eight short scaffolding pipes, orthographic three-quarter isometric salvage asset concept, galvanised metal with dull oxidation and rusty cut ends, two dark binding straps, dust on upper pipes and fresh scrape streaks, visible open pipe ends, two-metre scale, clean low-poly silhouette, neutral light, plain grey background, no readable text, no people.”

### C06 — Safety harness (`safety_harness`)

1. **Item name & category:** Folded fall-arrest safety harness; `Hardware`, recoverable. Approximate folded size 0.35 × 0.50 × 0.12 m.
2. **3D model specs:** 700–1,600 triangles. Medium complexity: broad ribbon loops, two buckles and one large attachment ring. No LODGroup for the folded pickup; use LOD0/1/2 on a larger hanging support presentation. Straps are rigid authored forms in the first pass, without cloth simulation.
3. **Material & texture maps:** Base Color, Normal, Metallic/Smoothness, AO. Yellow/black woven webbing, scuffed steel buckles, dirt at folds, dust on exposed straps and a worn sewn label. Weave and stitching are normal-map detail. No opacity needed when straps are solid ribbon geometry.
4. **Prefab setup:** Dynamic folded pickup, Rigidbody mass 1.5 kg, one BoxCollider approximating the bundle. Hanging cage display is static, no Rigidbody unless intentionally recoverable. `InteractionAnchor` at top folded loop, `CarryGrip_A` at main ring, `OutlineAnchor` at bundle centre and `LabelAnchor` on sewn patch. No functional safety system is implied.
5. **AI concept prompt:** “One folded industrial fall-arrest safety harness, orthographic isometric object concept for a salvage game, broad yellow and black woven webbing loops, two simple steel buckles and a large attachment ring, dirty folds, dusty exposed straps and blank sewn label, readable low-poly-friendly strap construction, neutral lighting, plain grey background, no text, no wearer, no environment.”

## Zone D — Vogue Thread: 7 assets

Textile stock is Material. The safe and tag detachers are Hardware. Garment racks, mannequins and cash-wrap fixtures default to static retail-remnant dressing; their optional recoverable variants require explicit loot definitions. The displaced racks define a ground-floor forklift path without blocking it.

### D01 — Textile bundle (`textile_bundle`)

1. **Item name & category:** Compressed textile bale; `Material`, recoverable. Approximate size 0.75 × 0.50 × 0.55 m.
2. **3D model specs:** 500–1,100 triangles. Low-to-medium complexity: irregular bevelled bale, two bands and a few cloth fold protrusions. LOD0/1/2. Cloth scraps are geometry only where they affect silhouette; internal garments are not individually modelled.
3. **Material & texture maps:** Base Color, Normal, Metallic/Smoothness, AO. Mixed muted fabric colours, woven normal detail, compression folds, dusty top, grime around packing bands and frayed corner threads. Metalness is zero except optional band clasp. Separate `TEXTILES / BALE` sticker on the band.
4. **Prefab setup:** Dynamic Rigidbody, mass 16 kg, one BoxCollider or low-poly convex hull for the bale. No cloth simulation. `InteractionAnchor` above top band, `CarryGrip_A/B` on broad sides, `OutlineAnchor` at centre and `LabelAnchor` on band. Static pallet fill variant has no Rigidbody.
5. **AI concept prompt:** “One compressed bale of salvaged retail textiles, orthographic isometric asset concept, muted coloured folded garments forming an irregular rectangular bundle, two tight dark packing bands, frayed cloth corners, demolition dust on top and dirty compression creases, blank classification label shape, low-poly-friendly silhouette, neutral light, plain grey background, no readable text, no people or pallet.”

### D02 — Security tag detacher (`tag_detacher`)

1. **Item name & category:** Countertop security-tag detacher; `Hardware`, recoverable. Approximate size 0.12 × 0.05 × 0.12 m.
2. **3D model specs:** 250–600 triangles. Low complexity: broad rounded puck, shallow central recess and mounting flange. No LODGroup. Use 12–16-sided round forms and texture detail for screws. No internal mechanism or working security function is required.
3. **Material & texture maps:** Base Color, Normal, Metallic/Smoothness, AO. Brushed stainless top, dark plastic underside, scratched rim, dust around the screw holes and a clean mount footprint. Small retail stock identifier uses the shared label atlas. No emission or transparency.
4. **Prefab setup:** Dynamic Rigidbody, mass 0.6 kg, low-sided convex hull or shallow BoxCollider. `InteractionAnchor` above centre, `OutlineAnchor` at centre, `CarryGrip_A` at rim. Mounted support copy on a counter has no Rigidbody and no separate loot behavior unless authored as removable inventory.
5. **AI concept prompt:** “One small countertop retail security-tag detacher puck, orthographic isometric game asset concept, brushed stainless circular top with a shallow central recess, dark plastic base and two simple mounting holes, scratched rim and dusty screw recesses, compact low-poly-friendly industrial object, neutral lighting, plain grey background, no text, no tags, no hands or people.”

### D03 — Floor safe (`floor_safe`)

1. **Item name & category:** Compact floor safe; `Hardware`, recoverable heavy item. Approximate size 0.48 × 0.45 × 0.42 m.
2. **3D model specs:** 800–1,600 triangles. Medium complexity: bevelled body, thick front door, recessed dial/keypad block and bolt/hinge shapes. LOD0/1/2. Keep door closed in the first pass; an opened static version is a separate authored pose, not an unrequested lockpicking feature.
3. **Material & texture maps:** Base Color, Normal, Metallic/Smoothness, AO. Dark painted steel, bright dragged-edge scuffs, dust on top, rust freckles near anchoring holes and a cleaner strip where a removed wall covered it. Salvage stencil and empty warning-label shapes use separate decals.
4. **Prefab setup:** Dynamic loot Rigidbody solver mass 70 kg; gameplay carrying weight can be higher. One BoxCollider plus a small control-block box. `InteractionAnchor` above dial, `CarryGrip_A/B` on side edges, `OutlineAnchor` at body centre and `LabelAnchor` on door. Nonrecoverable embedded dressing copy is static with no Rigidbody.
5. **AI concept prompt:** “Single compact heavy floor safe exposed during shop demolition, orthographic isometric salvage asset concept, thick dark painted steel body and closed door, recessed dial block, simple hinges and anchor holes, dragged bright edge scuffs, dust on top and small rust freckles, orange clearance mark, solid low-poly forms, neutral light, plain grey background, no readable text or people.”

### D04 — Commercial laundry bin (`laundry_bin`)

1. **Item name & category:** Commercial wheeled textile/laundry bin; `Material`, recoverable textiles-bin variant. Approximate size 0.85 × 0.95 × 0.65 m. The catalogue groups this apparel recovery item with its textile contents; the plastic/metal surface materials remain independent of that gameplay category.
2. **3D model specs:** 900–1,900 triangles. Medium complexity: tapered tub, thick rim, four wheel blocks and grip apertures represented with shallow detail. LOD0/1/2. An optional clothing insert is static dressing within the same assembly; it is not a collection of hidden pickup objects.
3. **Material & texture maps:** Base Color, Normal, Metallic/Smoothness, AO. Scuffed blue polymer tub, dirty rim, rubber wheel scuffs, dust on an upper corner and old fabric fibres inside. Cloth insert uses the textile shared material. Contractor classification sticker remains distinct from old tenant stock tags.
4. **Prefab setup:** Dynamic empty-bin Rigidbody, mass 14 kg, compound boxes for outer shell/base or a simplified convex hull. Avoid narrow interior collisions if the bin is carried as one assembly. Static full-bin support version has no Rigidbody. `InteractionAnchor` near rim, `CarryGrip_A/B` at side grips, `OutlineAnchor` at tub centre and `LabelAnchor` on side.
5. **AI concept prompt:** “One commercial wheeled laundry bin from an abandoned clothing store, orthographic isometric game prop concept, large tapered blue polymer tub with thick rim and chunky caster wheels, scratched sides, dirty rim and dusty corners, a few muted fabric folds visible just below the rim, blank sorting-label rectangle, low-poly-friendly forms, neutral light, plain grey background, no readable text or people.”

### D05 — Stripped mannequin (`mannequin`)

1. **Item name & category:** Stripped retail display mannequin; `Furniture`, static dressing by default. Approximate height 1.70 m, base diameter 0.38 m.
2. **3D model specs:** 1,200–2,600 triangles. Medium-to-high complexity: simplified torso, head, limbs and small base. Use LOD0/1/2. Provide complete and missing-arm variants sharing materials; preserve recognisable retail mannequin anatomy without organic microdetail or clothing simulation.
3. **Material & texture maps:** Base Color, Normal, Metallic/Smoothness, AO. Matte ivory fibreglass/plastic, scratched hips and shoulders, seam lines, light dust on head and collarbone, rust on metal base. No skin material, emission or transparency. Salvage chalk marks can sit on a removable tag.
4. **Prefab setup:** Static, no Rigidbody or active loot outline. Use one torso CapsuleCollider and a broad base proxy only where it is a substantial reachable obstacle; side-bay display copies can omit player collision if route policy allows. `InteractionAnchor` at torso, inactive `OutlineAnchor`, optional `CarryGrip_A/B` at waist for a future recoverable variant and `LabelAnchor` on base. Do not add ragdoll physics.
5. **AI concept prompt:** “Single stripped clothing-store mannequin, orthographic isometric salvage-game concept, simplified matte ivory fibreglass human display form on a small metal base, one missing forearm, mould seam lines, scratches at hip and shoulder, dust on head and collarbone, clearly artificial retail object, clean low-poly-friendly silhouette, neutral lighting, plain grey background, no clothing, no readable text, no people.”

### D06 — Garment rack (`garment_rack`)

1. **Item name & category:** Empty wheeled garment rack; `Furniture`, static dressing by default. Approximate size 1.45 × 1.65 × 0.55 m.
2. **3D model specs:** 700–1,500 triangles. Medium complexity: rectangular tube frame, crossbar, feet and wheel blocks. LOD0/1/2. A few empty hangers may be simple silhouette children removed at LOD1; do not model a dense forest of thin wires.
3. **Material & texture maps:** Base Color, Normal, Metallic/Smoothness, AO. Chrome/painted steel, polished hand-contact area, dusty lower bar, scuffed wheels and slight rust around welds. Empty hangers use black plastic. A blank tenant inventory tag can hang from one end; no emission or transparency.
4. **Prefab setup:** Static support, no Rigidbody or loot outline. Compound simple boxes/capsules for visible frame volumes, positioned outside haul lanes; avoid a single huge box that blocks open usable space unnecessarily. Optional recoverable variant uses one Rigidbody at 9 kg. `InteractionAnchor` on crossbar, inactive `OutlineAnchor`, `CarryGrip_A/B` at frame ends and `LabelAnchor` on upright.
5. **AI concept prompt:** “One empty wheeled clothing-store garment rack, orthographic isometric asset concept, simple chrome tube frame with horizontal rail and broad wheeled feet, two abandoned black hangers, scuffed chrome, dust on lower crossbar and faint weld rust, low-poly-friendly open construction, neutral studio light, plain grey background, no clothes, no readable text, no people or store interior.”

### D07 — Cash-wrap counter (`cashwrap_counter`)

1. **Item name & category:** Overturned cash-wrap sales counter; `Furniture`, static dressing. Approximate upright size 1.70 × 1.0 × 0.65 m.
2. **3D model specs:** 900–1,900 triangles. Medium complexity: laminate shell, checkout ledge, two drawer faces and open service recess. LOD0/1/2. Author overturned orientation as the prefab pose; removable tills and detachers are separate assets rather than fused valuables.
3. **Material & texture maps:** Base Color, Normal, Metallic/Smoothness, AO. White laminate, raw chipboard where one corner broke, dark rubber edge strip, till-mount screw holes, scraped side panel and heavy dust on newly exposed faces. A faded Vogue Thread colour band can identify the former retailer without cluttering the whole surface.
4. **Prefab setup:** Static, no Rigidbody or pickup outline. Compound BoxColliders align with the overturned shell; keep it in a side pocket with clearance for detacher pickups. `InteractionAnchor` near accessible counter face, inactive `OutlineAnchor`, `LabelAnchor` on front fascia; optional `Attachment_Detacher` and `Attachment_Till` locate separate props. No new cashier gameplay is implied.
5. **AI concept prompt:** “One overturned apparel-shop cash-wrap counter, orthographic isometric demolition salvage asset concept, white laminate rectangular counter with a dark edge band, simple checkout ledge and drawer fronts, broken corner exposing chipboard, mounting holes where till was removed, scraped side and drywall dust on exposed surfaces, low-poly-friendly construction, neutral light, plain grey background, no readable text, no people, no extra cash register.”

## Zone E — Central concourse and clearance staging: 15 assets

The dry fountain and route-safe sorting pad host the salvage staging hub. Most E equipment is crew-owned static support rather than sale inventory. Labeled cargo shows the operation; payment and extraction remain at the existing front truck. Loaded stacks stay low enough to preserve views and never consume protected circulation.

### E01 — Wooden shipping pallet (`shipping_pallet`)

1. **Item name & category:** Heavy wooden shipping pallet; `Material`, static staging support; empty recoverable variant optional. Approximate size 1.20 × 0.15 × 1.0 m.
2. **3D model specs:** 600–1,200 triangles. Low-to-medium complexity: slats, three bearers and bottom runners with readable fork openings. LOD0/1/2. Use cracks and nail heads in textures, not geometry; preserve the fork gap in near views and collapse concealed underside pieces at distance.
3. **Material & texture maps:** Base Color, Normal, Metallic/Smoothness, AO. Weathered softwood grain, chipped board ends, dusty top slats, dark lift marks and a faded clearance stencil. Metalness zero for timber; nails can use a small metallic region. Separate classification tags identify loads, not every slat.
4. **Prefab setup:** Static pallet stacks have no Rigidbody and use a few BoxColliders around substantial occupied volumes. Optional empty-pallet loot uses one Rigidbody at 18 kg and compound slat/bearer boxes; avoid many small snagging colliders along a floor route. `InteractionAnchor` at front upper edge, inactive/optional `OutlineAnchor`, `CarryGrip_A/B` at side runners and `LabelAnchor` on a bearer.
5. **AI concept prompt:** “One heavy wooden shipping pallet, orthographic isometric salvage-game asset concept, weathered softwood slats and three bearers with readable forklift openings, chipped board ends, dusty top, dark handling marks and faded stencil shapes, realistic 1.2 by 1 metre proportions, clear low-poly-friendly construction, neutral studio light, plain grey background, no load, no readable text, no people.”

### E02 — Industrial dumpster (`industrial_dumpster`)

1. **Item name & category:** Compact industrial scrap dumpster; `Hardware`, crew-owned static support. Approximate size 1.80 × 1.10 × 1.20 m.
2. **3D model specs:** 1,200–2,400 triangles. Medium complexity: bevelled tub, reinforced rim, fork sleeves and optional fixed lid. LOD0/1/2. A shallow scrap insert uses simple silhouettes; do not create a deep internal physics pile. Maintain a legible tub profile from the concourse.
3. **Material & texture maps:** Base Color, Normal, Metallic/Smoothness, AO. Dark green/grey painted steel, scraped rim, rust near seams, dents represented by mesh or broad normal detail, dust streaks and contractor ownership mark. Scrap insert shares metal/debris materials and does not imply recoverable inventory.
4. **Prefab setup:** Static, no Rigidbody, loot registration or active outline. Use compound BoxColliders for the shell or a simple occupied-volume proxy where the interior is inaccessible. `InteractionAnchor` near rim for any later sorting action, inactive `OutlineAnchor`, `LabelAnchor` on outer face and `Attachment_Scrap` under rim. Place outside the permanent ring; no new disposal gameplay is added by this asset.
5. **AI concept prompt:** “One compact industrial scrap dumpster for a shopping-mall clearance crew, orthographic isometric asset concept, reinforced dark green painted-steel tub, thick rim and simple fork sleeves, scraped edges, seam rust, shallow dusty metal scrap visible inside and blank contractor-label rectangle, solid low-poly-friendly shapes, neutral lighting, plain grey background, no readable text, no people.”

### E03 — Hydraulic pallet jack (`pallet_jack`)

1. **Item name & category:** Manual hydraulic pallet jack; `Hardware`, crew-owned static support. Approximate size 1.55 × 1.20 × 0.65 m, including handle.
2. **3D model specs:** 1,700–3,300 triangles. High relative complexity: paired long forks, compact pump block, steering wheels and upright loop handle. LOD0/1/2. Keep the forks' clearance and length readable; collapse axles and pump detail at later LODs. No driven vehicle physics in this pass.
3. **Material & texture maps:** Base Color, Normal, Metallic/Smoothness, AO. Yellow painted forks, scraped steel tips, dark hydraulic body, worn rubber handle and dusty wheels. Oil/grease darkens the pump area; Solutions Inc. ownership mark distinguishes it from recoverable stock.
4. **Prefab setup:** Static parked support, no Rigidbody or salvage outline. Compound BoxColliders for each fork/pump and a smooth CapsuleCollider for the handle only if substantial; keep its fork tips out of travel lanes. Optional movable support uses one 55 kg Rigidbody and utility interactions, not a loot definition by default. `InteractionAnchor` at handle, inactive `OutlineAnchor`, `CarryGrip_A/B` on handle and `LabelAnchor` on pump housing.
5. **AI concept prompt:** “One manual hydraulic pallet jack isolated in orthographic isometric view, long paired yellow steel forks, compact dark pump block, black steering wheels and upright loop handle, polished scrape marks at fork tips, dust on wheels, greasy pump area and blank contractor ownership sticker, practical low-poly-friendly industrial game asset, neutral light, plain grey background, no pallet, no readable text or people.”

### E04 — Cargo crate (`cargo_crate`)

1. **Item name & category:** Labeled wooden cargo crate; `Material`, static extraction-ready support by default. Approximate size 0.85 × 0.75 × 0.65 m.
2. **3D model specs:** 600–1,200 triangles. Low-to-medium complexity: plank box, reinforced corner battens and fixed lid. LOD0/1/2. Provide three size variants sharing texture and label atlas. The crate is not automatically a loot container; contents and container interaction need explicit gameplay definitions.
3. **Material & texture maps:** Base Color, Normal, Metallic/Smoothness, AO. Pale plywood/planks, nail marks, scraped corners, dust, banding rubs and large readable label panel. Use authored `TECH / TEST`, `METAL / WEIGH`, `TEXTILES / BALE` or `HOLD / FRAGILE` label variants. Wood is dielectric; small metal fasteners use metallic patches.
4. **Prefab setup:** Static staging variant, no Rigidbody or loot outline, one BoxCollider for occupied crate volume. An explicitly recoverable closed crate can use one 12–40 kg Rigidbody depending on gameplay contents. `InteractionAnchor` above front edge, inactive/optional `OutlineAnchor`, `CarryGrip_A/B` on side grips and `LabelAnchor` on front face. Stacks stay below the 1.4 m sightline target.
5. **AI concept prompt:** “One reinforced wooden shipping cargo crate awaiting extraction, orthographic isometric salvage game concept, pale plywood panels, chunky corner battens, fixed lid, dusty top, scraped corners and band rub marks, large blank classification-label panel on front, realistic compact crate proportions, simple low-poly construction, neutral light, plain grey background, no readable text, no people or additional crates.”

### E05 — Plastic sorting crate (`plastic_sorting_crate`)

1. **Item name & category:** Stackable plastic classification crate; `Material`, static sorting support. Approximate size 0.60 × 0.32 × 0.40 m.
2. **3D model specs:** 650–1,300 triangles. Medium complexity: tapered body, thick rim, grip recesses and exterior ribs. LOD0/1/2; texture small ventilation slots rather than cut many holes. Filled and empty render variants share geometry; contents are shallow static inserts unless separately registered.
3. **Material & texture maps:** Base Color, Normal, Metallic/Smoothness, AO. Blue/grey polypropylene, pale scratches at rim, dust trapped between ribs, dirty handgrips and a large white classification label. No metalness on plastic. Use different label colours for Tech and textiles while keeping the same contractor visual language.
4. **Prefab setup:** Static stacked support, no Rigidbody or active outline. Use a BoxCollider per meaningful crate stack or per individual crate as needed; no tiny rib colliders. Optional carryable empty-crate variant uses one 2 kg Rigidbody. `InteractionAnchor` at rim, inactive/optional `OutlineAnchor`, `CarryGrip_A/B` beside grips and `LabelAnchor` on short side.
5. **AI concept prompt:** “One stackable plastic sorting crate for an industrial clearance crew, orthographic isometric game prop concept, blue polypropylene tapered box, thick rim, recessed handgrips and broad external ribs, pale scratches, dusty rib valleys and dirty grip areas, large blank white classification label, simple low-poly-friendly forms, neutral light, plain grey background, no readable text, no contents or people.”

### E06 — Mobile work floodlight (`work_floodlight`)

1. **Item name & category:** Twin-head mobile floodlight mast; `Hardware`, crew-owned static support. Approximate size 1.0 × 2.20 × 1.0 m.
2. **3D model specs:** 1,500–3,000 triangles. Medium-to-high complexity: weighted/tripod base, mast, two rectangular lamp heads and fixed short cable. LOD0/1/2; reduce lamp fins, stand segmentation and fasteners at distance. Light cones are separate optional atmospheric renderers, not geometry inside the base asset.
3. **Material & texture maps:** Base Color, Normal, Metallic/Smoothness, AO, plus Emission for lamp faces. Yellow painted frame, aluminium heads, dusty housing, rubbed adjustment knobs and contractor label. Lamps use a separate emissive material; actual illumination comes from child Light components. No blanket transparency on the prop.
4. **Prefab setup:** Static, no Rigidbody, pickup trigger or loot outline. Compound boxes/capsules for base and mast; route-safe placement is crucial because tripod feet spread. `InteractionAnchor` at switch height, inactive `OutlineAnchor`, `LabelAnchor` on mast, `Light_Left`/`Light_Right` attachment children at lamp faces and `CableExit` at base. Add separate light/cable objects without rigid joints.
5. **AI concept prompt:** “One industrial mobile twin-head work floodlight on a tall mast, orthographic isometric salvage-game asset concept, wide weighted tripod base, yellow painted stand, two rectangular aluminium lamps with pale bright faces, scratched adjustments, dusty housings and short black cable exit, blank contractor-label shape, clear low-poly construction, neutral ambient light, plain grey background, no readable text or people.”

### E07 — Generator hub (`generator_hub`)

1. **Item name & category:** Temporary clearance power distribution hub; `Hardware`, crew-owned static assembly. Approximate occupied size 1.50 × 0.90 × 1.10 m, excluding wall-routed cables.
2. **3D model specs:** 1,800–3,500 triangles. High relative complexity: compact generator body, protective rail, distribution box and three short bundled cable exits. LOD0/1/2. This is a hub composition, distinct from sale-stock portable generators; reuse their submeshes where appropriate rather than duplicate detailed internal engine art.
3. **Material & texture maps:** Base Color, Normal, Metallic/Smoothness, AO, small Emission mask for distribution status indicator. Scuffed yellow/black metal, grey electrical-box plastic, dusty cable jackets, oily generator feet and readable `LIVE TEMP POWER` contractor label. No screen or large glow is required.
4. **Prefab setup:** Static, no Rigidbody or salvage outline. Compound BoxColliders for base and distribution box; thin cable tails are nonblocking. `InteractionAnchor` at distribution switch, inactive `OutlineAnchor`, `LabelAnchor` on box, `CableExit_A/B/C` near sockets and `AudioAnchor` near generator engine. Long crosswalk cables use separate safe routing/cable-ramp dressing.
5. **AI concept prompt:** “One temporary industrial power hub for a mall salvage crew, orthographic isometric game asset concept, compact yellow-and-black generator in a protective rail beside a grey power distribution box, three short neatly zip-tied black cable exits, dusty casing, scratched paint and oily feet, blank live-power label panel, practical low-poly-friendly assembly, neutral light, plain grey background, no readable text, no people.”

### E08 — Demolition fence (`demolition_fence`)

1. **Item name & category:** Temporary chain-link safety fence panel; `Hardware`, static support. Approximate size 2.0 × 2.0 × 0.45 m including feet.
2. **3D model specs:** 700–1,600 triangles. Medium complexity: tube perimeter, two feet, simple clamps and one chain-link plane. LOD0/1/2 on the frame; chain-link pattern uses an alpha-cutout material, not individually modelled wire diamonds. A later fully opaque simplified fence LOD can reduce fine-pattern shimmer.
3. **Material & texture maps:** Base Color, Normal, Metallic/Smoothness, AO and alpha-cutout Opacity for chain link. Galvanised steel frame, rust at clamps, dusty feet and torn clearance-tag corners. The mesh plane can share a repeatable chain-link opacity tile while contractor signs use a separate label plane.
4. **Prefab setup:** Static, no Rigidbody or loot outline. Use one thin BoxCollider across the blocking panel plus simple foot proxies when necessary; keep fence returns in side bays and parallel to circulation. `InteractionAnchor` at a mounted sign, inactive `OutlineAnchor`, `LabelAnchor` at 1.5 m height and `Join_Left/Right` sockets for adjacent panels. No autonomous gate motion is part of this asset.
5. **AI concept prompt:** “One temporary demolition safety fence panel, orthographic three-quarter isometric asset concept, galvanised rectangular tube frame, chain-link infill, two broad stabilising feet and simple joining clamps, rusty clamps, dusty base and a blank orange clearance sign attached at chest height, low-poly-friendly frame with chain link as surface pattern, neutral light, plain grey background, no readable text or people.”

### E09 — Orange safety netting (`orange_netting`)

1. **Item name & category:** Orange demolition safety-net section; `Material`, static visual support. Approximate span 2.0 × 1.10 m with fixed posts or attached to an existing fence.
2. **3D model specs:** 200–700 triangles for a lightly sagging strip, plus simple posts when supplied. Low complexity. No LODGroup for short netting; distance culling or a low-density plane is enough. Use alpha-cutout holes and fixed folds; no cloth solver in the first pass.
3. **Material & texture maps:** Base Color, Normal, Metallic/Smoothness, AO, alpha-cutout Opacity. Faded safety orange plastic, stretched grid holes, torn lower corner, dust caught on upper folds and small dark zip-tie shapes. Plastic remains dielectric and rough. Post metal uses the shared metal surface.
4. **Prefab setup:** Static, no Rigidbody, pickup trigger or outline. Net itself is nonblocking visual dressing; fence/posts carry any required barrier collider. `Attachment_Left/Right` locates ties, `LabelAnchor` can hold a warning tag and inactive `OutlineAnchor` is optional. Avoid invisible collision spanning a route merely because a transparent strip is present.
5. **AI concept prompt:** “One short section of orange construction safety netting, orthographic isometric game asset concept, faded orange perforated plastic mesh sagging between two simple dark posts, torn lower corner, dusty upper folds and visible black zip ties, low-poly fixed-fold geometry with grid holes as opacity texture, neutral light, plain grey background, no readable text, no people or surrounding construction site.”

### E10 — Heavy dust sheet (`dust_sheet`)

1. **Item name & category:** Heavy canvas/poly dust sheet; `Material`, static dressing. Approximate 1.8 × 1.4 m draped footprint with up to 0.5 m height over its supporting object.
2. **3D model specs:** 700–1,500 triangles. Medium complexity from broad folds, hanging corner and uneven hem. LOD0/1/2 for large sheets; omit fine wrinkles at LOD1/2. Author one draped crate pose and one hanging-partition pose. Do not bake the underlying loot mesh into the cloth asset.
3. **Material & texture maps:** Base Color, Normal, Metallic/Smoothness, AO. Dirty off-white canvas or thick pale polyethylene, chalky dust, dark fold valleys, dragged corner grime and a faint orange contractor spray mark. Opaque canvas is preferred for the first pass; only a separate thin-poly variant needs local opacity.
4. **Prefab setup:** Static, no Rigidbody, cloth simulation or pickup outline. No collider on the sheet itself; underlying crate/wall supplies necessary collision. `Attachment_Top` and `Attachment_Corner` locate fixed ties, `LabelAnchor` on an accessible fold, inactive `OutlineAnchor` optional. Avoid covering every grip of an intended recoverable prop.
5. **AI concept prompt:** “One heavy dusty off-white canvas sheet draped over a simple implied rectangular load, orthographic isometric salvage-game asset concept, broad fixed folds, one loose dragged corner, chalky drywall dust, dark fold valleys and faint orange spray-paint mark, low-poly-friendly drapery with wrinkles as texture, neutral light, plain grey background, no readable text, no visible cargo, no people.”

### E11 — Exposed partition (`exposed_partition`)

1. **Item name & category:** Demolished drywall partition segment; `Material`, static architectural dressing attached to the existing structural-section workflow. Approximate size 2.0 × 2.40 × 0.15 m.
2. **3D model specs:** 1,100–2,500 triangles. Medium complexity: three steel studs, one track, two broken plasterboard islands and fixed conduit. LOD0/1/2; fracture perimeter geometry is broad and readable. Do not add a new structural wall or passage simply to place this decorative segment.
3. **Material & texture maps:** Base Color, Normal, Metallic/Smoothness, AO. Painted drywall front, chalky gypsum at torn edges, galvanised studs, dull conduit and dust. Add `ZONE B - CLEAR FOR DEMO` via an authored label/spray decal near a side-bay gate, with the adjacent high-value hold tag handled separately.
4. **Prefab setup:** Static, no Rigidbody or loot outline. BoxCollider on substantial remaining wall volumes; use existing structure proxies and avoid duplicate blocking colliders when mounted on existing walls. Thin conduit is visual. `Attachment_Section` binds to the owning structural section, `LabelAnchor` at eye height, `DebrisAnchor` at base and inactive `OutlineAnchor` optional. Detached collapse fragments follow the project's existing nonblocking cosmetic policy.
5. **AI concept prompt:** “One two-metre-wide partially demolished interior drywall partition segment, orthographic isometric game environment asset concept, three exposed galvanised steel studs, jagged white gypsum board remnants and visible grey conduit, chalky dust on torn edges, orange spray-tag shape on surviving panel, clean readable low-poly fracture silhouette, neutral light, plain grey background, no readable text, no people or room.”

### E12 — Cable bundle (`cable_bundle`)

1. **Item name & category:** Zip-tied temporary power cable bundle; `Material`, static support dressing. Approximate segment 2.0 m long with 0.04–0.08 m diameter; ceiling variant hangs above 2.2 m on routes.
2. **3D model specs:** 400–1,000 triangles. Low-to-medium complexity: three 6–8-sided curved cable paths and a few zip-tie bands. LOD0/1/2 for long visible runs; collapse to a single thicker cable at distance. Use fixed paths rather than rope physics; provide wall-base and hanging variants.
3. **Material & texture maps:** Base Color, Normal, Metallic/Smoothness, AO. Matte black rubber, rubbed bends, dusty upper surfaces, colour-coded tape ends and pale plastic ties. Metalness zero on jacket/ties. Do not make cables glow; the powered story is conveyed by their route and labels at the hub.
4. **Prefab setup:** Static, no Rigidbody or loot outline, and no individual cable collider. Wall-base cables stay outside circulation; crossings use a low smooth cable-ramp proxy or purely visual flat routing. `Cable_Start/End` sockets locate segment joins; `LabelAnchor` near the coloured tape and inactive `OutlineAnchor` optional. Hanging cables never become accidental head-height barriers.
5. **AI concept prompt:** “One two-metre temporary power cable bundle isolated in orthographic isometric view, three curved matte black rubber cables tied together with pale zip ties, colour-coded tape at short ends, rubbed bends and demolition dust on upper surfaces, fixed low-poly-friendly geometry, neutral studio light, plain grey background, no plugs connected to equipment, no readable text, no people.”

### E13 — Salvage status tag (`salvage_tag`)

1. **Item name & category:** Mounted clearance status tag/decal; `Material`, static narrative support. Approximate panel 0.50 × 0.30 m, paper or spray-decal variant.
2. **3D model specs:** 2–24 triangles. Very low complexity: flat decal/plane, optional bent corner and small mounting tab. No LODGroup. Readability matters more than geometry. Author exact wording on a separate label atlas rather than request reliable typography from generated art.
3. **Material & texture maps:** Base Color, Normal, Metallic/Smoothness, AO; alpha Opacity only for torn-paper or sprayed-edge variants. Dusty white/orange paper, scuffed printed black lettering, fold creases and overspray edges. Required text variants: `HOLD - RECOVER HIGH VALUE`, `ZONE B - CLEAR FOR DEMO`, `SALVAGE FIRST`, `LIVE TEMP POWER`, plus contractor identification.
4. **Prefab setup:** Static, no Rigidbody, collider or loot outline. `LabelAnchor` defines the front reading plane; `Attachment_Top` locates a tie if used. Offset the surface slightly from the host mesh to avoid overlap. Place doorway tags at 1.4–1.7 m and keep them outside player prompt areas that would obscure reading.
5. **AI concept prompt:** “One worn industrial salvage status tag as an isometric paper object concept, rectangular dusty white card with orange border, bent corner, torn mounting hole and black stencil-like placeholder strokes, subtle creases and spray overshoot, neutral studio lighting, plain grey background, clean simple game-decal reference, no actual readable lettering, no person or wall.”

### E14 — Forklift tire-track patch (`tire_track`)

1. **Item name & category:** Forklift tire tracks through drywall dust; `Material`, static floor decal/dressing. Approximate patch 2.0 × 1.20 m, straight and curved variants.
2. **3D model specs:** 2–12 triangles. Very low complexity: floor-aligned plane or compatible decal representation. No LODGroup. Broad tread silhouette and dust-cleared edges live in opacity/Base Color; do not extrude a trail into physical geometry.
3. **Material & texture maps:** Base Color, subtle Normal, Metallic/Smoothness, AO, alpha Opacity. Grey-white drywall dust displaced by two dark rubber tread tracks, cleaner concrete visible between tread blocks, scuffed curve edges and lighter feathered dust outside the mark. Low smoothness, metalness zero. Use transparent edges and avoid an opaque rectangular footprint.
4. **Prefab setup:** Static, no Rigidbody, collider, pickup trigger or outline. `Attachment_Start/End` supports aligning sequential track segments; no interaction socket is needed. Apply only to ground-floor routes from real loaded pallets through broad store aisles toward E. Do not place upstairs forklift tracks or tracks through counters and solid walls.
5. **AI concept prompt:** “One isolated ground-plane asset concept of paired forklift tire tracks pressed through pale drywall dust, orthographic three-quarter isometric view, dark rubber tread blocks exposing cleaner concrete, subtle curved motion, feathered dusty edges, flat decal-like appearance with no raised geometry, neutral even lighting, plain grey background, no vehicle, no readable text, no people or surrounding floor scene.”

### E15 — Broken-glass patch (`broken_glass`)

1. **Item name & category:** Small scattered broken display/skylight glass patch; `Material`, static floor dressing. Approximate footprint 1.0 × 0.8 m, shard height below 0.03 m.
2. **3D model specs:** 100–450 triangles. Low complexity: eight to sixteen broad flat shards plus optional faint sparkle decal. No LODGroup; use culling or a simplified decal at distance. Limit piece count and avoid particle-style geometry density around the entire concourse.
3. **Material & texture maps:** Base Color, Normal, Metallic/Smoothness, AO; optional low Opacity on one glass-only material. Pale green-grey tint, dusty surfaces, sharp-edge highlight bands and restrained high smoothness on cleaner fragments. For the first pass, opaque tinted shards with modest reflections are acceptable and avoid transparent sorting complexity. Glass is dielectric, not metallic.
4. **Prefab setup:** Static, no Rigidbody, player collision, pickup trigger or outline. `Attachment_Surface` aligns the patch to the floor; optional `DamageSourceAnchor` ties its placement to a smashed case or visually fractured skylight. Keep the roof's physical closure intact. Shards are cosmetic and never individually obstruct walking or carrying.
5. **AI concept prompt:** “One small scattered patch of broken retail display glass as an orthographic isometric game asset concept, eight to sixteen thin irregular pale green-grey shards lying flat, restrained clean edge highlights, dust on broad faces and a few tiny chips represented as surface detail, low-poly-friendly silhouettes, neutral studio light, plain grey background, no container, no readable text, no people.”

## Dressing recipes and implementation boundary

The implementation entry is `SalvageAssetBuilder.AuthorAll()` under **Tools → Abandoned → 0.15.5 → Author Salvage Asset Library**. It authors the following assets without entering play mode or running checks:

- Collider-free art prefabs: `Assets/_Project/Art/Generated/Salvage0155/Models/{id}.prefab`.
- Persistent combined mesh assets: `Assets/_Project/Art/Generated/Salvage0155/Models/Meshes/{id}_LOD{n}_{finish}.asset`; existing mesh GUIDs are retained when rebuilding.
- Shared PBR textures/materials and focus-outline material: `Assets/_Project/Art/Generated/Salvage0155/Materials/`.
- Static dressing variants: `Assets/_Project/Prefabs/SalvageDressing/{id}.prefab`; all 45 variants have no Rigidbody and no loot pickup registration.
- Recoverable item definitions: `Assets/_Project/Data/Loot/Loot_salvage_{id}.asset`; 35 definitions have individual value, weight, carry class and model data.
- Standard registered network loot prefabs: `Assets/_Project/Prefabs/Loot/Loot_salvage_{id}.prefab`; the existing generator calls `SalvageLootPrefabBuilder.Apply()` to install proxy colliders, focus outline and category metadata.
- Construction manifest: `Assets/_Project/Art/Generated/Salvage0155/SalvageManifest.json`; records effective tuning, dimensions, paths and combined recipe triangle totals per LOD. Those totals are construction bookkeeping, not a test or audit result.

Art and static variants use a floor-contact pivot. Loot models are fitted into the existing centre-pivot pickup prefab, with root interaction/label/grip sockets in that centred space. Heavy/Huge definitions receive four grip points and initial two/three-carrier requirements without changing their gameplay weight to match the physics mass cap. Existing inspector tuning survives a library rebuild. Readable labels use the project's depth-tested world-text material, including recovery labels for crates/batteries, a server-rack hold tag, temporary-power identification and the general salvage-first tag.

These recipes reuse the 45 assets rather than expand the catalogue with unrequested gameplay systems:

- **Interrupted Tech recovery:** PC tower beside an empty display rectangle; two circuit boards on a static shelf insert; rear server rack with hold label; copper spool and battery at the pallet edge. Place broken glass beneath the damaged display, not everywhere in A.
- **Two-floor Furniture identity:** Ground B groups cabinet, table, executive chair and dolly around its heavy-haul side bay. Upper B repeats the same retailer colours with desks, ergonomic chairs, stripped shelving and fabric sample rolls. Use identical label treatment and matching sign bands on both floors; use existing concourse circulation between them.
- **Hardware crib:** Tool chest and jack at the cage perimeter; grinders on a half-cleared bench; harness on a hook; tied pipe bundle beside the crib. Separate crew-owned generator paint/label treatment from sale inventory so focus outlines never suggest recovering the live power supply.
- **Interrupted Apparel packing:** Static cash wrap, displaced racks and stripped mannequins frame recoverable textiles, detachers and safe. Put demolition netting/partition fragments in the stock-wall side pocket, leaving the former shop aisle open.
- **Central staging:** Label cargo crates by classification, align pallet fork openings with the front extraction route, place plastic sorting crates at Tech/textile ends and keep hub cables wall-routed. Work lights illuminate sheet folds and existing dust effects without filling the court with physics props.

Show dust as one coherent fall pattern, then cut tire tracks through it. Let exposed framing sit next to partly wrapped stock and contradictory clearance tags so demolition and recovery read as overlapping operations. Keep tall support assets outside protected doors, haul lanes, flight approaches, player/threat/loot spawn reservations and truck approaches. The existing approved layout and structural attachment rules govern final placement.

**0.15.5 handoff:** Mesh authoring, surface maps, material definitions, readable label variants, prefab/interaction authoring and catalogue integration belong to the build. Network ownership, authoritative value/weight, player pickup and extraction should continue through the existing project systems. Tests, compilation, gameplay review, visual inspection and geometry/physics verification are deliberately deferred to the user as requested.
