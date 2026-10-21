# Asset credits

Every third-party asset in the game, with where it came from and its licence. Only free CC0 assets
(Kenney, Quaternius, Poly Haven and similar), plus the existing user-approved SIL Open Font License
fonts. The approved 0.13.0 Meridian environment uses grounded realism; characters, threats and loot
retain their established style. New original mall art is recorded below alongside the existing credits.
Imported files live under `Game/Assets/_Project/Art/ThirdParty/<Author>/<Pack>/` with the pack's
own License.txt beside them.

| Pack | Author | Licence | Source | What we use | In the project |
|------|--------|---------|--------|-------------|----------------|
| Furniture Kit 2.0 | Kenney (www.kenney.nl) | CC0 1.0 | https://kenney.nl/assets/furniture-kit | Loot models (laptop, TVs, coffee machine, ...), store props | Art/ThirdParty/Kenney/FurnitureKit/ (all 141 FBX) |
| Car Kit 3.1 | Kenney (www.kenney.nl) | CC0 1.0 | https://kenney.nl/assets/car-kit | The HQ van, parked vehicles in the mall lot | Art/ThirdParty/Kenney/CarKit/ (delivery, truck, van + colormap.png) |
| Impact Sounds | Kenney (www.kenney.nl) | CC0 1.0 | https://kenney.nl/assets/impact-sounds | Loot impacts per material, footsteps, the plank snap and collapse debris | Art/ThirdParty/Kenney/ImpactSounds/ (126 of the pack's OGGs) |
| Interface Sounds | Kenney (www.kenney.nl) | CC0 1.0 | https://kenney.nl/assets/interface-sounds | Menu clicks, confirm/back/error, open/close | Art/ThirdParty/Kenney/InterfaceSounds/ (37 OGGs) |
| RPG Audio | Kenney (www.kenney.nl) | CC0 1.0 | https://kenney.nl/assets/rpg-audio | Structure creaks/groans, cloth (pickup), pocketing, coins (payday, the Collector), latch (truck lever), click (flashlight), distant settling | Art/ThirdParty/Kenney/RPGAudio/ (21 OGGs) |
| Kenney Fonts | Kenney (www.kenney.nl) | CC0 1.0 | https://kenney.nl/assets/kenney-fonts | Menu text until the UI overhaul (now unused) | Art/ThirdParty/Kenney/Fonts/ (2 TTFs) |
| Game Icons | Kenney (www.kenney.nl) | CC0 1.0 | https://kenney.nl/assets/game-icons | Menu icons (settings, audio, crew, trophy, warning, exit...) | Art/ThirdParty/Kenney/GameIcons/ (35 PNGs) |
| Generic Items | Kenney (www.kenney.nl) | CC0 1.0 | https://kenney.nl/assets/generic-items | Gear icons (flashlight, walkie-talkie, medkit, bolt cutters...) | Art/ThirdParty/Kenney/GenericItems/ (21 PNGs, renamed by gear) |
| Board Game Icons | Kenney (www.kenney.nl) | CC0 1.0 | https://kenney.nl/assets/board-game-icons | HUD symbols (skull, dollar, hourglass, lock, award...) | Art/ThirdParty/Kenney/BoardGameIcons/ (18 PNGs) |
| Input Prompts | Kenney (www.kenney.nl) | CC0 1.0 | https://kenney.nl/assets/input-prompts | Mouse button glyphs in key prompts | Art/ThirdParty/Kenney/InputPrompts/ (8 PNGs) |
| Barlow, Barlow Condensed | Jeremy Tribby (The Barlow Project Authors) | SIL OFL 1.1 | https://github.com/jpt/barlow (via Google Fonts) | Body text (Barlow), headings and buttons (Barlow Condensed) | Art/ThirdParty/Fonts/Barlow/, BarlowCondensed/ (4 TTFs + OFL.txt) |
| Noto Sans, Noto Sans Arabic | The Noto Project Authors (Google) | SIL OFL 1.1 | https://github.com/google/fonts (ofl/notosans*) | Russian headings, and names and chat in Cyrillic, Greek and Arabic (0.19.9) | Art/ThirdParty/Fonts/Noto/ (2 TTFs + OFL.txt) |
| Noto Sans SC, Noto Sans JP, Noto Sans KR (Medium) | Adobe and Google (The Noto CJK Authors) | SIL OFL 1.1 | https://github.com/notofonts/noto-cjk (Sans/SubsetOTF) | The Chinese, Japanese and Korean interface, and names and chat in those scripts (0.24.8; they replace the variable-weight SC and KR files of 0.19.9) | Art/ThirdParty/Fonts/Noto/ (3 OTFs + OFL-CJK.txt) |
| Saira Stencil One | Omnibus-Type (The Saira Stencil Project Authors) | SIL OFL 1.1 | https://github.com/Omnibus-Type/Saira (via Google Fonts) | Big titles and stamps only | Art/ThirdParty/Fonts/SairaStencilOne/ (1 TTF + OFL.txt) |
| IBM Plex Mono | IBM Corp. | SIL OFL 1.1 | https://github.com/IBM/plex (via Google Fonts) | The appraisal receipt | Art/ThirdParty/Fonts/IBMPlexMono/ (2 TTFs + OFL.txt) |
| VT323 | Peter Hull (The VT323 Project Authors) | SIL OFL 1.1 | https://github.com/phoikoi/VT323 (via Google Fonts) | The HQ terminal | Art/ThirdParty/Fonts/VT323/ (1 TTF + OFL.txt) |
| Scanned surfaces (0.24.12): Concrete034, Concrete031, Concrete042A, Bricks106A, Plaster001, PaintedPlaster017, Tiles107, Terrazzo018, WoodFloor051, Planks037A, Carpet016, Asphalt031, Metal049A, Metal063, MetalWalkway014, CorrugatedSteel009, Gravel023, Ground037, Grass004, Rock051, Leather037 | ambientCG (Lennart Demes) | CC0 1.0 | https://ambientcg.com (fetched by Tools/Art/fetch_textures.py) | Colour, normal, roughness, AO and metalness of the environment materials (Editor/Art/MaterialUpgrade) | Art/Scanned/<SetId>/ (2K JPGs + packed MetalSmooth PNGs) |
| Scanned surfaces (0.24.13): PaintedMetal012, Metal016, Metal041B, Plastic010, Plastic011, Rubber004, Concrete046, Concrete036, Plaster007, PaintedPlaster006, PaintedBricks004, OfficeCeiling001, Tiles141, Tiles140, Tiles036, Chipboard004, WoodSiding009, Marble012, PavingStones126A, DiamondPlate008A, Fabric030, Snow010A, Snow013, Rock035 | ambientCG (Lennart Demes) | CC0 1.0 | https://ambientcg.com (fetched by Tools/Art/fetch_textures.py) | The surfaces that were still flat or code-drawn: painted metal, plastic, ceilings, tiles, block walls, snow, sea rock (Editor/Art/MaterialUpgrade) | Art/Scanned/<SetId>/ (2K JPGs + packed MetalSmooth PNGs) |
| Recorded sound library (0.24.12) | Freesound contributors, one row per sound in Tools/Audio/library.csv | CC0 1.0 (only CC0 sounds are fetched; the search itself filters on it) | https://freesound.org (high-quality previews, fetched by Tools/Audio/fetch_library.py) | Footsteps per surface, impacts per material, structure, doors, gear, environment loops, monster vocals (Editor/Audio/SoundLibrary) | Art/Audio/LibrarySource/ (as fetched) and Art/Audio/Library/ (trimmed, levelled WAVs) |
| Steam Audio 4.8.1 (Unity plugin) | Valve Corporation | Apache-2.0 (third-party parts: Docs/ThirdParty/SteamAudio_THIRDPARTY.md) | https://github.com/ValveSoftware/steam-audio/releases/tag/v4.8.1 | Head-related 3D panning of world sounds and crew voices (Audio/SpatialAudio) | Assets/Plugins/SteamAudio/ (scripts, Resources, macOS and Windows x64 binaries only) |
| Kokoro-82M speech model | hexgrad | Apache-2.0 | https://huggingface.co/hexgrad/Kokoro-82M (run locally through kokoro-onnx, MIT) | The voices of the boss's voicemails, the dispatch telephone and the training job's dispatcher and trainer | Art/Audio/Voicemail/, Art/Audio/HQDispatch/, Art/Audio/TutorialDispatch/ and Art/Audio/TutorialNarration/ (WAVs made by Tools/Audio/*.py; the model itself is not in the project) |

