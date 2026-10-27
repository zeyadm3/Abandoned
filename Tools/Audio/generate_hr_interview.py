#!/usr/bin/env python3
"""Author the hiring's HR manager (2026 remake): every line she speaks, Kokoro speech (kokoro_voice.py), dry and close,
as in a small office.

Reads each Say("...") in Game/Assets/_Project/Editor/Onboarding/OnboardingDialogue.cs (the player's replies are
You("...") and are not spoken). Each file is named "hr_" plus the first 8 hex digits of the MD5 of the line (UTF-8),
which is how Abandoned.Onboarding.DialogueVoice finds it. A line without a clip still shows as a subtitle.

    python3 Tools/Audio/generate_hr_interview.py                                  # every line, into the game
    python3 Tools/Audio/generate_hr_interview.py --voice af_bella --out /tmp/try --limit 3   # a trial
"""

from pathlib import Path
import argparse
import hashlib
import re
import struct
import tempfile
import wave

import kokoro_voice

REPO = Path(__file__).resolve().parents[2]
SOURCE = REPO / "Game/Assets/_Project/Editor/Onboarding/OnboardingDialogue.cs"
DESTINATION = REPO / "Game/Assets/_Project/Art/Audio/Onboarding/Resources/Onboarding"
VOICE, SPEED, RATE = "af_sarah", 0.96, 24000


def lines():
    source = SOURCE.read_text(encoding="utf-8")
    seen, unique = set(), []
    for m in re.finditer(r'Say\("((?:[^"\\]|\\.)*)"\)', source):
        text = m.group(1).replace('\\"', '"')
        if text not in seen:
            seen.add(text)
            unique.append(text)
    return unique


def tidy(raw_file):
    """Trim the silence Kokoro leaves, a short room tail of quiet, a little headroom."""
    with wave.open(str(raw_file), "rb") as source:
        rate = source.getframerate()
        samples = list(struct.unpack("<%dh" % source.getnframes(), source.readframes(source.getnframes())))
    loud = [i for i, s in enumerate(samples) if abs(s) > 300]
    if loud:
        samples = samples[max(0, loud[0] - int(rate * .03)):loud[-1] + int(rate * .12)]
    peak = max((abs(s) for s in samples), default=1) or 1
    gain = min(1.0, 29000 / peak)
    frames = [0] * int(rate * .04) + [int(s * gain) for s in samples] + [0] * int(rate * .08)
    return rate, struct.pack("<%dh" % len(frames), *frames)


def main():
    parser = argparse.ArgumentParser(description=__doc__.splitlines()[0])
    parser.add_argument("--voice", default=VOICE, help="the Kokoro voice (default %s)" % VOICE)
    parser.add_argument("--out", type=Path, default=DESTINATION, help="folder to write to (default: the game's)")
    parser.add_argument("--limit", type=int, default=0, help="only the first N lines (a trial)")
    options = parser.parse_args()
    if options.out == DESTINATION and (options.voice != VOICE or options.limit > 0):
        parser.error("--voice and --limit are for trials: give --out a folder outside the game")
    options.out.mkdir(parents=True, exist_ok=True)
    texts = lines()[:options.limit] if options.limit > 0 else lines()
    if not texts:
        raise SystemExit("No Say(...) lines found in OnboardingDialogue.cs.")
    names = ["hr_" + hashlib.md5(text.encode("utf-8")).hexdigest()[:8] for text in texts]
    readme = ["# The HR manager (the hiring)", "",
              "Every line spoken by the Kokoro-82M speech model (Apache-2.0 weights), voice %s, through" % options.voice,
              "Tools/Audio/generate_hr_interview.py. No paid service and no recording of a real person; never `say`.",
              "English only: the subtitles carry the text in every language.", ""]
    with tempfile.TemporaryDirectory(prefix="abandoned_hr_") as directory:
        raws = [Path(directory) / (name + ".wav") for name in names]
        kokoro_voice.speak(list(zip(raws, texts)), options.voice, SPEED, RATE)
        for raw, name, text in zip(raws, names, texts):
            rate, frames = tidy(raw)
            with wave.open(str(options.out / (name + ".wav")), "wb") as w:
                w.setnchannels(1); w.setsampwidth(2); w.setframerate(rate); w.writeframes(frames)
            readme.extend(["## %s" % name, "", text, ""])
            print("Authored", name, text[:70])
    if options.out == DESTINATION and options.limit <= 0:
        (DESTINATION.parent.parent / "README.md").write_text("\n".join(readme), encoding="utf-8")


if __name__ == "__main__":
    main()
