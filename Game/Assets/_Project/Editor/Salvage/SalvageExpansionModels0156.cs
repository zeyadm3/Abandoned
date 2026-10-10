using System.Collections.Generic;
using System.Linq;
using Abandoned.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using static Abandoned.EditorTools.SalvageModelFactory;

namespace Abandoned.EditorTools
{
    /// <summary>Original retail and clearance silhouettes, with persistent meshes and shared worn finishes.</summary>
    public static class SalvageExpansionModels0156
    {
        public const string Folder = "Assets/_Project/Art/Generated/Salvage0156";
        public static readonly string[] Items = { "receipt_printer", "card_terminal", "headphone_carton",
            "network_switch", "showroom_sofa", "floor_lamp", "office_printer", "locker_bank",
            "air_compressor", "welding_trolley", "workshop_vacuum", "shoe_cartons", "cash_register",
            "clearance_barricade", "extension_reel", "mobile_planning_board", "duct_stack", "recovery_cart" };

        public static GameObject Load(string id) => AssetDatabase.LoadAssetAtPath<GameObject>($"{Folder}/Prefabs/{id}.prefab");

        public static void Author()
        {
            PolishAssets.EnsureFolder(Folder + "/Meshes");
            PolishAssets.EnsureFolder(Folder + "/Prefabs");
            foreach (string id in Items)
            {
                var root = new GameObject(id);
                try
                {
                    var lods = new LOD[3];
                    Bounds bounds = default;
                    for (int lod = 0; lod < 3; lod++)
                    {
                        Transform level = Group("LOD" + lod, root.transform);
                        Recipe(id, level, lod);
                        if (lod == 0) bounds = ModelFit.LocalBounds(level.gameObject, root.transform);
                        lods[lod] = new LOD(lod == 0 ? .34f : lod == 1 ? .13f : .03f,
                            Combine(level, id, lod));
                    }
                    root.AddComponent<LODGroup>().SetLODs(lods);
                    root.GetComponent<LODGroup>().RecalculateBounds();
                    var collider = root.AddComponent<BoxCollider>();
                    collider.center = bounds.center;
                    collider.size = bounds.size;
                    root.AddComponent<SurfaceTag>().EditorSet(id is "showroom_sofa" or "shoe_cartons"
                        ? SurfaceMaterial.Wood : SurfaceMaterial.Metal);
                    Transform sockets = Group("Sockets", root.transform);
                    Group("HighlightAnchor", sockets).localPosition = bounds.center;
                    Group("InspectionAnchor", sockets).localPosition = bounds.center + Vector3.forward * (bounds.extents.z + .1f);
                    PrefabUtility.SaveAsPrefabAsset(root, $"{Folder}/Prefabs/{id}.prefab");
                }
                finally { Object.DestroyImmediate(root); }
            }
            Debug.Log($"[MallExpansion0156] Authored {Items.Length} new original prop families, each with three saved LODs.");
        }

        private static Renderer[] Combine(Transform level, string id, int lod)
        {
            var groups = new Dictionary<Material, List<CombineInstance>>();
            MeshFilter[] sources = level.GetComponentsInChildren<MeshFilter>();
            foreach (MeshFilter source in sources)
            {
                Material material = source.GetComponent<MeshRenderer>().sharedMaterial;
                if (!groups.TryGetValue(material, out var parts)) groups[material] = parts = new();
                parts.Add(new CombineInstance { mesh = source.sharedMesh,
                    transform = level.worldToLocalMatrix * source.transform.localToWorldMatrix });
            }
            var renderers = new List<Renderer>();
            foreach (var pair in groups.OrderBy(pair => pair.Key.name, System.StringComparer.Ordinal))
            {
                var mesh = new Mesh { name = id + "_LOD" + lod + "_" + pair.Key.name,
                    indexFormat = IndexFormat.UInt32 };
                mesh.CombineMeshes(pair.Value.ToArray(), true, true);
                mesh.RecalculateBounds();
                mesh = SalvageAssetBuilder.SaveMesh(mesh, $"{Folder}/Meshes/{mesh.name}.asset");
                Transform draw = Group(pair.Key.name, level);
                draw.gameObject.AddComponent<MeshFilter>().sharedMesh = mesh;
                var renderer = draw.gameObject.AddComponent<MeshRenderer>();
                renderer.sharedMaterial = pair.Key;
                renderers.Add(renderer);
            }
            foreach (MeshFilter source in sources) Object.DestroyImmediate(source.gameObject);
            return renderers.ToArray();
        }

