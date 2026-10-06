# ABANDONED — Claude Code Playbook
### How to build the game with Claude Code, milestone by milestone

**Setup:** follow `01_SETUP_GUIDE.md` exactly, with two changes:
- Name the Unity project and GitHub repo `Abandoned` instead of `TheLastPerson`
- In Step 10, also install **AI Navigation** from the Unity Registry (for monster pathfinding)

---

## Part 1 — How this works

**Claude Code does:** writes C# scripts, writes Editor scripts that build scenes and prefabs for you, finds bugs, explains errors, handles git.

**You do:** press Play and test, paste errors from the Unity Console, judge what feels fun, import assets, playtest with friends.

**Claude is the programmer. You're the game director and QA tester.**

The loop:

```
1. Ask for one small thing
2. Claude writes it
3. Click into Unity → recompile → check Console
4. Press Play → test
5. Report: "works" / paste error / describe what feels wrong
6. Works → commit
```

---

## Part 2 — Project setup for Claude

### Step 1: Docs folder

```
Abandoned/
├── Assets/
├── Docs/
│   ├── GDD.md        ← ABANDONED_GDD.md renamed
│   └── PLAYBOOK.md   ← this file
├── CLAUDE.md         ← step 2
└── ...
```

### Step 2: Create `CLAUDE.md` in the project root

Claude Code reads this at the start of every session. Copy it in and keep it updated:

````markdown
# ABANDONED — Project Rules for Claude

## What this is
A 1–4 player co-op first-person salvage horror-comedy for Steam, built by a solo developer.
Players carry physical loot out of collapsing abandoned buildings while avoiding monsters.
The headline feature is the STRUCTURAL SYSTEM: floors, stairs and walkways have load limits,
crack under weight and impacts, and collapse. Full design: Docs/GDD.md.

## Locked tech stack — do not substitute
- Unity 6.3 LTS, URP, C#
- Netcode for GameObjects (NGO), host/client. Do NOT use Mirror, FishNet, Photon or Fusion.
- Transports: Unity Transport (local testing) and Facepunch Transport (Steam),
  switchable via a TransportMode enum on NetworkBootstrap.
- Steam: Facepunch.Steamworks (NOT Steamworks.NET). App ID 480 during development.
- Voice: Dissonance + Dissonance for NGO (Milestone 4+).
- AI: Unity AI Navigation (NavMesh).
- Input: Unity Input System. Do NOT use the legacy Input Manager.
- No new packages or Asset Store assets without asking me first.

## Architecture rules
- Host is authoritative for: loot value and damage, structural damage and collapse,
  monster AI, money, quota, extraction, save data.
- Clients own their movement and input, and request pickups/drops/interactions via RPC.
  The host validates, then applies.
- Loot items and StructuralSections are host-controlled NetworkObjects.
- Collapse debris is NOT networked: the host broadcasts "section X collapsed" plus a seed,
  and every client plays the same pre-fractured break locally.
- Monsters run only on the host; clients see synced position/animation state.
- Data-driven content: loot items, threats, contracts, modifiers and equipment are
  ScriptableObject definitions. Adding a new loot item must not require new code.

## Code style
- One class per file. File name = class name.
- Namespaces: Abandoned.<Area> (Core, Player, Interaction, Loot, Structure, Networking,
  Threats, Extraction, Company, Contracts, Equipment, Voice, Audio, UI).
- Folders: Assets/_Project/Scripts/<Area>/, Assets/_Project/Data/<Area>/,
  Assets/_Project/Prefabs/, Assets/_Project/Scenes/, Assets/_Project/Editor/
- [SerializeField] private fields, not public fields.
- Tunables (speeds, capacities, damage, timers) live in ScriptableObject configs.
- Small components over giant scripts. Nothing over ~300 lines without a reason.
- Comment WHY, not WHAT.

## Working rules
- ONE task at a time, small enough to test in Play mode in under 5 minutes.
- For new systems, explain the plan first (files, classes, data, networking).
- After changes, give me numbered Unity Editor steps. If it's more than ~5 clicks,
  write an Editor script with a menu item under Tools/Abandoned/... instead.
