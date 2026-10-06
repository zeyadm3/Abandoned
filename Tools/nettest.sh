#!/usr/bin/env bash
# Multi-process localhost network test: 1 host + N clients of a dev Mac player, headless, over
# Unity Transport on 127.0.0.1. Each instance runs the named scenario (NetTestRunner/NetTestScenarios),
# writes a JSON result and quits; this script waits, kills stragglers, checks every result and log.
#
# Usage: Tools/nettest.sh [scenario] [--clients N] [--timeout S] [--rebuild | --no-build]
#   scenario    default "basic" (see NetTestScenarios.cs for the list)
#   --clients   clients besides the host (default 3 -> 4 players)
#   --timeout   seconds before stragglers are killed (default 120)
#   --rebuild   always rebuild Game/Builds/MacDev first; --no-build never does (default: when stale)
# Exit code 0 only when every instance passed and no log has an exception.
set -uo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
PROJECT="$ROOT/Game"
APP="$PROJECT/Builds/MacDev/Abandoned.app"
BIN="$APP/Contents/MacOS/Abandoned"
OUT="$PROJECT/Logs/nettest"

scenario="basic"; clients=3; timeout=120; build="auto"
while [ $# -gt 0 ]; do
  case "$1" in
    --clients) clients="$2"; shift 2 ;;
    --timeout) timeout="$2"; shift 2 ;;
    --rebuild) build="always"; shift ;;
    --no-build) build="never"; shift ;;
    -*) echo "unknown option $1" >&2; exit 2 ;;
    *) scenario="$1"; shift ;;
  esac
done

# Stale = any asset, package or setting newer than the build's BUILD.txt (written after the build
# script resets the stamped BuildInfo asset, so that reset doesn't count as a change).
STAMP="$PROJECT/Builds/MacDev/BUILD.txt"
needs_build() {
  [ -x "$BIN" ] && [ -f "$STAMP" ] || return 0
  [ -n "$(find "$PROJECT/Assets" "$PROJECT/Packages" "$PROJECT/ProjectSettings" -newer "$STAMP" -type f \
          ! -name '*.meta' -print -quit 2>/dev/null)" ]
}

if [ "$build" = "always" ] || { [ "$build" = "auto" ] && needs_build; }; then
  echo "=== nettest: building dev Mac player"
  "$ROOT/Tools/unity.sh" build-dev || { echo "=== FAILED: nettest (dev build failed)"; exit 1; }
elif [ ! -x "$BIN" ]; then
  echo "no dev build at $APP (run without --no-build)" >&2; exit 1
else
  echo "=== nettest: reusing fresh dev build"
fi

# A free UDP port, so a hosting editor or another test on 7777 doesn't collide.
port="$(python3 -c 'import socket; s=socket.socket(socket.AF_INET, socket.SOCK_DGRAM); s.bind(("127.0.0.1",0)); print(s.getsockname()[1]); s.close()')"
rm -rf "$OUT"; mkdir -p "$OUT"
echo "=== nettest '$scenario': host + $clients client(s) on 127.0.0.1:$port (timeout ${timeout}s)"

pids=()
launch() { # role name
  "$BIN" -batchmode -nographics -logFile "$OUT/$2.log" \
    -nettest "$1" -nettestPort "$port" -nettestScenario "$scenario" -nettestClients "$clients" \
    -nettestTimeout "$((timeout - 10))" -nettestOut "$OUT/$2.json" >/dev/null 2>&1 &
  pids+=($!)
}

deadline=$((SECONDS + timeout))
launch host host
# Clients only retry for ~10 s (NetworkConfig.MaxConnectAttempts), so wait until the host listens.
while [ ! -f "$OUT/host.json.ready" ] && kill -0 "${pids[0]}" 2>/dev/null && [ $SECONDS -lt $deadline ]; do sleep 0.2; done
if [ -f "$OUT/host.json.ready" ]; then
  # Not seq: BSD seq 1 0 counts down and would launch two clients.
  for ((i = 1; i <= clients; i++)); do launch client "client$i"; done
else
  echo "host never started listening (see $OUT/host.log)"
fi

while [ $SECONDS -lt $deadline ]; do
  alive=0
  for pid in "${pids[@]}"; do kill -0 "$pid" 2>/dev/null && alive=1; done
  [ $alive -eq 0 ] && break
  sleep 1
done
killed=0
for pid in "${pids[@]}"; do
  if kill -0 "$pid" 2>/dev/null; then kill "$pid" 2>/dev/null; killed=$((killed + 1)); fi
done
sleep 1
for pid in "${pids[@]}"; do kill -9 "$pid" 2>/dev/null; done
[ $killed -gt 0 ] && echo "killed $killed straggler(s) after ${timeout}s"

python3 - "$OUT" "$clients" <<'PY'
import json, os, re, sys
out, clients = sys.argv[1], int(sys.argv[2])
names = ["host"] + [f"client{i}" for i in range(1, clients + 1)]
# Clients that never launched have no log/result; they still count as failures below.
exc = re.compile(r"^[A-Za-z0-9_.]*Exception( |:)")
# Errors that mean the run isn't testing what it claims, even when every scenario step passed.
fatal = re.compile(r"doesn't match the host's|^\[Netcode\].*\b(Error|Exception)\b")
bad = 0
for n in names:
    path = os.path.join(out, n + ".json")
    log = os.path.join(out, n + ".log")
    problems = []
    try:
        r = json.load(open(path))
    except Exception as e:
        r = None
        problems.append(f"no result ({e.__class__.__name__})")
    if r is not None and not r.get("passed"):
        problems += r.get("errors") or ["failed without an error message"]
    if os.path.exists(log):
        hits = [l.rstrip() for l in open(log, errors="replace") if exc.match(l) or fatal.search(l)]
        problems += [f"exception in log: {h}" for h in hits[:5]]
    else:
        problems.append("no log")
    if r is not None:
        detail = f"client {r.get('clientId')}, {r.get('seconds', 0):.1f}s, {r.get('version')}"
        for note in r.get("notes", []): detail += f"\n      - {note}"
    else:
        detail = ""
    print(f"{'PASS' if not problems else 'FAIL'}  {n:8s} {detail}")
    for p in problems: print(f"      ! {p}")
    bad += bool(problems)
host = os.path.join(out, "host.json")
if os.path.exists(host):
    r = json.load(open(host))
    for v in r.get("views", []):
        seen = ", ".join(f"P{p['owner']}({p['position']['x']:.2f},{p['position']['y']:.2f},{p['position']['z']:.2f})" for p in v["players"])
        print(f"      machine {v['observer']} sees: {seen}")
print(f"nettest: {len(names) - bad}/{len(names)} instances passed (results + logs in {out})")
sys.exit(1 if bad else 0)
PY
status=$?
[ $status -eq 0 ] && echo "=== OK: nettest $scenario" || echo "=== FAILED: nettest $scenario"
exit $status
