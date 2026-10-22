# Tools > Abandoned menu map

**Read this before adding, moving or renaming anything in the Tools menu.** Every item lives in exactly one place
below. Paths and priorities come from `Game/Assets/_Project/Editor/ToolMenu.cs`; never type a raw
`"Tools/Abandoned/..."` string into a `[MenuItem]`.

## How to add an item

1. Find its group with "Where does it go?" below.
2. Use that group's path constant and priority band from `ToolMenu`:
   ```csharp
   [MenuItem(ToolMenu.Factory + "Add Slag Carts", priority = ToolMenu.FactoryP + 21)]
   ```
3. Choose a priority inside the group's band (see the tree). Unity draws a separator wherever two neighbouring
   priorities differ by more than 10: +0 to +9 is the group's main block, +20, +40 and so on start new sections.
4. Add the item to the tree in this file, in the same position.
5. If it is a new map, add a `ToolMenu` path and band (the next free x100 in the Maps band), give it a
   `Maps/<Map>/Rebuild <Map>` item and add that rebuild to `Editor/RebuildAllMaps.cs`.

## Naming rules

- Start with a verb: Rebuild, Add, Create, Build, Generate, Render, Apply, Audit, Validate.
- A whole-scene rebuild of a map is always `Rebuild <map name>`. It is the first item in that map's submenu.
- One-off release steps that add to a saved scene go in that map's `Updates/` submenu. Release features that only
  make data go in `Content/Features/`. Both are named `<version> <action>` (`0.16.4 Add Vents`), in version order,
  one priority step apart.
- Everywhere else a version number goes in brackets at the end, and only if the step belongs to one release
  (`Language Fonts (0.24.8)`). Never put a version in a Rebuild item's name.
- Anything that only reports and changes nothing goes in `Checks/`, not beside the thing it checks.
- No duplicates: if two items run the same code, keep one. Batch entry points (`...AndExit`) need no menu item.

## Where does it go?

| It... | Group |
|---|---|
| rebuilds or adds to one map's saved scene | `Maps/<Map>/` (a release step: `Maps/<Map>/Updates/`) |
| rebuilds every map | `Rebuild All Maps` (top level, the only top-level item) |
| writes ScriptableObject configs or catalogs | `Content/Data/` |
| makes or rewires prefabs (player, network, threats) | `Content/Prefabs/` |
| loot definitions, loot prefabs, loot models | `Content/Loot/` |
| sound bank / audio assets | `Content/Audio/` |
| cosmetics, character models | `Content/Characters/` |
| one release feature's data pass (0.16+) | `Content/Features/` |
| render settings, occlusion, meshes, art previews | `Art/` |
| menu/HUD assets, fonts, icons | `UI/` |
| only reports (audits, validators, verifiers) | `Checks/` |
| project settings, layers; one-off repairs | `Project/` (repairs: `Project/Fixes/`) |
| player builds | `Build/` |
| Steam store, logo, trailer | `Store/` |

## The menu (priority band in brackets)