Synthesised in our own code (no third party): the Blind One's click, the truck horn, the noise maker's shriek, radio static,
the collapse rumble, the ambience (wind, light hum), and all the music (HQ, travel and departure loops,
and the existing stings).

Crediting Kenney isn't required by CC0; we do it anyway (credits screen, M8).

Original art generated by this project's own code for the 0.11.0 presentation pass: low-poly salvage
worker body parts and clothing details, four monster silhouettes, 22 collectible models, mall decay
materials/textures, signage and sparse decorative debris. These use no additional third-party pack. Generated materials
and meshes live under `Art/Polish/`; their editor builders are kept with the project so they can be regenerated.


Original assets for the 0.12.0 horror redesign (2026-10-08):

| Pack | Author | Licence | Source | What we use | In the project |
|------|--------|---------|--------|-------------|----------------|
| Meridian environment kit | ABANDONED original art | Project-owned original work | Tools/Blender/environment.py | 52 custom modular architecture, prop, decay and mystery FBX assets; 21 procedural PNG textures | Art/Custom/Environment/ |
| Threat rigs and animation | ABANDONED original art | Project-owned original work | Tools/Blender/threats.py | Eight custom skinned threat silhouettes, Idle/Walk/Chase/Attack/Special animation takes and procedural grime | Art/Custom/Threats/ |
| Horror sound design | ABANDONED original audio | Project-owned original work | Scripts/Audio/HorrorAudio.cs and Scripts/Threats/ThreatHorrorAudio.cs | Engine, siren, roar, whispers, ceiling movement, wall knocking, area water/vent sounds and threat signatures | Synthesised at runtime |
| Salvage paperwork texture | ABANDONED original art | Project-owned original work | Editor/UI/UiHorrorTextureBuilder.cs | Procedural worn paper and grime for readable horror menus | Art/UI/ |
| Menu film grain, vignette, scratches, stain and glow | ABANDONED original art | Project-owned original work | Editor/UI/MenuEffectsBuilder.cs | 0.12.2 procedural textures for the horror menus' grain, vignette, worn title and button glow | Art/UI/Menu*.png |
| Surface grain and soft particle maps | ABANDONED original art | Project-owned original work | Editor/Art/PolishAssets.cs, Editor/Art/MaterialAudit.cs | 0.12.2 procedural maps for previously untextured materials | Art/Polish/ |
| Wardrobe and HQ props | ABANDONED original art | Project-owned original work | Tools/Blender/environment.py | 0.12.5: 14 hats and 11 accessories fitted to the crew worker, the HQ answering machine, 11 more procedural palette textures | Art/Custom/Environment/Cosmetics/, Art/Custom/Environment/ |
| World text shader | ABANDONED original code | Project-owned original work | Art/Shaders/WorldText.shader | Single-sided, depth-tested sign and label text | Art/Shaders/ |

