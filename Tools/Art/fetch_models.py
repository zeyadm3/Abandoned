#!/usr/bin/env python3
"""Downloads the CC0 photo-scanned models the TestMap showcase uses (Poly Haven, CC0 1.0) and keeps what Unity needs:
the FBX, the colour map, the OpenGL normal map, the emission map (when the model has one) and one packed mask made
from Poly Haven's ARM map (AO, roughness, metal) in the layout URP Lit reads:

    R = metallic, G = ambient occlusion, B = 0, A = smoothness (1 - roughness)

URP Lit takes metallic from R and smoothness from A of _MetallicGlossMap, and occlusion from G of _OcclusionMap, so
one texture serves both. The packing runs in Blender (Tools/Art/pack_mask.py), which ships numpy; nothing is installed.

    python3 Tools/Art/fetch_models.py            fetch every model that isn't already there
    python3 Tools/Art/fetch_models.py --force    fetch again
    python3 Tools/Art/fetch_models.py <id> ...   fetch only these

Files land in Game/Assets/_Project/Art/ThirdParty/PolyHaven/<id>/. Editor/TestMap/ShowcaseModels.cs makes the
materials. Credits: Docs/ASSET_CREDITS.md ("Poly Haven models"). Reads and writes only that folder.
"""
import json, os, subprocess, sys, tempfile, urllib.request

ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
OUT = os.path.join(ROOT, "Game", "Assets", "_Project", "Art", "ThirdParty", "PolyHaven")
BLENDER = os.environ.get("BLENDER", "/Applications/Blender.app/Contents/MacOS/Blender")
PACK = os.path.join(os.path.dirname(os.path.abspath(__file__)), "pack_mask.py")
AGENT = {"User-Agent": "AbandonedAssetFetch/1.0"}  # Poly Haven refuses Python's default agent

# Model id -> texture resolution, plus ":variant" for models sold in several finishes (Barrel_01: oxide_grey,
# oxide_green, explosive, radiation). 2k for anything seen up close or large, 1k for small clutter.
# Editor/TestMap/ShowcaseModels.cs keeps the same ids.
MODELS = {
    # warehouse floor
    "steel_frame_shelves_01": "2k", "steel_frame_shelves_02": "2k", "worn_metal_rack": "2k",
    "wooden_crate_01": "2k", "wooden_crate_02": "2k", "Barrel_01": "2k:oxide_grey", "barrel_03": "1k",
    "cardboard_box_01": "1k", "plastic_crate_01": "1k", "metal_tool_chest": "2k", "portable_generator": "2k",
    "hand_truck": "2k", "propane_tank": "1k", "cement_bag": "1k", "old_tyre": "1k", "metal_jerrycan": "1k",
    "multi_cleaner_5_litre": "1k", "plastic_broom": "1k", "metal_trash_can": "1k", "trashbag": "1k",
    "WetFloorSign_01": "1k", "tool_cart": "2k",
    # offices upstairs
    "metal_office_desk": "2k", "drawer_cabinet": "2k", "wooden_bookshelf_worn": "2k", "SchoolChair_01": "2k",
    "GreenChair_01": "2k", "Sofa_01": "2k", "desk_lamp_arm_01": "1k", "clipboard": "1k", "binder_notebook": "1k",
    "stationery_supplies": "1k",
    # light fittings and building services (wall-mounted models are left out: which way they face is unknown)
    "mounted_fluorescent_lights": "2k", "hanging_industrial_lamp": "2k", "caged_hanging_light": "1k",
    "utility_box_01": "1k", "exterior_aircon_unit": "1k:rusted_01",
    # outside
    "covered_car": "2k", "concrete_road_barrier_02": "2k", "street_lamp_01": "2k", "water_manhole_cover": "1k",
}


def get(url):
    with urllib.request.urlopen(urllib.request.Request(url, headers=AGENT)) as response:
        return response.read()


def save(url, path):
    data = get(url)
    with open(path, "wb") as f:
        f.write(data)
    return len(data)


def texture(files, variant, kind, resolution, path_without_extension):
    """One map, by the asset's own name (Diffuse, nor_gl, arm, Emission) or its variant's (<variant>_diff, ...).
    Returns the file written, or None when the model has no such map."""
    key = (f"{variant}_" + ("diff" if kind == "Diffuse" else kind.lower())) if variant else kind
    if key not in files:
        return None
    formats = files[key][resolution]
    extension = "jpg" if "jpg" in formats else "png"
    path = f"{path_without_extension}.{extension}"
    save(formats[extension]["url"], path)
    return path


def fetch(model_id, spec, force):
    resolution, _, variant = spec.partition(":")
    folder = os.path.join(OUT, model_id)
    fbx = os.path.join(folder, model_id + ".fbx")
    if os.path.exists(fbx) and not force:
        return 0
    os.makedirs(folder, exist_ok=True)
    files = json.loads(get(f"https://api.polyhaven.com/files/{model_id}"))
    save(files["fbx"][resolution]["fbx"]["url"], fbx)
    base = os.path.join(folder, model_id)
    if texture(files, variant, "Diffuse", resolution, base + "_diff") is None:
        sys.exit(f"{model_id}: no colour map{' for variant ' + variant if variant else ''}; pick a variant in MODELS.")
    texture(files, variant, "nor_gl", resolution, base + "_nor_gl")
    texture(files, variant, "Emission", resolution, base + "_emission")
    with tempfile.TemporaryDirectory() as scratch:
        arm = texture(files, variant, "arm", resolution, os.path.join(scratch, "arm"))
        if arm is None:
            sys.exit(f"{model_id}: no ARM map; it can't be packed.")
        mask = os.path.join(folder, model_id + "_mask.png")
        subprocess.run([BLENDER, "--background", "--factory-startup", "--python", PACK, "--", arm, mask],
                       check=True, stdout=subprocess.DEVNULL)
    return sum(os.path.getsize(os.path.join(folder, f)) for f in os.listdir(folder))


def main(argv):
    force = "--force" in argv
    wanted = [a for a in argv if not a.startswith("--")] or list(MODELS)
    total = 0
    for model_id in wanted:
        if model_id not in MODELS:
            sys.exit(f"Unknown model '{model_id}'; add it to MODELS first.")
        size = fetch(model_id, MODELS[model_id], force)
        total += size
        print(f"{model_id}: {'kept' if size == 0 else f'{size / 1e6:.1f} MB'}")
    print(f"Done, {total / 1e6:.0f} MB downloaded into {OUT}")


if __name__ == "__main__":
    main(sys.argv[1:])
