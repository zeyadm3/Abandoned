# QA Resolution Report — 2026-10-10

Static QA pass over the game's runtime code at 0.24.6b, with fixes applied in place. Shipped as 0.24.7.
All paths below are under `Game/Assets/_Project/Scripts/`.

## 1. Executive summary

**Nothing here was played.** The fixes were written from reading the code and were compiled outside Unity
(see "Verification"). Unity itself was not opened, no tests were run and no build was made.

**Before.** The codebase is in better shape than most at this stage: host authority is applied consistently,
every static is reset for play-mode re-entry, event subscriptions are all paired with unsubscriptions, there
are no empty catch blocks, no deprecated Unity API calls and no unbounded loops. The risk sits where
`Docs/AUTOBUILD_0.16-0.24.md` says it does: the 0.16–0.24 features that were written and never run. That is
where almost every bug below was found.

**After.** 44 runtime files changed (430 lines added, 87 removed). Fixed: 3 money/payday bugs, 4 ways a
player could end up stuck, misplaced or wrongly invisible to monsters, 1 threading race in the sound library,
3 leaks and 5 per-frame allocation sources, plus hardening of host RPCs and save handling. The four C#
assemblies (runtime, editor, edit-mode tests, play-mode tests) compile with 0 errors.

**Coverage, honestly.** About 75 files (the save and payday path, the run state and all its 0.16–0.20
extras, the player's networking, loot networking, the threat director, the structure simulation, the sound
library and pool, gear use, voice relay) were read line by line. All 605 runtime files were pattern-scanned
for leaks, per-frame allocations, unpaired events, swallowed errors, deprecated calls and risky loops.
Not reviewed in depth: the 282 editor scripts, most UI views, the Steam lobby flow and the 0.22 stylesheet
(which needs eyes, not a reader).

## 2. Critical bugs squashed

### Money and payday

| # | Bug | Fix |
|---|-----|-----|
| 1 | **Rush orders paid for loot that was never rushed.** A rush is called on a kind with at least one item still in the building, but the half-second bay check then noted every item of that kind in the bay, including ones loaded long before. Two chairs already aboard plus one outside meant an instant double payout on the two. | `Extraction/Bonuses/RunRush.cs:89` remembers what was aboard when the rush was called and leaves it out (`:69`). The appraisal line also stops naming only the last rush's kind when several paid. |
| 2 | **Overtime was paid on a stale haul.** The haul was only tallied while the phase was Running, so everything thrown in during the honk was missing from the synced haul that overtime read; the truck display and HUD froze for the whole honk too. | `Extraction/RunState.cs:148` tallies during the honk; `Extraction/Bonuses/RunOvertime.cs:63` now sums what is actually aboard at departure. |
| 3 | **Difficulty quota was applied in one place only.** The run scaled the quota, but the board, van prompt and depot printed the unscaled one (a documented known gap), and a job quit on purpose was penalised on the unscaled quota stored in the save. | One source: `Company/CompanyService.Board.cs:33` (`QuotaFor`, from the synced preset, same numbers as before). `Extraction/RunDirector.cs:118` no longer scales it a second time. |
| 4 | **Bankruptcy silently reset the host's difficulty to Standard.** | `Company/CompanyLedger.cs:67` carries it over. |
| 5 | **A failed save could stop payday reaching the crew.** `SaveStore.Save` promised never to throw but only caught IO errors; anything else escaped before the outcome was published. | `Company/SaveStore.cs:133` catches every failure, logs it and retries at the next save. |
| 6 | **A null string anywhere in the results would strand everyone on a truck that had "left".** NGO cannot serialise null strings; the phase was already Departed when the RPC threw. | `Extraction/RunResults.cs:30`, `:97` write nulls as empty. |
| 7 | **Pockets of a knocked-down rider counted for the haul but not the bonuses** (two different "is aboard" tests). | `Extraction/RunState.cs:275` uses the same test. |

### Players stuck, misplaced or wrongly hidden

