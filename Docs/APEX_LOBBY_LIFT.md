# Cloudline lobby and passenger elevator

24 October 2026. Lobby reference: the polished corporate elevator hall in Suits, interpreted for Cloudline rather than copying fictional signage. Pale bookmatched stone, walnut wall panels, champagne metal doors, a long concierge desk, linear ceiling lights and ivory executive sofas replace the former scattered hotel furnishings. The actual central lift is flanked by closed private bays; decoration never crosses its doorway or the fire-stair approach.

The cab now has a stone floor, walnut liners, brushed metal reveals, back mirror, tubular handrails, a luminous ceiling, labelled L/M/A controls and a live height/destination display. Cab and landing doors slide into pockets. The host holds departure until the doors have closed, and opens a landing only after the car is stopped and latched there; loss of power between floors leaves the cab doors shut.

## Falling through the lift

Reproduced with the real Player prefab: at a 100ms frame, the old motor searched below its previous position after the cab moved upward, lost its supporting collider, and fell through the deck. The motor now remembers the previous step's support, carries the player before their own movement, then reacquires support. It clears the cached contact when jumping, walking off or resetting after a teleport. The carry sweep temporarily ignores its own moving floor and cab shell so a large height update cannot trap the rider below the floor or on top of the roof; external obstacles still block movement. MovingPlatform explicitly synchronizes moved colliders before character queries, and records the first displacement correctly.

Verification commands:

- `Tools/unity.sh playmode ElevatorRideTests` — full 404m rides at 60/30/10/4 fps with a stationary arrival hold.
- `Tools/unity.sh playmode SkyscraperJourneyTests` — saved map, real solo host, actual RPC and door interlock, restaurant arrival and walking out across the threshold.
- `Tools/unity.sh editmode SkyscraperRebuildTests` — saved-map circulation and authoring checks.

Reapply: **Tools → Abandoned → Maps → Skyscraper Restaurant → Rebuild Skyscraper Restaurant**. The saved map and world model are rebuilt together. No packages added.

The hosted check also exposed duplicate NetworkLoot components on heavy Skyscraper salvage: NetworkSharedCarry had already added the required component before the builder added another. The builder now reuses it, and repairs existing prefabs so each has one configured instance. This removes the collision exception that interrupted real sessions.

PlayMode validation: 27/27 passed — six platform regressions, twenty existing movement checks and the hosted round trip. The saved-scene test uses an accelerated 80 m/s trip while retaining real door timing; the platform regressions use the normal 8 m/s speed over all 404 metres. Cross-machine multiplayer remains an interactive verification item.

Visual reference: [Suits lobby still](https://moviemaps.org/images/1zf5). Suggested commit: `Rebuild Cloudline lobby and fix elevator passenger support`.

Saved-map validation: 49/49 EditMode checks passed, including each landing sill, the unobstructed lobby approach and repaired salvage prefabs. Twenty review captures cover the saved destination, including the new lobby, cab and executive seating.
