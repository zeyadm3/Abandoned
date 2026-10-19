using System.Collections.Generic;
using System.IO;
using Abandoned.Core;
using UnityEditor;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;

namespace Abandoned.EditorTools
{
    /// <summary>Fitted construction detail and PBR surfaces for the existing, unlock-compatible wardrobe.</summary>
    public static class WardrobeArtDetails
    {
        private const string GroupName = "CharacterWardrobeDetail";

        public static void UpgradeSaved()
        {
            CharacterMaterialBuilder.Prepare();
            foreach (string path in Directory.GetFiles(CosmeticsBuilder.PrefabFolder, "*.prefab"))
            {
                string name = Path.GetFileNameWithoutExtension(path);
                int split = name.IndexOf('_');
                if (split < 0) continue;
                GameObject root = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    Build(root.transform, name[..split], name[(split + 1)..]);
                    PrefabUtility.SaveAsPrefabAsset(root, path);
                }
                finally { PrefabUtility.UnloadPrefabContents(root); }
            }
        }

        public static void Build(Transform root, string prefix, string id)
        {
            CharacterMaterialBuilder.Prepare();
            Transform old = root.Find(GroupName);
            if (old != null) Object.DestroyImmediate(old.gameObject);
            RemapSurfaces(root, prefix + "_" + id);
            Transform g = Group(GroupName, root);
            Material rubber = CharacterSurfaceLibrary.Get("Rubber"), metal = CharacterSurfaceLibrary.Get("Steel");
            Material tape = CharacterSurfaceLibrary.Get("Tape"), canvas = CharacterSurfaceLibrary.Get("Canvas");
            Material leather = CharacterSurfaceLibrary.Get("Leather");
            if (prefix == "Hat")
            {
                switch (id)
                {
                    case "hardhat": case "goldhat": case "miner":
                        foreach (float s in new[] { -1f, 1f })
                        {
                            for (int i = 0; i < 5; i++)
                                Box("ShellVent", g, new Vector3(s * .151f, -.015f, -.075f + i * .035f), new Vector3(.006f, .012f, .021f), rubber, false);
                            Box("SideReflector", g, new Vector3(s * .158f, -.045f, .055f), new Vector3(.006f, .024f, .09f), tape, false);
                            Stud(g, new Vector3(s * .11f, -.067f, .13f), .006f, metal);
                        }
                        Loop(g, "HarnessAdjuster", new Vector3(0f, -.09f, -.149f), new Vector3(.065f, .028f, .014f), metal);
                        Box("FrontBadgeInset", g, new Vector3(0f, -.002f, .172f), new Vector3(.043f, .023f, .006f), metal, false);
                        if (id == "miner")
                            for (int i = 0; i < 12; i++)
                            {
                                float a = i * Mathf.PI * 2f / 12f;
                                Box("LensCage", g, new Vector3(Mathf.Cos(a) * .033f, -.01f + Mathf.Sin(a) * .033f, .223f), new Vector3(.004f, .004f, .009f), metal, false);
                            }
                        break;
                    case "cap": case "beanie": case "bucket": case "fedora":
                        foreach (float s in new[] { -1f, 1f })
                            for (int i = 0; i < 12; i++)
                            {
                                float a = (.12f + i * .19f) * s;
                                Stud(g, new Vector3(Mathf.Sin(a) * .149f, -.07f, Mathf.Cos(a) * .151f), .0017f, tape);
                            }
                        Box("WovenIssueTab", g, new Vector3(.09f, -.065f, .117f), new Vector3(.024f, .019f, .006f), canvas, false);
                        break;
                    case "welding": case "hazmat":
                        Loop(g, "VisorGasket", new Vector3(0f, -.13f, .181f), new Vector3(id == "hazmat" ? .185f : .148f, id == "hazmat" ? .13f : .06f, .009f), rubber);
                        foreach (float s in new[] { -1f, 1f })
                            Stud(g, new Vector3(s * .107f, -.075f, .18f), .006f, metal);
                        for (int i = 0; i < 7; i++)
                            Box("ChinVent", g, new Vector3((i - 3) * .018f, -.228f, .172f), new Vector3(.008f, .021f, .006f), rubber, false);
                        break;
                    case "nvg":
                        foreach (float s in new[] { -1f, 1f })
                        {
                            Cylinder(g, "FocusRing", new Vector3(s * .035f, -.1f, .245f), .026f, .008f, metal, Vector3.forward);
                            for (int i = 0; i < 6; i++) Box("OpticHousingRib", g, new Vector3(s * .035f, -.078f, .193f + i * .008f), new Vector3(.034f, .006f, .003f), rubber, false);
                        }
                        Box("BridgeHinge", g, new Vector3(0f, -.075f, .19f), new Vector3(.022f, .023f, .018f), metal, false);
                        break;
                    case "box":
                        for (int i = 0; i < 5; i++) Box("CrushedFold", g, new Vector3(-.15f + i * .075f, -.29f, .173f), new Vector3(.014f, .022f, .003f), canvas, false);
                        break;
                    case "cone":
                        foreach (float s in new[] { -1f, 1f })
                            Box("BaseScuff", g, new Vector3(s * .155f, -.049f, .08f), new Vector3(.035f, .004f, .1f), rubber, false);
                        break;
                    case "crown": case "saucepan":
                        for (int i = 0; i < 12; i++)
                        {
                            float a = i * Mathf.PI * 2f / 12f;
                            Stud(g, new Vector3(Mathf.Sin(a) * .153f, -.025f, Mathf.Cos(a) * .153f), .004f, metal);
                        }
                        break;
                }
            }
            else
            {
                switch (id)
                {
                    case "goggles": case "shades": case "gasmask":
                        foreach (float s in new[] { -1f, 1f })
                        {
                            Loop(g, "EyeSeal", new Vector3(s * .052f, -.115f, .147f), new Vector3(.072f, .051f, .009f), rubber);
                            Stud(g, new Vector3(s * .091f, -.111f, .152f), .0025f, metal);
                        }
                        if (id == "gasmask")
                        {
                            for (int i = 0; i < 7; i++) Box("FilterFlute", g, new Vector3((i - 3) * .008f, -.238f, .213f), new Vector3(.003f, .026f, .004f), metal, false);
                            Cylinder(g, "ExhaleValve", new Vector3(.07f, -.19f, .151f), .019f, .006f, rubber, Vector3.forward);
                        }
                        break;
                    case "dustmask":
                        for (int i = 0; i < 4; i++) Box("FilterPleat", g, new Vector3(0f, -.17f - i * .015f, .154f), new Vector3(.088f - i * .006f, .003f, .005f), tape, false);
                        Box("NoseBridge", g, new Vector3(0f, -.15f, .145f), new Vector3(.05f, .005f, .008f), metal, false);
                        break;
                    case "earmuffs":
                        foreach (float s in new[] { -1f, 1f })
                        {
                            Loop(g, "CupSeal", new Vector3(s * .17f, -.12f, 0f), new Vector3(.013f, .085f, .08f), rubber);
                            Stud(g, new Vector3(s * .181f, -.087f, 0f), .007f, metal);
                        }
                        break;
                    case "toolbelt":
                        foreach (float s in new[] { -1f, 1f })
                        {
                            Loop(g, "PouchBinding", new Vector3(s * .14f, -.72f, .192f), new Vector3(.065f, .075f, .006f), leather);
                            Stud(g, new Vector3(s * .14f, -.687f, .199f), .004f, metal);
                        }
                        Loop(g, "Keeper", new Vector3(0f, -.67f, .169f), new Vector3(.047f, .038f, .012f), metal);
                        break;
                    case "rucksack":
                        foreach (float s in new[] { -1f, 1f })
                        {
                            Box("CompressionWebbing", g, new Vector3(s * .112f, -.45f, -.346f), new Vector3(.019f, .29f, .006f), rubber, false);
                            Loop(g, "CompressionBuckle", new Vector3(s * .112f, -.375f, -.353f), new Vector3(.031f, .036f, .011f), metal);
                            for (int i = 0; i < 8; i++) Box("MolleLoop", g, new Vector3(s * .075f, -.48f - i * .017f, -.355f), new Vector3(.052f, .006f, .006f), canvas, false);
                        }
                        Box("TopGrabHandle", g, new Vector3(0f, -.22f, -.24f), new Vector3(.105f, .018f, .025f), rubber, false);
                        break;
                    case "radio":
                        for (int i = 0; i < 6; i++) Box("SpeakerSlot", g, new Vector3(.13f, -.366f - i * .006f, .2f), new Vector3(.035f, .002f, .003f), rubber, false);
                        Box("VolumeControl", g, new Vector3(.116f, -.302f, .175f), new Vector3(.011f, .012f, .014f), metal, false);
                        break;
                    case "bandana":
                        for (int i = 0; i < 9; i++) Box("HemStitch", g, new Vector3((i - 4) * .02f, -.224f, .131f), new Vector3(.007f, .002f, .002f), tape, false);
                        break;
                    case "dogtags":
                        foreach (float s in new[] { -1f, 1f }) Loop(g, "TagLip", new Vector3(s * .014f, -.385f, .173f), new Vector3(.029f, .045f, .003f), metal);
                        break;
                    case "moustache":
                        for (int i = 0; i < 8; i++) Box("HairStrand", g, new Vector3((i - 3.5f) * .012f, -.165f, .142f), new Vector3(.002f, .01f, .003f), rubber, false);
                        break;
                }
            }
            Bake(g, prefix + "_" + id);
        }