```
Tools > Abandoned
  Rebuild All Maps                                   [0]
  ──────────
  Maps
    Mall                                             [1000-1099]
      Rebuild Mall and Parking                       1000
      ──
      Updates                                        1020-1043
        0.15.5 Build Salvage Content
        0.15.5b Fill Salvage Mall
        0.15.6 Finish Threat and Content Update
        0.15.7 More Pickups and Collection Truck
        0.15.7b Replace Collection Truck
        0.15.8 Store Branding and Sale Artwork
        0.15.8b Remove Decorative Text
        0.15.8c Fix Artwork Orientation
        0.16.0 Add Doors and Lockers
        0.16.1 Add Breaker Rooms
        0.16.3 Add Freight Lift
        0.16.4 Add Vents
        0.16.5 Add Store Alarms
        0.16.6 Add Vault Safe
        0.16.7 Add Security Office
        0.17.2 Set Floor Types
        0.17.3 Add Noisy Ground
        0.17.5 Add Flooded Areas
        0.18.4 Add Shopping Carts
        0.19.1 Add Agency Radio
        0.20.3 Add Truck Winch
        0.21.4 Truck Cab Instruments
        0.24.9 Replace Loot Look-alikes
        0.24.11 Basement Ground and Solid Props
      ──
      Rebuild Mall Only (no material restore)        1080
    School                                           [1100-1199]
      Rebuild Jackson P Burley Middle School         1100
    Iron Factory                                     [1200-1299]
      Rebuild Iron Factory                           1200
      ──
      Add Iron Factory Loot                          1220
    Island Prison                                    [1300-1399]
      Rebuild Island Prison                          1300
      ──
      Add Island Prison Loot                         1320
    HQ                                               [1400-1499]
      Rebuild Salvage Hub                            1400
      Rebuild Salvage Hub and Run Audits             1401
      ──
      Add Staff Door and Exterior                    1420
      Author Dispatch Audio                          1421
      Create Garage Door Prefab                      1422
      ──
      Save Garage Door Placement                     1440
      Reset Garage Door Placement                    1441
      ──
      Updates                                        1460-1462
        0.18.7 Add Loot Catalogue and Trophy Shelf
        0.19.4 Add Difficulty Switch
        0.19.6 Add Break-Room Toys
    Tutorial House                                   [1500-1599]
      Rebuild Tutorial House                         1500
      Add Training Clipboard to HQ                   1501
      ──
      Build Tutorial Lessons                         1520
      ──
      Reset Tutorial Lessons to Defaults             1540
    Test Map                                         [1600-1699]
      Rebuild Test Map                               1600
      Bake Test Map Lighting                         1601
    Hospital                                         [1700-1749]
      Rebuild Saint Brigid Hospital                  1700
    Motel                                            [1750-1799]
      Rebuild Last Light Motor Lodge                 1750
    Police Station                                   [1800-1849]
      Rebuild Police Station                         1800
      ──
      Add Police Station Loot                        1820
    Fire Station                                     [1850-1899]
      Rebuild Fire Station                           1850
      ──
      Add Fire Station Loot                          1870
  ──────────
  Content
    Data                                             [2000-2099]
      Create Company Content, Create Equipment Content, Create Health and Flashlight Content,
      Create Structure Content, Create Door and Locker Config, Create Pacing and Decay Configs,
      Create Crew Director Config, Build Achievements                     2000-2007
    Prefabs                                          [2100-2199]
      Create Network Content, Create Structure Net Prefab, Create Player Prefab,
      Create Threat Prefabs, Create New Horror Threats                   2100-2104
    Loot                                             [2200-2299]
      Create Example Loot Definitions, Generate Loot Prefabs, Assign Loot Models   2200-2202
      ──
      Author Salvage Asset Library (0.15.5), Author Additional Pickable Stock (0.15.7)  2220-2221
    Audio                                            [2300-2399]
      Build Sound Bank, Build Recorded Library (0.24.12)   2300, 2302
    Characters                                       [2400-2499]
      Build Cosmetics                                2400
      ──
      Generate and Apply Full Redesign, Apply Redesign from Exported Models  2420-2421
    Features                                         [2500-2599]
      0.16.2 Seeded Route Changes ... 0.24.3 Achievement Icons (21 items, version order, 2500-2520)
  ──────────
  Art                                                [3000-3999]
    Apply Render Budget, Bake Occlusion (open scene)                    3000-3001
    ──  Generate Fractured Tile                                          3020
    ──  Loot Lineup (unsaved scene)                                      3040
    ──  0.24.12: Apply 0.24.12 Look to Saved Maps, Upgrade Surfaces (Scanned Textures), Make Decal Set,
        Scatter Wear Decals (Open Scene), 0.24.12 Look, Surfaces, Sound and Decals   3060-3063, 3065
        Upgrade New Maps (Hospital, Motel, Skyscraper, Stations): the same, new maps only   3066
    ──  0.24.13: Separate Coplanar Faces (Open Scene)                    3080
        Rebuild Lighting and Shadows                                   3081
        Remaster All Map Art and Objects                               3082
        Map Sites: Sky, Trees, Grass and Sound                         3083
  ──────────
  UI                                                 [4000-4999]
    Build Menu Assets, Create Menu Effects, Create Horror Textures, Create Gear Icons,
    Render Loot Icons (all)                                              4000-4004
    ──  Font Fallbacks (0.19.9), Language Fonts (0.24.8)                 4020-4021
  ──────────
  Checks                                             [5000-5999]
    Verify Project Setup, Validate Content                               5000-5001
    ──  Audit All Levels, Audit Saved Mall, Audit and Render Island Prison  5020-5022
  ──────────
  Project                                            [6000-6999]
    Setup Physics Layers                                                 6000
    Fixes: Apply Steam Plugin Settings, Clear Project-wide Input Actions,
           Silence Player Voice Sources                                  6020-6022
  ──────────
  Build                                              [7000-7999]
    Mac, Windows, Both (Mac + Windows)                                   7000-7002
    ──  Dev Mac (for tests)                                              7020
    ──  Demo (Mac + Windows), Release (Mac + Windows, real App ID)       7040-7041
  ──────────
  Store                                              [8000-8999]
    Install Application Logo                                             8000
    ──  Render Store Art and Screenshots, Render Draft Trailer           8020-8021
    ──  Prepare Steam Release                                            8040
```

