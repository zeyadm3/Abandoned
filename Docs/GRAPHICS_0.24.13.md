# 0.24.13: texture flicker fixed, surfaces and lighting rebuilt

The user (2026-10-21): "the graphics in the textures I still think a 3 out of 10. I want them to be at least an eight ...
a lot of texture bugs in every single map ... the textures are overlapping and switching between them ... Fix that in all
the maps, fix all the textures. I want better textures ... improve all the shadows and the lighting." Sound unchanged.
Branch `worktree-graphics-overhaul` (from main 73abb561). Maps, gameplay and UI unchanged.

## What was wrong

Measured with `Editor/Probes/GraphicsAudit` (batch: `Tools/unity.sh exec Abandoned.EditorTools.GraphicsAudit.RunAndExit`,
CSVs in Game/Logs/graphics):

1. **The flicker.** Thousands of pairs of faces lay exactly in the same plane and overlapped, so the depth test picked a
   different one every frame ("two textures swapping"). Before: HQ 1,739 pairs, Mall 19,787, School 2,648, Factory 1,320,
   Prison 6,525, Tutorial 4,887. The mall's ceiling coffers (each one's trim overlapping its neighbours') and fluorescent
   fixtures were the worst; kerbs on walks, plants on roofs, boxes merged into one mesh, wall strips, the training
   street's houses.
2. **Scans the wrong size.** 0.24.12 assumed every mesh gives one UV unit per metre. Most don't: the HQ's district earth
   gave 0.002 (one texture repeat per 1.5 km, a flat colour), its steel 0.03 (a 50 m blur), the mall's own surfaces 0.25.
3. **Most surfaces were never upgraded.** Anything with its own normal map counted as "authored", so every map drawn by
   our builders in code (the mall's MALL_* set, the factory and school sets, the prison's borrowed ones: speckled noise,
   blurry fills) was kept, and flat-coloured paint, lockers, bins, cabinets and plastics had no texture at all.
