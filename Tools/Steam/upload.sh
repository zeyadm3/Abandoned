#!/bin/bash
# Uploads the release depots with SteamPipe (0.24.6). Needs steamcmd installed and a Steamworks account with
# build rights. Usage: Tools/Steam/upload.sh <app_id> <windows_depot_id> <mac_depot_id> <steam_login>
set -euo pipefail
cd "$(dirname "$0")/steampipe"
[ $# -eq 4 ] || { echo "usage: $0 <app_id> <windows_depot_id> <mac_depot_id> <steam_login>"; exit 1; }
mkdir -p ../work
for f in app_build.vdf depot_windows.vdf depot_mac.vdf; do
  sed -e "s/APP_ID/$1/g" -e "s/WINDOWS_DEPOT_ID/$2/g" -e "s/MAC_DEPOT_ID/$3/g" "$f" > "../work/$f"
done
steamcmd +login "$4" +run_app_build "$(cd ../work && pwd)/app_build.vdf" +quit
