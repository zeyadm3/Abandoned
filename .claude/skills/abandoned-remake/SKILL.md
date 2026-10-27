---
name: abandoned-remake
description: Conventions for the ABANDONED 2026 remake (Blender MCP + Unity MCP). Use before modelling, baking, exporting or importing any remake asset (characters, monsters, gear, truck, loot, map kits, onboarding), or when wiring remake art into Unity builders.
---

# ABANDONED 2026 remake

Read `Docs/REMAKE_2026.md` (art bible, progress, Blocked list) and the plan at its end before starting a phase.
Rules that hold for the whole run (user, 2026-10-27):

- **Build continuously, never test, verify or run the game.** Compiling so editor builders can run is part of
  building, and so is `look` in Blender while modelling. No Play mode, EditMode/PlayMode tests, nettests or
  player builds.
- **Fails twice → log it under "Blocked" in Docs/REMAKE_2026.md and move on.** Don't stop.
- **No AI generators.** Original Blender modelling plus Poly Haven (CC0, credited in Docs/ASSET_CREDITS.md).
- **Grounded realistic.** Worn, used, believable. Monsters keep their readable silhouettes and gameplay bounds.
- **Don't touch the open world** (`Scripts/World`, `Editor/World`, `World.unity`, `WorldVanTest.unity`) or the TestMap.
- **Gameplay, networking, balance and layouts stay**, except the truck-cosmetics feature and the hiring flow.
- **Commit once per phase to `main`**, staging only remake paths; never push.

## Blender

- Sources: `Tools/Blender/remake/` (`core/` helpers, one folder per area). Every asset is a script.
- Axes: build facing **+Y**, +Z up, +X right, metres. `core/export.py` writes Unity space (bake_space_transform for
  static models; armatures keep transforms, Unity bakes the axis conversion).
- **Batch is the build path** (it can read and write `~/Documents`):
  `/Applications/Blender.app/Contents/MacOS/Blender --background --factory-startup --python <script> -- <args>`
- **The live Blender can't read `~/Documents`** (macOS privacy). Run `Tools/Blender/remake/sync_live.sh`, then in
  the MCP: `exec(open(os.path.expanduser('~/Library/Caches/AbandonedRemake/src/remake/boot.py')).read())`.
  Batch scripts call `preview.save(name)`; live calls `preview.show(name)` then `look`.
- Never use `bpy.ops.wm.read_factory_settings` in the live Blender (it unloads the MCP add-on); `kit.reset()` is safe.
- Join with `kit.join` (it takes materials from the original objects; evaluated copies crash Blender).
- Bake every non-shared surface into one set per asset with `bake.bake_asset(ob, name, <area>/Textures)`; the
  final material is `RM_<name>`. Shared surfaces (Glass, Emit*, Screen, HairCard, Decal) keep `RM_<Shared>`.
- Output: `Game/Assets/_Project/Art/Remake/<Area>/` (FBX) and `<Area>/Textures/` (PNG). Maps: `Maps/<Map>/`.
- LODs: `export.make_lods` names meshes `<Name>_LOD0/1/2` (Unity builds the LODGroup). Anchors are empties named
  `Anchor_*`, sockets `Socket_*`.
- Crew skeleton: `core/rig.CREW` matches WorkerRig's joints by name and position (Body, Torso, Head, ArmL/R,
  ElbowL/R, LegL/R, KneeL/R; eyes at 1.65 m).

## Unity

- `Editor/Remake/RemakeImport` handles import settings for `Art/Remake/**`; `RemakeMaterials.GetOrCreate` makes
  URP Lit materials from the sets; menu `Tools > Abandoned > Art > Remake: Refresh Materials`.
- Menu items: `ToolMenu` constants only, placed per `Docs/TOOL_MENU.md`, which is updated in the same change.
- Long MCP calls: a SessionState token per call, a log marker, results in `Game/Logs/remake/`, bounded waits
  (deadline + Unity CPU idle). One map per call.
- Never hand-edit `.unity`/`.prefab` YAML; use editor scripts. Keep `.meta` files.
- New text: `Loc.T` / `Loc.Mark`, then `python3 Tools/Text/check.py` at 0 missing (all nine languages).
- Append-only enums and catalogs (cosmetics, truck upgrades, truck cosmetics, sound ids).
