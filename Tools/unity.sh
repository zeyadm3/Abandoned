#!/usr/bin/env bash
# Runs Unity 6000.3.25f1 in batch mode against Game/ and checks the log afterwards.
# Usage: Tools/unity.sh <compile|rebuild|verify|editmode [filter]|playmode [filter]|screenshots|all
#                        |build-mac|build-win|build|build-dev|build-demo|build-release|nettest [scenario]>
# build-release makes the Steam depots (no steam_appid.txt); it refuses while the App ID is still 480.
# build-mac/build-win/build make shareable Mono players in Game/Builds/<Mac|Windows>/ and zip each into
# $BUILD_ZIP_DIR (default ~/Documents/Abandoned-builds/dev). build-dev makes Game/Builds/MacDev (tests).
# nettest runs Tools/nettest.sh (multi-process localhost test; it builds MacDev when stale).
# Unity must NOT be open on the project (batch mode needs the project lock).
# Exit code is non-zero if Unity failed, tests failed, or the log has compile errors / exceptions.
set -uo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
PROJECT="$ROOT/Game"
UNITY="${UNITY:-/Applications/Unity/Hub/Editor/6000.3.25f1/Unity.app/Contents/MacOS/Unity}"
LOGDIR="$PROJECT/Logs/batch"
RESULTS="$PROJECT/TestResults"
mkdir -p "$LOGDIR" "$RESULTS"

if [ -f "$PROJECT/Temp/UnityLockfile" ] && pgrep -f "Unity.app/Contents/MacOS/Unity -projectpath $PROJECT" >/dev/null; then
  echo "Unity editor has the project open; close it first." >&2
  exit 2
fi

# Prints problems found in a log; returns 1 if any.
check_log() {
  local log="$1" bad=0
  if grep -q "error CS" "$log"; then
    echo "--- compile errors:"; grep "error CS" "$log" | sort -u | head -40; bad=1
  fi
  if grep -E "Assets/_Project/.*warning CS" "$log" >/dev/null; then
    echo "--- warnings in our code:"; grep -E "Assets/_Project/.*warning CS" "$log" | sort -u | head -40; bad=1
  fi
  # Exceptions logged anywhere (tests can pass while something threw in the background).
  if grep -E "^[A-Za-z0-9_.]*Exception( |:)" "$log" | grep -v "TestRunner" >/dev/null; then
    echo "--- exceptions:"; grep -E -A3 "^[A-Za-z0-9_.]*Exception( |:)" "$log" | head -60; bad=1
  fi
  return $bad
}

# Summarises an NUnit XML result file; returns 1 on any failure.
summarise_tests() {
  python3 - "$1" <<'PY'
import sys, xml.etree.ElementTree as ET
try:
    root = ET.parse(sys.argv[1]).getroot()
except Exception as e:
    print(f"no test results ({e})"); sys.exit(1)
a = root.attrib
print(f"tests: {a.get('total')} total, {a.get('passed')} passed, {a.get('failed')} failed, {a.get('skipped')} skipped")
failed = 0
for case in root.iter('test-case'):
    if case.attrib.get('result') == 'Failed':
        failed += 1
        msg = case.find('failure/message')
        print(f"FAIL {case.attrib.get('fullname')}\n     {(msg.text or '').strip()[:500] if msg is not None else ''}")
sys.exit(1 if failed or a.get('result','').startswith('Failed') else 0)
PY
}

run_unity() {
  local name="$1"; shift
  local log="$LOGDIR/$name.log"
  echo "=== $name"
  "$UNITY" -batchmode -projectPath "$PROJECT" -logFile "$log" "$@"
  local code=$?
  local bad=0
  check_log "$log" || bad=1
  [ $code -ne 0 ] && echo "Unity exited with $code (log: $log)" && bad=1
  return $bad
}

method() { run_unity "$1" -executeMethod "Abandoned.EditorTools.BatchCommands.$2"; }

# tests <EditMode|PlayMode> [filter]: a filter (NUnit -testFilter regex/names) writes <platform>Filtered.xml.
tests() {
  local platform="$1" filter="${2:-}" name="$1"
  local args=(-runTests -testPlatform "$platform")
  if [ -n "$filter" ]; then name="${platform}Filtered"; args+=(-testFilter "$filter"); fi
  local xml="$RESULTS/$name.xml"
  rm -f "$xml"
  run_unity "$name" "${args[@]}" -testResults "$xml"
  local bad=$?
  summarise_tests "$xml" || bad=1
  return $bad
}

BUILD_ZIP_DIR="${BUILD_ZIP_DIR:-$HOME/Documents/Abandoned-builds/dev}"

# Zips Game/Builds/<folder> into $BUILD_ZIP_DIR/Abandoned-<version>-<commit>-<folder>.zip.
zip_build() {
  local folder="$1" dir="$PROJECT/Builds/$1"
  local info="$dir/BUILD.txt"
  [ -f "$info" ] || { echo "no $info; did the build run?" >&2; return 1; }
  local version commit zip
  version="$(sed -n 's/^version=//p' "$info")"
  commit="$(sed -n 's/^commit=//p' "$info")"
  # Shareable zips should come from a committed tree so the name and the in-game version check
  # point at real code; a -dirty build still zips (local testing) but says so.
  case "$commit" in
    *-dirty|unknown) echo "WARNING: $folder build is from uncommitted changes ($commit); commit first, then rebuild to share it." >&2 ;;
  esac
  mkdir -p "$BUILD_ZIP_DIR"
  zip="$BUILD_ZIP_DIR/Abandoned-$version-$commit-$folder.zip"
  rm -f "$zip"
  # ditto keeps the .app bundle's symlinks and permissions intact; plain zip is fine for Windows.
  if [ "$folder" = "Mac" ]; then
    (cd "$PROJECT/Builds" && ditto -c -k --norsrc --keepParent "$folder" "$zip") || return 1
  else
    (cd "$PROJECT/Builds" && zip -qr "$zip" "$folder") || return 1
  fi
  echo "zipped: $zip ($(du -h "$zip" | cut -f1))"
}

cmd="${1:-all}"
status=0
case "$cmd" in
  compile)     run_unity compile -quit || status=1 ;;
  rebuild)     method rebuild RebuildContent || status=1 ;;
  verify)      method verify VerifyAll || status=1 ;;
  screenshots) method screenshots Screenshots || status=1 ;;
  editmode)    tests EditMode "${2:-}" || status=1 ;;
  playmode)    tests PlayMode "${2:-}" || status=1 ;;
  build-mac)   { method build-mac BuildMac && zip_build Mac; } || status=1 ;;
  build-win)   { method build-win BuildWindows && zip_build Windows; } || status=1 ;;
  build)       { method build BuildBoth && zip_build Mac && zip_build Windows; } || status=1 ;;
  build-dev)   method build-dev BuildMacDev || status=1 ;;
  build-demo)  { method build-demo BuildDemo && zip_build MacDemo && zip_build WindowsDemo; } || status=1 ;;
  build-release) { method build-release BuildRelease && zip_build MacRelease && zip_build WindowsRelease; } || status=1 ;;
  nettest)     shift; exec "$ROOT/Tools/nettest.sh" "$@" ;;
  all)
    run_unity compile -quit || exit 1
    method rebuild RebuildContent || status=1
    method verify VerifyAll || status=1
    tests EditMode || status=1
    tests PlayMode || status=1
    method screenshots Screenshots || status=1
    ;;
  *) echo "unknown command: $cmd" >&2; exit 2 ;;
esac
[ $status -eq 0 ] && echo "=== OK: $cmd" || echo "=== FAILED: $cmd"
exit $status
