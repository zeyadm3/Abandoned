# Meridian original environment kit — 0.13.2

This folder is generated only by `Tools/Blender/meridian.py`. All meshes, texture patterns,
graffiti and material sources are original ABANDONED work. No third-party inputs were used.
The legacy Environment kit, HQ and wardrobe exports are outside this generator's scope.

```sh
/Applications/Blender.app/Contents/MacOS/Blender --background --python Tools/Blender/meridian.py -- /absolute/repository all
```

Use `textures` or `meshes` instead of `all` to regenerate one part. Add `--force-textures`
to regenerate existing PNG contents. Preserve every existing `.meta` when regenerating.

The kit now has 97 FBX models. `detail` regenerates only the fourteen fitted fixtures introduced
in 0.13.2 by `Tools/Blender/meridian_detail.py`: ConcourseBench, MallBin, AdvertisingLightbox,
ClothingRack, RetailGondola, CheckoutCounter, FoodBooth, FoodServiceStation, CinemaScreen,
TicketCounter, ServiceCart, StockCage, CeilingDuct and ShutteredKiosk. Slats, empty hangers,
drawer fronts, mesh cage wire, sink/drain parts, duct seams and grille hardware reduce at LOD1/2.
All share the existing 25 PBR sets; no new textures or third-party inputs were added.
These are fixed dressing modules without imported colliders, loose loot or text.

`MeridianManifest.json` lists exact FBX keys and all PBR map names/resolutions.
`MeridianGeometry.json` records measured metre-space bounds, material slots and renderer
names after a Blender FBX round trip. `meridian_inspect.py` regenerates that handoff data;
it does not launch Unity or perform gameplay tests.

## Coordinate and import contract

- Unity `(x,y,z)` is authored as Blender `(x,z,y)`, in metres, with front/climb along Unity +Z.
- FBX uses forward -Z, up Y, file units, and `bake_space_transform=False`. Baking space
  transforms a second time beneath nested LOD empty groups produces incorrect units/axes.
- Unity imports should keep `useFileScale` and `bakeAxisConversion`, no imported colliders,
  animations or lights, imported normals and calculated Mikk tangents. Keep a neutral Unity
  placement holder; preserve its imported model child's conversion transforms.
- Every intact model has `LOD0`, `LOD1`, `LOD2` children, each with `<Asset>_LOD0/1/2`
  renderers. Builders must create an LODGroup or activate exactly one level; all three
  groups are present in the FBX. Decorative fragments reduce count at lower LODs.
- FluorescentFixture also has a separate root `Diffuser` renderer using `MALL_FixtureGlow`.
  Drive that renderer's emission with LightFixture, and attach the complete fixture to its slab.
- No FBX supplies collision. Builders author only useful gameplay collision around solid
  geometry; the thin floor/wall decay pieces and fixtures are decorative.
- EscalatorFractured, HaulRampFractured and ServiceStairFractured (also exported as
  StairsFractured) preserve their intact flight dimensions, with ten sibling Chunk_XX
  solids and centroid pivots; they do not contain duplicate LOD meshes.

## Structural and module dimensions

| Asset | Walking/solid dimensions | Pivot |
|---|---|---|
| FloorTerrazzo/Tile/Carpet/Concrete/Wood/Bridge | 4 x 4 m slab, 0.25 m thick; finish top Y=0 | Walking top centre |
| Floor*Cracked, Floor*Sagged | Same module; authored crack branches; sag 0.13 m at centre | Walking top centre |
| Floor*Fractured | Nine sibling Chunk_00..08, each with finish/core and centroid pivot | Common slab top-centre root |
| WallPlaster, WallBlock | 4 m wide x 3.75 m tall; core 0.24 m thick | Base centre |
| StorefrontGlazed/Boarded | 4 m display bay; not an entrance | Base centre |
| StorefrontEntry | 3.4 m nominal entrance; 3.37 m measured between inner jamb faces | Base centre |
| Column | 0.70 m maximum outer footprint; 3.75 m high | Base centre |
| GlassRail4m | 4 m nominal; handrail top 1.158 m; posts/rail/glass are separate solid spans | Base centre |
| Escalator | 2.4 m tread width; 8 m run, 4 m rise; tread midpoints follow Y=.5Z+.1 | Foot centre |
| ServiceStair | 3 m clear; 8 m run, 4 m rise; tread midpoints follow Y=.5Z+.1 | Foot centre |
| HaulRamp | 4 m clear; 8 m run, 2 m rise; walking skin Y=.25Z+.04 | Foot centre |
| RollerShutter | 3.4 m wide x 3.1 m tall | Base centre |
| CeilingGrid/Coffer/Damaged/Sagging | 4 x 4 m suspended panel module | Grid centre at mounting plane |
| SkylightFrame | 16 x 16 m perimeter, shallow pitched glazing; two broken panels | Perimeter base centre |
| EntranceCanopy | 12 x 4 m canopy; three-dimensional column/soffit/front crown | Base centre |

Escalator side assembly outer width is 2.94 m; ServiceStair is 3.41 m; HaulRamp is 4.41 m.
Handrails lie outside the clear walking surface. Use the measured JSON for exact extents.
Storefront and ceiling versions share trim and tiling materials rather than unique textures.
For a 4 m clear carry door, scale StorefrontEntry on X by `4 / 3.37` (about 1.187) and reserve
a 4.748 m facade bay; X=1.20 gives 4.044 m clear in a 4.8 m bay. Keep Z scale at one. Author
separate pier/jamb/header colliders; neither display glazing nor the empty entrance gets a
full-bay collider. WallPlaster/WallBlock opaque cores accept a 0.24 m thick wall collider;
their moulding and decal geometry remain decorative.

## Material contract

FBX material slots are named `MALL_<Set>`. Maps live at
`Textures/MALL_<Set>_BaseColor.png`, `_Normal.png`, `_Mask.png`.
Mask channels are R metallic, G occlusion, B spare, A smoothness. Bind the same mask to
URP Lit's metallic-gloss and occlusion slots; smoothness uses mask alpha.

Hero maps are 2048, standard finishes 1024, small props/decals 512. Base colours are sRGB;
normal and mask data are linear. Import normals as normal maps, use mipmaps, explicit desktop
compression/Max Size and no CPU readability. Glass/Water use transparent materials; the
Graffiti/Moss/Leaves/Scorch sets carry coverage alpha. FixtureGlow supplies the emissive diffuser.

Manufactured texture patterns include multicolour terrazzo aggregate, brushed steel, brass
trim strips, mortar/grout joints, carpet weave, wood grain/knots, mineral ceiling perforations,
concrete pores and weathering. Metro trim UVs choose stable ribbon regions; ordinary surfaces
use metre-based tiling UVs. The estimated compressed texture allocation for all 25 triplets,
including mipmaps, is about 127 MiB; actual platform memory must be reported by Unity.
