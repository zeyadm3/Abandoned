# Cloudline / Apex rebuild review

23 October 2026. Unity batch captures of the saved Skyscraper scene with post processing, nearby reflection probes and the runtime indirect-light preview. No geometry or lights were added only for the pictures.

- `01-arrival`: street approach and tower canopy.
- `02-lobby`: open lobby and vertical circulation.
- `03-dining`: central restaurant aisle, chandeliers and cloud view at 404 metres.
- `04-lounge`: cocktail bar and skyline glazing.
- `05-kitchen`: commercial preparation and cooking lines.
- `06-stairwell`: continuous carry ramps and landings near the summit.
- `07-roof`: plant, parapets and maintenance gantry at 410 metres.
- `08-wine`: reserve cellar and its wide east entrance.
- `09-cloud-crown`: restaurant crown surrounded by the moving cloud bank.
- `10-tall-tower`: 410 metre Cloudline tower among the surrounding downtown.
- `11-downtown-street`: Broadway and neighboring high-rise blocks from street level.
- `12-downtown-panorama`: the city and cloud deck from the restaurant windows.

Regenerate: `Tools/unity.sh exec Abandoned.EditorTools.SkyscraperReview.RenderAndExit`.

Validation: `Tools/unity.sh editmode SkyscraperRebuildTests` — 33 checks cover circulation from street to roof, room access, unobstructed shaft, moving cab shell/controls, exposed call buttons, protected escape stairs, cloud shader/occlusion behavior, the two-part stair navigation, the 48-tower downtown, the saved world atlas and distant tower geometry, and terrain/forest clearance beneath the city. This is automated authoring verification; multiplayer carrying and lift timing still need an interactive playtest.
