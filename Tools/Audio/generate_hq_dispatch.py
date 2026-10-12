#!/usr/bin/env python3
"""Author the HQ dispatch WAVs: Kokoro speech (kokoro_voice.py) put through a copper telephone line.

    python3 Tools/Audio/generate_hq_dispatch.py                      # the game's four clips
    python3 Tools/Audio/generate_hq_dispatch.py --voice bf_emma --out /tmp/try   # hear another voice first
"""

from pathlib import Path
import argparse
import math
import struct
import tempfile
import wave

import kokoro_voice


REPO = Path(__file__).resolve().parents[2]
DESTINATION = REPO / "Game/Assets/_Project/Art/Audio/HQDispatch"
RATE = 24000
VOICE, SPEED = "af_heart", 0.92
LINES = {
    "dispatch_routine": (
        "Abandoned Salvaging dispatch. Review the job sheet before departure. "
        "Keep the crew together, listen for floor warnings, and load salvage into the truck. Bring everyone home."
    ),
    "dispatch_power_off": (
        "Abandoned Salvaging dispatch. This job has no power. "
        "Bring charged flashlights and keep the crew together. Review the job sheet, and listen for floor warnings."
    ),
    "dispatch_high_threat": (
        "Abandoned Salvaging dispatch. This is a high threat job. "
        "Expect more monsters, sooner. Keep noise down, read the field guide, and bring the crew home together."
    ),
    "dispatch_sealed": (
        "Abandoned Salvaging dispatch. Access is shuttered on this job. "
        "Pack bolt cutters or a crowbar before departure. Review the job sheet, and leave room for salvage in the truck."
    ),
}


def telephone_frames(raw_file):
    with wave.open(str(raw_file), "rb") as source:
        rate = source.getframerate()
        samples = struct.unpack("<%dh" % source.getnframes(), source.readframes(source.getnframes()))
    # A narrow copper telephone line makes the locally authored voice fit the workbench's rotary handset.
    high_pass = math.exp(-2 * math.pi * 270 / rate)
    low_pass = 1 - math.exp(-2 * math.pi * 3450 / rate)
    previous_input = previous_high = first_low = second_low = 0.0
    filtered = []
    for pcm in samples:
        value = pcm / 32768.0
        high = high_pass * (previous_high + value - previous_input)
        previous_input, previous_high = value, high
        first_low += low_pass * (high - first_low)
        second_low += low_pass * (first_low - second_low)
        filtered.append(math.tanh(second_low * 1.5))
    peak = max((abs(value) for value in filtered), default=1.0) or 1.0
    gain = 0.64 / peak
    # Give the existing line-connected beep a short lead before the operator speaks.
    output = [0] * int(rate * 0.32)
    output.extend(int(max(-1, min(1, value * gain)) * 32767) for value in filtered)
    output.extend([0] * int(rate * 0.12))
    return rate, struct.pack("<%dh" % len(output), *output)


def main():
    parser = argparse.ArgumentParser(description=__doc__.splitlines()[0])
    parser.add_argument("--voice", default=VOICE, help="Kokoro voice name (default %(default)s)")
    parser.add_argument("--out", type=Path, default=DESTINATION, help="folder to write to (default: the game's)")
    options = parser.parse_args()
    options.out.mkdir(parents=True, exist_ok=True)
    with tempfile.TemporaryDirectory(prefix="abandoned_dispatch_author_") as directory:
        raw_files = {name: Path(directory) / (name + ".wav") for name in LINES}
        kokoro_voice.speak([(raw_files[name], transcript) for name, transcript in LINES.items()],
                           options.voice, SPEED, RATE)
        for name in LINES:
            rate, frames = telephone_frames(raw_files[name])
            with wave.open(str(options.out / (name + ".wav")), "wb") as authored:
                authored.setnchannels(1)
                authored.setsampwidth(2)
                authored.setframerate(rate)
                authored.writeframes(frames)
            print("Authored " + name + ".wav")
    if options.out != DESTINATION:
        return  # a trial of another voice: the game's notes stay as they are
    documentation = [
        "# HQ dispatch recordings (0.14.5; voice replaced in 0.24.7c)", "",
        "Original Abandoned Salvaging dialogue for the dispatch telephone, spoken by the Kokoro-82M speech model",
        "(Apache-2.0 weights, voice '%s') on this machine and processed into a copper-line telephone sound by" % options.voice,
        "Tools/Audio/generate_hq_dispatch.py. No paid service and no recording of a real person. The first clips",
        "used a macOS system voice, which Apple does not license for commercial use; never go back to `say`.", "",
        "Runtime terms and crew-scaled quota remain in the selected job's visible transcript; these fixed clips",
        "speak safety warnings only. The telephone shows their exact spoken words even with sound muted.", "",
    ]
    for name, transcript in LINES.items():
        documentation.extend(["## " + name, "", transcript, ""])
    (DESTINATION / "README.md").write_text("\n".join(documentation), encoding="utf-8")


if __name__ == "__main__":
    main()
