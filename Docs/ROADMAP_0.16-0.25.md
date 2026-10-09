# ABANDONED — feature roadmap, 0.16 to 0.25.0 Early Access

Written 2026-10-09; Early Access moved from 0.20.0 to 0.25.0 on 2026-10-10 (user: more time, more
features). **Plan only: nothing here is built.** 0.15 (the mall remodel) is planned separately
and is not in this file. Nothing here needs a second map.

- Each feature has its own version number, in the order to build them.
- **Bold** rows are the core: keep these if time runs short.
- Sizes are estimates from reading the code, not measured.
- Each feature was searched for by name in the code and not found; something could exist under a
  name that wasn't tried.

Already built, so not planned again: the field guide, save slots, the guided first job, kick and crew
rules, handing gear to a teammate, the Collector's nest, seven monsters plus the Last Hunter, voices
attracting the Blind One.

---

## What the 0.15 remodel must leave room for

These features need physical places in the building. Their code comes later, but the remodel should
include the spaces, or the mall gets rebuilt twice.

| Needed in the mall | For |
|---|---|
| Door frames on stores and service corridors | 0.16.0 closable doors |
| Locker alcoves | 0.16.0 hiding lockers |
| A breaker room per power sector | 0.16.1 breaker rooms, 0.17.5 live water |
| Alternative stairs, wings that can close, sections that can start fallen | 0.16.2 seeded route changes |
| A lift shaft through every storey | 0.16.3 freight lift |
| Vent runs between rooms | 0.16.4 vents |
| Wired shopfronts | 0.16.5 store alarms |
| A vault room in a jewellery store | 0.16.6 vault drill |
| A security office, camera positions | 0.16.7 security office |
| Glass, timber and tile floor areas | 0.17.2 floor types |
| Patches of broken glass, loose tile, puddles | 0.17.3 noisy ground |
| Low areas that can flood | 0.17.5 live water |
| Cart bays | 0.18.4 shopping carts |

---

## 0.16 — the building's rooms

| Version | Feature | What it is | Size |
|---|---|---|---|
| **0.16.0** | **Closable doors and hiding lockers** | The Hunter's missing counterplay (GDD 9); today no door closes | Large |
| **0.16.1** | **Breaker rooms** | Restore one sector's lights for a few minutes, loudly | Medium |
| **0.16.2** | **Seeded route changes** | Blocked stairs, fallen sections and closed wings differ per job, so the one mall plays differently | Medium–large |
| 0.16.3 | Freight lift | Needs power, slow and loud, has a load limit and drops when overloaded | Large |
| 0.16.4 | Vents | Crouch-only escape routes most monsters can't follow | Medium |
| 0.16.5 | Store alarms | Some shops are wired; tripping one draws monsters, bolt cutters disarm it | Small–medium |
| 0.16.6 | Vault drill | A jewellery safe opened by a long, loud drill someone must guard | Medium |
| 0.16.7 | Security office | Camera screens so one player can guide the crew | Medium–large |

Notes:
- Doors and lockers touch monster AI and the NavMesh, so they go first and get the most time.
- The freight lift is the hardest networking item in this roadmap: players own their own movement, and
  riding a moving platform is where that is most likely to jitter.
- The security office adds camera views on top of the 0.14.0 shadow budget; four-player host performance
  is still unmeasured.

## 0.17 — structure and hazards

| Version | Feature | What it is | Size |
|---|---|---|---|
| **0.17.0** | **Sledgehammer** | Break a floor on purpose to make a hole to drop loot through | Small–medium |
| **0.17.1** | **Crash mat** | Placed under a hole; loot dropped onto it survives | Medium |
| 0.17.2 | Floor types | Glass, timber and tile sections with their own limits and warning sounds | Medium |
| **0.17.3** | **Noisy ground** | Broken glass, loose tiles and puddles that make noise underfoot | Small |
| 0.17.4 | Aftershocks | Telegraphed tremors that damage random sections mid-job | Small–medium |
| 0.17.5 | Live water | Flooded areas are electrified while that sector's power is on | Medium |
| 0.17.6 | Spray chalk | Draw marks on floors and walls: weak spots, routes, anything | Medium |
| 0.17.7 | Glow sticks and flares | Cheap throwable light | Small |

