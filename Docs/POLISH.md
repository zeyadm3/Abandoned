# ABANDONED — feel and presentation pass

Direction agreed 2026-10-07. The user asked to build the whole pass first and will playtest afterward.
Compile, content regeneration and Mac/Windows builds are allowed; do not run EditMode, PlayMode,
nettests, gameplay sessions or QA screenshots for this pass. A successful build is not a playtest.

## Direction

The world plays horror straight; the crew, physical mishaps and the company's dry humour provide comedy.
The defining experience is greed versus gravity: a valuable haul can destroy its own escape route.
Keep the mall as the Early Access location, existing company progression, and simplified equipment.

| Area | Agreed direction |
|---|---|
| References | Lethal Company's atmosphere, dread and uncaring company; R.E.P.O.'s object handling and team carrying comedy. |
| World | A believable abandoned mall, with liminal emptiness and readable structural decay. |
| Art | Clean low-poly shapes; decay through materials, stains, dust, lighting and sparse dressing. No retro filter. |
| Crew | Salvage coveralls, hard hats, team colours; restrained movement. Ragdolls and carrying supply comedy. |
| Monsters | Four distinct silhouettes recognizable at distance, including in the mall's fog. |
| Lighting | Flashlights matter indoors; warm daylight through the roof and exterior offers relief. One-hand loot leaves a hand for a light. |
| Movement | Immediate empty-handed controls; weight-slowed walking for TwoHand loot, no sprint during TwoHand/shared/drag carries. |
| Handling | Moderate shared sway and tugging, real doorframe snags; add held-object rotation and deliberate gentle placement. |
| Camera | Subtle bob and landing dip; stronger shake reserved for nearby collapses. Keep comfort switches. |
| Structure | Cracks and sound teach risk. A small HUD warning only during active Failing; optional warning subtitles. |
| Tools | Keep simplified trolley/pulley interactions for Early Access. |
| UI | Keep hazard yellow, amber terminals, paper receipts. Stamina hides at full; exploration monetary values appear on scan. Contextual HQ/truck/appraisal screens retain their finances. |
| Audio | Quiet runs with building sounds; music at HQ/menu, during travel and truck departure. Creak → groan → snap is the signature. |
| Hardware | Target 60 FPS at 1080p on a mid-range PC; actual performance remains to be measured. Keyboard/mouse for Early Access. |
| Assets | Existing free assets and original code-built art. No new paid assets, packages or budget inferred. |

## Implementation rules

- Rotation changes the target for an individually held object's physical spring; it does not teleport
  through geometry or change who owns damage/value. Shared carrying retains host physics and crew steering.
- Tap Drop releases normally; hold Drop lowers the item toward a supported, clear placement.
  Gentle placement protects valuables by reducing physical energy, never by granting damage immunity.
  Shared and dragged items keep an immediate release for escape and coordination.
- Sprint restrictions apply to movement state, stamina, camera bob and monster-heard footsteps together.
- Hide the flashlight beam when carrying occupies both hands; restore its previous switch state on release.
- Cosmetic animation and dressing must not add gameplay collision or obscure structural cracks.
- Preserve host authority, existing network protocols where possible, save identity and cosmetics.

## New handling controls

- Hold middle mouse or left Alt and move the mouse to rotate an individually held item. The rotation
  action can be rebound in Settings → Controls. Shared carries still turn through crew steering.
- Tap right mouse to drop. Hold right mouse to lower the item; it releases after finding a clear,
  supported spot and settling. Release early to cancel placement and reposition. Shared/dragged
  items release immediately. Existing throw controls remain hold/release left mouse.
- TwoHand, shared and dragged carries cannot sprint; occupied hands hide the flashlight beam while
  retaining its switch position. OneHand carries still leave a hand for light.

## User playtest after delivery

1. Start at HQ, choose/equip gear, select a job and travel. Check text, transitions, save/progression and controls.
2. Walk/sprint/crouch/jump empty-handed and with OneHand/TwoHand loot. Compare speed, stamina and camera comfort.
3. Rotate loot through a doorway and on stairs. Tap-drop, charge-throw, then gently place fragile loot
   on a floor/shelf/truck bay. Check blocked placement and unsupported edges; real hard impacts must still damage it.
