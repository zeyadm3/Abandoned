#!/usr/bin/env python3
"""Downloads the CC0 Poly Haven models the TestMap site uses beyond fetch_models.py's single-material scans: trees,
grass, shrubs, rocks and multi-material kits (fences, power poles, fire escape...), and turns them into what Unity
reads (Editor/TestMap/ShowcaseNature.cs) in Game/Assets/_Project/Art/ThirdParty/PolyHaven/<id>/:

    <id>.fbx                       the model (trees: rebuilt with LOD0, LOD1 and an impostor LOD2, see bake_tree.py)
    <material>_diff/_nor_gl/_mask  per material, see bake_common.py

    python3 Tools/Art/fetch_nature.py              fetch every model that isn't there yet
    python3 Tools/Art/fetch_nature.py --force      fetch again
    python3 Tools/Art/fetch_nature.py <id> ...     only these
    FETCH_CACHE=<dir> python3 Tools/Art/fetch_nature.py   keep (or reuse) the raw downloads in <dir>

The raw scans are big (fir_tree_01 is a 249 MB FBX); only the baked results go into the project. The Blender scripts
run with --factory-startup; Blender ships numpy, nothing is installed. Credits: Docs/ASSET_CREDITS.md ("Poly Haven
models"); Poly Haven assets are CC0. Also fetches the sky (HDRI) and the terrain's ground scans (fetch_ground below).
"""
import json, os, shutil, subprocess, sys, tempfile, urllib.request

ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
OUT = os.path.join(ROOT, "Game", "Assets", "_Project", "Art", "ThirdParty", "PolyHaven")
GROUND = os.path.join(ROOT, "Game", "Assets", "_Project", "Art", "TestMap", "Ground")
SKY = os.path.join(ROOT, "Game", "Assets", "_Project", "Art", "TestMap", "Sky")
BLENDER = os.environ.get("BLENDER", "/Applications/Blender.app/Contents/MacOS/Blender")
HERE = os.path.dirname(os.path.abspath(__file__))
AGENT = {"User-Agent": "AbandonedAssetFetch/1.0"}  # Poly Haven refuses Python's default agent

# Trees: id -> bake spec (card faces kept at LOD0 and LOD1 per variant, trunk decimation ratios). See bake_tree.py.
TREES = {
    "fir_tree_01": {"lod_cards": [95000, 18000], "lod_wood": [0.3, 0.06], "lod_grow": [3.2, 4.5]},
    "tree_small_02": {"lod_cards": [40000, 9000], "lod_wood": [0.3, 0.06]},
    "fir_sapling_medium": {"lod_cards": [20000, 5000], "lod_wood": [0.4, 0.1], "impostor": 256},
    "pine_sapling_small": {"lod_cards": [16000, 4000], "lod_wood": [0.4, 0.1], "impostor": 256},
    "dead_tree_trunk_02": {"lod_cards": [0, 0], "lod_wood": [0.35, 0.08]},
}
# Everything else is used as it comes; only the maps are repacked (bake_kit.py).
KITS = [
    # ground cover and plants
    "grass_medium_01", "grass_medium_02", "shrub_02", "fern_02", "nettle_plant", "weed_plant_02", "dry_branches_medium_01",
    # rocks and wood
    "rock_moss_set_01", "boulder_01", "tree_stump_01", "dead_tree_trunk",
    # the yard
    "modular_chainlink_fence", "modular_electricity_poles", "modular_metal_gutter", "fire_hydrant", "security_light",
    "utility_box_02", "portable_searchlight", "barrel_stove", "street_lamp_02", "Barrel_02", "plastic_crate_03",
    "old_military_crate", "small_lpg_tank", "portable_welding_cart", "security_camera_01", "ladder_sectioned_01",
    "rusted_wheel_rim_01", "concrete_road_barrier",
]
# Terrain layers (ShowcaseTerrain) and the yard buildings' cladding (ShowcaseSite), packed like the models' maps.
GROUND_SCANS = ["forrest_ground_01", "grass_ground", "forest_leaves_02", "forest_ground_04", "rocky_trail",
                "gravel_ground_01", "asphalt_02", "brown_mud_leaves_01",
                "container_side", "corrugated_iron_02", "factory_wall", "box_profile_metal_sheet",
                "concrete_moss", "factory_brick"]
HDRI = "evening_road_01_puresky"
# The real maps' skies (Editor/Art/MapSite/MapSiteProfiles.cs says which map uses which), into Art/Skies.
MAP_SKIES = ["kloppenheim_01_puresky", "kloppenheim_07_puresky", "kloppenheim_06_puresky", "industrial_sunset_02_puresky",
             "kloofendal_43d_clear_puresky", "kloofendal_misty_morning_puresky"]
SKIES = os.path.join(ROOT, "Game", "Assets", "_Project", "Art", "Skies")
NATURE = os.path.join(ROOT, "Game", "Assets", "_Project", "Art", "TestMap", "Nature")
# The terrain's grass card: scanned clumps rendered side by side (bake_card.py).
CARDS = {"grass_card": ("grass_medium_01", ["grass_medium_01_tall_a_LOD0", "grass_medium_01_mid_a_LOD0",
                                            "grass_medium_01_tall_b_LOD0", "grass_medium_01_tall_c_LOD0"])}