        private static void RemapSurfaces(Transform root, string identity)
        {
            foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>())
            {
                Material[] materials = renderer.sharedMaterials;
                for (int i = 0; i < materials.Length; i++)
                {
                    Material old = materials[i];
                    if (old == null || AssetDatabase.GetAssetPath(old).StartsWith(CharacterMaterialBuilder.Folder)) continue;
                    string label = old.name.ToLowerInvariant();
                    string key = label.Contains("lens") || label.Contains("glass") ? "Glass" : label.Contains("metal") || label.Contains("brass") || label.Contains("gold") ? "Steel"
                        : label.Contains("rubber") || label.Contains("black") ? "Rubber" : label.Contains("leather") ? "Leather"
                        : label.Contains("wool") || label.Contains("felt") || label.Contains("canvas") || label.Contains("denim") ? "Canvas" : "Paint";
                    Color colour = old.HasProperty("_BaseColor") ? old.GetColor("_BaseColor") : old.color;
                    if (key == "Glass") colour.a = .4f;
                    string safe = System.Text.RegularExpressions.Regex.Replace(old.name, "[^a-zA-Z0-9_]", "_");
                    Material material = CharacterMaterialBuilder.Tinted(key, colour, identity + "_" + safe);
                    // Palette atlases encode multiple original colours in mesh UV islands. Preserve their colour
                    // map while adding the close-up normal and roughness treatment.
                    if (old.HasProperty("_BaseMap") && old.GetTexture("_BaseMap") != null) material.SetTexture("_BaseMap", old.GetTexture("_BaseMap"));
                    materials[i] = material;
                }
                renderer.sharedMaterials = materials;
            }
        }

        private static void Loop(Transform g, string name, Vector3 at, Vector3 size, Material material)
        {
            float t = Mathf.Min(.006f, Mathf.Min(size.x, size.y) * .15f);
            foreach (float s in new[] { -1f, 1f })
            {
                Box(name, g, at + Vector3.right * s * (size.x - t) * .5f, new Vector3(t, size.y, size.z), material, false);
                Box(name, g, at + Vector3.up * s * (size.y - t) * .5f, new Vector3(size.x, t, size.z), material, false);
            }
        }

        private static void Stud(Transform g, Vector3 at, float radius, Material material) =>
            Primitive(PrimitiveType.Sphere, "Fastener", g, at, Vector3.one * radius * 2f, material, false);

        private static void Cylinder(Transform g, string name, Vector3 at, float radius, float length, Material material, Vector3 axis)
        {
            GameObject part = Primitive(PrimitiveType.Cylinder, name, g, at, new Vector3(radius * 2f, length * .5f, radius * 2f), material, false);
            part.transform.localRotation = Quaternion.FromToRotation(Vector3.up, axis);
        }

        private static void Bake(Transform group, string identity)
        {
            string folder = CharacterMaterialBuilder.Folder + "/Meshes/Wardrobe";
            PolishAssets.EnsureFolder(folder);
            var materials = new Dictionary<Material, List<CombineInstance>>();
            var parts = new List<GameObject>();
            foreach (MeshFilter filter in group.GetComponentsInChildren<MeshFilter>())
            {
                Material material = filter.GetComponent<Renderer>().sharedMaterial;
                if (!materials.TryGetValue(material, out List<CombineInstance> entries)) materials[material] = entries = new();
                entries.Add(new CombineInstance { mesh = filter.sharedMesh, transform = group.worldToLocalMatrix * filter.transform.localToWorldMatrix });
                parts.Add(filter.gameObject);
            }
            foreach (var pair in materials)
            {
                string path = $"{folder}/{identity}_{pair.Key.name}.asset";
                Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
                if (mesh == null) { mesh = new Mesh { name = identity + "_" + pair.Key.name }; AssetDatabase.CreateAsset(mesh, path); }
                else mesh.Clear();
                mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
                mesh.CombineMeshes(pair.Value.ToArray(), true, true);
                mesh.RecalculateTangents();
                mesh.RecalculateBounds();
                EditorUtility.SetDirty(mesh);
                GameObject go = new(pair.Key.name);
                go.transform.SetParent(group, false);
                go.AddComponent<MeshFilter>().sharedMesh = mesh;
                go.AddComponent<MeshRenderer>().sharedMaterial = pair.Key;
            }
            foreach (GameObject part in parts) Object.DestroyImmediate(part);
        }
    }
}
