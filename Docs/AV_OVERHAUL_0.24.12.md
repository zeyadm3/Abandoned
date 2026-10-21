# 0.24.12: sound and picture overhaul

The user (2026-10-21): "a total sound and graphical overhaul ... I want the maps to stay, all that, the UI ... the graphics
to take three steps forward and the sound to be proper sound". Plan approved with the user's "build build build, don't
stop until everything is done and built and rebuilt". Maps, models, gameplay and UI are unchanged. Branch `av-overhaul`.

## Picture

| What | Where | Notes |
|------|-------|-------|
| Forward+, HDR grading (32 LUT), 4 sun cascades to 50 m, 4096 sun map, high soft shadows | `Editor/Art/RenderPipelineSetup.cs` | Forward+ lifts the four-lights-per-object cap the fixture-dense rooms hit. The Shadows setting lowers cascades (1/1/2/4) and distance (0/18/30/50 m) at run time (`VideoApplier`). |
| Volumetric light | `Scripts/Core/Rendering/VolumetricFog*.cs`, `Art/Shaders/VolumetricFog.shader` | Marched through the depth buffer (Low 1/4 resolution x 16 steps, Medium 1/3 x 24, High 1/2 x 32; samples bunched near the camera), lit by the sun (hard shadow taps, faded out past the shadow distance so daylight never pours through roofs) and every clustered local light with its shadows and cookie, faded in over the first 1.5 m from each bulb; depth-aware blur and upsample; blended over the opaque scene. Lights without shadows scatter at 25 % (they would leak through walls). Game and scene cameras only, never overlays or the small monitors. Settings: Graphics > Volumetric light Off/Low/Medium/High (Medium default). Per map: a `VolumetricFogOverride` on its volume profile; the type is registered in `Assets/Settings/DefaultVolumeProfile` (Unity 6 builds every volume stack from that list). |
| Painted light shafts | `Scripts/Core/Rendering/LightShaftSwitch.cs` | The old shaft cards hide while volumetric light runs and come back when it is off. The lighthouse beam is never touched. |
| Eye adaptation | `Scripts/Core/Rendering/AutoExposure*.cs`, `Art/Shaders/AutoExposure.shader` | Deliberately soft (strength 0.35, -1.5..+1 EV, centre-weighted): dark halls stay dark; walking in from daylight settles in a second or two. Settings: Graphics > Eye adaptation. |
| Per-map look | `Editor/Art/AvLook.cs` | Fog density/colour, exposure key, white balance, split-toned shadows, lens dirt in the bloom, a trace of chromatic aberration. Mall dusty and green-grey, HQ warm and clear, factory smoky and warm, prison cold sea mist, school clear winter, tutorial December afternoon. Applied by the map builders and to the saved volume assets. |
| Light cookies | `Scripts/Core/Rendering/LightCookies.cs` | The flashlight's reflector pattern (hot centre, seam ring, smudges) and a diffuser for spot fixtures, drawn in code, both kept close to full strength. The flashlight's beam is raised by `FlashlightConfig.cookieGain` (1.2) to make up what its pattern takes, so it is as bright as the 0.13.1 torch. `FlashlightConfig.UseCookie`. |
| Anti-aliasing, upscaler, motion blur | `VideoSettings.Rendering.cs`, `VideoExtras.cs` | Anti-aliasing gains SMAA and TAA (appended; saved choices keep their meaning). Upscaler STP (FSR where compute shaders are missing) / FSR / Bilinear below 100 % render scale. Motion blur off by default. |
| Scanned surfaces | `Tools/Art/fetch_textures.py`, `Editor/Art/MaterialUpgrade.cs`, `Art/Scanned/` | 21 CC0 ambientCG sets (2K). Every flat or procedural environment material without its own normal map is matched by name to a set (first rule wins; glow, glass, foliage, printed, transparent and emissive materials are skipped). Changed in place (same GUIDs). The scan keeps the material's hue (paint fully, brick a little) and its tuned brightness, worked out in linear space, so lighting balance is unchanged. Each material's colour before its first upgrade is kept in `Art/Scanned/MaterialUpgradeOriginals.json`, so running it again never compounds. Painted metal (lockers, rails, poles, consoles) is not metallic; only rust, grating and cladding keep the scan's metalness. Colour and normal maps 2K, AO and metal/smoothness 1K. Roof shingles and crew, character and kit materials are left alone. Builders recreate their materials, so it runs after every map rebuild. |
| Wear decals | `Editor/Art/DecalAssets.cs`, `Editor/Art/DecalScatter.cs` | URP decals (DBuffer, 40 m). Grime at the foot of walls, water stains and soot on ceilings, floor stains, rust runs (iron works), seeded from each scene's NavMesh and name (a fixed hash, so a rerun puts them back), in the scene's `_AV_Decals` group (replaced on every run). Each box reaches only 3-5 cm out of its surface, so boots, loot and props against walls stay clean. |
| Floor cracks | `Scripts/Structure/StructureCrackDecals.cs` | Hairlines at Stressed, the full fracture at Cracking/Failing, on the walking face and the ceiling beneath (20 cm boxes, so one face never shows through to the other); from the replicated stage, so every machine draws the same; gone on collapse and on restore. A creak still only ever means the floor; the crack shows what it already says. |
| Bounce light (tried, dropped) | (removed) | Adaptive probe volumes were baked per map in two scenarios (power on / blackout) and blended by the share of dark fixtures. Side-by-side renders showed the bake replacing each map's hand-tuned flat ambient with the little a dim fixture bounces: interior walls went black. Removed (probe data and tools deleted, scenes back to Unity's default lighting data); the game stays realtime-only (M7.2). |

