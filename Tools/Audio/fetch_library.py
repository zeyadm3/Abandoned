#!/usr/bin/env python3
"""Fetches the 0.24.12 recorded sound library: CC0 sounds from Freesound (Creative Commons 0 only, filtered in the
search itself), their high-quality previews, into Game/Assets/_Project/Art/Audio/LibrarySource/<target>/.

    python3 Tools/Audio/fetch_library.py           fetch targets that have no files yet
    python3 Tools/Audio/fetch_library.py --force   fetch every target again

A target is where the sound goes in the bank (Editor/Audio/SoundLibrary reads the folder names):
    Steps/<SurfaceMaterial>          footsteps on that surface
    Impacts/<SurfaceMaterial>/Light  knocks; .../Heavy drops and hard hits
    Cues/<SoundId>                   a named sound (one-shots and loops)
Editor/Audio/SoundLibrary then trims, fades, levels (and crossfades loops) into Art/Audio/Library and fills the bank.
Every file is listed in Tools/Audio/library.csv (id, title, author, licence, page) for Docs/ASSET_CREDITS.md.
Never: the heartbeat, breathing, monster signatures and monster songs (their sound is part of the rules).
"""
import csv, html, math, os, re, sys, time, urllib.parse, urllib.request

ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
OUT = os.path.join(ROOT, "Game", "Assets", "_Project", "Art", "Audio", "LibrarySource")
CSV = os.path.join(ROOT, "Tools", "Audio", "library.csv")
AGENT = {"User-Agent": "Mozilla/5.0 (Abandoned sound library fetch; CC0 previews)"}
NEVER = ("music", "song", "melody", "beat ", "voice over", "speech", "talking", "laugh", "loop" + "ed music")

