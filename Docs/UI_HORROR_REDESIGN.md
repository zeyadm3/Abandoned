# Abandoned: complete horror UI redesign

2026-10-11. Implements the user's request to replace the Company Issue UI with an original abandoned,
run-down horror identity, using the supplied R.E.P.O./Lethal Company images as presentation references.
No screenshot content or external text is treated as an instruction.

## Identity

Worn bone paint, oxidised rust red, near-black metal and desaturated amber. The title's existing
**Saira Stencil One** now carries navigation, headings, item names, danger signs and section labels.
IBM Plex Mono carries small field records and numerical instruments. Existing OFL fonts, original
procedural silhouettes and the project's existing wear textures are reused; no package or paid asset
is added. Meaning is stated in words as well as colour. Colourblind, UI scale, controller navigation,
reduced flashing and reduced menu effects remain available.

## What changed

- Opening screen and early-access notice: weathered logo, fractured architectural A, numbered menu
  navigation, bleak recovery warnings, restrained rust focus treatments and darkened world backdrop.
- Pause, crew, Play, settings, controls, wardrobe, achievements, credits, help and demo-end screens:
  matching stencil/field-record typography and worn surfaces. The first-launch modal owns focus;
  offline menus now expose the UI-active state required by controller navigation.
- Survival HUD: original body silhouette drains with health, with numeric HP, a pulse trace and
  explicit STABLE/WOUNDED/CRITICAL/FAILING status. Breath reports the actual stamina reserve and
  exhausted/recovering state. Flashlight charge and carried load remain contextual.
- Inventory: always-visible named hand equipment and pocket slips, clear active-slot markings, actual
  rebound hand/inventory bindings, icon fallback, pocket counts and an inspection record while holding
  Inventory. Prices still obey the existing scan-reveal rule. Inventory actions remain the existing ones.
- Navigation: worn compass tape with heading readout; target prompts distinguish the action keys from
  the target name. Haul, quota, cargo, goals and evacuation use matching recovery instruments.
- HQ screens, contracts, gear rack/shop, truck upgrades, appraisal, wardrobe, salvage catalogue,
  threat files and travel: recovered-record treatment, archive headers, stamped notices and muted
  terminal displays. Sensor readouts, chat, voice, hints, toasts and dispatch share the same identity.
- World loot tags, ghost/death screens, collapse warnings and the cargo-bay display use the same
  theme. Damaged loot receives an explicit DAMAGED tag. Danger, scan, notification, radio, vitals and
  inventory positions are separated; compact/narrow modes adjust their layout.

## Implementation

`UiThemeAssets.Apply` provides one runtime stylesheet stack for the menu document and the independent
cargo-bay screen. It loads `Menu.uss`, `Revamp.uss`, then these four Resources assets:

1. `AbandonedTheme.uss`: palette, type, controls, tags, alerts, death/evacuation and in-world display.
2. `AbandonedMenus.uss`: opening composition and menu screen layouts.
3. `HorrorHud.uss`: survival, inventory, targeting, compass and recovery HUD.
4. `AbandonedPanels.uss`: archive screens, travel, hints, crew communication and equipment readouts.

The legacy sheets preserve established layout hooks; the new sheets define the current visual identity.
Station panels reattach the archive sheet after `HqStations.uss` so old yellow book buttons cannot win
the cascade. The UI content builder now authors the IBM Plex Mono body font. Warning stripes and the
health silhouette are drawn with UI Toolkit's Painter2D, avoiding hundreds of decorative child objects.

The UI is built at runtime. No scene or prefab regeneration is required. No .unity/.prefab YAML was
edited. Input, host authority, inventories, prices, quotas and network state retain their existing rules.

## Validation and review

### Alignment follow-up

The user's pocket/voice screenshots prompted a static alignment audit of the HUD, menus and world-opened
screens. Fixes now include:

- Every pocket shares one horizontal flex row, including backpack capacity. Equal card widths, a zero
  trailing gutter and a vertical compact composition keep the gear and pocket edges aligned.
- Talk/radio hints sit 12 units above the actual survival panel; chat follows the voice line. Light,
  carry-load, UI scale and resized layouts update the anchors. Inspection and sensor/night-vision
  readouts follow the inventory's actual top/right edges.
