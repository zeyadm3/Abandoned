#!/usr/bin/env python3
"""Speech for the game's voice lines from Kokoro-82M (Apache-2.0 weights), run locally through kokoro-onnx.

Two roles in one file:
- imported by the generate_*.py scripts (plain python3, standard library only): speak() hands the lines to
  the private environment in Tools/Audio/.tts and gets one mono 16-bit WAV back per line;
- run inside that environment, it does the speaking.

The environment and the model files are never part of the game or the repository; Tools/Audio/README.md
says how to set them up again. macOS system voices (`say`) must not be used: Apple licenses them for
personal, non-commercial use only.
"""

from pathlib import Path
import json
import subprocess
import sys
import tempfile
import wave

HERE = Path(__file__).resolve().parent
TOOLS = HERE / ".tts"
PYTHON = TOOLS / "venv/bin/python"
MODEL = TOOLS / "models/kokoro-v1.0.onnx"
VOICES = TOOLS / "models/voices-v1.0.bin"


def speak(jobs, voice, speed, rate):
    """jobs: [(wav path, text)]. Writes each WAV at `rate` Hz; raises with setup advice if the tools are missing."""
    missing = [str(p) for p in (PYTHON, MODEL, VOICES) if not p.exists()]
    if missing:
        raise SystemExit("Voice tools are not set up (missing %s). See Tools/Audio/README.md." % ", ".join(missing))
    with tempfile.TemporaryDirectory(prefix="abandoned_voice_jobs_") as directory:
        listing = Path(directory) / "jobs.json"
        listing.write_text(json.dumps([{"out": str(out), "text": text} for out, text in jobs]), encoding="utf-8")
        # -I: the environment's own packages only, nothing picked up from the working folder or the user's site.
        subprocess.run([str(PYTHON), "-I", str(Path(__file__).resolve()), str(MODEL), str(VOICES), voice,
                        str(speed), str(rate), str(listing)], check=True)


def _synthesise(model, voices, voice, speed, rate, listing):
    import numpy as np
    from kokoro_onnx import Kokoro

    kokoro = Kokoro(model, voices)
    known = kokoro.get_voices()
    if voice not in known:
        raise SystemExit("Unknown voice '%s'. Available: %s" % (voice, ", ".join(sorted(known))))
    # Voice names start with the accent: a = American, b = British.
    language = "en-gb" if voice.startswith("b") else "en-us"
    for job in json.loads(Path(listing).read_text(encoding="utf-8")):
        samples, native = kokoro.create(job["text"], voice=voice, speed=speed, lang=language)
        samples = np.asarray(samples, dtype=np.float64).reshape(-1)
        if native != rate:
            count = max(1, int(round(len(samples) * rate / native)))
            samples = np.interp(np.linspace(0, len(samples) - 1, count), np.arange(len(samples)), samples)
        pcm = (np.clip(samples, -1.0, 1.0) * 32767).astype("<i2")
        with wave.open(job["out"], "wb") as out:
            out.setnchannels(1)
            out.setsampwidth(2)
            out.setframerate(rate)
            out.writeframes(pcm.tobytes())


if __name__ == "__main__":
    if len(sys.argv) != 7:
        raise SystemExit("Run the generate_*.py scripts instead; they call this with the right arguments.")
    _synthesise(sys.argv[1], sys.argv[2], sys.argv[3], float(sys.argv[4]), int(sys.argv[5]), sys.argv[6])
