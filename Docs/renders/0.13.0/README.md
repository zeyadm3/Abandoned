# Meridian Mall 0.13.0 — art review

The stills are generated from the saved builder-owned Mall scene at 1920 × 1080 with
`Tools/unity.sh render-meridian`, using the real URP materials, volume, fog and fixture data.
The scene is opened and discarded without saving; its bytes are checked unchanged. These
are art renders, not gameplay or screenshot tests. One flashlight is present in each normal
view; the atrium also has a separate four-flashlight view.

- `atrium-*`: the fountain, clipped atrium, escalators, bridge and upper circulation.
- `food-court-*`: the counter frontage and fixed seating.
- `cinema-*`: the Final Reel marquee and top-floor public entrance.
- `dead-circuit-*`: a fixed retail bay with scavenger damage.
- `service-corridor-*`: the permanent service spine and painted block finishes.
- `basement-*`: shallow flooding and stock/service spaces.
- `parking-night-*`: the actual player spawn view towards the recessed entry and anchor frontage.
- `gallery-wound-*`: the authored gallery collapse and exposed slab edges.
- `terrazzo-surface-*`: close views of the stable, cracking and failing authored floor art.

Power-on respects dead fixtures and shows faulty fixtures at steady 75% brightness. Power-off
removes their lamp and diffuser emission. Shutters are shown in their ordinary initial open pose;
a Sealed job and later director slams are runtime choices. Renders contain no randomly spawned
loot, monsters or players. Existing pickup art retains its previous style and needs your in-game
review against the new environment.

`floorplans/` contains an SVG and an ASCII map for each level, exported from authoritative layout
ownership and generated section data. `PERFORMANCE.md` contains texture import/storage figures,
scene counts and the available raw Editor counters. Actual four-player host FPS, visible batches,
GC and loading time still require your Editor F1/profiler pass; zero batch-render counters are
unavailable statistics, not zero rendering cost.