No paid packs or new Unity packages were added for this pass. Existing CC0 vehicle/loot/audio and OFL font credits above still apply.

Original assets for the approved 0.13.0 Meridian Mall rebuild (2026-10-08):

| Pack | Author | Licence | Source | What we use | In the project |
|------|--------|---------|--------|-------------|----------------|
| Meridian realistic architecture and structure kit | ABANDONED original art | Project-owned original work | Tools/Blender/meridian.py, meridian_core.py, meridian_surfaces.py, meridian_architecture.py, meridian_props.py, meridian_decay.py | 83 deterministic FBX exports: six floor finishes with crack/sag variants and nine-piece fracture siblings; ten-piece flight fractures; walls, storefronts, columns, balustrades, escalators, stairs, haul ramp, ceilings, skylight, entrance canopy, facade, fixed retail fixtures, cinema, dock and decay props; major models carry LOD0/1/2 groups | Art/Custom/Environment/Meridian/ (exact file inventory: MeridianManifest.json) |
| Meridian original PBR surfaces | ABANDONED original art | Project-owned original work | Tools/Blender/meridian_textures.py | 25 procedural PBR sets / 75 PNGs: original terrazzo aggregate, tile/mortar, carpet weave, concrete pores, plaster, brushed metal, brass trim, ceiling perforations, wood grain, rust, glass, water and small dressing surfaces; 2048 hero, 1024 standard, 512 small props/decals; base colour, normal and packed metallic/occlusion/smoothness mask | Art/Custom/Environment/Meridian/Textures/ (all MALL_* PNG files listed in MeridianManifest.json) |
| Meridian fictional sign and graffiti designs | ABANDONED original art and writing | Project-owned original work | Tools/Blender/meridian_decay.py, meridian_textures.py and Editor/Mall/ builders | Original sign frames, worn notice silhouettes and non-offensive abstract graffiti; fictional store/wayfinding text uses the existing Abandoned/WorldText shader and credited fonts | Art/Custom/Environment/Meridian/SignFrame.fbx, MissingPoster.fbx, GraffitiTag.fbx and Textures/MALL_Graffiti_*.png; generated mall world text |
| Meridian overcast night sky | ABANDONED original art and code | Project-owned original work | Editor/Mall/MeridianSkyBuilder.cs and Art/Shaders/MeridianSky.shader | Deterministic 512 px cloud texture and textured URP sky shader for the mall exterior | Art/Generated/MeridianAtmosphere/OvercastNight.png and OvercastNight.mat; Art/Shaders/MeridianSky.shader |

