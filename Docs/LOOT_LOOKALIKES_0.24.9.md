# 0.24.9 — No loot look-alikes

## The bug

Playing co-op, the user and a friend found pairs of identical items side by side, for example two chairs, where
only one could be picked up. The second one was never loot: it was **scenery built from the same model**.

- 0.15.5 authored every salvage family twice from one model: a loot prefab and a static dressing prefab. The
  dressing passes (`SalvageMallDressing`, the 0.15.5b density layer) placed the static copies throughout the
  stores, a few metres from loot pads asking for the same item (the Ergonomic Chair: `SalvageMallDressing` and
  `SalvageRetailDensity` placed static ones, `SalvageMallLootPoints` spawns a real one in the same room).
- 0.15.7 promoted 16 of the 0.15.6 scenery families (sofa, office printer, cash register, headphone cartons…) to
  loot and left all their scenery copies standing.

About 50 loot models were scenery somewhere, in roughly 225 placement lines. Only the Mall: School, Factory and
Prison dress with plain boxes, and the Kenney furniture models are only ever loot.

## The rule

**If it looks like loot, it is loot.** A loot model never stands in a level as scenery.
`Editor/Salvage/LootLookalikes` is the list (every `SalvageAssetCatalog` loot spec plus `SalvagePickables0157`,
so a new loot family is covered without touching this code).

## What takes the place of a look-alike (user's choice: option C, fill the gaps)

| Original | Becomes |
| --- | --- |
| Modular shelving unit | **Fixed gondola shelving**: kick plinth, solid back panel, bolted rear uprights, price rails, header. Same shelf heights, so the stock on it still sits right. |
| Garment rack | **Fixed garment rail**: floor fixing rail, square posts on bolted base plates, header bar. No casters. |
| Cash-wrap counter | **Fixed service counter**: stone top, painted panels, recessed plinth, power conduit into the floor. No cash drawer. |
| Wooden shipping pallet | **Plastic pallet**: one-piece moulded deck, same 1.24 × 0.146 × 1.0 m. |
| Industrial dumpster | **Waste compactor**: anchored base frame, ram housing, hydraulic cylinder, control box, conduit. |
| Anything else standing on the floor | A **dust sheet** draped over the item's exact bounds (canvas, rounded shoulders, a folded skirt). |
| Stock on a shelf, a pallet or in a display | **Crushed empty packaging**: flattened carton, torn flap, shrink-wrap scrap. Walk-through. |

The user chose to keep the five fixture items as loot and build fixed versions rather than stop them being loot.
The loot items themselves, their values, spawns and prefabs are unchanged.

## How the saved mall was changed (in place)

`Tools > Abandoned > Mall > Replace Loot Look-alikes (0.24.9)` (batch: `Tools/unity.sh exec
Abandoned.EditorTools.Release0249.AuthorAndExit`):

1. Authors the five fixtures (`SalvageAssetBuilder.AuthorScenery`, recipes in `SalvageFixtureModels`); loot
   prefabs are not regenerated.
2. **Prefab instances** (the 0.15.5 dressing) are swapped directly. Instances with no section binding of their
   own were `Inside()` placements (display contents, the tipped chair) and become packaging.
3. **Batched layers** (`_SALVAGE_DENSITY`, `_SALVAGE_EXPANSION_0156`): the batcher merged props into per-slab
   meshes and kept an empty holder per prop. For each look-alike holder the pass re-creates the original geometry
   exactly as the batcher saw it and cuts **those triangles** out of the batch (`BatchTriangleSet`, matched by
   position within 2 mm, never by bounds, so stock on a shelf or a touching neighbour keeps its triangles). Then
   the holder is swapped.
4. **Colliders are kept**: each stand-in carries the original's colliders unchanged, so collision and the baked
   NavMesh stay valid. The NavMesh is deliberately **not** rebaked: no release has rebaked it since the 0.16.0
   doors and lockers went in, and a rebake now would bake them in as walls.
5. Storey light layers and the shadow-caster rule are re-applied for the new renderers. Occlusion is unaffected
   (stand-ins are not static).

Nothing else in the scene moves. Future full mall rebuilds apply the same rule in the placement helpers
(`SalvageDressingPlacement.Model/Inside`, `SalvageDensityPlacement.Model`).

## Checks

- Rebuild line: `[MallValidation] Mall: static loot look-alikes: N` must stay **0** (`LootLookalikeAudit`).
  The 0.24.9 step fails if any are left.
- `[Lookalikes0249] Batch geometry not found …` warnings name any batch where a prop's triangles couldn't be
  matched (the old prop may still show there).
- **F1** in play: `LOOT LOOK-ALIKES: N swapped · 0 left` under the mall stock line; with gizmos on, an amber box
  marks every swap.

## Verification (2026-10-18)

- `Editor/Probes/LookalikeVerify0249` (batch, changes nothing; needs the committed scene and meshes in
  `Assets/_VerifyTemp0249`, see its summary): every cut batch mesh compared with its committed copy.
  **491 batched look-alikes in 125 batches (2,267 meshes): 341,976 triangles removed, 0 not a look-alike's,
  0 look-alike triangles left, 0 meshes misaligned.** Saved scene: 185 dust sheets + 350 packaging + 60 fixtures
  = 595, all on a slab, no missing mesh or material, no solid packaging, every sheet keeps collision, audit 0,
  F1 report 595, no object named as a loot id. The probe left Mall.unity byte-identical.
- The copied root colliders of 0.15.6 props first kept the prop's name (94 of them); they are now
  `Collision_<id>` (`LootLookalikeStandIns.ProxyName`), renamed in place by re-running the step, which swapped 0.
- No other scene has a loot model as scenery: no prefab instance of a loot model or look-alike prefab, and no
  static renderer using a loot model's mesh, in Mall, HQ, School, Factory, Prison or TestMap.
- EditMode: the 5 new `LootLookalikeTests` pass (dust sheet faces outward, the triangle cutter keeps a
  neighbour's quad, every loot family is a look-alike, fixtures are scenery-only, collider names). 286 total,
  283 passed; the 3 failures are older and untouched by this change: InputBindingsTests (HUD prompt "G | "),
  LootDataTests (Loot_shopping_cart, 0.18.4, is Heavy under 50 kg), SoundBankTests (no Water light impact).
- Close-up renders checked: sheets, gondolas, rails, plastic pallets, compactor and packaging look right. The
  Vogue Thread "OverturnedCashWrap" wreck keeps its counter on its side, as the 0.15.5 dressing placed it: fixtures
  keep the original pose exactly.

## Try it (about 3 minutes)

1. Open `Scenes/Mall.unity`, press Play, press **F1**: the look-alike line should read `0 left`.
2. Nordic Habitat furniture floors (front right, both storeys): every chair, sofa, desk and lamp you see is
   either grabbable or under a dust sheet.
3. Apex Circuit (electronics): shelves are the fixed gondolas; anything on them that looks like a product is loot.
4. Vogue Thread and the clearance staging area: rails, counter, pallets and the compactor are fixed; nothing that
   looks like a pickup refuses to lift.
