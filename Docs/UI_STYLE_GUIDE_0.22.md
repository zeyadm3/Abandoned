# ABANDONED: UI style guide and brief (0.22.0)

**Superseded by the user's full horror redesign, 2026-10-11.** The current identity, implementation and
editor review steps are in [UI_HORROR_REDESIGN.md](UI_HORROR_REDESIGN.md). The old yellow Company Issue
sheet is retained as a layout/compatibility layer; the current horror sheets load after it.

Written 2026-10-10 during the autonomous 0.16–0.24 build. The roadmap planned the 0.22 revamp from a
reference the user has in mind and their friend's work; that reference was not available, so this guide is
a stand-in direction the friend can keep, change or throw away. Everything it drives lives in ONE stylesheet,
`Art/UI/Resources/Revamp.uss`, loaded after `Art/UI/Menu.uss` (MenuUi), plus small C# changes listed per step
in Docs/AUTOBUILD_0.16-0.24.md. Deleting Revamp.uss returns the pre-0.22 look.

## Direction: "Company Issue"
The crew work for a salvage company they hate. Its paperwork, signage and kit are cheap, standardised and
loud: hazard-yellow on cool slate, stencilled labels, taped edges, forms with boxes. Not grimy horror
(the building does that); the company is bland and bossy. The UI is the company talking.

## Colours (USS variables on :root)
| Token | Value | Use |
|---|---|---|
| --c-ink | rgb(10, 11, 12) | darkest: text on yellow, hazard bars |
| --c-panel / -2 / -3 | slate 18/27/40 | panel, raised, control |
| --c-accent | rgb(242, 190, 40) | hazard yellow: focus, headings, active |
| --c-accent-hot | rgb(255, 214, 92) | pressed |
| --c-text / --c-dim / --c-faint | warm white / grey / 60 % | body, secondary, hints |
| --c-good / --c-bad / --c-money | green / signal red / green | meaning never changes (colourblind scanner keeps its own) |
| --c-paper / --c-paper-ink | form beige / near black | job sheets, receipts |
| --c-crt / --c-crt-bg | amber on green-black | the HQ terminal and security monitors only |

## Type
Barlow (body, OFL), Barlow Condensed ExtraBold/SemiBold (display, buttons), Saira Stencil One (titles only,
never small text), IBM Plex Mono (receipts, numbers that must line up), VT323 (CRT only). Noto fallbacks for
names and chat (0.19.9). Sizes at the 1920×1080 reference, scaled by height: title 88, heading 40, section 22,
body 20, small 16. Never below 15.

## Shapes
Square corners (radius 0–2). Panels: slate, a 6 px yellow spine on the left and a 2 px rule on top.
Buttons: big menu buttons stay words-only (hover: yellow and a nudge); chips are slate slabs that fill yellow on
hover/focus. Fields and sliders: slate track, yellow fill. Key caps: yellow outline boxes. Hazard tape strips
mark danger (the truck leaving, warnings), never decoration.

## Icons
Kenney CC0 white icons, drawn in the gear builder's flat silhouette style; yellow when active, dim otherwise.

## Motion
Short and mechanical: 70–120 ms slides and fades, no bounces or overshoot. Reduce menu effects and Reduce
flashing switch every pulse and glitch off (unchanged rules).

## Rules that stay
Health is always-visible blocks; the compass stays top centre; captions never give direction; the colourblind
scanner colours keep their meaning; no stencil font for small text; fonts/icons CC0 or OFL and credited; the
company is Abandoned Salvaging; nothing named after a real person.

## Screens (who builds what)
All by this autonomous pass, restyled through Revamp.uss in steps 0.22.1–0.22.9: kit (buttons, sliders,
toggles, tabs, panels, key caps) → in-job HUD → world tags, ping markers, toasts, subtitles, chat → damage and
death, ghost → main menu and Play → pause, crew, settings → HQ screens (board, guide, catalogue, gear,
wardrobe) → job flow (travel, truck display, appraisal, blame, security monitors) → consistency (focus
highlight for 0.23 controller navigation, long text, scale). Screen list: MainMenu, Play, Settings (tabs),
Pause, Crew, Achievements/Stats, Credits, HowToPlay, HUD (run haul, clock, health blocks, stamina, hand
slots, pockets, compass, prompts, crosshair, tips, first-day guide), LootTags, Pings, Toasts, Subtitles, Chat,
DeathCard, Ghost, TruckBanner, Travel, TruckDisplay, Appraisal (+ who did what), ContractBoard, FieldGuide,
LootCatalogue, GearRack/Shop/TruckUpgrades, ScrapDepot, AnsweringMachine, Wardrobe, SecurityMonitor,
DroneView, ThermalView, VentCrawl.

## Open for the friend
Replace this direction with the real reference; keep the token names so the C# needs no change.