- Never edit Library/, Temp/, Logs/, obj/, or Packages/ (except manifest.json when I ask).
- Never hand-edit .unity or .prefab YAML unless I ask; use Editor scripts.
- Don't delete or rename files outside the current task without asking.
- If something fails twice, stop and explain possible causes instead of guessing.
- Add debug visualizations (Gizmos, on-screen debug text) for every new system,
  toggleable with F1.
- Suggest a commit message when a step works. Don't push unless I ask.

## Testing
- Local co-op: Multiplayer Play Mode + Unity Transport, up to 4 players on one Mac.
- Steam: Facepunch Transport, separate machines, Steam running, App ID 480.

## Current milestone
Milestone 0 — Foundation

## Decisions log
- Stack: NGO + Facepunch + Dissonance (same combination as Lethal Company).
- Structure uses modular sections + pre-fractured mesh swaps, not real-time destruction.
- Levels are hand-built layouts with randomized contents, not procedural generation.
````

### Step 3: First session

```bash
cd ~/Projects/Abandoned
claude
```

First message:

```
Read CLAUDE.md and Docs/GDD.md. Don't write code yet.
Summarize: the game, the structural system, the locked stack, the architecture
rules, and Milestones 0–2. Then list questions and anything in the GDD that
seems technically risky for a solo developer.
```

Fix CLAUDE.md if the summary is off.

---

## Part 3 — Working rules (short version)

1. **Small tasks.** "Add a StructuralSection that tracks load" — not "build the structure system."
2. **Plan Mode first** for any new system (Shift+Tab). Read the plan, push back, approve.
3. **Commit every working step.** "That works, commit it." Tag each milestone.
4. **Paste full Console errors**, including the file and line.
5. **Describe feel precisely.** "Collapse warning is too short — I want ~2.5s between the snap sound and the fall."
6. **`/clear` between unrelated tasks**, `/compact` in long sessions.
7. **"Add that to the Decisions log"** whenever something important is learned.
8. **Stop wandering:** "Stop. Undo everything except X. Stay on the current task."
9. **End-of-milestone review:** "Review this milestone's code for host-only bugs, desyncs, null risks and things that will be hard to extend. List issues, don't fix yet."

---

## Part 4 — Milestone prompts

Use one at a time. Don't move on until it works and is committed.

---

### Milestone 0 — Foundation (≈1 week)

**0.1 Structure**
```
Create the folder structure and namespaces from CLAUDE.md, an assembly definition
for runtime scripts and one for Editor scripts.
```

**0.2 Input**
```
Create an Input Actions asset matching the controls in GDD section 19 (Gameplay
and UI maps), generate the C# class, and tell me how to verify it.
```

**0.3 Test building**
```
Write an Editor script (Tools/Abandoned/Create Test Building) that builds a
two-story greybox building: ground floor and upper floor made of 4x4m floor tiles,
a staircase, a balcony overlooking an atrium, a doorway too narrow for large items
plus a wide loading door, and a parking area outside. Save as
Assets/_Project/Scenes/TestBuilding.unity.
```

✅ Done: compiles clean, test building exists. Tag `milestone-0`.

---

### Milestone 1 — "The Feel" (≈2–3 weeks)

**1.1 Movement**
```
Plan mode: first-person controller with CharacterController and the Input System.
Walk, sprint + stamina, crouch, jump, mouse look. Carried weight will later slow the
player, so expose a speed multiplier. Tunables in a PlayerMovementConfig ScriptableObject.
Separate input from movement so networking is easy later.
```

**1.2 Pickup, carry, throw**
```
Plan mode: interaction system. Look at a Grabbable within 2.5m, E to pick up.
Carry classes from GDD 7.2: Pocket items go to inventory (4 slots); One-hand and
Two-hand items follow a hold point with spring/damped motion (not glued). Hold left
mouse to charge throw, right mouse to drop. Item weight reduces move speed and
increases stamina drain. Heavy/Huge items can't be lifted alone yet (show a hint).
```

**1.3 Loot data and value damage**
```
Plan mode: LootDefinition ScriptableObject with all properties from GDD 7.1.
A LootItem component uses it: tracks current value, takes value damage from
collision impacts above a threshold scaled by fragility, shatters to $0 for
"Extreme" fragility, plays impact sound by material, shows floating "-$X" text.
Create 10 example items from GDD 7.3 using primitive shapes for now, plus an
Editor tool that generates a loot prefab from a LootDefinition.
```

