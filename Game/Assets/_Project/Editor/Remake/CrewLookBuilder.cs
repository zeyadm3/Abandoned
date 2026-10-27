using System.Collections.Generic;
using System.IO;
using Abandoned.Player;
using UnityEditor;
using UnityEngine;

namespace Abandoned.EditorTools.Remake
{
    /// <summary>
    /// The 2026 remake's characters and outfits (Tools/Blender/remake/crew): reads Head_&lt;id&gt; / Outfit_&lt;id&gt; FBX and
    /// JSON from Art/Remake/Characters, re-skins every mesh onto the crew skeleton in WorkerRig's joint order with bind
    /// poses for the rest pose (feet at the origin, joints unrotated), saves the meshes as assets, and writes the
    /// Character_ / Outfit_ cosmetic definitions and their catalog lists (append-only: existing order is kept).
    /// </summary>
    public static class CrewLookBuilder
    {
        public const string MeshFolder = RemakePaths.Characters + "/Meshes";

        /// <summary>Bone order on every look mesh, with each joint's rest position in feet space (WorkerVisualBuilder).</summary>
        public static readonly string[] Bones = { "Body", "Torso", "Head", "ArmL", "ElbowL", "ArmR", "ElbowR", "LegL", "KneeL", "LegR", "KneeR" };
        public static readonly Vector3[] Rest =
        {
            new(0f, .95f, 0f), new(0f, 1.10f, 0f), new(0f, 1.52f, 0f), new(-.19f, 1.415f, 0f), new(-.19f, 1.11f, 0f),
            new(.19f, 1.415f, 0f), new(.19f, 1.11f, 0f), new(-.102f, .90f, 0f), new(-.102f, .50f, 0f), new(.102f, .90f, 0f), new(.102f, .50f, 0f),
        };

        [System.Serializable]
        private class HeadMeta { public string id, name, blurb, style; public int build; public float crown, scale; }

        [System.Serializable]
        private class OutfitMeta { public string id, name, blurb; public int price; public string[] tint; }

        public static bool Available => File.Exists(RemakePaths.Characters + "/Head_doyle.fbx");

        [MenuItem(ToolMenu.Characters + "Build Crew Looks (remake)", priority = ToolMenu.CharactersP + 1)]
        public static void Build()
        {
            if (!Available)
            {
                Debug.LogWarning("[CrewLook] no remake characters yet (run Tools/Blender/remake/crew/characters.py and outfit_build.py).");
                return;
            }
            AssetDatabase.ImportAsset(RemakePaths.Characters, ImportAssetOptions.ImportRecursive);
            PolishAssets.EnsureFolder(MeshFolder);
            var characters = new List<CosmeticDefinition>();
            var outfits = new List<CosmeticDefinition>();
            foreach (string json in Directory.GetFiles(RemakePaths.Characters, "Head_*.json"))
            {
                var meta = JsonUtility.FromJson<HeadMeta>(File.ReadAllText(json));
                CosmeticDefinition d = Character(meta);
                if (d != null) characters.Add(d);
            }
            foreach (string json in Directory.GetFiles(RemakePaths.Characters, "Outfit_*.json"))
            {
                var meta = JsonUtility.FromJson<OutfitMeta>(File.ReadAllText(json));
                CosmeticDefinition d = Outfit(meta);
                if (d != null) outfits.Add(d);
            }
            var catalog = AssetDatabase.LoadAssetAtPath<CosmeticCatalog>(CosmeticsBuilder.CatalogPath);
            if (catalog == null)
            {
                Debug.LogError("[CrewLook] no cosmetic catalog: run Build Cosmetics first.");
                return;
            }
            catalog.EditorSetLooks(Ordered(catalog.Characters, characters, "doyle"), Ordered(catalog.Outfits, outfits, "salvage"));
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
            Debug.Log($"[CrewLook] {characters.Count} characters, {outfits.Count} outfits.");
        }

        /// <summary>Keeps the existing network order and appends new ids; a fresh list starts with `first`.</summary>
        private static List<CosmeticDefinition> Ordered(IReadOnlyList<CosmeticDefinition> existing, List<CosmeticDefinition> built, string first)
        {
            var byId = new Dictionary<string, CosmeticDefinition>();
            foreach (CosmeticDefinition d in built) byId[d.Id] = d;
            var list = new List<CosmeticDefinition>();
            foreach (CosmeticDefinition d in existing)
                if (d != null && byId.TryGetValue(d.Id, out CosmeticDefinition fresh)) { list.Add(fresh); byId.Remove(d.Id); }
            if (list.Count == 0 && byId.TryGetValue(first, out CosmeticDefinition head)) { list.Add(head); byId.Remove(first); }
            var rest = new List<string>(byId.Keys);
            rest.Sort(System.StringComparer.Ordinal);
            foreach (string id in rest) list.Add(byId[id]);
            return list;
        }

