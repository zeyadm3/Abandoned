#!/usr/bin/env python3
"""Author the training job's dispatcher lines: Kokoro speech (kokoro_voice.py) over a handheld radio.

Reads every lesson's RadioLine from Game/Assets/_Project/Data/Tutorial/Steps/*.asset (or, before Unity has written
those, from Editor/Tutorial/TutorialLessons.cs), speaks it without its "DISPATCH:" label, and runs it through a
walkie-talkie: a narrow band, a little grit, a squelch at each end. Each file is named "td_" plus the first 8 hex
digits of the MD5 of the whole line as the game holds it (UTF-8), which is how TutorialRadioVoice finds it. A line
with no recording simply stays a caption.

The lines take turns through the HQ's three dispatch operators (user, 2026-10-12: "keep it random").

    python3 Tools/Audio/generate_tutorial_dispatch.py                        # every line, into the game
    python3 Tools/Audio/generate_tutorial_dispatch.py --voice bf_emma --out /tmp/try --limit 2   # a trial
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
DESTINATION = REPO / "Game/Assets/_Project/Art/Audio/TutorialDispatch/Resources/TutorialDispatch"
VOICES, SPEED, RATE = ["af_heart", "af_bella", "bf_emma"], 0.95, 22050
LABEL = "DISPATCH:"


def yaml_value(raw):
    """A Unity YAML scalar after 'name: ' on one line (the dispatcher's lines are short and never fold)."""
    value = raw.strip()
    if value.startswith("'") and value.endswith("'"):
        return value[1:-1].replace("''", "'")
    if value.startswith('"') and value.endswith('"'):
        return bytes(value[1:-1], "utf-8").decode("unicode_escape")
    return value


FIELD = "<RadioLine>k__BackingField:"


def lines():
    found = []
    if STEPS.exists():
        for path in sorted(STEPS.glob("*.asset")):
            rows = path.read_text(encoding="utf-8").splitlines()
            for i, raw in enumerate(rows):
                stripped = raw.strip()
                if not stripped.startswith(FIELD):
                    continue
                # Unity folds a long scalar onto further, deeper-indented lines (joined back with spaces).
                indent = len(raw) - len(raw.lstrip())
                parts = [stripped[len(FIELD):].strip()]
                for more in rows[i + 1:]:
                    if not more.strip() or len(more) - len(more.lstrip()) <= indent:
                        break
                    parts.append(more.strip())
                text = yaml_value(" ".join(parts))
                if text: found.append(text)
    if not found and LESSONS.exists():
        source = LESSONS.read_text(encoding="utf-8")
        found = [m.group(1).replace('\\"', '"') for m in re.finditer(r'radio:\s*"((?:[^"\\]|\\.)*)"', source)]
    seen, unique = set(), []
    for text in found:
        if text not in seen:
            seen.add(text)
            unique.append(text)
    return unique


def spoken(text):
    return text[len(LABEL):].strip() if text.startswith(LABEL) else text


def radio(raw_file, seed):
    with wave.open(str(raw_file), "rb") as source:
        rate = source.getframerate()
        samples = struct.unpack("<%dh" % source.getnframes(), source.readframes(source.getnframes()))
    rng = random.Random(seed)
    # A handheld's speaker: nothing under 400 Hz or over 2.8 kHz, driven a little hard.
    hp = math.exp(-2 * math.pi * 400 / rate)
    lp = 1 - math.exp(-2 * math.pi * 2800 / rate)
    prev_in = prev_hp = low1 = low2 = 0.0
    out = []
    for pcm in samples:
        v = pcm / 32768.0
        h = hp * (prev_hp + v - prev_in)
        prev_in, prev_hp = v, h
        low1 += lp * (h - low1)
        low2 += lp * (low1 - low2)
        out.append(math.tanh(low2 * 2.4) + rng.uniform(-1, 1) * 0.01)
    peak = max((abs(v) for v in out), default=1.0) or 1.0
    # The squelch: a short burst of hiss that cuts off, before and after the voice.
    def burst(seconds, fade_in):
        n = int(rate * seconds)
        return [rng.uniform(-1, 1) * 0.22 * ((i / n) if fade_in else (1 - i / n)) for i in range(n)]
    frames = burst(0.09, True) + [0.0] * int(rate * 0.05) + [v * 0.75 / peak for v in out] + [0.0] * int(rate * 0.04) + burst(0.12, False)
    return rate, struct.pack("<%dh" % len(frames), *(int(max(-1, min(1, v)) * 32767) for v in frames))


def main():
    parser = argparse.ArgumentParser(description=__doc__.splitlines()[0])
    parser.add_argument("--voice", help="try one Kokoro voice for every line (use with --out)")
    parser.add_argument("--out", type=Path, default=DESTINATION, help="folder to write to (default: the game's)")
    parser.add_argument("--limit", type=int, default=0, help="only the first N lines (a trial)")
    options = parser.parse_args()
    if options.out == DESTINATION and (options.voice or options.limit > 0):
        parser.error("--voice and --limit are for trials: give --out a folder outside the game")
    options.out.mkdir(parents=True, exist_ok=True)
    texts = lines()
    if options.limit > 0: texts = texts[:options.limit]
    if not texts: raise SystemExit("No dispatcher lines found (run Tools > Abandoned > Maps > Tutorial House > Build Tutorial Lessons first).")
    names = ["td_" + hashlib.md5(text.encode("utf-8")).hexdigest()[:8] for text in texts]
    voices = [options.voice or VOICES[i % len(VOICES)] for i in range(len(texts))]
    readme = ["# The training job's dispatcher", "",
              "Spoken on this machine by the Kokoro-82M speech model (Apache-2.0 weights) through",
              "Tools/Audio/generate_tutorial_dispatch.py and run through a handheld radio. No paid service and no",
              "recording of a real person; never `say`. Lines take turns through " + ", ".join(VOICES) + ".", ""]
    with tempfile.TemporaryDirectory(prefix="abandoned_tutorial_dispatch_") as directory:
        raws = [Path(directory) / (name + ".wav") for name in names]
        for voice in sorted(set(voices)):
            kokoro_voice.speak([(raws[i], spoken(texts[i])) for i in range(len(texts)) if voices[i] == voice], voice, SPEED, RATE)
        for i, (name, text) in enumerate(zip(names, texts)):
            rate, frames = radio(raws[i], i)
            with wave.open(str(options.out / (name + ".wav")), "wb") as w:
                w.setnchannels(1); w.setsampwidth(2); w.setframerate(rate); w.writeframes(frames)
            readme.extend(["## %s (%s)" % (name, voices[i]), "", text, ""])
            print("Authored", name, voices[i], text[:60])
    if options.out == DESTINATION and options.limit <= 0:
        (DESTINATION.parent.parent / "README.md").write_text("\n".join(readme), encoding="utf-8")


if __name__ == "__main__":
    main()