| # | Bug | Fix |
|---|-----|-----|
| 8 | **Respawned into a vent.** A respawn arrives as an RPC before the hiding value clears, so the late "out of the vent" teleported a freshly respawned player to the duct's far grille. Lockers already handled this; vents did not. | `Networking/NetworkPlayer.cs:287`, `NetworkPlayer.Vents.cs:106` |
| 9 | **Dying mid-crawl dropped the body 60 m under the level**, and a missing exit grille left the player parked there with controls on, falling for ever. A venting player also survived the total collapse, because they were "outside" the building's bounds. | `Networking/NetworkPlayer.cs:265`, entry-point fallback in `NetworkPlayer.Vents.cs:106`, `Extraction/BuildingDecay.cs:171` |
| 10 | **Invisible-to-monsters exploit.** Launching the drone from inside a locker, then landing it, switched the motor back on while the player was still flagged hidden. | `Equipment/PlayerEquipment.Tools.cs:25`, `Equipment/DronePilot.cs:51` |
| 11 | **Drone and security desk handed controls to a ragdolled player**, re-creating the QA B-31 state (motor on while the body lies on the floor), and a knocked-down pilot steered body and drone with one stick. | `Equipment/DronePilot.cs:51`, `Extraction/Security/SecurityDesk.cs:56` |
| 12 | **Photo mode could hide the HUD for the rest of the session**: if the Pictures folder refused the write (macOS privacy), if two photos overlapped, or if you left with F9 mid-shot. | `Player/FreeCamera.cs:67`, `:97`; falls back to the game's own folder and always restores the HUD. |
| 13 | **The winch let go halfway up the ramp.** The "stuck" timer compared against the best distance of the previous leg, so progress on the ramp leg never registered. | `Extraction/Winch/RunWinch.cs:85` |
| 14 | **A NaN shove would permanently break a player's position**; the cap was applied only by the sender. Same class of problem for ping, spray and ghost-nudge rays. | `Networking/NetworkPlayer.Shove.cs:23`, `Extraction/Pings/RunPings.cs:59`, `Equipment/PlayerEquipment.Tools.cs:105`, `Extraction/Ghosts/RunGhosts.cs:43` |

### Sync and logic

| # | Bug | Fix |
|---|-----|-----|
| 15 | **Ringing loot was out of sync.** The ring roll used the item's seed, which only the host has; on a client every phone and clock rang together, and never when the host's monsters heard it. | `Loot/LootQuirks.cs:85` keys the roll by NetworkObjectId. |
| 16 | **"Dead longest" was arbitrary.** Deaths were only timed the first time someone looked at the hire radio. | `Extraction/Hires/RunHires.cs:51` |
| 17 | **The lift jumped at the moment it gave way** unless `SnapAt` was exactly 0.5 (linear snap point against an eased ride). | `Extraction/Lift/RunLift.cs:50` |
| 18 | **Riders could be left behind by the lift**: the platform probe hit the rider's own colliders or loot underfoot, and the platform moved in undefined order relative to the motor. | `Core/MovingPlatform.cs:35`, `Extraction/Lift/FreightLift.cs:14` |
| 19 | **Pings past a player's third were silent.** | `Extraction/Pings/RunPings.cs:44` |
| 20 | **A damaged save could stop payday for good in editor and dev builds**: an over-long catalogue id threw from `Update` every frame, before the payout hook. | `Company/CompanyService.Catalogue.cs:43`; same wire-safe cut in `RunRequests.cs:77`, `RunRush.cs:93`, `NetworkPlayer.cs:228` |

## 3. Performance improvements

**Threading race in the sound library** (`Audio/Synth/SynthLibrary.cs:72`, `:110`). When the main thread
rendered a sound on the spot while the worker was rendering the same one — likely at start-up, since the
worker begins with the sounds the menu plays first — the worker's copy was stranded. `Adopt` takes the first
entry it finds, so it met that entry every frame and adopted nothing else for the rest of the session:
finished renders were held as raw sample arrays instead of being freed.

**Leaks plugged**
- Tool models (`Equipment/ToolModels.cs:79`): a new material per part every time the gear in a hand changed,
  for every player on every machine, never destroyed. Now one shared material per colour.
- Arachnophobia lumps (`Threats/ArachnophobiaSwap.cs:40`): six materials per swarm; now one, destroyed with it.
- Chalk mesh (`Extraction/Chalk/RunChalk.cs:46`) and security feed texture (`Security/SecurityDesk.cs:113`):
  one object left behind per job.

