using System;
using Abandoned.Core;
using Abandoned.Structure;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Abandoned.EditorTools.GreyboxFactory;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The TestMap as a graphics showcase (2026-10-22): the test building finished as a real warehouse (scanned
    /// surfaces, photo-scanned furniture and fittings, working light fixtures, a roof), and a gallery beside it where
    /// the crew and every monster stand still. TestMapBuilder calls <see cref="Dress"/> once the gameplay is in, then
    /// <see cref="Finish"/> after the first save for the passes the real maps get (NavMesh, reflection probes, wear
    /// decals, weathering, lighting calibration, the flicker fix). Gameplay colliders are untouched; see
    /// Docs/TESTMAP_SHOWCASE.md.
    /// </summary>
    public static class TestMapShowcase
    {
        private const float Tile = TestMapBuilder.Tile, Upper = TestMapBuilder.StoryHeight;
        private static float Width => TestMapBuilder.TilesX * Tile;
        private static float Depth => TestMapBuilder.TilesZ * Tile;
        private static float RoofY => TestMapBuilder.StoryHeight + TestMapBuilder.UpperWallHeight;

        public static void Dress(Transform root)
        {
            ShowcaseStructure.Girders(Group("Girders", root), Tile);
            float skin = TestMapBuilder.WallThickness + ShowcaseWalls.SkinGap + ShowcaseWalls.SkinThickness;
            ShowcaseStructure.Roof(Group("Roof", root), Vector3.zero, new Vector3(Width, 0f, Depth), RoofY, skin + .05f);
            Trim(Group("Trim", root), root);
            Markings(Group("Markings", root));
            ShowcaseDressing.Dress(root);
            Lights(Group("Lights", root), root);
            ShowcaseExterior.Build(root);
            ShowcaseGallery.Build(root);
            LevelAtmosphere.Apply(SceneName, ShowcaseLighting.Mood, root);
        }

        public static string SceneName => System.IO.Path.GetFileNameWithoutExtension(TestMapBuilder.ScenePath);

        public static void Finish(Scene scene, Transform root)
        {
            // The NavMesh is only for the passes below (probes and decals are laid out from it); nothing walks it here.
            LocationPlumbing.BakeNavMesh(root, SceneName, Array.Empty<string>());
            int probes = RoomReflectionProbes.Place(scene);
            int decals = DecalScatter.Scatter(scene.name);
            SurfaceAging.Place(scene);
            var config = AssetDatabase.LoadAssetAtPath<IndirectLightingConfig>(LightingRebuild.BounceConfigPath);
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<IndirectLightingConfig>();
                AssetDatabase.CreateAsset(config, LightingRebuild.BounceConfigPath);
            }
            LightingRebuild.ApplyScene(scene, config);
            GalleryContrast(root);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            (int moved, int faces, int left) = CoplanarFix.FixOpenScene(save: true);
            Debug.Log($"[Showcase] TestMap finished: {probes} reflection probes, {decals} wear decals, " +
                      $"{moved} objects moved off shared planes ({left} faces still within 0.1 mm).");
        }

        // After calibration every fixture lights its floor equally; the gallery wants its statues bright and the room dim.
        private static void GalleryContrast(Transform root)
        {
            Transform lights = root.Find("Gallery/Lights");
            if (lights == null) return;
            foreach (LightFixture fixture in lights.GetComponentsInChildren<LightFixture>(true))
            {
                if (fixture.Lamp == null) continue;
                if (fixture.name.StartsWith(ShowcaseGallery.CeilingLights)) fixture.Lamp.intensity *= .3f;
                else if (fixture.name.StartsWith(ShowcaseGallery.StatueLights)) fixture.Lamp.intensity *= 1.8f;
            }
        }

        private static void Lights(Transform lights, Transform root)
        {
            Transform ground = Group("GroundFloor", lights), upper = Group("UpperFloor", lights), atrium = Group("Atrium", lights);
            var tube = new Vector3(1.6f, .4f, .5f);
            int seed = 500, index = 0;
            for (int x = 0; x < TestMapBuilder.TilesX; x++)
            for (int z = 0; z < TestMapBuilder.TilesZ; z++)
            {
                StructuralSection floor = ShowcaseDressing.UpperSection(root, new Vector2Int(x, z));
                if (floor == null) continue; // atrium and stairwell: open to the roof
                var centre = new Vector3((x + .5f) * Tile, 0f, (z + .5f) * Tile);
                // Downstairs the tubes hang under the joists and go down with the floor above them; a few are failing.
                ShowcaseLighting.Ceiling(ground, "mounted_fluorescent_lights", centre + Vector3.up * ShowcaseStructure.UnderJoists, 0f, tube,
                    ShowcaseLighting.Fluorescent, seed++, faulty: index % 5 == 2, dead: index == 9, support: floor);
                ShowcaseLighting.Ceiling(upper, "mounted_fluorescent_lights", centre + Vector3.up * (RoofY - .01f), 0f, tube,
                    ShowcaseLighting.Fluorescent, seed++, faulty: index % 6 == 4);
                index++;
            }
            foreach (Vector2 p in new[] { new Vector2(6f, 6f), new Vector2(10f, 6f), new Vector2(6f, 10f), new Vector2(10f, 10f) })
                ShowcaseLighting.Ceiling(atrium, "hanging_industrial_lamp", new Vector3(p.x, RoofY - .01f, p.y), 0f, new Vector3(.7f, 1.8f, .7f),
                    ShowcaseLighting.Tungsten, seed++);
            ShowcaseLighting.Ceiling(atrium, "caged_hanging_light", new Vector3(18f, RoofY - .01f, 7f), 0f, new Vector3(.4f, 1.2f, .4f),
                ShowcaseLighting.Tungsten, seed);
        }

        // Skirting round the upper floor's outside walls, one board per tile edge so each goes with its tile.
        private static void Trim(Transform trim, Transform root)
        {
            const float corner = .0175f; // boards along z stop short of the ones along x
            for (int x = 0; x < TestMapBuilder.TilesX; x++)
            for (int z = 0; z < TestMapBuilder.TilesZ; z++)
            {
                StructuralSection floor = ShowcaseDressing.UpperSection(root, new Vector2Int(x, z));
                if (floor == null) continue;
                float x0 = x * Tile, x1 = x0 + Tile, z0 = z * Tile, z1 = z0 + Tile;
                if (z == 0) ShowcaseStructure.Skirting(trim, new Vector3(x0, Upper, 0f), new Vector3(x1, Upper, 0f), Vector3.forward, floor);
                if (z == TestMapBuilder.TilesZ - 1) ShowcaseStructure.Skirting(trim, new Vector3(x0, Upper, Depth), new Vector3(x1, Upper, Depth), Vector3.back, floor);
                float za = z == 0 ? z0 + corner : z0, zb = z == TestMapBuilder.TilesZ - 1 ? z1 - corner : z1;
                if (x == 0) ShowcaseStructure.Skirting(trim, new Vector3(0f, Upper, za), new Vector3(0f, Upper, zb), Vector3.right, floor);
                if (x == TestMapBuilder.TilesX - 1) ShowcaseStructure.Skirting(trim, new Vector3(Width, Upper, za), new Vector3(Width, Upper, zb), Vector3.left, floor);
            }
        }

        // Yellow lines on the warehouse floor: the loading bay inside the big door, and the walkway from the small one.
        private static void Markings(Transform markings)
        {
            Material yellow = ShowcaseMaterials.SafetyLine;
            ShowcaseStructure.Stripe(markings, new Vector3(12.2f, 0f, .25f), new Vector3(12.2f, 0f, 4.2f), .1f, yellow);
            ShowcaseStructure.Stripe(markings, new Vector3(15.8f, 0f, .25f), new Vector3(15.8f, 0f, 4.2f), .1f, yellow);
            ShowcaseStructure.Stripe(markings, new Vector3(12.25f, 0f, 4.2f), new Vector3(15.75f, 0f, 4.2f), .1f, yellow);
            ShowcaseStructure.Stripe(markings, new Vector3(5.25f, 0f, .25f), new Vector3(5.25f, 0f, 3.5f), .08f, yellow);
            ShowcaseStructure.Stripe(markings, new Vector3(6.75f, 0f, .25f), new Vector3(6.75f, 0f, 3.5f), .08f, yellow);
            MarkStatic(markings);
        }
    }
}
