using System.Collections.Generic;
using Abandoned.Core;
using Abandoned.Structure;
using UnityEditor;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>Authors dense stock against the saved mall's reserved routes and existing fittings.</summary>
    public sealed class SalvageDensityPlacement
    {
        private readonly Transform mall, root;
        private readonly MallRouteClearance routes;
        private readonly List<Bounds> fittings = new();
        public int Props { get; private set; }
        public int Details { get; private set; }
        public Transform Root => root;

        public SalvageDensityPlacement(Transform mallRoot, Transform densityRoot)
        {
            mall = mallRoot;
            root = densityRoot;
            Physics.SyncTransforms();
            routes = new MallRouteClearance(mall);
            foreach (Collider collider in mall.GetComponentsInChildren<Collider>(true))
                if (collider.enabled && !collider.isTrigger &&
                    collider.GetComponentInParent<StructuralSection>() == null)
                    fittings.Add(collider.bounds);
        }

        public Transform Zone(string name) => GreyboxFactory.Group(name, root);

        public GameObject Prop(string id, Transform parent, int floor, float x, float z,
            float yaw = 0, float lift = 0, bool surfaceStock = false)
            => Model(UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(SalvageAssetBuilder.DressingPath(id)),
                parent, floor, x, z, yaw, lift, surfaceStock);

        public GameObject Model(GameObject prefab, Transform parent, int floor, float x, float z,
            float yaw = 0, float lift = 0, bool surfaceStock = false)
        {
            if (prefab == null || !HasFloor(floor, x, z)) return null;
            GameObject item = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            item.transform.SetLocalPositionAndRotation(new Vector3(x, MallLayout.FloorY(floor) + lift, z),
                Quaternion.Euler(0, yaw, 0));
            PrefabUtility.UnpackPrefabInstance(item, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            Bounds bounds = ModelFit.RendererBounds(item);
            if (!Available(bounds, surfaceStock))
            {
                Object.DestroyImmediate(item);
                return null;
            }
            // 0.24.9: a loot model never stands as scenery; fitted on the original's bounds, then swapped.
            string lookalike = LootLookalikes.IdOf(prefab);
            if (lookalike != null) item = LootLookalikeStandIns.Swap(item, lookalike, surfaceStock);
            if (surfaceStock) VisualOnly(item, "Packed stock rests inside its supporting display or pallet collision.");
            else fittings.Add(bounds);
            Bind(item, floor, x, z);
            Props++;
            return item;
        }

        public GameObject Box(Transform parent, string name, int floor, Vector3 center,
            Vector3 size, string finish, Vector3 euler = default, bool solid = true)
        {
            if (!HasFloor(floor, center.x, center.z)) return null;
            center.y += MallLayout.FloorY(floor);
            GameObject item = SalvageModelFactory.Box(parent, name, center, size, finish, euler);
            bool physical = solid && size.y >= .3f && Mathf.Min(size.x, size.z) >= .08f;
            Bounds bounds = ModelFit.RendererBounds(item);
            if (physical && !Available(bounds, false))
            {
                Object.DestroyImmediate(item);
                return null;
            }
            if (physical)
            {
                item.AddComponent<BoxCollider>();
                item.AddComponent<SurfaceTag>().EditorSet(finish is "Wood" or "Paper" ? SurfaceMaterial.Wood
                    : finish is "Metal" or "Rust" ? SurfaceMaterial.Metal : SurfaceMaterial.Concrete);
                fittings.Add(bounds);
            }
            else VisualOnly(item, "Thin packing remnants, dust and shelf contents are nonblocking dressing.");
            Bind(item, floor, center.x, center.z);
            Details++;
            return item;
        }

        public void Scatter(Transform parent, int floor, Rect area, int count, int seed)
        {
            var random = new System.Random(seed);
            string[] finishes = { "Paper", "Concrete", "Wood", "Plastic", "Fabric" };
            for (int i = 0; i < count; i++)
            {
                float x = Mathf.Lerp(area.xMin, area.xMax, (float)random.NextDouble());
                float z = Mathf.Lerp(area.yMin, area.yMax, (float)random.NextDouble());
                float width = Mathf.Lerp(.12f, .58f, (float)random.NextDouble());
                float depth = Mathf.Lerp(.08f, .35f, (float)random.NextDouble());
                Box(parent, "PackingAndDrywall_" + i, floor, new Vector3(x, .009f + i % 3 * .002f, z),
                    new Vector3(width, .008f, depth), finishes[i % finishes.Length],
                    new Vector3(0, (float)random.NextDouble() * 360, 0), false);
            }
        }

        private bool HasFloor(int floor, float x, float z)
        {
            var cell = new Vector2Int(Mathf.FloorToInt(x / MallLayout.Tile), Mathf.FloorToInt(z / MallLayout.Tile));
            return !MallLayout.IsVoid(cell, floor) && MeridianDressingUtility.Support(mall, cell, floor) != null;
        }

        private bool Available(Bounds bounds, bool surfaceStock)
        {
            // These are placement constraints, not a later audit: stock never enters reserved carry/spawn pockets.
            if (routes.Conflict(bounds) != null) return false;
            if (surfaceStock) return true;
            Bounds inner = bounds;
            inner.Expand(-.035f);
            foreach (Bounds fitting in fittings)
                if (inner.Intersects(fitting)) return false;
            return true;
        }

        private static void VisualOnly(GameObject item, string reason)
        {
            foreach (Collider collider in item.GetComponentsInChildren<Collider>(true)) Object.DestroyImmediate(collider);
            var marker = Abandoned.EditorTools.SerializedWiring.GetOrAdd<WalkThroughObject>(item);
            marker.EditorSetup(reason);
        }

        private void Bind(GameObject item, int floor, float x, float z)
        {
            var cell = new Vector2Int(Mathf.FloorToInt(x / MallLayout.Tile), Mathf.FloorToInt(z / MallLayout.Tile));
            StructuralSection support = MeridianDressingUtility.Support(mall, cell, floor);
            (Abandoned.EditorTools.SerializedWiring.GetOrAdd<SectionProp>(item)).EditorSetup(support);
            foreach (Renderer renderer in item.GetComponentsInChildren<Renderer>(true))
                GameObjectUtility.SetStaticEditorFlags(renderer.gameObject, 0);
        }
    }
}
