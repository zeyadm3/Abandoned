using System.Collections.Generic;
using Abandoned.Audio;
using Abandoned.Structure;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using static Abandoned.EditorTools.MeridianDressingUtility;

namespace Abandoned.EditorTools
{
    /// <summary>Mall-only volume, air and reflections; no baked lights or changes to HQ assets.</summary>
    public static class MeridianAtmosphere
    {
        public static void Apply(Transform mall)
        {
            MeridianPresentationConfig config = MeridianPresentationConfigBuilder.Load();
            Transform group = Group("MeridianAtmosphere", mall);
            MeridianAtmosphereVolume.Apply(group, config);
            BuildingInteriorAtmosphere atmosphere = mall.GetComponent<BuildingInteriorAtmosphere>()
                ?? mall.gameObject.AddComponent<BuildingInteriorAtmosphere>();
            atmosphere.EditorSetup(MallLayout.BuildingBounds);
            var data = new SerializedObject(atmosphere);
            data.FindProperty("interiorFog").colorValue = config.InteriorFog;
            data.FindProperty("interiorDensity").floatValue = config.InteriorFogDensity;
            data.ApplyModifiedPropertiesWithoutUndo();
            Air(group, config);
            Reflections(group, config);
            AmbienceBuilder.Mall(mall);
        }

        private static void Air(Transform group, MeridianPresentationConfig config)
        {
            Material dust = MeridianAtmosphereDust.Material();
            for (int floor = 0; floor < MallLayout.Floors; floor++)
            foreach (MallLayout.Zone zone in MallLayout.ZonesByFloor[floor])
            {
                if (zone.Kind == MallLayout.Kind.Walkway || zone.Name == "GrandAtrium") continue;
                Vector3 size = new(zone.Area.width * MallLayout.Tile, MallLayout.StoryHeight - 0.6f,
                    zone.Area.height * MallLayout.Tile);
                Vector3 at = new(zone.Area.center.x * MallLayout.Tile, MallLayout.FloorY(floor) + 2f,
                    zone.Area.center.y * MallLayout.Tile);
                var air = Group($"Area_{floor}_{zone.Name}", group);
                air.localPosition = at;
                air.gameObject.AddComponent<HorrorAreaAmbience>().EditorSetup(size, zone.Tag == "cinema" ? 1 : 0);
                MeridianAtmosphereDust.Motes(air, size, dust, config);
            }
            Vector3 atriumSize = new(MallLayout.Atrium.width * MallLayout.Tile, MallLayout.Floors * MallLayout.StoryHeight,
                MallLayout.Atrium.height * MallLayout.Tile);
            Transform atrium = Group("AtriumAir", group);
            atrium.localPosition = new Vector3(MallLayout.Atrium.center.x * MallLayout.Tile,
                MallLayout.Floors * MallLayout.StoryHeight * 0.5f, MallLayout.Atrium.center.y * MallLayout.Tile);
            atrium.gameObject.AddComponent<HorrorAreaAmbience>().EditorSetup(atriumSize, 0);
            MeridianAtmosphereDust.Motes(atrium, atriumSize, dust, config);
            Transform basement = Group("BasementAir", group);
            basement.localPosition = new Vector3(MallLayout.Basement.center.x * MallLayout.Tile, MallLayout.FloorY(-1) + 2f,
                MallLayout.Basement.center.y * MallLayout.Tile);
            basement.gameObject.AddComponent<HorrorAreaAmbience>().EditorSetup(new Vector3(MallLayout.Basement.width * MallLayout.Tile,
                MallLayout.StoryHeight, MallLayout.Basement.height * MallLayout.Tile), 2);
        }

        private static void Reflections(Transform group, MeridianPresentationConfig config)
        {
            var probes = new List<ReflectionProbe>();
            Probe("Atrium", new Vector3(MallLayout.Atrium.center.x * MallLayout.Tile, 5.7f,
                MallLayout.Atrium.center.y * MallLayout.Tile), new Vector3(23f, 12f, 23f));
            Probe("FoodCourt", new Vector3(24f, 2f, 44f), new Vector3(40f, 4f, 10f));
            Probe("Basement", new Vector3(MallLayout.Basement.center.x * MallLayout.Tile, -2f,
                MallLayout.Basement.center.y * MallLayout.Tile), new Vector3(16f, 4f, 24f));
            group.gameObject.AddComponent<MeridianReflectionRefresh>().EditorSetup(probes.ToArray(), config);

            void Probe(string name, Vector3 at, Vector3 size)
            {
                Transform holder = Group("Reflection_" + name, group);
                holder.localPosition = at;
                var probe = holder.gameObject.AddComponent<ReflectionProbe>();
                probe.mode = ReflectionProbeMode.Realtime;
                probe.refreshMode = ReflectionProbeRefreshMode.ViaScripting;
                probe.timeSlicingMode = ReflectionProbeTimeSlicingMode.IndividualFaces;
                probe.resolution = Mathf.ClosestPowerOfTwo(config.ReflectionResolution);
                probe.size = size;
                probe.intensity = config.ReflectionIntensity;
                probe.boxProjection = true;
                probe.blendDistance = 2f;
                probe.nearClipPlane = 0.2f;
                probe.farClipPlane = 35f;
                probe.cullingMask = ~LayerMask.GetMask(Abandoned.Core.GameLayers.Player, Abandoned.Core.GameLayers.Loot,
                    Abandoned.Core.GameLayers.Debris, "Ignore Raycast");
                probes.Add(probe);
            }
        }
    }
}