# target, query, how many, min seconds, max seconds, words that disqualify a title
T = [
    ("Steps/Concrete", "footstep concrete", 10, 0.1, 1.0, ()),
    ("Steps/Tile", "footstep tile", 8, 0.1, 1.0, ()),
    ("Steps/Wood", "footstep wood floor", 10, 0.1, 1.0, ()),
    ("Steps/Metal", "footstep metal", 8, 0.1, 1.0, ()),
    ("Steps/Carpet", "footstep carpet", 8, 0.1, 1.0, ()),
    ("Steps/Dirt", "footstep gravel", 8, 0.1, 1.0, ()),
    ("Steps/Glass", "glass crunch step", 6, 0.1, 1.5, ()),
    ("Steps/Water", "footstep puddle", 6, 0.1, 1.2, ()),
    ("Impacts/Metal/Light", "metal impact", 6, 0.1, 1.2, ("door",)),
    ("Impacts/Metal/Heavy", "heavy metal impact", 5, 0.3, 2.5, ("door", "rock")),
    ("Impacts/Wood/Light", "wood impact", 6, 0.1, 1.2, ("door",)),
    ("Impacts/Wood/Heavy", "heavy wood impact", 5, 0.3, 2.5, ("door",)),
    ("Impacts/Glass/Light", "glass clink", 6, 0.1, 1.2, ()),
    ("Impacts/Glass/Heavy", "glass break", 5, 0.4, 3.0, ("window",)),
    ("Impacts/Plastic/Light", "plastic impact", 6, 0.1, 1.2, ()),
    ("Impacts/Plastic/Heavy", "plastic drop", 4, 0.2, 2.0, ()),
    ("Impacts/Concrete/Light", "stone impact", 6, 0.1, 1.2, ()),
    ("Impacts/Concrete/Heavy", "concrete impact heavy", 5, 0.3, 2.5, ()),
    ("Impacts/Paper/Light", "cardboard box drop", 5, 0.1, 1.5, ()),
    ("Impacts/Fabric/Light", "bag drop", 5, 0.1, 1.5, ()),
    ("Impacts/Fabric/Heavy", "sandbag drop", 4, 0.2, 2.0, ()),
    ("Impacts/Dirt/Light", "dirt impact", 5, 0.1, 1.5, ()),
    ("Cues/Creak", "wood creak", 6, 0.5, 4.0, ("door",)),
    ("Cues/Groan", "metal groan", 5, 1.0, 6.0, ()),
    ("Cues/Snap", "wood snap", 5, 0.1, 1.5, ()),
    ("Cues/CrashDebris", "debris falling", 5, 1.0, 6.0, ()),
    ("Cues/RubbleLand", "rubble impact", 4, 0.3, 3.0, ()),
    ("Cues/DistantSettle", "building creak", 4, 1.0, 6.0, ("door",)),
    ("Cues/DeepRumble", "deep rumble", 3, 2.0, 8.0, ()),
    ("Cues/Aftershock", "earthquake rumble", 3, 2.0, 10.0, ()),
    ("Cues/FixtureFall", "metal crash", 4, 0.5, 3.0, ("car",)),
    ("Cues/GlassCrunch", "glass crunch", 4, 0.2, 1.5, ()),
    ("Cues/GlassFloorCrack", "ice crack", 3, 0.3, 2.0, ()),
    ("Cues/TimberFloorCrack", "wood crack", 3, 0.3, 2.0, ()),
    ("Cues/TileFloorCrack", "ceramic crack", 3, 0.2, 2.0, ()),
    ("Cues/DoorSlam", "door slam", 4, 0.3, 2.5, ("car",)),
    ("Cues/DoorSlide", "sliding door metal", 3, 0.5, 3.0, ("car", "elevator")),
    ("Cues/DoorShut", "door close", 4, 0.3, 2.0, ("car",)),
    ("Cues/DoorBash", "door kick", 3, 0.2, 1.5, ()),
    ("Cues/DoorBreak", "door break wood", 3, 0.5, 3.0, ()),
    ("Cues/LockerOpen", "locker open", 3, 0.2, 2.0, ()),
    ("Cues/LockerShut", "locker close", 3, 0.2, 2.0, ()),
    ("Cues/BreakerThrow", "breaker switch", 3, 0.1, 1.5, ()),
    ("Cues/Lever", "lever metal", 3, 0.1, 1.5, ()),
    ("Cues/ShutterOpen", "roller shutter", 2, 1.0, 5.0, ()),
    ("Cues/ShutterSlam", "roller shutter close", 2, 0.5, 4.0, ()),
    ("Cues/FlashlightClick", "flashlight click", 3, 0.05, 0.6, ()),
    ("Cues/LootPickup", "pick up item", 4, 0.1, 1.0, ()),
    ("Cues/LootRattle", "rattle", 3, 0.2, 1.5, ("snake",)),
    ("Cues/LootBreak", "glass break", 4, 0.3, 2.5, ("window",)),
    ("Cues/BodyFall", "body fall", 3, 0.2, 1.5, ()),
    ("Cues/PlayerLand", "jump land", 3, 0.1, 1.0, ()),
    ("Cues/PlayerHurt", "pain grunt male", 4, 0.2, 1.2, ()),
    ("Cues/Shove", "punch", 3, 0.1, 0.8, ()),
    ("Cues/SledgeHit", "sledgehammer", 3, 0.2, 1.5, ()),
    ("Cues/CrowbarHit", "crowbar", 3, 0.2, 1.5, ()),
    ("Cues/WaterDrip", "water drip", 4, 0.05, 1.0, ()),
    ("Cues/PuddleSplash", "splash puddle", 3, 0.2, 1.5, ()),
    ("Cues/LightBuzz", "fluorescent buzz", 2, 0.5, 4.0, ()),
    ("Cues/PowerDown", "power down", 2, 1.0, 4.0, ("game", "synth")),
    ("Cues/PowerUp", "generator start", 2, 1.0, 5.0, ()),
    ("Cues/AlarmBell", "alarm bell", 2, 1.0, 6.0, ("clock",)),
    ("Cues/Thunder", "thunder", 3, 2.0, 10.0, ()),
    ("Cues/Foghorn", "foghorn", 2, 1.0, 6.0, ()),
    ("Cues/SteamBlast", "steam hiss", 3, 0.5, 4.0, ()),
    ("Cues/FactoryChainRattle", "chain rattle", 3, 0.5, 4.0, ()),
    ("Cues/Knock", "knock door", 3, 0.3, 2.5, ()),
    ("Cues/Whisper", "whisper", 3, 0.5, 4.0, ()),
    ("Cues/CeilingScuttle", "scratching", 3, 0.5, 4.0, ()),
    ("Cues/Roar", "monster roar", 3, 0.5, 5.0, ()),
    ("Cues/Scream", "creature scream", 3, 0.5, 4.0, ()),
    ("Cues/ThreatStrike", "monster growl", 3, 0.3, 2.5, ()),
    ("Cues/WindLoop", "wind loop", 2, 10.0, 60.0, ()),
    ("Cues/HumLoop", "electrical hum", 2, 5.0, 60.0, ()),
    ("Cues/RoomToneLoop", "room tone", 2, 10.0, 90.0, ()),
    ("Cues/SurfLoop", "ocean waves", 2, 10.0, 90.0, ()),
    ("Cues/TravelEngineLoop", "van interior engine", 1, 10.0, 60.0, ()),
    ("Cues/EngineIdle", "truck engine idle", 1, 5.0, 40.0, ()),
]

