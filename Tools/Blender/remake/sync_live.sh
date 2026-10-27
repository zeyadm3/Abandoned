#!/bin/sh
# Mirror the remake sources where the live Blender may read them (macOS keeps it out of ~/Documents).
# Live:  exec(open(os.path.expanduser('~/Library/Caches/AbandonedRemake/src/remake/boot.py')).read())
set -e
SRC="$(cd "$(dirname "$0")" && pwd)"
DST="$HOME/Library/Caches/AbandonedRemake/src/remake"
mkdir -p "$DST"
rsync -a --delete --exclude '__pycache__' "$SRC/" "$DST/"
echo "synced $DST"
