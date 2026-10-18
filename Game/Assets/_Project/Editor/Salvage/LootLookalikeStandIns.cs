using System;
using System.Collections.Generic;
using Abandoned.Core;
using Abandoned.Structure;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// What takes a static loot look-alike's place: a fixed fixture for store fittings, a dust sheet over
    /// anything else standing on the floor, crushed empty packaging for stock on shelves and in displays.
    /// The original's colliders move over unchanged, so collision and the baked NavMesh stay exactly as they were.
    /// </summary>
    public static class LootLookalikeStandIns
    {
        public const string Folder = SalvageAssetBuilder.ArtFolder + "/Lookalikes0249";
        private const string DustReason = "Packaging left where stock was cleared; walk-through like other shelf stock.";

        private static readonly Dictionary<string, Bounds> modelBounds = new();
        private static readonly List<Bounds> spots = new();

        /// <summary>World bounds of every swap since the last call (for the F1 report), then clears them.</summary>
        public static Bounds[] TakeSpots()
        {
            Bounds[] taken = spots.ToArray();
            spots.Clear();
            return taken;
        }

        /// <summary>
        /// Replaces <paramref name="original"/> (a scenery copy of loot <paramref name="id"/>) and returns the
        /// stand-in. <paramref name="contents"/> marks stock resting on or in something else: it becomes packaging.
        /// <paramref name="footprint"/> is the original's world bounds when it has no renderers left (batched).
        /// </summary>
        public static GameObject Swap(GameObject original, string id, bool contents, Bounds? footprint = null)
        {
            Transform parent = original.transform.parent;
            Bounds world = footprint ?? ModelFit.RendererBounds(original);
            string fixture = LootLookalikes.FixtureFor(id);
            GameObject standIn;
            if (fixture != null)
            {
                standIn = Fixture(fixture, parent);
                standIn.transform.SetLocalPositionAndRotation(original.transform.localPosition, original.transform.localRotation);
            }
            else
            {
                Bounds local = ModelBounds(id);
                standIn = contents ? Packaging(parent, id, local) : Sheet(parent, id, local);
                // Upright, sitting where the original's lowest point was: a tipped chair's sheet lies on the floor.
                standIn.transform.rotation = Quaternion.Euler(0, original.transform.eulerAngles.y, 0);
                standIn.transform.position = original.transform.position;
                Bounds placed = ModelFit.RendererBounds(standIn);
                standIn.transform.position += new Vector3(world.center.x - placed.center.x,
                    world.min.y - placed.min.y, world.center.z - placed.center.z);
            }
            standIn.transform.localScale = Vector3.one;
            standIn.transform.SetSiblingIndex(original.transform.GetSiblingIndex());

            if (!contents || fixture != null) CopyCollision(original, standIn);
            if (!HasCollision(standIn))
            {
                var marker = standIn.GetComponent<WalkThroughObject>() ?? standIn.AddComponent<WalkThroughObject>();
                marker.EditorSetup(DustReason);
            }
            var binding = original.GetComponent<SectionProp>();
            if (binding != null && binding.Section != null)
                (standIn.GetComponent<SectionProp>() ?? standIn.AddComponent<SectionProp>()).EditorSetup(binding.Section);
            foreach (Renderer renderer in standIn.GetComponentsInChildren<Renderer>(true))
                GameObjectUtility.SetStaticEditorFlags(renderer.gameObject, 0);

            spots.Add(ModelFit.RendererBounds(standIn));
            Object.DestroyImmediate(original);
            return standIn;
        }

        public static bool HasCollision(GameObject item)
        {
            foreach (Collider collider in item.GetComponentsInChildren<Collider>(true))
                if (collider.enabled && !collider.isTrigger) return true;
            return false;
        }

        private static GameObject Fixture(string fixture, Transform parent)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(SalvageAssetBuilder.DressingPath(fixture));
            if (prefab == null)
                throw new InvalidOperationException($"Fixture '{fixture}' is not authored yet; run Tools > Abandoned > Mall > Replace Loot Look-alikes (0.24.9).");
            var item = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            // Unpacked like the density layer's own props: a prefab instance's colliders can't be swapped out.
            PrefabUtility.UnpackPrefabInstance(item, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            item.name = fixture;
            // The original's own colliders come over instead, so the NavMesh stays valid without a rebake.
            foreach (Collider collider in item.GetComponentsInChildren<Collider>(true)) Object.DestroyImmediate(collider);
            Transform proxies = item.transform.Find("_CollisionProxies");
            if (proxies != null) Object.DestroyImmediate(proxies.gameObject);
            return item;
        }

        private static GameObject Sheet(Transform parent, string id, Bounds local)
        {
            var root = new GameObject(id + "_DustSheeted");
            root.transform.SetParent(parent, false);
            var draw = new GameObject("DustSheet", typeof(MeshFilter), typeof(MeshRenderer));
            draw.transform.SetParent(root.transform, false);
            draw.transform.localPosition = new Vector3(local.center.x, local.min.y, local.center.z);
            draw.GetComponent<MeshFilter>().sharedMesh = DustSheetMesh.For(local.size, Folder);
            draw.GetComponent<MeshRenderer>().sharedMaterial = SalvageMaterials.Get("Fabric");
            root.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Fabric);
            return root;
        }

        private static GameObject Packaging(Transform parent, string id, Bounds local)
        {
            var root = new GameObject(id + "_EmptyPackaging");
            root.transform.SetParent(parent, false);
            float width = Mathf.Clamp(local.size.x, .18f, .60f), depth = Mathf.Clamp(local.size.z, .14f, .45f);
            Vector3 c = new Vector3(local.center.x, local.min.y, local.center.z);
            SalvageModelFactory.Box(root.transform, "Flattened carton", c + new Vector3(0, .012f, 0),
                new Vector3(width, .024f, depth), "Paper", new Vector3(0, 7, 0));
            SalvageModelFactory.Box(root.transform, "Torn carton flap", c + new Vector3(width * .18f, .03f, -depth * .1f),
                new Vector3(width * .55f, .012f, depth * .6f), "Paper", new Vector3(0, -24, 9));
            SalvageModelFactory.Box(root.transform, "Loose shrink-wrap", c + new Vector3(-width * .2f, .02f, depth * .15f),
                new Vector3(width * .4f, .01f, depth * .35f), "Wrap", new Vector3(0, 38, 0));
            return root;
        }

        private static void CopyCollision(GameObject original, GameObject standIn)
        {
            Transform holder = null;
            foreach (Collider source in original.GetComponentsInChildren<Collider>(true))
            {
                if (!source.enabled || source.isTrigger) continue;
                if (holder == null)
                {
                    holder = new GameObject("_CollisionProxies").transform;
                    holder.SetParent(standIn.transform, false);
                }
                // Never the prop's own name: a collider called "showroom_sofa" would read as the prop in a search.
                var part = new GameObject(ProxyName(source.name)).transform;
                part.SetParent(holder, false);
                part.SetPositionAndRotation(source.transform.position, source.transform.rotation);
                part.localScale = Vector3.Scale(source.transform.lossyScale, Inverse(holder.lossyScale));
                part.gameObject.layer = source.gameObject.layer;
                var copy = (Collider)part.gameObject.AddComponent(source.GetType());
                EditorUtility.CopySerialized(source, copy);
            }
        }

        public static string ProxyName(string source) => LootLookalikes.Is(source) ? "Collision_" + source : source;

        private static Vector3 Inverse(Vector3 scale) => new(1f / scale.x, 1f / scale.y, 1f / scale.z);

        /// <summary>The loot model's upright bounds in its own space, measured once per id.</summary>
        private static Bounds ModelBounds(string id)
        {
            if (modelBounds.TryGetValue(id, out Bounds known)) return known;
            GameObject source = LootLookalikes.Source(id);
            if (source == null) throw new InvalidOperationException($"The scenery prefab for '{id}' is missing.");
            var probe = (GameObject)PrefabUtility.InstantiatePrefab(source);
            try { return modelBounds[id] = ModelFit.LocalBounds(probe, probe.transform); }
            finally { Object.DestroyImmediate(probe); }
        }
    }
}
