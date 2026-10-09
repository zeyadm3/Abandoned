# ABANDONED — feature roadmap, 0.16 to 0.20.0 Early Access

Written 2026-10-09. **Plan only: nothing here is built.** 0.15 (the mall remodel) is planned separately
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
| Menu navigation | Most of the work | Every screen needs a focus order and a visible focus highlight | Second, after Early Access unless a Steam Deck rating is wanted at launch |

Do not tick "Full controller support" on Steam until the second tier is done.

## 0.20.0 — Early Access

Fixes from testing only. No new features.

---

## Kept out until after Early Access

- Hospital, Hotel and the Mimic (GDD 25).
- The drone, thermal camera and truck winch.
- First-person hands and held tools: good for screenshots, but art-heavy for code-built models.
- Public matchmaking and mid-run joining.
- Controller menu navigation (second tier), unless a Steam Deck rating is wanted at launch.

## Open points

- 37 features is a lot for one person. The 14 bold rows are the core; the rest can move after launch.
- Everything from 0.10 onward is still unplayed. The full test pass may reorder this list.
- Rich presence and the daily job wait on the Steam Direct fee and a real App ID (Docs/LAUNCH.md).
- The earlier, longer idea list is in Docs/QA_REPORT_2026-10-08.md, sections 7–10.
