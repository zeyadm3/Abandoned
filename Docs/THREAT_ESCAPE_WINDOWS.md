# Monster escape windows — 18 October 2026

Status: written; not compiled, run, or playtested. The project rule reserves game runs and verification for the user unless requested. No scene or prefab rebuild is needed for these changes. No version bump or player builds.

The reported problem was continuous monster pressure making corner or locker waiting the only viable play. There were two causes in the code: the Stalker and several roaming monsters used living players' current coordinates without a fresh sighting, and pacing aggressively drifted/replaced monsters near the crew. The Blind One also kept investigating an unreachable sound indefinitely because its search only finished after arriving.

## Resulting encounter loop

An ordinary monster detects the player, pursues or searches a remembered position, then withdraws. Breaking sight no longer supplies a moving target through walls. A failed search gives the player a chance to leave cover and resume hauling. Continuously refreshed encounters also have a 25-second limit, followed by 25 seconds of withdrawal without acquisition or attacks from that creature. This is per monster, not global player invulnerability; other monsters and environmental hazards remain active.

- **Stalker:** acquires only visible players within its watch range. Without sight, it checks its last known position for six seconds, clears dread/prey warnings, and leaves. Looking at it still freezes it; a sustained encounter eventually makes it withdraw so a solo player is not permanently pinned. A hit or unreachable rush also ends the encounter.
- **Hunter:** checks its last sighting, searches within a six-second lost-sight budget, then leaves. Its door breach still works and the whole engagement is bounded by the same 25-second cap. Falling/stunning and lethal attacks retain their existing rules.
- **Blind One:** still hunts by sound. An investigation expires when the sound memory expires even if it cannot reach the noise. A finished investigation or a continuously refreshed encounter leads to withdrawal, with old hearing history cleared.
- **Crawlers:** acquire by sight and search the last known position after losing sight. Flashlight scattering retains its existing duration and then leads into withdrawal. Light, lockers, carried-loot knock-offs and vent concealment retain their special rules.
- **Thing:** flashlight exposure still wakes it. It follows the last position it could perceive rather than live unseen coordinates, and withdraws when fixation ends or it lands a hit. Continuous exposure cannot extend an encounter forever.
- **Weight:** can follow only a visible nearby player. It withdraws after a structural strike or losing its prey, rather than repeatedly following the same player across the map.
- **Collector / Last Hunter:** keep their existing theft and final-overstay rules respectively. The Last Hunter does not use withdrawal.

Withdrawal selects a complete NavMesh route away from the encounter and crew, favouring destinations behind cover. It does not teleport. A disconnected or enclosed NavMesh can still prevent a physical retreat; those map cases need a playtest. F1 on the host shows the withdrawal count, and Scene-view gizmos draw retreat destinations in cyan.

## Starting balance

All values are in `Game/Assets/_Project/Data/Threats/Resources/ThreatPacingConfig.asset`; matching defaults are in `ThreatPacingConfig.cs`. The existing mall builder's `ThreatPressure0156.Apply` uses these values too, so rebuilding the mall will not restore the previous pressure settings.

| Setting | Previously | Now |
| --- | --- | --- |
| Base opening delay | 12 s | 30 s |
| First-job minimum delay | 20 s | 45 s |
| Opening population, LOW / MEDIUM / HIGH | 2 / 3 / 4 | 1 / 1 / 2 |
| Base interval between reinforcements | 28–42 s | 60–95 s |
| Solo alive cap, LOW / MEDIUM / HIGH | 4 / 5 / 6 | 2 / 3 / 4 |
| Maximum ordinary monsters for any crew | 10 | 6 |
| Time before a distant monster drifts toward crew | 10 s | 40 s |
| Far-away relocation delay | 55 s | 120 s |

Contract threat level and difficulty still scale arrival intervals. Danger still unlocks additional arrivals, subject to the alive cap. Retreating monsters count toward that cap, so withdrawal does not immediately free a slot for a replacement. Pacing ignores concealed/sheltered players when choosing drift destinations and resets its drift/relocation clocks while a monster withdraws.

## Unity check (under five minutes)

1. Let Unity import the scripts, open the Mall scene, and enter Play mode as a solo host. No builder menu is needed.
2. Let a Stalker acquire you, then get behind a solid wall or enter a locker after breaking sight. It should check its last known position for about six seconds, clear the prey warning, and walk away. Moving out during its recovery should not instantly reacquire you.
3. In a separate encounter, keep looking at the Stalker. It should freeze initially, then withdraw after about 25 seconds rather than force you to keep watching forever. F1 shows withdrawal state; cyan Scene gizmos show the destination.
4. Repeat a broken-sight escape from the Hunter or Crawlers, and lead the Blind One to a noise behind a shut/unreachable route. Each should abandon its lead and leave rather than keep camping. Repeat with a joined client when testing co-op; the host makes all AI decisions.

Regression tests added, not run: `BlindOneTests.AnUnreachableNoiseIsForgottenWithoutArriving`, `BlindOneTests.WithdrawalClearsNoiseHistoryBeforeTheNextEncounter`, and PlayMode `ThreatEscapeTests` for lost-sight recovery/reacquisition and bounded solo Stalker attention.

Suggested commit after the user's check: `Give monsters bounded pursuits and real escape windows`.