Notes:
- The sledgehammer and crash mat turn the structural system into something players do on purpose, not
  only something that happens to them. Protected routes and stairs stay unbreakable, as today.
- Live water gives the breaker rooms a cost: light in a flooded sector makes its water dangerous.
- A creak must still only ever mean the floor; new floor types need warning sounds that keep that rule.

## 0.18 — loot and money

| Version | Feature | What it is | Size |
|---|---|---|---|
| **0.18.0** | **Boss requests** | "Bring me the piano" bonus on top of quota; pushes crews toward heavy loot | Medium |
| 0.18.1 | Rush orders | Mid-job radio call: one named item pays double for a few minutes | Small–medium |
| **0.18.2** | **Loot quirks** | Data traits on items: rattles, leaks, rings at random, top-heavy | Medium |
| 0.18.3 | Loot sets | Bonus for bringing back a complete set | Small–medium |
| **0.18.4** | **Shopping carts** | Found in the mall; hold several small items and rattle | Medium |
| 0.18.5 | Moving blanket | Wrap one fragile item to soften knocks | Small |
| 0.18.6 | Overtime bonus | Pay multiplier that climbs after quota while the building decays | Small |
| 0.18.7 | Loot catalogue and trophy shelf | A collection that fills in as items are sold; a long-term goal at the HQ | Medium |

Notes:
- Loot quirks must be data on LootDefinition, so a new quirky item needs no new code.
- The trophy shelf is added to the existing HQ scene; the HQ interior is not regenerated.

## 0.19 — crew, comfort and Steam

| Version | Feature | What it is | Size |
|---|---|---|---|
| **0.19.0** | **Ping marker** | Mark loot, a weak floor or a monster with one key; essential without a mic | Small |
| **0.19.1** | **Replacement worker** | Pay a fee from the haul at the truck and a dead crewmate returns as a new hire | Medium |
| 0.19.2 | Ghost knock, flicker and nudge | Dead players can warn the living or push a small object | Medium |
| **0.19.3** | **Post-run blame screen** | Biggest drop, longest fall, who broke what, plus a job title per player from it | Small–medium |
| 0.19.4 | Host difficulty presets | Relaxed / standard / brutal, scaling numbers already in configs | Small |
| 0.19.5 | Emotes | Point, wave, panic; depends on the character rig | Medium |
| 0.19.6 | HQ break-room toys | Basketball hoop and dartboard while waiting for the crew | Small–medium |
| 0.19.7 | Arachnophobia and streamer modes | Swap the Crawlers' look; hide names and lobby ids | Small |
| **0.19.8** | **Controller, first tier** | Gameplay on a pad plus button glyphs (details below) | Medium |
| **0.19.9** | **Text table and font coverage** | Text out of code so translation is possible later; names and chat in Arabic, Cyrillic and CJK | Medium–large |
| 0.19.10 | More achievements and a stats page | Beyond the current 12 | Small–medium |
| 0.19.11 | Rich presence | Needs the game's own App ID, not 480 | Small |
| 0.19.12 | Daily seeded job with leaderboard | Same mall and loot for everyone that day; needs the game's own App ID | Medium |

Notes:
- The replacement worker replaces the cut body-drag revive. A job can run 40 minutes, so dying early
  otherwise means a long wait as a ghost. It is a respawn, not a join, so "join only at the HQ" stands.
- Ping and the replacement worker are small and valuable; move them earlier if 0.16–0.18 slip.

### Controller support (0.19.8)

