# The 2026 remake

Started 2026-10-27. The user asked for the whole game to be remodelled with the Blender MCP and the Unity MCP:
player characters (now a roster of eight), all eight monsters, all loot, the UI, every map's buildings and props,
the buyable gear, the collection truck and its upgrades, the wardrobe; plus two new features, **truck cosmetics**
and **the hiring** (a first-launch HR interview, then the training, then "you're hired", then the character
creator, then the HQ), after which the game is HQ-centred and the Esc menu only resumes, opens Settings, leaves
or quits. The approved plan is copied at the end of this file ("The plan").

Decisions (user, 2026-10-27): "PUs" means pickups and UIs; maps keep their layouts and get new art; all maps
including the five newer ones; models from original Blender work and Poly Haven (CC0), **no AI generators**;
grounded realistic style; pick one of eight workers then dress them; UI "clean modern horror"; a commit to `main`
per phase. Out of scope: the open world and the TestMap. **No testing, verifying or running the game**: compiling
so editor builders can run is part of building; the user checks everything afterwards.

## Art bible

**Tone.** Grounded and realistic, worn by use, never cartoon. Things look like they were bought cheap, used hard
and left behind: chipped paint to bare metal on edges, grime in crevices, sun-faded plastics, scuffed rubber.
Monsters are realistic and wrong: believable flesh, cloth, stone, bone; their silhouettes must stay readable
because gameplay depends on recognising them.

**Scale and axes.** One unit is one metre. Blender models face **+Y** (Unity +Z), +Z up, +X to the model's right;
`core/export.py` writes Unity space. Pivots: props at the base centre; gear at the grip; characters at the feet.

**Texel density.** 512 px per metre (`core/bake.py` picks the atlas size from surface area, 256 to 2048).

