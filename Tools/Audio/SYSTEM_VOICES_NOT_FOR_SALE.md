# The voice clips can't be sold as they are

While this file exists, `Tools/unity.sh build-release` refuses to build (BuildScript.SystemVoicesMarker).
Friends' builds (`build`, `build-demo`) are not blocked, but see "Demo" below.

## Why

The 25 spoken clips were made with macOS system voices through `say`:

- 4 dispatch briefings: `Game/Assets/_Project/Art/Audio/HQDispatch/dispatch_*.wav`
  (Tools/Audio/generate_hq_dispatch.py, the default voice)
- 21 boss voicemails: `Game/Assets/_Project/Art/Audio/Voicemail/Resources/Voicemail/vm_*.wav`
  (Tools/Audio/generate_boss_voicemails.py, the voice "Ralph")

Apple's macOS licence (section 2F, "Voices; Live Captions", read on this Mac's macOS 27.0.1 on 2026-10-12,
`/System/Library/CoreServices/Setup Assistant.app/Contents/Resources/en.lproj/OSXSoftwareLicense.rtf`) allows
system voices "to create your own original content and projects for your personal, non-commercial use" and
permits no other use, naming recording, publishing and redistribution "in a profit, non-profit, public
sharing or commercial context". The tape and telephone processing does not change whose voice it is.

This is a reading of the licence, not legal advice.

## Ways out

1. **Record them.** You or a friend read the 25 lines (the text is in the two README.md files beside the
   clips). Save each as a mono WAV with the same file name, then run the clips through the same tape and
   telephone processing (ask Claude to add a "process this recording" mode to the two scripts).
2. **Another synthetic voice whose licence allows selling the output.** This needs a download, so it needs
   your OK first. Claude then swaps `say` in the two scripts and regenerates all 25 files.
3. **Ship without speech.** Both screens already show the words: the telephone prints each briefing and the
   answering machine shows a line whose clip is missing. Delete the 21 voicemail WAVs (and their .meta files);
   for the dispatch clips, remove their four rows from the sound bank first (ask Claude for the menu item),
   then delete the WAVs.

After any of these, delete this file, update the voice paragraph in Docs/ASSET_CREDITS.md and the README.md
files beside the clips, and `build-release` will run.

## Demo

A demo on Steam is public sharing too. Do one of the above before uploading a demo build, even though
`build-demo` does not check for this file (friends' test builds use it).
