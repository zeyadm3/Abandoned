# Skyscraper Restaurant (Cloudline Tower) — Plan & Implementation

## 1. Concept & Setting
**Location:** Cloudline Tower & The Apex Restaurant  
**Setting:** The upper floors and roof crown of an abandoned 1980s corporate skyscraper towering over the district.
The crew's van parks at street level in the forecourt plaza. The salvage target is **The Apex Restaurant & Sky Lounge** at the tower's summit, a high-roller venue left frozen in time with vintage wines, silver services, commercial kitchen equipment, and executive vaults.

### Verticality & The Core Loop
The Skyscraper is built around high-stakes vertical salvage:
- **Floor 0: Street Level & Plaza (y = 0 m)**: Forecourt arrival plaza, van parking spot, company extraction truck, revolving doors, grand marble lobby, reception desk, turnstiles, and the dual vertical access points: the **Elevator Bank** and the **Emergency Fire Stairwell**.
- **Floor 1: Intermediate Service & Mechanical Deck (y = 20 m)**: Mid-tower technical level. Houses the elevator hoist motors, HVAC air handlers, high-voltage transformer cages, electrical switchgear (Sector 1 breaker), and catwalks overlooking the elevator void. Also features the mid-tower stairwell landing and the emergency elevator shaft escape doors.
- **Floor 2: The Apex Restaurant & Sky Lounge (y = 44 m to 52 m)**:
  - **Sky Lobby & Reception (y = 44 m)**: Elevator foyer, coat check, maître d' station.
  - **Grand Dining Room (y = 44 m)**: Panoramic floor-to-ceiling curtain glass overlooking the abyss and distant skyline, white linen banquet tables, fallen chandeliers, central marble statuary, and deteriorating cantilevered floor slabs.
  - **Cocktail Lounge & Bar (y = 44 m)**: Curved mahogany bar, back-bar mirror displays, brass glass racks, espresso machines, leather lounge armchairs.
  - **Commercial Kitchen & Prep (y = 44 m)**: Stainless steel ranges, fryers, prep tables, dishwashing station, hood vents, staff lockers.
  - **Walk-in Cold Store & Pantry (y = 44 m)**: Heavy insulated refrigeration door, meat hooks, wire racks, chilling compressor.
  - **Private Dining (The Penthouse Suite, y = 44 m)**: Elite private dining room, boardroom table, private cellar display, executive wall safe.
  - **Observation Terrace (y = 44 m, outdoor)**: Cantilevered observation deck with safety railings and wind shields over the city void.

---

## 2. The Elevator & Stairwell System

### Elevator Mechanics (NGO Host-Authoritative)
The elevator is the crew's lifesaver and their potential coffin:
- **Cab & Platform:** Enclosed steel-and-glass cab riding vertical guide rails. The floor of the cab carries an active `MovingPlatform` component.
- **Three Stops:**
  - Floor 0: Street Lobby (y = 0 m)
  - Floor 1: Mechanical Deck (y = 20 m)
  - Floor 2: Apex Restaurant (y = 44 m)
- **Call & Send Controls:** Interactive `SkyscraperElevatorButton` (`IUsable`, `IHintSource`) on each landing to call the lift, plus a 3-button car panel inside the cab to select destination floors. Requests invoke server RPCs validated by the host.
- **Landing Gates:** Heavy automated industrial elevator doors on each landing that slide open only when the cab is parked at that floor, preventing players and loot from tumbling into the open shaft during transit.

### What Makes the Lift Fail (Proposed Decisions)
The prompt specifically instructs: *"You decide what makes the lift fail (power, damage, monsters, or a mix) and record it as a proposed decision."*
We propose a four-tier failure system:
1. **Weight Overload & Cable Snap (`LiftSnap`)**:
   - Safe operating load is **450 kg** (players + carried loot + loose items on deck, calculated via `LoadSources.All`).
   - If the crew greedily loads heavy salvage (or multiple players carry the 340 kg Grand Vintage Wine Cask onto the lift together), the cables snap midway through the trip.
   - The cab free-falls down the shaft, triggers the `LiftSnap` and `Crash` sounds, deals crushing drop damage (40 HP) to everyone inside, and permanently breaks the lift for the remainder of the run.
2. **Power Blackouts & Sector Danger**:
   - The lift motor is powered by **Sector 1 (Mechanical Deck)**.
   - When overall run danger increases or a blackout event hits Sector 1, the lift halts immediately in the shaft.
   - If stalled between floors, the crew must exit via the cab's emergency roof hatch into the shaft maintenance ladders or step onto the Floor 1 mechanical catwalks to reach the stairwell.
   - Power can be restored by hiking down to the Mechanical Floor (y = 20 m) and throwing the Sector 1 breaker.
3. **Monster Audio Attraction**:
   - Elevator motor operation emits heavy mechanical hums and cable friction noise (`NoiseSystem.Emit(lift.Deck, 0.75f, NoiseSource.Other)`), alerting stalking threats. Monsters frequently wait outside landing doors when the lift arrives (`LiftArrive`).
