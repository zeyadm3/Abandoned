# 0.15.8.EEA — generated mall branding and old sale campaigns

The user requested image-based store identities, themed offers behind storefront glass and hanging
sale banners, plus removal of useless text. This pass installs original printed artwork over the
saved mall without rebuilding the layout, pickups, threats or collection truck. The advertisements
are remnants of the last retail campaigns before the mall shut down.

**0.15.8b update:** the physical operational/truck-text retention policy below is superseded by
`Docs/TEXT_DECLUTTER_0.15.8b.md`. Decorative truck lettering, mall status labels and nonessential
captions are removed; essential directions are fitted to their plaques. All ten generated artwork
assets and 39 placements remain, along with functional UI.

## Generated artwork

Built-in `image_gen` created ten original opaque PNGs. The original files are copied directly into
`Game/Assets/_Project/Art/Generated/MallBranding0158/Images/`, with no external image dependencies.
The four store logos and mall identity are wide 3:1 graphics; sale artwork is portrait 2:3.

| Identity | Logo file | Campaign file | Offer |
| --- | --- | --- | --- |
| Apex Circuit | `ApexCircuit_Logo.png` | `ApexCircuit_Sale.png` | 50% off tech; buy a phone, get a laptop free |
| Nordic Habitat | `NordicHabitat_Logo.png` | `NordicHabitat_Sale.png` | 50% off showroom; buy a chair, get one free |
| Titan Tools | `TitanTools_Logo.png` | `TitanTools_Sale.png` | 90% off warehouse clearance |
| Vogue Thread | `VogueThread_Logo.png` | `VogueThread_Sale.png` | 90% off end of season; buy one, get one free |
| Meridian Mall | `MeridianMall_Logo.png` | `MeridianMall_Sale.png` | Up to 90% off; final weekend sale |

Exact prompts and generation mode are recorded in [logo prompts](/Users/zeyad/Documents/Abandoned/Docs/MALL_BRANDING_0158_LOGO_PROMPTS.md)
and [campaign prompts](/Users/zeyad/Documents/Abandoned/Docs/MALL_BRANDING_0158_CAMPAIGN_PROMPTS.md).
The final images are saved in [project image assets](/Users/zeyad/Documents/Abandoned/Game/Assets/_Project/Art/Generated/MallBranding0158/Images/).
Output inspection and gameplay QA remain deferred
to the user under the standing instruction.

## Scene installation

The artwork pass authors 39 placements: five doorway badges, five adjacent window fascias, ten
behind-glass sale prints, ten interior wall advertisements, five showroom hanging campaigns,
three central banners and one Meridian entrance identity. Entrances receive fitted image fascias,
with sale prints mounted inside glazed panels rather than
in the walking lane. Both Nordic Habitat floors receive their own campaign placements. Interior
wall advertisements and suspended prints use the corresponding store campaign, while Meridian
campaign banners occupy the central concourse. Printed media uses ordinary lit materials, subtle
paper/cloth presentation, preserved aspect ratios and no blocking colliders. Structural attachments
use the existing SectionProp behavior so collapse and restoration continue to affect mounted art.

## Text cleanup

Unity authoring removed 89 obsolete or caption signs from the scene, simplified one stock label
and cleaned four source-model labels. Repeated captions such as RECOVER / CRATE, RECOVER / BATTERY,
SORT / MATERIAL and DISPLAY / REMOVED are removed from source models and the saved scene. Plaintext main-store identities are
replaced by the generated graphic signs, and obsolete lore notices and stale CLOSED OFFICES labels
are removed. Generic SALVAGE FIRST tags are discarded.

Exit, service, stairs and collection directions remain, as do real high-value recovery holds,
demolition clearance, live temporary power, fragile stock classifications and fitted truck markings.
Interactive UI, prompts, pickup definitions and payout systems are retained.

The retained operational labels and physical truck markings described in this historical section
are removed by the 0.15.8b follow-up; the generated logos and sale advertisements are retained.

## Authoring and delivery

Menu: **Tools > Abandoned > Mall > Store Branding and Sale Artwork (0.15.8)**.
Batch entry: `Abandoned.EditorTools.MallBrandingRelease0158.AuthorAndBuildPlayersAndExit`.
The full mall builder also installs the cleanup and campaign artwork on regeneration.

The scene and its updated occlusion data have been saved; Mac (441 MB) and Windows (420 MB) players
were built as **0.15.8.EEA** with one shared stamp. The editor entry saves assets
and the scene, authors updated occlusion data and builds both players with one stamp. Printed art
does not change the collision navigation geometry. No tests, gameplay runs, QA screenshots,
verification or optional signature inspection were performed.