**1.4 Ragdoll**
```
I'll import a free low-poly humanoid. Ragdoll component: switch animated ↔ ragdoll,
test key K, ragdoll on falls over 4m and when hit by heavy objects above a
force threshold. Editor helper to set up ragdoll joints if possible.
```

**1.5 Feel pass**
```
Head bob, landing dip, footsteps by surface, camera shake on heavy impacts nearby.
All toggleable in a settings config.
```

✅ Exit test: Is it satisfying to pick up, throw and break things alone for 5 minutes? Tag `milestone-1`.

---

### Milestone 2 — "The Weight" (≈3–4 weeks) ⭐ most important

**2.1 Structural section**
```
Plan mode: StructuralSection component for floor tiles, stairs and walkways
(GDD section 6). Each has capacity, health and current load. Load = total mass of
rigidbodies and players resting on it (decide the best detection method and
explain trade-offs). Over capacity drains health over time; impacts deal instant
damage based on impulse. Debug Gizmo shows load/capacity and health per section (F1).
Single-player only for now.
```

**2.2 Warning stages**
```
Add the 5 stress stages from GDD 6.1: Stable, Stressed, Cracking, Failing, Collapsed.
Each stage changes visuals (crack decals or material swap, dust particles) and audio
(creaks → groans → snaps). Failing gives a ~2s sag before collapse. Use placeholder
assets and make all thresholds/timings tunable in a StructureConfig.
```

**2.3 Collapse**
```
Plan mode: on collapse, swap the intact section for a pre-fractured version whose
chunks become rigidbodies and fall. Everything resting on it falls too. Debris sleeps
or despawns after ~5s. Write an Editor tool that generates a simple pre-fractured
version of a floor tile (grid of chunks with some randomness) so I don't need
external fracture tools yet.
```

**2.4 Pre-damage and stability**
```
Add a building-wide Stability % that scales capacities and decay. A seeded random
pass pre-damages some sections at start. Debug keys to change stability and
regenerate damage so I can test quickly.
```

**2.5 Structure noise events**
```
Create a global NoiseEvent system (position, loudness, source type). Creaks,
collapses, dropped loot and footsteps emit noise events. Debug view draws noise
spheres. Monsters will listen to this later.
```

✅ **Exit test (the most important one in the project):** Alone in the test building, is it exciting to drag a heavy object across a weak floor and watch it creak, crack and collapse? Can you predict when it will fail? If not, keep tuning here. **Record clips of this and post them** — this is your first marketing. Tag `milestone-2`.

---

### Milestone 3 — "Together" (≈4–6 weeks)

**3.1 Network bootstrap**
```
Plan mode: add NGO. NetworkBootstrap with TransportMode (UnityTransport now, Steam
later). Debug menu: Host / Join (IP) / Disconnect. Networked player prefab,
owner-authoritative movement, only the owner's camera, audio listener and input active.
```
Test with Window → Multiplayer → Multiplayer Play Mode, 2–4 virtual players.

**3.2 Networked loot**
```
Make loot host-controlled NetworkObjects. Pickup/drop/throw go through the host,
which validates. Value damage and shattering are calculated on the host and synced.
Two players can't hold the same item.
```

**3.3 Shared carrying**
```
Plan mode: Heavy and Huge items have multiple carry points (GDD 15). Players grab
points with E. The item lifts only when enough carriers hold it, moves at the
slowest carrier's speed, and wobbles/tilts when carriers pull in different
directions. Letting go drops it (with impact damage). Explain how this works
over the network before building it.
```
Built in the autonomous build as M3.5 (network design in Docs/PROGRESS.md, "M3.5 notes"); `Tools/unity.sh nettest sharedcarry`.

**3.4 Networked structure**
```
Structural damage and collapse run on the host only. Sync each section's stage so
clients see the same warnings. On collapse, broadcast the section ID and a seed;
clients play the identical pre-fractured break locally. Debris stays local.
```

**3.5 Steam**
```
Implement the Steam TransportMode with Facepunch.Steamworks + Facepunch Transport.
App ID 480. Friends-only lobby, invite via Steam overlay, join from invite, member
list. Clear error if Steam isn't running.
```
Test: make a build (`Tools/unity.sh build`, or Tools/Abandoned/Build/Both), send the zip to a friend,
invite through Steam.