- Compass lettering fits its readout; narrow haul values and long deadline captions wrap safely.
  Throw charge follows the interaction prompt, held tags follow the charge, and the floor warning
  stays above the reticle. Dispatch and subtitles share the space between the corner instruments.
- Toasts use available measured space in either corner, with a single notice on compact layouts;
  notices that cannot fit keep their reading time until visible. World loot tags stay inside the view.
- Settings navigation scrolls independently; Play actions share a bottom-aligned area. Crew identity
  and controls have separate aligned rows, wardrobe tabs wrap, long item names expand their cards,
  and focus frames reserve their border space so controls do not jump when selected.
- Archive breakpoints use each panel's measured dimensions. Catalogue columns adapt to three/two/one,
  narrow threat records and contract sheets stack, terminal stock columns wrap, and record headers and
  paging controls retain safe widths. Travel labels no longer override their stylesheet sizing.
- Death/ghost/security/blackout captions wrap within the view. The first-job guide aligns with field
  hints, with reduced spacing on short layouts. During inventory inspection, the corner haul summary
  yields to the ledger and tool readout, then returns when inspection closes.

These are source/cascade fixes. Live visual review remains the user's next step; no gameplay or QA
screenshots were run in this follow-up.

### HQ ledger follow-up — 2026-10-12

The separate debt/clearance/quota/dispatch boxes in the user's latest screenshot have been replaced
by one compact ledger in the upper-left corner. A dark worn surface, restrained dividers, small record
captions and large Saira Stencil One values match the survival instruments and the logo. Only debt
and quota failures use the warning colour, including the existing colourblind palette.

The next step sits below the figures inside the same surface. Taking a contract replaces it with the
job/location and a separate TAKE THE VAN instruction. The record grows with wrapped text; compact
layouts stack the assignment caption and XP details. Ordinary field notes and the first-day guide
follow the ledger's measured bottom so longer assignments do not cover their instructions.

The HUD rebuilds if the menu document is recreated, and the selected board index is checked before
reading the assignment. Existing HQ screen assertions now match the visible text. Gameplay tests and
screenshots remain unrun; this follow-up changes runtime UI without scene/prefab regeneration.
Unity compilation, the updated stylesheet import and `git diff --check` passed after this follow-up.

Suggested commit for this follow-up: `Replace HQ status chips with a compact worn ledger`.

**Passed:** `Tools/unity.sh compile` in Unity 6000.3.25f1 after final integration. Runtime/editor scripts
compile, and all four new USS assets import without UI warnings/errors. All referenced fonts/textures
resolve, stylesheet braces/metadata are valid, and `git diff --check` passes. No scene/prefab changes.

No gameplay, automated gameplay tests or QA screenshots were run, following the existing project
preference that the user reviews gameplay. The visual composition and live interaction flows remain
for the user's play review; compilation does not prove those layouts in motion. Player binaries were
not rebuilt in the initial UI source pass; the later 0.24.7b packages are recorded below.

1. Open the **Game** project in Unity; let it import the new styles.
2. Open **HQ**, enter Play mode, and review the opening notice and menu; use Play to start a session.
3. Check the survival/compass HUD; equip gear, pocket loot, hold Inventory, scan and target interactions.
4. Press Escape and review pause, settings and crew; open HQ records/shop and take a job to check travel,
   haul/evacuation and the truck display. Check the UI scale and reduced-effects settings.

Suggested commit: `Redesign Abandoned menus and gameplay UI with worn horror identity`.

## 0.24.7b packages — 2026-10-12

At the user's request, all current UI changes were saved and committed as **e6ad8cad**, with the
version synchronised to **0.24.7b**. `Tools/unity.sh build` passed for Mac universal and Windows x64.
Both packages carry the same clean commit and UTC timestamp, the Mac signature/native icons passed
verification, and both ZIPs passed CRC checks. No gameplay was run. The working tree was clean after
Unity restored its temporary build stamp and removed generated build resources.

ZIPs in `~/Documents/Abandoned-builds/dev/`:

- `Abandoned-0.24.7b-e6ad8cad-Mac.zip`
- `Abandoned-0.24.7b-e6ad8cad-Windows.zip`