No third-party files were downloaded for the 0.13.0 Meridian kit. Its texture patterns, mesh
geometry and graffiti are authored from the original scripts above. Existing third-party credits
remain valid; no paid assets or new Unity packages were introduced. Import settings, map packing,
pivots and measured FBX geometry are documented in the kit README/manifest and Tools/Blender/README.md.

Original presentation for the 0.13.1 visibility and parking update (2026-10-08):

| Pack | Author | Licence | Source | What we use | In the project |
|------|--------|---------|--------|-------------|----------------|
| Meridian parking and collection markings | ABANDONED original art and writing | Project-owned original work | Editor/Mall/MeridianParkingBuilder.cs and MeridianCollectionBayBuilder.cs | Parking bays, crosswalk, directional arrows, utility lamps, original crew-van lettering and collection/cash-out notices; material variants reuse the original Meridian PBR maps | Art/Generated/MeridianParking/ and Art/Generated/MeridianCollection/; generated Mall scene |

The crew arrival van and three parked vehicles reuse the already credited Kenney Car Kit models and
colour texture under `Art/ThirdParty/Kenney/CarKit/` (Kenney, CC0, https://kenney.nl/assets/car-kit).
No additional third-party files were downloaded for 0.13.1.

Original assets for the 0.13.2 mall density pass (2026-10-08):

| Pack | Author | Licence | Source | What we use | In the project |
|------|--------|---------|--------|-------------|----------------|
| Meridian detailed fitted fixtures | ABANDONED original art | Project-owned original work | Tools/Blender/meridian_detail.py | Fourteen original modules: slatted concourse bench, mall bin, blank advertising lightbox, empty clothing rack/hangers, retail gondola, checkout counter, food booth, food service station, cinema screen, ticket counter, empty wire service cart, stock cage, ceiling duct and shuttered kiosk. LOD0/1/2 meshes share the original Meridian PBR maps | Art/Custom/Environment/Meridian/ (exact names in MeridianManifest.json) |

The fixture geometry is generated by the original source above. No new third-party files, packages
or paid assets were added; the existing PBR textures and vehicle/font credits continue to apply.

Voice lines (0.14.5 dispatch, 0.24.2 boss voicemails; voice replaced 2026-10-12) are original text spoken by the
Kokoro-82M speech model (row above; Apache-2.0 weights) on the developer's machine through Tools/Audio/*.py and
processed into telephone/tape sound. Voices `af_heart`, `af_bella` and `bf_emma` answer the dispatch telephone (one at
random per call); the voicemails take turns through `am_michael`, `am_fenrir`, `am_onyx` and `bm_george`. The training
job (2026-10-21) uses the same three operators for its dispatcher's radio lines and `am_michael` for the trainer who
reads every lesson (Tools/Audio/generate_tutorial_dispatch.py, generate_tutorial_narration.py). No paid
service and no recording of a real person. The first clips used macOS system voices, which Apple licenses for personal,
non-commercial use only; they are gone from the project and `say` must not be used for shipped audio.

The ABANDONED desktop emblem (0.24.6b, 2026-10-10) is original project artwork generated
with the built-in `image_gen` tool: fractured architectural A with light and dark concrete
tiles. Final Unity PNGs are under `Art/Branding/`; masters, Windows light-only ICO and exact
prompts are under `Tools/Branding/0.24.6b/`. No third-party logo, font, pack or Unity package
is used by this emblem. Details: `Docs/APP_LOGO_0.24.6b.md`.

### Tutorial house — Larch Court refinement (2026-10-21)

- **Source:** original procedural geometry and PBR textures authored for ABANDONED; no third-party downloads.
- **Files:** `Game/Assets/_Project/Art/Custom/Environment/LarchCourt/` — 19 FBX models with three LODs each and 16 seamless texture sets (48 PNGs).
- **Generators:** `Tools/Blender/tutorial/build.py`, `Tools/Blender/tutorial/textures.py`, using the project's original `meridian_core.py` helpers.
- **Integration and verification:** `Docs/TUTORIAL_HOUSE_REFINEMENT.md`.

### Map and object art remaster (2026-10-22)

- **Paper001:** [ambientCG Paper 001](https://ambientcg.com/view?id=Paper001), CC0 1.0. 2K colour, normal and roughness maps for unprinted paper fibres and page backs.
- **Cardboard001:** [ambientCG Cardboard 001](https://ambientcg.com/view?id=Cardboard001), CC0 1.0. 2K colour, normal and roughness maps for cartons and corrugated packing.
- Downloaded with the existing `Tools/Art/fetch_textures.py` workflow into `Art/Scanned/`. Existing texture, model, font and audio credits remain in force. No new packages or paid assets.
