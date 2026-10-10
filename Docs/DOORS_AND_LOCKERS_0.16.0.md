# 0.16.0 — closable doors and hiding lockers

Written 2026-10-10. **Code and Editor tooling only: nothing was compiled, run or tested, and the mall
scene has not been re-saved yet.** Run the menu item below once, then play.

## What to do in Unity

1. Open the project and let it compile.
2. **Tools > Abandoned > Mall > Add Doors and Lockers (0.16.0)**. It opens the Mall scene, creates
   `Data/Extraction/Resources/DoorConfig.asset`, adds `RunDoors` to the RunState prefab, adds the six new
   sound rows to the sound bank, places 10 doors and up to 27 lockers under `Mall/_ROOMS_0160`, and saves.
   It does not bake the NavMesh or occlusion (neither needs it) and makes no builds.
3. Read the Console: `[Rooms0160] N hiding lockers placed, M left out` and one warning per locker it
   refused to place (with the reason). `M` should be 0.
4. Play a job from the HQ.

Batch mode, Unity closed: `-executeMethod Abandoned.EditorTools.MallRooms0160.AuthorAndExit`.
`Tools/unity.sh rebuild-horror` also places them (the full mall builder calls the same code).

## The rules, as a player meets them

**Doors**
- E on a door slides it open or shut. It will not shut on a player or loot standing in it.
- A shut door blocks movement, thrown loot, sight and sound (walls dull sound, and so does a door).
- Each job starts with a seeded few shut: a store's doors 25 % of the time, a service door 50 %.
- Monsters cannot open doors. A shut door is a wall to them, and the NavMesh is cut there while it is shut.
- **The Hunter** breaks down a door that is shut between it and the place it last saw you, if the door is
  within 12 m: it walks up, winds up, and lands 3 blows 1.3 s apart (about 4 s in all). Every blow is heard
  across the building and by anything that listens. The door then hangs open, broken, for the rest of the job.
  If it lost you before you went through the door, it never comes to the door at all.
- **The Last Hunter** breaks any shut door within 3 m of it at once.

**Lockers**
- E in front of a locker with empty hands (pockets are fine) climbs in. One player per locker.
- Inside you cannot move; you look out through the slats (the view turns 55° each way); E or Jump climbs out.
  Flashlight, radio and voice still work. Stamina recovers.
- Nothing that hunts by sight finds you, **unless it was after you and watched you climb in** (within 16 m,
  clear line of sight at that moment). Then it comes to the locker and drags you out.

| Monster | A shut door | A locker |
|---|---|---|
| Blind One | Stops it | Safe while silent. It strikes whoever it has just heard there (the locker door itself is a small noise) |
| Stalker | Stops it | Loses you and waits where it stands. If it was already rushing and saw you go in, it reaches in. Looking at it through the slats still freezes it |
| Collector | Stops it | A hidden player does not count as guarding loot or as scaring it |
| Hunter | Breaks it down if it saw you go through | Safe unless it watched you climb in |
| Crawlers | Stops them | No shelter: they get in, bite, and you are out |
| The Weight | Stops it | It ignores hidden players |
| The Thing | Stops it | Safe unless it was awake, coming for you, and watched you climb in |
| Last Hunter | Breaks it | No shelter |

The threat files (HQ field guide), the Hunter tip and two new one-time tips (Door, Locker) say the same.

## Where they are

**Doors (10).** No wall was cut: every door stands in an opening the 0.15 mall already has.
- 8 store doors: a pair of sliding panels in each store's four-metre doorway, on the store's side of the
  roller shutter, in the tenant's fascia colour. Open, the panels lie against the inside of the storefront
  either side of the doorway. The upright pull on each panel's leading edge shows in the opening, so a
  door can be slid shut from the concourse as well as from inside.
- 2 service doors: one steel panel across the mouth of the service corridor (stairs and haul ramps) on each
  floor, at x = 43.8, z = 32.1 to 35.6 (36.0 upstairs). It slides north and parks beside the stairs.

**Lockers (up to 27, in 15 banks).** Wall runs that were bare in the saved 0.15.8c scene, clear of loot
pads, monster spawns, doorways, flights and carry lanes:
- Ground: service corridor south wall (2) and east end (1), Titan Tools east wall (2), Nordic Habitat east
  wall (2), Apex Circuit west wall (2), Vogue Thread west wall (2), food court north wall (2).
- Upper: service corridor south wall (2), cinema north wall (2) and west wall (1), Toy Archive south wall
  (2) and east wall (2), Nordic Habitat east wall (2), closed offices west wall (1).
- Basement: west wall (2).

Each site is checked again when it is built (solid things, dressing, floor, carry routes, spawn points).
One that fails is left out and reported, never forced in.

## How it is built

- `Extraction/Doors/SlidingDoor` and `Extraction/Lockers/HidingLocker` are plain scene objects on every
  machine, like `RollerShutter`. Neither is a NetworkObject.
- `Extraction/Doors/RunDoors` (on the RunState prefab, next to `RunShutters`): two host-written 64-bit
  masks, shut and broken. A player's E is an RPC; the host checks reach, cooldown and the doorway, then
  flips the bit. Doors animate locally from the masks. A new run is a new RunState, so doors reset.
- Hiding is one host-written value on the player (`NetworkPlayer.HidingIn`, in `NetworkPlayer.Hiding.cs`).
  The owner's machine moves its own player in and out (players own their movement), switches its motor
  and interactor off meanwhile, and limits the view. No change to the Player prefab.
- Monsters: `Threat` keeps who it watched climb in; `DamageWithinReach`, the Hunter's sight and
  `RoamingThreat.Nearest` skip hidden players it does not know about. The Hunter's door-breaking is
  `Threats/Hunter.Doors.cs` (new replicated state `HunterState.Breach`).
- A locker is one solid box, so a monster's line of sight stops at it with no special case. A shut door
  has a blocker thicker than its panels.
- NavMesh: nothing rebaked. A shut door enables a carving obstacle (as shutters do); each locker carves its
  own footprint all the time.
- Tunables: `Data/Extraction/Resources/DoorConfig` (code defaults until the asset exists).
- Sounds: six synthesised cues (`Audio/Synth/DoorPatches`): rollers, thuds and sheet metal. No creak:
  a creak still only ever means the floor.
- F1: gizmos on doors (green open, red shut, orange broken) and lockers, and a `DOORS` line under `SHUTTERS`.
- Tests written, not run: `Tests/EditMode/DoorTests`, and the nettest scenario `rooms`
  (`Tools/unity.sh nettest rooms`).

## Things to look at first when testing

1. Compile errors: none of this has been through the compiler.
2. The Console after the menu item: lockers left out.
3. Store doors against the roller shutters: they share a doorway (shutter in the wall plane, door 24 cm
   inside it). Check a locked store with shut doors reads sensibly from both sides.
4. The service doors beside the stairs: the parked panel and its track run along the stair rail. The stair
   model may stand a few centimetres proud of its collider there.
5. Standing in a locker: the view through the slats, and whether a crewmate's body shows through the door.
6. The Hunter at a door: that it reaches the door, the blows are heard, and it carries on through.
7. Four-player performance is unmeasured, as before. Doors and lockers add about 170 renderers.

## Not in 0.16.0

- No hinged doors and no new openings. Cutting back doors between stores needs a NavMesh and occlusion
  rebake, and the walls there are lined with dressing that is merged per floor tile.
- Lockers do not lock, and a monster never checks one at random.
- TestMap and the HQ are unchanged.
