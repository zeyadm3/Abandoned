# Lighting and rendering rebuild — 2026-10-21

Applied to the saved HQ, Mall, School, Factory, Prison and Tutorial scenes on `codex/lighting-rebuild`.
The existing Unity 6.3 / URP 17.3 stack and scanned surface library remain in use. No new packages.

## Visual direction

Readable pools of practical light, cool daylight against warmer fixtures, grounded contact shadows, and clear
material response. Darkness comes from the lighting arrangement instead of heavy grain, lens dirt, chromatic
aberration or dense haze. Power failures, faulty fixtures and collapsing supports still control their lights.

## Changes

- **Fixture calibration:** `Editor/Art/LightingRebuild.cs` traces each downward fixture to a receiving surface and
  compensates for distance and URP range attenuation. A tall pendant no longer uses the same dim intensity as a
  domestic fitting. 505 fixtures calibrated: HQ 11, Mall 279, School 40, Factory 90, Prison 67, Tutorial 18. The pass
  uses absolute targets, so repeated application does not increase brightness. Existing fixture colours and failure
  assignments remain intact. Broad inner cones soften the edge of the light pool.
- **Diffuse bounce:** `Core/Rendering/IndirectLighting.cs` allocates at most 0 / 2 / 4 / 6 local bounce lights by
  shadow quality. Each samples a real floor receiver and bounds its range with wall-clearance rays. Power, source
  intensity, source colour and missing receivers affect it. One receiver is retraced per frame, periodically rather
  than every frame for every fixture. This is a local approximation, not path-traced or baked global illumination.
  Tunables live in `Data/Core/Resources/IndirectLightingConfig.asset`. F1 displays active lights and Scene gizmos
  show their receivers and influence.
- **Shadow selection:** `Structure/ShadowDirector.cs` ranks fixtures by influence on the player and surfaces ahead,
  checks whether the source is behind a collider, and retains the existing hysteresis, fading and atlas accounting.
  Low / Medium / High allow 2 / 6 / 10 managed lights; at most one point light consumes six faces. The flashlight's
  atlas reservation remains. Fixtures use explicit 0.35 / 0.25 bias, 0.08 m near clip and high soft-shadow filtering.
  Thin architectural panels that were explicitly disabled by the older blanket flat-surface pass can regain
  two-sided casting; no such panels required restoration in the six saved maps.
- **Surface stability:** the surface forward pass filters specular smoothness using derivatives of the shaded
  normal to reduce distant glints from detailed scan normals. Contact AO has a smaller 0.35 m radius, stronger
  indirect grounding and less effect on direct light. The existing anti-tiling and scanned maps remain.
- **Air and grading:** `AvLook.cs` sets restrained per-map fog, a 0.35 scattering anisotropy and no scattering from
  unshadowed local lights. Ordinary interior haze is reduced as well. ACES, mild contrast, nearly neutral saturation,
  0.12 bloom with an 8-unit input clamp, 0.12 lens dirt, 0.035 grain, 0.12 vignette and zero chromatic aberration preserve detail. Different maps
  retain their warm/cold balance. Ambient values are authored per map rather than multiplied repeatedly.
- **Exposure:** narrow adaptation (-0.8 to +1.25 stops), fast adjustment to bright views and slower adjustment to
  dark ones. GPU histories now belong to each camera and reset after large camera cuts; another camera cannot
  reuse the player's exposure. Histories and RTHandles are released when unused or disposed.
- **RenderGraph blending:** fog compositing and exposure multiplication declare `ReadWrite` access to the colour
  attachment they blend with, preserving its existing contents on tile GPUs.
- **Reflection startup:** URP 17.3's clustered-light setup subtracts every null-texture probe from an already capped
  count. Dense maps with many cold probes could allocate too few cluster entries and fail to draw. Saved probes
  now carry a valid dark fallback cube. `RoomReflections` replaces it through a bounded realtime refresh queue,
  closest rooms first. Unity requires a probe to be enabled when requesting a render; only the small in-flight set
  changes to realtime at once. The old mall-only refresher is disabled to avoid two owners requesting the same cubes.
- **Beam fallback:** painted light shafts rescan on scene changes, tolerate a not-yet-created volume stack, and apply
  their initial visibility even when it matches a previous map. The capture path applies the same switch as the game.

## Apply and review

The saved scenes already include the pass. Rebuild All Maps runs `AvOverhaul.AfterMaps`, which applies it last.
After an individual map's standalone rebuild, run the lighting menu below to restore the complete pass.
To apply it independently: **Tools > Abandoned > Art > Rebuild Lighting and Shadows**. This preserves the editor's
scene setup and updates the saved maps without regenerating geometry, navigation, loot or gameplay.

Batch application:

```sh
Tools/unity.sh exec Abandoned.EditorTools.LightingRebuild.ApplyAndExit
```

Repeatable player-height review captures:

```sh
LIGHTING_REVIEW=lighting-after Tools/unity.sh exec Abandoned.EditorTools.LightingReview.CaptureAndExit
```

Optional `LIGHTING_MAP=Mall` limits the capture to one scene. Captures live in `Docs/renders/lighting-after`.
The renderer uses the same fixture selection and local bounce logic; editor gizmo/icon preferences are temporarily
disabled and restored. Captures are diagnostic saved-scene views, not a network gameplay or performance benchmark.

## Validation

- Unity compilation and batch application completed.
- Edit Mode: **329 / 329 passed**, including fixture calibration repeatability, compensation for fixture height,
  spotlight range/cone exclusion, preference for visible shadows, thin architecture and valid cold-probe fallbacks.
- Play Mode: the two new realtime-lighting tests pass (bounce follows power and losing its floor receiver; fallback
  probes carry valid textures and Unity accepts their queued refresh requests). Batch mode does not service Unity's
  native reflection scheduler, so actual refresh completion is checked only when that test runs interactively.
- Existing mall fixtures, power/collapse and render-budget tests pass. The budget test measured 4,543 / 10,226 /
  8,247 / 4,481 batches at its four views, below its existing 12,500 regression limit. This is not a gameplay FPS
  measurement; the batch session omits the player-following shadow director.
- All six saved scenes rendered successfully with the production fog, bloom and exposure active, including Factory
  (which failed clustered rendering with the original cold-probe setup). Final display-space images use an sRGB PNG
  target; earlier linear-HDR PNG diagnostics were discarded. See `renders/lighting-after/README.md`.

## Practical limits

Realtime shadows remain budgeted, so distant unselected lights can still leak through geometry. Bounce uses a
conservative local influence volume, not full scene transport. Reflections initially use a dark fallback and populate
over several frames. Windows GPU appearance and frame time still need a hardware playtest; no performance target
is claimed from batch screenshots.
