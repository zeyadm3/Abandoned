using System.Collections.Generic;
using System.Linq;
using Abandoned.Structure;
using UnityEngine;
using UnityEngine.Rendering;
using static Abandoned.EditorTools.MeridianDressingUtility;

namespace Abandoned.EditorTools
{
    /// <summary>Realtime mall lights follow their real ceiling support and the existing power director.</summary>
    public static class MeridianLighting
    {
        public static int Place(Transform mall)
        {
            MeridianPresentationConfig config = MeridianPresentationConfigBuilder.Load();
            Transform group = Group("MeridianFixtures", mall);
            int fixtures = 0, shadows = 0;
            for (int floor = -1; floor < MallLayout.Floors; floor++)
            for (int x = 0; x < MallLayout.TilesX; x++)
            for (int z = 0; z < MallLayout.TilesZ; z++)
            {
                var cell = new Vector2Int(x, z);
                bool serviceSpine = x == 0 || x == MallLayout.Basement.xMin;
                bool coreLanding = x == MallLayout.Basement.xMin + 1 && (z == 8 || z == 11);
                if (!Occupied(cell, floor) || (!coreLanding && ((!serviceSpine && (x + floor + 2) % 2 != 0) || z % 2 != 0))) continue;
                if (MallLayout.AllFlights.Any(f => (f.Floor == floor || f.Floor == floor - 1) && f.Tiles.Contains(cell))) continue;
                StructuralSection support = floor + 1 < MallLayout.Floors ? Support(mall, cell, floor + 1) : null;
                if (floor + 1 < MallLayout.Floors && support == null) continue;
                if (floor == MallLayout.Floors - 1 && MallLayout.IsAtriumVoid(cell)) continue;
                if (fixtures >= config.MaximumFixtures) continue;
                int hash = Hash(cell, floor);
                bool shadow = floor >= 0 && shadows < config.ShadowLights && hash % 13 == 0;
                if (shadow) shadows++;
                Fixture(group, cell, floor, support, config, shadow);
                fixtures++;
            }
            Alarms(group, config);
            Debug.Log($"[MeridianLighting] {fixtures} realtime fixtures, {shadows} shadow lights; basement included.");
            return fixtures;
        }

        private static void Fixture(Transform group, Vector2Int cell, int floor, StructuralSection support,
            MeridianPresentationConfig config, bool shadow)
        {
            int hash = Hash(cell, floor);
            Transform holder = Group($"Fixture_{floor}_{cell.x}_{cell.y}", group);
            float drop = MallLayout.SlabThickness + 0.1f;
            if (MeridianArt.Dress)
                drop = MallLayout.SlabThickness + 0.04f
                    - MeridianLightingCeilingBounds.MinimumY(MeridianDressingCeilings.KeyFor(cell, floor))
                    + config.FixtureCeilingClearance;
            holder.localPosition = MallLayout.TileTopCenter(cell, floor + 1) + Vector3.down * drop;
            holder.localRotation = Quaternion.Euler(0f, hash % 2 * 90f, 0f);
            GameObject visual = Prop("FluorescentFixture", holder, Vector3.zero, Quaternion.identity);
            Transform lampObject = Group("Light", holder);
            lampObject.localPosition = Vector3.down * 0.18f;
            lampObject.localRotation = Quaternion.Euler(90f, 0f, 0f);
            var light = lampObject.gameObject.AddComponent<Light>();
            light.type = LightType.Spot;
            light.range = config.FixtureRange;
            light.spotAngle = config.FixtureAngle;
            light.innerSpotAngle = config.FixtureAngle * 0.66f;
            light.intensity = config.FixtureIntensity * (floor < 0 ? config.BasementIntensityScale : 1f);
            string tag = floor < 0 ? "stock" : MallLayout.ZoneAt(cell, floor)?.Tag;
            light.color = tag == "food" || tag == "furniture" || tag == "gallery" ? config.Sodium : config.Fluorescent;
            light.shadows = shadow ? LightShadows.Hard : LightShadows.None;
            light.shadowResolution = LightShadowResolution.Low;
            light.shadowBias = 0.025f;
            light.shadowNormalBias = 0.2f;
            Renderer diffuser = visual != null ? visual.GetComponentsInChildren<Renderer>(true).FirstOrDefault(r => r.name.Contains("Diffuser")) : null;
            holder.gameObject.AddComponent<LightFixture>().EditorSetup(light, diffuser,
                hash % 100 < config.FaultyPercent, hash / 7 % 100 < config.DeadPercent, hash % 997);
            if (support == null) return;
            if (visual != null) holder.gameObject.AddComponent<MeridianFixtureFall>().EditorSetup(support, visual.transform, config);
            holder.gameObject.AddComponent<SectionProp>().EditorSetup(support);
        }

        private static void Alarms(Transform group, MeridianPresentationConfig config)
        {
            int index = 0;
            foreach (var exit in MallLayout.Exterior)
            {
                if (exit.window) continue;
                Alarm(MallLayout.ExteriorPosition(exit) + Vector3.up * 3f, index++);
            }
            for (int floor = -1; floor < MallLayout.Floors; floor++)
                Alarm(new Vector3(43.7f, MallLayout.FloorY(floor) + 3f, 34.5f), index++);

            void Alarm(Vector3 at, int number)
            {
                Transform holder = Group($"HorrorAlarm_{number}", group);
                holder.localPosition = at;
                GameObject visual = Prop("FluorescentFixture", holder, Vector3.zero, Quaternion.identity, null, new Vector3(0.3f, 0.65f, 0.65f));
                // Alarm housings never emit before the final phase; only the controlled point lamp lights them.
                if (visual != null)
                    foreach (Renderer renderer in visual.GetComponentsInChildren<Renderer>(true))
                    {
                        var block = new MaterialPropertyBlock();
                        block.SetColor("_EmissionColor", Color.black);
                        renderer.SetPropertyBlock(block);
                    }
                var light = holder.gameObject.AddComponent<Light>();
                light.type = LightType.Point;
                light.color = new Color(1f, 0.025f, 0.01f);
                light.intensity = config.AlarmIntensity;
                light.range = config.AlarmRange;
                light.shadows = LightShadows.None;
                light.enabled = false;
            }
        }
    }
}