What exists: all 21 gameplay actions go through one Input System asset, rebinding exists, and on-screen
key prompts are read from the bindings.

| Job | Size | What it involves | Tier |
|---|---|---|---|
| Gameplay on a pad | Small | No gamepad bindings and only a Keyboard&Mouse scheme today. Bindings, stick sensitivity and dead zone, item rotation (reads the mouse wheel directly) | First |
| Fitting 21 actions on a pad | Design decision | Hand slots, inventory, radio, push-to-talk, scan, flashlight, give gear and chat need holds or a wheel | First |
| Closing screens | Small | Eight screens close on a hard-coded Escape key; use the existing Cancel action | First |
| Button glyphs | Needs approval | A glyph set such as Kenney's CC0 Input Prompts is a new asset | First |
| Text chat | Small | Steam's on-screen keyboard, or unavailable on a pad | First |
| Menu navigation | Most of the work | Every screen needs a focus order and a visible focus highlight | Second: 0.23.0 |

Do not tick "Full controller support" on Steam until the second tier (0.23.0) is done.

## 0.20 to 0.24 — the added room

Moving Early Access to 0.25.0 freed five blocks. 0.22 is the user's own choice; 0.20, 0.21, 0.23 and 0.24
were drawn from GDD items not built and Docs/QA_REPORT_2026-10-08.md sections 7–10, and the user approved
all of them on 2026-10-10.

## 0.20 — gear the GDD promised

| Version | Feature | What it is | Size |
|---|---|---|---|
| **0.20.0** | **Portable floodlight** | A placed work light on a battery that lights a room | Small–medium |
| **0.20.1** | **Lockpick** | Opens a locked store quietly but slowly; the crowbar stays fast and loud | Small–medium |
| 0.20.2 | Flashlight recharge at the truck | Top up the battery at the truck during a job | Small |
| **0.20.3** | **Truck winch** | Truck upgrade: a cable that drags a Heavy or Huge item across the front court and up the ramp | Medium–large |
| 0.20.4 | Thermal camera | Handheld view that shows monsters through dark and fog, on a short battery | Medium |
| 0.20.5 | Drone | A flown camera for scouting ahead; limited battery, and its noise draws monsters | Large |
| 0.20.6 | Threat forecast | The contract reads "Known threats: ???" until an upgrade reveals them | Small |

Notes:
- All gear is EquipmentDefinition data in the existing catalogue; the winch joins the truck upgrades.
- The floodlight is one more shadowed light inside the 0.14.0 shadow budget; it must not push the atlas over.
- Thermal view is owner-local, like night vision. The drone is the costly one: a second camera view and
  something new for monsters to hear.

## 0.21 — hands, bodies and the truck

| Version | Feature | What it is | Size |
|---|---|---|---|
| **0.21.0** | **First-person hands** | Visible arms that reach, grip and strain under a carried item | Large |
| **0.21.1** | **Held tools in view** | Flashlight, crowbar, medkit, radio and the rest shown in the hand, and seen by crewmates | Medium–large |
| 0.21.2 | Light player pushing | Players can shove each other a little: comedy, and a risk near a hole | Small–medium |
| **0.21.3** | **Monster animation polish** | Turning, a hit reaction and a distinct idle for each monster | Medium–large |
| 0.21.4 | Truck interior | A real cab, doors and mirror in place of the code-built box and quad | Medium |

Notes:
- Hands and held tools are art-heavy for code-built models and may be better made by an artist than built
  here. A bought or commissioned rig is a new asset and needs the user's approval.
- Players own their own movement, so a push is a request the pushed player's machine applies, capped small.
- The truck's cargo bay size, trigger and payout do not change with the new interior.

## 0.22 — total UI revamp (the user's friend)

Added 2026-10-10. The user's friend redoes the whole UI from a reference the user has in mind (first
planned as a paid freelancer, changed the same day). It merges here: after every feature that adds a
screen (0.16–0.21) exists, and before controller menu navigation (0.23) is built on top of it. The friend
can start 0.22.0 and 0.22.1 much earlier, since neither depends on the features in between.

