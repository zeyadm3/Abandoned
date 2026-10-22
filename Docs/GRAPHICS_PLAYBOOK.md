# Graphics playbook: how the TestMap site and the map sites were made (2026-10-22)

A record of the October 2026 graphics work, written so it can be repeated or extended without rediscovering anything.
The short version: **real photographed assets (Poly Haven, CC0), a photographed sky, real terrain and vegetation, a
level that is a place rather than a box, and judging every step from screenshots.** Details per map:
`Docs/TESTMAP_SHOWCASE.md` (the TestMap) and `Docs/MAP_SITES.md` (the six real maps).

---

## 1. What was done

### TestMap (commit 96b96fc9)
The 20 x 16 m warehouse was turned into a fenced industrial yard on a 640 m wooded terrain. The tested block (tile
grid, section names, stairs, doors, spawn row, loot) was kept exactly, so no PlayMode test needed changing.
- **Level:** gravel road through the forest, power line, gate and guard hut, cracked forecourt with cars, trailer,
  skip, pallets and wet patches, boiler-house ruin and chimney, container lot, water tower, lean-to, chain-link fence.
- **Nature:** ~8,000 scanned trees with LODs and impostors, scanned grass card, ferns, weeds, rocks, logs; eight
  scanned ground layers painted by rule.
- **Light:** HDRI sky (evening_road_01), matched sun, a TestMap-only Shadowmask bake (lightmaps + light probes),
  warm haze and grade.
- **Materials:** factory_brick at real scale, scanned asphalt, toned container claddings.
- **Sound:** birdsong, wind in trees, crickets, crows (four new SoundIds, CC0 Freesound recordings).