## Sound

| What | Where | Notes |
|------|-------|-------|
| Mastering | `Scripts/Audio/MasterBus.cs` | Stereo-linked compressor (-16 dB, 3:1, 8 ms / 180 ms, no makeup gain, so it only ever takes loud moments down) and soft limiter (-1 dBFS) last on the listener. Unity can't author mixer assets from code, so the master bus is this filter; the planned `.mixer` asset was replaced by it. Numbers in `AudioConfig` (Mastering). |
| States | `ListenerSpace.cs` | Hiding muffles the world to 1500 Hz (through the locker's steel); low health keeps its own muffle. |
| Sidechain | `AudioPool.cs`, `AudioMix.cs` | Loud structural warnings and monsters near you push the ambience down (to 55 %, fast down, slow back). Music is never ducked by them (the monster songs are warnings). |
| 3D sound | `Scripts/Audio/SpatialAudio.cs`, `Assets/Plugins/SteamAudio/` | Steam Audio 4.8.1 (Apache-2.0): head-related panning for every 3D voice, loop and crew voice. It only places sounds; walls stay with our `AudioOcclusion` (warnings never dulled, monsters never much quieter). Falls back to Unity's panning if the plugin is missing or `AudioConfig.SteamAudio` is off. The spatializer is chosen in Project Settings > Audio (`Editor/Audio/SpatialAudioSetup.cs`, run with the render setup; a player can't pick one at run time), and its settings asset ships in `Assets/Plugins/SteamAudio/Resources`. Only macOS and Windows x64 binaries are in the project; the Mac build re-seals the plugin bundles before signing the app. |
| Recorded library | `Tools/Audio/fetch_library.py`, `Editor/Audio/SoundLibrary.cs`, `Tools/Audio/library.csv` | CC0 Freesound recordings: footsteps on 8 surfaces (aliases for the rest), impacts per material light/heavy, structure (creak, groan, snap, debris, rubble, cracks), doors, lockers, shutters, breakers, gear, water, power, alarms, weather, foghorn, steam, chains, monster vocals (slowed), environment loops (wind, hum, room tone, surf, engines). Trimmed, faded, levelled per kind, loops crossfaded. They replace the Kenney clips and synth patches in the bank; Layered cues keep their synth layer. Never recorded (`SoundLibrary.Never`): the heartbeat, breathing, monster signatures and monster songs; DistantSettle (a recorded settle is a creak, and a creak only ever means the floor) and PlayerHurt (a recording is one person's voice; the crew are anyone). |

Not done in this pass: the plan's recorded-instrument music. The songs are rendered on a worker thread from synth voices;
moving them to samples touches the monster songs and their 15-second warning, so the music is unchanged.

## Menus (Tools > Abandoned)

- Art: Apply Render Budget (now the whole renderer), and 3060-3065: Apply 0.24.12 Look to Saved Maps, Upgrade Surfaces
  (Scanned Textures), Make Decal Set, Scatter Wear Decals (Open Scene), 0.24.12 Look, Surfaces, Sound and Decals.
- Content > Audio: Build Recorded Library (0.24.12).
- Rebuild All Maps now ends with "0.24.12 look, surfaces, sound and decals".
- Batch: `Tools/unity.sh exec Abandoned.EditorTools.AvOverhaul.AuthorAll` (rebuild every map but the HQ, then all of the
  above); `AvOverhaul.AfterMapsAndExit` (all of the above on the saved maps, no rebuild); `AvOverhaul.RefreshAndExit`
  (renderer, looks, surfaces only); `AvRenders.RenderAndExit` (review renders into Game/Logs/renders/av, with fog-only
  and without-fog/eye comparison views).

## F1

Top right: Forward+, the fog's level, density, resolution and steps, or why it isn't running (V shows the fog alone), and
the eye's exposure; the audio lines gain the mix state, the master bus's gain reduction, the ambience duck and the
spatial mode.

## Status (2026-10-21)

- Compiled; every map rebuilt (HQ kept), 128 materials upgraded, 257 recordings processed (51 named cues and 25
  surface rows play recordings), wear decals on five maps (the HQ has no NavMesh, so none there). Text check 0 missing.
- Builds from d1fbf784: `~/Documents/Abandoned-builds/dev/Abandoned-0.24.11-d1fbf784-{Mac,Windows}.zip` (the version
  stamp still says 0.24.11; BuildScript.Version wasn't bumped). Zips test clean, the Mac signature verifies, Steam Audio
  is in both.
- Review renders (Game/Logs/renders/av): the fog, cookies, scanned surfaces and grade render without errors. Interiors
  look darker in these batch captures than the game should: a batch session doesn't derive the ambient from the scene's
  colours the way play does. Check the brightness in play first.
- Never played. Not heard: the recordings were chosen by Freesound rating and downloads, not by ear. Nine searches
  found nothing (glass and puddle footsteps, heavy concrete and sandbag impacts, ceramic crack, shutters, sledgehammer,
  crowbar); those keep their Kenney or synth sound.
- Unchanged: the music. Not measured: frame time (volumetric light is the heaviest effect; Graphics > Volumetric light).

## What to check first

1. Brightness in the mall and factory with power on and off (eye adaptation is soft; Graphics > Brightness still works).
2. The flashlight beam in dust, and a crewmate's beam (unshadowed lights scatter at 25 %).
3. Footsteps, creaks and impacts on headphones (Steam Audio placement), and the hiding muffle in a locker.
4. F1: the render line (V = fog only) and the MIX line (master gain reduction, ambience duck, spatial mode).
5. Frame time on the friend's Windows PC at Medium.
