# Facepunch Transport for Netcode for GameObjects (ABANDONED fork)

By **Nico Thomas**, **Floris van Onna**<br>
Credits to **Garry Newman** (Author of Facepunch.Steamworks)

This is an **embedded, patched fork** used by ABANDONED. Do not replace it with the upstream git package.

- Upstream: `com.community.netcode.transport.facepunch` 2.0.0, commit `2444fe2277eb6e2ca9b01ea28658ec6988aed6c2`
  (targets NGO 1.0; compiles unchanged against NGO 2.13.3).
- Uses **Facepunch.Steamworks 2.5.2** (universal macOS native library, works on Apple Silicon).
- Patches (see CHANGELOG.md): Steam lifetime owned by the game (`Abandoned.Networking.SteamBootstrap`),
  StartClient/StartServer fail cleanly without Steam, real RTT from Steam's connection ping.

## Plugin import settings (checked by Tools/Abandoned/Verify Project Setup)
| File | Editor | Players |
|---|---|---|
| `Facepunch.Steamworks.Posix.dll` | macOS | macOS, Linux x64 |
| `Facepunch.Steamworks.Win64.dll` | Windows | Windows x64 |
| `redistributable_bin/osx/libsteam_api.dylib` (universal) | macOS | macOS |
| `redistributable_bin/win64/steam_api64.dll` | Windows | Windows x64 |
| `redistributable_bin/linux64/libsteam_api.so` | Linux | Linux x64 |

`Tools/Abandoned/Fix/Apply Steam Plugin Settings` re-applies them (also part of the batch rebuild).
A Linux editor is not supported (Unity's plugin importer allows one editor OS per file).

## Using it
Steam must be initialised by `SteamBootstrap` before `NetworkManager.StartHost/StartClient` with this
transport; otherwise they return false with a clear log. Set `targetSteamId` to the host's Steam ID on
clients (the lobby code does this).
