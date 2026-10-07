using System.Collections.Generic;
using System.Linq;
using Abandoned.Structure;
using UnityEditor;
using UnityEngine;
using static Abandoned.EditorTools.MallLayout;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Ceiling fixtures cast downward pools, sputter with their diffusers, and fall with their slab.
    /// Drifting dust catches the remaining light. Placement is deterministic per tile.
    /// Seeded by tile, so rebuilding gives the same mall.
    /// </summary>
    public static class MallFixtures
    {
        private const float Range = 7.2f, Intensity = 1.15f;
        private const int FaultyPercent = 55, DeadPercent = 22;
        private static readonly Color Fluorescent = new(0.9f, 0.96f, 1f), Warm = new(1f, 0.9f, 0.75f);

        public static int Place(Transform root, Transform tiles)
        {
            Transform parent = GreyboxFactory.Group("Fixtures", root);
            int count = 0;
            for (int f = 0; f < Floors; f++)
                foreach (Zone zone in ZonesByFloor[f])
                foreach (int ix in Spread(zone.Area.width))
                foreach (int iz in Spread(zone.Area.height))
                {
                    var c = new Vector2Int(zone.Area.xMin + ix, zone.Area.yMin + iz);
                    if (IsVoid(c, f) || Flights.Any(fl => fl.Tiles.Contains(c) && (fl.Floor == f || fl.Floor == f - 1))) continue;
                    StructuralSection ceiling = null;
                    if (f + 1 < Floors)
                    {
                        // The atrium has no intermediate slab to support a fixture.
                        if (IsVoid(c, f + 1)) continue;
                        ceiling = tiles.Find($"Floor_{f + 1}/{MallBuilder.TileName(c, f + 1)}")?.GetComponent<StructuralSection>();
                        if (ceiling == null) continue;
                    }
                    else if (Atrium.Contains(c)) continue;
                    Build(parent, c, f, zone, ceiling);
                    count++;
                }
            return count;
        }

        /// <summary>About one fixture per two tiles along a side, spread evenly (3 -> 0,2; 6 -> 1,3,5).</summary>
        private static IEnumerable<int> Spread(int tiles)
        {
            int n = Mathf.Max(1, Mathf.RoundToInt(tiles / 2f + 0.01f));
            for (int k = 0; k < n; k++) yield return Mathf.FloorToInt((k + 0.5f) * tiles / n);
        }

        private static void Build(Transform parent, Vector2Int c, int floor, Zone zone, StructuralSection ceiling)
        {
            int hash = Hash(c, floor);
            float ceilingY = FloorY(floor) + StoryHeight - (floor + 1 < Floors ? TestMapBuilder.SlabThickness : 0f);
            var holder = new GameObject($"Fixture_{floor}_{c.x}_{c.y}");
            holder.transform.SetParent(parent, false);
            holder.transform.position = new Vector3((c.x + 0.5f) * Tile, ceilingY, (c.y + 0.5f) * Tile);
            holder.transform.rotation = Quaternion.Euler(0f, hash % 2 == 0 ? 0f : 90f, 0f);

            GameObject fixture = CustomMallArt.Place("light_fixture",holder.transform,new Vector3(0,-0.08f,0),Quaternion.identity);
            var lampObject = new GameObject("Light");
            lampObject.transform.SetParent(holder.transform, false);
            lampObject.transform.localPosition = new Vector3(0f, -0.3f, 0f);
            lampObject.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            var lamp = lampObject.AddComponent<Light>();
            lamp.type = LightType.Spot;
            lamp.range = Range;
            lamp.spotAngle = 112f;
            lamp.innerSpotAngle = 76f;
            // Stores have warm display lighting; concourses and back rooms buzzing tubes.
            lamp.color = zone.Kind == Kind.Store && zone.Tag != "stock" && zone.Tag != "office" ? Warm : Fluorescent;
            lamp.intensity = Intensity;
            lamp.shadows = hash % 11 == 0 ? LightShadows.Hard : LightShadows.None;
            lamp.shadowResolution = UnityEngine.Rendering.LightShadowResolution.Low;
            lamp.shadowBias = 0.03f;
            lamp.shadowNormalBias = 0.2f;

            int roll = hash / 7 % 100;
            holder.AddComponent<LightFixture>().EditorSetup(lamp, fixture.GetComponentInChildren<Renderer>(),
                isFaulty: roll < FaultyPercent, isDead: roll >= 100 - DeadPercent, fixtureSeed: hash % 997);
            if (ceiling != null) holder.AddComponent<SectionProp>().EditorSetup(ceiling);
        }

        /// <summary>Slow dust in every zone's air: lit motes, so they show in light and flashlight beams.</summary>
        public static void AddDust(Transform root)
        {
            Transform parent = GreyboxFactory.Group("Dust", root);
            Material dust = LightingAssets.Dust();
            for (int f = 0; f < Floors; f++)
                foreach (Zone zone in ZonesByFloor[f])
                {
                    if (zone.Kind == Kind.Walkway) continue; // the walkway ring is mostly the atrium's air
                    var size = new Vector3(zone.Area.width * Tile, StoryHeight - 0.6f, zone.Area.height * Tile);
                    var centre = new Vector3(zone.Area.center.x * Tile, FloorY(f) + StoryHeight / 2f, zone.Area.center.y * Tile);
                    Motes($"Dust_{f}_{zone.Name}", parent, centre, size, dust);
                }
            // The atrium's tall column of air, through all floors.
            var atrium = new Vector3(Atrium.width * Tile, Floors * StoryHeight - 0.5f, Atrium.height * Tile);
            Motes("Dust_Atrium", parent, new Vector3(Atrium.center.x * Tile, Floors * StoryHeight / 2f, Atrium.center.y * Tile), atrium, dust);
        }

        private static void Motes(string name, Transform parent, Vector3 centre, Vector3 size, Material material)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = centre;
            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            float volume = size.x * size.y * size.z;
            int count = Mathf.Clamp(Mathf.RoundToInt(volume * 0.12f), 20, 160);

            ParticleSystem.MainModule main = ps.main;
            main.loop = true;
            main.prewarm = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(10f, 18f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0f, 0.04f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.015f, 0.04f);
            main.maxParticles = count;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.gravityModifier = 0.002f;
            ParticleSystem.EmissionModule emission = ps.emission;
            emission.rateOverTime = count / 14f;
            ParticleSystem.ShapeModule shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = size;
            ParticleSystem.NoiseModule noise = ps.noise;
            noise.enabled = true;
            noise.strength = 0.05f;
            noise.frequency = 0.15f;
            ParticleSystem.ColorOverLifetimeModule fade = ps.colorOverLifetime;
            fade.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.2f), new GradientAlphaKey(1f, 0.8f), new GradientAlphaKey(0f, 1f) });
            fade.color = gradient;
            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }

        /// <summary>
        /// A fake volumetric shaft under the skylight: the opening extruded along the sun's direction,
        /// fading out over a few metres (additive, so it brightens the dust and fog it passes through).
        /// </summary>
        public static void AddSkylightShaft(Transform root, Vector3 sunForward, float length = 7f)
        {
            float y = Floors * StoryHeight;
            float x0 = Atrium.xMin * Tile + 0.3f, x1 = Atrium.xMax * Tile - 0.3f, z0 = Atrium.yMin * Tile + 0.3f, z1 = Atrium.yMax * Tile - 0.3f;
            Vector3 dir = sunForward.y < -0.1f ? sunForward / -sunForward.y : Vector3.down; // 1 m down per unit
            Vector3[] top = { new(x0, y, z0), new(x1, y, z0), new(x1, y, z1), new(x0, y, z1) };
            var vertices = new List<Vector3>();
            var colors = new List<Color>();
            var triangles = new List<int>();
            for (int i = 0; i < 4; i++)
            {
                Vector3 a = top[i], b = top[(i + 1) % 4];
                int v = vertices.Count;
                vertices.AddRange(new[] { a, b, b + dir * length, a + dir * length });
                colors.AddRange(new[] { new Color(1f, 1f, 1f, 0.09f), new Color(1f, 1f, 1f, 0.09f), Color.clear, Color.clear });
                triangles.AddRange(new[] { v, v + 1, v + 2, v, v + 2, v + 3 });
            }
            // Refill the same mesh asset in place, so the scene's reference (and GUID) never changes.
            string path = $"{LightingAssets.Folder}/Mall_SkylightShaft.asset";
            var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (mesh == null)
            {
                mesh = new Mesh();
                AssetDatabase.CreateAsset(mesh, path);
            }
            mesh.Clear();
            mesh.name = "Mall_SkylightShaft";
            mesh.SetVertices(vertices);
            mesh.SetColors(colors);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            EditorUtility.SetDirty(mesh);

            var go = new GameObject("SkylightShaft");
            go.transform.SetParent(root, false);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = go.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = LightingAssets.Shaft();
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }

        private static int Hash(Vector2Int c, int floor) => ((c.x * 92837111) ^ (c.y * 689287499) ^ (floor * 283923481)) & int.MaxValue;
    }
}