        private static void Recipe(string id, Transform root, int lod)
        {
            int sides = lod == 0 ? 12 : lod == 1 ? 8 : 6;
            switch (id)
            {
                case "receipt_printer":
                    Box(root, "Printer case", new(0, .12f, 0), new(.32f, .24f, .36f), "Plastic");
                    Box(root, "Paper roll lid", new(0, .265f, .04f), new(.30f, .05f, .24f), "Paint");
                    Box(root, "Unfinished receipt", new(0, .265f, -.15f), new(.19f, .008f, .17f), "Paper");
                    if (lod < 2) Box(root, "Feed slot", new(0, .238f, -.184f), new(.20f, .025f, .01f), "Rubber");
                    break;
                case "card_terminal":
                    Box(root, "Payment terminal", new(0, .055f, 0), new(.19f, .11f, .28f), "Plastic");
                    Box(root, "Dead LCD", new(0, .117f, .06f), new(.145f, .018f, .095f), "Glass");
                    if (lod < 2) for (int i = 0; i < 9; i++)
                        Box(root, "Keypad", new(-.055f + i % 3 * .055f, .118f, -.035f - i / 3 * .045f), new(.035f, .012f, .025f), "Rubber");
                    break;
                case "headphone_carton":
                    Box(root, "Headphone retail carton", new(0, .16f, 0), new(.25f, .32f, .18f), "Paper");
                    Box(root, "Dark printed sleeve", new(0, .16f, -.094f), new(.21f, .24f, .008f), "Plastic");
                    Cylinder(root, "Ear cup graphic", new(-.052f, .16f, -.102f), new(.066f, .009f, .066f), "Paint", new(90, 0, 0), sides);
                    Cylinder(root, "Ear cup graphic", new(.052f, .16f, -.102f), new(.066f, .009f, .066f), "Paint", new(90, 0, 0), sides);
                    break;
                case "network_switch":
                    Box(root, "Rack network switch", new(0, .055f, 0), new(.48f, .11f, .25f), "Metal");
                    Box(root, "Port bank", new(0, .055f, -.13f), new(.39f, .054f, .012f), "Plastic");
                    if (lod < 2) for (int i = 0; i < 8; i++) Box(root, "Ethernet socket", new(-.16f + i * .046f, .055f, -.14f), new(.023f, .025f, .008f), "Copper");
                    break;
                case "showroom_sofa":
                    Box(root, "Sofa base", new(0, .22f, 0), new(1.75f, .28f, .76f), "Fabric");
                    Box(root, "Sofa back", new(0, .59f, .29f), new(1.72f, .65f, .18f), "Fabric");
                    foreach (float side in new[] { -1f, 1f })
                    {
                        Box(root, "Upholstered arm", new(side * .79f, .45f, 0), new(.19f, .46f, .73f), "Fabric");
                        Box(root, "Separate cushion", new(side * .37f, .40f, -.02f), new(.68f, .12f, .53f), "Fabric");
                        foreach (float z in new[] { -.26f, .26f }) Box(root, "Sofa foot", new(side * .70f, .07f, z), new(.09f, .14f, .09f), "Wood");
                    }
                    break;
                case "floor_lamp":
                    Cylinder(root, "Weighted lamp base", new(0, .035f, 0), new(.4f, .07f, .4f), "Metal", sides: sides);
                    Bar(root, "Lamp upright", new(0, .06f, 0), new(0, 1.55f, 0), .035f, "Metal", sides);
                    Cylinder(root, "Dusty fabric lamp shade", new(0, 1.54f, 0), new(.38f, .25f, .38f), "Fabric", sides: sides);
                    break;
                case "office_printer":
                    Box(root, "Printer cabinet", new(0, .38f, 0), new(.65f, .76f, .57f), "Plastic");
                    Box(root, "Scanner platen", new(0, .81f, .035f), new(.69f, .10f, .6f), "Metal");
                    Box(root, "Scanner glass", new(0, .865f, .07f), new(.52f, .012f, .39f), "Glass");
                    Box(root, "Paper tray", new(0, .22f, -.30f), new(.55f, .09f, .05f), "Paint");
                    if (lod < 2) Box(root, "Printer touch panel", new(.16f, .87f, -.25f), new(.20f, .035f, .1f), "Glass", new(20, 0, 0));
                    break;
                case "locker_bank":
                    Box(root, "Three locker carcass", new(0, .9f, 0), new(1.08f, 1.8f, .46f), "Metal");
                    for (int i = 0; i < 3; i++)
                    {
                        Box(root, "Painted locker door", new(-.36f + i * .36f, .9f, -.242f), new(.33f, 1.72f, .025f), "Paint");
                        if (lod < 2) Box(root, "Locker vent and latch", new(-.36f + i * .36f, 1.24f, -.262f), new(.14f, .22f, .015f), "Plastic");
                    }
                    break;
                case "air_compressor":
                    Cylinder(root, "Compressed air receiver", new(0, .34f, 0), new(.43f, .81f, .43f), "Paint", new(0, 0, 90), sides);
                    Box(root, "Compressor motor", new(0, .65f, 0), new(.42f, .33f, .34f), "Metal");
                    Bar(root, "Push handle", new(-.32f, .20f, .22f), new(-.32f, .91f, .22f), .04f, "Metal", sides);
                    Wheels(root, .3f, .13f, sides);
                    break;
                case "welding_trolley":
                    Cart(root, .64f, .56f, sides);
                    Cylinder(root, "Shield gas bottle", new(.17f, .77f, .06f), new(.20f, 1.0f, .20f), "Paint", sides: sides);
                    Box(root, "Welder power unit", new(-.10f, .62f, -.05f), new(.38f, .34f, .47f), "Rust");
                    if (lod < 2) Box(root, "Welder front grille", new(-.10f, .63f, -.29f), new(.26f, .19f, .02f), "Plastic");
                    break;
                case "workshop_vacuum":
                    Cylinder(root, "Dust extraction drum", new(0, .4f, 0), new(.43f, .57f, .43f), "Metal", sides: sides);
                    Cylinder(root, "Vacuum motor lid", new(0, .72f, 0), new(.48f, .16f, .48f), "Plastic", sides: sides);
                    Bar(root, "Rigid suction pipe", new(.23f, .08f, -.18f), new(.23f, .87f, -.18f), .052f, "Cable", sides);
                    Wheels(root, .18f, .10f, sides);
                    break;
                case "shoe_cartons":
                    for (int i = 0; i < (lod < 2 ? 6 : 3); i++)
                    {
                        int tier = lod < 2 ? i / 2 : i;
                        float x = lod < 2 ? (i % 2 == 0 ? -.18f : .18f) : 0;
                        Box(root, "Footwear carton", new(x, .10f + tier * .205f, 0), new(lod < 2 ? .33f : .68f, .20f, .39f), "Paper");
                        if (lod < 2) Box(root, "Carton lid seam", new(x, .18f + tier * .205f, -.201f), new(.32f, .018f, .012f), "Paint");
                    }
                    break;
                case "cash_register":
                    Box(root, "Till cash drawer", new(0, .09f, 0), new(.55f, .18f, .38f), "Metal");
                    Box(root, "Till keypad body", new(0, .25f, .05f), new(.37f, .20f, .27f), "Plastic", new(-15, 0, 0));
                    Box(root, "Customer display", new(.18f, .43f, .10f), new(.20f, .10f, .065f), "Glass");
                    break;
                case "clearance_barricade":
                    foreach (float side in new[] { -1f, 1f })
                    {
                        Box(root, "Barricade foot", new(side * .66f, .045f, 0), new(.18f, .09f, .55f), "Rubber");
                        Box(root, "Barricade upright", new(side * .66f, .52f, 0), new(.08f, 1.04f, .07f), "Metal");
                    }
                    Box(root, "Orange recovery barrier", new(0, .81f, 0), new(1.7f, .38f, .06f), "Safety");
                    if (lod < 2) for (int i = 0; i < 6; i++) Box(root, "Hazard band", new(-.70f + i * .28f, .81f, -.034f), new(.1f, .37f, .009f), "Rubber", new(0, 0, -20));
                    break;
                case "extension_reel":
                    Cylinder(root, "Extension cable drum", new(0, .31f, 0), new(.37f, .23f, .37f), "Cable", new(90, 0, 0), sides);
                    foreach (float z in new[] { -.14f, .14f }) Cylinder(root, "Cable drum flange", new(0, .31f, z), new(.43f, .027f, .43f), "Paint", new(90, 0, 0), sides);
                    Box(root, "Reel foot", new(0, .045f, 0), new(.37f, .09f, .36f), "Metal");
                    Bar(root, "Reel handle", new(-.15f, .06f, 0), new(-.15f, .52f, 0), .035f, "Metal", sides);
                    break;
                case "mobile_planning_board":
                    Box(root, "Dusty site planning board", new(0, 1.03f, 0), new(.86f, .78f, .045f), "Paper");
                    foreach (float side in new[] { -1f, 1f })
                    {
                        Box(root, "Board support", new(side * .37f, .63f, 0), new(.04f, 1.26f, .04f), "Metal");
                        Box(root, "Board stabiliser", new(side * .37f, .065f, 0), new(.08f, .08f, .59f), "Metal");
                    }
                    if (lod < 2) for (int i = 0; i < 5; i++) Box(root, "Pinned salvage plan", new(-.27f + i % 3 * .26f, 1.25f - i / 3 * .35f, -.032f), new(.18f, .22f, .01f), i % 2 == 0 ? "Safety" : "Plastic");
                    break;
                case "duct_stack":
                    for (int i = 0; i < 3; i++) Box(root, "Removed ventilation duct", new(0, .105f + i * .18f, i % 2 * .03f), new(1.15f - i * .04f, .17f, .54f), "Metal");
                    if (lod < 2) foreach (float side in new[] { -1f, 1f }) Box(root, "Duct flange", new(side * .50f, .27f, 0), new(.04f, .5f, .59f), "Rust");
                    break;
                case "recovery_cart":
                    Cart(root, .80f, .54f, sides);
                    Box(root, "Packed cart tray", new(0, .73f, 0), new(.75f, .12f, .49f), "Paint");
                    foreach (float x in new[] { -.21f, .21f }) Box(root, "Unsorted cartridge carton", new(x, .88f, 0), new(.33f, .20f, .39f), "Paper");
                    break;
            }
        }

        private static void Wheels(Transform root, float x, float radius, int sides)
        {
            foreach (float side in new[] { -1f, 1f }) Cylinder(root, "Rubber wheel", new(side * x, radius, 0),
                new(radius * 2, .06f, radius * 2), "Rubber", new(0, 0, 90), sides);
        }

        private static void Cart(Transform root, float width, float depth, int sides)
        {
            Box(root, "Trolley lower tray", new(0, .22f, 0), new(width, .08f, depth), "Metal");
            foreach (float side in new[] { -1f, 1f })
            {
                Box(root, "Trolley upright", new(side * (width * .5f - .04f), .6f, depth * .41f), new(.035f, 1.0f, .035f), "Metal");
                foreach (float z in new[] { -depth * .35f, depth * .35f }) Cylinder(root, "Trolley caster", new(side * width * .37f, .08f, z), new(.16f, .045f, .16f), "Rubber", new(0, 0, 90), sides);
            }
            Box(root, "Trolley push bar", new(0, 1.04f, depth * .41f), new(width, .035f, .04f), "Metal");
        }
    }
}