**Build + multi-process test (added in the autonomous build as task 3.3)**
`Tools/unity.sh build` makes zipped Mac + Windows Mono builds; `Tools/unity.sh nettest [scenario]`
runs 1 host + 3 headless clients of a dev build on 127.0.0.1 and fails on any disagreement.
Every networked feature (loot, shared carry, structure, disconnects) adds a nettest scenario.

**3.6 Robustness**
```
Handle client disconnect (drop their carried items), host quitting (everyone back to
menu with a message), full lobby, and build version mismatch.
```

✅ Exit test: You and a friend, on different machines over Steam, carry a server rack across a cracking floor and it collapses for both of you at the same moment. Tag `milestone-3`.

---

### Milestone 4 — "Can You Hear Me?" (≈1–2 weeks)

*Buy and import Dissonance + Dissonance for NGO first.*

**4.1 Proximity voice**
```
Read the Dissonance docs in the imported package. Plan mode: Dissonance + NGO,
proximity voice with falloff, push-to-talk (V) and open-mic option, volume slider,
speaking indicator above players.
```

**4.2 Occlusion + radio**
```
Muffle voices through walls. Walkie-talkie item: hold R to transmit to all radio
holders anywhere, with static and a "transmitting" indicator.
```

**4.3 Voice as noise**
```
When a player is speaking, emit NoiseEvents scaled by their voice volume, so
monsters can hear voice chat (GDD 17). Radio transmissions also emit noise
at the speaker's location.
```

✅ Exit test: Whispering feels different from yelling, and both carry through the building believably. Tag `milestone-4`.

---

### Milestone 5 — "The Run" (≈4–6 weeks) → MVP

**5.1 Greybox mall**
```
Plan mode: show me a top-down layout plan for the Abandoned Mall (GDD section 8):
3-floor atrium with walkways and escalators, 6–8 stores, food court, security
office, loading bay, parking lot. Then write an Editor script that builds it from
modular structural pieces so every floor and walkway is a StructuralSection.
```

**5.2 Loot spawning**
```
LootSpawnPoint components with allowed carry classes and tags (jewelry store,
electronics, gallery). A seeded spawner fills them per run. 1–2 jackpot spawn
points placed on upper floors or behind weak structure. Add 10 more loot items
(20 total).
```

**5.3 Truck and extraction**
```
Plan mode: truck in the parking lot with a cargo volume. Loot inside counts toward
the haul. Haul vs. quota HUD (GDD 10). Anyone can start the truck: 10s honk, then it
leaves with players inside; others die. Cargo capacity limit. Extraction window
timer.
```

**5.4 Appraisal screen**
```
After extraction: list every item, its starting value, damage lost, final value,
total vs. quota, and funny stats (most valuable item broken, longest fall, who
broke the most). Then a "Next run" button that restarts in under 30 seconds.
```

**5.5 The Blind One**
```
Plan mode: first monster (GDD section 9). Runs on host only. NavMesh agent that
listens to NoiseEvents, investigates the loudest recent one, wanders when idle,
kills players on contact. States: Wander, Investigate, Hunt, Attack. Debug view
shows what it heard. Update the NavMesh when sections collapse.
```

**5.6 Danger escalation**
```
Danger level rises over time: faster structural decay, more aggressive monster,
ambient cues (static, flickering lights, building groans). Sharp increase after the
extraction window ends.
```

✅ **MVP exit test:** Run through the checklist in GDD section 26. Then play with 2–3 friends and use the GDD section 27 checklist. Tag `milestone-5`.

---

### Milestone 6 — "The Company" (≈4–6 weeks)

```
6.1 Company HQ scene: contract board, shop, truck garage, money display.
6.2 Save system: company money, level, unlocks, owned equipment. JSON on the host's
    machine. Survives quitting.
6.3 Contract generator (GDD section 14): 3 contracts with location, loot estimate,
    quota, stability, power, window, one modifier. Implement 4 modifiers first.
6.4 Shop + equipment: flashlight, radio, medkit, stress scanner, hand trolley,
    rope, planks, noise maker. EquipmentDefinition ScriptableObjects.
6.5 Progression: company XP and levels, quota failure → debt → bankruptcy after
    3 misses (GDD section 13).
6.6 Two more threats: The Stalker and The Collector (GDD section 9).
6.7 Ghost spectator mode for dead players.
```