ATTR = re.compile(r'data-([a-z-]+)="([^"]*)"')


def search(query, lo, hi, page):
    f = f'license:"Creative Commons 0" duration:[{lo} TO {hi}]'
    url = "https://freesound.org/search/?" + urllib.parse.urlencode({"q": query, "f": f, "page": page})
    text = urllib.request.urlopen(urllib.request.Request(url, headers=AGENT), timeout=60).read().decode("utf-8", "replace")
    results = []
    for block in text.split('class="bw-player"')[1:]:
        attrs = dict(ATTR.findall(block[:2500]))
        rating = re.search(r'Average rating of ([0-9.]+)', block[:6000])
        if "sound-id" not in attrs or "ogg" not in attrs:
            continue
        results.append({
            "id": attrs["sound-id"], "user": attrs.get("username", ""), "title": html.unescape(attrs.get("title", "")),
            "duration": float(attrs.get("duration", "0") or 0), "downloads": int(attrs.get("num-downloads", "0") or 0),
            "rating": float(rating.group(1)) if rating else 3.0,
            "ogg": attrs["ogg"].replace("-lq.ogg", "-hq.ogg"),
        })
    return results


def fetch_target(target, query, count, lo, hi, exclude, writer, force):
    folder = os.path.join(OUT, *target.split("/"))
    if not force and os.path.isdir(folder) and any(f.endswith(".ogg") for f in os.listdir(folder)):
        print(f"  {target}: already here")
        return 0
    os.makedirs(folder, exist_ok=True)
    seen, found = set(), []
    for page in (1, 2):
        for r in search(query, lo, hi, page):
            if r["id"] in seen:
                continue
            seen.add(r["id"])
            low = r["title"].lower()
            if any(w in low for w in NEVER + tuple(exclude)):
                continue
            found.append(r)
        time.sleep(0.6)
    # Well-liked and well-used first; one uploader at most three times, so one microphone doesn't make every take.
    found.sort(key=lambda r: -(r["rating"] * math.log10(10 + r["downloads"])))
    picked, per_user = [], {}
    for r in found:
        if per_user.get(r["user"], 0) >= 3:
            continue
        per_user[r["user"]] = per_user.get(r["user"], 0) + 1
        picked.append(r)
        if len(picked) >= count:
            break
    for r in picked:
        data = urllib.request.urlopen(urllib.request.Request(r["ogg"], headers=AGENT), timeout=120).read()
        with open(os.path.join(folder, f"fs{r['id']}.ogg"), "wb") as out:
            out.write(data)
        writer.writerow([target, r["id"], r["title"], r["user"], "CC0 1.0", f"https://freesound.org/people/{r['user']}/sounds/{r['id']}/"])
        time.sleep(0.3)
    print(f"  {target}: {len(picked)} of {len(found)} candidates ({query})")
    return len(picked)


def main():
    force = "--force" in sys.argv
    os.makedirs(OUT, exist_ok=True)
    existing = []
    if os.path.isfile(CSV) and not force:
        with open(CSV, newline="", encoding="utf-8") as f:
            existing = list(csv.reader(f))[1:]
    with open(CSV, "w", newline="", encoding="utf-8") as f:
        w = csv.writer(f)
        w.writerow(["target", "freesound_id", "title", "author", "licence", "page"])
        for row in existing:
            w.writerow(row)
        total, failed = 0, []
        for target, query, count, lo, hi, exclude in T:
            try:
                total += fetch_target(target, query, count, lo, hi, exclude, w, force)
                f.flush()
            except Exception as e:
                failed.append(target)
                print(f"  {target}: FAILED {e}")
    print(f"fetched {total} sounds; failed: {', '.join(failed) if failed else 'none'}")
    return 0


if __name__ == "__main__":
    sys.exit(main())