| Version | Step | What it covers | Size |
|---|---|---|---|
| **0.22.0** | **Reference, style guide and brief** | The reference, a one-page style guide (colours, fonts, panel shapes, icon style, motion), the full screen list, who builds what | Small |
| **0.22.1** | **Theme and kit** | Fonts, colours, buttons, sliders, tabs, panels, key caps, icons and menu effects in UiKit, MenuKit and the theme assets. Every screen picks up most of the new look from this step alone | Medium–large |
| **0.22.2** | **In-job HUD** | Health blocks, stamina, hand slots, compass, quota and haul, floor warning, interaction prompts, tips and the first-day guide | Medium |
| 0.22.3 | World tags and feed | Loot scan tags, ping markers, floating text, toasts, achievement toast, subtitles, text chat | Medium |
| 0.22.4 | Damage and death | Low-health vignette and blood, death card, ghost view, replacement-worker prompt | Small–medium |
| **0.22.5** | **Main menu and Play flow** | Title, backdrop, host / join / solo, save slots, Early Access notice and watermark, demo end screen | Medium |
| 0.22.6 | Pause, crew and settings | Pause menu, crew list with volumes and kick, settings tabs, controls and rebinding, how to play, achievements and stats, credits | Medium–large |
| 0.22.7 | HQ screens | Contracts, boss requests, field guide, inventory and upgrades, wardrobe, loot catalogue | Large |
| 0.22.8 | Job-flow screens | Travel screen, truck display, appraisal receipt, blame screen, security camera screens | Medium |
| **0.22.9** | **Consistency pass** | Every screen at 16:9, 16:10 and ultrawide and at UI Scale 0.75x–1.5x; long translated text and Arabic, Cyrillic and CJK names; controller glyphs and the focus highlight 0.23 needs; old theme assets removed | Medium |

Notes:
- If time runs short, the bold rows alone give a game that looks new: the kit restyles every screen, and
  the HUD and main menu are what players and the store page show most.
- Sizes assume the friend designs and the screens are rebuilt here. Every screen today is UI Toolkit built
  in C# (no UXML, no extra package), about 6,000 lines under Scripts/UI, so a mockup is the easiest thing
  to hand over. If the friend works in Unity instead, they need the repo, their own branch, and this file.
- Each step ends the same way: the user looks at it in the three aspect ratios before the next one starts.
- 0.22.3, 0.22.7 and 0.22.8 include screens from 0.16–0.21 (ping, boss requests, catalogue, blame screen,
  security cameras). If one of those features was cut, its screen drops out of the step.
- Until 0.22, new features get plain UI on the current kit, kept thin so it is cheap to reskin.

Rules that stay through the revamp:
- Health is always-visible blocks; the compass stays top centre; captions never give direction.
- Reduce flashing and Reduce menu effects keep working; colourblind scanner colours keep their meaning.
- No stencil font for small text. Fonts and icons are CC0, OFL or the friend's own work, and each one goes
  in Docs/ASSET_CREDITS.md (the credits screen is generated from it). No new Unity package without asking.
- Nothing in the fiction is named after a real person; the company stays Abandoned Salvaging.

Not decided yet:
- The reference itself.
- Whether the friend delivers mockups and art, or working Unity UI.
- A short written note that the friend's work may be used and sold in the game, and how they are credited.
  Worth having even between friends, before the store page goes up.

## 0.23 — options, accessibility and full controller

