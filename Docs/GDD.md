# ABANDONED
## Game Design Document (GDD)
### Version 0.2 — Pre-Production (Solo Developer Edition)
*0.2 (2026-10-06): decisions from the pre-production technical review — see CLAUDE.md Decisions log.*

---

# 1. Game Overview

**Title:** ABANDONED (working title — check Steam for name conflicts before announcing)
**Genre:** Co-op physics salvage horror-comedy
**Platform:** PC (Windows first, macOS supported), Steam
**Players:** 1–4 online co-op (solo playable, best with 3–4)
**Perspective:** First-person
**Run length:** 10–20 minutes per contract, 45–90 minute sessions
**Price target:** $7.99–$9.99
**Engine:** Unity 6.3 LTS + URP + C#
**Developer:** Solo, with Claude Code

### Elevator Pitch

> Society collapsed. You run a two-bit salvage company. Go into abandoned buildings, carry out anything valuable, and get out before the building — or whatever lives in it — gets you.

### The Hook (what makes it not a R.E.P.O. clone)

> **The buildings are falling apart, and your loot is heavy.**

Every floor has a load limit. Every staircase has cracks. A grand piano is worth $35,000 — and weighs enough to put you through the floor. Drop the server rack and the ceiling below might give way. Monsters hear the creaking.

Other salvage games ask: *"Can you carry it without breaking it?"*
ABANDONED asks: **"Can the building survive you carrying it?"**

### Steam one-liner

*Four friends. One collapsing building. A $100,000 statue that weighs two tons. What could go wrong?*

---

# 2. Design Pillars

## Pillar 1 — Greed vs. gravity
Every big decision trades money against risk: heavier loot, weaker floors, more noise, more danger.

## Pillar 2 — The building is the main enemy
Monsters threaten you. The structure is what ends most runs. Collapse should be readable, predictable enough to plan around, and spectacular when it happens.

## Pillar 3 — Physical comedy
Carrying things together is hard and funny. Dropping things is expensive and funnier.

## Pillar 4 — One more floor
The truck is right outside. You can leave whenever you want. The game is about not wanting to.

## Pillar 5 — Clips
Every run should create at least one moment worth sharing: a floor caving in, a $40k vase shattering, someone falling two stories still holding the TV.

## Pillar 6 — Small, deep, finishable
Built by one person. Fewer locations, more replayability. Every system must earn its development time.

---

# 3. Target Audience

- Friend groups of 2–4 on Discord
- Fans of R.E.P.O., Lethal Company, PEAK, Content Warning, Phasmophobia
- Streamers who play with friends
- Solo players who like extraction tension (secondary; solo must be playable but co-op is the focus)

---

# 4. Setting and Tone

**Setting:** Some years after an unexplained collapse. Cities are emptied. Buildings are decaying. Salvage companies sell recovered goods to the few places still buying. You are the cheapest one.

