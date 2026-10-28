# Steam setup (when the real App ID exists)

Everything the game needs from the Steamworks partner site, in one place. Until then the game runs on
App ID 480 (Spacewar) and all of this stays local.

## 1. App IDs in the project
| Where | What |
|---|---|
| `Game/Assets/_Project/Data/Networking/NetworkConfig.asset` -> SteamAppId | the game's App ID (the rebuild keeps `Game/steam_appid.txt` in sync) |
| `Game/Assets/_Project/Data/Core/Resources/DemoConfig.asset` -> StoreAppId | the FULL game's App ID: the demo's "Wishlist on Steam" opens its store page |
| Demo build | Steam demos are their own App ID: build with `Tools/unity.sh build-demo` and upload to the demo's depot (set its NetworkConfig App ID for that build, or ask me to add a demo App ID field) |

Release builds (`BuildFlavor.Release`) never ship steam_appid.txt: Steam launches them with the real ID.
Since 0.25.0 a release or demo build started outside Steam asks Steam to relaunch it
(`SteamClient.RestartAppIfNecessary`, skipped for App ID 480), and hosts through Steam by default.

## 2. Steam Cloud (Auto-Cloud, no game code)
The company save is a plain file in Unity's persistent data folder; Auto-Cloud syncs it.
Steamworks -> App Admin -> Cloud -> Steam Cloud Settings: quota e.g. 1 MB / 10 files, then Root Paths:

| OS | Root | Subdirectory | Pattern |
|---|---|---|---|
| Windows | WinAppDataLocalLow | `Zeyad Games/Abandoned` | `company*.json` |
| macOS | MacHome | `Library/Application Support/Zeyad Games/Abandoned` | `company*.json` |

(macOS: add it as a Root Override of the Windows root, OS = macOS, as the partner site describes.)
`company.json` is the full game's company (save slot 1), `company_2.json` and `company_3.json` slots 2 and 3,
`company_demo.json` the demo's; the pattern covers all four. A save from a newer build is
never overwritten by an older one (SaveStore refuses), and Steam shows its own conflict dialog.
Not synced: settings, key bindings, cosmetics picks and achievement stats (PlayerPrefs, per machine).

## 3. Achievements
Create these on Steamworks -> Stats & Achievements (API name = what the game sends):

| API name | Name | Unlocks when |
|---|---|---|
| ACH_FIRST_JOB | First Day on the Job | Finish a run |
| ACH_MADE_IT_OUT | Made It Out | Get on the truck before it leaves |
| ACH_CAREER_SALVAGER | Career Salvager | Make it out of 25 runs |
| ACH_QUOTA_CRUSHER | Quota Crusher | Meet the quota 10 times |
| ACH_SIX_FIGURES | Six Figures | Haul $100,000 over your career |
| ACH_MILLIONAIRE | Millionaire | Haul $1,000,000 over your career |
| ACH_THE_GREED_ITEM | The Greed Item | Get a jackpot out of a building |
| ACH_GRAVITY_WINS | Gravity Wins | Be there when 10 floors give way |
| ACH_OCCUPATIONAL_HAZARD | Occupational Hazard | Die on the job |
| ACH_MEDIC | Medic | Get a downed crewmate back on their feet |
| ACH_TIMBER | Timber! | Watch the Hunter fall through a floor |
| ACH_LIGHTS_OUT | Lights Out | Make it out of a job with the power off or after dark |
| ACH_LET_THERE_BE_LIGHT | Let There Be Light | Throw a breaker |
| ACH_SAFECRACKER | Safecracker | Be there when the vault drill gets through |
| ACH_GOING_UP | Going Up | Ride the freight lift 10 times |
| ACH_DUCTWORK | Ductwork | Crawl through 5 vents |
| ACH_MATCHING_SET | Matching Set | Bring a complete loot set home |
| ACH_TEACHERS_PET | Teacher's Pet | Bring the boss what they asked for 3 times |
| ACH_EXPRESS_DELIVERY | Express Delivery | Deliver a rush order in time |
| ACH_UNDER_NEW_MANAGEMENT | Under New Management | Come back as a replacement worker |
| ACH_LOOK_OVER_THERE | Look Over There | Ping 100 things |
| ACH_WORKAHOLIC | Workaholic | Get paid overtime |

All 22 (0.19.10 added the last ten), from `Tools/Steam/achievements.csv`, which is written from the
game's own catalog and also names each icon pair in `Tools/Steam/AchievementIcons/`. If the two disagree, the
CSV is right.

The definitions live in `Game/Assets/_Project/Data/Core/Achievements/` (thresholds are data). The game
calls Steam while it runs, and re-sends every local unlock when Steam starts (offline unlocks catch up).

## 4. Builds and depots
- `Tools/unity.sh build` -> Mac (universal) + Windows (x64) Mono zips; `build-demo` the same for the demo.
- `Tools/unity.sh build-release` -> the Steam depots (Game/Builds/MacRelease, WindowsRelease, zipped too): no
  steam_appid.txt; it refuses to build while NetworkConfig's App ID is still 480. Upload with SteamPipe
  (Docs/LAUNCH.md has the order of things).
- One depot per OS. Launch options: Windows `Abandoned.exe`, macOS `Abandoned.app`.
- Lobbies/invites: friends-only lobbies, max 4 (NetworkConfig.MaxPlayers); "Join game" from the friends
  list uses +connect_lobby (already handled).
