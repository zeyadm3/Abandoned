# Open world plan: driving between POIs (2026-10-23)

The user: "prepare the game to become an open world game with player driving in between POIs". This is a plan
for review, not a build. It changes locked decisions (CLAUDE.md: hand-built levels loaded per job, joining only at
the HQ, travel by the van's travel screen), so nothing is built until the questions at the end are answered.

## What exists today
- Each job is its own scene (Mall, School, Factory, Prison, Tutorial), loaded by `SessionTravel` on every machine.
  The HQ is the hub; the van "drive" is a travel screen, not driving.
- Host-authoritative structure, loot, monsters, money, quota; clients own their movement. NGO, no scene management.
- Each map now has photographed sky, scanned trees and edge terrain (Docs/MAP_SITES.md), which helps a shared world.

## Proposed shape
1. **One world scene** with a large terrain (several km) and roads, built like the TestMap site (`ShowcaseTerrain`,
   `fetch_nature.py`): forest, hills, a road network.
2. **POIs** = today's maps, placed in the world. Two ways (decision 1):
   - A. **Streamed:** each map stays its own scene, loaded additively when the van nears it, unloaded when it
     leaves. Keeps every builder, test and structure system as is. Recommended.
   - B. **Merged:** every map copied into one scene. Simpler at run time, but huge scene, every builder and test
     changes.
3. **Driving:** a drivable company van (host-authoritative `NetworkObject`, `WheelColliders` in Unity physics, driver
   input sent by RPC, passengers seated; Cinemachine camera). Loot rides in its cargo bay as today's truck does.
4. **Jobs:** the contract board picks a POI; the run starts when the crew arrives there, and the existing run flow
   (quota, extraction at the truck, decay clock) runs at that POI. Driving back to the HQ ends the job.
5. **Networking:** floating origin or keeping the world under ~5 km so precision holds; NetworkObjects of a POI spawn
   when it streams in (the "levels spawn their objects dynamically" rule already fits).
6. **Performance:** terrain LOD, tree impostors (already have them), POIs only loaded near the crew, occlusion.

## Order of work (each step small enough to test, per CLAUDE.md)
1. World scene with terrain and roads, HQ placed in it, nothing else.
2. Drivable van, solo, host only.
3. Van over the network (driver + passengers).
4. Stream one POI (the Mall) in and out by distance.
5. Contract → drive → run at the POI → drive back → payday.
6. Remaining POIs, map-to-map roads, polish (signs, radio, fuel?).

## Decisions (user, 2026-10-23)
1. POIs are their own scenes, **loaded in by distance** (option A).
2. **Driving is the only way** to reach a salvage location; the travel screen goes.
3. Players still **join only at the HQ**.
4. **Monsters, hazards and jump scares on the roads** too.
5. **Fuel, van damage and breakdowns: yes.**

## How it is built (Claude CLI, 2026-10-23)
Menu: **Tools > Abandoned > Maps > Open World** (priority 1900): Rebuild World, Build Van, Build Van Test Scene.

### The moving origin (why no map scene is ever moved)
Every map is built around its own 0,0,0, uses static batching, a baked NavMesh and absolute coordinates in its configs
(HorrorConfig, threat spawns, kill volumes). So a place's scene always loads **exactly where it was built**, and the
**world moves instead**: `WorldState` (on the van's NetworkObject) holds the host's **anchor**, the place whose origin is
the game's origin; `WorldRoot` (terrain, roads, site stand-ins) sits at minus that place's world position. Only the
anchor's scene is ever loaded. When the van has driven past the anchor's unload distance and no machine still has it
loaded, the host makes the nearest place the anchor; every machine moves the land, then everything it has authority
over (host: van, loot, monsters; each owner: their player) by the same step. Side benefit: coordinates stay small
(no float precision trouble across the 6 km world).

### The world (Scenes/World, `Editor/World/WorldBuilder`)
- 6 x 6 km of wooded hills, 36 joined terrain tiles (2 m heights) from one grid (`WorldTerrain`), painted, planted and
  grassed with the TestMap's own scans, trees and grass, read from its saved terrain, never changed (`WorldTerrainPaint`).
  The TestMap's photographed sky and grade (profile copied to Data/World/World_Volume). Realtime light only.
- Places (`WorldPlan`, written to Data/World/Resources/WorldAtlas): HQ at the world origin; Mall, School, Factory,
  Prison, Hospital, Motel, Police Station, Fire Station, Skyscraper about 2 km apart. Each site is levelled to its
  scene's ground with a **terrain hole** over that ground (its own ground and basements show), and a flat **stand-in**
  fills the hole while the scene isn't loaded (from far away, and for places not built yet).
- Roads (`WorldRoads`): one from the HQ's district side facing each place, graded through the hills, asphalt mesh with a
  dropped edge, dashed centre line, a collider per 200 m.
- Streaming (`PoiStreamer`, every machine): the anchor's scene loads within its load distance (700 m from its centre)
  and unloads past 900 m. A loaded scene is fitted in (`PoiAdapter`): its camera, audio listener, sun, global
  volumes and far horizon (`hideInWorld`) are switched off; the world's are used. **Each map's own hour and grade are
  not used in the world yet** (one sky for all; a per-place lighting zone is a possible next step).
- Readiness: `SessionTravel.StreamingReady` (new hook): while the anchor's scene is loaded on the host but not yet on
  every machine, the level directors in it hold their spawns, as they do for a level load.

### The van (Prefabs/World/WorldVan, `WorldVanBuilder`)
- The collection truck's body (the truck every job is loaded into), rebuilt into its own meshes with the wheels cut
  out to turn and steer; four built-in WheelColliders, rear-wheel drive, anti-roll bars (`VanDrive`).
- Host-authoritative (`WorldVan`): the driver's machine sends throttle/steer/handbrake/horn by unreliable RPC; the host
  simulates and writes `VanNetState` (fuel, damage, engine, broken down, ramp, lights, steering) for every machine.
- Seats (`VanSeats`): 0 driver, 1-2 in the cab, 3 on a bench in the cargo box; E at a cab door or the bench. The host
  writes who sits where; each player's own machine pins them to the seat and turns their view with the van.
  Controls: W/S drive, A/D steer, Crouch handbrake, Use horn, E get out.
- The ramp slides in under the deck and the rear doors shut whenever someone is at the wheel (`VanTailgate`).
- Fuel, damage and breakdowns (`VanCondition`, tunables in Data/World/Resources/VanConfig): hard knocks damage it, a
  breakdown is rolled per km (likelier when damaged), an empty tank or a breakdown stops the engine; pointing at a
  broken-down van offers the roadside repair (stay by it for 12 s).
- F1: the van's numbers (`VanDebugView`) and the world's anchor and places (`WorldDebugView`).
- Test scene: Scenes/WorldVanTest (a pad with a ramp, bumps, a slalom and a wall). `VanTestDriver` (dev only) holds
  the controls from a script for editor checks.

### Status
1. Done: World scripts compile (42e520ba).
2. Done: drivable van, solo/host, test scene (b215c2af).
3. Done: World scene; the HQ streams in round the van at the start; driving 1.4 km away unloads it and moves the
   origin to the next place, van and seated player carried over (checked in Play mode, editor captures in
   Game/Logs/renders/open-world).

## Upcoming salvage locations (user, 2026-10-23)
New POIs for the open world, each its own scene streamed by distance like the current maps:
1. **Hospital** (also listed in the 2026-10-06 decisions as a post-Early-Access candidate).
2. **Motel**.
3. **Skyscraper restaurant**: the restaurant is high up inside a skyscraper; the crew must take the **elevator** to
   reach it (and to get loot back down). The lift system (`LiftStart`/`LiftArrive`/`LiftSnap` sounds exist) is the
   natural base. **Stairs too** (user, 2026-10-23): a stairwell runs the full height as the slow way up and the way
   down when the lift is out; carrying heavy loot down many flights is meant to be hard. What makes the lift fail
   (power, damage, monsters) is still to decide.
4. **Police station**.
5. **Fire station**.
Each needs a plan of its own (layout, structure/collapse, loot, threats, hiding) before it is built.

## Original questions
1. Streamed POI scenes (A) or one merged scene (B)?
2. Does the world replace the HQ travel screen entirely, or is driving optional?
3. Still join only at the HQ?
4. Do monsters or hazards exist on the roads, or only at POIs?
5. Fuel, van damage, breakdowns: yes or no?
6. New packages: none planned (WheelColliders are built in). OK?