| Version | Feature | What it is | Size |
|---|---|---|---|
| **0.23.0** | **Controller menu navigation** | A focus order and visible highlight on every screen; the second controller tier, needed for a Steam Deck rating | Large |
| **0.23.1** | **Accessibility options** | Subtitles on by default with speaker labels and a background, reduce jump scares, shake and head bob as sliders, low-health blur and vignette off, colourblind-safe tags and warnings, heartbeat volume | Medium |
| 0.23.2 | Control options | Separate horizontal and vertical sensitivity, hold or toggle for sprint, talk and radio, a second binding per action, wheel to switch hand slots, choose which pocket item to drop | Medium |
| 0.23.3 | Audio and voice options | Microphone gain and open-mic threshold, output device, radio volume, voice or text chat off, remembered per-friend volume | Medium |
| 0.23.4 | Video options | Monitor picker, toggles for fog, bloom, grain and post-processing, texture quality, FPS counter, field of view above 100 | Small–medium |
| **0.23.5** | **Rejoin at the HQ** | A player who dropped can come back to the same crew at the HQ between jobs; still no joining mid-job | Medium |
| 0.23.6 | Photo mode | The F9 free camera as a real feature, for dead players and the HQ | Small–medium |

Notes:
- Menu navigation is built on the 0.22 UI, which is why it sits after it. "Full controller support" is
  ticked on Steam only once 0.23.0 is done.
- Every new option is a row in the existing settings tabs; 0.22.6 should leave room for them.
- "Players join only at the HQ" stands: rejoining is the same HQ join, with the seat remembered.

## 0.24 — sound, music and store material

| Version | Feature | What it is | Size |
|---|---|---|---|
| **0.24.0** | **Sound cue pass** | Replace the placeholder cues players must learn: creak, groan, snap and each monster's signature | Medium |
| 0.24.1 | Music tracks | Real tracks for the menu, the HQ and travel; jobs still have no music of their own | Medium |
| 0.24.2 | Voices for the boss and dispatch | Recorded lines for the voicemails and the dispatch telephone | Small–medium |
| **0.24.3** | **Achievement icons** | An icon pair for every achievement; Steam requires them | Small |
| **0.24.4** | **Store art and screenshots** | Capsules, library art and five or more screenshots (Docs/STORE_PAGE.md) | Small–medium |
| **0.24.5** | **Trailer** | Cut from captured play (Docs/TRAILER.md) | Medium |
| **0.24.6** | **Steam app and release build** | Steam Direct fee, the game's own App ID, achievements, cloud and depots, then `build-release` (Docs/LAUNCH.md) | Medium |

Notes:
- 0.24.0–0.24.2 are new audio assets: each needs the user's approval and a line in Docs/ASSET_CREDITS.md.
  A creak must still only ever mean the floor, and low health stays the heartbeat alone.
- Screenshots and the trailer come after 0.22, so they show the new UI.
- Rich presence (0.19.11) and the daily job (0.19.12) need the App ID from 0.24.6. Paying the Steam Direct
  fee earlier lets them be built in their own slot; otherwise they move here.
- This block is deliberately light on code so the test pass before 0.25.0 starts from a stable game.

Not placed in any version: the Mimic (GDD 25), the "Flooded basement" modifier, a permanent bonus after
bankruptcy, and van and truck paint (paint may clash with "the company isn't theirs", 0.12.7).

## 0.25.0 — Early Access

Fixes from testing only. No new features.

---

## Kept out until after Early Access

- Hospital and Hotel: the mall still carries Early Access.
- Public matchmaking and mid-run joining.

## Open points

- 0.16–0.19 alone is 37 features, a lot for one person. The 14 bold rows are the core; 0.20–0.24 are
  extra and should be the first to shrink if the core slips.
- Everything from 0.10 onward is still unplayed. The full test pass may reorder this list.
- The UI revamp (0.22) depends on the friend's free time. 0.25.0 should not ship with half the screens
  in each style: set a cut-off date after which the old UI ships and the revamp moves past launch.
- Rich presence and the daily job wait on the Steam Direct fee and a real App ID (Docs/LAUNCH.md).
- The earlier, longer idea list is in Docs/QA_REPORT_2026-10-08.md, sections 7–10.