def get(url):
    with urllib.request.urlopen(urllib.request.Request(url, headers=AGENT)) as response:
        return response.read()


def save(url, path):
    if not os.path.exists(path):
        with open(path, "wb") as f:
            f.write(get(url))


def download(model_id, cache):
    """The FBX and every map at 2k into <cache>/<id>/ (the FBX's include list misses some, e.g. alpha)."""
    files = json.loads(get(f"https://api.polyhaven.com/files/{model_id}"))
    folder = os.path.join(cache, model_id)
    os.makedirs(os.path.join(folder, "textures"), exist_ok=True)
    fbx = files["fbx"]["2k"]["fbx"]
    save(fbx["url"], os.path.join(folder, model_id + ".fbx"))
    for rel, v in fbx.get("include", {}).items():
        save(v["url"], os.path.join(folder, rel))
    for key, v in files.items():
        if key in ("fbx", "gltf", "usd", "blend") or not isinstance(v, dict) or "2k" not in v:
            continue
        formats = v["2k"]
        ext = "png" if "png" in formats else "jpg" if "jpg" in formats else None
        if ext:
            save(formats[ext]["url"], os.path.join(folder, "textures", f"{model_id}_{key}_2k.{ext}"))
    return folder


def blender(script, *args):
    command = [BLENDER, "--background", "--factory-startup", "--python", os.path.join(HERE, script), "--", *args]
    # EEVEE renders now and then abort on exit when another app holds the GPU (Unity); once more is enough.
    if subprocess.run(command, stdout=subprocess.DEVNULL).returncode != 0:
        subprocess.run(command, check=True, stdout=subprocess.DEVNULL)


def fetch(model_id, cache, force):
    target = os.path.join(OUT, model_id)
    if os.path.exists(os.path.join(target, model_id + ".fbx")) and not force:
        return "kept"
    raw = download(model_id, cache)
    with tempfile.TemporaryDirectory() as scratch:
        if model_id in TREES:
            blender("bake_tree.py", os.path.join(raw, model_id + ".fbx"), os.path.join(raw, "textures"), scratch, model_id,
                    json.dumps(TREES[model_id]))
        else:
            blender("bake_kit.py", os.path.join(raw, model_id + ".fbx"), os.path.join(raw, "textures"), scratch)
            shutil.copy(os.path.join(raw, model_id + ".fbx"), scratch)
        os.makedirs(target, exist_ok=True)
        for f in os.listdir(scratch):
            shutil.copy(os.path.join(scratch, f), target)
    return f"{sum(os.path.getsize(os.path.join(target, f)) for f in os.listdir(target)) / 1e6:.1f} MB"


def fetch_ground(force):
    """Each ground scan's colour, normal and ARM (packed to the mask like the models) at 2k, into Art/TestMap/Ground."""
    os.makedirs(GROUND, exist_ok=True)
    for tid in GROUND_SCANS:
        if os.path.exists(os.path.join(GROUND, tid + "_diff.jpg")) and not force:
            continue
        files = json.loads(get(f"https://api.polyhaven.com/files/{tid}"))
        with tempfile.TemporaryDirectory() as raw:
            for key, kind in (("Diffuse", "diff"), ("nor_gl", "nor_gl"), ("arm", "arm")):
                formats = files[key]["2k"]
                ext = "jpg" if "jpg" in formats else "png"
                save(formats[ext]["url"], os.path.join(raw, f"{tid}_{kind}_2k.{ext}"))
            blender("bake_ground.py", raw, GROUND, tid)
        print(f"{tid}: ground")


def fetch_sky(force):
    os.makedirs(SKIES, exist_ok=True)
    for sky in MAP_SKIES:
        path = os.path.join(SKIES, sky + "_4k.hdr")
        if force or not os.path.exists(path):
            save(json.loads(get(f"https://api.polyhaven.com/files/{sky}"))["hdri"]["4k"]["hdr"]["url"], path)
        print(f"{sky}: sky")
    os.makedirs(SKY, exist_ok=True)
    path = os.path.join(SKY, HDRI + "_4k.hdr")
    if force or not os.path.exists(path):
        files = json.loads(get(f"https://api.polyhaven.com/files/{HDRI}"))
        save(files["hdri"]["4k"]["hdr"]["url"], path)
    print(f"{HDRI}: sky")


def main(argv):
    force = "--force" in argv
    wanted = [a for a in argv if not a.startswith("--")]
    cache = os.environ.get("FETCH_CACHE")
    scratch = None
    if not cache:
        scratch = tempfile.TemporaryDirectory()
        cache = scratch.name
    for model_id in wanted or list(TREES) + KITS:
        if model_id not in TREES and model_id not in KITS:
            sys.exit(f"Unknown model '{model_id}'; add it to TREES or KITS first.")
        print(f"{model_id}: {fetch(model_id, cache, force)}", flush=True)
    if not wanted:
        fetch_ground(force)
        fetch_sky(force)
        os.makedirs(NATURE, exist_ok=True)
        for card, (model_id, parts) in CARDS.items():
            target = os.path.join(NATURE, card + ".png")
            if force or not os.path.exists(target):
                blender("bake_card.py", os.path.join(OUT, model_id), model_id, target, ",".join(parts))
            print(f"{card}: card")
    if scratch:
        scratch.cleanup()


if __name__ == "__main__":
    main(sys.argv[1:])
