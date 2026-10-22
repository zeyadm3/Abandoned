using System.Collections.Generic;
using Abandoned.Audio;
using Abandoned.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The TestMap's look carried to the real maps (user, 2026-10-22: "apply all techniques ... to all the other maps"),
    /// added on top of each saved scene, never regenerating it, and keeping each map's own hour and weather
    /// (MapSiteProfiles). Realtime light only, as before (the user kept M7.2: no baked light on the real maps).
    ///   1. a photographed sky for the skybox, the ambient and the reflections, its sun turned to the map's (MapSky);
    ///   2. the brick and asphalt scans the TestMap moved to (factory_brick at 1.5 m, asphalt_02 at 5 m);
    ///   3. the map's code-built trees swapped for scanned ones (MapTrees);
    ///   4. on a map that ends at a slab edge, wooded land past it (MapRing);
    ///   5. grass on its grass and soil (MapGrass, drawn by Core.GrassField);
    ///   6. outdoor sound round its edge (Audio.OutdoorSoundscape).
    /// Everything it adds lives under one "_MapSite" object; running it again first puts back what it replaced.
    /// Rebuild All Maps runs it last (AvOverhaul.AfterMaps).
    /// </summary>
    public static class MapSite
    {
        public const string RootName = "_MapSite";

        [MenuItem(ToolMenu.Art + "Map Sites: Sky, Trees, Grass and Sound", priority = ToolMenu.ArtP + 83)]
        public static void Menu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            ApplySaved();
        }

        /// <summary>Every map in AvOverhaul.MapScenes, opened, given its site and saved.</summary>
        public static void ApplySaved()
        {
            foreach (string path in AvOverhaul.MapScenes)
            {
                Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                ApplyToScene(scene);
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }
            AssetDatabase.SaveAssets();
        }

        public static void ApplyToScene(Scene scene)
        {
            if (!MapSiteProfiles.TryGet(scene.name, out MapSiteProfile profile)) return;
            Clear(scene);
            Physics.SyncTransforms();
            Bounds level = LevelBounds(scene);
            var site = new GameObject(RootName);
            SceneManager.MoveGameObjectToScene(site, scene);
            var record = site.AddComponent<MapSiteRecord>();

            MapSky.Apply(profile);
            int materials = Rescan(scene);
            int trees = MapTrees.Swap(scene, profile, level, site.transform, record);
            Physics.SyncTransforms();
            Terrain ring = profile.Ring ? MapRing.Build(scene, profile, level, site.transform) : null;
            (GrassField grass, List<Vector3> spots) = profile.Grass ? MapGrass.Grow(scene, level, site.transform) : (null, new List<Vector3>());
            Sound(site.transform, profile, level, spots);
            Debug.Log($"[MapSite] {scene.name}: sky {profile.Sky} (ambient x{RenderSettings.ambientIntensity:0.00}), {materials} materials rescanned, " +
                      $"{trees} trees swapped, {(ring != null ? ring.terrainData.treeInstanceCount + " trees on the land past the edge, " : "")}" +
                      $"{(grass != null ? grass.TuftCount : 0)} grass tufts.");
        }

        /// <summary>Puts back what the last run replaced and removes what it added.</summary>
        public static void Clear(Scene scene)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                if (root.name != RootName) continue;
                if (root.TryGetComponent(out MapSiteRecord record))
                    foreach (GameObject original in record.Replaced)
                        if (original != null) original.SetActive(true);
                Object.DestroyImmediate(root);
            }
        }

        // The map's extent, from its solid colliders (the far backdrops, over 600 m across, left out).
        private static Bounds LevelBounds(Scene scene)
        {
            var bounds = new Bounds();
            bool any = false;
            foreach (GameObject root in scene.GetRootGameObjects())
            foreach (Collider c in root.GetComponentsInChildren<Collider>())
            {
                if (c.isTrigger || c is TerrainCollider) continue;
                Bounds b = c.bounds;
                if (b.size.x > 600f || b.size.z > 600f) continue;
                if (!any) { bounds = b; any = true; }
                else bounds.Encapsulate(b);
            }
            return bounds;
        }

        // The remaster's brick and asphalt scans, everywhere this map uses them, swapped for the ones the TestMap moved
        // to; each keeps its mapping (triplanar or measured UVs), rescaled to the new scan's size. Shared materials are
        // swapped once (the next map finds them done).
        private static int Rescan(Scene scene)
        {
            var done = new HashSet<Material>();
            foreach (GameObject root in scene.GetRootGameObjects())
            foreach (Renderer r in root.GetComponentsInChildren<Renderer>(true))
            foreach (Material m in r.sharedMaterials)
            {
                if (m == null || !m.HasProperty("_BaseMap") || !done.Add(m)) continue;
                string map = m.GetTexture("_BaseMap") is Texture t ? AssetDatabase.GetAssetPath(t) : "";
                if (map.Contains("/Bricks106A/")) Swap(m, "factory_brick", 1.6f, 1.5f);
                else if (map.Contains("/Asphalt031/") && MaterialRemaster.Classify(m.name) == MaterialRemaster.SurfaceKind.Asphalt)
                    Swap(m, "asphalt_02", .85f, 5f);
                else done.Remove(m);
            }
            return done.Count;
        }

        private static void Swap(Material m, string scan, float oldMetres, float newMetres)
        {
            string folder = $"{ShowcaseTerrain.GroundFolder}/{scan}";
            Texture2D colour = ShowcaseNature.Texture(folder + "_diff", srgb: true, normal: false, cutout: false);
            if (colour == null) return;
            Texture2D mask = ShowcaseNature.Texture(folder + "_mask", srgb: false, normal: false, cutout: false);
            Vector2 scale = m.GetTextureScale("_BaseMap") * (oldMetres / newMetres);
            m.SetTexture("_BaseMap", colour);
            m.SetTexture("_BumpMap", ShowcaseNature.Texture(folder + "_nor_gl", srgb: false, normal: true, cutout: false));
            m.SetTexture("_MetallicGlossMap", mask);
            m.SetTexture("_OcclusionMap", mask);
            m.SetTextureScale("_BaseMap", scale);
            // The scans carry their own colour; the remaster's match to the old flat colour would tint them.
            m.SetColor("_BaseColor", Color.white);
            EditorUtility.SetDirty(m);
        }

        // Birds and wind from round the map's edge, crows high over it, crickets where the long grass is.
        private static void Sound(Transform parent, MapSiteProfile profile, Bounds level, List<Vector3> grass)
        {
            if (profile.Birds <= 0f && profile.Wind <= 0f && profile.Crickets <= 0f && !profile.Crows) return;
            Transform sound = GreyboxFactory.Group("Soundscape", parent);
            Transform[] Ring(string name, int count, float outside, float height)
            {
                var points = new Transform[count];
                for (int i = 0; i < count; i++)
                {
                    float a = (i + .5f) * Mathf.PI * 2f / count;
                    var p = new Vector3(level.center.x + Mathf.Cos(a) * (level.extents.x + outside), level.min.y + height,
                        level.center.z + Mathf.Sin(a) * (level.extents.z + outside));
                    points[i] = GreyboxFactory.Group($"{name}_{i}", sound);
                    points[i].position = p;
                }
                return points;
            }
            Transform[] birds = profile.Birds > 0f ? Ring("Birds", 8, 10f, 8f) : new Transform[0];
            Transform[] wind = profile.Wind > 0f ? Ring("Wind", 8, 15f, 12f) : new Transform[0];
            Transform[] crows = profile.Crows ? Ring("Crow", 7, 30f, 20f) : new Transform[0];
            var crickets = new List<Transform>();
            if (profile.Crickets > 0f)
                for (int i = 0; i < grass.Count && crickets.Count < 8; i += Mathf.Max(1, grass.Count / 8))
                {
                    Transform point = GreyboxFactory.Group("Crickets", sound);
                    point.position = grass[i] + Vector3.up * .3f;
                    crickets.Add(point);
                }
            sound.gameObject.AddComponent<OutdoorSoundscape>().EditorSetup(birds, wind, crickets.ToArray(), crows,
                profile.Birds, profile.Wind, profile.Crickets);
        }
    }
}