**Per-frame work removed**
- Lift button prompt (`Extraction/Lift/RunLift.cs:76`): a whole-scene search for loot every frame someone
  looked at the button. Now reads the existing load registry.
- Spray chalk (`Extraction/Chalk/RunChalk.cs:24`): three fresh lists of up to 800 dots on every new dot,
  about 16 times a second while spraying. Buffers are reused.
- Ringing loot (`Loot/LootQuirks.cs:85`): a `System.Random` per item per frame. Now one roll per 75 s window.
- Threat director (`Threats/ThreatDirector.cs:47`), ghost spectator (`Networking/GhostSpectator.cs:59`),
  threat pacing (`Threats/ThreatPacing.cs:89`), worker body (`Player/Presentation/WorkerPresentation.cs:80`):
  LINQ queries, path objects and component lookups repeated every frame or tick.

**Also tidied:** primitive colliders on tool models, lumps and blankets are disabled before the deferred
destroy (`NetworkLoot.Blanket.cs:45`), so they are not solid for a frame. Six F1 debug lines that drew on
top of each other now have their own rows. A mis-set vault range no longer throws (`RunVault.cs:104`), and a
broken translation file falls back to English (`Core/Text/TextTable.cs:62`).

## 4. Verification

- `dotnet build` of Unity's generated project files, with all output sent to a scratch folder so nothing in
  `Game/` was written: **Abandoned.Runtime, Abandoned.Editor, Abandoned.Tests.EditMode and
  Abandoned.Tests.PlayMode all build with 0 errors.** It caught one mistake of mine (a missing namespace),
  now fixed.
- This is a C# compile only. It does not run Netcode's code generation, so the real check is still:

  ```bash
  Tools/unity.sh compile
  ```

  I did not run it, in line with your standing rule about batch Unity runs.
- Two `CS0649` warnings in `Core/Text/TextTable.cs` appear under plain `dotnet` and not in earlier Unity
  logs. They predate this pass.
- No tests were run. The existing tests do not assert anything these fixes change, as far as reading them shows.

## 5. Pending action items

### Decisions for you (not changed)
1. **A job lost to the total collapse still costs a missed quota.** CLAUDE.md (2026-10-08) says "the job is
   lost (haul 0, save untouched)", but payday runs the ledger with a haul of 0: penalty, a strike toward
   bankruptcy and a counted job. If "save untouched" was meant literally, it is a three-line change in
   `Company/CompanyService.Payday.cs`.
2. **The truck can be overloaded during the honk.** Capacity is only checked when the lever is pulled. With
   the haul now live during the honk, the display will say OVERLOADED and the truck still leaves.
3. **Bankruptcy clears the loot catalogue and trophy shelf.** Consistent with GDD 13, but worth a conscious yes.
4. **Other gear still works while hiding** (medkit, noise maker). Only the drone is blocked.

### Needs your eyes in play
5. Vents: crawl, then have the host start the next job or return to HQ mid-crawl; die mid-crawl (dev hurt key).
6. Winch: a Heavy item from the lot up the ramp into the bay.
7. Freight lift: riding up and down as host and as client, and an overloaded drop.
8. Rush orders, overtime and the difficulty switch: check the appraisal lines and that the board, van and HUD
   show the same quota on Relaxed and Brutal.
9. Hire radio: the respawn uses the truck's root position; confirm the new hire lands in the bay.
10. Ringing loot as a client, with the host confirming monsters react at the same moment.
11. Photo mode (F9, Enter) on the Mac build: the first photo may raise a privacy prompt.
12. The 0.22 UI restyle remains unverified at every resolution and UI scale.

### Larger items flagged, not rewritten
13. `NetworkList` fields are created at declaration in five classes (alarms, breakers, chalk, pings,
    catalogue). Netcode's guidance is to create them in `Awake`; watch the console for leak warnings.
14. Spray and ping requests have no host-side rate limit. Fine among friends; worth adding before public lobbies.
15. Voice allocates a byte array per packet while someone talks (about 50 a second).
16. In-place "next run" (dev only) leaves crash mats, floodlights and drones from the previous run.

### Suggested commit message
```
0.24.7: static QA pass (rush/overtime/difficulty pay, vent and drone states, synth race, leaks)
```
