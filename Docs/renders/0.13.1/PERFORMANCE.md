# Meridian 0.13.1 — asset and Editor render report

Captured from the saved Mall in Edit mode at 1920 × 1080. No gameplay, network, EditMode/PlayMode or screenshot tests were run.
FPS and the 60 FPS four-player mid-range-PC-host target remain pending the user's in-game pass. These art captures do not measure host simulation or multiplayer cost.

Graphics device: Apple M4 Max; API: Metal; Unity: 6000.3.25f1.
Referenced mall and scene-sky textures: 80; format/block/mipmap GPU storage: 127.98 MiB; Profiler runtime/native allocation: 256.04 MiB.
GPU storage is calculated from each actual imported graphics format, dimensions and mip count. Profiler allocation includes Editor/native overhead; realtime reflection targets and transient render buffers are reported separately from imported material maps.
Unique instantiated meshes, including inactive LODs: 194; stored vertices: 156,793; stored triangles: 131,832.
Renderers: 7982; material slots: 20106; unique materials including sky: 44; instancing-enabled materials: 34.
LOD groups: 2140; realtime fixtures: 121; mall plus global directional lights: 136; global directionals: 1; shadow-capable lights: 5.
Reflection probes: 3; allocated realtime probe textures: 0.00 MiB; sections: 432; collapsible: 171; permanent: 261.
Static renderer flags — batching: 1474; occluder: 821; occludee: 1474.

| Art view | Flashlights | Editor batches | SetPass | Triangles | Vertices |
|---|---:|---:|---:|---:|---:|
| atrium-power-on | 1 | 0 | 0 | 0 | 0 |
| atrium-power-off | 1 | 0 | 0 | 0 | 0 |
| food-court-power-on | 1 | 0 | 0 | 0 | 0 |
| food-court-power-off | 1 | 0 | 0 | 0 | 0 |
| cinema-power-on | 1 | 0 | 0 | 0 | 0 |
| cinema-power-off | 1 | 0 | 0 | 0 | 0 |
| dead-circuit-power-on | 1 | 0 | 0 | 0 | 0 |
| dead-circuit-power-off | 1 | 0 | 0 | 0 | 0 |
| service-corridor-power-on | 1 | 0 | 0 | 0 | 0 |
| service-corridor-power-off | 1 | 0 | 0 | 0 | 0 |
| basement-power-on | 1 | 0 | 0 | 0 | 0 |
| basement-power-off | 1 | 0 | 0 | 0 | 0 |
| parking-night-power-on | 1 | 0 | 0 | 0 | 0 |
| parking-night-power-off | 1 | 0 | 0 | 0 | 0 |
| parking-arrival-power-on | 1 | 0 | 0 | 0 | 0 |
| parking-arrival-power-off | 1 | 0 | 0 | 0 | 0 |
| collection-bay-power-on | 1 | 0 | 0 | 0 | 0 |
| collection-bay-power-off | 1 | 0 | 0 | 0 | 0 |
| gallery-wound-power-on | 1 | 0 | 0 | 0 | 0 |
| gallery-wound-power-off | 1 | 0 | 0 | 0 | 0 |
| atrium-four-flashlights | 4 | 0 | 0 | 0 | 0 |
| terrazzo-surface-stable | 1 | 0 | 0 | 0 | 0 |
| terrazzo-surface-cracking | 1 | 0 | 0 | 0 | 0 |
| terrazzo-surface-failing | 1 | 0 | 0 | 0 | 0 |

UnityStats are raw Editor counters sampled after synchronous art rendering and readback; they may include Editor/probe work and are not an isolated gameplay profiler measurement. All-zero counters mean the batch renderer did not expose usable statistics.
Every normal view uses one owner flashlight. The extra atrium view uses four static beams, one shadowed and three unshadowed, matching the current runtime flashlight policy. Power-on respects dead fixtures and depicts faulty fixtures at a steady 75%; power-off removes fixture lamps and all their diffuser emission. The scene file is byte-checked unchanged after capture.
Gallery stills show the authored collapse wound. The three close terrazzo stills swap the existing stable/cracking/failing art variants without advancing structural simulation or damage.

