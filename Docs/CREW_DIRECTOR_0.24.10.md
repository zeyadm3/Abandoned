# 0.24.10 — The Crew Director

Left 4 Dead-style pacing, from Michael Booth's "The AI Systems of Left 4 Dead" (Valve, AIIDE 2009). The user asked
for it after a How It Spawns reel on the AI Director, to give the crew room to actually salvage between scares.

**It changes when monsters come, never how hard they hit.** The contract's threat level, monster damage, the alive
cap, the opening wave, the danger level, building decay, the 40-minute total collapse, the Last Hunter and the
monster songs are all untouched.

## Why

Before 0.24.10 a monster arrived every 28-42 s for the whole job (ThreatPacingConfig; the "90-150 s" in the
2026-10-08 decisions-log entry is out of date), idle ones drifted toward the crew after 10 s and far ones were moved
near them after 55 s. That is close to Booth's "constant, unchanging combat is fatiguing": no quiet in which to line
up a three-person carry, read a cracked floor or argue about what's worth the weight.

## How it works (host only, nothing networked)

1. **Stress.** Each living player has a hidden number, 0-100 (`IntensityTrack`). It rises when they:
   - lose health (1.2 per HP)
   - are knocked down (+20)
   - have a chasing or attacking monster within 14 m (+6 per second)
   - are within 8 m of a floor section that starts cracking (+8) or failing (+8), or collapses (+25)
   - are within 18 m of a crewmate's death (+35)

   It drains 5 per second, starting 2 s after the last rise, and **never while chased** (Booth's rule).
2. **The loop** (`CrewDirectorCycle`) watches the most stressed living player:
   - **Build up**: normal arrivals and drift, for at least 20 s.
   - **Peak**: stress reaches 70. Held 3-5 s.
   - **Peak fade**: until the highest stress is under 35, at most 15 s.
   - **Relax**: 30-45 s.

   Then it builds again. The longest quiet is 15 + 45 = 60 s.
3. **Quiet (fade and relax).**
   - `ThreatDirector` holds later arrivals and unlocked danger waves. The opening wave never waits.
   - `ThreatPacing` stops drifting monsters toward the crew and stops relocating them near it.
   - Every drift-paced monster's wandering is re-centred on a spawn point at least 28 m from everyone. The hint
     only steers wandering, so a hunt or fight already on finishes first, then it leaves.
   - The Stalker, Collector and Last Hunter keep their own way of finding people.
   - Nothing despawns.

## Files

| File | What |
| --- | --- |
| `Scripts/Threats/CrewDirector.cs` | Host MonoBehaviour on the ThreatDirector: stress inputs, ticks the loop, `CrewDirector.Quiet` |
| `Scripts/Threats/CrewDirectorCycle.cs`, `IntensityTrack.cs`, `CrewDirectorPhase.cs` | The rules, plain C# (tested without a scene) |
| `Scripts/Threats/CrewDirectorConfig.cs` | Every number, plus **Enabled** (off = the old clock alone) |
| `Data/Threats/Resources/CrewDirectorConfig.asset` | The tunables (Inspector) |
| `Scripts/Threats/CrewDirectorDebug.cs` | F1 graph and stress bars |
| `ThreatDirector.cs`, `ThreatPacing.cs` | Hold arrivals; retreat instead of drift during the quiet |
| `Editor/Releases/Release02410.cs` | Creates the asset, stamps the version |

## F1 (host, editor or dev build)

- Under the THREATS line: `DIRECTOR <phase>  stress N/70  holding monsters`.
- The last two minutes of the highest stress against the peak line, with quiet stretches shaded.
- Each player's stress by name.
- The THREATS line's new `away N` counts monsters sent away.
- In the Scene view, a green-to-red stress bar above each player.

## Checks (2026-10-18, batch mode, not played)

- Compiles in Unity.
- EditMode: the 4 new `CrewDirectorTests` pass. 290 total, 287 passed; the 3 failures are the older ones from
  0.24.9.
- PlayMode, the real mall (`MallCrewDirectorTests`, 2/2):
  - A 60 HP hit peaks the Director.
  - The quiet holds a due arrival and sends an idle Blind One to a spawn point more than 14 m away.
  - When the quiet ends, the next arrival comes (a Stalker).
  - Switched off, it never holds anything.
- `MallThreatTests`, `MallThreatMixTests` and `MallDangerTests` fail 7 of 8 **with or without this change**: they
  stand the player in the parking lot (off the NavMesh, so nothing spawns) and check coordinates from the pre-0.15
  mall. Not fixed here.

## Full test pass (2026-10-18, second round)

- **Code review fixes:**
  - A fleeing Collector (running off with loot) no longer counts as chasing.
  - The building lookup runs once per job, not four times a second.
  - Each phase change is logged as `[CrewDirector] A -> B at Ns (stress N)`.
- **Full PlayMode suite**: 262 tests, the same 39 failures as the unchanged code (40 there; one lighting test is
  flaky). **No test fails because of this change.** The older failures are mostly tests written for the pre-0.15
  mall (coordinates, routes, player left in the parking lot) and UI screenshot checks.
- **Verify** (project setup + content): setup passes. The 2 content errors are the 0.18.4 shopping cart (visual size,
  Heavy under 50 kg), older than this change.
- **Network, 1 host + 3 clients over 127.0.0.1** (dev build): `basic`, `run` (a whole job, truck, results, next
  run) and `soak` (3 jobs back to back) all pass 4/4. Object count stays flat (37085 -> 37078) and memory stays
  steady.
- **Soak, `MallPacingSoakTests`** (explicit, about 11 min): real jobs in the mall for 5 minutes each. The player stands
  inside and is healed every second; a one-touch kill ends the job and the next starts.

  | 5 min | Off | On |
  | --- | --- | --- |
  | Arrivals | 16 | 8 |
  | HP lost | 781 | 485 |
  | Deaths | 2 | 1 |
  | Quiet | 0 s | 183 s, longest 60 s |
  | Arrivals in a quiet | n/a | 0 |

  Crawlers dealt most of the damage both ways. A player who never moves stays at full stress (monsters are always
  on them), so the Director runs purely on its timers: about 60 % of the time quiet. That is the worst case. In
  real play, stress drains when you get away.

## Tune it in play

- If the mall feels empty, lower **RelaxSeconds** first (try 15-25), then **Peak**.
- If it still feels relentless, raise **RelaxSeconds** or lower **Peak**.
- To compare with the old pacing in the same session, untick **Enabled** on the asset.
- Test player: puddles and floods are noisy ground; standing in a live flood shocks you (that is how the first test
  run died).