4. **Structural Creep & Shaft Jamming**:
   - Tower sway and structural fatigue introduce a seeded door-jam hazard on late-game danger stages, requiring manual pry interactions to open stuck doors.

### The 44-Metre Fire Stairwell
- Runs continuously from Street Level (y = 0 m) to the Restaurant (y = 44 m) within a reinforced concrete core.
- **11 continuous stair runs** with 180-degree switchback landings every 4 vertical metres.
- Fire break doors at Ground Lobby (y = 0 m), Mechanical Floor (y = 20 m), and Restaurant Sky Lobby (y = 44 m).
- **Carry Difficulty:** Heavy and Huge loot cannot be sprinted with. Dragging a 220 kg safe down 11 flights of tight concrete stairs requires navigating corners, managing stamina exhaustion, and fending off monsters echoing in the vertical stairwell. Several landings feature weakened concrete that cracks under heavy impact.

---

## 3. Structural Sections & Collapse Design
Using `StructureSceneSetup` and `StructuralSection`:
- **Dining Room Window Slabs**: 4 m x 4 m floor sections along the northern panoramic curtain wall are weakened by years of rain intrusion and thermal shock (`health = 0.65f`, `capacity = 0.55f`). Walking across while carrying heavy loot causes visible fracture stages and eventual collapse into the 44-metre drop below.
- **Mechanical Floor Catwalks**: Steel grating walkways (`SectionType.Walkway`) over the central elevator machinery void. Overloaded catwalks collapse, dropping salvage into the lower mechanical crawlspace.
- **Stairwell Landing Floor 6 (y = 24 m)**: Weakened landing section that tests squads dragging heavy loot.
- All collapsible sections feature `SectionNavCarver` so AI pathfinding dynamically updates and monster pathing diverts around collapsed floor chasms.
- Safe zones (e.g. truck ramp, lobby elevator entrance) are stamped with `SectionKeepClear` to ensure extraction routes remain navigable.

---

## 4. Power Sectors & Lighting
The building is wired into five distinct power sectors:
- **Sector 0: Street Level Forecourt & Grand Lobby**: Exterior floodlights, facade up-lighting, lobby reception pendants.
- **Sector 1: Elevator Shaft & Mechanical Deck**: Motor room high-bay lights, maintenance bulkheads, elevator cab ceiling fixture, and shaft indicator lanterns.
- **Sector 2: Grand Dining Room & Observation Deck**: Crystal chandeliers, recessed perimeter downlights, terrace wall sconces.
- **Sector 3: Commercial Kitchen & Cold Store**: Industrial fluorescent fixtures, prep station task lights, cold store vapor-proof bulkheads.
- **Sector 4: Penthouse VIP Suite & Private Lounge**: Dim recessed luxury halogens, showcase cabinet backlights, private bar pendants.

Ceiling fixtures are set up with `LightFixture` components (handling functional, flickering/faulty, and burned-out tubes) linked to `PowerController`. Breaker panels are installed in the Mechanical Room (Sector 1) and Kitchen Service Corridor (Sector 3).

---

## 5. Hiding Places (`HidingLocker`)
Ten themed hiding spots using `HidingLocker` and `HideSpotKind`:
1. `HideSpotKind.Cabinet`: Dining room waiter station service credenza #1 (North).
2. `HideSpotKind.Cabinet`: Dining room waiter station service credenza #2 (South).
3. `HideSpotKind.Cabinet`: Lounge back-bar storage credenza.
4. `HideSpotKind.Locker`: Kitchen staff locker #1.
5. `HideSpotKind.Locker`: Kitchen staff locker #2.
6. `HideSpotKind.Locker`: Kitchen staff locker #3.
7. `HideSpotKind.Closet`: Walk-in cold store insulated refrigeration cabinet.
8. `HideSpotKind.Cabinet`: Mechanical floor high-voltage switchgear cabinet #1.
9. `HideSpotKind.Cabinet`: Mechanical floor electrical control cabinet #2.
10. `HideSpotKind.Closet`: Penthouse VIP executive wardrobe closet.

---

## 6. Salvage Loot Catalog (Data/Loot/Skyscraper/)
24 distinct high-value salvage items across 5 restaurant-themed tags:
- `skyscraper_dining`
- `skyscraper_kitchen`
- `skyscraper_bar`
- `skyscraper_cellar`
- `skyscraper_executive`

