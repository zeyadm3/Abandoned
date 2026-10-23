# Skyscraper Restaurant (Cloudline Tower) — Plan & Implementation

## 1. Concept & Setting
**Location:** Cloudline Tower & The Apex Restaurant  
**Setting:** The upper floors and roof crown of an abandoned 1980s corporate skyscraper towering over the district.
The crew's van arrives at street level along Broadway Avenue in the forecourt plaza. The salvage target is **The Apex Restaurant & Sky Lounge** at the tower's summit, a high-roller venue left frozen in time with vintage wines, silver services, commercial kitchen equipment, executive vaults, and a vertiginous roof terrace with a window-cleaning gantry.

### Open World Integration
- **Coordinate Authoring:** Authored at the origin `(0, 0, 0)`. The map scene is loaded additively without moving its root.
- **Van Parking Spot:** `(0, 0, -22)` facing North into the grand entrance canopy. Marked with white parking bay lines and yellow hazard diagonal hatching.
- **Ground Cutout Rectangle:** `[-24.0, -35.0]` to `[24.0, 22.0]` (48 m wide by 57 m deep).
- **Edge Elevation:** `0.0 m` meeting the surrounding open world terrain smoothly.
- **Objects for World Streaming to Hide:**
  - `Exterior/Truck` (in open world, the crew's van carries `TruckCargo`)
  - `Exterior/Skyline` (surrounding metropolitan city skyline authored for standalone viewing)
- **Natural Scenery:** No custom sky, trees, or grass added; all atmospheric scattering and horizons are managed by `Art > Upgrade New Maps` (`AvOverhaul`).

---

## 2. Surrounding Streetscape, Forecourt Plaza & Urban Scenery

### Broadway Avenue Streetscape (`SkyscraperPlaza`)
The street surrounding Cloudline Tower is a fully realized urban sector:
- **Two-Lane City Avenue (`z = -34` to `-22`)**: Paved with `Asphalt031`, complete with double-yellow centerlines, pedestrian crosswalks, drainage manhole covers, and storm sewer grates.
- **Modern Sodium Street Lighting**: 4 tall steel street lamp posts with dedicated downward spot illumination casting warm pools of light across the pavement.
- **Abandoned Vehicles**:
  - *Executive Towncar* parked along the curb.
  - *Yellow Taxi Cab* with illuminated roof taxi sign, stalled diagonally in the intersection.
  - *Courier Delivery Van* abandoned mid-dropoff.
- **East Barricade**: Concrete Jersey barriers and a rusted commercial dumpster blocking off through-traffic.

### Cloudline Forecourt Plaza (`z = -20` to `-10`)
Pedestrian approach square paving the transition between Broadway Avenue and the tower entrance:
- **Paved Square**: Interlocking stone paving (`PavingStones126A`).
- **Drained Tiered Reflecting Pool**: Dark granite coping rims, dry concrete basin floor, murky water puddle (`WaterMurky`), and a central 5-tier abstract brass spiral sculpture (*The Ascendant Spiral*).
- **Architectural Planters & Landscaping**: Heavy concrete troughs with soil and weathered, withered trees flanking the walkway.
- **Plaza Amenities**: Teak-slat and steel plaza benches, corporate granite monument sign (*"CLOUDLINE TOWER / 1200 BROADWAY"*), and chrome flagpoles with burgundy corporate pennants.
- **Underground Parking Garage Ramp**: Concrete retaining walls, 15-degree asphalt descent slope, barrier gate arm, and guard security booth.
- **Van Extraction Bay**: Company parking stall directly behind the street sidewalk with yellow diagonal hazard striping and `VanParking` survey marker.

---

## 3. Surrounding Metropolitan Skyline (`SkyscraperSkyline`)

Authored under `Exterior/Skyline` to provide breathtaking urban depth whether looking down from street level or gazing out the 44m restaurant curtain glass:
1. **Trans-Continental Tower (North-West, 85m)**: Stepped Art-Deco skyscraper with setback tiers, vertical window mullions, roof spire, and a pulsing red aviation beacon.
2. **Metropolitan Financial Plaza (North, 72m)**: Modern glass twin towers connected by an enclosed pedestrian skybridge at y = 42m (directly aligned with the Apex Restaurant's elevation), topped with industrial rooftop cooling towers.
3. **Vanguard Corporate Monolith (North-East, 92m)**: Imposing dark granite corporate monolith with angled glass crown and summit aviation beacon.
4. **Federal Reserve Building (West, 38m)**: Heavy brutalist concrete colonnade structure with deep window reveals.
5. **Grand Central Highway Viaduct (South-West, 8m elevation)**: Concrete elevated freeway overpass spanning across the horizon, carrying abandoned stalled sedans and vans.
6. **Suspension Bridge Anchor Tower (East, 110m)**: Towering steel suspension bridge cable pylon with cross-portals and navigation warning lights.

---

## 4. Tower Facade & Grand Marble Lobby

### Architectural Shell (`SkyscraperTowerShell`)
- **Fluted Base Colonnade**: 6m tall dark granite structural piers along the street-level south facade.
- **Grand Porte-Cochère Canopy**: Cantilevered steel space-frame truss with structural glass skylight, bronze fascia lettering, and suspended downlight lanterns.
- **Tower Massing**: 4 massive 50m corner concrete pylons anchoring the tower footprint.
- **Curtain Glass & Mullions**: Full-height curtain glass enclosure on North, East, and West faces, articulated with vertical architectural mullions every 4.5m and horizontal structural spandrel bands at every floor level.
- **Rooftop Crown**: Reinforced concrete roof slab (y = 50m) and continuous steel parapet coping.

### Grand Double-Height Marble Lobby (`SkyscraperLobby`)
- **Flooring**: Polished Carrara marble field (`Marble012`) framed with dark granite border inlays (`Concrete034`).
- **Coffered Ceiling**: Acoustic coffer tiles at y = 6.0m.
- **Interior Colonnade**: 4 double-height fluted structural pillars with polished brass plinths and capitals.
- **Entrance Vestibule**: Double-height exterior glazing and a revolving 3-wing glass cylinder door flanked by steel egress portals.
- **Concierge Reception Desk**: Curved dark granite front, polished brass countertop, mahogany workstation, reservation register ledger, and key card storage rack.
- **Security Screening Checkpoint**: 4 optical glass-flap security turnstiles, walk-through metal detector portal with indicator lamps, and baggage screening station.
- **Executive Waiting Lounge**: Black leather executive sofas, armchairs, smoked-glass brass coffee table, and an architectural potted ficus tree.
- **Tenant Directory Monument**: Polished black granite monument plaque with etched brass directory listing all tower floors.

---

## 5. The Vertical Climb & Transits

Verticality is the heartbeat of Cloudline Tower. The run challenges squads to navigate between the ground plaza (y = 0 m) and the roof terrace (y = 50 m) through two contrasting transit routes:

### Dual Vertical Transit Architecture
1. **The Passenger / Freight Elevator (`SkyscraperElevator`, NGO Host-Authoritative)**:
   - Cab rides guide rails through a 48 m central structural shaft.
   - 3 active stops:
     - Floor 0: Street Lobby (y = 0 m)
     - Floor 1: Mechanical Deck (y = 20 m)
     - Floor 2: Apex Restaurant Sky Lobby (y = 44 m)
   - Synchronized sliding landing gates at each level that seal off the open shaft whenever the cab is not present.
   - Interactive call stations at each landing and a 3-button destination panel inside the car.

2. **The 44-Metre Fire Stairwell (`SkyscraperStairwell`)**:
   - 11 continuous switchback stair flights enclosed within a reinforced concrete core.
   - Fire break exit doors opening onto distinct intermediate landings:
     - **Story 0 (y = 0 m)**: Street lobby entrance foyer.
     - **Story 3 (y = 12 m)**: Flooded corporate floor with standing water, burst sprinkler pipes, and wet office desks.
     - **Story 5 (y = 20 m)**: Intermediate mechanical deck, transformer cage, and catwalks across the shaft void.
     - **Story 8 (y = 32 m)**: Ransacked executive cubicles, server racks, and broken exterior windows with howling wind.
     - **Story 11 (y = 44 m)**: Summit Apex Restaurant sky lobby.
   - **Roof Stair (y = 50 m)**: Exterior steel access stairs leading from the dining terrace up to the helipad roof crown.
   - **Carrying Decision**: Dragging a 220 kg safe or the 340 kg wine cask down 11 flights of tight concrete stairs tests stamina, corner clearance, and coordination when the elevator is dead.

### What Makes the Lift Fail
The lift fails for three clear, readable reasons:
1. **No Power in Sector 1 (Mechanical Deck)**:
   - The lift motor is wired to Sector 1. When a blackout event occurs or danger peaks, Sector 1 trips.
   - Elevator halts immediately. Call and send button prompts clearly display: `Lift (No Power)`.
   - The indicator lamps above the landing gates turn off.
   - **Remedy:** Squad must hike to the Mechanical Floor (y = 20 m) and throw the Sector 1 Breaker panel.
2. **Overweight Cable Snap (`LiftSnap` -> `Crash`)**:
   - Safe working load is **450 kg** (players + carried loot + loose platform cargo via `LoadSources.All`).
   - Button prompt warns `Lift (Overloaded)` if current load exceeds 450 kg.
   - If sent while overloaded, cables snap midway through the shaft: `LiftSnap` plays, camera shakes violently, and the cab plunges into free-fall down the shaft.
   - Impacts the bottom buffer springs with `SoundId.Crash`, deals 40 HP crushing damage to all occupants, and permanently disables the lift (`Lift (Out of Order)`).
3. **Shaft Jamming & Structural Damage**:
   - High-danger aftershocks or monster aggression can jam landing gates, requiring manual inspection and forcing the crew to use intermediate escape doors to the fire stairwell.

---

## 6. Top-Level Content & Atmosphere

### Floor 2: The Apex Restaurant (y = 44 m)
- **Sky Lobby & Reception**: Maître d' podium, guest ledger salvage, and velvet stanchions.
- **Grand Dining Room**: White linen banquet tables, fallen brass chandeliers, marble pedestals, and floor-to-ceiling curtain glass windows overlooking the district.
- **Cocktail Lounge & Bar**: Curved rich mahogany bar, mirrored spirit shelves, brass tap tower, and commercial dual-boiler espresso machine (65 kg).
- **Commercial Kitchen Line**: Stainless steel cooking ranges with cast-iron burners, overhead exhaust hoods, prep tables with butcher blocks, dishwashing sinks, and gravity meat slicer.
- **Walk-in Cold Store**: Heavy insulated refrigeration latch door, ceiling meat rails, wire shelving, and aged Jamón Ibérico legs.
- **Penthouse Executive Suite**: VIP private dining boardroom, rich mahogany table, 8 leather executive armchairs, climate-controlled glass wine display wall, and the executive wall safe (220 kg).
- **Observation Terrace**: Outdoor cantilevered viewing deck with tempered safety glass barriers.

### Summit Roof Terrace & Window Gantry (y = 50 m)
- **Exterior Access Stairs**: Connects restaurant terrace up to the roof parapet.
- **Helipad**: Painted contrast "H" helicopter landing target on the open concrete slab.
- **Window-Cleaning Gantry**: Heavy steel crane rail track along the northern parapet, motorized trolley, A-frame boom arm, and a suspended cradle platform hanging out over the 50 m vertical drop.
- **Viewing Telescope**: High-power brass observation telescope overlooking the skyline.

---

## 7. Structural Sections & Collapses

Configured via `StructureSceneSetup` and `StructuralSection`:
- **Dining Room North Window Slabs**: Cantilevered floor tiles along the glass curtain wall weakened by moisture (`health = 0.65f`, `capacity = 0.55f`). Overloaded squads or dropped heavy loot trigger progressive fractures and collapse into the abyss.
- **Mechanical Catwalk Shaft Bridge**: Central steel diamond plate catwalk over the elevator machinery void (`health = 0.60f`, `capacity = 0.50f`). Buckles under excessive load.
- **Stairwell Landing 6 (y = 24 m)**: Concrete landing section testing players carrying heavy salvage down the stairwell.
- **Window Washer Cradle & Roof Terrace Overlook**: Roof edge platforms that can fail if overloaded.
- **Protected Core Route**: The interior elevator entrance, primary stair runs, and core lobby corridors are protected by `SectionKeepClear` to ensure an extraction path always exists.
- **Dynamic AI Updates**: All collapsible modules have `SectionNavCarver` so monster AI paths automatically reroute around broken floor chasms.

---

## 8. Power Sectors & Breaker Panels

All 5 building sectors are fully wired with functional `LightFixture` networks and dedicated `BreakerPanel` units:
- **Sector 0 (Ground Lobby & Exterior)**: Plaza street lamps, entrance canopy downlights, and grand lobby chandeliers. Breaker panel located in the lobby security niche.
- **Sector 1 (Elevator Shaft & Stairwell)**: Elevator hoist motors, cab ceiling fixture, shaft bulkheads, and emergency stairwell lamps on all 11 stories. Breaker panel located in the Mechanical Deck electrical room (y = 20 m).
- **Sector 2 (Grand Dining & Lounge)**: Crystal chandeliers, recessed downlights, and terrace sconces. Breaker panel in the bar service corridor.
- **Sector 3 (Kitchen & Cold Store)**: Industrial fluorescent tubes and cold store vapor-proof bulkheads. Breaker panel on the kitchen utility wall.
- **Sector 4 (Penthouse VIP Suite)**: Luxury halogen recessed downlights and private wine wall backlights. Breaker panel in the VIP vestibule.

---

## 9. Environmental Soundscape (`SkyscraperSoundscape`)

Authored in `Scripts/Skyscraper/SkyscraperSoundscape.cs` using only existing `SoundId`s:
- **Broken Glass Wind Whistling (`SoundId.GhostWhoosh`)**: Spatial high-altitude wind gusts at broken window nodes on the 32 m office floor and the restaurant north curtain wall.
- **Shaft Cable Creaking (`SoundId.Groan` / `SoundId.Creak`)**: Cable tension and hoist strain resonating throughout the 48 m elevator shaft.
- **Piano Lid Dropping (`SoundId.DoorSlam`)**: Resonant thud from the grand piano in the lounge.
- **Bar Glasses Rattling (`SoundId.LootRattle`)**: Clinking stemware on the back-bar racks and sommelier wine displays.
- **Flooded Floor Water Dripping (`SoundId.WaterDrip`)**: Continuous spatial drip loop beneath the burst sprinkler pipes on Floor 3.
- **F1 Gizmos**: Enabled via `DebugView.Visible`:
  - Blue: Wind nodes
  - Orange: Shaft cable nodes
  - Magenta: Piano node
  - Yellow: Glass rattle nodes
  - Cyan: Water drip nodes
  - Elevator Gizmos: Cab wireframe (Green = idle, Yellow = moving, Red = broken), shaft stops (Cyan), and load sphere (White = safe, Magenta = overloaded).

---

## 10. Salvage Loot Catalog (32 Items, 2 Jackpots)

Authored in `Data/Loot/Skyscraper/` with procedural 3D models in `Art/Skyscraper/Loot/`, fully localized in 9 languages:
| Item ID | Name | Carry Class | Weight | Value Range | Tag | Description |
|---|---|---|---|---|---|---|
| `silver_cutlery_canteen` | Silver Cutlery Canteen | OneHand | 3.5 kg | $800 - $2,200 | dining | Sterling silver 24-piece dinner service |
| `caviar_tin_reserve` | Reserve Imperial Caviar | Pocket | 1.2 kg | $1,500 - $4,000 | dining | Gold-leaf tin of vacuum-sealed sturgeon caviar |
| `crystal_wine_decanter` | Crystal Wine Decanter | OneHand | 1.8 kg | $600 - $1,800 | dining | Cut lead crystal spirits decanter |
| `silver_serving_cloche` | Sterling Serving Cloche | OneHand | 1.5 kg | $450 - $1,300 | dining | Polished dome food cover |
| `gilded_china_stack` | Gilded Fine China Stack | TwoHand | 6.0 kg | $900 - $2,600 | dining | Stack of 5 porcelain plates with 24k gold rims |
| `silver_candelabra` | Silver 5-Arm Candelabra | OneHand | 2.8 kg | $550 - $1,600 | dining | Victorian 5-candle silver dining centerpiece |
| `maitre_d_leather_ledger` | Maître D' Leather Ledger | OneHand | 1.4 kg | $400 - $1,200 | dining | Bound calfskin reservation book |
| `brass_observation_telescope` | Tower Viewing Telescope | Heavy | 52.0 kg | $3,500 - $8,800 | dining | Heavy brass pedestal viewing telescope |
| `copper_saute_pan` | Solid Copper Saute Pan | OneHand | 3.2 kg | $700 - $1,900 | kitchen | French copper sauté pan with brass handle |
| `damascus_knife_roll` | Master Chef Damascus Roll | OneHand | 2.5 kg | $1,200 - $3,200 | kitchen | Folded steel Japanese culinary knives in leather |
| `black_truffle_crate` | Crate of Black Truffles | OneHand | 4.0 kg | $2,000 - $5,500 | kitchen | Preserved winter truffles in cedar shavings |
| `aged_iberico_ham` | Aged Jamon Iberico Leg | TwoHand | 8.5 kg | $1,100 - $2,900 | kitchen | Whole cured leg on wooden carving stand |
| `sous_vide_circulator` | Commercial Immersion Unit | OneHand | 4.2 kg | $850 - $2,400 | kitchen | Stainless steel precision immersion cooker |
| `heavy_copper_stockpot` | Heavy Copper Stockpot | TwoHand | 12.0 kg | $1,400 - $3,800 | kitchen | Commercial dual-handled stockpot |
| `brass_meat_slicer` | Commercial Gravity Meat Slicer | Heavy | 48.0 kg | $3,200 - $7,500 | kitchen | Heavy motorized deli slicer |
| `gold_cocktail_shaker` | Gold-Plated Cocktail Shaker | Pocket | 0.7 kg | $650 - $1,800 | bar | Art-deco engraved cocktail shaker |
| `crystal_tumbler_set` | Lead Crystal Tumblers (4) | OneHand | 2.0 kg | $400 - $1,100 | bar | Heavy crystal old-fashioned glasses |
| `vintage_soda_siphon` | Wire-Mesh Soda Siphon | OneHand | 2.2 kg | $300 - $850 | bar | Vintage seltzer bottle with wire casing |
| `commercial_espresso_machine` | Dual-Boiler Espresso Unit | Heavy | 65.0 kg | $4,500 - $9,500 | bar | Chrome and brass Italian dual-group machine |
| `brass_draught_tap_tower` | Triple Brass Tap Tower | TwoHand | 14.5 kg | $1,800 - $4,200 | bar | 3-tap draft beer counter tower |
| `vintage_cognac_1945` | 1945 Imperial Cognac | Pocket | 1.6 kg | $1,800 - $4,800 | cellar | Wax-sealed reserve bottle from cellar |
| `grand_cru_bordeaux` | 1982 Grand Cru Bordeaux | Pocket | 1.5 kg | $2,500 - $7,000 | cellar | Premier grand cru vintage red wine |
| `sommelier_tasting_kit` | Master Sommelier Kit | OneHand | 2.8 kg | $950 - $2,700 | cellar | Mahogany case with silver tastevin and openers |
| `champagne_dom_magnum` | Reserve Champagne Magnum | OneHand | 3.0 kg | $1,600 - $4,200 | cellar | 1.5L magnum bottle in presentation box |
| `vintage_single_malt_whisky` | 50-Year Islay Single Malt | Pocket | 1.4 kg | $2,200 - $5,600 | cellar | Rare peated Scotch whisky |
| `grand_wine_cask` | Grand Vintage Cask (Jackpot 1) | Huge | 340.0 kg | $25,000 - $60,000 | cellar | Massive French oak cask requiring team haul |
| `platinum_fountain_pen` | Solid Platinum Desk Pen | Pocket | 0.15 kg | $1,200 - $3,500 | executive | Executive signing pen with iridium nib |
| `vintage_phonograph_console` | Executive Turntable Console | TwoHand | 16.0 kg | $2,200 - $6,000 | executive | Rare mahogany audiophile turntable console |
| `executive_leather_briefcase` | Executive Leather Briefcase | OneHand | 3.0 kg | $1,500 - $4,500 | executive | Full-grain leather case with brass locks |
| `gantry_anemometer_gauge` | Roof Wind Anemometer Kit | OneHand | 2.6 kg | $750 - $2,100 | executive | Precision weather instrument from roof gantry |
| `bronze_bull_statue` | Solid Bronze Bull (Jackpot 2) | Heavy | 85.0 kg | $14,000 - $32,000 | executive | Solid cast charging bull sculpture on plinth |
| `executive_wall_safe` | Executive Wall Safe | Heavy | 220.0 kg | $8,000 - $22,000 | executive | Reinforced alloy safe from VIP penthouse |

---

## 11. Hiding Places (15 Spots, 4 Kinds)

1. `HideSpotKind.Cabinet`: Street Lobby concierge back-room credenza
2. `HideSpotKind.Locker`: Street Lobby security staff locker
3. `HideSpotKind.Cabinet`: Flooded Floor (12 m) utility supply cabinet
4. `HideSpotKind.Cabinet`: Mechanical Deck (20 m) switchgear cabinet #1
5. `HideSpotKind.Cabinet`: Mechanical Deck (20 m) switchgear cabinet #2
6. `HideSpotKind.Machine`: Mechanical Deck (20 m) HVAC chiller maintenance compartment
7. `HideSpotKind.Closet`: Looted Office Floor (32 m) executive records closet
8. `HideSpotKind.Cabinet`: Restaurant Dining North waiter credenza
9. `HideSpotKind.Cabinet`: Restaurant Dining South waiter credenza
10. `HideSpotKind.Cabinet`: Cocktail Lounge back-bar cabinet
11. `HideSpotKind.Locker`: Kitchen staff locker #1
12. `HideSpotKind.Locker`: Kitchen staff locker #2
13. `HideSpotKind.Closet`: Walk-in cold store insulated cabinet
14. `HideSpotKind.Closet`: Penthouse VIP executive wardrobe
15. `HideSpotKind.Locker`: Roof Terrace window washer gear locker (50 m)

---

## 12. Occlusion Culling Design

- **Occluder Static Geometry**: Applied to permanent solid architecture: massive corner pylons, core concrete walls (stairwell core, elevator shaft walls), lobby perimeter walls, and roof parapets.
- **Never Occluder Static**: All floor slabs, tiles, walkways, catwalks, ramps, stairs, and structural section modules carry no `OccluderStatic` flags, ensuring that collapsed floors never incorrectly occlude what lies below or above them.
- **Bake Procedure**: `ApplyOcclusionFlags` marks static flags, scene is saved, `OcclusionBake.Bake()` is executed, and scene is re-saved.

---

## 13. What to Check First (Review Checklist)

When loading the map in Unity via **Tools > Abandoned > Maps > Skyscraper Restaurant > Rebuild Skyscraper Restaurant**:
1. **Streetscape, Forecourt Plaza & Skyline**:
   - Check Broadway Avenue: double-yellow lines, pedestrian crosswalks, street lamps, abandoned vehicles (Executive towncar, yellow cab, courier van), and East barricades with rusted dumpster.
   - Check Cloudline Plaza: paved stone square, drained reflecting pool with murky puddle, "The Ascendant Spiral" modern brass sculpture, concrete planters with withered trees, teak benches, corporate monument sign, chrome flagpoles with burgundy banners, and underground garage ramp.
   - Look into the distance / up at the horizon: verify the 6 skyline features (85m Trans-Continental spire with red beacon, 72m twin towers with 42m skybridge, 92m Vanguard monolith, 38m Federal Reserve, 8m elevated highway with stalled cars, 110m suspension bridge tower).
2. **Grand Double-Height Marble Lobby**:
   - Walk through the revolving cylinder glass doors into the lobby.
   - Check the Carrara marble and dark granite floor, fluted double-height pillars, concierge reception desk, optical security turnstiles, metal detector portal, executive leather waiting lounge, and tenant directory plaque.
3. **Elevator Operation & Indicators**:
   - Check landing call buttons at Lobby (0 m), Mechanical Deck (20 m), and Restaurant (44 m). Verify landing gates open only when cab arrives.
   - Verify car destination buttons (Lobby, Mechanical, Restaurant) move the elevator smoothly with riders on `MovingPlatform`.
   - Test Sector 1 power cut: trip Sector 1 breaker -> verify elevator stops and prompts show `Lift (No Power)`. Throw breaker -> verify elevator operates.
   - Test Overload Snap: place > 450 kg on the cab deck -> verify prompt warns `Lift (Overloaded)`, cables snap with `LiftSnap`, cab plunges into free fall, deals damage, crashes at bottom (`Crash`), and breaks permanently (`Lift (Out of Order)`).
4. **The 44-Metre Fire Stairwell & Intermediate Floors**:
   - Walk the continuous 11 flights from 0 m to 44 m to ensure seamless stair colliders.
   - Inspect intermediate landing fire exit doors:
     - Floor 3 (12 m): Flooded floor with burst pipes, standing water, and office clutter.
     - Floor 5 (20 m): Mechanical deck with transformers and catwalks.
     - Floor 8 (32 m): Looted corporate office with broken window letting in wind.
     - Floor 11 (44 m): Restaurant sky lobby.
5. **Summit Content & Roof Terrace**:
   - Inspect the panoramic dining room, mahogany bar, commercial kitchen line, cold store, and VIP penthouse.
   - Walk up the exterior stairs from the dining terrace to the Roof Terrace (50 m). Check the painted Helipad, window-cleaning gantry rig, suspended cradle, and viewing telescope.
6. **Structural Collapse**:
   - Walk over northern dining room glass-edge cantilever slabs while carrying heavy loot -> observe fracture stages and collapse.
   - Check central mechanical catwalk over the shaft void.
   - Verify `SectionNavCarver` carves NavMesh when a slab falls.
7. **Soundscape & F1 Gizmos**:
   - Listen for broken window wind whistling, cable creaks in the shaft, piano lid thuds, bar glasses rattling, and flooded floor drips.
   - Press F1 (`DebugView.Visible`) -> verify colored Gizmo spheres appear at all soundscape points and elevator cab/stops/load.
8. **Open World Integration**:
   - Verify van parking point at `(0, 0, -22)` with hazard hatching.
   - Ground cutout rectangle `[-24, -35] to [24, 22]` at elevation `0.0 m`.
   - Confirm `Exterior/Truck` and `Exterior/Skyline` exist under `Exterior` for world streaming to toggle.

---

## 14. Proposed Decisions for CLAUDE.md

- **Skyscraper Restaurant (2026-10-23):** Cloudline Tower & The Apex Restaurant added as an open-world streamed salvage POI (`Scenes/Skyscraper.unity`).
  - Dual vertical transit: Host-authoritative 48 m `SkyscraperElevator` (stops: Ground, Mechanical, Apex Restaurant) with weight-limited cable snap at 450 kg, power dependence on Sector 1, and an 11-flight fire stairwell with stops at Flooded Floor (12 m), Mechanical Deck (20 m), and Looted Office (32 m).
  - Rich urban setting & streetscape: Broadway Avenue with painted markings, sodium lamps, abandoned vehicles (executive car, yellow cab, courier van), drained reflecting pool with modern brass sculpture, corporate plaza, and underground parking ramp.
  - Atmospheric 360-degree surrounding cityscape (`Exterior/Skyline`) with 6 distinct architectural landmarks (stepped Art-Deco spire, twin towers with 42m skybridge, dark monolith, brutalist Federal Reserve, elevated highway with wrecks, suspension bridge tower).
  - Grand double-height marble lobby (`Lobby`) featuring revolving cylinder doors, fluted colonnade, concierge desk, optical turnstiles, metal detector portal, and executive waiting lounge.
  - 5 power sectors with independent `BreakerPanel`s (Lobby 0, Mechanical/Shaft 1, Dining/Terrace 2, Kitchen/Cold Store 3, VIP Penthouse 4).
  - 32 restaurant & executive themed salvage definitions (`Data/Loot/Skyscraper/`), including 2 jackpots: `bronze_bull_statue` (85 kg) and `grand_wine_cask` (340 kg), 100% localized in all 9 languages in `skyscraper.txt`.
  - Roof crown (50 m) featuring helipad, window-cleaning gantry crane with suspended cradle, and observation telescope.
  - Environmental `SkyscraperSoundscape` (wind whistling, shaft cables creaking, piano lid falling, glasses rattling, flooded pipe drips) with F1 debug Gizmos.
  - Strict occlusion culling: permanent walls and shell set as `OccluderStatic`, floor slabs excluded.
