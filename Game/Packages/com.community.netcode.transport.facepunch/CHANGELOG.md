# Changelog
All notable changes to this package will be documented in this file. The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)

## 2.0.0-abandoned.2 (ABANDONED fork, 2026-10-06)
### Fixed
- Patch 4: a client drains pending messages before reporting a disconnect, and the host closes remote
  connections with linger, so a disconnect reason sent just before ("The host left the game.", full,
  other build) reaches NGO instead of being dropped (M3 review).

## 2.0.0-abandoned.1 (ABANDONED fork, 2026-10-06)
Embedded copy of upstream 2.0.0 (git commit 2444fe2) in Game/Packages/, patched for ABANDONED.

### Changed
- Facepunch.Steamworks 2.3.2 -> **2.5.2** (2026-04-23): `Facepunch.Steamworks.Posix.dll` (macOS editor +
  macOS/Linux players), `Facepunch.Steamworks.Win64.dll` (Windows editor + Windows x64 player), universal
  x86_64+arm64 `libsteam_api.dylib` (the old one was i386/x86_64 only -> DllNotFoundException on Apple
  Silicon), `steam_api64.dll`, linux64 `libsteam_api.so`.
- Patch 1: `Shutdown()` no longer calls `SteamClient.Shutdown()`. Ending a network session must not kill
  Steam (lobbies, invites, rich presence). The game's `Abandoned.Networking.SteamBootstrap` owns Steam.
- Patch 1b: `Initialize()` no longer calls `SteamClient.Init()` and `OnEarlyUpdate()` no longer calls
  `SteamClient.RunCallbacks()` (same reason: one owner of Steam's lifetime; also keeps tests from
  starting Steam on the developer's logged-in account).
- Patch 2: `StartClient()` / `StartServer()` return false and log `SteamNotRunningMessage` when
  `SteamClient.IsValid` is false (upstream returned true and then threw on first use).
- Patch 3: `GetCurrentRtt()` returns Steam's connection ping (`Connection.QuickStatus().Ping`, ms)
  instead of always 0.
- Null-safe log level (works without `NetworkManager.Singleton`), `Send` to a missing server connection
  logs instead of throwing, unused `InitSteamworks` coroutine and `steamAppId` field removed (the App ID
  lives in the game's NetworkConfig asset).

### Removed
- 32-bit leftovers: linux32 `libsteam_api.so`, `steam_api.dll`, and the `.lib` import libraries.
- `WindowsStandalone32` from the assembly's platforms (no 32-bit Steamworks DLL ships).

## 2.0.0

### Changed
- Targets the Netcode for GameObjects 1.0.0 package.
- Renamed namespaces from MLAPI to Netcode.

### Removed
- Removed support for channels.
- No longer send 1 byte of channel information in each message.

## 1.0.0
First version of the Facepunch Transport as a Unity package.