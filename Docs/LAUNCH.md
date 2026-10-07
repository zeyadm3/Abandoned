# Early Access launch checklist (M10.11)

Everything that has to happen outside the code, in order. The game side is done (M3–M10); see
Docs/PROGRESS.md "Needs you" for the playtests that come first.

## 1. Before the store page
- [ ] Pay the Steam Direct fee; create the app (and a demo app if you keep the demo).
- [ ] Put the App ID in `Data/Networking/NetworkConfig.asset` (SteamAppId) and the demo's
      `Data/Core/Resources/DemoConfig.asset` (StoreAppId = the full game). Run `Tools/unity.sh rebuild`.
- [ ] Docs/STEAM_SETUP.md: achievements (12 API names), Auto-Cloud paths, depots.

## 2. Store page
- [ ] Copy from Docs/STORE_PAGE.md; capsules (header 920x430, small 462x174, main 1232x706, vertical 748x896,
      library assets), 5+ screenshots (F10 + F9), the trailer (Docs/TRAILER.md).
- [ ] Set tags, system requirements, the Early Access questions, price ($7.99–$9.99).
- [ ] Submit the store page for review (takes a few days); go "Coming soon" to collect wishlists.

## 3. Community
- [ ] Create the Discord (channels: announcements, bug-reports, looking-for-crew, clips, suggestions).
- [ ] Put the invite in `Data/Core/Resources/LaunchConfig.asset` (DiscordUrl) and a feedback form or the Steam
      discussions URL in FeedbackUrl. Empty fields hide their buttons, so do this before the release build.

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
