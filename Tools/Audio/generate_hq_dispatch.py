#!/usr/bin/env python3
"""Author original HQ dispatch WAVs using the installed macOS system voice; no downloaded content."""

from pathlib import Path
import math
import struct
import subprocess
import tempfile
import wave


REPO = Path(__file__).resolve().parents[2]
DESTINATION = REPO / "Game/Assets/_Project/Art/Audio/HQDispatch"
RATE = 24000
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
    DESTINATION.mkdir(parents=True, exist_ok=True)
    with tempfile.TemporaryDirectory(prefix="abandoned_dispatch_author_") as directory:
        for name, transcript in LINES.items():
            raw_file = Path(directory) / (name + ".wav")
            # No voice download or setup: say uses the machine's already-installed default speech voice.
            subprocess.run([
                "/usr/bin/say", "-r", "158", "--file-format=WAVE", "--data-format=LEI16@24000",
                "--channels=1", "-o", str(raw_file), transcript,
            ], check=True)
            rate, frames = telephone_frames(raw_file)
            with wave.open(str(DESTINATION / (name + ".wav")), "wb") as authored:
                authored.setnchannels(1)
                authored.setsampwidth(2)
                authored.setframerate(rate)
                authored.writeframes(frames)
            print("Authored " + name + ".wav")
    documentation = [
        "# Original HQ dispatch recordings (0.14.5)", "",
        "Original Abandoned Salvaging dialogue authored for the dispatch telephone. Speech was rendered locally",
        "with the installed macOS system voice and processed into a copper-line telephone sound using the",
        "standard-library authoring script at Tools/Audio/generate_hq_dispatch.py. No audio pack, sample,",
        "voice download, paid service, or recording of a real person was used.", "",
        "Runtime terms and crew-scaled quota remain in the selected job's visible transcript; these fixed clips",
        "speak safety warnings only. The telephone shows their exact spoken words even with sound muted.", "",
    ]
    for name, transcript in LINES.items():
        documentation.extend(["## " + name, "", transcript, ""])
    (DESTINATION / "README.md").write_text("\n".join(documentation), encoding="utf-8")


if __name__ == "__main__":
    main()