**Tone:** Horror atmosphere played straight; players bring the comedy. Dry, bleak humor from the company (job board messages, your boss's voicemails, absurd appraisal notes).

**Visual style:**
- Stylized low-poly, readable shapes, strong silhouettes
- Decay everywhere: water damage, sagging ceilings, exposed rebar, moss, collapsed sections
- Lighting carries the mood: flashlight beams, dust in the air, shafts of daylight through holes in the roof, darkness in basements
- Players in salvage coveralls with hard hats; color-coded per player

**Never:** realistic gore. Deaths are ragdolls.

---

# 5. Core Loop

```
Company HQ (shop, upgrades, contract board)
        ↓
Choose a contract (location + risk profile)
        ↓
Drive in → truck parks outside
        ↓
Scout → find loot → judge weight, value, route
        ↓
Carry loot out (alone, together, with tools)
        ↓
Threats + structural decay escalate over time
        ↓
Decide: leave now, or one more floor?
        ↓
Extract → appraisal → payout
        ↓
Pay bills, buy gear, upgrade the truck → harder contracts
```

**The run goal:** meet the contract's **quota** (minimum haul value). Beat it to earn profit. Miss it and you lose money (debt), not your save. Three missed quotas in a row ends the company (start over with a few permanent unlocks — see Progression).

---

# 6. The Structural System (the headline feature)

This is the system that makes the game unique. It gets the most design and polish time.

## 6.1 How it works (player view)

- Every **floor section**, **staircase**, **balcony**, **walkway** and **ceiling** has a hidden load capacity and current health.
- **Weight on it** (players + carried loot + loose objects) drains health slowly when over capacity.
- **Impacts** (dropping heavy loot, falling players, explosions) deal instant damage.
- As health drops, the section shows clear warnings:
  1. **Stable** — nothing
  2. **Stressed** — creaking sounds, dust falling
  3. **Cracking** — visible cracks, louder groans, small debris
  4. **Failing** — loud snapping, section sags, ~2 seconds to get off
  5. **Collapsed** — section breaks into debris and falls; anything on it falls too
- Collapses are **loud** and attract threats.
- Collapses can **open new routes** (drop loot down a hole to the floor below) or **close old ones** (stairs gone).

## 6.2 Contract stability

Each contract has a **Structural Stability %** (e.g., 63%). Lower stability means lower load capacities, faster decay and more pre-damaged sections.

## 6.3 Tools that interact with it

- **Stress scanner:** shows section health as a color overlay for a few seconds
- **Support jack:** place under a floor section to raise its capacity
- **Planks:** bridge gaps left by collapses
- **Rope & pulley:** lower heavy loot through holes instead of using stairs
- **Crowbar:** deliberately break weak sections (make your own shortcut)

## 6.4 Design rules

- Collapse must be **readable**: players should almost always get warnings before a section fails. Deaths should feel like "we got greedy," not "the game cheated."
- Collapse must be **consistent**: same weight + same damage = same result. Players learn the system.
- **Never** collapse the only route to extraction without an alternative. Every level has **authored fallback routes** (rope points, windows, ramps) that can't collapse. There are no runtime connectivity checks: the structure stays consistent, and the level design guarantees a way out.

## 6.5 Technical approach (keep it achievable)

No real-time destruction simulation. Instead:
- Levels are built from **modular structural pieces** (floor tiles ~4×4m, stair segments, walkway segments)
- Each piece has a `StructuralSection` component tracking capacity, health and load
- Load is **logical**, not physics contacts: each load source (a player plus what they carry, a resting item, a share of a shared carry) checks downward for its supporting section(s) and splits its **gameplay weight** across them. Gameplay weight is separate from the clamped Rigidbody mass.
- When health hits zero, the intact mesh swaps to a **pre-fractured version** whose chunks get rigidbodies and fall
- Debris chunks sleep or despawn after a few seconds for performance
- The **host decides** damage and collapse, including cascades onto the sections below; clients receive "section X collapsed" and play the same pre-fractured break locally. The host flips the section's collider off through a networked state change, so everyone falls at the same moment.
- Debris is **cosmetic only**: its own physics layer, never collides with players or loot, never deals gameplay damage

---

# 7. Loot

## 7.1 Loot properties

| Property | Effect |
|---|---|
| **Value** | Base money |
| **Weight** | Carry speed, stamina drain, floor load, how many people needed |
| **Size** | Fits through doors? Fits in the truck? Two-hand carry? |
| **Fragility** | How much value is lost per impact; some items shatter to $0 |
| **Noise** | Sound when moved/dropped (attracts threats) |
| **Condition** | Starting value multiplier (dusty, damaged, pristine) |
| **Rarity** | How often it spawns |

## 7.2 Carry classes

| Class | Examples | Who can carry |
|---|---|---|
| **Pocket** | Watch, jewelry, phone, cash | Anyone, goes in inventory |
| **One-hand** | Laptop, small painting, bottles | One player, can still hold flashlight |
| **Two-hand** | TV, computer tower, vase | One player, slow, no flashlight |
| **Heavy** | Server rack, safe, vending machine | 2 players, or 1 with a hand trolley |
| **Huge** | Piano, statue, military generator | 3–4 players, or tools (trolley + ramp, pulley) — a solo player can move them slowly with a trolley + ramp |

**Solo:** the hand trolley is available from the start. Jackpots are possible solo, just hard.

## 7.3 Example loot table (launch target: ~60 items)

| Item | Value | Weight | Class | Fragility | Notes |
|---|---:|---|---|---|---|
| Gold watch | $7,500 | Tiny | Pocket | Low | Easy money, rare |
| Cash bundle | $500–2,000 | Tiny | Pocket | None | Common |
| Laptop | $1,200 | Light | One-hand | Medium | |
| Painting (small) | $3,000–15,000 | Light | One-hand | Medium | Value varies wildly |
| Flat-screen TV | $2,500 | Medium | Two-hand | High | Screen cracks easily |
| Antique vase | $25,000 | Medium | Two-hand | Extreme | One drop = $0 |
| Glass sculpture | $40,000 | Medium | Two-hand | Extreme | Rings when it hits anything |
| Server rack | $18,000 | Very heavy | Heavy | Low | Floor killer |
| Safe | $5,000 + contents | Very heavy | Heavy | None | Crack it open or carry it out whole |
| Vending machine | $6,000 | Very heavy | Heavy | Low | Rattles loudly |
| Grand piano | $35,000 | Massive | Huge | Medium | Won't fit through normal doors — find the loading bay |
| Marble statue | $100,000 | Massive | Huge | Medium | The greed item |
| Military generator | $60,000 | Massive | Huge | Low | Needs 4 people or a trolley + ramp |

## 7.4 Loot rules

- Every location has 1–2 **jackpot items** (huge, valuable, awkward) and lots of small stuff
- Jackpots are usually on **upper floors** or **behind weak structure**
- The truck has **limited cargo space** (upgradeable) — you can't take everything

---

# 8. Locations

## Launch: 3 locations, each fully replayable

> **Early Access scope:** the Mall carries Early Access. Hospital and Hotel are **post-Early-Access candidates** (see roadmap).

### 1. Abandoned Mall *(build this first)*
- Multi-level atrium with walkways, escalators, a glass roof
- Stores (electronics, jewelry, furniture, art gallery), food court, cinema, security office, parking garage
- Collapse showcase: atrium walkways, escalators, the glass roof
- Jackpot: art gallery statue on the top floor

### 2. Hospital *(post-Early-Access candidate)*
- Operating rooms, pharmacy, ICU, morgue, basement, elevator shafts
- Collapse showcase: water-damaged upper floors, elevator shafts as drop routes
- Darker, tighter, scarier

### 3. Luxury Hotel *(post-Early-Access candidate)*
- Lobby, restaurant, casino, ballroom, suites, service tunnels
- Collapse showcase: tall atrium, penthouse balconies, ballroom chandelier
- Most valuable loot, weakest structure

### Post-launch locations
Cruise Ship (tilting, flooding), Military Base, Office Tower, Museum, Classified Facility (endgame).

## Level generation

**Hand-built layouts, randomized contents.** No procedural level generation (too expensive solo).
- Each location has a fixed layout made of modular pieces
- Each run randomizes: loot spawns, jackpot location, pre-damaged sections, locked doors, threat types, power on/off, entry point
- Later: 2–3 layout variants per location if time allows

---

# 9. Threats

Each threat has one clear rule players can learn. Launch with **4**, add more post-launch.

### Launch threats

**The Blind One**
- Can't see. Hunts by sound: footsteps, voices, dropped loot, creaking floors, collapses.
- Counterplay: move slowly, crouch, don't drop things, whisper.
- Pairs perfectly with the structure system (creaking floors attract it).

**The Stalker**
- Follows players at a distance. Rarely attacks. Stares.
- Attacks isolated players who look away from it too long.
- Counterplay: stay grouped, keep checking behind you.

**The Collector**
- Doesn't hurt you. Steals loot that's left unattended and hides it somewhere in the building.
- Counterplay: guard your loot pile, carry things straight to the truck.
- Comedy and frustration in equal parts — tune carefully.

**The Hunter**
- Patrols and chases players on sight. Kills on contact.
- Counterplay: break line of sight, close doors, hide, lure it onto weak floors (it's heavy).

### Post-launch threats
- **The Mimic** — copies player voices (stretch; needs voice recording)
- **The Weight** — something huge in the walls; its movement damages structure
- **The Thing** — rules hidden; players figure it out over time

### Threat escalation

- Runs start with 0–1 threats active
- Every few minutes, the **danger level** rises: more threats spawn, existing ones get more aggressive, structure decays faster
- Danger level shown subtly (radio static, lights flickering, a building-wide groan)

---

# 10. Extraction and Greed

- The truck is parked outside. Loot counts only when it's **in the truck** when you leave.
- Anyone can start the truck. It honks for 10 seconds, then leaves with whoever is inside.
- Players left behind are dead for that run (they lose their carried pocket items).
- **Extraction window:** each contract has a soft time limit. After it ends, danger escalates sharply.
- The HUD shows **current haul vs. quota** so the "one more floor" argument always has a number attached.

---

# 11. Death and Revival

- Death = ragdoll + ghost spectator for the rest of the run
- Ghosts can follow teammates and see threats but can't help
- Dead players' pocket loot drops where they died (teammates can recover it)
- Death ragdolls are local only (not networked)
- ~~**Revive**: drag a body back to the truck to revive at the end of the run for a fee~~ — **cut for launch** (would need networked, draggable ragdolls)

---

# 12. Equipment

Bought at HQ between runs. Limited inventory: **2 hand slots + 4 pocket slots**.

### Basic (starting)
- Flashlight
- Walkie-talkie radio
- Medkit
- Hand trolley (so solo players can move Heavy items)

### Mid
- Stress scanner
- Backpack (more pocket slots)
- Rope (lower loot through holes)
- Planks (bridge gaps)
- Motion detector
- Noise maker (distract the Blind One)

### Advanced
- Support jack
- Night vision
- Thermal camera
- Bolt cutters / lockpick (locked rooms)
- Drone (scout ahead)

### Team equipment
- Flatbed trolley + ramp (Huge items, slowly)
- Pulley system (lower loot between floors)
- Portable floodlight (scares some threats, uses power)

---

# 13. Company Progression

## Company level

| Level | Unlocks |
|---|---|
| 1 | Beat-up van, small cargo, mall contracts, hand trolley |
| 3 | Better trolley, stress scanner in shop |
| 5 | Truck (more cargo), hospital contracts |
| 8 | Pulley, support jacks, harder modifiers |
| 10 | Hotel contracts |
| 15 | Night contracts, elite modifiers |
| 20+ | Classified contracts (post-launch endgame) |

## Truck upgrades
Cargo space, engine (faster honk-to-leave), armor (Hunter can't follow inside), floodlights, winch.

## Company failure
Three failed quotas in a row → bankruptcy. Start a new company, keep cosmetic unlocks and a small permanent bonus. Runs stay tense without saves feeling fragile.

---

# 14. Dynamic Contracts

The contract board shows 3 contracts at a time. Each one rolls:

```
HOTEL — "The Meridian"
Estimated loot:         $120,000 – $250,000
Quota:                  $90,000
Threat level:           HIGH
Known threats:          ??? (scanner upgrade reveals)
Structural stability:   63%
Power:                  OFF
Extraction window:      18 min
Modifier:               Flooded basement
Payout bonus:           +20%
```

### Modifiers (launch target: ~10)
Power off, flooded basement, night (darker, more threats), heavy jackpot (one massive item), fragile collection (lots of glass), already picked over (less loot, more traps), unstable (stability −20%), rush job (shorter window, higher bonus), sealed (must break in), storm (noise covers footsteps, structure decays faster).

---

# 15. Physics and Interaction

- Pick up, carry, drop, throw; hold to charge throws
- **Shared carrying:** 2–4 players grab different points of a Heavy/Huge item; it moves at the speed of the slowest carrier and wobbles if carriers pull in different directions
  - Heavy items have 2 handles and need 2 people to lift; Huge items have 4 handles and need 3 (the military generator needs 4). Each LootDefinition can override the crew size or place its own handles; otherwise handles are generated at the ends of the item's long axis, then the middles of its long sides.
  - E on the item takes the nearest free handle; RMB lets go. Short of a full crew the item stays on the floor: one person can still drag a Heavy item (trolley stand-in); a Huge item short of its crew can only be nudged about 0.75 m from where it was first held (letting go and regrabbing doesn't refill that), so Huge items really need a team or the trolley + ramp.
  - Lifted, it rises about 30 cm and goes at the slowest carrier's pace (crouching or loaded-down carriers slow everyone; no sprinting). Each carrier is tied to their handle: you can't walk away from it, and you get pulled along if the others move it. Your grip turns with the item, so the crew can swing a long item round a corner by walking around each other.
  - Letting go, getting knocked down or getting dragged too far from your handle drops your share; when the crew falls below the requirement the item falls and takes normal impact damage.
  - Each carrier's share of the weight loads the floor under their own feet; the lifted item itself loads nothing.
- Loot loses value from impacts above a threshold (fragility scales the loss); a small floating "-$1,200" pops up on damage
- Objects can roll, slide, tip, block doors, trigger alarms and make noise
- Players ragdoll from big falls, heavy impacts and collapses
- Players can push each other lightly

**Networking rule:** the host owns loot value and damage, and structural state. While one player carries an item, that carrier owns its physics; shared carries are simulated by the host from the carriers' input forces. Small debris is local and cosmetic only.

---

# 16. Audio

Audio carries information and comedy.

- **Structure:** creaks, groans, snaps, dust trickles — layered by stress stage. Players should learn the sounds.
- **Loot:** impact sounds per material (glass ring, metal clang, wood thud), cash-register "cha-ching" on extraction
- **Threats:** each has a signature sound (the Blind One's clicking, the Stalker's breathing)
- **Proximity voice:** distance falloff, muffled through walls, radio for long range
- **Ambience:** wind through broken windows, dripping water, distant collapses

---

# 17. Proximity Voice

Using Dissonance:
- Distance-based falloff and wall muffling
- Walkie-talkie for long range (with static)
- **The Blind One can hear voice chat.** Talking loudly near it is dangerous. (Turns voice into a mechanic, and creates great clips.)

---

# 18. Customization

**Launch:** coverall colors, 10–15 hats/helmets, a few accessories (masks, goggles, backpacks), unlocked by playing.
**Later:** more cosmetics, emotes, truck paint jobs.

No gameplay advantages from cosmetics. No microtransactions at launch.

---

# 19. Controls

| Key | Action |
|---|---|
| WASD | Move |
| Shift | Sprint |
| C | Crouch |
| Space | Jump |
| E | Interact / pick up / grab a carry point |
| Left mouse | Use item / throw (hold to charge) |
| Right mouse | Drop |
| Q | Scan (if scanner equipped) |
| F | Flashlight |
| 1 / 2 | Hand slots |
| Tab | Inventory + haul total |
| V | Push-to-talk |
| R | Radio |
| Esc | Menu |

---

# 20. Settings and Accessibility

Volume sliders (master, voice, SFX, music), push-to-talk toggle, mouse sensitivity, FOV, camera shake toggle, head bob toggle, subtitles for structural warnings and threat sounds, colorblind-safe scanner colors, remappable keys.

---

# 21. Technical Architecture

| System | Choice |
|---|---|
| Engine | Unity 6.3 LTS, URP |
| Networking | Netcode for GameObjects (NGO), host-client |
| Transports | Unity Transport (local testing) / Facepunch Transport (Steam), switchable |
| Steam | Facepunch.Steamworks (lobbies, invites, rich presence, achievements, cloud saves later) |
| Voice | Dissonance + Dissonance for NGO |
| AI navigation | Unity AI Navigation (NavMesh), updated around collapsed sections — approach decided at M5 (recommended: per-section navmesh with carving/toggling + NavMeshLinks, no runtime rebakes) |
| Camera | Cinemachine (head bob, shake, landing dip) |
| Input | Unity Input System |
| Levels | Modular pieces assembled in-editor; ProBuilder for greybox |
| Save data | Host's machine stores the company save (JSON); Steam Cloud later |

### Authority
- Host decides: loot value/damage, structural damage and collapse, threat AI, money, quota, extraction
- Clients: own movement and input; request pickups/drops; host validates
- A single carrier owns the carried item's physics; shared carries are host-simulated
- Players join only at HQ between runs (no mid-run joining)
- Builds: made on Mac (Mono), shared as a single zipped folder; Windows tested on a friend's PC

### Performance budget
- Target 60 FPS at 1080p on a mid-range PC
- Debris chunks sleep or despawn after ~5 seconds
- Limit simultaneously active rigidbodies; loot sleeps when untouched
- Occlusion culling and baked lighting per location

---

# 22. Monetization

- Premium, one-time purchase: **$7.99–$9.99**
- Launch discount 10–20%
- Later: free content updates (new locations, threats) to drive sales; maybe a cosmetic supporter pack
- No pay-to-win, no battle pass, no loot boxes

---

# 23. Solo Developer Roadmap

Time estimates assume steady part-time to near-full-time work with Claude Code. They will slip — that's normal. What matters is the order.

| Phase | Milestone | Goal | Rough time |
|---|---|---|---|
| 0 | Foundation | Project, repo, CLAUDE.md, input, test scene | 1 week |
| 1 | The Feel | Movement, pickup/throw, loot value damage, ragdoll | 2–3 weeks |
| 2 | The Weight | **Structural system prototype** in one test building | 3–4 weeks |
| 3 | Together | NGO + Steam co-op, shared carrying, synced collapse | 4–6 weeks |
| 4 | Can You Hear Me | Proximity voice, radio | 1–2 weeks |
| 5 | The Run | Truck, extraction, quota, appraisal, the Blind One, greybox mall | 4–6 weeks |
| 6 | The Company | HQ, shop, contracts, saving, 2 more threats | 4–6 weeks |
| — | **Playtest gate** | 10+ sessions with friends. Fun? If not, fix before continuing. | 2–4 weeks |
| 7 | Vertical Slice | Mall art pass, audio pass, menus, settings → **Steam page goes live** | 6–8 weeks |
| 8 | Demo | Polished mall-only demo → Steam Next Fest | 4–6 weeks |
| 9 | Content | Mall content depth, 4th threat, full loot table, modifiers, progression (Hospital + Hotel are post-EA candidates) | 3–5 months |
| 10 | Early Access launch | Bug fixing, balance, trailer, launch | 1–2 months |

**Spikes:** 1-hour Facepunch Transport + NGO 2.13 compatibility spike at the end of M1. Dissonance + NGO 2.x spike before buying Dissonance at M4.

**Post-Early-Access candidates:** Hospital, Hotel.

**Total: roughly 12–18 months to Early Access.** Plenty of friend-slop games launched in Early Access with one or two locations; you don't need everything on day one.

### The two gates that matter most
1. **End of Milestone 2:** Is the structural system fun to mess with alone? If watching a floor creak and collapse under a server rack isn't exciting, rework it before building anything else on top of it.
2. **Playtest gate after Milestone 6:** Do friends ask to play again? If not, don't start the art pass.

---

# 24. Solo Developer Survival Rules

- **Art: buy, don't make.** Use low-poly asset packs (Synty-style paid packs; Kenney and Quaternius are free) and keep one consistent style. Spend your time on systems.
- **Audio: library first.** Free/cheap sound libraries (e.g., the free annual Sonniss GDC bundle, freesound.org with license checks). Hire a freelancer only for signature sounds later.
- **One location until it's fun.** The mall carries the game until the playtest gate.
- **Steam page early.** Wishlists take months to build. Page live after the vertical slice; demo for Next Fest.
- **Post clips constantly.** TikTok/Shorts/X of collapses and dropped loot from Milestone 2 onward. This genre is sold by clips.
- **Weekly playtests from Milestone 5.** Record everything.
- **Keep a cut list.** When a feature isn't working after a week, cut it or shelve it.
- **Budget (approximate):** Claude Max subscription, Dissonance, art packs ($100–400 total), Steam Direct fee ($100), optional Windows PC for builds. You can reach a demo for well under $1,000 plus the subscription.

---

# 25. Things NOT to Build (for launch)

- Procedural level generation
- More than 3 locations (Early Access ships with the Mall; Hospital and Hotel come after)
- Body-drag revive
- Mid-run joining
- Real-time mesh destruction (use pre-fractured pieces)
- The Mimic (voice copying)
- Character creator
- Public matchmaking (friends/invite lobbies only)
- Story mode, cutscenes, lore documents
- Console, mobile, VR
- More than 4 players

---

# 26. MVP Definition (end of Milestone 5)

- [ ] 1–4 players join via Steam
- [ ] Proximity voice works
- [ ] Greybox mall with breakable floors, walkways and stairs
- [ ] 20+ loot items with weight, value and fragility
- [ ] Shared carrying works for heavy items
- [ ] Structural sections creak, crack and collapse under load
- [ ] The Blind One hunts by sound
- [ ] Truck extraction with haul vs. quota
- [ ] Appraisal screen with payout
- [ ] Next run starts in under 30 seconds

---

# 27. Playtest Checklist

After every session:
1. Did someone yell "DON'T DROP IT"?
2. Did a floor collapse, and did players understand why?
3. Did the group argue about leaving vs. one more floor?
4. Did anyone get scared?
5. Did something happen that someone wanted to clip?
6. Did a jackpot attempt fail spectacularly?
7. Did they want another run?

---

# 28. What Makes It Viral

1. Four players carrying a statue across a cracking walkway. It holds… it holds… it doesn't.
2. "$42,000 → $0" as the vase hits the floor.
3. Whispering past the Blind One while carrying a rattling vending machine.
4. Someone falls through the floor, lands on the floor below, still holding the TV. The TV survives. They don't.
5. Truck honking, one player sprinting out with a painting, the Hunter right behind them.
6. The piano won't fit through the door. Twenty minutes of attempts.

---

# 29. Steam Page

**Short description:** *Run a salvage company in a collapsed world. Carry priceless loot out of decaying buildings with your friends — before the floor gives way or something hears you.*

**Trailer beats (30s):**
- 0–5s: Flashlight beams in a dark mall atrium. "Okay, it's just one statue."
- 5–12s: Four players lifting it. Creaking. "Is that… is the floor supposed to do that?"
- 12–15s: Floor collapses. Everyone falls one story. Statue lands intact. Cheering.
- 15–25s: Fast cuts — vase shatters ($0), the Blind One turns toward a whisper, the piano stuck in a doorway, truck honking, someone sprinting with a TV.
- 25–30s: Title card. *ABANDONED.*

---

# 30. The Core Rule

Every feature must pass:

> **Does this make carrying loot out of a collapsing building more tense, more interesting, or funnier?**

If not, cut it.

---

# END OF DOCUMENT
