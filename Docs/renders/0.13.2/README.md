# Meridian Mall 0.13.2 — density and lighting review

Editor art renders show the builder-owned map at 1920 × 1080, with matching power on/off views.
`Tools/unity.sh render-meridian` captures the saved scene with temporary cameras/torch/power state
and checks its bytes unchanged. These are art stills; no gameplay or screenshot tests were run.

- `atrium-*`: roof-hung lights, fountain, circulation and fitted concourse remnants.
- `dead-circuit-*`, `threadbare-interior-*`: fixed shelves/counters/clothes rails and retail lighting.
- `food-court-*`: booths, serving stations and seating.
- `cinema-*`, `cinema-interior-*`: simplified marquee, fitted screen/counter and seating.
- `service-corridor-*`, `basement-*`: service fittings and flooded storage.
- `parking-night-*`, `parking-arrival-*`, `collection-bay-*`: the arrival/extraction routes with simpler signs.
- `gallery-wound-*`: the authored collapse wound.
- `atrium-four-flashlights` and `terrazzo-surface-*`: beam coverage and authored structural damage art.

Fourteen new original FBX modules add manufactured detail while reusing the existing PBR texture
sets. Scene dressing follows measured model footprints, layout/loot clearance and floor supports.
Scattered slogans, repeated storefront labels, van lettering and the instruction billboard are removed.
A small set of mounted identification/wayfinding remains, alongside the live HUD and truck haul board.

Power-on respects dead fixtures and shows faulty fixtures at a steady 75%; power-off switches off
mall fixture lights and their diffuser emission. Street utility lights/truck battery lighting are separate.
Art captures do not spawn loot, threats or players and do not reproduce runtime flicker/power-sector timing.

`floorplans/` is generated from the actual level data. `PERFORMANCE.md` records imported texture
memory, scene/fixture counts and raw Editor counters. Batch-render zero counters are unavailable data;
the four-player 60 FPS / ~2,500 batch target still needs the user's in-game profile.
