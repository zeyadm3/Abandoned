using System.Collections.Generic;
using Abandoned.Core;
using Abandoned.World;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The world's roads as meshes laid on their graded beds: asphalt with a dropped edge (no gap shows against the
    /// shoulder), a dashed centre line, a collider per 200 m stretch so the van drives on the road, not the terrain.
    /// Also each place's stand-in: level ground over its site while its scene isn't loaded.
    /// </summary>
    public static class WorldRoads
    {
        public const string Folder = "Assets/_Project/Art/Generated/World/Roads";
        private const int ChunkSamples = 100;
        private const float Lift = .1f, Skirt = .45f, DashOn = 3f, DashPeriod = 9f, LineWidth = .16f;

        public static void Build(Transform parent, List<List<Vector3>> roads)
        {
            PolishAssets.EnsureFolder(Folder);
            foreach (string old in AssetDatabase.FindAssets("t:Mesh", new[] { Folder })) AssetDatabase.DeleteAsset(AssetDatabase.GUIDToAssetPath(old));
            Material asphalt = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Polish/HQ_District_Asphalt.mat");
            Material paint = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Polish/HQ_District_RoadPaint.mat");
            Transform group = GreyboxFactory.Group("Roads", parent);
            for (int r = 0; r < roads.Count; r++)
            {
                List<Vector3> line = roads[r];
                for (int start = 0, chunk = 0; start < line.Count - 1; start += ChunkSamples, chunk++)
                {
                    int end = Mathf.Min(line.Count - 1, start + ChunkSamples);
                    string name = $"Road_{r}_{chunk}";
                    Mesh surface = Save(Strip(line, start, end, out Vector3 centre), name);
                    var go = new GameObject(name);
                    go.transform.SetParent(group, false);
                    go.transform.localPosition = centre;
                    go.AddComponent<MeshFilter>().sharedMesh = surface;
                    var renderer = go.AddComponent<MeshRenderer>();
                    renderer.sharedMaterial = asphalt;
                    renderer.shadowCastingMode = ShadowCastingMode.Off;
                    go.AddComponent<MeshCollider>().sharedMesh = surface;
                    go.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Asphalt);

                    Mesh dashes = Save(Dashes(line, start, end, centre), name + "_Line");
                    var lineGo = new GameObject(name + "_Line");
                    lineGo.transform.SetParent(go.transform, false);
                    lineGo.AddComponent<MeshFilter>().sharedMesh = dashes;
                    var lineRenderer = lineGo.AddComponent<MeshRenderer>();
                    lineRenderer.sharedMaterial = paint;
                    lineRenderer.shadowCastingMode = ShadowCastingMode.Off;
                }
            }
        }

        private static Mesh Save(Mesh mesh, string name)
        {
            mesh.name = name;
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            mesh.RecalculateBounds();
            AssetDatabase.CreateAsset(mesh, $"{Folder}/{name}.asset");
            return mesh;
        }

        private static Vector3 Side(List<Vector3> line, int i)
        {
            Vector3 a = line[Mathf.Max(0, i - 1)], b = line[Mathf.Min(line.Count - 1, i + 1)];
            Vector3 along = b - a;
            along.y = 0f;
            return Vector3.Cross(Vector3.up, along.normalized);
        }

        // Asphalt across the width (four quads: dropped edge, surface, surface, dropped edge), UVs in metres / 4.
        private static Mesh Strip(List<Vector3> line, int start, int end, out Vector3 centre)
        {
            centre = line[(start + end) / 2];
            var vertices = new List<Vector3>();
            var uv = new List<Vector2>();
            var triangles = new List<int>();
            float w = WorldPlan.RoadHalfWidth, along = 0f;
            float[] across = { -w - .2f, -w, 0f, w, w + .2f };
            float[] drop = { -Skirt, 0f, 0f, 0f, -Skirt };
            for (int i = start; i <= end; i++)
            {
                if (i > start) along += Vector3.Distance(line[i - 1], line[i]);
                Vector3 side = Side(line, i);
                for (int k = 0; k < across.Length; k++)
                {
                    vertices.Add(line[i] + side * across[k] + Vector3.up * (Lift + drop[k]) - centre);
                    uv.Add(new Vector2((across[k] + w) / 4f, along / 4f));
                }
                if (i == start) continue;
                int row = vertices.Count - across.Length, prev = row - across.Length;
                for (int k = 0; k < across.Length - 1; k++)
                    triangles.AddRange(new[] { prev + k, row + k, prev + k + 1, prev + k + 1, row + k, row + k + 1 });
            }
            var mesh = new Mesh();
            mesh.SetVertices(vertices);
            mesh.SetUVs(0, uv);
            mesh.SetTriangles(triangles, 0);
            return mesh;
        }

        private static Mesh Dashes(List<Vector3> line, int start, int end, Vector3 centre)
        {
            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            float along = 0f;
            for (int i = start + 1; i <= end; i++)
            {
                float before = along;
                along += Vector3.Distance(line[i - 1], line[i]);
                // A dash covers this step if the step starts inside the painted part of the period.
                if (Mathf.Repeat(before, DashPeriod) > DashOn) continue;
                Vector3 a = line[i - 1], b = line[i], sa = Side(line, i - 1) * LineWidth * .5f, sb = Side(line, i) * LineWidth * .5f;
                Vector3 up = Vector3.up * (Lift + .01f);
                int v = vertices.Count;
                vertices.AddRange(new[] { a - sa + up - centre, a + sa + up - centre, b - sb + up - centre, b + sb + up - centre });
                triangles.AddRange(new[] { v, v + 2, v + 1, v + 1, v + 2, v + 3 });
            }
            var mesh = new Mesh();
            mesh.SetVertices(vertices);
            mesh.SetUVs(0, new List<Vector2>(new Vector2[vertices.Count]));
            mesh.SetTriangles(triangles, 0);
            return mesh;
        }

        /// <summary>Per place, level ground over its site (shown while its scene isn't loaded); returns them in atlas order.</summary>
        public static GameObject[] StandIns(Transform parent, IReadOnlyList<WorldPoi> places)
        {
            Transform group = GreyboxFactory.Group("SiteStandIns", parent);
            Material home = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Polish/HQ_District_Asphalt.mat");
            Material lot = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Greybox/Location_Asphalt.mat");
            var sites = new GameObject[places.Count];
            for (int i = 0; i < places.Count; i++)
            {
                WorldPoi p = places[i];
                Rect r = WorldPlan.GroundRect(p);
                GameObject site = GreyboxFactory.Box($"Site_{p.scene}", group, new Vector3(r.center.x, WorldPlan.GroundY(p) - 1f, r.center.y),
                    new Vector3(r.width, 2f, r.height), i == 0 ? home : lot);
                site.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;
                site.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Asphalt);
                sites[i] = site;
            }
            return sites;
        }
    }
}
