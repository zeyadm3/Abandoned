#!/usr/bin/env python3
"""Downloads the CC0 photo-scanned PBR sets the 0.24.12 surface upgrade uses (ambientCG, CC0 1.0) and keeps
only the maps Unity needs: Color, NormalGL, Roughness, AmbientOcclusion, Metalness (when the set has one).

    python3 Tools/Art/fetch_textures.py            fetch every set that isn't already there
    python3 Tools/Art/fetch_textures.py --force    fetch again

Files land in Game/Assets/_Project/Art/Scanned/<SetId>/. Editor/Art/MaterialUpgrade.cs maps materials to sets.
Credits: Docs/ASSET_CREDITS.md ("Scanned surfaces"). Reads and writes only those folders.
"""
import io, json, os, sys, urllib.parse, urllib.request, zipfile

ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
OUT = os.path.join(ROOT, "Game", "Assets", "_Project", "Art", "Scanned")
RESOLUTION = "2K-JPG"
KEEP = ("_Color.jpg", "_NormalGL.jpg", "_Roughness.jpg", "_AmbientOcclusion.jpg", "_Metalness.jpg")

# Set id -> what it covers (Editor/Art/MaterialUpgrade.cs keeps the same ids).
SETS = {
    "Concrete034": "clean poured concrete, pavements",
    "Concrete031": "worn, stained concrete",
    "Concrete042A": "damp, mottled concrete (basements, the prison)",
    "Bricks106A": "brick and block walls",
    "Plaster001": "plaster walls and ceilings",
    "PaintedPlaster017": "painted plaster (institutional walls)",
    "Tiles107": "floor tile, lino, epoxy",
    "Terrazzo018": "terrazzo floors",
    "WoodFloor051": "wooden floors, gym and stage boards",
    "Planks037A": "planks, siding, timber",
    "Carpet016": "carpet and rugs",
    "Asphalt031": "asphalt, roads, car parks",
    "Metal049A": "painted and bare steel",
    "Metal063": "rusted iron and oxidised steel",
    "MetalWalkway014": "steel grating",
    "CorrugatedSteel009": "corrugated cladding",
    "Gravel023": "gravel, ballast, beach shingle",
    "Ground037": "dirt, earth, clay",
    "Grass004": "grass and turf",
    "Rock051": "rock and cliffs",
    "Leather037": "leather",
    # 0.24.13: the surfaces that were still flat colour or code-drawn.
    "PaintedMetal012": "painted steel with chipped paint and rust (lockers, cabinets, doors)",
    "Metal016": "light painted metal, scuffed (consoles, appliances, machine casings)",
    "Metal041B": "painted steel with rust runs (rails, posts, outdoor steel)",
    "Plastic010": "smooth plastic (light colours)",
    "Plastic011": "smooth plastic (dark colours)",
    "Rubber004": "rubber flooring and mats",
    "Concrete046": "smooth cast concrete walls",
    "Concrete036": "dark old concrete (cellars, tunnels)",
    "Plaster007": "old painted plaster, chipped (interior walls)",
    "PaintedPlaster006": "weathered painted render (exterior facades)",
    "PaintedBricks004": "painted brick and block walls (corridors, cells, classrooms)",
    "OfficeCeiling001": "suspended acoustic ceiling tiles",
    "Tiles141": "beige rectangular tiles, dirty (kitchens, washrooms)",
    "Tiles140": "dark industrial tiles, corroded",
    "Tiles036": "white glazed wall tiles",
    "Chipboard004": "chipboard and boarding",
    "WoodSiding009": "painted lap siding (houses)",
    "Marble012": "polished grey marble",
    "PavingStones126A": "concrete paving slabs",
    "DiamondPlate008A": "steel tread plate",
    "Fabric030": "upholstery fabric",
    "Snow010A": "fresh snow (the school grounds, the training street)",
    "Snow013": "trodden snow over paving and asphalt",
    "Rock035": "dark wet sea rock (the island)",
}


def fetch(set_id, force):
    folder = os.path.join(OUT, set_id)
    if not force and os.path.isdir(folder) and any(f.endswith("_Color.jpg") for f in os.listdir(folder)):
        print(f"  {set_id}: already here")
        return
    url = f"https://ambientcg.com/get?file={urllib.parse.quote(set_id)}_{RESOLUTION}.zip"
    req = urllib.request.Request(url, headers={"User-Agent": "Mozilla/5.0 (Abandoned texture fetch)"})
    data = urllib.request.urlopen(req, timeout=300).read()
    os.makedirs(folder, exist_ok=True)
    kept = 0
    with zipfile.ZipFile(io.BytesIO(data)) as z:
        for name in z.namelist():
            base = os.path.basename(name)
            if not base.endswith(KEEP) or "/" in base or base.startswith("."):
                continue
            with z.open(name) as src, open(os.path.join(folder, base), "wb") as dst:
                dst.write(src.read())
            kept += 1
    print(f"  {set_id}: {kept} maps ({len(data) // 1048576} MB zip)")


def main():
    force = "--force" in sys.argv
    print(f"ambientCG CC0 sets -> {OUT}")
    failed = []
    for set_id in SETS:
        try:
            fetch(set_id, force)
        except Exception as e:  # keep going; the upgrade skips a set that isn't there
            failed.append(set_id)
            print(f"  {set_id}: FAILED {e}")
    print("failed: " + (", ".join(failed) if failed else "none"))
    return 1 if failed else 0


if __name__ == "__main__":
    sys.exit(main())
