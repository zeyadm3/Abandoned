# Early Access launch checklist (M10.11)

Everything that has to happen outside the code, in order. The game side is done (M3–M10); see
Docs/PROGRESS.md "Needs you" for the playtests that come first.

## 0.24.6: what's ready, and what only you can do (2026-10-10)
Ready in the repo:
- `Tools/unity.sh set-app-id <id>` puts the game's own App ID in NetworkConfig (and the demo's store target)
  and rewrites Game/steam_appid.txt; after it, `Tools/unity.sh build-release` builds MacRelease/WindowsRelease.
- Tools/Steam/achievements.csv: all 22 achievements (API name, name, description) and their icon pairs in
  Tools/Steam/AchievementIcons (256 px, achieved and locked).
- Tools/Steam/rich_presence_english.vdf (0.19.11): upload under Rich Presence localization. 0.24.8 adds one
  file per language beside it (rich_presence_german.vdf ...) and Tools/Steam/achievements_languages.csv, the
  achievements' names and descriptions in the nine languages (Docs/LOCALIZATION_0.24.8.md, "Steam's own files").
- Tools/Steam/steampipe/*.vdf and Tools/Steam/upload.sh <app> <win depot> <mac depot> <login>: SteamPipe upload
  (install steamcmd first); it sets the build live on a "beta" branch, not the default.
- Docs/StoreArt: draft capsules at the current sizes, six screenshots, trailer_draft.mp4 (0.24.4/0.24.5).
- The daily job's leaderboards (`daily_YYYYMMDD`) are created by the game on first upload; nothing to set up.
Only you: pay the Steam Direct fee, create the app (and its two depots: Windows, macOS), run set-app-id, enter
the achievements and icons, Auto-Cloud (paths in Docs/STEAM_SETUP.md), the store page, capture a real trailer,
submit the build and the page for review.

## 0.24.7c: found in the readiness review (2026-10-12)
- [x] **Voices** (done 2026-10-12). The 25 dispatch and voicemail clips were spoken by macOS system voices,
      which Apple licenses for personal, non-commercial use only. They were regenerated with the Kokoro-82M
      model (Apache-2.0) in several voices, mixed on purpose; Tools/Audio/README.md has the set-up and how to
      try other voices.
- [ ] **The play pass.** Nothing from 0.16.0 on has been played, the automated tests have not run since M10,
      and four-player host performance is unmeasured. Docs/AUTOBUILD_0.16-0.24.md ends with where to look first.
- [ ] **Real Steam** (App ID 480, two machines): lobbies, invites, the relay, Steam voice, achievements, cloud.
      Rich presence and the daily leaderboard can only be checked with the game's own App ID.
- [ ] **The name.** Other games on Steam are called "Abandoned" (Docs/STORE_PAGE.md "Name").
- [ ] **AI disclosure** in Steam's content survey: draft in Docs/STORE_PAGE.md.
- [ ] **Real screenshots and trailer**: Docs/StoreArt holds editor renders and a silent animatic.
- [ ] **Mac depot**: the app is signed ad hoc, not notarized. Install it through Steam on a second Mac.
- [ ] **Studio name.** "Zeyad Games" fixes the save folder and the Auto-Cloud paths; change it before
      release or never.
- [ ] Merge `game-called-abandoned` into `main` and tag the release commit.
- [ ] Four rule decisions from QA_Resolution_Report.md section 5: the total-collapse penalty was decided on
      2026-10-12 (kept; see CLAUDE.md). Still yours: the truck can be overloaded during the honk, bankruptcy
      clears the catalogue and trophy shelf, gear other than the drone works while hiding.

## 1. Before the store page
- [ ] Pay the Steam Direct fee; create the app (and a demo app if you keep the demo).
- [ ] Put the App ID in `Data/Networking/NetworkConfig.asset` (SteamAppId) and the demo's
      `Data/Core/Resources/DemoConfig.asset` (StoreAppId = the full game). Run `Tools/unity.sh rebuild`.
- [ ] Docs/STEAM_SETUP.md: achievements (22 API names: Tools/Steam/achievements.csv), Auto-Cloud paths, depots.

## 2. Store page
- [ ] Copy from Docs/STORE_PAGE.md; capsules (header 920x430, small 462x174, main 1232x706, vertical 748x896,
      library assets), 5+ screenshots (F10 + F9), the trailer (Docs/TRAILER.md).
- [ ] Set tags, system requirements, the Early Access questions, price ($7.99–$9.99).
- [ ] Submit the store page for review (takes a few days); go "Coming soon" to collect wishlists.

## 3. Community
- [ ] Create the Discord (channels: announcements, bug-reports, looking-for-crew, clips, suggestions).
- [ ] Put the invite in `Data/Core/Resources/LaunchConfig.asset` (DiscordUrl). Empty fields hide their
      buttons, so do this before the release build.
- [ ] Feedback (0.24.7c): the Steam builds (release and demo) never use FeedbackUrl, which holds the private
      address friends' builds show. They use ReleaseFeedbackUrl (a form, or a public support address as
      `mailto:...`); left empty, the Feedback button, "Report a problem" and the launch notice open the game's
      Steam discussions, from the App ID `set-app-id` wrote. The launch notice shows once per version there.

## 4. Builds
- [ ] Playtest the release candidate: `Tools/unity.sh build` (zips with steam_appid.txt 480 for friends).
- [ ] `Tools/unity.sh build-release` (refuses while the App ID is 480): Game/Builds/MacRelease and WindowsRelease.
- [ ] Upload both as depots with SteamPipe (steamcmd + app_build vdf); set launch options (Abandoned.exe /
      Abandoned.app); set the default branch live on a private "beta" branch first.
- [ ] Submit the build for review (Steam checks it launches); fix and resubmit if needed.

## 5. Launch
- [ ] Launch discount 10–20 % (GDD 22) set in the partner site's discounting tool (the base price must have
      been live for 30 days before any later discount; a launch discount is the exception).
- [ ] Pick the date (avoid big releases and Next Fest weeks); press "Release" on the day.
- [ ] Post the trailer + clips; message the streamers you contacted for the demo.
- [ ] Watch the Discord bug channel; logs come from the in-game "Open logs" / "Report a problem" buttons.

## 6. After launch
- Small regular updates (PLAYBOOK part 6). The post-EA list: Hospital, Hotel, the Mimic/The Weight, layout
  variants, more cosmetics, the winch, the drone, the floodlight (GDD 8, 9, 12).
