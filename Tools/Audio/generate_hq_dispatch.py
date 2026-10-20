#!/usr/bin/env python3
"""Author the HQ dispatch WAVs: Kokoro speech (kokoro_voice.py) put through a copper telephone line.

Every briefing is recorded once per operator in VOICES; the game picks one of them each time the phone is
used, so the same job can be read out by a different person (user, 2026-10-12: "keep it random").

    python3 Tools/Audio/generate_hq_dispatch.py                      # the game's clips, every operator
    python3 Tools/Audio/generate_hq_dispatch.py --voice bm_lewis --out /tmp/try   # hear another voice first
"""

from pathlib import Path
import argparse
import hashlib
import math
import struct
import tempfile
import wave

import kokoro_voice


REPO = Path(__file__).resolve().parents[2]
DESTINATION = REPO / "Game/Assets/_Project/Art/Audio/HQDispatch"
RATE = 24000
# The operators. The first keeps the original file names (dispatch_routine.wav); the others add _2, _3...
# Adding one here also needs its clips in the sound bank: Tools > Abandoned > Maps > HQ > Author Dispatch Audio.
VOICES, SPEED = ["af_heart", "af_bella", "bf_emma"], 0.92
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


def ensure_meta(clip, base):
    """A new operator's clip gets the first operator's import settings and an id of its own, fixed by its name,
    so the sound bank's reference to it survives on any machine. Unity never has to invent one."""
    meta = clip.with_name(clip.name + ".meta")
    base_meta = base.with_name(base.name + ".meta")
    if meta.exists() or not base_meta.exists():
        return
    lines = base_meta.read_text(encoding="utf-8").splitlines()
    guid = hashlib.md5(("abandoned/" + clip.name).encode("utf-8")).hexdigest()
    meta.write_text("\n".join("guid: " + guid if line.startswith("guid: ") else line for line in lines) + "\n",
                    encoding="utf-8")


def main():
    parser = argparse.ArgumentParser(description=__doc__.splitlines()[0])
    parser.add_argument("--voice", help="try one Kokoro voice (use with --out)")
    parser.add_argument("--out", type=Path, default=DESTINATION, help="folder to write to (default: the game's)")
    options = parser.parse_args()
    trial = options.out != DESTINATION or options.voice is not None
    if trial and options.out == DESTINATION:
        parser.error("--voice is for trying a voice: give --out a folder outside the game")
    voices = [options.voice] if options.voice else VOICES
    options.out.mkdir(parents=True, exist_ok=True)
    with tempfile.TemporaryDirectory(prefix="abandoned_dispatch_author_") as directory:
        for index, voice in enumerate(voices):
            suffix = "" if index == 0 else "_%d" % (index + 1)
            raw_files = {name: Path(directory) / (name + suffix + ".wav") for name in LINES}
            kokoro_voice.speak([(raw_files[name], transcript) for name, transcript in LINES.items()],
                               voice, SPEED, RATE)
            for name in LINES:
                rate, frames = telephone_frames(raw_files[name])
                clip = options.out / (name + suffix + ".wav")
                with wave.open(str(clip), "wb") as authored:
                    authored.setnchannels(1)
                    authored.setsampwidth(2)
                    authored.setframerate(rate)
                    authored.writeframes(frames)
                if not trial:
                    ensure_meta(clip, DESTINATION / (name + ".wav"))
                print("Authored " + clip.name + " (" + voice + ")")
    if trial:
        return  # a trial of another voice: the game's notes stay as they are
    documentation = [
        "# HQ dispatch recordings (0.14.5; voice replaced in 0.24.7c)", "",
        "Original Abandoned Salvaging dialogue for the dispatch telephone, spoken by the Kokoro-82M speech model",
        "(Apache-2.0 weights) on this machine and processed into a copper-line telephone sound by",
        "Tools/Audio/generate_hq_dispatch.py. No paid service and no recording of a real person. The first clips",
        "used a macOS system voice, which Apple does not license for commercial use; never go back to `say`.", "",
        "Each briefing exists once per operator and the game plays one at random: " +
        ", ".join("%s%s" % (voice, " (name.wav)" if i == 0 else " (name_%d.wav)" % (i + 1)) for i, voice in enumerate(VOICES)) + ".", "",
        "Runtime terms and crew-scaled quota remain in the selected job's visible transcript; these fixed clips",
        "speak safety warnings only. The telephone shows their exact spoken words even with sound muted.", "",
    ]
    for name, transcript in LINES.items():
        documentation.extend(["## " + name, "", transcript, ""])
    (DESTINATION / "README.md").write_text("\n".join(documentation), encoding="utf-8")


if __name__ == "__main__":
    main()
