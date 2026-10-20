#!/usr/bin/env python3
"""Author the training job's trainer: every lesson's spoken line, Kokoro speech (kokoro_voice.py) over a clean radio.

Reads each lesson's VoiceLine from Game/Assets/_Project/Data/Tutorial/Steps/*.asset (or, before Unity has written
those, the voice: lines in Editor/Tutorial/TutorialLessons.cs) and speaks it in one steady voice, lightly band-limited
so it sits on the same radio as the dispatcher but stays easy to follow. Each file is named "tn_" plus the first 8
hex digits of the MD5 of the line as the game holds it (UTF-8), which is how TutorialRadioVoice finds it. A line
with no recording is simply not spoken; the lesson card still shows it.

    python3 Tools/Audio/generate_tutorial_narration.py                         # every line, into the game
    python3 Tools/Audio/generate_tutorial_narration.py --voice am_fenrir --out /tmp/try --limit 3   # a trial
"""

from pathlib import Path
import argparse
import hashlib
import math
import random
import re
import struct
import tempfile
import wave

import kokoro_voice

REPO = Path(__file__).resolve().parents[2]
STEPS = REPO / "Game/Assets/_Project/Data/Tutorial/Steps"
LESSONS = REPO / "Game/Assets/_Project/Editor/Tutorial/TutorialLessons.cs"
DESTINATION = REPO / "Game/Assets/_Project/Art/Audio/TutorialNarration/Resources/TutorialNarration"
VOICE, SPEED, RATE = "am_michael", 0.97, 22050
FIELD = "<VoiceLine>k__BackingField:"


def unquote(value):
    """A Unity YAML scalar, folded lines already joined with spaces."""
    value = value.strip()
    if value.startswith("'") and value.endswith("'"):
        return value[1:-1].replace("''", "'")
    if value.startswith('"') and value.endswith('"'):
        return bytes(value[1:-1], "utf-8").decode("unicode_escape")
    return value


def from_assets():
    found = []
    for path in sorted(STEPS.glob("*.asset")):
        rows = path.read_text(encoding="utf-8").splitlines()
        for i, raw in enumerate(rows):
            stripped = raw.strip()
            if not stripped.startswith(FIELD):
                continue
            indent = len(raw) - len(raw.lstrip())
            parts = [stripped[len(FIELD):].strip()]
            # Unity folds a long scalar onto further, deeper-indented lines.
            for more in rows[i + 1:]:
                if len(more) - len(more.lstrip()) <= indent or not more.strip():
                    break
                parts.append(more.strip())
            text = unquote(" ".join(parts))
            if text:
                found.append(text)
    return found


def lines():
    found = from_assets() if STEPS.exists() else []
    if not found and LESSONS.exists():
        source = LESSONS.read_text(encoding="utf-8")
        found = [m.group(1).replace('\\"', '"') for m in re.finditer(r'voice:\s*"((?:[^"\\]|\\.)*)"', source)]
    seen, unique = set(), []
    for text in found:
        if text not in seen:
            seen.add(text)
            unique.append(text)
    return unique


def clean_radio(raw_file, seed):
    """The trainer's set: a wide band (220 Hz to 5 kHz), barely driven, a soft click either side."""
    with wave.open(str(raw_file), "rb") as source:
        rate = source.getframerate()
        samples = struct.unpack("<%dh" % source.getnframes(), source.readframes(source.getnframes()))
    rng = random.Random(seed)
    hp = math.exp(-2 * math.pi * 220 / rate)
    lp = 1 - math.exp(-2 * math.pi * 5000 / rate)
    prev_in = prev_hp = low1 = low2 = 0.0
    out = []
    for pcm in samples:
        v = pcm / 32768.0
        h = hp * (prev_hp + v - prev_in)
        prev_in, prev_hp = v, h
        low1 += lp * (h - low1)
        low2 += lp * (low1 - low2)
        out.append(math.tanh(low2 * 1.3) + rng.uniform(-1, 1) * 0.003)
    peak = max((abs(v) for v in out), default=1.0) or 1.0

    def click(seconds, fade_in):
        n = int(rate * seconds)
        return [rng.uniform(-1, 1) * 0.07 * ((i / n) if fade_in else (1 - i / n)) for i in range(n)]

    frames = click(0.03, True) + [0.0] * int(rate * 0.06) + [v * 0.82 / peak for v in out] + [0.0] * int(rate * 0.05) + click(0.04, False)
    return rate, struct.pack("<%dh" % len(frames), *(int(max(-1, min(1, v)) * 32767) for v in frames))


def main():
    parser = argparse.ArgumentParser(description=__doc__.splitlines()[0])
    parser.add_argument("--voice", default=VOICE, help="the Kokoro voice (default %s)" % VOICE)
    parser.add_argument("--out", type=Path, default=DESTINATION, help="folder to write to (default: the game's)")
    parser.add_argument("--limit", type=int, default=0, help="only the first N lines (a trial)")
    options = parser.parse_args()
    if options.out == DESTINATION and (options.voice != VOICE or options.limit > 0):
        parser.error("--voice and --limit are for trials: give --out a folder outside the game")
    options.out.mkdir(parents=True, exist_ok=True)
    texts = lines()
    if options.limit > 0:
        texts = texts[:options.limit]
    if not texts:
        raise SystemExit("No trainer lines found (TutorialLessons.cs voice: lines, or the lesson assets' VoiceLine).")
    names = ["tn_" + hashlib.md5(text.encode("utf-8")).hexdigest()[:8] for text in texts]
    readme = ["# The training job's trainer", "",
              "Every lesson read out by the Kokoro-82M speech model (Apache-2.0 weights), voice %s, through" % options.voice,
              "Tools/Audio/generate_tutorial_narration.py and a light radio filter. No paid service and no recording of a",
              "real person; never `say`. English only: the lesson card carries the text in every language.", ""]
    with tempfile.TemporaryDirectory(prefix="abandoned_tutorial_narration_") as directory:
        raws = [Path(directory) / (name + ".wav") for name in names]
        kokoro_voice.speak(list(zip(raws, texts)), options.voice, SPEED, RATE)
        for i, (name, text) in enumerate(zip(names, texts)):
            rate, frames = clean_radio(raws[i], i)
            with wave.open(str(options.out / (name + ".wav")), "wb") as w:
                w.setnchannels(1); w.setsampwidth(2); w.setframerate(rate); w.writeframes(frames)
            readme.extend(["## %s" % name, "", text, ""])
            print("Authored", name, text[:70])
    if options.out == DESTINATION and options.limit <= 0:
        (DESTINATION.parent.parent / "README.md").write_text("\n".join(readme), encoding="utf-8")


if __name__ == "__main__":
    main()
