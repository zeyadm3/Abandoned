# Meridian Mall 0.13.1 — visibility and parking review

24 stills captured from the saved builder-owned Mall scene at 1920 × 1080 with
`Tools/unity.sh render-meridian`. These are Editor art renders; no gameplay or screenshot tests
were run. The capture discards its temporary lighting/camera state and checks the scene bytes unchanged.

The stronger flashlight uses the same 26 m, intensity 28, 68-degree outer / 40.8-degree inner cone
as the runtime equipment config. Powered mall fixtures are four times their 0.13.0 intensity;
ambient/moon fill is brighter, fog is thinner and the mall volume adds 0.2 EV exposure.

- `parking-night-*`: the actual player spawn view toward the recessed mall entrance.
- `parking-arrival-*`: the crew van, parking bays and pedestrian crossing.
- `collection-bay-*`: the real extraction truck, widened haul ramp and cargo boundaries.
- `atrium-*`, `food-court-*`, `cinema-*`, `dead-circuit-*`, `service-corridor-*`, `basement-*`
  and `gallery-wound-*`: matching power-on and power-off interior art views.
- `atrium-four-flashlights`: four beams with the existing owner-only flashlight shadow policy.
- `terrazzo-surface-*`: stable, cracking and failing floor-art closeups.

Power-off disables ceiling fixture lights and diffuser emission. Street utility lamps and the truck's
battery lamp remain independent. Power-on depicts faulty fixtures at a steady 75% and respects dead ones.
These captures contain no randomly spawned loot, threats or players, and do not reproduce runtime flicker.

The collection apron is a haul approach. Load items fully inside the existing truck cargo bay,
check its haul board, pull the departure lever and board the truck; payment follows departure.

`floorplans/` contains an SVG and ASCII plan for each level, exported from actual layout data.
`PERFORMANCE.md` records texture storage and scene counts. Batch-render UnityStats returning zero
means counters are unavailable; actual FPS/batches need the user's Editor F1 or Development-build profile.

Shareable builds passed from clean code commit **d95ced2** (Unity 6000.3.25f1, Mono):

- Mac: `~/Documents/Abandoned-builds/dev/Abandoned-0.13.1-d95ced2-Mac.zip` — 92.1 MiB.
- Windows: `~/Documents/Abandoned-builds/dev/Abandoned-0.13.1-d95ced2-Windows.zip` — 82.5 MiB.

Both BUILD.txt stamps carry version 0.13.1, clean commit d95ced2 and UTC timestamp
2026-10-08T15:23:09Z. Archive metadata was inspected without launching either player;
non-shipping debug folders are excluded. The subsequent delivery-note commit changes only documentation.
