#!/usr/bin/env python3
"""0.24.2: author the boss's voicemails with an installed macOS system voice; no downloaded content.

Reads every voicemail line from Game/Assets/_Project/Data/Company/CompanyMessages.asset, speaks it with `say`,
and runs it through an answering-machine tape (narrow band, wow and flutter, hiss). Each file is named by the
first 8 hex digits of the MD5 of the line's text (UTF-8), which is how AnsweringMachine finds it at run time.
"""

from pathlib import Path
import hashlib
import math
import random
import struct
import subprocess
import tempfile
import wave

REPO = Path(__file__).resolve().parents[2]
SOURCE = REPO / "Game/Assets/_Project/Data/Company/CompanyMessages.asset"
DESTINATION = REPO / "Game/Assets/_Project/Art/Audio/Voicemail/Resources/Voicemail"
VOICE, RATE_WPM, RATE = "Ralph", "168", 22050
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
    DESTINATION.mkdir(parents=True, exist_ok=True)
    readme = ["# The boss's voicemails (0.24.2)", "",
              "Spoken locally with the installed macOS voice '%s' by Tools/Audio/generate_boss_voicemails.py and run" % VOICE,
              "through an answering-machine tape. No download, paid service or recording of a real person.", ""]
    with tempfile.TemporaryDirectory(prefix="abandoned_voicemail_") as directory:
        for i, text in enumerate(lines()):
            name = "vm_" + hashlib.md5(text.encode("utf-8")).hexdigest()[:8]
            raw = Path(directory) / (name + ".wav")
            subprocess.run(["/usr/bin/say", "-v", VOICE, "-r", RATE_WPM, "--file-format=WAVE", "--data-format=LEI16@%d" % RATE,
                            "--channels=1", "-o", str(raw), text], check=True)
            rate, frames = tape(raw, i)
            with wave.open(str(DESTINATION / (name + ".wav")), "wb") as w:
                w.setnchannels(1); w.setsampwidth(2); w.setframerate(rate); w.writeframes(frames)
            readme.extend(["## " + name, "", text, ""])
            print("Authored", name, text[:50])
    (DESTINATION.parent.parent / "README.md").write_text("\n".join(readme), encoding="utf-8")


if __name__ == "__main__":
    main()
