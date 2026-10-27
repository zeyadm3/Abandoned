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
| 2 Monsters | |
| 3 Gear and shop | |
| 4 Truck, upgrades, van, truck cosmetics | |
| 5 Loot | |
| 6 Maps | |
| 7 The hiring and the HQ-centred game | |
| 8 UI | |
| 9 Integration and handover | |

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
files in `Art/Custom/Threats/`, keeping their GUIDs, so prefabs re-link. Then `Editor/Art/ThreatModelImporter`,
`ThreatRigBuilder` and `ThreatVisualBuilder` rebuild the threat prefabs.

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