### Highlight Loot Items
| Item ID | Name | Carry Class | Weight | Value Range | Tag | Description |
|---|---|---|---|---|---|---|
| `antique_silverware` | Antique Silver Cutlery Set | OneHand | 2.5 kg | $600 - $1,800 | dining | Sterling silver 24-piece dinner service |
| `crystal_champagne_flutes` | Crystal Flute Set | OneHand | 1.8 kg | $400 - $1,200 | bar | Hand-blown lead crystal glasses in satin case |
| `vintage_cognac` | 1945 Reserve Cognac | Pocket | 0.9 kg | $1,200 - $3,500 | cellar | Wax-sealed reserve bottle from the private cellar |
| `gold_cocktail_shaker` | Gold-Plated Cocktail Shaker | Pocket | 0.6 kg | $800 - $2,200 | bar | Engraved art-deco triple-plated shaker |
| `copper_cookware_set` | Copper Chef Pan Stack | TwoHand | 14.0 kg | $1,500 - $4,200 | kitchen | Heavy solid copper French saute pans |
| `damascus_knife_roll` | Master Chef Damascus Roll | OneHand | 3.2 kg | $900 - $2,600 | kitchen | Custom Japanese folded steel culinary knives |
| `grand_espresso_machine` | Commercial Espresso Machine | Heavy | 65.0 kg | $4,500 - $9,500 | bar | Dual-boiler brass and chrome Italian machine |
| `caviar_tin_reserve` | Imperial Caviar Tin (1kg) | Pocket | 1.1 kg | $2,000 - $5,000 | dining | Vacuum-sealed chilled gold-leaf tin |
| `vintage_phonograph` | Executive Turntable Console | TwoHand | 18.0 kg | $2,200 - $5,800 | executive | Rare mahogany hi-fi system from the penthouse |
| `bronze_bull_statue` | Bronze Bull Centerpiece (Jackpot) | Heavy | 85.0 kg | $14,000 - $32,000 | executive | Solid cast bronze charging bull statue |
| `executive_wall_safe` | Executive Wall Safe (Jackpot) | Heavy | 220.0 kg | $8,000 - $22,000 | executive | Reinforced alloy safe containing corporate bonds |
| `grand_wine_cask` | Grand Vintage Wine Cask (Jackpot) | Huge | 340.0 kg | $25,000 - $60,000 | cellar | French oak port barrel requiring multi-crew carry |

---

## 7. Threat Spawns & NavMesh Layout
18 threat spawn nodes positioned for vertical pacing:
- Lower shaft pit & ground lobby back-hall
- Stairwell landings at levels 3, 6, 9 (cutting off retreat down the stairs)
- Mechanical deck transformer shadows and catwalk access
- Kitchen dishwashing station and walk-in freezer door
- Dining room curtain wall corners
- Penthouse private dining corridor

---

## 8. What to Check First (Review Checklist)
When loading the map in Unity via **Tools > Abandoned > Maps > Skyscraper Restaurant > Rebuild Skyscraper Restaurant**:
1. **Elevator Operation:**
   - Stand in front of the Lobby Elevator doors at (0, 0, 10). Use the call button. Verify the cab descends/parks smoothly, the landing gates open, and `LiftArrive` plays.
   - Step inside onto the deck. Verify `MovingPlatform` detects feet. Press button 2 (Restaurant). Verify doors close, `LiftStart` plays, and cab ascends 44 metres smoothly to the restaurant.
   - Verify landing gates open at the summit.
2. **Elevator Overload Test:**
   - Place > 450 kg on the elevator deck (or multiple heavy test items). Send the lift.
   - Verify `LiftSnap` triggers mid-flight, the cab plunges downward, and `Crash` triggers upon striking the buffer springs.
3. **Stairwell Navigation & Physicality:**
   - Walk up/down the fire stair tower from y = 0 to y = 44 m. Verify all 11 flights and landings have seamless collider connections without player snagging.
   - Check the fire exit doors at Ground, Mechanical, and Restaurant levels.
4. **Restaurant Floor Atmosphere & Scenery:**
   - Check the panoramic glass view overlooking the skyline and distant foggy mountain basin.
   - Inspect the dining room tables, chandeliers, bar counter, commercial kitchen fixtures, and walk-in cold store.
5. **Structural Integrity & Collapses:**
   - Inspect the designated collapsing dining room floor section near the northern broken window.
   - Confirm `SectionNavCarver` carves the NavMesh when collapsed.
6. **Loot Spawning:**
   - Check that `LootSpawnPoint` tags match the 24 Skyscraper loot items.
   - Verify heavy loot (such as the Commercial Espresso Machine and Executive Wall Safe) can be dragged or carried out to the elevator or down the stairs.
7. **Extraction & Truck:**
   - Verify the company salvage truck sits in the entrance plaza with its rear ramp aligned to the plaza pavement.
   - Confirm carried salvage dropped in the truck cargo bay registers with `TruckCargo`.

---

## 9. Proposed Decisions for CLAUDE.md
The following decisions are proposed for logging in `CLAUDE.md`:
- **Skyscraper Restaurant (2026-10-23):** Cloudline Tower added as an open-world streamed salvage POI (`Scenes/Skyscraper.unity`).
  - Dual vertical transit: Host-authoritative 44 m `SkyscraperElevator` (stops: Ground, Mechanical, Apex Restaurant) with weight-limited cable snap at 450 kg, power dependence on Sector 1, and an 11-flight fire stairwell as the reliable, exhausting alternate route.
  - 5 power sectors (Lobby 0, Mechanical/Shaft 1, Dining/Terrace 2, Kitchen/Cold Store 3, VIP Penthouse 4).
  - 24 restaurant-themed loot definitions (`Assets/_Project/Data/Loot/Skyscraper/`), including the 340 kg Grand Vintage Wine Cask jackpot.
  - Collapsible dining window slabs and mechanical catwalks with dynamic `SectionNavCarver` path updates.