Not under Tools: the right-click **CONTEXT/LootDefinition/Generate Prefab** on a loot definition asset.

## The 2026-10-20 reorganisation

Every item was moved into the groups above. No method was renamed, so `Tools/unity.sh exec <Class.Method>`
batch runs are unchanged. Three duplicate menu items were removed; their methods stay for batch commands.
Paths in Docs/, CLAUDE.md and code comments/messages were updated to the new places.

| Old path (Tools/Abandoned/...) | New path (Tools/Abandoned/...) |
|---|---|
| Mall/Rebuild Mall and Parking (0.15.0) | Maps/Mall/Rebuild Mall and Parking |
| Create Mall | Maps/Mall/Rebuild Mall Only (no material restore) |
| Mall/Build Salvage Content (0.15.5) | Maps/Mall/Updates/0.15.5 Build Salvage Content |
| Mall/Fill Salvage Mall (0.15.5b) | Maps/Mall/Updates/0.15.5b Fill Salvage Mall |
| Mall/Finish Threat and Content Update (0.15.6) | Maps/Mall/Updates/0.15.6 Finish Threat and Content Update |
| Mall/More Pickups and Collection Truck (0.15.7) | Maps/Mall/Updates/0.15.7 More Pickups and Collection Truck |
| Mall/Replace Collection Truck (0.15.7b) | Maps/Mall/Updates/0.15.7b Replace Collection Truck |
| Mall/Store Branding and Sale Artwork (0.15.8) | Maps/Mall/Updates/0.15.8 Store Branding and Sale Artwork |
| Mall/Remove Decorative Text (0.15.8b) | Maps/Mall/Updates/0.15.8b Remove Decorative Text |
| Mall/Fix Artwork Orientation (0.15.8c) | Maps/Mall/Updates/0.15.8c Fix Artwork Orientation |
| Mall/Add Doors and Lockers (0.16.0) | Maps/Mall/Updates/0.16.0 Add Doors and Lockers |
| Mall/Add Breaker Rooms (0.16.1) | Maps/Mall/Updates/0.16.1 Add Breaker Rooms |
| Mall/Add Freight Lift (0.16.3) | Maps/Mall/Updates/0.16.3 Add Freight Lift |
| Mall/Add Vents (0.16.4) | Maps/Mall/Updates/0.16.4 Add Vents |
| Mall/Add Store Alarms (0.16.5) | Maps/Mall/Updates/0.16.5 Add Store Alarms |
| Mall/Add Vault Safe (0.16.6) | Maps/Mall/Updates/0.16.6 Add Vault Safe |
| Mall/Add Security Office (0.16.7) | Maps/Mall/Updates/0.16.7 Add Security Office |
| Mall/Set Floor Types (0.17.2) | Maps/Mall/Updates/0.17.2 Set Floor Types |
| Mall/Add Noisy Ground (0.17.3) | Maps/Mall/Updates/0.17.3 Add Noisy Ground |
| Mall/Add Flooded Areas (0.17.5) | Maps/Mall/Updates/0.17.5 Add Flooded Areas |
| Mall/Add Shopping Carts (0.18.4) | Maps/Mall/Updates/0.18.4 Add Shopping Carts |
| Mall/Add Agency Radio (0.19.1) | Maps/Mall/Updates/0.19.1 Add Agency Radio |
| Mall/Add Truck Winch (0.20.3) | Maps/Mall/Updates/0.20.3 Add Truck Winch |
| Mall/Truck Cab Instruments (0.21.4) | Maps/Mall/Updates/0.21.4 Truck Cab Instruments |
| Mall/Replace Loot Look-alikes (0.24.9) | Maps/Mall/Updates/0.24.9 Replace Loot Look-alikes |
| Mall/Apply 0.24.11 (basement ground, solid props) | Maps/Mall/Updates/0.24.11 Basement Ground and Solid Props |
| Create School | *removed: duplicate of Maps/School/Rebuild Jackson P Burley Middle School* |
| Factory/Rebuild Iron Factory | Maps/Iron Factory/Rebuild Iron Factory |
| Factory/Add Iron Factory Loot | Maps/Iron Factory/Add Iron Factory Loot |
| Prison/Rebuild Island Prison | Maps/Island Prison/Rebuild Island Prison |
| Create Prison | *removed: duplicate of Maps/Island Prison/Rebuild Island Prison* |
| Prison/Add Island Prison Loot | Maps/Island Prison/Add Island Prison Loot |
| Locations/Rebuild School and Prison (0.24.11) | *removed: duplicate of Rebuild All Maps (method kept for batch runs)* |
| HQ/Rebuild Salvage Hub (0.14.5b) | Maps/HQ/Rebuild Salvage Hub |
| Create HQ | Maps/HQ/Rebuild Salvage Hub and Run Audits |
| HQ/Author Dispatch Audio (0.14.5) | Maps/HQ/Author Dispatch Audio |
| HQ/Add Loot Catalogue and Trophy Shelf (0.18.7) | Maps/HQ/Updates/0.18.7 Add Loot Catalogue and Trophy Shelf |
| HQ/Add Difficulty Switch (0.19.4) | Maps/HQ/Updates/0.19.4 Add Difficulty Switch |
| HQ/Add Break-Room Toys (0.19.6) | Maps/HQ/Updates/0.19.6 Add Break-Room Toys |
| Tutorial/Rebuild Tutorial House | Maps/Tutorial House/Rebuild Tutorial House |
| Tutorial/Add Training Clipboard to HQ | Maps/Tutorial House/Add Training Clipboard to HQ |
| Tutorial/Build Tutorial Content | Maps/Tutorial House/Build Tutorial Lessons |
| Tutorial/Reset Tutorial Lessons To Defaults | Maps/Tutorial House/Reset Tutorial Lessons to Defaults |
| Create Test Map | Maps/Test Map/Rebuild Test Map |
| Create Company Content | Content/Data/Create Company Content |
| Create Equipment Content | Content/Data/Create Equipment Content |
| Create Health and Flashlight Content | Content/Data/Create Health and Flashlight Content |
| Create Structure Content | Content/Data/Create Structure Content |
| Content/Create Door And Locker Config | Content/Data/Create Door and Locker Config |
| Content/Create Pacing And Decay Configs | Content/Data/Create Pacing and Decay Configs |
| Threats/Create Crew Director Config (0.24.10) | Content/Data/Create Crew Director Config |
| Build Achievements | Content/Data/Build Achievements |
| Create Network Content | Content/Prefabs/Create Network Content |
| Create Structure Net Prefab | Content/Prefabs/Create Structure Net Prefab |
| Create Player Prefab | Content/Prefabs/Create Player Prefab |
| Create Threat Prefabs | Content/Prefabs/Create Threat Prefabs |
| Create New Horror Threats | Content/Prefabs/Create New Horror Threats |
| Create Example Loot Definitions | Content/Loot/Create Example Loot Definitions |
| Generate Loot Prefabs | Content/Loot/Generate Loot Prefabs |
| Art/Assign Loot Models | Content/Loot/Assign Loot Models |
| 0.15.5/Author Salvage Asset Library | Content/Loot/Author Salvage Asset Library (0.15.5) |
| 0.15.7/Author Additional Pickable Stock | Content/Loot/Author Additional Pickable Stock (0.15.7) |
| Audio/Build Sound Bank | Content/Audio/Build Sound Bank |
| Cosmetics/Build Cosmetics | Content/Characters/Build Cosmetics |
| Characters/Generate and Apply Full Redesign | Content/Characters/Generate and Apply Full Redesign |
| Characters/Apply Redesign from Exported Models | Content/Characters/Apply Redesign from Exported Models |
| Content/Seeded Route Changes (0.16.2) | Content/Features/0.16.2 Seeded Route Changes |
| Content/Sledgehammer (0.17.0) | Content/Features/0.17.0 Sledgehammer |
| Content/Crash Mat (0.17.1) | Content/Features/0.17.1 Crash Mat |
| Content/Aftershocks (0.17.4) | Content/Features/0.17.4 Aftershocks |
| Content/Spray Chalk (0.17.6) | Content/Features/0.17.6 Spray Chalk |
| Content/Glow Sticks and Flares (0.17.7) | Content/Features/0.17.7 Glow Sticks and Flares |
| Content/Boss Requests (0.18.0) | Content/Features/0.18.0 Boss Requests |
| Content/Rush Orders (0.18.1) | Content/Features/0.18.1 Rush Orders |
| Content/Loot Quirks (0.18.2) | Content/Features/0.18.2 Loot Quirks |
| Content/Loot Sets (0.18.3) | Content/Features/0.18.3 Loot Sets |
| Content/Moving Blanket (0.18.5) | Content/Features/0.18.5 Moving Blanket |
| Content/Overtime (0.18.6) | Content/Features/0.18.6 Overtime |
| Content/Ping Marker (0.19.0) | Content/Features/0.19.0 Ping Marker |
| Content/Ghost Knock, Flicker and Nudge (0.19.2) | Content/Features/0.19.2 Ghost Knock, Flicker and Nudge |
| Content/More Achievements (0.19.10) | Content/Features/0.19.10 More Achievements |
| Content/Portable Floodlight (0.20.0) | Content/Features/0.20.0 Portable Floodlight |
| Content/Lockpick (0.20.1) | Content/Features/0.20.1 Lockpick |
| Content/Thermal Camera (0.20.4) | Content/Features/0.20.4 Thermal Camera |
| Content/Scout Drone (0.20.5) | Content/Features/0.20.5 Scout Drone |
| Content/Threat Forecast (0.20.6) | Content/Features/0.20.6 Threat Forecast |
| Content/Achievement Icons (0.24.3) | Content/Features/0.24.3 Achievement Icons |
| Generate Fractured Tile | Art/Generate Fractured Tile |
| Verify Project Setup | Checks/Verify Project Setup |
| Validate Content | Checks/Validate Content |
| Mall/Audit All Levels | Checks/Audit All Levels |
| Mall/Audit Saved Mall | Checks/Audit Saved Mall |
| Prison/Audit and Render Island Prison | Checks/Audit and Render Island Prison |
| Setup Physics Layers | Project/Setup Physics Layers |
| Fix/Apply Steam Plugin Settings | Project/Fixes/Apply Steam Plugin Settings |
| Fix/Clear Project-wide Input Actions | Project/Fixes/Clear Project-wide Input Actions |
| Fix/Silence Player Voice Sources | Project/Fixes/Silence Player Voice Sources |
| Branding/Install Application Logo (0.24.6b) | Store/Install Application Logo |
| Store/Render Store Art and Screenshots (0.24.4) | Store/Render Store Art and Screenshots |
| Store/Render Draft Trailer (0.24.5) | Store/Render Draft Trailer |
| Store/Prepare Steam Release (0.24.6) | Store/Prepare Steam Release |

