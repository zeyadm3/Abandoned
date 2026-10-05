#!/usr/bin/env bash
# Runs Unity 6000.3.25f1 in batch mode against Game/ and checks the log afterwards.
# Usage: Tools/unity.sh <compile|rebuild|verify|editmode|playmode|screenshots|all> [-- extra Unity args]
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

tests() {
  local platform="$1" xml="$RESULTS/$1.xml"
  rm -f "$xml"
  run_unity "$platform" -runTests -testPlatform "$platform" -testResults "$xml"
  local bad=$?
  summarise_tests "$xml" || bad=1
  return $bad
}

cmd="${1:-all}"
status=0
case "$cmd" in
  compile)     run_unity compile -quit || status=1 ;;
  rebuild)     method rebuild RebuildContent || status=1 ;;
  verify)      method verify VerifyAll || status=1 ;;
  screenshots) method screenshots Screenshots || status=1 ;;
  editmode)    tests EditMode || status=1 ;;
  playmode)    tests PlayMode || status=1 ;;
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
