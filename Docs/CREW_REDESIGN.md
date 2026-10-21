# Crew redesign (2026-10-22)

The player character is replaced. The user asked for a complete redesign, rejected the first stylized pass as
not realistic enough, then chose: **tactical salvage gear**, a **realistic visible face**, a **stocky labourer**
build. Preview renders: `Docs/renders/crew-redesign/` (the rejected stylized pass is in `v1-stylized/`).

## The character
- About 1.78 m, eyes at 1.65 m (`PlayerMovementConfig.StandingEyeHeight`), real proportions: 15.5 cm wide head,
  thick neck and arms, a bit of a gut.
- Team-coloured ripstop jacket (the coverall colour tints it): reflective bands on both arms, charcoal elbow
  patches, collar and cuffs, a shoulder pocket on each arm with the crew number.
- Wolf-grey plate-carrier vest: front and back plates, cummerbund with webbing rows, padded shoulder straps with
  reflective stripes, radio, utility and flashlight pouches, a green chem light, a drag handle, the crew number on
  the chest ID patch and big on the back panel.
- Khaki cargo trousers bloused into laced leather work boots, hard knee pads, a belt with a pouch and a claw hammer.
- Realistic face with eyelids, age lines, crew cut and a short beard (painted into a baked face texture).
- About 31k triangles for the whole worker.

## Where it lives
- Source: `Tools/Blender/crew/` (see its README). `crew_export.py` writes
  `Art/Custom/Characters/Crew/Crew_<Part>.fbx` (one per joint, one mesh per material key), `Crew_GloveL/R.fbx` for
  first person, and the baked `CrewHead_BaseColor.png` / `CrewEye_BaseColor.png`.
- `Editor/Art/WorkerVisualBuilder` places the parts on the rig and assigns materials by key (new variants
  `CrewVest`, `CrewWebbing`, `CrewTrousers`, `CrewGlove`, `CrewGlovePalm`, `CrewBoot`, `CrewLace`,
  `CrewChemLight`, `CrewHead`, `CrewEye` in `Art/Characters/Materials/Variants`).
- `WorkerClothingDetails` and `WorkerHeadDetails` (the old code-built worker) are deleted.

## What changed in the game
- Rig: shoulders at (+-0.19, 1.415) (were +-0.28, 1.39), head pivot at 1.52 with the crown anchor at 1.77,
  hips at (+-0.102, 0.90), knees 0.40 below the hips, and a new **elbow** on each arm (`WorkerRig.LeftElbow` /
  `RightElbow`, optional so old saved workers still work).
- `WorkerPresentation`: arms hang 9 degrees out, forearms bend 10 degrees at rest; carrying is arms -40 / elbows
  -40 (was arms -52); holding gear is arm -35 / elbow -45 and the tool sits in the forearm's hand; point
  straightens the elbow, wave and panic bend it; crouch lowers the body 0.63 m (was 0.6).
- Ragdoll parts are resized to the new body; each arm body carries the upper arm and the forearm.
- First person: the new glove model, the sleeve cuff in charcoal.
- Wardrobe: every hat and accessory gets a `Fit` node (`CosmeticsBuilder.Fit`) that refits the old kit to the
  new head and body: head pieces narrowed and flattened, face pieces at the new eye and mouth heights, the tool
  belt, rucksack, dog tags and radio moved onto the new belt, vest and back. The kit models themselves are unchanged.

## Rebuild
`Tools/unity.sh exec Abandoned.EditorTools.CrewRedesign.AuthorAndExit` (Build Cosmetics, then Create Player
Prefab). After changing the Blender source: run `crew_export.py` first (README), then the same command.

## Not checked
Built and saved in batch mode only; not played. Check first: the coverall colour on the jacket, crew numbers
(size and facing), the wardrobe fit of every hat and accessory, carrying and holding gear with the new elbows,
crouching (feet on the floor), the ragdoll, and the first-person gloves.