4. Explore the mall in power-on and power-off jobs. Judge flashlight necessity, daylight relief,
   steps/holes/cracks readability, navigation and monster silhouettes at approximately 20 metres.
5. Scan and let the reveal expire. Confirm money/value timing, pocket UI, stamina hiding, urgent warnings and departure cues.
6. Overload a floor through its warning stages. Judge the two-second Failing window, subtitles,
   directional creaks/groans/snaps and collapse shake with comfort settings on/off.
7. Repeat solo/client pickup, rotation, placement and two-/three-player carrying. Check snags,
   opposite pulls, crouching, release, lost grip, ownership handoff and any visible jitter or delay.
8. Load/extract, leave a crew member behind, die/spectate/revive, appraise and return to HQ.
   Check quiet exploration, travel/departure music, voice intelligibility and rewards.
9. Note FPS at 1080p in the atrium with multiple flashlights and during a collapse. Record hardware,
   graphics settings, host/client role and approximate network latency.

The first tuning order comes from those observations. Carrying, controls and multiplayer responsiveness
are provisional priorities, not measured faults. Record the three roughest moments with where/when,
expected versus actual behaviour, and host/client role.

## Delivery status

Implemented and built 2026-10-08 (local time), version **0.11.0**. Unity compilation and full content
regeneration passed, including 22 original collectible models and 57 refreshed inventory icons.
Full Mac/Windows and demo Mac/Windows builds passed. The source changes are uncommitted (`400ff52-dirty`);
the matching platform pairs share their build timestamp and compatibility stamp.

| Build | Playable output | Archive |
|---|---|---|
| Full Mac (universal) | `Game/Builds/Mac/Abandoned.app` | [Mac zip](/Users/zeyad/Documents/Abandoned-builds/dev/Abandoned-0.11.0-400ff52-dirty-Mac.zip) |
| Full Windows (64-bit) | `Game/Builds/Windows/Abandoned.exe` | [Windows zip](/Users/zeyad/Documents/Abandoned-builds/dev/Abandoned-0.11.0-400ff52-dirty-Windows.zip) |
| Demo Mac | `Game/Builds/MacDemo/Abandoned.app` | [Mac demo zip](/Users/zeyad/Documents/Abandoned-builds/dev/Abandoned-0.11.0-400ff52-dirty-MacDemo.zip) |
| Demo Windows | `Game/Builds/WindowsDemo/Abandoned.exe` | [Windows demo zip](/Users/zeyad/Documents/Abandoned-builds/dev/Abandoned-0.11.0-400ff52-dirty-WindowsDemo.zip) |

The regular builds contain the full game for the user's test pass; demos retain their existing
five-job limit and separate saves. Mac archives use `ditto` to preserve application bundle links
and permissions. Development Steam App ID 480 remains in these local builds. No paid packs or
packages were added; all new art is original code-built content.

To play a build, extract the whole archive and open `Abandoned.app` (Mac) or `Abandoned.exe`
(Windows), keeping the accompanying files beside it. Use matching full builds for co-op;
the demos form a separate matching pair.

For an Editor playtest after delivery:

1. Open the `Game/` project with Unity 6000.3.25f1.
2. Open `Assets/_Project/Scenes/HQ.unity`.
3. Press Play; the existing editor auto-host setting starts the local HQ session.
4. Use Settings → Controls to review/rebind rotation and the other inputs, then follow the checklist above.

No gameplay sessions, EditMode/PlayMode tests, nettests or QA screenshots were run. Runtime feel,
silhouette readability at distance, balance, 1080p performance and network smoothness remain for
the user's playtest. Five pre-existing edited materials were restored byte-for-byte after the
rebuild; existing save identity, cosmetic catalog indices and user settings were preserved.

Suggested commit: `Polish salvage handling and abandoned mall presentation`.