        private static CosmeticDefinition Character(HeadMeta meta)
        {
            GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>($"{RemakePaths.Characters}/Head_{meta.id}.fbx");
            if (model == null) return null;
            Mesh mesh = Combine(model, $"Head_{meta.id}", out Material[] materials);
            CosmeticDefinition d = Definition($"Character_{meta.id}", meta.id, meta.name, CosmeticKind.Character, meta.blurb, 0);
            // Hats were fitted to the neutral head (scale 1, crown 1.778); smaller heads get smaller hats.
            d.EditorSetLook(mesh, null, materials, -1, meta.build, Mathf.Clamp(meta.scale, .85f, 1.1f));
            EditorUtility.SetDirty(d);
            return d;
        }

        private static CosmeticDefinition Outfit(OutfitMeta meta)
        {
            GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>($"{RemakePaths.Characters}/Outfit_{meta.id}.fbx");
            if (model == null) return null;
            var meshes = new Mesh[3];
            Material[] materials = null;
            for (int b = 0; b < 3; b++)
                meshes[b] = Combine(model, $"Outfit_{meta.id}_B{b}", out materials, $"Outfit_{meta.id}_B{b}");
            int tint = -1;
            if (materials != null)
                for (int i = 0; i < materials.Length; i++)
                    if (materials[i] != null && materials[i].name.EndsWith("_Tint")) tint = i;
            CosmeticDefinition d = Definition($"Outfit_{meta.id}", meta.id, meta.name, CosmeticKind.Outfit, meta.blurb, meta.price);
            d.EditorSetLook(null, meshes, materials, tint, 0, 1f);
            EditorUtility.SetDirty(d);
            return d;
        }

        private static CosmeticDefinition Definition(string file, string id, string name, CosmeticKind kind, string about, int price)
        {
            string path = $"{CosmeticsBuilder.DataFolder}/{file}.asset";
            var d = AssetDatabase.LoadAssetAtPath<CosmeticDefinition>(path);
            if (d == null)
            {
                d = ScriptableObject.CreateInstance<CosmeticDefinition>();
                AssetDatabase.CreateAsset(d, path);
            }
            d.EditorSetup(id, name, kind, about, Color.white, null, price, "");
            return d;
        }

