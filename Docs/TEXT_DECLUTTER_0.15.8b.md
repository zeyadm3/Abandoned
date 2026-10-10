# 0.15.8b.EEA — decorative world-text removal

The user still found the mall and collection truck covered in bloated, unnecessary lettering after
the 0.15.8 branding pass. That pass retained too many salvage status labels and prop captions;
unfitted font-size and glyph metrics also allowed physical lettering to overflow its intended sign
faces. This follow-up removes those sources and fits the few essential directions to their plaques.

## Retained artwork and directions

All ten original generated logo and campaign PNGs and their 39 scene placements remain. Store
fascias, window offers, interior advertisements and hanging sale banners continue to provide the
retail identities and campaign copy. Both Nordic Habitat floors retain their themed artwork.

Only essential physical directions remain: EXIT, SERVICE, SERVICE STAIRS and COLLECTION. Their
world lettering is fitted to the physical plaque bounds rather than relying on an arbitrary font
size. Functional UI is preserved, including the truck haul/cargo display, pickup HUD, interaction
prompts and menus. Pickup definitions, values, extraction, threats and layout are unchanged.

## Removed lettering and regeneration policy

Decorative TextMesh lettering is removed from the collection truck and its generation source.
Mall operation-status labels, stock captions, kiosk labels and plaintext room identities are
removed, including lettering nested in original model and loot prefabs. The corresponding status
and truck text generation is disabled so a content rebuild does not restore the clutter.

This supersedes the 0.15.8 policy that retained physical recovery/hazard statuses, fragile/power
labels and truck company markings. The generated advertising artwork remains part of 0.15.8b.

## Authoring and delivery status

Menu: **Tools > Abandoned > Mall > Remove Decorative Text (0.15.8b)**.
Batch entry: `Abandoned.EditorTools.MallBrandingRelease0158.DeclutterAndBuildPlayersAndExit`.

The scene and updated occlusion data are saved through Unity. The cleanup removed 115 decorative
TextMesh objects (including previously disabled caption objects) and six source-library labels,
and fitted the remaining mounted exit sign. All generated image artwork remains.

Shareable Mac (441 MB) and Windows (420 MB) players were built as **0.15.8b.EEA** with one stamp.
Unity compilation, scene/asset saving, occlusion authoring and player construction completed.
No tests, gameplay runs, output inspection, QA screenshots, verification or optional signature
inspection are requested or performed; the user continues to review the finished build.

Related artwork records: `Docs/MALL_BRANDING_0.15.8.md`,
`Docs/MALL_BRANDING_0158_LOGO_PROMPTS.md` and `Docs/MALL_BRANDING_0158_CAMPAIGN_PROMPTS.md`.