Send these one at a time, in Plan Mode for each.

---

### Playtest gate (≈2–4 weeks)

- At least 10 sessions with different friend groups
- Record every session (OBS)
- Score each against GDD section 27
- After each session, tell Claude what happened:

```
Playtest notes: nobody used the stress scanner, the Collector was more annoying
than funny, everyone loved dropping things through the atrium. Suggest changes
for each point, ranked by impact vs. effort. Don't code yet.
```

**Don't start Milestone 7 until people ask to play again without being prompted.**

---

### Milestone 7 — Vertical Slice (≈6–8 weeks)

```
7.1 Art pass on the mall with purchased low-poly packs (keep one consistent style).
7.2 Lighting pass: baked lighting, flashlight shadows, dust, fog, daylight shafts.
7.3 Audio pass: structure sounds, loot materials, threat signatures, ambience.
7.4 Main menu, lobby screen, settings (GDD section 20), pause menu.
7.5 Character cosmetics: coverall colors + hats.
7.6 Performance pass: profile with 4 players, cap active rigidbodies, occlusion culling.
```

→ **Pay the $100 Steam Direct fee, create the real App ID, launch the Steam page.** Start collecting wishlists.

### Milestone 8 — Demo (≈4–6 weeks)
Mall-only, polished, ~30–60 minutes of content. Submit for **Steam Next Fest**. Push clips hard during the fest.

### Milestone 9 — Content (≈3–5 months)
Hospital, Hotel, The Hunter, full ~60-item loot table, all modifiers, support jacks and pulleys, achievements, Steam Cloud saves.

### Milestone 10 — Early Access launch (≈1–2 months)
Bug fixing, balance passes, trailer, store page polish, launch discount, community Discord.

---

## Part 5 — When things go wrong

| Problem | What to tell Claude |
|---|---|
| Red errors | Paste the full error: "Fix this and explain the cause." |
| Works for host, broken for clients | "Works on host, not clients. Check ownership, RPC direction, and whether the object is spawned yet." |
| Collapse looks different on each machine | "Collapse is desyncing. Check that the seed and section ID are sent and the fracture is deterministic." |
| Physics goes crazy (things launching) | "Objects are launching on contact. Check mass ratios, collision detection mode and whether held objects push into static geometry." |
| Same fix failing repeatedly | "Stop. List three possible causes and how to test each." |
| Session broke everything | "Show git status and recent commits. Revert to the last working commit." |
| Code getting messy | "Refactor X into smaller components without changing behavior. Plan first." |
| Claude forgot a decision | Put it in CLAUDE.md's Decisions log. |

**Unity gotchas:**
- Click into Unity after Claude edits scripts so it recompiles; wait for the spinner.
- Always test networking changes with 2+ players in Multiplayer Play Mode.
- Steam needs separate machines — test locally with Unity Transport.
- Physics feel depends on Fixed Timestep (Project Settings → Time). Ask Claude before changing it.

---

## Part 6 — Marketing as you build (solo-dev essentials)

| When | Do |
|---|---|
| Milestone 2 | Start posting short clips of collapses (TikTok, YouTube Shorts, X). Greybox clips work if the moment is funny. |
| Milestone 5 | Start a Discord for playtesters |
| Milestone 7 | Steam page live, trailer v1, post everywhere |
| Milestone 8 | Next Fest demo; contact small streamers who play friend-slop games |
| Early Access | Launch discount, regular small updates, listen to the Discord |

Claude can help here too: *"Write 10 short clip ideas from today's build"* or *"Draft the Steam page description from GDD section 29."*

---

## Part 7 — Weekly rhythm

| When | Do |
|---|---|
| Most days | 1–3 small tasks: prompt → test → commit |
| Weekly | Post 2–3 clips; from Milestone 5, one recorded playtest |
| End of milestone | Code review prompt, tag, update "Current milestone" in CLAUDE.md |

---

**The only question until the playtest gate:**

> Did your friends say "one more run"?