**Texture sets.** `<Set>_BaseColor` (sRGB), `<Set>_Normal` (OpenGL tangent, Unity's convention), `<Set>_Mask`
(R metallic, G occlusion, B 0, A smoothness; URP Lit reads it as both _MetallicGlossMap and _OcclusionMap).
Glass, emissive, screens, hair cards and decals are **shared** materials (`Art/Remake/Shared`), never baked.

**Triangle budgets (LOD0; LOD1 ~40 %, LOD2 ~12 %).**

| Asset | LOD0 |
|---|---|
| Playable character (body + head) | 35k |
| Outfit layer | 8k |
| Monster | 25-60k |
| First-person gear | 3-8k |
| Truck | 120k |
| Loot | 0.5-8k by size |
| Architecture module | 0.2-4k |

The mall's 12,500 render-budget limit stays.

**Palettes per map (base tones the kits lean on).**

| Map | Palette |
|---|---|
| HQ | oxide red steel, galvanised grey, yellow safety paint, tired fluorescent white |
| Mall | terrazzo cream, brushed steel, faded brand colours, dusty glass |
| School (Pine Hollow) | pale green tile, varnished oak, chalk dust, winter light |
| Iron Factory | soot black, rust orange, molten amber, cold steel blue |
| Island Prison | salt-bleached concrete, flaking institutional green, iron bars, sea grey |
| Tutorial House | 70s ranch: wood panelling, beige carpet, Michigan winter |
| Hospital | clinical white, mint green, stainless steel, stained ceiling tiles |
| Motel | sun-bleached teal and coral, nicotine yellow, neon |
| Police / Fire stations | municipal beige, navy, fire-engine red, polished brass |
| Skyscraper | glass, black granite, brushed brass, smoked mirror |

## Pipeline

**Blender** (`Tools/Blender/remake/`): `core/` holds the shared helpers (kit, materials, bake, export, rig, sculpt,
fracture, polyhaven, preview); one folder per area holds the asset scripts. Every asset is a script, so every asset
can be rebuilt. Build in batch mode from the repository:

```sh
/Applications/Blender.app/Contents/MacOS/Blender --background --factory-startup \
    --python Tools/Blender/remake/<area>/<script>.py -- <args>
```

macOS keeps the live Blender (the MCP's) out of `~/Documents`, so:
- batch scripts save a preview `.blend` with `core/preview.save(name)` into `~/Library/Caches/AbandonedRemake/preview`;
- `Tools/Blender/remake/sync_live.sh` mirrors the sources to `~/Library/Caches/AbandonedRemake/src`, and the live
  Blender boots with `exec(open(os.path.expanduser('~/Library/Caches/AbandonedRemake/src/remake/boot.py')).read())`,
  then `preview.show(name)` and the MCP's `look` to see the model.

**Unity** (`Editor/Remake/`): `RemakeImport` sets import rules for `Art/Remake/**` (visual only, Generic rig for
characters and threats, textures typed by suffix); `RemakeMaterials` builds URP Lit materials from each set
(`Art > Remake: Refresh Materials` after a batch export).

**Long Unity operations** through the MCP: guard each with a SessionState token, log a marker, write results under
`Game/Logs/remake/`, wait with a deadline plus a Unity CPU idle check, one map per call.

**Poly Haven** assets are fetched by `core/polyhaven.py` (cache in `~/Library/Caches/AbandonedRemake/polyhaven`)
and credited in `Docs/ASSET_CREDITS.md`.

## Progress

| Phase | State |
|---|---|
| 0 Pipeline and art bible | done (2026-10-27) |
| 1 Characters and wardrobe | done (2026-10-27): 8 workers, 10 outfits x 3 builds, 20 hats, 15 accessories |
| 2 Monsters | done (2026-10-27): all 8 sculpted, baked, skinned, five takes each |
| 3 Gear and shop | done (2026-10-27): 17 items; rack and shop terminal are remodelled with the HQ (phase 6) |
| 4 Truck, upgrades, van, truck cosmetics | done (2026-10-27) except the arrival van and the Prison boat hull (see notes) |
| 5 Loot | done (2026-10-27): all 345 items remastered and installed |
| 6 Maps | done (2026-10-27) for props and fittings: 75 kit models, 5,327 props swapped across the 11 maps; architecture itself not remodelled (see notes) |
| 7 The hiring and the HQ-centred game | done (2026-10-27): HR office, interview, training, "you're hired", creator; title screen, cut-down pause, HQ stations |
| 8 UI | done (2026-10-27): clean modern horror theme layer, 3D icons for gear, cosmetics, truck fittings and loot |
| 9 Integration and handover | done (2026-10-27) |

## Phase 1 notes (characters and wardrobe)

- Blender: `Tools/Blender/remake/crew/` - `characters.py` (heads; `face.py` parameterised sculpt, `skin.py`, `hair.py`,
  `roster.py`), `outfit_build.py` (`outfits.py` recipes on the 2026-10-22 crew garments, three builds by reshaping one
  mesh so they share UVs), `wearables.py` (hats and accessories on the crown), `skinned.py` (merge, weights, bake source).
- Unity: `Editor/Remake/CrewLookBuilder` (Content > Characters > Build Crew Looks (remake)) re-skins every FBX onto
  WorkerRig's joint order with rest bind poses, writes `Character_*` / `Outfit_*` definitions and appends them to the
  catalog. `Scripts/Player/Presentation/WorkerLook` swaps meshes at run time; the ragdoll has bone helpers at the same
  rest positions, so one set of bind poses serves both. `CosmeticChoice` gained Character and Outfit bytes (5 bytes on
  the wire). The coverall colour tints each outfit's `_Tint` material (same textures as its base set).
- Wardrobe tabs: Worker, Outfit, Colour, Hats, Accessories. Text: `Data/Text/.../remake_crew.txt`.
- Not done here: the first-person gloves are still the 2026-10-22 crew gloves for every outfit; the crew numbers
  (TextMesh badges) are gone from the skinned worker.

## Phase 2 notes (monsters)

- Blender: `Tools/Blender/remake/threats/` - `anatomy.py` (node graphs skinned into one sculpt with the Skin modifier,
  voxel remesh, node-label skin weights), `monsters.py` (the eight designs, horror flesh and stone materials),
  `takes.py` (the same rig and take names/lengths as Tools/Blender/threats.py, eased keys, a Special per monster).
- Same heights, widths, hip heights and arm lengths as the 2026-10-08 threats, so agents, reach and timing still fit.
- Unity: `Art/Remake/Threats/<Name>.fbx`; `ThreatModelImporter` handles that folder too; `ThreatRigBuilder` prefers the
  remake model and keeps its baked material; `Editor/Remake/MonsterInstaller` (Content > Prefabs > Install Remade
  Monsters (remake)) upgrades the eight prefabs, and Rebuild Entire Game runs it after the threat builders.
- Blender's voxel remesh returns almost nothing below about 1.2 cm voxels; `anatomy.Body.build` clamps to that.

## Phase 3 notes (gear)

- Blender: `Tools/Blender/remake/gear/toolart.py` is a twin of `Scripts/Equipment/ToolArtBuilder` (same primitives,
  Unity coordinates, sizes as diameters), so `gear.py` ports every C# design line for line and adds bevels, turned
  profiles, knurled grips and worn baked paint. Grip at the origin, +Z forward, as before.
- Unity: `Scripts/Equipment/GearModels` (Data/Equipment/Resources/GearModels) maps each kind to its model;
  `ToolModels.Build` (first person, crewmates' hands, dropped tools) uses it and falls back to the code-built mesh.
  `Editor/Remake/GearModelBuilder` (Content > Data > Create Gear Models (remake)) also dresses the thrown noise maker
  and the deployed drone; the glow stick keeps its emissive body, which is its light.

## Phase 4 notes (truck and truck cosmetics)

- Blender: `Tools/Blender/remake/truck/truck.py` builds the cab-over truck and 29 parts in TRUCK SPACE (Unity
  coordinates relative to the truck root), keeping the 0.15.7b footprint (box +-1.41 x -2.6..2.6, deck 1.05, roof 3.4,
  cab to z 5.07, axles at 3.75 and -1.65). Painted body faces use `RM_Truck_Body_Tint` (same textures as the body).
- Unity: `CollectionTruck0157.Upgrade` installs the remade body (`Remake/TruckRemakeBuilder.Install`) instead of the
  code-built art, keeping cargo, deck, ramp, ignition, display, lights and the hinged rear doors. Boats are skipped.
  `Art > Remake: Refit Trucks in Saved Maps` re-runs it on every saved map (5 maps have the truck; the four newer
  places use the open-world van).
- Truck cosmetics (new feature): `Scripts/Company/TruckCosmetics/` - `TruckCosmeticSlot` (Paint, Livery, Bumper, Roof,
  Wheels, Mudflaps, Ornament, Horn), `TruckCosmeticDefinition`, `TruckCosmeticCatalog` (Data/Company/Resources, 39
  items, append only, under 64), `TruckLook` (on every truck: fitted parts, paint on the `_Tint` slot of each LOD,
  owned upgrades' `VisualPrefab`, the fitted horn). `CompanySave.truckLook` ("Slot:id"), owned ones in unlocks as
  `truckcos:id`; `CompanyNetState.TruckCosmetics` (64-bit owned mask) and `TruckLook` (one byte per slot).
  `CompanyService.TruckCosmetics.cs`: buy and fit, host-checked (money, level, the crew's spending rule).
- Horns: SoundIds HornAir, HornTrain, HornGoose, HornDeep (appended) with synth patches; the departure horn is the
  fitted one (`TruckLook.CurrentHorn`).
- Garage screen: `Scripts/UI/Garage/TruckGarageView` (+ `TruckGaragePreview`, Resources/TruckPreview), `MenuScreen.TruckGarage`,
  `MenuUi.OpenTruckGarage`; `TruckGarageTerminal` is the HQ desk (placed in the HQ in phase 7).
- Remake models imported before their materials existed got empty slots; `RemakeMaterials.EnsureModelMaterials`
  re-imports them, and every remake builder calls it first.
- Not done: the arrival van (still the Kenney van) and the Island Prison boat's hull keep their old art.

## Phase 5 notes (loot)

- `Editor/Remake/LootExport` (Content > Loot > Export Loot for Remaster (remake)) writes every loot prefab's visible look
  in prefab space to `Game/Logs/remake/loot/*.obj` + `manifest.json` (345 items: the 239 originals plus the newer maps').
- `Tools/Blender/remake/loot/remaster.py` reads them (own OBJ reader; Blender = (-x, z, y) of the OBJ), welds, gives
  light models one creased subdivision, bevels hard edges, classifies each material into a worn surface (or keeps its
  texture), decimates to the carry class's budget, bakes one 512/1024 set, writes `Art/Remake/Loot/Loot_<id>.fbx`
  with three LODs. Log: `Game/Logs/remake/loot.log` (0 FAILED of 345).
- `Editor/Remake/LootInstaller` (Content > Loot > Install Remastered Loot (remake)) points each definition with a
  remaster at it (yaw 0: the export already carried the old yaw; the Size box is kept, so colliders, spawns and weight
  don't move) and regenerates those prefabs in place (GUIDs and network ids stay). Rebuild Entire Game calls
  `LootInstaller.Assign` before `GenerateAll`.
- Look-alikes: nothing to re-run. `LootLookalikes` swaps scenery placed from the dressing prefabs, which are separate
  from the loot prefabs; the remastered loot now looks less like any old scenery, not more.
- Loot icons are re-rendered with the UI in phase 8.

## Phase 6 notes (maps)

- Inventory: `Editor/Remake/MapInventory` (Art > Remake: Inventory Open Map) wrote `Game/Logs/remake/maps/<Scene>_inventory.json`
  for all 11 maps (about 230,000 renderers). The maps are mostly code-built from primitives, so the remake works on
  **props and fittings**: whole groups named like a desk, a car, a window, a shelf.
- Kit: `Tools/Blender/remake/maps/` - `parts.py` (boxes by extents, posts, rails, lathes, frames; bake and LOD export),
  `kit_furniture.py`, `kit_services.py`, `kit_street.py`, `kit.py` (batch entry). 46 families, 75 models
  (`Art/Remake/Maps/Kit/Kit_<family>_<a-d>.fbx`, one baked set each, three LODs). Log: `Game/Logs/remake/kit.log`.
- Matching: `Editor/Remake/KitFamilies` (names, a real one's size, a tolerance, how it fits: stand, stretch or hang,
  whether it backs onto a wall, one style per map for windows). A group is swapped only if it only draws: renderers,
  colliders, LOD groups, section props and passive markers (SurfaceTag, NavMesh hints). Anything with a loot spawn
  point, a door, a light, text or a script is left alone, as are PolyHaven scans, remake models and the 0.24.9
  look-alike stand-ins (dust sheets, packaging, fixed fittings). `Art > Remake: Survey Kit Props in All Maps` writes
  `kit_survey.txt` (matches with sample names, what was turned down and why, prop-like names no family knows).
- Swap: `Editor/Remake/KitSwap` takes the group's renderers off (recorded in `KitSwapRecord` on the scene's `_KitSwap`
  object, with LOD levels) and stands the model in its place: proportions pick the variant and quarter turn,
  wall pieces turn their back to the nearest wall (raycast), the old layer, storey light layers, shadows and static
  flags carry over. Colliders stay, so **no NavMesh or occlusion rebake**. Re-running restores everything first;
  `Remake: Undo Kit Props in Open Map` restores only. `AvOverhaul.AfterMaps` runs it right after the map looks, so
  Rebuild All Maps keeps it. `ObjectRemaster` now skips anything under `Art/Remake/`.
- Swapped: HQ 203, Mall 214, School 399, Factory 181, Prison 156, Tutorial 50, Hospital 850, Motel 684,
  Skyscraper 606, Police Station 1,035, Fire Station 949.
- Not done in this phase: walls, slabs, stairs and facades keep their code-built geometry and 0.24.12/13 materials
  (no architecture kit or new fracture stages); the HQ gear rack and shop terminal (functional, left alone); the
  arrival van; the Prison boat hull. The kit was swapped into the saved scenes without running the rest of the map
  chain (CoplanarFix, remaster) afterwards; Rebuild All Maps runs everything in order.

## Phase 7 notes (the hiring and the HQ-centred game)

- Flow (`Scripts/Onboarding/OnboardingFlow`, on the session object): first launch, once the early-access notice is
  closed, loads `Scenes/HrOffice` offline: the interview; then hosts solo at the HQ and goes straight to the training
  house (as the old Training card did); when the training is finished and the crew is home, it ends the session and
  loads the HR office again (`ReturnToMenu.Next`): "you're hired", then the character creator, then the HQ.
  Progress is local (`OnboardingProgress`: New, Interviewed, Trained, Hired + creator done), so quitting resumes there.
  A machine that played before (a finished training, a run or a company save) counts as hired and sees the creator
  once. During the hiring's training, the pause menu hides "skip lesson" and "leave training".
- A Steam invite accepted before being hired (`SteamLobbyFlow.InviteGate`) is kept and joined after the creator; if
  that lobby is gone, the player gets their own HQ and a toast.
- The editor auto-hosts on Play, which skips the title screen and so the hiring: test it in a build, or turn off
  `NetworkConfig.AutoHostInEditor`. Maps > HQ > Reset Hiring on This Machine starts it again.
- The scene: `Editor/Onboarding/HrOfficeBuilder` (Maps > HQ > Rebuild HR Office (hiring)): a small office from the map
  kit, the manager (Dana Whitfield, fictional; `Tools/Blender/remake/onboarding/hr_manager.py`, the crew pipeline with
  her own face and an office outfit, built as a look outside the wardrobe by `CrewLookBuilder.BuildNpc`), six
  Cinemachine shots (shot 0 the seated first-person view with limited look, five set-ups). `HrManagerActor` seats the
  crew figure, turns her head to you and nods with her voice (no blinks or jaw: the crew rig has no face bones).
- The scenes are data (`Data/Onboarding/Dialogue_*.asset`, written from `Editor/Onboarding/OnboardingDialogue`): lines,
  the shot to cut to, replies that change a line (never the outcome) and conditions on how long the training took.
  `CutsceneDirector` plays them on Cinemachine priorities and the brain's blends (Timeline wasn't needed). Click moves
  on; holding Space skips the rest.
- Voice: `Tools/Audio/generate_hr_interview.py` (Kokoro-82M, voice af_sarah) records every `Say(...)` line to
  `Art/Audio/Onboarding/Resources/Onboarding/hr_<md5>.wav` (28 lines). Subtitles in all nine languages
  (`Data/Text/Resources/Text/remake_onboarding.txt`).
- Title screen: Continue (hosts the chosen save), the save slot, Settings, Credits, Quit; development builds keep
  "Join by address (dev)" (the old Play screen) for LAN tests and nettests. Training is the HQ clipboard.
- Pause menu: Resume, Settings, Leave game, Quit to desktop (plus the training buttons in the practice house).
- HQ stations (`Editor/Onboarding/HqStations`, Maps > HQ > Add Remake Stations to HQ, added to the saved HQ in place):
  the crew board in the dispatch office (`CrewBoardView`: the crew, voice levels, kick, Steam invites, host rules), the
  trophy cabinet in the crew room (achievements), the notice board (how to play), and the truck garage desk by the
  garage door (`TruckGarageTerminal`, from phase 4). Per-player voice levels are also in Settings > Voice
  (`CrewRoster`, shared by both).
- Rebuild All Maps runs the stations and the HR office after the training clipboard.

## Phase 8 notes (UI)

- Theme: `Art/UI/Resources/Remake2026.uss`, loaded last in `UiThemeAssets.Layers` (the older sheets stay, in order):
  a colder, darker palette (the `.horror-theme` variables), one sodium-amber accent and a blood red for danger, flat
  panels with hairline rules, sharp corners, a focus bar on buttons, flat cards with the accent on the chosen one,
  and the new screens (subtitles, creator, crew board, cut-down pause). No class was renamed, so every screen and HUD
  piece picks it up without code changes; fonts are the project's OFL set.
- Icons: `Editor/Remake/IconRenderer` (UI > Render Remake Icons) renders every remade model alone on a transparent
  ground into `Art/Remake/Icons/`: gear (`item/<id>`, replacing the drawn silhouette of the same name), hats,
  accessories, workers (portraits) and outfits (`cosmetic/<id>`), truck fittings (`truckcos/<id>`), and re-renders
  every loot icon with the remastered models; then it rebuilds the UiIcons registry. Rebuild Entire Game runs it last.
  The wardrobe, the character creator and the truck garage show the icon on each card (`UI/IconChip`, the letter
  stays as the fallback). MSAA x8 render textures come back empty on this Mac's Metal; the studio uses x4.
- Not done: the title screen's backdrop is still the HQ behind the menu (no staged crew-and-truck scene), and the HUD,
  contract board, shop, results and travel screens were re-themed, not re-laid out.

## Handover (phase 9)

Built, compiled and authored into the saved scenes and assets; **never played, tested or verified** (the user's
instruction). Commits on `main`: phase 0 `f3aaed061`, 1 `ce4cb9040`, 2 `ecef7920e`, 3 `7f58a3018`, 4 `3d7c891eb`,
5 `b6107e28d`, 6 `90c6ae494`, 7 `a8cfaa983`, 8 `1cb06acf8`, 9 (this one). Never pushed.

Rebuild Entire Game reapplies everything in order: gear models and truck cosmetics (data), loot (the remastered models
assigned before the prefabs), the crew looks, the monsters, every map (KitSwap inside AvOverhaul.AfterMaps), the HQ
stations and the HR office (after the training clipboard), and last the icons.

**What is not done (honest list):**
- Maps: walls, slabs, stairs and facades keep their code-built geometry (no architecture kit, no new fracture
  stages) [being worked on, see "After the handover"].
- Characters: first-person gloves are the same for every outfit; no crew number on the skinned worker.
- The HR manager has no blinks or lip sync (the crew rig has no face bones): her head turns and nods with her voice.
  Her office outfit is the crew's jacket shape in a dark office colour.
- UI: re-themed, not re-laid out (HUD, contract board, shop, results, travel screen keep their layouts); the title
  screen's backdrop is the HQ behind the menu, not a staged scene.
- The kit was swapped into the saved scenes without re-running the rest of the map chain (CoplanarFix, remaster);
  Rebuild All Maps runs them all in order.

**What to check first (in this order):**
1. Compile (`Tools/unity.sh compile`), then open each map and look at the swapped props (`_KitSwap` lists them;
   Art > Remake: Undo Kit Props in Open Map takes one map back). Windows are stretched to their openings, one style
   per map; wall pieces turn their back to the nearest wall by raycast, so check lockers, shelves and beds.
2. The hiring: a player build (or AutoHostInEditor off, then Maps > HQ > Reset Hiring on This Machine): interview,
   training, "you're hired", the creator, the HQ. Try quitting at each stage, and accept a Steam invite as a new hire.
3. The HQ: crew board (invite, kick, rules, voice levels), trophy cabinet, notice board, garage desk, the cut-down Esc
   menu, the title screen's Continue and save slot.
4. A Mall job with the remastered loot (sizes, grips, how it sits on shelves) and the remodelled gear.
5. Each of the eight monsters, the truck with upgrades and cosmetics, the wardrobe and creator icons.
6. Then `Tools/unity.sh all`, the nettests (cosmetic and truck-look sync), and the Checks menu audits.

## After the handover (user: "work on the unfinished items")

- **Vans and the launch:** `Tools/Blender/remake/vehicles/vehicles.py` (a panel van with the company band, a box
  delivery van, the launch's hull and wheelhouse). `Editor/Remake/VehicleRemake` (Art > Remake: Refit Vans and the
  Launch in Saved Maps) swapped every Kenney van in the saved maps at the same footprint (HQ 1, Mall 5) and dressed the
  launch (the hull in the swaying HullArt group; the wheelhouse over the solid block, whose collider stays).
  `Vehicles.Dress` prefers the remade vans and `IslandBoat.Build` dresses the launch, so rebuilds keep them.
  Remake models imported before their textures need Art > Remake: Refresh Materials (the vehicles did).
- **HQ gear rack and shop terminal:** `Tools/Blender/remake/hq/hq_stations.py`, each drawn in its station's own
  space; `Editor/Remake/HqStationModels` takes the old drawing off (recorded in `_KitSwap`, so undo brings it back) and
  stands the model in, keeping the colliders, the scripts and the terminal's amber screen. KitSwap.Apply on the HQ
  does it, so Rebuild All Maps keeps it.
- **Blinks and lip sync:** `Tools/Blender/remake/crew/face_shapes.py` gives every head (the eight workers and the HR
  manager) a Blink and a JawOpen shape key, plus a dark mouth cavity; `core/export.py` keeps shape keys (it skips
  modifiers for those meshes). `CrewLookBuilder.Combine` copies blend shapes into the combined look meshes (it writes
  into the existing mesh asset: copying a mesh with blend shapes over it crashed Unity). `Player/Presentation/WorkerFace`
  (added by WorkerLook) blinks every 2-6 s and opens the jaw with a crewmate's voice chat, or with the HR manager's
  recorded line. Honest limit: the head has no mouth slit, so the jaw drops the chin; the lips don't part.
  **Still to run in Unity:** Content > Characters > Build Crew Looks (remake), then Maps > HQ > Rebuild HR Office, then Art > Remake:
  Refresh Materials (the heads were re-exported; the saved look meshes don't have the shapes until then).
- **The HR meeting (user: "don't work, laggy, bugs"):** the editor's auto-host skipped the hiring (now a first-time
  host at home is sent to the office); the cursor stayed locked during replies (freed, and 1-4 pick a reply); the
  seated manager's crew presentation scripts fought her pose every frame (all off but the rig, look and face); the
  office played on its own did nothing (it previews both scenes, with its own subtitle document); the office's point
  lights cast shadows (six maps each a frame; now only the sun casts); the skip bar no longer restyles every frame.
- **Screen layouts:** `Art/UI/Resources/Remake2026Layouts.uss` after Remake2026: flat contract cards (no tilt), a
  two-column appraisal, the travel screen, the HUD pockets, compass and danger marks. `ScreenPanel` re-adds the theme
  layers in order.
- **Map chain:** not re-run yet (about 2.7 hours in the editor: KitSwap, CoplanarFix, remaster, MapSite). The Hospital
  CoplanarFix stall was already fixed on 2026-10-24. Run Rebuild All Maps (or `AvOverhaul.AfterMaps`) when the editor
  is free.
- **2026-10-28 (user: "get rid of the open world", "fix the hospital", "keep working on the cinematic"):** the open
  world is deleted (code, scenes, van, menus, hooks); the HQ no longer drops every session into it. The five newer maps
  are contract jobs now (`LocationBoard`; the Hospital opens at company level 1) and the Hospital, Motel, Police and
  Fire stations got a collection truck on their van dock (the world's van had been their truck). The frozen HR office
  was the character creator being cleared by the menu every frame (fixed in MenuUi); the HR manager was untextured
  (RemakeMaterials found the wrong texture folder); replies no longer get picked by the Enter that moved the last
  line on. A player who played before skips the interview and only sees the creator: to see the cinematic, reset the
  hiring (Maps > HQ > Reset Hiring on This Machine).
- **Architecture per map:** parked in Docs/remodelv2.md (user, 2026-10-28), then built there the same day ("What was
  built"): every map's box architecture rebuilt as fitted construction, lighting redone on top.

## Blocked

Things that failed twice and were skipped (CLAUDE.md's "fails twice, stop" became "log it and skip" for this run).

(none yet)

## The plan

As approved on 2026-10-27 (headings shifted down one level).



### Context

You want the whole game's look rebuilt using the Blender MCP and the Unity MCP. That covers the player character
(now a roster of characters), all 8 monsters, all loot items, the UI, every map's buildings and props, the 17 buyable
gear items, the collection truck and its 7 upgrades, and the wardrobe cosmetics. It also adds two new features: buyable **truck cosmetics**, and a
**first-launch hiring** (an HR interview, then the training, then "you're hired", then the character creator, then
the HQ). After that the game is HQ-centred and the Esc menu only resumes, opens Settings, leaves or quits.

What you decided:
- "PUs" means both the pickups (loot) and the UIs.
- Maps keep their current floor plans; their art is remodelled.
- All maps are in scope, including the five the other agents built.
- Models come from original Blender work and Poly Haven (CC0). AI generators are not used (your decision,
  2026-10-27).
- The style is grounded realistic.
- Players pick from 8 distinct characters, then dress them in outfits.
- The UI style is clean modern horror.
- I commit to main after each phase.

**Out of scope:** the open world (`Scripts/World`, `Editor/World`, `World.unity`, `WorldVanTest.unity`, its van) and
the TestMap (a dev map, not in the game). Gameplay rules, networking, balance and level layouts don't change. The one
exception is the truck-cosmetics feature.

**How I'll work:** I build continuously, phase by phase, without stopping for approval. I don't play, test or
screenshot-verify the game. I don't run EditMode/PlayMode/nettests or make player builds. Two things are part of
building, not checking:
1. Unity must compile so the editor builders can run.
2. I use Blender's `look` while modelling, to see the model I'm making.

If something blocks me, it goes on a "Blocked" list in `Docs/REMAKE_2026.md` and I move to the next item instead of
stopping. CLAUDE.md's "fails twice, stop" rule becomes "fails twice, log it and skip". You get the whole list at the
end.

---

### Before I start (your side)

1. **Stop the other agents.** That means ChatGPT (Hospital/Motel), the Claude desktop session (Police/Fire),
   Antigravity (Skyscraper) and the open-world CLI session. I'll be rewriting their maps' art and the shared files
   (`ToolMenu.cs`, `RebuildAllMaps.cs`, `TOOL_MENU.md`, `ASSET_CREDITS.md`). Two drivers on one editor corrupt
   scenes.
2. **Keep Unity (the `Game` project) and Blender 4.3.2 open** with both MCP servers running, and keep the Mac from
   sleeping (for example `caffeinate -dis` in a terminal).
3. ~~Switch on Poly Haven~~: done. `get_addon_status` shows Poly Haven on and every AI generator off.
4. **Skills:** you don't need to download anything. The `unity-mcp-skill` is already installed, and the Blender MCP's
   own instructions cover Blender. In Phase 0 I'll write a project skill (`.claude/skills/abandoned-remake/SKILL.md`).
   It holds this pipeline's conventions so they survive context summaries during the long run.
5. **The uncommitted `Prison.unity` change.** My first commit saves it as-is ("save current Prison scene before
   remake"). Tell me if you'd rather discard it.
6. **The monsters-off flag** (`ThreatDirector.ExploreWithoutMonsters = true`) stays on. I won't touch it.

---

### Shared conventions (apply to every phase)

- **Source of truth is code.** Every asset is a Python script under `Tools/Blender/remake/<area>/`. I run it in the
  live Blender through `execute_blender_code` and look at it with `look`. The same script can also be re-run in batch
  mode (`Blender --background --python ...`), so every asset can be regenerated. Poly Haven imports are pulled
  through scripts too. The cleaned result (scale, pivot, decimation, UVs, baked maps) is saved as a `.blend` plus FBX.
- **Reuse `meridian_core.py`, `crew_geo.py` and `crew_export.py`** for the axis and unit conversion (Unity `(x,y,z)`
  becomes Blender `(x,z,y)`), LOD export and nested-LOD `bake_space_transform=False`.
- **Output** goes to `Art/Remake/<Area>/<Asset>/`, keeping every `.meta` file. Texture sets follow the existing
  convention:
  - `_BaseColor` / `_Normal` / `_Mask` (R metallic, G occlusion, A smoothness).
  - 2048 px for hero assets, 1024 standard, 512 small.
  - Environment materials go on `Abandoned/Surface`.
- **Triangle budgets (LOD0, then LOD1 and LOD2 at about 40% and 12%):**

  | Asset | LOD0 budget |
  |---|---|
  | Character | 35k |
  | Outfit layer | 8k |
  | Monster | 25–60k |
  | First-person gear | 3–8k |
  | Truck | 120k |
  | Loot | 0.5–8k by size |
  | Architecture module | 0.2–4k |

  The mall's 12,500 render-budget limit stays.
- **Unity side:**
  - Explicit importer settings follow the `CustomArtImport` pattern (no import postprocessor that would rewrite kit
    meshes, per the CoplanarFix rule).
  - Models are fitted with `Editor/Art/ModelFit`.
  - Menu items use the `ToolMenu` constants and go where `Docs/TOOL_MENU.md` says, which I update in the same change.
  - Each new builder gets a line in `RebuildEntireGame.cs` or `RebuildAllMaps.cs`.
- **Long Unity operations:**
  - Each long call is guarded with a SessionState token.
  - Each logs a marker to `Editor.log` and writes its results under `Game/Logs/remake/`.
  - Waits are bounded with a deadline plus a Unity-CPU idle check.
  - I run one map per call (from memory: MCP retries timed-out calls, and `delayCall` never fires when unfocused).
- **Credits:** every Poly Haven asset gets a line in `Docs/ASSET_CREDITS.md` naming its source and
  licence.
- **Text:** all new text goes through `Loc.T` or `Loc.Mark`. Each phase ends with `python3 Tools/Text/check.py` at 0
  missing, with translations into all 9 languages added.
- **Commits:** one commit per phase on `main`, staging my paths only, never pushed. Each commit message ends with the
  Co-Authored-By line.

---

### Phase 0: Pipeline and art bible

- Write `Docs/REMAKE_2026.md`. It holds:
  - The art bible: palettes per map, wear language, texel density 512 px/m, budgets, naming.
  - The per-phase checklist and the Blocked list.
- Write `Tools/Blender/remake/core/`:
  - `export.py`: FBX with LODs and named anchor empties.
  - `materials.py`: the PBR setup and bake-to-Mask channel packing.
  - `polyhaven.py`: search and import, normalise scale and pivot, rebake to our maps.
  - `sculpt.py`: code-driven sculpting (multires, displacement from noise and curvature masks, then bake to normal
    and AO). It gives faces, skin and monster flesh realistic detail without an AI generator.
  - `rig.py`: the shared crew armature and the monster armature helpers.
  - `fracture.py`: intact, cracked and broken stages plus chunk siblings, as `meridian_surfaces.py` does now.
- Unity: create `Editor/Remake/` with:
  - `RemakeImport`: importer settings per folder.
  - `RemakePaths`.
  - `RemakeMenu` entries under Content/Art, per TOOL_MENU.
- Write the project skill `.claude/skills/abandoned-remake/SKILL.md` (the conventions above).

### Phase 1: Player characters and the wardrobe

**8 workers.** Men and women, with different ages, builds, skin tones, faces and hair. All of them keep the 1.78 m
eye-height rules in `PlayerMovementConfig`. One way to get there is a single height with build variety, so that
first-person eye height, ragdolls and collision don't change. Faces build on the current
`Tools/Blender/crew/crew_head.py` head. Each character gets its own proportions (jaw, brow, nose, cheekbones, ears,
age lines), sculpted detail baked to normal maps, and painted skin, eye and hair textures. Hair and beards are
hair cards. Honest note: realistic faces without AI generation or scans are the hardest part of this remake.
They'll be clearly different people, but don't expect film-quality faces.

**Skinned, on the existing rig:**
- One Blender armature whose bones are named and placed like the `WorkerRig` joints: hips, spine, chest, neck, head,
  shoulders, the new elbows, knees.
- Each character and each outfit is exported as a skinned mesh on that armature.
- `Editor/Art/WorkerVisualBuilder` binds each `SkinnedMeshRenderer`'s bones to the rig transforms that
  `WorkerPresentation` already rotates.
- Animation code, carrying, the ragdoll (`PlayerRagdollBuilder`) and the first-person view keep working unchanged.
  Bodies now bend smoothly instead of being rigid parts.

**Outfits:** about 10 full outfits (tactical salvage, coveralls, hazmat, winter parka, firefighter turnout, scrubs,
mechanic, rain gear, and so on). The old coverall colours become outfit tints, so existing saves still map.
First-person gloves and sleeves follow the outfit.

**Hats and accessories:** all 15 hats and 12 accessories are remodelled, plus about 8 new ones. Each character has
head and body sockets (crown, eyes, mouth, back, belt) exported as empties. These replace `CosmeticsBuilder.Fit`'s
one-size refit.

**Data (append-only):**
- `CosmeticKind.Character` and `CosmeticKind.Outfit` are appended.
- `CosmeticChoice` gains two bytes. It stays owner-written, like the current `NetworkVariable` in
  `Scripts/Player/Cosmetics/PlayerCosmetics.cs`.
- `PlayerProfile` stores the two new choices.
- `CosmeticDefinition` gains `CharacterPrefab`, `OutfitPrefab` and per-character sockets.
- New entries are appended to `CosmeticCatalog` (the byte index on the wire stays stable).

**Wardrobe screen:** a Character tab using the existing `WardrobePreview` mannequin camera.

**Files:**
- `Tools/Blender/remake/crew/*`
- `Editor/Cosmetics/CosmeticsBuilder.cs`
- `Editor/Art/WorkerVisualBuilder.cs`, `WorkerViewModelBuilder.cs`
- `Editor/PlayerPrefabBuilder.cs`
- `Scripts/Player/Cosmetics/*`
- `Data/Player/Cosmetics/`

### Phase 2: Monsters (all 8)

The monsters are the Blind One, Stalker, Collector, Hunter, Last Hunter, Crawlers, the Thing and the Weight. Each one
keeps its identity and readable silhouette, because the gameplay depends on recognising them: the eyeless listener,
the suspended coat, the hunched collector, the broad stone hunter. Each is redesigned in grounded realistic horror.

**Process:**
1. The base sculpt comes from Blender code (`sculpt.py`), with Poly Haven textures (rock, bark, skin-like
   organics) where they fit.
2. Retopology to budget.
3. Rigged with `rig.py`, auto-weighted.
4. Hand-keyed takes with the same names and lengths: Idle, Walk, Chase, Attack, Special. This lets
   `ThreatAnimationSync` and `ThreatPresentation` work unchanged.

**What must not change:**
- The NavMesh agent radius and height, attack reach, the Weight's 450 kg load and the Hunter's door-breach timing.
  Each model is fitted to its current bounds.
- The Crawlers' arachnophobia alternative (`ArachnophobiaSwap`).
- `ThreatGrime`, which gets a new texture set.

**Pipeline:** the new scripts in `Tools/Blender/remake/threats/` replace `threats*.py`. The output replaces the FBX
files in `Art/Custom/Threats/`, keeping their GUIDs, so prefabs re-link. Then `Editor/Art/ThreatModelImporter` and
`ThreatRigBuilder` rebuild the threat prefabs (`ThreatVisualBuilder` was unused and removed on 2026-10-28).

### Phase 3: Gear (17 items) and the shop

**The items:** battery, bolt cutters, crowbar, drone, flashlight, glow sticks, hand trolley, lockpick, medkit, motion
detector, night vision, noise maker, planks, radio, sledgehammer, stress scanner, thermal camera. The gear cut on
2026-10-22 stays cut.

**Models:** each item gets a first-person model and a world/rack model, with anchors for the grip, the light origin
(flashlight) and screens (scanner, thermal, detector).

**Data:** a new `GearModels` ScriptableObject in `Data/Equipment/Resources` maps `EquipmentKind` to its models and
grip offsets. `Scripts/Equipment/ToolModels*.cs` and `ToolArtBuilder` use it first and fall back to the current
code-built mesh if an entry is empty. Adding gear still needs no new code.

**Network prefabs** (`Prefabs/Equipment/Drone`, `GlowStick`, `NoiseMaker`) get the new visuals. The HQ gear rack and
shop terminal get remodelled too.

### Phase 4: Truck, upgrades, van, and **truck cosmetics** (new feature)

**The collection truck** is remodelled in Blender as one hero model built from parts: cab, cargo box, ramp, rear
doors, wheels, lights. Every functional point is a named empty: cargo trigger, deck at 1.05 m, ramp hinge (4.4 m run),
ignition, haul display, floodlights, rear-view camera, boat-job attach. The current `Editor/Truck/CollectionTruck*.cs`
builders keep their components but place them on those anchors. Cargo dimensions and capacity don't change. The
arrival van gets the same treatment, and the Prison boat gets a remodelled hull.

**Visible upgrades.** `TruckUpgradeDefinition` gets a `VisualPrefab` field:

| Upgrade | What appears on the truck |
|---|---|
| cargo_1 | Side racks |
| cargo_2 | Extended box |
| engine_1 | Exhaust stack |
| engine_2 | Hood scoop and intercooler |
| floodlights | Roof light bar |
| forecast | Weather mast and radar dish |
| winch | Front winch |

### Truck cosmetics

| Part | What it is |
|---|---|
| **Slots** | A new `TruckCosmeticSlot` enum, append-only: Paint, Livery, Bumper, Roof, Wheels, Mudflaps, Ornament, Horn. |
| **Catalogue** | About 40 items: 12 paints in matte, gloss and patina finishes; wraps such as hazard stripes, flames, camo, racing stripes and grime art; bull bar, push bar and plough; beacon racks; rims; bobblehead and hood ornaments; horn sounds. |
| **Data** | `TruckCosmeticDefinition` (Id, name, description, slot, price, unlock level, prefab or material or livery texture, horn `SoundId`) in an append-only `TruckCosmeticCatalog` in `Data/Company/TruckCosmetics/`. A new item needs no code. |
| **Ownership and save** | Owned items go in `CompanySave.unlocks` as `truckcos:<id>`. A new `List<string> truckLook` field holds what's equipped and defaults to empty, so old saves load as the stock truck. |
| **Authority** | New `CompanyService.TruckCosmetics.cs` partial. A client asks by RPC; the host checks money, `CrewRules` and unlock level, charges through `CompanyService.TryCharge`, then saves. |
| **Sync** | `CompanyNetState` gets one host-written byte per slot. |
| **Showing it** | A `TruckLook` component on every truck (each map plus the HQ display truck) swaps parts and sets paint and livery through material variants and a livery UV channel. Horn variants are new `SoundId`s added to `Audio/SoundCatalog` and the SoundBank. |
| **UI** | A "Truck Garage" screen at the HQ: a new interactable by the garage door, a 3D preview camera like `WardrobePreview`, tabs per slot, buy and equip. Prices go in `Docs/BALANCE.md`. |

Truck-upgrade unlocks keep working as they do now (`CompanyService.TruckUpgrades.cs`).

### Phase 5: Pickups (all 239 loot items)

**Families.** Items are grouped by family (electronics, furniture, antiques and art, industrial, medical, police,
fire, kitchen, toys, jewellery and cash, school, prison, factory, and so on). Each family gets a shared material and
wear set. Where each model comes from:
- a matching Poly Haven CC0 model if one exists;
- hand-built Blender models for jackpots and hero items;
- Blender family templates for everything else.

**What doesn't change.** The `LootDefinition` Size box, gameplay weight, Rigidbody mass and colliders all stay.
`LootModelBuilder` gets a "keep authored size" mode that fits the model into the box rather than resizing the box, so
spawn points and shelves stay valid. Then `LootPrefabGenerator` regenerates the prefabs in place, which keeps GUIDs
and network IDs.

**Look-alikes.** The `LootLookalikes` rule (0.24.9) runs again so that no scenery uses a loot model.

### Phase 6: Maps (layouts kept, art remodelled)

**Order:** HQ, Mall, School, Iron Factory, Island Prison, Tutorial House and street, Hospital, Motel, Police Station,
Fire Station, Skyscraper (with the Apex restaurant and lobby).

**Per map:**
1. **Inventory.** Through the Unity MCP, list the saved scene's renderers by role (wall run, slab, stair, window,
   door frame, facade, roof, column, railing, ceiling, fixture, prop family) and their sizes. The result goes to
   `Game/Logs/remake/<map>_inventory.json`.
2. **Kit.** A shared architecture core in `Tools/Blender/remake/arch/` plus a style pack per map. The packs are
   modular walls in standard lengths with trims, windows, doors, columns, stairs, slabs with fracture stages, facade
   pieces (cornices, setbacks, signage frames without text) and roofs. On top of that, the map's prop families are
   remodelled.
3. **Swap.** A new `Editor/Remake/KitSwap` pass is modelled on `MapSite/MapTrees`. It matches each piece by name,
   role and bounds, places the fitted kit model, and switches the old renderer off.
   - Colliders, `StructuralSection`s, doors, lockers, spawns and gameplay components stay as they are.
   - Every swap is recorded in a `KitSwapRecord`, so the pass can be re-run or undone.
   - Structural slabs point their crack and break stages at the kit's fractured variants.
   - Because colliders don't move, **no NavMesh or occlusion rebake** is needed.
4. **Chain.** KitSwap runs inside `AvOverhaul.AfterMaps`, before `MaterialUpgrade` and `CoplanarFix`. Then the
   remaster, lighting rebuild and MapSite passes run. One map per MCP call. `GraphicsAudit`'s "can flicker" count
   should stay near 0.
5. **HQ.** The pass works on the saved scene, so your hand edits survive. There's no HQ regeneration.

**Note:** the five newer maps also stream into the open world, so their new look will show there too. No open-world
code changes.

### Phase 7: The hiring (first launch) and an HQ-centred game

This is how the game moves forward from now on: everything happens in the world. The Esc menu only resumes, opens
Settings, leaves or quits.

### First launch, step by step

1. **Interview.** The player opens the game and lands in a new `HrOffice` scene: the HR office at Abandoned
   Salvaging's head office. The HR manager is hiring them and explains: "This is your training. Do well and you're
   hired."
2. **Training.** Straight into the Tutorial House, solo, no HQ in between. The existing 46 lessons and `RunTraining`
   run unchanged.
3. **Back to HR.** When training finishes, the player returns to the HR office. "You did well, you're hired." A line
   or two reacts to how the training went (time, items damaged, falls).
4. **Character creator.** Full screen: pick one of the 8 workers, an outfit, colours, a hat and an accessory, shown in
   a 3D preview. It writes `PlayerProfile`.
5. **HQ.** A new company save is created, and `FirstDayGuide` takes over as it does today.

### How the HR scene plays

- **About 70% first person, seated across the desk.** You can look around within limits. The manager is voiced and
  the player picks short replies. The replies change a line or two, never the outcome.
- **About 30% cinematic.** The camera cuts to set shots at key beats: walking in, over the shoulder while signing
  the contract, the handshake, "you're hired", the drive away.
- **Built with Timeline + Cinemachine.** Both are already in `manifest.json` (`com.unity.timeline` 1.8.13,
  Cinemachine 3.1.7), so no new package. The first-person view is its own Cinemachine camera. Timeline handles
  blends and cuts. Dialogue is driven by Signals.
- **The dialogue is data.** `DialogueDefinition` holds lines, speaker, voice clip, choices, the shot to cut to and
  wait times. A new line or shot needs no code. `Scripts/Onboarding/CutsceneDirector` plays it.
- **The HR manager** is a new NPC built with the Phase 1 character pipeline. They have blinks, eye look-at and a jaw
  driven by the voice clip's loudness.
- **The voice** is spoken with Kokoro-82M (the project rule) through `Tools/Audio`, the same way as the dispatch
  voices.
- **The office** is a small Blender-built map, `Editor/Onboarding/HrOfficeBuilder`. Its menu path goes in
  `Docs/TOOL_MENU.md` with a line in `RebuildAllMaps.cs`.

### Rules

- **Skipping.** Holding a key skips HR dialogue and cinematic shots. Training can't be skipped the first time.
- **Progress** is a local profile stage: New, Interviewed, Trained, Hired. If the player quits partway, the next
  launch resumes at that stage. It's local per player, like the wardrobe, never in the company save.
- **Existing saves.** A machine that already has a company save is marked Hired, but sees the character creator
  once.
- **A first-time player accepts a Steam invite.** The invite's lobby is remembered in `SteamLobbyFlow`. They go
  through HR, training and the creator solo, then join that lobby automatically if it's still open. Otherwise they
  go to their own HQ with a toast.
- **Onboarding is always offline and solo.** It reuses the tutorial's solo host, so nothing is networked between
  machines.

### Later launches

A short title screen: Continue, Save slot, Settings, Credits, Quit. It loads straight into your HQ, where you are
always the host. Friends join only through Steam invites sent from inside the HQ.
- `MainMenuView` and `PlayView` are trimmed to that.
- The Training card moves out; the HQ's training clipboard already replays training.
- Dev builds keep `-host`/`-connect` and the Unity Transport join for nettests.

### Everything moves into the HQ

`PauseMenuView` keeps only **Resume, Settings, Leave game and Quit to desktop**. Leave and Quit stay because a player
must always be able to get out. Everything else moves into an HQ station:

| Was in the pause menu | Now |
|---|---|
| Invite friends (Steam overlay), crew list, kick, host crew rules | **Crew phone / crew board** in the dispatch office (new) |
| Per-player voice volume and mute | **Settings > Voice** (they must work during jobs too) and the crew board |
| Achievements | **Trophy cabinet** in the break room (new) |
| How to play / controls | **Notice board** (new), plus the existing training clipboard |
| Report a problem | Title screen and Settings |

Already in the HQ: the contracts telephone, field guide, wardrobe locker (now the character locker), gear rack and
shop terminal, the Truck Garage from Phase 4, and the training clipboard.

Each new station is an interactable added to the saved HQ scene by an Editor step, so your hand edits survive.

**Files:**
- `Scripts/Onboarding/*` (new `Abandoned.Onboarding` area; add it to CLAUDE.md's namespace list)
- `Editor/Onboarding/*`
- `Data/Onboarding/` (dialogue assets)
- `Scenes/HrOffice.unity` (added to `BuildScenes.All`)
- `Scripts/UI/Menus/{PauseMenuView,MainMenuView,PlayView}.cs`
- `Scripts/Tutorial/TutorialLaunch.cs` (the new entry from onboarding, and the end hook back to HR)
- `Scripts/Networking/Lobby/SteamLobbyFlow.cs` (the deferred invite)
- `Scripts/Player/Cosmetics/PlayerProfile.cs` (the onboarding stage)
- New HQ station scripts in `Scripts/Company/`

### Phase 8: UI redesign (clean modern horror)

- Evolve the bone, rust and amber look into a sharper layout. That means a new USS theme in `Art/UI/Resources/` and
  `UiThemeAssets.Apply`'s order kept.
- Screens: the main menu gets a live 3D backdrop (the crew and truck in the HQ yard). Also redone: the HUD (vitals,
  compass, pockets), contract board, shop, wardrobe, Truck Garage, results and receipt, pause, settings and the travel
  screen.
- 3D item icons. A new `Editor/Remake/IconRenderer` renders every gear, loot, cosmetic, truck-cosmetic and character
  model to `Art/UI/Icons/`, and the UI uses them.
- Rules I keep:
  - `MenuKit.Page`.
  - The `if (x == null || x.panel == null) Build();` pattern in every HUD piece.
  - MenuKit buttons never get child elements.
  - Everything goes through `Loc.T`, and host text stays English.
  - Fonts are OFL or CC0 only.

### Phase 9: Integration and handover

- `RebuildEntireGame.cs` order: characters and the HR manager, monsters, gear, truck, loot, maps (with KitSwap in
  `AfterMaps`) plus the HR office, HQ stations, icons, UI.
- `TOOL_MENU.md`, `ASSET_CREDITS.md`, `BALANCE.md` and `Docs/REMAKE_2026.md` get updated, plus the CLAUDE.md
  milestone and decisions log.
- Final `check.py` at 0 missing, and the last commit.
- A report for you: what's done, what's blocked, and what to check first.

---

### Files I'll change most (representative)

- **New:**
  - `Tools/Blender/remake/**`
  - `Editor/Remake/**` (`RemakeImport`, `KitSwap`, `KitSwapRecord`, `IconRenderer`)
  - `Scripts/Company/TruckCosmetic*.cs`, `CompanyService.TruckCosmetics.cs`
  - `Scripts/Company/TruckLook.cs`
  - `Scripts/Equipment/GearModels.cs`
  - `Data/Company/TruckCosmetics/`
  - `Art/Remake/**`
- **Edited:**
  - `Editor/Art/WorkerVisualBuilder.cs`, `Editor/Cosmetics/CosmeticsBuilder.cs`, `Editor/PlayerPrefabBuilder.cs`
  - `Scripts/Player/Cosmetics/*`
  - `Editor/Art/Threat*Builder.cs`
  - `Scripts/Equipment/ToolModels*.cs`
  - `Editor/Truck/*`
  - `Scripts/Company/{CompanySave,CompanyNetState,TruckUpgradeDefinition}.cs`
  - `Editor/Art/LootModelBuilder.cs`
  - `Editor/Art/AvOverhaul.cs`
  - `Editor/RebuildEntireGame.cs`, `Editor/RebuildAllMaps.cs`, `Editor/ToolMenu.cs`
  - UI scripts and USS
- **Untouched:**
  - Open world: `Scripts/World`, `Editor/World`, `World.unity`, `WorldVanTest.unity`
  - TestMap
  - `Game/Library`, `Game/Packages`
  - Gameplay tunables

### Checking (yours, after I finish)

I won't run any of this. When I finish, the report gives the order to check things:
1. Compile.
2. Rebuild Entire Game (keep HQ).
3. A fresh profile: HR interview (first person plus cinematic cuts), training, back to HR, character creator, HQ.
   Then accept an invite as a new player.
4. Walk HQ: the crew board (invite, kick, rules), trophy cabinet, notice board, the cut-down Esc menu, character select and wardrobe, Truck Garage buy, equip and preview, gear rack.
5. Play one Mall job: carry loot, use the gear, the truck with upgrades, then collapse a floor (fracture stages).
6. A monster encounter for each of the 8.
7. The other maps.
8. `Tools/unity.sh all` for EditMode/PlayMode, and `nettest` for cosmetic and truck-look sync with 4 players.
9. The Checks menu audits (Audit All Levels, MallValidation lines, GraphicsAudit flicker count, render budget).

**Expect this to run long.** Characters, monsters and loot alone are hundreds of assets. A full map chain took about
2.7 hours last time. That's why each phase is committed and usable on its own.
