# ABANDONED app logo — 0.24.6b

The desktop logo is a fractured architectural A: concrete legs, exposed reinforcement,
a broken floor crossbar and hazard-yellow fracture edges. Its two appearances keep the
same silhouette. Light uses a pale concrete tile and grey structure; dark uses a charcoal
tile and lighter structure. Both preserve transparency outside the tile.

## Assets

- Unity PNGs: `Game/Assets/_Project/Art/Branding/AbandonedIcon-Light.png` and
  `AbandonedIcon-Dark.png` (1024 × 1024, RGBA).
- Original generated masters: `Tools/Branding/0.24.6b/AbandonedIcon-*-Master.png`
  (1280 × 1280, RGBA).
- Windows export: `Tools/Branding/0.24.6b/Abandoned-Light.ico`, containing
  16, 24, 32, 48, 64, 128 and 256 px light-appearance images.
- Small previews: `Tools/Branding/0.24.6b/AbandonedIcon-*-64.png`.
- Exact generation and cleanup prompts: `Tools/Branding/0.24.6b/GENERATION_PROMPTS.json`.
  Artwork was generated with the built-in `image_gen` tool; it uses no downloaded logo
  or additional Unity package.

## Platform behavior

Windows uses only the light PNG through Unity's standalone Player Settings icon slots.
macOS uses a native Icon Composer asset compiled by Xcode into `Assets.car`, with light
and dark artwork assigned to their corresponding appearances. The build adds the icon
catalog and bundle icon keys, then re-signs the app before the existing signature check.
Older macOS versions use the light `.icns` fallback. Native appearance selection depends
on the macOS version and the user's icon appearance setting.

Apple's icon workflow: https://developer.apple.com/documentation/xcode/creating-your-app-icon-using-icon-composer

## Rebuild

The build version is `0.24.6b`. With Unity closed:

```sh
Tools/unity.sh exec Abandoned.EditorTools.Release0246b.AuthorAndExit
Tools/unity.sh build
```

The authoring step imports the artwork and applies icon and version settings. The build
produces universal Mac and Windows players and archives them in
`~/Documents/Abandoned-builds/dev/`. The Mac build requires Xcode with `actool` and
`codesign`, and fails if native icon compilation or signing fails.

No game scenes or gameplay systems are changed by this release. Gameplay review remains
with the user, following the existing project workflow.
