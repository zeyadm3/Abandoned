# Cloudline / Apex ground-up rebuild — 23 October 2026

The saved Skyscraper scene is regenerated from an empty Unity scene. The old independent room, stair, floor and façade layouts have been replaced by one coordinated architecture. Existing salvage definitions, host-authoritative gameplay and world-streaming identity remain connected.

## Layout

Cloudline is now 410 metres tall. The restaurant sits at 404 metres, with a moving, depth-tested cloud bank around the summit and a broad cloud deck below the windows. Procedural 3D density noise needs no downloaded textures or packages. The cloud volume clears the building interior and has no collision. The working lift travels at 8 m/s, taking about 51 seconds to reach the restaurant from the street.

The street and forecourt meet the world at elevation zero. A six metre open entrance leads into a marble lobby. The freight lift serves 0, 120 and 404 metres. The west fire core has 205 alternating, gently sloped half-flights, 2.7 metre carrying widths and full-width return landings. It reaches the flooded offices at 120 metres, mechanical floor at 200, abandoned offices at 280, restaurant at 404 and roof at 410. Open fire portals replace the previous solid door-frame cubes. The principal stair route is protected against collapse.

The Apex has an open central dining aisle with six dressed tables, an eastern cocktail bar, a northeast private dining room and southeast restrooms, a northwest commercial kitchen and a southwest reserve cellar with a separate 3.6 metre east entrance. A clear arrival corridor links both vertical routes. Floor-to-ceiling glazing exposes the skyline; bronze mullions, dark stone spandrels, mahogany, marble and leather establish a consistent palette. The roof has continuous access, perimeter guards, plant equipment, a helipad marking and a maintenance gantry.

All decks are split around the same exact lift and stair rectangles. No floor or ceiling crosses the cab travel volume. Dining window bays retain structural collapse and navigation carving, with obstacle dimensions taken from the real collider. The moving platform now carries the cab walls, roof, controls and light, rather than leaving those objects behind at ground level.

Salvage, threats, eight hiding places, 258 practical lights and breakers are authored against this layout. Navigation and occlusion data are regenerated. The map receives the existing sky, surface aging, object refinement, glazing, reflection and receiver-calibrated lighting passes during its individual rebuild.

The distant world model is refreshed to the new 410 metre silhouette along with the saved atlas look. Individual-map rebuilds update only this site's world model. The world survey explicitly retains summit weather while hiding standalone skyline scenery.

## Downtown expansion

The second pass raises Cloudline to a 410 metre roof with the restaurant at 404 metres, and replaces the six small skyline props with a persistent 720 × 720 metre downtown district. Its 48 neighbours range from 108 to 396 metres, with several substantial towers immediately around the restaurant. They have four façade palettes, individually proportioned shafts and setbacks, podium retail fronts, rooftop plant, distinct crowns, lit window patterns and aviation beacons. A regular avenue grid has sidewalks, crossings, street lamps, parked cars and bus shelters.

The city lives under `Exterior/Downtown`, remains present during open-world streaming, and participates in the distant world model. The atlas footprint and unloaded collision pad match the enlarged district. Only the four world terrain tiles touching this site are fitted; terrain holes and forest removal keep the new streets unobstructed. The downtown is closed-shell scenery with physical street surfaces; enemy navigation remains baked for the salvage building and its approach.

The main cloud deck sits lower beneath the restaurant, with sparse summit wisps, so neighboring rooftops can break through the cloud layer. Main-tower lift, stair landings, power, salvage and threat placements follow the new elevations.

The 410 metre switchback core exceeds the usable stacked-layer span of a single navigation bake. `SkyscraperNavigation` authors two bounded surfaces, split at 201 metres, and a bidirectional connection along the actual sloping stair flight at the mechanical level. The endpoints sit inside the two end landings of one continuous physical stair flight. Collider sources are explicitly assigned to one height band so a reloaded link cannot attach to an overlapping island. The automated checks exercise complete paths from the street to every accessible floor and roof through this join.

The cloud proxy also clamps its projected depth at the camera far plane, avoiding hard polygon holes in the cloud deck.

## Rebuild and inspect

1. In Unity, choose **Tools > Abandoned > Maps > Skyscraper Restaurant > Rebuild Skyscraper Restaurant** (the existing map menu).
2. Open `Assets/_Project/Scenes/Skyscraper.unity` and play to review carrying, lift interactions and room scale.

Batch authoring with the editor closed:

```sh
Tools/unity.sh exec Abandoned.EditorTools.SkyscraperBuilder.BuildAndExit
Tools/unity.sh editmode SkyscraperRebuildTests
Tools/unity.sh exec Abandoned.EditorTools.SkyscraperReview.RenderAndExit
```

Review captures are in `Docs/renders/skyscraper-rebuild`. Automated checks cover full-height stair openings, per-floor connections, complete street-to-roof navigation, restaurant room access, shaft clearance, cab child motion and protected stairs. Interactive multiplayer playtesting remains a separate check.
