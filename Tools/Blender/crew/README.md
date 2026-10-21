# Crew redesign (preview only)

Blender source for the proposed new player character. **Not in the game yet**: nothing here touches the
Unity project. It exists so the design can be reviewed and changed before integration.

**v2 (current):** a realistic stocky labourer in tactical salvage gear. Team-coloured ripstop jacket,
wolf-grey plate-carrier vest with pouches, khaki cargo trousers, hard knee pads, laced work boots, a realistic
face with a crew cut and short beard. Renders: `Docs/renders/crew-redesign/`. The first, stylized
design (v1) is kept for comparison in `Docs/renders/crew-redesign/v1-stylized/`.

```sh
/Applications/Blender.app/Contents/MacOS/Blender --background --factory-startup \
    --python Tools/Blender/crew/crew.py -- "$PWD" Docs/renders/crew-redesign turn face portrait lineup dark
```

Add `--fast` for half-resolution drafts.

| File | What it holds |
| --- | --- |
| `crew.py` | Entry point: studio, cameras, poses, the turnaround shot |
| `crew_shots.py` | Face close-up, portraits, four-person line-up, dark corridor |
| `crew_body.py` | Jacket, trousers, belt, arms with elbows, gloves, legs, boots on the rig pivots |
| `crew_vest.py` | Plate carrier: plates, cummerbund, straps, pouches, radio, drag handle, numbers |
| `crew_head.py` | The head: dense loft sculpted with numpy fields (nose, lips, brow, eyelids, age lines) |
| `crew_head_parts.py` | Neck, ears, eyes (cornea, lashes), hair/beard/brow shell grown from scalp masks |
| `crew_hats.py` | Hard hat, beanie and cap remodelled at real size |
| `crew_geo.py` | Lofts, quad spheres, tubes, straps pressed onto garments, padded slabs |
| `crew_materials.py` | Procedural Cycles materials; only `Jacket` takes the player's coverall colour |

Fit with the game: eyes at 1.65 m (`PlayerMovementConfig.StandingEyeHeight`), about 1.78 m tall. The parts hang
from pivots named like `WorkerRig`'s; what changes is listed in the integration plan (shoulders at
(+-0.19, 1.415), hips at (+-0.102, 0.90), knees at 0.50, a new elbow at 1.11, crouch drop 0.63 instead of 0.6).
The wardrobe's hats and face accessories were sized for the old 27 cm head and must be refitted; the hard hat,
beanie and cap here show the new size. The preview head is very dense (about 1 mm); the game mesh would be
retopologised with the detail baked into BaseColor/Normal/Mask maps.
