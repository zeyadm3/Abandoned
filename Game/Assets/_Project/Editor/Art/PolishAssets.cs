using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Abandoned.EditorTools
{
    /// <summary>Original, code-authored low-poly art. Assets keep their GUIDs across content rebuilds.</summary>
    public static class PolishAssets
    {
        public const string Folder = "Assets/_Project/Art/Polish";

        public static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = System.IO.Path.GetDirectoryName(path).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, System.IO.Path.GetFileName(path));
        }

        public static Material Material(string name, Color color, float roughness = 0.8f, Texture2D texture = null, float emission = 0f)
        {
            EnsureFolder(Folder);
            string path = $"{Folder}/{name}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                AssetDatabase.CreateAsset(material, path);
            }
            material.name = name;
            material.SetColor("_BaseColor", color);
            material.SetFloat("_Smoothness", 1f - roughness);
            material.SetTexture("_BaseMap", texture);
            if (emission > 0f)
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * emission);
            }
            else
            {
                material.DisableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", Color.black);
            }
            EditorUtility.SetDirty(material);
            return material;
        }

        public static Material Stain()
        {
            var material = Material("WaterStain", new Color(0.29f, 0.25f, 0.17f, 0.45f), texture: Texture("Stain", stain: true));
            material.SetFloat("_Surface", 1f);
            material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            material.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            material.SetFloat("_ZWrite", 0f);
            material.SetFloat("_Cull", (float)CullMode.Off);
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.SetOverrideTag("RenderType", "Transparent");
            material.renderQueue = (int)RenderQueue.Transparent;
            return material;
        }

        // Broad patches and a few quiet grout lines; no high-frequency retro/noise overlay.
        public static Texture2D Texture(string name, bool tiles = false, bool stain = false)
        {
            EnsureFolder(Folder);
            string path = $"{Folder}/{name}.asset";
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (texture != null) return texture;
            const int n = 128;
            texture = new Texture2D(n, n, TextureFormat.RGBA32, true) { name = name, wrapMode = TextureWrapMode.Repeat, filterMode = FilterMode.Bilinear };
            var pixels = new Color[n * n];
            for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                float u = x / (float)(n - 1), v = y / (float)(n - 1);
                float patch = Mathf.PerlinNoise(u * 4f + 9f, v * 4f + 11f);
                float shade = Mathf.Lerp(0.83f, 1f, patch);
                if (tiles && (x % 32 < 1 || y % 32 < 1)) shade *= 0.72f;
                float alpha = 1f;
                if (stain)
                {
                    float edge = Mathf.Clamp01((0.49f - Vector2.Distance(new Vector2(u, v), new Vector2(0.5f, 0.5f))) * 7f);
                    alpha = edge * Mathf.Clamp01((patch - 0.25f) * 2.5f);
                    shade = 1f;
                }
                pixels[y * n + x] = new Color(shade, shade, shade, alpha);
            }
            texture.SetPixels(pixels);
            texture.Apply(true, false);
            AssetDatabase.CreateAsset(texture, path);
            return texture;
        }

        public static Mesh Prism()
        {
            // Eight-sided bevelled prism with a narrower waist, useful for clothed limbs and torsos.
            return MeshAsset("Prism", () => Lathe(new[] { (0f, 0.40f), (0.06f, 0.5f), (0.9f, 0.5f), (1f, 0.40f) }, 8));
        }

        public static Mesh Ellipsoid() => MeshAsset("FacetedRound", () =>
        {
            var rings = new (float height, float radius)[7];
            for (int i = 0; i < rings.Length; i++)
            {
                float a = Mathf.PI * i / (rings.Length - 1);
                rings[i] = ((1f - Mathf.Cos(a)) * 0.5f, Mathf.Sin(a) * 0.5f);
            }
            return Lathe(rings, 10);
        });

        private static Mesh MeshAsset(string name, System.Func<Mesh> make)
        {
            EnsureFolder(Folder);
            string path = $"{Folder}/{name}.asset";
            var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (mesh != null) return mesh;
            mesh = make();
            mesh.name = name;
            AssetDatabase.CreateAsset(mesh, path);
            return mesh;
        }

        private static Mesh Lathe((float height, float radius)[] rings, int sides)
        {
            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            var uv = new List<Vector2>();
            void Triangle(Vector3 a, Vector3 b, Vector3 c)
            {
                int first = vertices.Count;
                vertices.AddRange(new[] { a, b, c });
                uv.AddRange(new[] { new Vector2(a.x + 0.5f, a.y + 0.5f), new Vector2(b.x + 0.5f, b.y + 0.5f), new Vector2(c.x + 0.5f, c.y + 0.5f) });
                triangles.AddRange(new[] { first, first + 1, first + 2 });
            }
            Vector3 At(int ring, int side)
            {
                float angle = side * Mathf.PI * 2f / sides + Mathf.PI / sides;
                return new Vector3(Mathf.Cos(angle) * rings[ring].radius, rings[ring].height - 0.5f, Mathf.Sin(angle) * rings[ring].radius);
            }
            for (int r = 0; r < rings.Length - 1; r++)
            for (int s = 0; s < sides; s++)
            {
                Vector3 a = At(r, s), b = At(r, s + 1), c = At(r + 1, s + 1), d = At(r + 1, s);
                Triangle(a, d, c); Triangle(a, c, b);
            }
            for (int s = 0; s < sides; s++)
            {
                Triangle(new Vector3(0f, rings[0].height - 0.5f, 0f), At(0, s), At(0, s + 1));
                int last = rings.Length - 1;
                Triangle(new Vector3(0f, rings[last].height - 0.5f, 0f), At(last, s + 1), At(last, s));
            }
            var mesh = new Mesh();
            mesh.SetVertices(vertices); mesh.SetTriangles(triangles, 0); mesh.SetUVs(0, uv);
            mesh.RecalculateNormals(); mesh.RecalculateBounds();
            return mesh;
        }

        public static GameObject Shape(string name, Transform parent, Vector3 at, Vector3 size, Material material, bool round = false)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = at;
            go.transform.localScale = size;
            go.AddComponent<MeshFilter>().sharedMesh = round ? Ellipsoid() : Prism();
            go.AddComponent<MeshRenderer>().sharedMaterial = material;
            return go;
        }
    }
}
