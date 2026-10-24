using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using K = Abandoned.EditorTools.StationTownBuildings.Kind;
using Plan = Abandoned.EditorTools.StationTownPlan;
using Site = Abandoned.EditorTools.StationSite;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The outer rings' buildings and cars as copies of a few dozen designs. Each design is built once with the town kit
    /// (every window, cornice and picket), merged into one saved mesh with a submesh per material, plus a far mesh of
    /// only its big parts (walls, roof, cornice, awning), so a ring of three hundred buildings is three hundred renderers
    /// with a level of detail each instead of a hundred thousand boxes. Copies keep the design's colliders. Meshes are
    /// saved under <c>{art}/RingMeshes</c> over the same assets every rebuild, so references keep their GUIDs.
    /// </summary>
    public sealed class StationRingStock
    {
        public const string NearName = "RingNear", FarName = "RingFar", DetailName = "RingDetail";

        private sealed class Design
        {
            public string Name;
            public float Width, Depth, FarMin, Near;
            public Action<Transform> Build;
            public bool Made;
            public Mesh NearMesh, FarMesh;
            public Material[] NearMats, FarMats;
            public readonly List<(Vector3 at, Quaternion turn, Vector3 scale, Vector3 centre, Vector3 size)> Solids = new();
            public readonly List<(Mesh mesh, Material[] mats, Matrix4x4 local)> Details = new();
        }

        private readonly StationArt a;
        private readonly List<Design> designs = new();

        /// <summary>Materials swapped as each design is merged (a winter map puts snow on the roofs this way). Fill it
        /// before the first copy is placed.</summary>
        public readonly Dictionary<Material, Material> Remap = new();

        public StationRingStock(StationArt art) => a = art;

        public float Width(int i) => designs[i].Width;
        public float Depth(int i) => designs[i].Depth;

        /// <summary>A design built by <paramref name="build"/> into an empty group at the origin, its front on z 0 facing +z,
        /// centred on x 0, running back to z -<paramref name="depth"/>.</summary>
        /// <param name="farMin">Parts at least this long (or 30 % of the whole) stay in the far mesh.</param>
        /// <param name="near">Screen height under which the copy drops to its far mesh.</param>
        public int Add(string name, float width, float depth, Action<Transform> build, float farMin = 3.5f, float near = .1f)
        {
            designs.Add(new Design { Name = $"{designs.Count:00}_{name}", Width = width, Depth = depth, Build = build, FarMin = farMin, Near = near });
            return designs.Count - 1;
        }

        public int Building(K kind, float width, float depth, int storeys, int seed) =>
            Add($"{kind}{width:0}x{depth:0}_{seed}", width, depth,
                t => StationTownBuildings.Build(t, kind, new Vector3(0f, Site.GroundTop, 0f), 0f, width, depth, storeys, seed, a));

        /// <summary>A clapboard house on a 13 m lot: set back behind a picket fence, a path to the door, a driveway at the side.</summary>
        public int House(int seed) => Add($"House_{seed}", 13f, 14.4f, t =>
        {
            StationTownArt town = StationTownArt.For(a);
            StationTownBuildings.Build(t, K.House, new Vector3(-1.5f, Site.GroundTop, -3.2f), 0f, 10f, 11f, 2, seed, a);
            Plan.Picket(t, a, new Vector3(-6.4f, 0f, -.1f), new Vector3(-2.2f, 0f, -.1f), town.Frame);
            Plan.Picket(t, a, new Vector3(-.8f, 0f, -.1f), new Vector3(3.5f, 0f, -.1f), town.Frame);
            Plan.Paving(t, Rect.MinMaxRect(-2.1f, -3.2f, -.9f, 0f), a.Sidewalk);
            Plan.Paving(t, Rect.MinMaxRect(3.7f, -14f, 6.3f, 0f), a.Concrete);
            StationStreet.Mailbox(t, a, new Vector3(-.3f, Site.GroundTop, .5f), 0f);
        });

        /// <summary>A parked car, nose toward +z.</summary>
        public int Car(StationVehicleKind kind, int seed, StationVehicleWear wear) => Car(kind, seed, wear, null);

        /// <summary>A parked car in <paramref name="paint"/> (a taxi's yellow, a cruiser's black), nose toward +z.</summary>
        public int Car(StationVehicleKind kind, int seed, StationVehicleWear wear, Material paint) =>
            Add($"Car{kind}_{seed}{(paint != null ? "_" + paint.name : "")}", 2f, 5f, t => StationVehicles.Car(t, "Car", new Vector3(0f, Site.GroundTop, 0f), 0f, kind,
                paint != null ? paint : StationVehicleArt.For(a).Paint(seed), a, wear, seed), .5f, .06f);

        /// <summary>A copy of design <paramref name="i"/> with its front's middle at <paramref name="at"/>, facing <paramref name="yaw"/>.</summary>
        public Transform Place(Transform parent, int i, Vector3 at, float yaw)
        {
            Design d = designs[i];
            if (!d.Made) Make(d);
            Transform t = GreyboxFactory.Group(d.Name, parent);
            t.SetPositionAndRotation(new Vector3(at.x, 0f, at.z), Quaternion.Euler(0f, yaw, 0f));
            var near = new List<Renderer>();
            if (d.NearMesh != null) near.Add(Draw(NearName, t, d.NearMesh, d.NearMats, Matrix4x4.identity));
            foreach ((Mesh mesh, Material[] mats, Matrix4x4 local) in d.Details) near.Add(Draw(DetailName, t, mesh, mats, local));
            Renderer far = d.FarMesh != null ? Draw(FarName, t, d.FarMesh, d.FarMats, Matrix4x4.identity) : null;
            var lod = t.gameObject.AddComponent<LODGroup>();
            lod.SetLODs(far != null
                ? new[] { new LOD(d.Near, near.ToArray()), new LOD(.003f, new[] { far }) }
                : new[] { new LOD(.003f, near.ToArray()) });
            lod.RecalculateBounds();
            foreach ((Vector3 p, Quaternion q, Vector3 s, Vector3 c, Vector3 size) in d.Solids)
            {
                Transform solid = GreyboxFactory.Group("Solid", t);
                solid.localPosition = p;
                solid.localRotation = q;
                solid.localScale = s;
                var box = solid.gameObject.AddComponent<BoxCollider>();
                box.center = c;
                box.size = size;
            }
            return t;
        }

        /// <summary>Every renderer under <paramref name="root"/> drawn with a material in <paramref name="map"/> gets its
        /// replacement (the town's streets and lots on a winter map). Returns how many renderers changed.</summary>
        public static int Repaint(Transform root, Dictionary<Material, Material> map)
        {
            int changed = 0;
            foreach (Renderer r in root.GetComponentsInChildren<Renderer>(true))
            {
                Material[] before = r.sharedMaterials, after = Swap(before, map);
                bool differs = false;
                for (int i = 0; i < before.Length; i++) differs |= before[i] != after[i];
                if (!differs) continue;
                r.sharedMaterials = after;
                changed++;
            }
            return changed;
        }

        private static Material[] Swap(Material[] mats, Dictionary<Material, Material> map)
        {
            var result = (Material[])mats.Clone();
            for (int i = 0; i < result.Length; i++)
                if (result[i] != null && map.TryGetValue(result[i], out Material to)) result[i] = to;
            return result;
        }

        private static Renderer Draw(string name, Transform parent, Mesh mesh, Material[] mats, Matrix4x4 local)
        {
            Transform t = GreyboxFactory.Group(name, parent);
            t.localPosition = local.GetPosition();
            t.localRotation = local.rotation;
            t.localScale = local.lossyScale;
            t.gameObject.AddComponent<MeshFilter>().sharedMesh = mesh;
            var r = t.gameObject.AddComponent<MeshRenderer>();
            r.sharedMaterials = mats;
            return r;
        }

        // Build the design at the origin, keep its colliders, merge what it draws, throw the pieces away.
        private void Make(Design d)
        {
            d.Made = true;
            Transform temp = GreyboxFactory.Group("RingDesign_" + d.Name, null);
            try
            {
                d.Build(temp);
                StationSurfaces.Apply(temp, a);
                var lower = new HashSet<Renderer>();
                foreach (LODGroup g in temp.GetComponentsInChildren<LODGroup>(true))
                {
                    LOD[] lods = g.GetLODs();
                    for (int l = 1; l < lods.Length; l++)
                        foreach (Renderer r in lods[l].renderers)
                            if (r != null) lower.Add(r);
                }
                var drawn = new List<MeshRenderer>();
                float size = 0f;
                foreach (MeshRenderer r in temp.GetComponentsInChildren<MeshRenderer>(true))
                {
                    if (!r.enabled || lower.Contains(r) || r.shadowCastingMode == ShadowCastingMode.ShadowsOnly) continue;
                    var f = r.GetComponent<MeshFilter>();
                    if (f == null || f.sharedMesh == null || r.sharedMaterials.Length == 0) continue;
                    drawn.Add(r);
                    Vector3 e = r.bounds.size;
                    size = Mathf.Max(size, Mathf.Max(e.x, Mathf.Max(e.y, e.z)));
                }
                var near = new Dictionary<Material, List<CombineInstance>>();
                var far = new Dictionary<Material, List<CombineInstance>>();
                var order = new List<Material>();
                foreach (MeshRenderer r in drawn)
                {
                    MeshFilter f = r.GetComponent<MeshFilter>();
                    Mesh mesh = f.sharedMesh;
                    Matrix4x4 m = f.transform.localToWorldMatrix;
                    // Meshes the editor can't read (imported scans) and mirrored pieces stand as their own renderers.
                    if (!mesh.isReadable || m.determinant < 0f)
                    {
                        d.Details.Add((mesh, Swap(r.sharedMaterials, Remap), m));
                        continue;
                    }
                    Vector3 e = r.bounds.size;
                    float longest = Mathf.Max(e.x, Mathf.Max(e.y, e.z));
                    bool big = longest >= d.FarMin || longest >= size * .3f;
                    Material[] mats = r.sharedMaterials;
                    for (int sub = 0; sub < mesh.subMeshCount; sub++)
                    {
                        Material mat = mats[Mathf.Min(sub, mats.Length - 1)];
                        if (mat == null) continue;
                        if (Remap.TryGetValue(mat, out Material swapped)) mat = swapped;
                        if (!near.ContainsKey(mat)) { near[mat] = new List<CombineInstance>(); far[mat] = new List<CombineInstance>(); order.Add(mat); }
                        var piece = new CombineInstance { mesh = mesh, subMeshIndex = sub, transform = m };
                        near[mat].Add(piece);
                        if (big) far[mat].Add(piece);
                    }
                }
                foreach (BoxCollider box in temp.GetComponentsInChildren<BoxCollider>(true))
                    if (box.enabled && !box.isTrigger)
                        d.Solids.Add((box.transform.position, box.transform.rotation, box.transform.lossyScale, box.center, box.size));
                foreach (CapsuleCollider cap in temp.GetComponentsInChildren<CapsuleCollider>(true))
                {
                    if (!cap.enabled || cap.isTrigger) continue;
                    Vector3 size3 = Vector3.one * cap.radius * 2f;
                    size3[cap.direction] = Mathf.Max(cap.height, cap.radius * 2f);
                    d.Solids.Add((cap.transform.position, cap.transform.rotation, cap.transform.lossyScale, cap.center, size3));
                }
                d.NearMesh = Save(near, order, $"{a.Prefix}Ring{d.Name}_Near", out d.NearMats);
                d.FarMesh = Save(far, order, $"{a.Prefix}Ring{d.Name}_Far", out d.FarMats);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(temp.gameObject);
            }
        }

        // One mesh per material, then those as the submeshes of one saved mesh.
        private Mesh Save(Dictionary<Material, List<CombineInstance>> parts, List<Material> order, string name, out Material[] mats)
        {
            var subs = new List<CombineInstance>();
            var used = new List<Material>();
            foreach (Material m in order)
            {
                if (!parts.TryGetValue(m, out List<CombineInstance> list) || list.Count == 0) continue;
                var part = new Mesh { indexFormat = IndexFormat.UInt32 };
                part.CombineMeshes(list.ToArray(), true, true);
                subs.Add(new CombineInstance { mesh = part, transform = Matrix4x4.identity });
                used.Add(m);
            }
            mats = used.ToArray();
            if (subs.Count == 0) return null;
            var merged = new Mesh { name = name, indexFormat = IndexFormat.UInt32 };
            merged.CombineMeshes(subs.ToArray(), false, true);
            foreach (CombineInstance s in subs) UnityEngine.Object.DestroyImmediate(s.mesh);
            merged.RecalculateBounds();
            return Keep(merged, name);
        }

        private Mesh Keep(Mesh mesh, string name)
        {
            string folder = a.Folder + "/RingMeshes";
            PolishAssets.EnsureFolder(folder);
            string path = $"{folder}/{name}.asset";
            var saved = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (saved == null)
            {
                AssetDatabase.CreateAsset(mesh, path);
                return mesh;
            }
            EditorUtility.CopySerialized(mesh, saved);
            saved.name = name;
            UnityEngine.Object.DestroyImmediate(mesh);
            EditorUtility.SetDirty(saved);
            return saved;
        }
    }
}
