using System;
using System.Collections.Generic;
using System.Linq;
using Abandoned.Core;
using Abandoned.Structure;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Abandoned.EditorTools
{
    /// <summary>Wall/floor junctions fitted from actual geometry, plus physically readable architectural glazing.</summary>
    public static class EnvironmentRemaster
    {
        public const string RootName = "_ArtRemaster_Environment";
        private const string Folder = "Assets/_Project/Art/Remaster/Environment";
        private const float SampleStep = .75f;
        private static readonly string[] WallWords = { "wall", "plaster", "brick", "concrete", "blockwork", "limestone" };

        public readonly struct Result
        {
            public readonly int GlazingMaterials, WallFaces, Junctions, Meshes, Triangles;
            public Result(int glazing, int faces, int junctions, int meshes, int triangles)
            { GlazingMaterials = glazing; WallFaces = faces; Junctions = junctions; Meshes = meshes; Triangles = triangles; }
            public override string ToString() => $"{GlazingMaterials} architectural glazing materials; {WallFaces} wall faces, " +
                $"{Junctions} fitted wall/floor junctions in {Meshes} chunks ({Triangles} triangles)";
        }

        public static void ResetSession() => EnvironmentRemasterGlass.ResetSession();

        public static Result ApplyToScene(Scene scene)
        {
            foreach (GameObject root in scene.GetRootGameObjects()) if (root.name == RootName) Object.DestroyImmediate(root);
            PolishAssets.EnsureFolder(Folder);
            int glass = EnvironmentRemasterGlass.ApplyToScene(scene);
            var go = new GameObject(RootName);
            SceneManager.MoveGameObjectToScene(go, scene);
            Physics.SyncTransforms();
            Renderer[] renderers = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Renderer>(true)).ToArray();
            Bounds[] skirting = renderers.Where(IsSkirting).Select(r => r.bounds).ToArray();
            var meshes = new Dictionary<(int, int, int, bool), EnvironmentRemasterMesh>();
            var keys = new HashSet<string>();
            int faces = 0, junctions = 0;
            int mask = ~LayerMask.GetMask(GameLayers.Player, GameLayers.Loot, GameLayers.Debris, "Ignore Raycast", "UI");
            foreach (Renderer wall in renderers.Where(Candidate).OrderBy(r => Hierarchy(r.transform), StringComparer.Ordinal))
            {
                if (faces >= 420) break;
                Bounds bounds = wall.bounds;
                bool narrowX = bounds.size.x < bounds.size.z;
                Vector3 axis = narrowX ? Vector3.forward : Vector3.right;
                Vector3 normal = narrowX ? Vector3.right : Vector3.forward;
                float length = narrowX ? bounds.size.z : bounds.size.x, depth = narrowX ? bounds.extents.x : bounds.extents.z;
                foreach (float sign in new[] { -1f, 1f })
                {
                    Vector3 outward = normal * sign;
                    Vector3 centre = bounds.center + outward * depth;
                    centre.y = bounds.min.y;
                    string key = Quantize(centre) + ":" + Quantize(outward) + ":" + Mathf.RoundToInt(length * 100f);
                    if (!keys.Add(key)) continue;
                    int samples = Mathf.Clamp(Mathf.CeilToInt(length / SampleStep), 1, 50);
                    float step = length / samples;
                    int begin = -1;
                    float floorY = 0f;
                    bool any = false;
                    for (int i = 0; i <= samples; i++)
                    {
                        Vector3 p = centre + axis * (-length * .5f + step * (i + .5f));
                        float y = floorY;
                        bool valid = i < samples && Receiver(wall, p, outward, mask, out y);
                        float sampleFloor = valid ? y : floorY;
                        if (valid && begin < 0) { begin = i; floorY = sampleFloor; }
                        if (begin >= 0 && (!valid || Mathf.Abs(sampleFloor - floorY) > .05f || i == samples))
                        {
                            Vector3 start = centre + axis * (-length * .5f + begin * step + .015f);
                            Vector3 end = centre + axis * (-length * .5f + i * step - .015f);
                            start.y = end.y = floorY;
                            if (Vector3.Distance(start, end) > .3f)
                            {
                                bool hasSkirting = skirting.Any(b => b.SqrDistance((start + end) * .5f + Vector3.up * .07f) < .012f);
                                Add(meshes, start, end, outward, hasSkirting, scene.name);
                                junctions++;
                                any = true;
                            }
                            begin = valid ? i : -1;
                            floorY = sampleFloor;
                        }
                    }
                    if (any) faces++;
                }
            }
            int triangles = SaveMeshes(go.transform, scene.name, meshes);
            return new Result(glass, faces, junctions, meshes.Count, triangles);
        }

        private static bool Receiver(Renderer wall, Vector3 p, Vector3 outward, int mask, out float y)
        {
            y = 0f;
            Vector3 check = p + outward * .12f + Vector3.up * .5f;
            // Sampling along the wall leaves a real doorway untrimmed, including holes in a combined wall mesh.
            if (!Physics.Raycast(check, -outward, out RaycastHit face, .32f, mask, QueryTriggerInteraction.Ignore) ||
                Mathf.Abs(Vector3.Dot(face.point - p, outward)) > .08f) return false;
            if (!Physics.Raycast(check, Vector3.down, out RaycastHit floor, 1.3f, mask, QueryTriggerInteraction.Ignore) || floor.normal.y < .8f) return false;
            // No decorative baseboards outdoors, on fences, or attached to a long wall above its actual floor.
            if (Mathf.Abs(floor.point.y - wall.bounds.min.y) > .35f ||
                !Physics.Raycast(floor.point + outward * .12f + Vector3.up * .3f, Vector3.up, 12f, mask, QueryTriggerInteraction.Ignore)) return false;
            y = floor.point.y;
            return true;
        }

        private static void Add(Dictionary<(int, int, int, bool), EnvironmentRemasterMesh> meshes, Vector3 start, Vector3 end,
            Vector3 outward, bool existingSkirting, string scene)
        {
            Vector3 mid = (start + end) * .5f;
            var cell = (Mathf.FloorToInt(mid.x / 24f), Mathf.FloorToInt(mid.z / 24f), Mathf.FloorToInt((mid.y + .2f) / 4f));
            EnvironmentRemasterMesh Get(bool trim)
            {
                var key = (cell.Item1, cell.Item2, cell.Item3, trim);
                if (!meshes.TryGetValue(key, out EnvironmentRemasterMesh mesh)) meshes[key] = mesh = new EnvironmentRemasterMesh();
                return mesh;
            }
            Get(false).Ribbon(start, end, outward, .004f, .014f, .004f);
            if (!existingSkirting) Get(true).Skirting(start, end, outward, scene == "Factory" ? .16f : .125f);
        }

        private static int SaveMeshes(Transform parent, string scene, Dictionary<(int, int, int, bool), EnvironmentRemasterMesh> meshes)
        {
            string folder = $"{Folder}/{scene}";
            PolishAssets.EnsureFolder(folder);
            Material trim = Material(scene, false), contact = Material(scene, true);
            int triangles = 0;
            foreach (var entry in meshes.OrderBy(e => e.Key.Item1).ThenBy(e => e.Key.Item2).ThenBy(e => e.Key.Item3).ThenBy(e => e.Key.Item4))
            {
                string name = $"Junction_{entry.Key.Item1}_{entry.Key.Item2}_{entry.Key.Item3}_{(entry.Key.Item4 ? "Trim" : "Contact")}";
                var child = new GameObject(name);
                child.transform.SetParent(parent, false);
                child.AddComponent<MeshFilter>().sharedMesh = entry.Value.Save($"{folder}/{name}.asset", name);
                var renderer = child.AddComponent<MeshRenderer>();
                renderer.sharedMaterial = entry.Key.Item4 ? trim : contact;
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.receiveShadows = true;
                renderer.lightProbeUsage = LightProbeUsage.BlendProbes;
                renderer.reflectionProbeUsage = ReflectionProbeUsage.BlendProbes;
                renderer.renderingLayerMask = LevelStoreys.AllLayers;
                GameObjectUtility.SetStaticEditorFlags(child, StaticEditorFlags.BatchingStatic | StaticEditorFlags.OccludeeStatic);
                triangles += entry.Value.Triangles;
            }
            return triangles;
        }

        private static Material Material(string scene, bool contact)
        {
            string path = $"{Folder}/{scene}_{(contact ? "ContactSeal" : "Skirting")}.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = $"Remaster_{scene}_{(contact ? "ContactSeal" : "Skirting")}" };
                AssetDatabase.CreateAsset(material, path);
            }
            string source = scene switch
            {
                "Tutorial" => "Assets/_Project/Art/Polish/House_Trim.mat",
                "Factory" => "Assets/_Project/Art/Factory/Materials/IronFactory_Steel.mat",
                "Prison" => "Assets/_Project/Art/Prison/Materials/Island_Dado.mat",
                "School" => "Assets/_Project/Art/Polish/Burley_MetalSteel.mat",
                _ => "Assets/_Project/Art/Polish/Mall_DoorTrim.mat"
            };
            Material original = AssetDatabase.LoadAssetAtPath<Material>(source);
            if (!contact && original != null) { material.shader = original.shader; material.CopyPropertiesFromMaterial(original); }
            else
            {
                material.shader = Shader.Find("Universal Render Pipeline/Lit");
                material.SetColor("_BaseColor", new Color(.055f, .05f, .042f));
                material.SetFloat("_Smoothness", .16f);
                material.SetFloat("_Metallic", 0f);
            }
            material.enableInstancing = true;
            EditorUtility.SetDirty(material);
            return material;
        }

        private static bool Candidate(Renderer renderer)
        {
            if (renderer is not MeshRenderer || !renderer.enabled || renderer.GetComponentInParent<StructuralSection>() != null ||
                renderer.GetComponentInParent<SectionProp>() != null || renderer.GetComponentInParent<LightFixture>() != null ||
                renderer.GetComponentInParent<Rigidbody>() != null || Hierarchy(renderer.transform).Contains("_ArtRemaster_")) return false;
            Vector3 size = renderer.bounds.size;
            float length = Mathf.Max(size.x, size.z), thickness = Mathf.Min(size.x, size.z);
            if (size.y < 1.8f || size.y > 8f || length < 1.4f || length > 36f || thickness > .55f || thickness < .015f) return false;
            string name = renderer.name.ToLowerInvariant();
            if (name.Contains("door") || name.Contains("window") || name.Contains("frame") || name.Contains("fence") || name.Contains("gate")) return false;
            foreach (Material material in renderer.sharedMaterials)
                if (material != null && material.renderQueue < 2450 && WallWords.Any(word => material.name.ToLowerInvariant().Contains(word))) return true;
            return false;
        }

        private static bool IsSkirting(Renderer renderer)
        {
            string name = renderer.name.ToLowerInvariant();
            return renderer.bounds.size.y < .4f && (name.Contains("skirting") || name.Contains("baseboard") || name.Contains("wallbase"));
        }

        private static string Quantize(Vector3 p) => $"{Mathf.RoundToInt(p.x * 100f)},{Mathf.RoundToInt(p.y * 100f)},{Mathf.RoundToInt(p.z * 100f)}";
        private static string Hierarchy(Transform transform) => transform.parent == null ? transform.name : Hierarchy(transform.parent) + "/" + transform.name;
    }
}