### Real maps (commit 624e894a)
A re-runnable pass, **Tools > Abandoned > Art > Map Sites: Sky, Trees, Grass and Sound**, added to HQ, Mall, School,
Iron Factory, Island Prison and Tutorial House. Realtime light only (user's choice; power cuts and collapses), each
map keeps its hour and weather. Sky, brick/asphalt swap, tree swap, snowy land past the edge (School, Tutorial),
grass, outdoor sound. Rebuild All Maps runs it last.

---

## 2. Where everything is

| What | Where |
|---|---|
| TestMap builders | `Game/Assets/_Project/Editor/TestMap/Showcase*.cs` (`ShowcaseSitePlan` holds every position) |
| Map-site pass | `Game/Assets/_Project/Editor/Art/MapSite/` (`MapSiteProfiles` = per-map settings) |
| Runtime pieces | `Scripts/Core/Rendering/GrassField.cs`, `Scripts/Core/MapSiteRecord.cs`, `Scripts/Audio/OutdoorSoundscape.cs` |
| Asset tools | `Tools/Art/fetch_nature.py` (download + bake), `bake_tree.py`, `bake_kit.py`, `bake_card.py`, `bake_ground.py`, `bake_common.py` |
| Scanned models | `Art/ThirdParty/PolyHaven/<id>/` |
| Ground/cladding scans | `Art/TestMap/Ground/` |
| Tree prefabs, grass card | `Art/TestMap/Nature/` |
| Skies | `Art/TestMap/Sky/` (TestMap), `Art/Skies/` (maps) |
| Sound sources | `Tools/Audio/fetch_library.py`, `Tools/Audio/library.csv`, `Art/Audio/Library(Source)/Cues/` |
| Credits | `Docs/ASSET_CREDITS.md` (every asset must be listed) |

---

## 3. How to do it again

### Rebuild
- TestMap: **Tools > Abandoned > Maps > Test Map > Rebuild Test Map** (~15 min, includes the bake). Re-bake only:
  **Bake Test Map Lighting**.
- Real maps: **Tools > Abandoned > Art > Map Sites: Sky, Trees, Grass and Sound** (~1 min, idempotent).

### Fetch or add assets
```
FETCH_CACHE=<scratch dir> python3 Tools/Art/fetch_nature.py          # everything missing
FETCH_CACHE=<scratch dir> python3 Tools/Art/fetch_nature.py --force fir_tree_01   # one model again
```
- Add a model: put its Poly Haven id in `TREES` (with LOD spec) or `KITS` in `fetch_nature.py`, run it, then place
  it with `ShowcaseNature.Part(id, partName, ...)` (parts are the FBX's children; list them with `ShowcaseNature.Parts`).
- Add a ground scan: add the id to `GROUND_SCANS`; use it with `ShowcaseMaterials.Rescan(material, id, metresPerRepeat)`
  or as a terrain layer.
- Add a sky: add it to `MAP_SKIES`, measure its sun's u (brightest pixel; see section 5), add a profile line.
- Add sounds: add a `T` row in `Tools/Audio/fetch_library.py`, fetch only that target, then run **Build Recorded
  Library** (processes only new files). New SoundIds are appended at `// @@ids` and get a row at `// @@catalog`.
- **Always** add a row to `Docs/ASSET_CREDITS.md`. Only CC0 (Poly Haven, Freesound CC0). No new packages.

### Give a new map the site pass
Add a `MapSiteProfile` line in `MapSiteProfiles.cs`: scene name, sky, sky sun u, sky level, **sky exposure**, winter,
grass, ring, sound levels. Add the scene to `AvOverhaul.MapScenes` if it isn't there. Run the menu item.

---

## 4. Techniques (what worked)

1. **Scanned trees as game trees.** Scans are millions of leaf cards; decimating cards destroys them. `bake_tree.py`
   keeps a random share of the cards and grows each kept card so the crown keeps its cover (LOD0 ~95k card faces for
   the tall fir, LOD1 ~18k), decimates the wood on its own object, and renders a two-quad impostor for LOD2.
   Leaf textures need `mipMapsPreserveCoverage` or distant trees go bald. Card smoothness ~0.3 or needles look frosted.
2. **Grass:** render scanned clumps into one alpha card (`bake_card.py`). On a Unity terrain use it as waving terrain
   grass (`DetailScatterMode.InstanceCountMode`!). On mesh ground use `GrassField` (instanced, drawn per camera in
   `beginCameraRendering`, so captures and every camera see it).
3. **Terrain:** create the TerrainData **asset first**, then set heights, layers, alphamaps, trees and details on it
   (otherwise the paint is lost). Paint layers by rule (slope, distance to road/forest, zones). For land around an
   existing map, cut the map's extent out with **terrain holes** so nothing inside changes.
4. **Level design:** keep the tested core, build a *place* around it: approach road, gate, landmarks visible from
   everywhere (chimney, water tower), a second explorable space, clutter that tells a story, forest closing the edges.
5. **Sky:** a photographed HDRI as a Cubemap skybox, rotated so its sun is behind the scene's sun:
   `_Rotation = (0.5 - u) * 360 - atan2(toSun.z, toSun.x)`.
6. **Ambient calibration (real maps):** Unity applies ambient intensity and sky exposure roughly **to the power 2.2**.
   Measure the old Trilight ambient probe, then correct the intensity a few times until the skybox probe matches.
   Draw the sky near the old sky's brightness (`SkyExposure`), or eye adaptation darkens or brightens the whole map.
7. **Baked light (TestMap only):** Shadowmask, GPU lightmapper, directional lightmaps, Mixed sun, light probes for
   anything that moves or collapses; models without UV2 and small objects get `ReceiveGI.LightProbes`.
8. **Materials:** a real scan at its real repeat beats tinting a generic one (factory_brick 1.5 m, asphalt_02 5 m).
   Glossy puddles without real reflections read as snow; use dark matte wet stains.
9. **Swapping old trees:** find by name + tree shape; merged tree patches are split by clustering vertices on a 2 m
   grid. Switch originals off and record them (`MapSiteRecord`) so the pass is undoable.

---

## 5. Working method and pitfalls

- **Work from screenshots.** Move the scene's **Main Camera** and capture with it (it has the post-processing).
  Compare a before shot from the same spot when judging brightness. Never "fix" from numbers alone.
- **Unity MCP:** long `execute_code` calls time out and the server **retries** them (a build ran twice). Guard with
  `if (SessionState.GetBool("job-N", false)) return; SessionState.SetBool("job-N", true);` and follow
  `~/Library/Logs/Unity/Editor.log` (record its size, grep from there for a marker you log). `delayCall` never fires
  while the editor is unfocused. Async bakes can be polled with `Lightmapping.isRunning`.
- **Never use `Tools/unity.sh` while Unity is open.**
- Blender (`/Applications/Blender.app`) runs headless with `--factory-startup`; it ships numpy. Run scripts with the
  downloads passed as arguments. Its EEVEE renders can abort when Unity holds the GPU: retry once.
- Measure a sky's sun: load the HDR in Blender, take the brightest pixel; u = x / width.
- Test-affecting changes: removing a ground object changes `SurfaceTests` (tag the replacement), and keep spawn rows,
  door lanes and the truck spot clear.
- **Commits:** the working tree carries the user's uncommitted work. Stage only your own files; for a file the user
  also edited, stage a blob of `HEAD` plus your lines (`git hash-object -w` + `git update-index --cacheinfo`).
  If the user has staged deletions, unstage them, commit, restage them. `.hdr` and `.exr` are in Git LFS.
- The six map scenes and the shared remaster materials changed by the map-site pass were **not committed** (they hold
  the user's uncommitted remaster work); re-running the menu item reproduces them.

---

## 6. Open items to check
- Frame rate: Prison/Mall grass, School/Tutorial ring trees, TestMap forest (levers: `GrassField.drawDistance`,
  terrain `detailObjectDistance`, fir LOD0 card count in `fetch_nature.py`).
- Each map's brightness in play (`SkyExposure`).
- Swapped trees' colliders (NavMesh not rebaked).
- TestMap: shadowmask hand-off at 35 m; terrain footsteps all "Dirt".
