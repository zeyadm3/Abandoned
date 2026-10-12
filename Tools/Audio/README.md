# Voice lines

The spoken lines in the game (4 dispatch briefings, 21 boss voicemails) are made here and saved as WAVs
in the Unity project. Nothing in this folder ships with the game.

| Script | Makes | Voice |
|---|---|---|
| `generate_hq_dispatch.py` | `Game/Assets/_Project/Art/Audio/HQDispatch/dispatch_*.wav` (telephone line) | `af_heart` |
| `generate_boss_voicemails.py` | `Game/Assets/_Project/Art/Audio/Voicemail/Resources/Voicemail/vm_*.wav` (answering-machine tape) | `am_michael` |

Both speak through `kokoro_voice.py`: the Kokoro-82M speech model (Apache-2.0 weights,
https://huggingface.co/hexgrad/Kokoro-82M) run on this machine by the `kokoro-onnx` package (MIT).

**Never use `say`.** The first clips were spoken by macOS system voices, which Apple licenses for personal,
non-commercial use only (macOS licence, "Voices"). They were replaced on 2026-10-12.

## Running

```bash
python3 Tools/Audio/generate_hq_dispatch.py
python3 Tools/Audio/generate_boss_voicemails.py
```

File names do not change (a voicemail is named by a hash of its text), so Unity keeps its references and
simply re-imports the sound. After editing a voicemail line in CompanyMessages, run the second script again
and delete any `vm_*.wav` whose line no longer exists.

## Trying another voice

```bash
python3 Tools/Audio/generate_boss_voicemails.py --voice bm_george --limit 3 --out Tools/Audio/.tts/audition/boss-bm_george
python3 Tools/Audio/generate_hq_dispatch.py --voice bf_emma --out Tools/Audio/.tts/audition/dispatch-bf_emma
```

With `--out`, nothing in the game is touched. Names start with the accent and gender: `am_` American male
(`am_michael`, `am_fenrir`, `am_onyx`, `am_adam`, `am_eric`...), `af_` American female (`af_heart`,
`af_bella`, `af_sarah`...), `bm_` British male (`bm_george`, `bm_lewis`...), `bf_` British female (`bf_emma`...).
An unknown name prints the whole list. To change the game's voice, edit `VOICE` at the top of the script and
run it without `--out`.

## Setting the tools up again (about 510 MB in `Tools/Audio/.tts/`, git-ignored, plus Python)

Done once on 2026-10-12. On another machine:

```bash
brew install python@3.12
/opt/homebrew/opt/python@3.12/bin/python3.12 -m venv Tools/Audio/.tts/venv
Tools/Audio/.tts/venv/bin/python -m pip install "kokoro-onnx==0.6.1"
mkdir -p Tools/Audio/.tts/models
curl -L -o Tools/Audio/.tts/models/kokoro-v1.0.onnx https://github.com/thewh1teagle/kokoro-onnx/releases/download/model-files-v1.0/kokoro-v1.0.onnx
curl -L -o Tools/Audio/.tts/models/voices-v1.0.bin https://github.com/thewh1teagle/kokoro-onnx/releases/download/model-files-v1.0/voices-v1.0.bin
shasum -a 256 Tools/Audio/.tts/models/*
```

The files used for the game's clips:

| File | Bytes | SHA-256 |
|---|---|---|
| `kokoro-v1.0.onnx` | 325,532,387 | `7d5df8ecf7d4b1878015a32686053fd0eebe2bc377234608764cc0ef3636a6c5` |
| `voices-v1.0.bin` | 28,214,398 | `bca610b8308e8d99f32e6fe4197e7ec01679264efed0cac9140fe9c29f1fbf7d` |

Packages in the environment then: kokoro-onnx 0.6.1, onnxruntime 1.31.0, numpy 2.5.4, phonemizer 3.4.0,
espeakng-loader 0.2.4 (espeak-ng only turns text into phonemes; none of it is in the game).
