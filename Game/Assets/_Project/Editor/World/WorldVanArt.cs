using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The open-world van's look: the collection truck's cab, chassis and box body (the truck every job is loaded into),
    /// rebuilt from the same code into this van's own meshes, with the wheels cut out so they can turn and steer.
    /// The truck's material assets are reused as they are, never rewritten.
    /// </summary>
    public static class WorldVanArt
    {
        public const string Folder = "Assets/_Project/Art/Generated/WorldVan";
        private const string TruckMaterials = CollectionTruckGeometry.Folder + "/Materials";

        // Front left, front right, rear left, rear right (the rear hubs sit between each pair of twin tyres).
        public static readonly Vector3[] Hubs =
        {
            new(-1.16f, .48f, 3.75f), new(1.16f, .48f, 3.75f), new(-1.215f, .48f, -1.65f), new(1.215f, .48f, -1.65f)
        };

        private static readonly string[] Finishes = { "Paint", "Steel", "Dark", "Chrome", "Tire", "Seat", "Deck", "CargoLining", "Amber", "Red", "Lamp", "Glass" };
        private static readonly HashSet<string> WheelParts = new()
        {
            "ContinuousCommercialTyre", "RecessedPressedSteelWheel", "AxleHubCap", "HexWheelLug", "RecessedCoolingVent"
        };

        /// <summary>Builds the art under <paramref name="van"/> and returns the four wheel pivots (at the hubs).</summary>
        public static Transform[] Build(Transform van)
        {
            UseTruckMaterials();
            PolishAssets.EnsureFolder(Folder + "/Meshes");
            Transform art = GreyboxFactory.Group("Art", van);
            int[] details = { 0, 2 };
            var lods = new LOD[details.Length];
            for (int i = 0; i < details.Length; i++)
            {
                Transform source = Source(art, details[i], out _);
                foreach (MeshFilter part in source.GetComponentsInChildren<MeshFilter>())
                    if (WheelParts.Contains(part.name)) Object.DestroyImmediate(part.gameObject);
                Renderer[] body = Combine(source, art, $"WorldVanBody_LOD{i}");
                Object.DestroyImmediate(source.gameObject);
                lods[i] = new LOD(i == 0 ? .08f : .006f, body);
            }
            var group = art.gameObject.AddComponent<LODGroup>();
            group.SetLODs(lods);
            group.RecalculateBounds();
            return Wheels(van);
        }

        // One middle-detail pass of the chassis, cut into four wheels pivoting on their hubs.
        private static Transform[] Wheels(Transform van)
        {
            var pivots = new Transform[Hubs.Length];
            var parts = new List<MeshFilter>[Hubs.Length];
            for (int i = 0; i < Hubs.Length; i++)
            {
                pivots[i] = GreyboxFactory.Group(i switch { 0 => "Wheel_FL", 1 => "Wheel_FR", 2 => "Wheel_RL", _ => "Wheel_RR" }, van);
                pivots[i].localPosition = Hubs[i];
                parts[i] = new List<MeshFilter>();
            }
            Transform source = GreyboxFactory.Group("WheelSource", van);
            CollectionTruckChassisRedo.Build(new CollectionTruckGeometry(source, 1));
            foreach (MeshFilter part in source.GetComponentsInChildren<MeshFilter>())
            {
                if (!WheelParts.Contains(part.name)) continue;
                Vector3 centre = source.InverseTransformPoint(part.transform.TransformPoint(part.sharedMesh.bounds.center));
                int nearest = -1;
                float best = .6f;
                for (int i = 0; i < Hubs.Length; i++)
                {
                    float d = Vector3.Distance(centre, Hubs[i]);
                    if (d < best) { best = d; nearest = i; }
                }
                if (nearest >= 0) parts[nearest].Add(part);
            }
            for (int i = 0; i < Hubs.Length; i++) Combine(parts[i], pivots[i], $"WorldVanWheel_{pivots[i].name}");
            Object.DestroyImmediate(source.gameObject);
            return pivots;
        }

        private static Transform Source(Transform art, int detail, out CollectionTruckGeometry geometry)
        {
            Transform source = GreyboxFactory.Group("Source", art);
            geometry = new CollectionTruckGeometry(source, detail);
            CollectionTruckCabRedo.Build(geometry);
            CollectionTruckChassisRedo.Build(geometry);
            Transform box = GreyboxFactory.Group("CargoSource", source);
            box.localPosition = Vector3.up * CollectionTruckFitRedo.DeckLift;
            CollectionTruckCargoRedo.Build(new CollectionTruckGeometry(box, detail));
            return source;
        }

        private static Renderer[] Combine(Transform source, Transform target, string name)
        {
            var filters = new List<MeshFilter>(source.GetComponentsInChildren<MeshFilter>());
            return Combine(filters, target, name);
        }

        // One mesh per material, saved in this van's own folder, drawn in the target's space.
        private static Renderer[] Combine(List<MeshFilter> filters, Transform target, string name)
        {
            var groups = new Dictionary<Material, List<CombineInstance>>();
            foreach (MeshFilter filter in filters)
            {
                Material material = filter.GetComponent<MeshRenderer>().sharedMaterial;
                if (!groups.TryGetValue(material, out List<CombineInstance> list)) groups[material] = list = new List<CombineInstance>();
                list.Add(new CombineInstance { mesh = filter.sharedMesh, transform = target.worldToLocalMatrix * filter.transform.localToWorldMatrix });
            }
            var renderers = new List<Renderer>();
            foreach (var pair in groups)
            {
                var mesh = new Mesh { name = $"{name}_{pair.Key.name}", indexFormat = IndexFormat.UInt32 };
                mesh.CombineMeshes(pair.Value.ToArray(), true, true);
                mesh.RecalculateBounds();
                string path = $"{Folder}/Meshes/{mesh.name}.asset";
                Mesh saved = AssetDatabase.LoadAssetAtPath<Mesh>(path);
                if (saved == null) { AssetDatabase.CreateAsset(mesh, path); saved = mesh; }
                else { EditorUtility.CopySerialized(mesh, saved); Object.DestroyImmediate(mesh); EditorUtility.SetDirty(saved); }
                Transform part = GreyboxFactory.Group(pair.Key.name, target);
                part.gameObject.AddComponent<MeshFilter>().sharedMesh = saved;
                var renderer = part.gameObject.AddComponent<MeshRenderer>();
                renderer.sharedMaterial = pair.Key;
                if (pair.Key.renderQueue >= (int)RenderQueue.Transparent) renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderers.Add(renderer);
            }
            return renderers.ToArray();
        }

        // CollectionTruckMaterials.Prepare() resets the truck's materials to their first authored values, which would
        // undo later art passes on the mall's truck; the van only reads the assets as they are now.
        private static void UseTruckMaterials()
        {
            FieldInfo field = typeof(CollectionTruckMaterials).GetField("materials", BindingFlags.NonPublic | BindingFlags.Static);
            var table = (Dictionary<string, Material>)field.GetValue(null);
            table.Clear();
            foreach (string finish in Finishes)
            {
                Material m = AssetDatabase.LoadAssetAtPath<Material>($"{TruckMaterials}/Truck_{finish}.mat");
                if (m == null) Debug.LogWarning($"[WorldVan] Missing truck material {finish}.");
                else table[finish] = m;
            }
        }

        /// <summary>The truck's rear barn doors, reusing its saved door meshes; returns the left and right hinges.</summary>
        public static (Transform left, Transform right) Doors(Transform van)
        {
            Transform Door(float side)
            {
                Transform hinge = GreyboxFactory.Group(side < 0 ? "RearDoorHinge_L" : "RearDoorHinge_R", van);
                hinge.localPosition = new Vector3(side * 1.35f, .40f + CollectionTruckFitRedo.DeckLift, -2.55f);
                string prefix = side < 0 ? "CargoRedoDoorLeft_LOD0_Truck_" : "CargoRedoDoorRight_LOD0_Truck_";
                foreach (string finish in new[] { "CargoLining", "Chrome", "Dark", "Paint", "Steel" })
                {
                    Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>($"{CollectionTruckGeometry.Folder}/Meshes/{prefix}{finish}.asset");
                    Material material = AssetDatabase.LoadAssetAtPath<Material>($"{TruckMaterials}/Truck_{finish}.mat");
                    if (mesh == null || material == null) continue;
                    Transform part = GreyboxFactory.Group(finish, hinge);
                    part.gameObject.AddComponent<MeshFilter>().sharedMesh = mesh;
                    part.gameObject.AddComponent<MeshRenderer>().sharedMaterial = material;
                }
                return hinge;
            }
            return (Door(-1f), Door(1f));
        }
    }
}
