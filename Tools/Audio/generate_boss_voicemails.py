#!/usr/bin/env python3
"""0.24.2: author the boss's voicemails: Kokoro speech (kokoro_voice.py) on an answering-machine tape.

Reads every voicemail line from Game/Assets/_Project/Data/Company/CompanyMessages.asset, speaks it, and runs it
through an answering-machine tape (narrow band, wow and flutter, hiss). Each file is named by the first 8 hex
digits of the MD5 of the line's text (UTF-8), which is how AnsweringMachine finds it at run time.

The lines take turns through VOICES, so the messages are left by different people (user, 2026-10-12: "keep it
random"); one message always has the same voice, so playing it again sounds the same.

    python3 Tools/Audio/generate_boss_voicemails.py                    # every line, into the game
    python3 Tools/Audio/generate_boss_voicemails.py --voice bm_george --out /tmp/try --limit 3   # a trial
"""

from pathlib import Path
import hashlib
import math
import random
import argparse
import struct
import tempfile
import wave

import kokoro_voice

REPO = Path(__file__).resolve().parents[2]
SOURCE = REPO / "Game/Assets/_Project/Data/Company/CompanyMessages.asset"
DESTINATION = REPO / "Game/Assets/_Project/Art/Audio/Voicemail/Resources/Voicemail"
VOICES, SPEED, RATE = ["am_michael", "am_fenrir", "am_onyx", "bm_george"], 0.95, 22050
KINDS = ["Welcome", "QuotaMet", "BigHaul", "QuotaMissed", "LastWarning", "Bankrupt", "LevelUp"]


def lines():
    """The list items under each kind, YAML plain or single-quoted, folded to one line like Unity reads them."""
    out, kind, item = [], None, None
    for raw in SOURCE.read_text(encoding="utf-8").splitlines():
        stripped = raw.strip()
        if stripped.startswith("<") and "k__BackingField:" in stripped:
            if item is not None: out.append(item)
            item = None
            name = stripped[1:stripped.index(">")]
            kind = name if name in KINDS else None
            continue
        if kind is None: continue
        if raw.startswith("  - "):
            if item is not None: out.append(item)
            item = raw[4:].strip()
        elif item is not None and raw.startswith("    ") and stripped:
            item += " " + stripped
    if item is not None: out.append(item)
    cleaned = []
    for text in out:
        if text.startswith("'") and text.endswith("'"): text = text[1:-1].replace("''", "'")
        cleaned.append(text)
    return cleaned


def tape(raw_file, seed):
    with wave.open(str(raw_file), "rb") as source:
        rate = source.getframerate()
        samples = struct.unpack("<%dh" % source.getnframes(), source.readframes(source.getnframes()))
    rng = random.Random(seed)
    hp = math.exp(-2 * math.pi * 320 / rate)
    lp = 1 - math.exp(-2 * math.pi * 3000 / rate)
    prev_in = prev_hp = low1 = low2 = 0.0
    out = []
    for i, pcm in enumerate(samples):
        v = pcm / 32768.0
        h = hp * (prev_hp + v - prev_in)
        prev_in, prev_hp = v, h
        low1 += lp * (h - low1)
        low2 += lp * (low1 - low2)
        t = i / rate
        wow = 1.0 + 0.05 * math.sin(2 * math.pi * 0.7 * t) + 0.02 * math.sin(2 * math.pi * 7.3 * t)
        out.append(math.tanh(low2 * 1.8 * wow) + rng.uniform(-1, 1) * 0.012)
    peak = max((abs(v) for v in out), default=1.0) or 1.0
    lead = [rng.uniform(-1, 1) * 0.012 for _ in range(int(rate * 0.6))]
    tail = [rng.uniform(-1, 1) * 0.012 for _ in range(int(rate * 0.4))]
    frames = lead + [v * 0.7 / peak for v in out] + tail
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
    readme = ["# The boss's voicemails (0.24.2; voice replaced in 0.24.7c)", "",
              "Spoken on this machine by the Kokoro-82M speech model (Apache-2.0 weights) through",
              "Tools/Audio/generate_boss_voicemails.py and run through an answering-machine tape. No paid service and",
              "no recording of a real person. The first clips used the macOS voice 'Ralph', which Apple does not",
              "license for commercial use; never go back to `say`.", "",
              "The messages are left by different people: the lines take turns through " + ", ".join(VOICES) + ".", ""]
    texts = lines()[:options.limit] if options.limit > 0 else lines()
    names = ["vm_" + hashlib.md5(text.encode("utf-8")).hexdigest()[:8] for text in texts]
    # In turn, so the lines of one kind (which the game chooses between) never share a voice with their neighbour.
    voices = [options.voice or VOICES[i % len(VOICES)] for i in range(len(texts))]
    with tempfile.TemporaryDirectory(prefix="abandoned_voicemail_") as directory:
        raws = [Path(directory) / (name + ".wav") for name in names]
        for voice in sorted(set(voices)):
            kokoro_voice.speak([(raws[i], texts[i]) for i in range(len(texts)) if voices[i] == voice], voice, SPEED, RATE)
        for i, (name, text) in enumerate(zip(names, texts)):
            rate, frames = tape(raws[i], i)
            with wave.open(str(options.out / (name + ".wav")), "wb") as w:
                w.setnchannels(1); w.setsampwidth(2); w.setframerate(rate); w.writeframes(frames)
            readme.extend(["## %s (%s)" % (name, voices[i]), "", text, ""])
            print("Authored", name, voices[i], text[:50])
    if options.out == DESTINATION and options.limit <= 0:
        (DESTINATION.parent.parent / "README.md").write_text("\n".join(readme), encoding="utf-8")


if __name__ == "__main__":
    main()
