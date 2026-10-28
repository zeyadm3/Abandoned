using System.Collections.Generic;
using Abandoned.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Grass on a map's own ground, the TestMap's scanned grass card (Art/TestMap/Nature/grass_card.png) on crossed
    /// quads: the ground is sampled from above on a grid, and tufts grow only outdoors (open sky above) on ground whose
    /// material is grass or soil (MaterialRemaster's classification), a few weeds on gravel, in drifts rather than an
    /// even carpet. A GrassField draws them; nothing collides with them and nothing about the map's floors changes.
    /// </summary>
    public static class MapGrass
    {
        private const string Folder = "Assets/_Project/Art/MapSite";
        private const float Step = .85f, ChunkSize = 16f;

        public static (GrassField field, List<Vector3> spots) Grow(Scene scene, Bounds level, Transform parent)
        {
            int mask = ~LayerMask.GetMask(GameLayers.Player, GameLayers.Loot, GameLayers.Debris, "Ignore Raycast");
            var chunks = new Dictionary<Vector2Int, List<Matrix4x4>>();
            var spots = new List<Vector3>();
            var random = new System.Random(scene.name.GetHashCode() ^ 0x5a5a);
            float top = level.max.y + 60f;
            var kinds = new Dictionary<Material, float>();
            for (float z = level.min.z; z < level.max.z; z += Step)
            for (float x = level.min.x; x < level.max.x; x += Step)
            {
                float px = x + (float)random.NextDouble() * Step, pz = z + (float)random.NextDouble() * Step;
                if (!Physics.Raycast(new Vector3(px, top, pz), Vector3.down, out RaycastHit hit, top - level.min.y + 5f, mask, QueryTriggerInteraction.Ignore)) continue;
                if (hit.normal.y < .8f || hit.collider is TerrainCollider) continue;
                float density = Density(hit.collider, kinds);
                if (density <= 0f) continue;
                // Drifts: long grass in patches, cropped between them.
                float drift = Mathf.PerlinNoise(px * .06f + 13f, pz * .06f - 4f);
                int count = (int)(density * Mathf.Lerp(-.6f, 3.4f, drift) + (float)random.NextDouble());
                if (count <= 0) continue;
                // Outdoors only: under a roof or a canopy, no grass.
                if (Physics.Raycast(hit.point + Vector3.up * .3f, Vector3.up, 40f, mask, QueryTriggerInteraction.Ignore)) continue;
                for (int i = 0; i < count; i++)
                {
                    var p = new Vector3(px + (float)(random.NextDouble() - .5) * Step, hit.point.y - .02f, pz + (float)(random.NextDouble() - .5) * Step);
                    float w = Mathf.Lerp(.7f, 1.3f, (float)random.NextDouble()), h = Mathf.Lerp(.6f, 1.25f, (float)random.NextDouble()) * Mathf.Lerp(.7f, 1.1f, drift);
                    var key = new Vector2Int(Mathf.FloorToInt(p.x / ChunkSize), Mathf.FloorToInt(p.z / ChunkSize));
                    if (!chunks.TryGetValue(key, out List<Matrix4x4> list)) chunks[key] = list = new List<Matrix4x4>();
                    list.Add(Matrix4x4.TRS(p, Quaternion.Euler(0f, (float)random.NextDouble() * 360f, 0f), new Vector3(w, h, w)));
                }
                if (drift > .6f && random.NextDouble() < .002) spots.Add(hit.point);
            }

            var placed = new List<GrassField.Chunk>();
            foreach (var pair in chunks)
                for (int start = 0; start < pair.Value.Count; start += GrassField.MaxPerChunk)
                {
                    var tufts = pair.Value.GetRange(start, Mathf.Min(GrassField.MaxPerChunk, pair.Value.Count - start)).ToArray();
                    var b = new Bounds(tufts[0].GetPosition(), Vector3.one);
                    foreach (Matrix4x4 m in tufts) b.Encapsulate(m.GetPosition());
                    b.Expand(new Vector3(1.5f, 2f, 1.5f));
                    placed.Add(new GrassField.Chunk { bounds = b, tufts = tufts });
                }
            if (placed.Count == 0) return (null, spots);
            var go = new GameObject("Grass");
            go.transform.SetParent(parent, false);
            var field = go.AddComponent<GrassField>();
            field.EditorSetup(Tuft(), Material(), 70f, placed);
            return (field, spots);
        }

        // How thickly grass grows on what the ray hit: by the surface's material, as the art remaster classifies it.
        private static float Density(Collider collider, Dictionary<Material, float> cache)
        {
            // GetComponentInParent looks at the collider's own object first (a "GetComponent ?? ..." never falls
            // through in the editor: a missing component is Unity's fake null).
            Renderer renderer = collider.GetComponentInParent<Renderer>();
            Material material = renderer != null ? renderer.sharedMaterial : null;
            if (material == null) return 0f;
            if (cache.TryGetValue(material, out float d)) return d;
            d = MaterialRemaster.Classify(material.name) switch
            {
                MaterialRemaster.SurfaceKind.Grass => 1f,
                MaterialRemaster.SurfaceKind.Soil => .45f,
                MaterialRemaster.SurfaceKind.Gravel => .1f,
                _ => 0f,
            };
            cache[material] = d;
            return d;
        }

        // Three crossed quads, a metre wide and 0.6 m tall, normals up so a tuft is lit like the ground it grows from.
        private static Mesh Tuft()
        {
            string path = $"{Folder}/GrassTuft.asset";
            var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (mesh != null) return mesh;
            PolishAssets.EnsureFolder(Folder);
            var vertices = new List<Vector3>();
            var uvs = new List<Vector2>();
            var triangles = new List<int>();
            for (int k = 0; k < 3; k++)
            {
                float a = k * Mathf.PI / 3f;
                var dir = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * .5f;
                int b = vertices.Count;
                vertices.AddRange(new[] { -dir, dir, dir + Vector3.up * .6f, -dir + Vector3.up * .6f });
                uvs.AddRange(new[] { new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(0f, 1f) });
                triangles.AddRange(new[] { b, b + 2, b + 1, b, b + 3, b + 2 });
            }
            mesh = new Mesh { name = "GrassTuft" };
            mesh.SetVertices(vertices);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(triangles, 0);
            var normals = new Vector3[vertices.Count];
            for (int i = 0; i < normals.Length; i++) normals[i] = Vector3.up;
            mesh.normals = normals;
            mesh.RecalculateBounds();
            AssetDatabase.CreateAsset(mesh, path);
            return mesh;
        }

        private static Material Material()
        {
            string path = $"{Folder}/MapSite_Grass.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                PolishAssets.EnsureFolder(Folder);
                material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                AssetDatabase.CreateAsset(material, path);
            }
            Texture2D card = ShowcaseNature.Texture($"{ShowcaseNature.Prefabs}/grass_card", srgb: true, normal: false, cutout: true);
            ShowcaseNature.Setup(material, card, null, null, cutout: true);
            material.SetColor("_BaseColor", new Color(.85f, .95f, .78f));
            material.SetFloat("_Smoothness", .1f);
            material.SetFloat("_SpecularHighlights", 0f);
            material.EnableKeyword("_SPECULARHIGHLIGHTS_OFF");
            EditorUtility.SetDirty(material);
            return material;
        }
    }
}
