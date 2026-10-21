# Map and object art remaster — 2026-10-22

The saved HQ, Mall, School, Factory, Prison and Tutorial scenes receive this pass. It extends the
[lighting rebuild](LIGHTING_REBUILD.md) with changes to surface materials, prop meshes, architectural glazing,
wall junctions and existing particle effects. Unity 6.3 / URP 17.3 remain the rendering stack.

## Surface and object changes

- Shared material assets keep their GUIDs. The material pass measures UV density across the maps and applies
  physical texture scales: small terrazzo aggregate, fine fabric weave, believable snow and appropriate tile sizes.
  Paint, bare metal, wood, cloth, leather, paper, rubber and concrete have distinct normal strength, roughness and
  metalness. Broad grime is reduced on painted fittings. New CC0 paper/cardboard scans join the existing library;
  [asset credits](ASSET_CREDITS.md) record their sources.
- Printed artwork and palette maps retain their authored UVs. A separate detail-normal layer supplies close detail
  without overwriting their colours. The custom Surface shader now evaluates that layer. Existing roof artwork is
  restored where an earlier scan pass had substituted gravel. Portable loot uses local texture mapping so the finish
  follows an object when it moves.
  The truck retains its authored vehicle paint, packed wear/response maps, glass and rubber; generic salvage scans
  no longer overwrite them. Painted fittings and polished hardware use fine surface detail rather than galvanized blotches.
- Recognizable closed box components in furniture, machinery, shelving and loot receive small inward bevels.
  These are real replacement render meshes, including rotated parts inside combined meshes. Material slots, vertex
  colours and UV channels survive. Source meshes, imported readability, transforms, collider geometry, LOD membership
  and gameplay components retain their authored values. Lower LODs, characters and structural panels are excluded.
- Architectural glazing gains transparent centres, perimeter deposits, wipe variation, fine normals and varying
  roughness. Glass uses premultiplied URP lighting. Where the geometry permits it, fitted wall/floor contact seals
  and capped skirting add depth. Raycasts avoid door openings and outdoor walls; spatial chunks limit extra draws.
- Existing snow, dust, steam and spark emitters get mipmapped soft sprites, camera/depth fading and bounded sizes.
  Dust, snow and steam use URP Particles/Lit so local practical lights can illuminate them. Spark trails use additive
  shading. Emitter locations, triggers, shapes, collision, emission and random seeds are preserved.
- Render review exposed overly dark areas between practical lights. Floor-receiver targets now range from 1.25
  to 1.5 by map, with a 0.32 bounce reflectance and 5.5 m maximum bounce radius. The mall also gets a modest ambient
  lift. Fixture positions, power and failure states still shape the light, and bounce retains the same light budget.

## Opening and reapplying

1. Open the project in Unity and let asset import finish.
2. Open HQ or another saved map and press **Play**. No manual material assignment or map rebuild is required.

**Tools > Abandoned > Art > Remaster All Map Art and Objects** reapplies the pass to all saved maps and prop prefabs.
It also reapplies the lighting rebuild. Rebuild All Maps invokes this after the older art passes so their settings
cannot overwrite the remaster. An individual map rebuild should be followed by this menu action.

Batch equivalent, with Unity closed:

```sh
Tools/unity.sh exec Abandoned.EditorTools.VisualRemaster.ApplyAndExit
```

F1 includes the scene's refined renderer, wall junction and particle counts. Mesh tuning lives in
`Art/Generated/ObjectRemaster/ObjectRemasterConfig.asset`; particle tuning lives in
`Data/Core/Resources/VisualEffectsConfig.asset`. Authoring uses Unity Editor APIs and records a report under
`Docs/renders/art-remaster/AuthoringReport.txt`.

## Render review

```sh
ART_REVIEW=1 LIGHTING_REVIEW=art-remaster Tools/unity.sh exec Abandoned.EditorTools.LightingReview.CaptureAndExit
```

Add `LIGHTING_MAP=Mall` to limit the review. The output uses display-space sRGB PNGs with production fog, exposure,
grading, selected shadows and local bounce. Art review initializes fixture state and temporarily renders the nearest
reflection probes so close surface response can be judged in batch mode. It also captures flashlight, surface and
prop detail views where suitable viewpoints exist. Probe assets and camera changes are discarded afterward.

These views show saved-map art, rather than a networked gameplay benchmark. The bevel pass improves existing forms;
it does not replace every model with a new sculpt. Runtime reflection refresh, full gameplay frame time and Windows
appearance still require an interactive hardware playtest.

## Validation

The saved scenes contain **7,878 refined prop renderers**, **281 fitted wall/floor junctions** in 92 spatial mesh chunks
(4,702 triangles), and **49 refined particle emitters**. The first prefab application updated 110 prop/loot/equipment
prefabs; later runs recorded only changes or migrations. More than 500 shared opaque material assets were processed,
with the truck's dedicated finishes restored separately. The final repeat made no further mesh replacements or
new cache assets. Cleanup removed 1,722 obsolete intermediate meshes; 1,710 referenced generated meshes remain.

| Map | Refined prop renderers | Fitted junctions | Particle emitters |
| --- | ---: | ---: | ---: |
| HQ | 824 | 1 | 10 |
| Mall | 519 | 0 | 22 |
| School | 3,431 | 102 | 5 |
| Factory | 1,605 | 49 | 5 |
| Prison | 1,245 | 86 | 2 |
| Tutorial | 254 | 43 | 5 |

- Unity compilation, application and all six map render captures completed. No scene/prefab YAML was hand-edited.
- Full EditMode suite: **401/401 passed**, covering semantics, physical scale, artwork, portable mapping, geometry,
  migration, collider isolation, doorway/outdoor exclusion and particle behaviour. Material and lighting migration
  regressions were also rerun in focused batches during iteration.
- Selected PlayMode checks: **12/12 passed** (mall power/fixture behaviour, local bounce and reflection startup,
  structural presentation/collapse, and render budget). The mall recorded 4,519 / 10,200 / 8,229 / 4,465 batches at
  the existing four views, below the unchanged 12,500 limit. Those views submitted 2.608 / 5.386 / 4.235 / 2.219 million
  triangles. These are batch regression counters, not a gameplay FPS measurement.
- Final review produced 30 PNGs across all six maps, with no render exceptions or shader errors. Vehicle paint was
  restored and the final images recaptured after the painted-metal correction.
- Suggested commit: `Remaster map materials, prop geometry and lighting`.