## Hospital (2026-10-23)

- `Maps/Hospital/Rebuild Saint Brigid Hospital` — priority 1700, `ToolMenu.Hospital` / `HospitalP`.
  Authors Hospital.unity, Hospital-local art/data/navigation and run plumbing; included in Rebuild All Maps.

## Motel (2026-10-23)

- `Maps/Motel/Rebuild Last Light Motor Lodge` — priority 1750, `ToolMenu.Motel` / `MotelP`.
  Authors Motel.unity, Motel-local art/data/navigation and run plumbing; included in Rebuild All Maps.
- Hospital and Motel ship through both `BuildScenes.SalvageScenes` and `BuildScenes.All`. Their generated
  Resources network prefabs register before NGO starts; the world owns distance streaming and POI activation.

## Skyscraper Restaurant (2026-10-23)

- `Maps/Skyscraper Restaurant/Rebuild Skyscraper Restaurant` — priority 1950, `ToolMenu.Skyscraper` / `SkyscraperP`.
  Authors Skyscraper.unity (Cloudline Tower & Apex Restaurant), dual vertical transit via 48m host-authoritative
  elevator and 11-flight continuous fire stairwell, mechanical floor, kitchen and cold store, VIP penthouse suite,
  24 themed salvage definitions, sector power, and structural collapse simulation; included in Rebuild All Maps.

## Police Station (2026-10-23)

- `Maps/Police Station/Rebuild Police Station` — priority 1800, `ToolMenu.PoliceStation` / `PoliceStationP`.
  Authors PoliceStation.unity (the Millbrook 3rd Precinct, Docs/POLICE_STATION_PLAN.md); included in Rebuild All Maps.
- `Maps/Police Station/Add Police Station Loot` — 1820. Fills in the precinct's loot definitions
  (Data/Loot/PoliceStation), models, prefabs and icons; the rebuild runs it first anyway.
- The scene is in `BuildScenes.All` only (an open-world POI, not yet a contract destination).

## Fire Station (2026-10-23)

- `Maps/Fire Station/Rebuild Fire Station` — priority 1850, `ToolMenu.FireStation` / `FireStationP`.
  Authors FireStation.unity (Millbrook Fire Station No. 4, Docs/FIRE_STATION_PLAN.md) with the police station's
  station kit; included in Rebuild All Maps.
- `Maps/Fire Station/Add Fire Station Loot` — 1870. Fills in the firehouse's loot definitions (Data/Loot/FireStation),
  models, prefabs and icons; the rebuild runs it first anyway.
- The scene is in `BuildScenes.All` only (an open-world POI, not yet a contract destination).
