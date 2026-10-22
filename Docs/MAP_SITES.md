# Map sites: the TestMap's look on every map (2026-10-22)

The user, after the TestMap site pass: "apply all techniques and things you have learned editing the test map to all
the other maps in the game to make them all the same quality as this". Their answers to the open questions:
**realtime light only** (no baking on the real maps: their power goes off and their floors fall, M7.2 stands),
**add to the saved scenes** (no map is regenerated, so hand edits survive), **each map keeps its own hour and
weather**, and one commit.

## Run it
**Tools > Abandoned > Art > Map Sites: Sky, Trees, Grass and Sound** (3083) applies it to all six maps and saves them
(about a minute). It is idempotent: everything it adds lives under one `_MapSite` object per scene, and a run first
puts back what the last one replaced. **Rebuild All Maps** runs it last (`AvOverhaul.AfterMaps`), after the art
remaster and the lighting rebuild, so a map rebuild gets it again.

Code: `Editor/Art/MapSite/` (`MapSite` runs the steps, `MapSiteProfiles` says what each map gets), runtime pieces
`Scripts/Core/Rendering/GrassField.cs`, `Scripts/Core/MapSiteRecord.cs` and `Scripts/Audio/OutdoorSoundscape.cs`
(the TestMap's soundscape, renamed and shared).

## What each map gets
| Map | Sky (Poly Haven, CC0) | Trees swapped | Land past the edge | Grass tufts | Outdoor sound |
|---|---|---|---|---|---|
| HQ | kloppenheim_01 (cloudy dusk) | 27 (the verge and boundary patches) | – | 8,140 | wind, a few birds, crickets, crows |
| Mall | kloppenheim_07 (night, town glow on cloud) | 39 | – | 79,027 | wind, crickets |
| School | kloppenheim_06 (low winter sun, broken cloud) | 39 | wooded snowy hills, ~33,000 trees | none (snow) | wind, crows |
| Iron Factory | industrial_sunset_02 (smoky dusk) | 0 (none to swap) | – | 36,943 (weeds on dirt) | wind, crickets, crows |
| Island Prison | kloofendal_43d_clear (noon) | 50 | – (the sea) | 118,207 | wind, birds (the surf is its own) |
| Tutorial House | kloofendal_misty_morning (misty December) | 110 | wooded snowy hills, ~15,000 trees | none (snow) | wind, crows |

## How each step works
1. **Sky (MapSky).** The photograph is the skybox, the ambient and the reflections. It is turned so its own sun stands
   behind the map's existing sun, whose direction and strength are not changed (shadows stay as tuned). The ambient is
   **matched to the map's old three-colour ambient**: the pass switches back to it, measures the ambient probe, then
   corrects the sky's intensity until the probe reads the same (Unity applies ambient intensity roughly to the power
   2.2, so it takes a few steps). The old colours stay stored in each scene, so a re-run measures the same target.
   The sky is drawn at an exposure close to the old sky's brightness (`SkyExposure`): far brighter and the eye
   adaptation darkens the whole map (the HQ went black), far darker and it brightens it (the Mall's night went blue).
2. **Brick and asphalt.** Wherever the remaster's brick (Bricks106A) or road asphalt (Asphalt031) scan is used, the
   TestMap's factory_brick (1.5 m) or asphalt_02 (5 m) replaces it, keeping the material's mapping and rescaling it.
   This edits the shared remaster materials in place.
3. **Trees (MapTrees).** Anything named like a tree and shaped like one (2.5-40 m tall, no wider than tall) becomes a
   scanned tree of the nearest height. A merged patch of trees near the map (the HQ's verges) is split into its trees
   by clustering its vertices on a 2 m grid. Far belts and horizon trees stay. Winter maps get firs and snags only.
   The originals are switched off, not deleted. Anything holding a structural section or a NetworkObject is left.
4. **Land past the edge (MapRing, School and Tutorial).** A terrain that starts level with the map's ground at the
   edge of everything the map draws and rises into hills, under snow, with scanned firs thickening with distance.
   The map's whole extent is cut out of it (terrain holes): nothing inside the map, basements included, is touched.
5. **Grass (MapGrass + GrassField).** The ground is sampled from above on a 0.85 m grid; tufts of the TestMap's grass
   card grow only outdoors (open sky above) on ground whose material classifies as grass (dense), soil (thinner) or
   gravel (a few weeds), in drifts. `GrassField` draws them instanced, per camera, in chunks within 70 m. No
   colliders, never networked.
6. **Sound.** `OutdoorSoundscape` beds round the map's edge with each map's levels.

## What to look at first
1. **Brightness in play.** Ambient is matched to the old level by measurement, and sky exposure by eye from editor
   captures; each map's eye adaptation in play is the real test. `SkyExposure` in `MapSiteProfiles` is the lever.
2. **Performance:** the Prison's and Mall's grass (118k and 79k tufts, drawn within 70 m) and the School's ~33,000
   ring trees. Levers: `GrassField.drawDistance`, the profile's `Grass`, `MapRing`'s tree cell size.
3. **Swapped trees with gameplay meaning.** The old trees' colliders are switched off with them; the scanned trees
   have a trunk collider (firs) or none (saplings). The NavMesh was not rebaked.
4. **Shared materials.** The brick and asphalt swap changes the remaster's material assets, so every map that uses
   them changes, including any map not listed here.

## Not checked
Applied and reviewed from editor captures only (Game/Logs/tmshots/maps). Never played; no tests run.