| Texture | Imported size / mips | Actual format | GPU MiB | Profiler MiB | Import type / sRGB / mipmaps | Default max | Standalone override / max / format |
|---|---|---|---:|---:|---|---:|---|
| Assets/_Project/Art/Custom/Environment/Meridian/Textures/MALL_Asphalt_BaseColor.png | 1024 × 1024 / 11 | RGBA_BC7_SRGB | 1.33 | 2.67 | Default / True / True | 1024 | True / 1024 / BC7 |
| Assets/_Project/Art/Custom/Environment/Meridian/Textures/MALL_Asphalt_Mask.png | 1024 × 1024 / 11 | RGBA_BC7_UNorm | 1.33 | 2.67 | Default / False / True | 1024 | True / 1024 / BC7 |
| Assets/_Project/Art/Custom/Environment/Meridian/Textures/MALL_Asphalt_Normal.png | 1024 × 1024 / 11 | RG_BC5_UNorm | 1.33 | 2.67 | NormalMap / False / True | 1024 | True / 1024 / BC5 |
| Assets/_Project/Art/Custom/Environment/Meridian/Textures/MALL_Block_BaseColor.png | 1024 × 1024 / 11 | RGBA_BC7_SRGB | 1.33 | 2.67 | Default / True / True | 1024 | True / 1024 / BC7 |
| Assets/_Project/Art/Custom/Environment/Meridian/Textures/MALL_Block_Mask.png | 1024 × 1024 / 11 | RGBA_BC7_UNorm | 1.33 | 2.67 | Default / False / True | 1024 | True / 1024 / BC7 |
| Assets/_Project/Art/Custom/Environment/Meridian/Textures/MALL_Block_Normal.png | 1024 × 1024 / 11 | RG_BC5_UNorm | 1.33 | 2.67 | NormalMap / False / True | 1024 | True / 1024 / BC5 |
| Assets/_Project/Art/Custom/Environment/Meridian/Textures/MALL_BoardWood_BaseColor.png | 512 × 512 / 10 | RGBA_BC7_SRGB | 0.33 | 0.67 | Default / True / True | 512 | True / 512 / BC7 |
| Assets/_Project/Art/Custom/Environment/Meridian/Textures/MALL_BoardWood_Mask.png | 512 × 512 / 10 | RGBA_BC7_UNorm | 0.33 | 0.67 | Default / False / True | 512 | True / 512 / BC7 |
| Assets/_Project/Art/Custom/Environment/Meridian/Textures/MALL_BoardWood_Normal.png | 512 × 512 / 10 | RG_BC5_UNorm | 0.33 | 0.67 | NormalMap / False / True | 512 | True / 512 / BC5 |
| Assets/_Project/Art/Custom/Environment/Meridian/Textures/MALL_BridgeMetal_BaseColor.png | 1024 × 1024 / 11 | RGBA_BC7_SRGB | 1.33 | 2.67 | Default / True / True | 1024 | True / 1024 / BC7 |
| Assets/_Project/Art/Custom/Environment/Meridian/Textures/MALL_BridgeMetal_Mask.png | 1024 × 1024 / 11 | RGBA_BC7_UNorm | 1.33 | 2.67 | Default / False / True | 1024 | True / 1024 / BC7 |
| Assets/_Project/Art/Custom/Environment/Meridian/Textures/MALL_BridgeMetal_Normal.png | 1024 × 1024 / 11 | RG_BC5_UNorm | 1.33 | 2.67 | NormalMap / False / True | 1024 | True / 1024 / BC5 |
| Assets/_Project/Art/Custom/Environment/Meridian/Textures/MALL_Carpet_BaseColor.png | 1024 × 1024 / 11 | RGBA_BC7_SRGB | 1.33 | 2.67 | Default / True / True | 1024 | True / 1024 / BC7 |
| Assets/_Project/Art/Custom/Environment/Meridian/Textures/MALL_Carpet_Mask.png | 1024 × 1024 / 11 | RGBA_BC7_UNorm | 1.33 | 2.67 | Default / False / True | 1024 | True / 1024 / BC7 |
| Assets/_Project/Art/Custom/Environment/Meridian/Textures/MALL_Carpet_Normal.png | 1024 × 1024 / 11 | RG_BC5_UNorm | 1.33 | 2.67 | NormalMap / False / True | 1024 | True / 1024 / BC5 |
| Assets/_Project/Art/Custom/Environment/Meridian/Textures/MALL_Ceiling_BaseColor.png | 2048 × 2048 / 12 | RGBA_BC7_SRGB | 5.33 | 10.67 | Default / True / True | 2048 | True / 2048 / BC7 |
| Assets/_Project/Art/Custom/Environment/Meridian/Textures/MALL_Ceiling_Mask.png | 2048 × 2048 / 12 | RGBA_BC7_UNorm | 5.33 | 10.67 | Default / False / True | 2048 | True / 2048 / BC7 |
| Assets/_Project/Art/Custom/Environment/Meridian/Textures/MALL_Ceiling_Normal.png | 2048 × 2048 / 12 | RG_BC5_UNorm | 5.33 | 10.67 | NormalMap / False / True | 2048 | True / 2048 / BC5 |
| Assets/_Project/Art/Custom/Environment/Meridian/Textures/MALL_Concrete_BaseColor.png | 1024 × 1024 / 11 | RGBA_BC7_SRGB | 1.33 | 2.67 | Default / True / True | 1024 | True / 1024 / BC7 |
| Assets/_Project/Art/Custom/Environment/Meridian/Textures/MALL_Concrete_Mask.png | 1024 × 1024 / 11 | RGBA_BC7_UNorm | 1.33 | 2.67 | Default / False / True | 1024 | True / 1024 / BC7 |
| Assets/_Project/Art/Custom/Environment/Meridian/Textures/MALL_Concrete_Normal.png | 1024 × 1024 / 11 | RG_BC5_UNorm | 1.33 | 2.67 | NormalMap / False / True | 1024 | True / 1024 / BC5 |
| Assets/_Project/Art/Custom/Environment/Meridian/Textures/MALL_Crack_BaseColor.png | 512 × 512 / 10 | RGBA_BC7_SRGB | 0.33 | 0.67 | Default / True / True | 512 | True / 512 / BC7 |
| Assets/_Project/Art/Custom/Environment/Meridian/Textures/MALL_Crack_Mask.png | 512 × 512 / 10 | RGBA_BC7_UNorm | 0.33 | 0.67 | Default / False / True | 512 | True / 512 / BC7 |
| Assets/_Project/Art/Custom/Environment/Meridian/Textures/MALL_Crack_Normal.png | 512 × 512 / 10 | RG_BC5_UNorm | 0.33 | 0.67 | NormalMap / False / True | 512 | True / 512 / BC5 |
| Assets/_Project/Art/Custom/Environment/Meridian/Textures/MALL_EscalatorSteel_BaseColor.png | 2048 × 2048 / 12 | RGBA_BC7_SRGB | 5.33 | 10.67 | Default / True / True | 2048 | True / 2048 / BC7 |
| Assets/_Project/Art/Custom/Environment/Meridian/Textures/MALL_EscalatorSteel_Mask.png | 2048 × 2048 / 12 | RGBA_BC7_UNorm | 5.33 | 10.67 | Default / False / True | 2048 | True / 2048 / BC7 |
| Assets/_Project/Art/Custom/Environment/Meridian/Textures/MALL_EscalatorSteel_Normal.png | 2048 × 2048 / 12 | RG_BC5_UNorm | 5.33 | 10.67 | NormalMap / False / True | 2048 | True / 2048 / BC5 |
| Assets/_Project/Art/Custom/Environment/Meridian/Textures/MALL_FacadeStone_BaseColor.png | 1024 × 1024 / 11 | RGBA_BC7_SRGB | 1.33 | 2.67 | Default / True / True | 1024 | True / 1024 / BC7 |
| Assets/_Project/Art/Custom/Environment/Meridian/Textures/MALL_FacadeStone_Mask.png | 1024 × 1024 / 11 | RGBA_BC7_UNorm | 1.33 | 2.67 | Default / False / True | 1024 | True / 1024 / BC7 |
| Assets/_Project/Art/Custom/Environment/Meridian/Textures/MALL_FacadeStone_Normal.png | 1024 × 1024 / 11 | RG_BC5_UNorm | 1.33 | 2.67 | NormalMap / False / True | 1024 | True / 1024 / BC5 |
| Assets/_Project/Art/Custom/Environment/Meridian/Textures/MALL_FixtureGlow_BaseColor.png | 512 × 512 / 10 | RGBA_BC7_SRGB | 0.33 | 0.67 | Default / True / True | 512 | True / 512 / BC7 |
| Assets/_Project/Art/Custom/Environment/Meridian/Textures/MALL_FixtureGlow_Mask.png | 512 × 512 / 10 | RGBA_BC7_UNorm | 0.33 | 0.67 | Default / False / True | 512 | True / 512 / BC7 |
| Assets/_Project/Art/Custom/Environment/Meridian/Textures/MALL_FixtureGlow_Normal.png | 512 × 512 / 10 | RG_BC5_UNorm | 0.33 | 0.67 | NormalMap / False / True | 512 | True / 512 / BC5 |
| Assets/_Project/Art/Custom/Environment/Meridian/Textures/MALL_FixtureTrim_BaseColor.png | 512 × 512 / 10 | RGBA_BC7_SRGB | 0.33 | 0.67 | Default / True / True | 512 | True / 512 / BC7 |
| Assets/_Project/Art/Custom/Environment/Meridian/Textures/MALL_FixtureTrim_Mask.png | 512 × 512 / 10 | RGBA_BC7_UNorm | 0.33 | 0.67 | Default / False / True | 512 | True / 512 / BC7 |
| Assets/_Project/Art/Custom/Environment/Meridian/Textures/MALL_FixtureTrim_Normal.png | 512 × 512 / 10 | RG_BC5_UNorm | 0.33 | 0.67 | NormalMap / False / True | 512 | True / 512 / BC5 |
| Assets/_Project/Art/Custom/Environment/Meridian/Textures/MALL_Glass_BaseColor.png | 512 × 512 / 10 | RGBA_BC7_SRGB | 0.33 | 0.67 | Default / True / True | 512 | True / 512 / BC7 |
| Assets/_Project/Art/Custom/Environment/Meridian/Textures/MALL_Glass_Mask.png | 512 × 512 / 10 | RGBA_BC7_UNorm | 0.33 | 0.67 | Default / False / True | 512 | True / 512 / BC7 |
| Assets/_Project/Art/Custom/Environment/Meridian/Textures/MALL_Glass_Normal.png | 512 × 512 / 10 | RG_BC5_UNorm | 0.33 | 0.67 | NormalMap / False / True | 512 | True / 512 / BC5 |
| Assets/_Project/Art/Custom/Environment/Meridian/Textures/MALL_Graffiti_BaseColor.png | 512 × 512 / 10 | RGBA_BC7_SRGB | 0.33 | 0.67 | Default / True / True | 512 | True / 512 / BC7 |
| Assets/_Project/Art/Custom/Environment/Meridian/Textures/MALL_Graffiti_Mask.png | 512 × 512 / 10 | RGBA_BC7_UNorm | 0.33 | 0.67 | Default / False / True | 512 | True / 512 / BC7 |
| Assets/_Project/Art/Custom/Environment/Meridian/Textures/MALL_Graffiti_Normal.png | 512 × 512 / 10 | RG_BC5_UNorm | 0.33 | 0.67 | NormalMap / False / True | 512 | True / 512 / BC5 |
| Assets/_Project/Art/Custom/Environment/Meridian/Textures/MALL_Leaves_BaseColor.png | 512 × 512 / 10 | RGBA_BC7_SRGB | 0.33 | 0.67 | Default / True / True | 512 | True / 512 / BC7 |
| Assets/_Project/Art/Custom/Environment/Meridian/Textures/MALL_Leaves_Mask.png | 512 × 512 / 10 | RGBA_BC7_UNorm | 0.33 | 0.67 | Default / False / True | 512 | True / 512 / BC7 |
| Assets/_Project/Art/Custom/Environment/Meridian/Textures/MALL_Leaves_Normal.png | 512 × 512 / 10 | RG_BC5_UNorm | 0.33 | 0.67 | NormalMap / False / True | 512 | True / 512 / BC5 |
| Assets/_Project/Art/Custom/Environment/Meridian/Textures/MALL_Moss_BaseColor.png | 512 × 512 / 10 | RGBA_BC7_SRGB | 0.33 | 0.67 | Default / True / True | 512 | True / 512 / BC7 |
| Assets/_Project/Art/Custom/Environment/Meridian/Textures/MALL_Moss_Mask.png | 512 × 512 / 10 | RGBA_BC7_UNorm | 0.33 | 0.67 | Default / False / True | 512 | True / 512 / BC7 |
| Assets/_Project/Art/Custom/Environment/Meridian/Textures/MALL_Moss_Normal.png | 512 × 512 / 10 | RG_BC5_UNorm | 0.33 | 0.67 | NormalMap / False / True | 512 | True / 512 / BC5 |
| Assets/_Project/Art/Custom/Environment/Meridian/Textures/MALL_Plaster_BaseColor.png | 2048 × 2048 / 12 | RGBA_BC7_SRGB | 5.33 | 10.67 | Default / True / True | 2048 | True / 2048 / BC7 |
| Assets/_Project/Art/Custom/Environment/Meridian/Textures/MALL_Plaster_Mask.png | 2048 × 2048 / 12 | RGBA_BC7_UNorm | 5.33 | 10.67 | Default / False / True | 2048 | True / 2048 / BC7 |
| Assets/_Project/Art/Custom/Environment/Meridian/Textures/MALL_Plaster_Normal.png | 2048 × 2048 / 12 | RG_BC5_UNorm | 5.33 | 10.67 | NormalMap / False / True | 2048 | True / 2048 / BC5 |
| Assets/_Project/Art/Custom/Environment/Meridian/Textures/MALL_Rust_BaseColor.png | 1024 × 1024 / 11 | RGBA_BC7_SRGB | 1.33 | 2.67 | Default / True / True | 1024 | True / 1024 / BC7 |
| Assets/_Project/Art/Custom/Environment/Meridian/Textures/MALL_Rust_Mask.png | 1024 × 1024 / 11 | RGBA_BC7_UNorm | 1.33 | 2.67 | Default / False / True | 1024 | True / 1024 / BC7 |
| Assets/_Project/Art/Custom/Environment/Meridian/Textures/MALL_Rust_Normal.png | 1024 × 1024 / 11 | RG_BC5_UNorm | 1.33 | 2.67 | NormalMap / False / True | 1024 | True / 1024 / BC5 |
| Assets/_Project/Art/Custom/Environment/Meridian/Textures/MALL_Scorch_BaseColor.png | 512 × 512 / 10 | RGBA_BC7_SRGB | 0.33 | 0.67 | Default / True / True | 512 | True / 512 / BC7 |
| Assets/_Project/Art/Custom/Environment/Meridian/Textures/MALL_Scorch_Mask.png | 512 × 512 / 10 | RGBA_BC7_UNorm | 0.33 | 0.67 | Default / False / True | 512 | True / 512 / BC7 |
| Assets/_Project/Art/Custom/Environment/Meridian/Textures/MALL_Scorch_Normal.png | 512 × 512 / 10 | RG_BC5_UNorm | 0.33 | 0.67 | NormalMap / False / True | 512 | True / 512 / BC5 |
| Assets/_Project/Art/Custom/Environment/Meridian/Textures/MALL_StorefrontTrim_BaseColor.png | 2048 × 2048 / 12 | RGBA_BC7_SRGB | 5.33 | 10.67 | Default / True / True | 2048 | True / 2048 / BC7 |
| Assets/_Project/Art/Custom/Environment/Meridian/Textures/MALL_StorefrontTrim_Mask.png | 2048 × 2048 / 12 | RGBA_BC7_UNorm | 5.33 | 10.67 | Default / False / True | 2048 | True / 2048 / BC7 |
| Assets/_Project/Art/Custom/Environment/Meridian/Textures/MALL_StorefrontTrim_Normal.png | 2048 × 2048 / 12 | RG_BC5_UNorm | 5.33 | 10.67 | NormalMap / False / True | 2048 | True / 2048 / BC5 |
| Assets/_Project/Art/Custom/Environment/Meridian/Textures/MALL_Terrazzo_BaseColor.png | 2048 × 2048 / 12 | RGBA_BC7_SRGB | 5.33 | 10.67 | Default / True / True | 2048 | True / 2048 / BC7 |
| Assets/_Project/Art/Custom/Environment/Meridian/Textures/MALL_Terrazzo_Mask.png | 2048 × 2048 / 12 | RGBA_BC7_UNorm | 5.33 | 10.67 | Default / False / True | 2048 | True / 2048 / BC7 |
| Assets/_Project/Art/Custom/Environment/Meridian/Textures/MALL_Terrazzo_Normal.png | 2048 × 2048 / 12 | RG_BC5_UNorm | 5.33 | 10.67 | NormalMap / False / True | 2048 | True / 2048 / BC5 |
| Assets/_Project/Art/Custom/Environment/Meridian/Textures/MALL_Tile_BaseColor.png | 1024 × 1024 / 11 | RGBA_BC7_SRGB | 1.33 | 2.67 | Default / True / True | 1024 | True / 1024 / BC7 |
| Assets/_Project/Art/Custom/Environment/Meridian/Textures/MALL_Tile_Mask.png | 1024 × 1024 / 11 | RGBA_BC7_UNorm | 1.33 | 2.67 | Default / False / True | 1024 | True / 1024 / BC7 |
| Assets/_Project/Art/Custom/Environment/Meridian/Textures/MALL_Tile_Normal.png | 1024 × 1024 / 11 | RG_BC5_UNorm | 1.33 | 2.67 | NormalMap / False / True | 1024 | True / 1024 / BC5 |
| Assets/_Project/Art/Custom/Environment/Meridian/Textures/MALL_Upholstery_BaseColor.png | 512 × 512 / 10 | RGBA_BC7_SRGB | 0.33 | 0.67 | Default / True / True | 512 | True / 512 / BC7 |
| Assets/_Project/Art/Custom/Environment/Meridian/Textures/MALL_Upholstery_Mask.png | 512 × 512 / 10 | RGBA_BC7_UNorm | 0.33 | 0.67 | Default / False / True | 512 | True / 512 / BC7 |
| Assets/_Project/Art/Custom/Environment/Meridian/Textures/MALL_Upholstery_Normal.png | 512 × 512 / 10 | RG_BC5_UNorm | 0.33 | 0.67 | NormalMap / False / True | 512 | True / 512 / BC5 |
| Assets/_Project/Art/Custom/Environment/Meridian/Textures/MALL_Water_BaseColor.png | 512 × 512 / 10 | RGBA_BC7_SRGB | 0.33 | 0.67 | Default / True / True | 512 | True / 512 / BC7 |
| Assets/_Project/Art/Custom/Environment/Meridian/Textures/MALL_Water_Mask.png | 512 × 512 / 10 | RGBA_BC7_UNorm | 0.33 | 0.67 | Default / False / True | 512 | True / 512 / BC7 |
| Assets/_Project/Art/Custom/Environment/Meridian/Textures/MALL_Water_Normal.png | 512 × 512 / 10 | RG_BC5_UNorm | 0.33 | 0.67 | NormalMap / False / True | 512 | True / 512 / BC5 |
| Assets/_Project/Art/Custom/Environment/Meridian/Textures/MALL_Wood_BaseColor.png | 1024 × 1024 / 11 | RGBA_BC7_SRGB | 1.33 | 2.67 | Default / True / True | 1024 | True / 1024 / BC7 |
| Assets/_Project/Art/Custom/Environment/Meridian/Textures/MALL_Wood_Mask.png | 1024 × 1024 / 11 | RGBA_BC7_UNorm | 1.33 | 2.67 | Default / False / True | 1024 | True / 1024 / BC7 |
| Assets/_Project/Art/Custom/Environment/Meridian/Textures/MALL_Wood_Normal.png | 1024 × 1024 / 11 | RG_BC5_UNorm | 1.33 | 2.67 | NormalMap / False / True | 1024 | True / 1024 / BC5 |
| Assets/_Project/Art/Generated/MeridianAtmosphere/DustDot.png | 512 × 512 / 10 | RGBA_BC7_SRGB | 0.33 | 0.67 | Default / True / True | 512 | True / 512 / BC7 |
| Assets/_Project/Art/Generated/MeridianAtmosphere/OvercastNight.png | 512 × 512 / 10 | RGBA_BC7_SRGB | 0.33 | 0.67 | Default / True / True | 512 | True / 512 / BC7 |
| Assets/_Project/Art/Polish/SurfaceGrain.asset | 128 × 128 / 8 | R8G8B8A8_SRGB | 0.08 | 0.17 | generated/sub-asset | 0 | n/a |
| Assets/_Project/Art/ThirdParty/Fonts/BarlowCondensed/BarlowCondensed-SemiBold.ttf | 256 × 256 / 1 | 54 | 0.06 | 0.13 | generated/sub-asset | 0 | n/a |
| Assets/_Project/Art/ThirdParty/Kenney/CarKit/Textures/colormap.png | 512 × 512 / 10 | RGBA_DXT1_SRGB | 0.17 | 0.33 | Default / True / True | 2048 | False / 2048 / AutomaticCompressed |