        /// <summary>
        /// One mesh in feet space from the model's skinned renderers (all of them, or only `only`), submeshes in material
        /// order, bone weights remapped to <see cref="Bones"/>, bind poses for the rest pose. Saved as an asset.
        /// </summary>
        private static Mesh Combine(GameObject model, string assetName, out Material[] materials, string only = null)
        {
            GameObject copy = Object.Instantiate(model);
            copy.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            copy.transform.localScale = Vector3.one;
            var verts = new List<Vector3>();
            var normals = new List<Vector3>();
            var tangents = new List<Vector4>();
            var uvs = new List<Vector2>();
            var weights = new List<BoneWeight>();
            var subs = new List<List<int>>();
            var mats = new List<Material>();
            try
            {
                foreach (SkinnedMeshRenderer smr in copy.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                {
                    if (only != null && smr.name != only) continue;
                    Mesh src = smr.sharedMesh;
                    Matrix4x4 m = smr.transform.localToWorldMatrix;
                    int[] remap = new int[smr.bones.Length];
                    for (int i = 0; i < remap.Length; i++)
                        remap[i] = Mathf.Max(0, System.Array.IndexOf(Bones, smr.bones[i] != null ? smr.bones[i].name : ""));
                    int start = verts.Count;
                    Vector3[] v = src.vertices, n = src.normals;
                    Vector4[] t = src.tangents;
                    Vector2[] uv = src.uv;
                    BoneWeight[] bw = src.boneWeights;
                    for (int i = 0; i < v.Length; i++)
                    {
                        verts.Add(m.MultiplyPoint3x4(v[i]));
                        normals.Add(i < n.Length ? m.MultiplyVector(n[i]).normalized : Vector3.up);
                        if (i < t.Length)
                        {
                            Vector3 tt = m.MultiplyVector(t[i]).normalized;
                            tangents.Add(new Vector4(tt.x, tt.y, tt.z, t[i].w));
                        }
                        else tangents.Add(new Vector4(1, 0, 0, 1));
                        uvs.Add(i < uv.Length ? uv[i] : Vector2.zero);
                        weights.Add(i < bw.Length ? Remap(bw[i], remap) : new BoneWeight { boneIndex0 = 2, weight0 = 1f });
                    }
                    Material[] shared = smr.sharedMaterials;
                    for (int s = 0; s < src.subMeshCount; s++)
                    {
                        Material mat = s < shared.Length ? shared[s] : null;
                        int slot = mats.IndexOf(mat);
                        if (slot < 0) { mats.Add(mat); subs.Add(new List<int>()); slot = mats.Count - 1; }
                        foreach (int index in src.GetTriangles(s)) subs[slot].Add(index + start);
                    }
                }
            }
            finally { Object.DestroyImmediate(copy); }

            var mesh = new Mesh { name = assetName, indexFormat = verts.Count > 65000 ? UnityEngine.Rendering.IndexFormat.UInt32 : UnityEngine.Rendering.IndexFormat.UInt16 };
            mesh.SetVertices(verts);
            mesh.SetNormals(normals);
            mesh.SetTangents(tangents);
            mesh.SetUVs(0, uvs);
            mesh.boneWeights = weights.ToArray();
            mesh.subMeshCount = subs.Count;
            for (int s = 0; s < subs.Count; s++) mesh.SetTriangles(subs[s], s);
            var bind = new Matrix4x4[Bones.Length];
            for (int i = 0; i < Bones.Length; i++) bind[i] = Matrix4x4.Translate(-Rest[i]);
            mesh.bindposes = bind;
            mesh.RecalculateBounds();
            materials = mats.ToArray();
            for (int i = 0; i < materials.Length; i++)
                if (materials[i] != null && materials[i].name.EndsWith("_Tint"))
                    materials[i] = RemakeMaterials.GetOrCreate(materials[i].name, RemakePaths.Characters);
            string path = $"{MeshFolder}/{assetName}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (existing != null)
            {
                EditorUtility.CopySerialized(mesh, existing);
                Object.DestroyImmediate(mesh);
                mesh = existing;
            }
            else AssetDatabase.CreateAsset(mesh, path);
            Debug.Log($"[CrewLook] {assetName}: {verts.Count} verts, bounds {mesh.bounds.min:F2}..{mesh.bounds.max:F2}");
            return mesh;
        }

        private static BoneWeight Remap(BoneWeight w, int[] map)
        {
            int Map(int i) => i >= 0 && i < map.Length ? map[i] : 0;
            return new BoneWeight
            {
                boneIndex0 = Map(w.boneIndex0), weight0 = w.weight0,
                boneIndex1 = Map(w.boneIndex1), weight1 = w.weight1,
                boneIndex2 = Map(w.boneIndex2), weight2 = w.weight2,
                boneIndex3 = Map(w.boneIndex3), weight3 = w.weight3,
            };
        }

        /// <summary>
        /// Adds the skinned look under `parent` (feet at `feet` in its local space), bound to `bones` in <see cref="Bones"/>
        /// order. Every bone must sit at its rest position, unrotated, when this is called. Wears the catalog's defaults.
        /// </summary>
        public static WorkerLook AddLook(GameObject host, Transform parent, Vector3 feet, Transform[] bones)
        {
            var catalog = AssetDatabase.LoadAssetAtPath<CosmeticCatalog>(CosmeticsBuilder.CatalogPath);
            CosmeticDefinition person = catalog != null ? catalog.Character(0) : null;
            CosmeticDefinition clothes = catalog != null ? catalog.Outfit(0) : null;
            SkinnedMeshRenderer head = Renderer("LookHead", parent, feet, bones);
            SkinnedMeshRenderer outfit = Renderer("LookOutfit", parent, feet, bones);
            var look = host.GetComponent<WorkerLook>() ?? host.AddComponent<WorkerLook>();
            look.EditorSetup(head, outfit);
            if (person != null)
            {
                head.sharedMesh = person.LookMesh;
                head.sharedMaterials = person.LookMaterials;
            }
            if (clothes != null)
            {
                outfit.sharedMesh = clothes.BuildMesh(person != null ? person.Build : 0);
                outfit.sharedMaterials = clothes.LookMaterials;
            }
            return look;
        }

        private static SkinnedMeshRenderer Renderer(string name, Transform parent, Vector3 feet, Transform[] bones)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = feet;
            var smr = go.AddComponent<SkinnedMeshRenderer>();
            smr.bones = bones;
            smr.rootBone = bones[0];
            smr.updateWhenOffscreen = false;
            smr.localBounds = new Bounds(new Vector3(0f, -.05f, 0f), new Vector3(1.4f, 2.2f, 1.4f));
            smr.quality = SkinQuality.Bone4;
            return smr;
        }
    }
}