4. **Wrong matches by name.** "IronFactory_Ballast" got steel ("iron"), house shingles got gravel.
5. **Lighting.** Shadow depth bias 0.1 (URP's is 1) drew acne that crawls with the flashlight; ambient occlusion at 8
   samples and short reach; no reflections indoors (the sky's, or none).

## What changed

| What | Where | Notes |
|------|-------|-------|
| Coplanar faces | `Editor/Art/CoplanarFix.cs` | Runs on every map in Rebuild All Maps (AvOverhaul) and alone: `CoplanarFix.FixMapsAndExit`. Faces inside one mesh first: the overlap graph is coloured, the big face stays, the others get their own corners lifted by odd multiples of 1.25 mm. A model's mesh can't change and the Blender kit is in centimetres scaled ×100, so each (model mesh, scale) gets a fixed copy in `Art/Generated/CoplanarMeshes` (registry `Sources.json`; every run points renderers back at the model first). Then objects (a renderer, or its whole LOD group): every overlapping coplanar neighbour within 10 mm is considered with its current gap; largest first, each takes the smallest step of 2.5 mm (toward the side its faces look at) that leaves every neighbour at least 1.1 mm away. Invisible at play distance, and far above a 32-bit depth buffer's resolution. |
| Surface shader | `Art/Shaders/AbandonedSurface.shader`, `AbandonedSurfaceInput.hlsl`, `AbandonedSurfaceForwardPass.hlsl` | URP 17.3 Lit with the same properties (a material keeps every value when its shader is swapped) plus: anti-tiling (two offset reads cross-faded by a slow noise, random surfaces only), triplanar mapping in the object's own metres (materials whose meshes have no consistent UVs), and weathering in world space: large light/dark variation, grime in the scan's cavities and patches and streaks down walls, dust on everything that faces up. Forward+, shadows, SSAO, decals, cookies, LOD cross-fade and motion vectors as URP's own. No GBuffer/2D/XR passes. |
| Weathering per map | `Scripts/Core/Rendering/SurfaceWeathering.cs`, `Editor/Art/SurfaceAging.cs` | The `_AV_Surfaces` object: strength and grime/dust colours (HQ 0.55 swept, mall 1.0 years shut, factory 1.1 soot, prison 1.0 salt, school 0.9, training house 0.45). F1 + B switches it off to compare. |
| Surface upgrade | `Editor/Art/MaterialUpgrade.cs`, `MaterialUpgradeRules.cs`, `ScannedSet.cs` | Every environment material moves to Abandoned/Surface. Every one whose maps are flat or drawn by our builders, and whose name or old map says what it is, gets a scan; tiling from the UV density measured on every map; triplanar where the meshes disagree by more than 2.5×. The colour keeps the old material's hue and brightness including its drawn map's average (a green tile map stays green). Map prefixes are ignored when matching. Loot, threats, players, gear and cosmetics are never touched (they move). |
| New scans | `Tools/Art/fetch_textures.py`, `Art/Scanned/` | 24 more CC0 ambientCG sets (45 in all): painted metal with chips and rust runs, scuffed light metal, plastics, rubber, cast and dark concrete, chipped plaster, weathered render, painted block, acoustic ceiling tiles, kitchen/industrial/glazed tiles, chipboard, painted siding, marble, paving, tread plate, upholstery, snow (fresh and trodden), dark sea rock. Docs/ASSET_CREDITS.md. Anisotropy 8. |
| Room reflections | `Editor/Art/RoomReflectionProbes.cs`, `Scripts/Core/Rendering/RoomReflections.cs`, `LightingEvents.cs` | Realtime box-projected probes per room from each map's NavMesh (cells per storey, indoors only, box pushed to the walls and ceiling), 64 px HDR, up to 160 per map. Drawn a few at a time after load and again 1.5 s after any lighting change (PowerController raises `LightingEvents`). Realtime because the lights change during a job (M7.2). F1 shows how many are drawn. |
| Shadows and AO | `Editor/Art/RenderPipelineSetup.cs` | Shadow bias 0.5 / 0.5 (no more acne crawling with the torch). SSAO 12 samples, full resolution, bilateral blur, radius 0.6, intensity 0.95, direct 0.35. |
| Audit | `Editor/Probes/GraphicsAudit.cs` | Coplanar pairs, stray LOD renderers, and per material: area, shader, maps, UV density. |
| Renders | `Editor/Probes/AvRenders.cs` | `RENDER_OUT=<folder>` keeps sets apart; probes baked for the renders only; two close views of the nearest wall. |

## Status (2026-10-21)

Applied to the saved maps (`AvOverhaul.AfterMapsAndExit`; the maps were not rebuilt). Faces close enough to flicker
(within 0.1 mm), before -> after:

| Map | Overlapping pairs before | Within 0.1 mm after | Within 1 mm after |
|-----|-------------------------|---------------------|-------------------|
| Mall | 19,787 | 95 | ~800 |
| Prison | 6,525 | 5 | 23 |
| Tutorial | 4,887 | 9 | 82 |
| School | 2,648 | 26 | 59 |
| HQ | 1,739 | 0 | 1 |
| Factory | 1,320 | 3 | 11 |

What is left is small (mostly under 0.1 m² each): pieces of one LOD level against a neighbour's other level, roller
shutter slats, a few kit details already 0.05-0.5 mm apart. A 32-bit depth buffer (URP's on Mac and Windows) resolves
0.003 mm at 50 m, so "within 1 mm" is a safety margin, not flicker.

- Surfaces: 464 materials on Abandoned/Surface, 275 with scans (116 triplanar). Materials left on URP Lit are glass,
  foliage, water/sea, lava, emissive, cut-out grating, cloth, vehicles, characters and UI, on purpose.
- Room reflections: mall 48, school 18, factory 90, prison 26 (HQ has no NavMesh; the training house keeps its own 4).
- Wear decals as 0.24.12. Weathering per map as above.
- Tests: EditMode, with `GraphicsOverhaulTests` (detection, the fix, the shader compiles, scan rules). The two tutorial
  art tests now accept Abandoned/Surface and a scan without an AO map.
- Review renders: Game/Logs/renders/gfx4 (the 0.24.12 ones for comparison are in the av-overhaul worktree's
  Logs/renders/av). Batch renders are darker than play: eye adaptation can't settle in three frames.
- Never played. Frame time not measured: the surface shader costs a few noise evaluations and, for anti-tiled
  materials, twice the texture reads; room reflections cost a burst of probe renders after load and after the power
  changes.

## What to check first

1. Walk the mall's food court and concourse looking at the ceilings: the coffer grid and the fluorescent fixtures
   should be steady (no crawling stripes) from any angle and distance.
2. Brightness in the mall and the iron works with the power on and off (ambient was lifted ~30 %).
3. F1: "surfaces: weathering" (B switches it off to compare) and "room reflections n/n".
4. Close to walls and floors in each map: the scan's size (a brick is a brick, terrazzo chips are centimetres).
5. Frame time on the Windows PC at Medium.
