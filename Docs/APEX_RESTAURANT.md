# Apex restaurant — two-story fine dining

24 October 2026. The Cloudline crown now contains the 404m dining room and the 410m gallery, below a relocated 416m maintenance roof.

The lower floor has a marble arrival hall, six dressed dining tables, a champagne salon and grand piano, cocktail bar, reserve wine vault, commercial kitchen and powder room. An open atrium joins it visually to the upper gallery. A broad switchback staircase connects the two dining stories, with continuous carry ramps beneath marble treads and brass nosings; the protected fire core also serves both stories and the roof.

The upper floor has four gallery tables, an eight-seat chef's tasting room with a finishing counter, a furnished display library and the private dining salon. Emerald upholstery, ivory linen, bone china, chargers, cutlery, crystal stemware, tasting cards, flowers, candles, balcony reveals, wood paneling and a three-tier continuous halo chandelier, opal pier sconces and salon floor lamps establish the fine-dining character. Cloud views and the abandoned tower setting remain part of the map.

The upper deck is cut around both the atrium and grand stairs. Its floor tiles participate in the existing structural simulation; selected east window tiles can collapse while the gallery circulation stays protected. Upper loot, hiding places, lights, a breaker panel and threat positions use the new floor. Roof equipment, roof loot, navigation bounds, atmosphere bounds and the distant world model follow the raised roof.

Apply in Unity: **Tools → Abandoned → Maps → Skyscraper Restaurant → Rebuild Skyscraper Restaurant**. The existing rebuild command includes the world model refresh.

Batch authoring: `Tools/unity.sh exec Abandoned.EditorTools.SkyscraperBuilder.BuildAndExit`.

Review renders: `Tools/unity.sh exec Abandoned.EditorTools.SkyscraperReview.RenderAndExit`.

Focused verification: `Tools/unity.sh editmode SkyscraperRebuildTests`.

Validation: Unity compilation and the saved scene rebuild completed successfully. All 44 focused EditMode checks passed, including direct grand-stair routing, standing headroom, upper-room and powder-room access, protected fire stairs, the raised roof, structural visuals, persisted chandelier emission, clouds and world streaming. Multiplayer carrying has not been playtested interactively.

Seventeen review renders were regenerated from the final saved scene; the atrium, upper gallery, grand staircase, chef's room, private salon and lounge were visually reviewed.
