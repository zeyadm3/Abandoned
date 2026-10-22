using Abandoned.Extraction;
using Abandoned.Structure;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using L = Abandoned.EditorTools.SkyscraperLayout;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Configures sector-powered illumination, practical light fixtures, and emergency beacons.
    /// Wires LightFixtures across Sector 0 (Lobby), 1 (Mechanical/Shaft), 2 (Dining), 3 (Kitchen), 4 (VIP).
    /// </summary>
    public static class SkyscraperLighting
    {
        public static int Build(Transform root, SkyscraperArt art)
        {
            Transform lightsGroup = Group("Lights", root);
            int count = 0;

            // 1. Sector 0: Plaza Floodlights & Grand Lobby Pendants
            Transform plazaLights = Group("PlazaLights_Sector0", lightsGroup);
            CreateSpotFixture(plazaLights, art, new Vector3(0f, 5.2f, -14.5f), new Vector3(0f, -0.7f, 1f),
                new Color(1f, 0.95f, 0.85f), 12f, 18f, 65f, ref count);
            for (float lx = -10f; lx <= 10f; lx += 10f)
            {
                CreateSpotFixture(plazaLights, art, new Vector3(lx, L.LobbyHeight - 0.2f, -3f), Vector3.down,
                    new Color(1f, 0.92f, 0.8f), 8f, 10f, 75f, ref count);
            }

            // 2. Sector 1: Elevator Cab & Mechanical High-Bays
            Transform shaftLights = Group("ShaftLights_Sector1", lightsGroup);
            // Cab ceiling fixture
            Transform cab = root.Find("ElevatorShaft/ElevatorCab");
            if (cab != null)
            {
                Transform cabLight = Group("CabCeilingFixture", cab);
                cabLight.localPosition = new Vector3(0f, 2.7f, 0f);
                CreateCeilingLamp(cabLight, art, Vector3.zero, new Color(1f, 0.95f, 0.88f), 7f, 6f, ref count);
            }

            // Mechanical deck high-bays (y = 24.5m)
            Transform mechLights = Group("MechLights_Sector1", lightsGroup);
            for (float mx = -10f; mx <= 10f; mx += 10f)
            {
                if (Mathf.Abs(mx) < 3f) continue;
                CreateCeilingLamp(mechLights, art, new Vector3(mx, L.MechanicalY + 4.6f, 5f),
                    new Color(1f, 0.85f, 0.65f), 10f, 14f, ref count);
            }

            // Stairwell Emergency Bulkheads (every story from Ground to Summit)
            Transform stairLights = Group("StairwellLights_Sector1", lightsGroup);
            for (int story = 0; story <= 11; story++)
            {
                float sy = story * 4.0f + 2.2f;
                CreateBulkheadFixture(stairLights, art, new Vector3(-12f, sy, -2.8f),
                    new Color(0.95f, 0.95f, 0.85f), 5.5f, 7.5f, ref count);
            }

            // Intermediate Floor Lighting: Flooded Floor (12m) and Office (32m)
            CreateBulkheadFixture(stairLights, art, new Vector3(-3.4f, 12.0f + 3.0f, 3.0f),
                new Color(0.80f, 0.90f, 1.0f), 6.0f, 8.0f, ref count);
            CreateFluorescentTube(stairLights, art, new Vector3(-3.4f, 32.0f + 3.2f, 3.0f), ref count);

            // 3. Sector 2: Grand Dining Room Chandeliers & Terrace Wall Sconces
            Transform diningLights = Group("DiningLights_Sector2", lightsGroup);
            Vector3[] chandelierPos = {
                new(-8f, L.RestaurantY + 4.5f, 6f), new(8f, L.RestaurantY + 4.5f, 6f),
                new(-8f, L.RestaurantY + 4.5f, 14f), new(8f, L.RestaurantY + 4.5f, 14f),
                new(0f, L.RestaurantY + 4.8f, 10f) // Center chandelier
            };

            for (int i = 0; i < chandelierPos.Length; i++)
                CreateChandelier(diningLights, art, chandelierPos[i], ref count);

            // Observation Terrace Outdoor Sconces
            CreateSpotFixture(diningLights, art, new Vector3(-16f, L.RestaurantY + 2.8f, 10f), Vector3.right,
                new Color(1f, 0.92f, 0.8f), 7f, 9f, 80f, ref count);
            CreateSpotFixture(diningLights, art, new Vector3(-16f, L.RestaurantY + 2.8f, 18f), Vector3.right,
                new Color(1f, 0.92f, 0.8f), 7f, 9f, 80f, ref count);

            // 4. Sector 3: Kitchen Industrial Fluorescents & Cold Store Bulkhead
            Transform kitchenLights = Group("KitchenLights_Sector3", lightsGroup);
            CreateFluorescentTube(kitchenLights, art, new Vector3(-9f, L.RestaurantY + 4.2f, -2f), ref count);
            CreateFluorescentTube(kitchenLights, art, new Vector3(-14f, L.RestaurantY + 4.2f, -2f), ref count);
            CreateFluorescentTube(kitchenLights, art, new Vector3(-9f, L.RestaurantY + 4.2f, 1f), ref count);

            // Cold Store vapor-proof globe lamp
            CreateBulkheadFixture(kitchenLights, art, new Vector3(-12.5f, L.RestaurantY + 3.2f, -6f),
                new Color(0.75f, 0.85f, 1.0f), 5f, 7f, ref count);

            // 5. Sector 4: VIP Penthouse Luxury Recessed Halogens
            Transform vipLights = Group("VIPLights_Sector4", lightsGroup);
            CreateCeilingLamp(vipLights, art, new Vector3(11f, L.RestaurantY + 4.8f, 10f),
                new Color(1f, 0.92f, 0.78f), 9f, 12f, ref count);
            CreateCeilingLamp(vipLights, art, new Vector3(11f, L.RestaurantY + 4.8f, 16f),
                new Color(1f, 0.92f, 0.78f), 9f, 12f, ref count);

            // 6. Breaker Panels for Sectors 0, 2, and 4 (Sectors 1 and 3 are in Mechanical & Kitchen)
            Transform breakersGroup = Group("Breakers", root);
            CreateBreakerPanel(breakersGroup, art, 0, new Vector3(-9.5f, L.GroundY, -4.5f), Quaternion.LookRotation(Vector3.forward));
            CreateBreakerPanel(breakersGroup, art, 2, new Vector3(14.5f, L.RestaurantY, -1.0f), Quaternion.LookRotation(Vector3.left));
            CreateBreakerPanel(breakersGroup, art, 4, new Vector3(6.2f, L.RestaurantY, 5.0f), Quaternion.LookRotation(Vector3.right));

            return count;
        }

        private static void CreateCeilingLamp(Transform parent, SkyscraperArt art, Vector3 at, Color color, float intensity, float range, ref int count)
        {
            Transform holder = Group($"CeilingLamp_{count}", parent);
            holder.position = at;
            Box("Housing", holder, Vector3.zero, new Vector3(0.6f, 0.12f, 0.6f), art.Steel, false);
            GameObject glow = Box("Lens", holder, new Vector3(0f, -0.06f, 0f), new Vector3(0.5f, 0.04f, 0.5f), art.LampEmissive, false);

            Transform lampGo = Group("LightSource", holder);
            lampGo.localRotation = Quaternion.Euler(90f, 0f, 0f);
            Light light = lampGo.gameObject.AddComponent<Light>();
            light.type = LightType.Spot;
            light.range = range;
            light.spotAngle = 85f;
            light.intensity = intensity;
            light.color = color;
            light.shadows = LightShadows.None;

            int roll = Mathf.Abs((count * 4919 + 719) % 100);
            holder.gameObject.AddComponent<LightFixture>().EditorSetup(light, glow.GetComponent<Renderer>(),
                roll >= 50 && roll < 75, roll < 15, 3100 + count);
            count++;
        }

        private static void CreateSpotFixture(Transform parent, SkyscraperArt art, Vector3 at, Vector3 forward, Color color, float intensity, float range, float angle, ref int count)
        {
            Transform holder = Group($"Spot_{count}", parent);
            holder.position = at;
            Box("Housing", holder, Vector3.zero, new Vector3(0.5f, 0.5f, 0.6f), art.Steel, false);
            GameObject glow = Box("Lens", holder, new Vector3(0f, 0f, 0.31f), new Vector3(0.4f, 0.4f, 0.04f), art.LampEmissive, false);

            Transform lampGo = Group("LightSource", holder);
            lampGo.rotation = Quaternion.LookRotation(forward);
            Light light = lampGo.gameObject.AddComponent<Light>();
            light.type = LightType.Spot;
            light.range = range;
            light.spotAngle = angle;
            light.intensity = intensity;
            light.color = color;
            light.shadows = LightShadows.None;

            holder.gameObject.AddComponent<LightFixture>().EditorSetup(light, glow.GetComponent<Renderer>(),
                false, false, 3200 + count);
            count++;
        }

        private static void CreateBulkheadFixture(Transform parent, SkyscraperArt art, Vector3 at, Color color, float intensity, float range, ref int count)
        {
            Transform holder = Group($"Bulkhead_{count}", parent);
            holder.position = at;
            Box("Base", holder, Vector3.zero, new Vector3(0.35f, 0.45f, 0.15f), art.Steel, false);
            GameObject glow = Box("GlassGlobe", holder, new Vector3(0f, 0f, 0.1f), new Vector3(0.25f, 0.35f, 0.15f), art.LampEmissive, false);

            Light light = holder.gameObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.range = range;
            light.intensity = intensity;
            light.color = color;
            light.shadows = LightShadows.None;

            int roll = Mathf.Abs((count * 3137 + 101) % 100);
            holder.gameObject.AddComponent<LightFixture>().EditorSetup(light, glow.GetComponent<Renderer>(),
                roll >= 60 && roll < 85, roll < 20, 3300 + count);
            count++;
        }

        private static void CreateChandelier(Transform parent, SkyscraperArt art, Vector3 at, ref int count)
        {
            Transform chandelier = Group($"Chandelier_{count}", parent);
            chandelier.position = at;
            Box("Chain", chandelier, new Vector3(0f, 0.6f, 0f), new Vector3(0.08f, 1.2f, 0.08f), art.Brass, false);
            Box("CrownRing", chandelier, Vector3.zero, new Vector3(2.2f, 0.15f, 2.2f), art.Brass, false);
            GameObject glow = Box("CrystalPendants", chandelier, new Vector3(0f, -0.3f, 0f), new Vector3(1.8f, 0.6f, 1.8f), art.LampEmissive, false);

            Light light = chandelier.gameObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.range = 14f;
            light.intensity = 11f;
            light.color = new Color(1f, 0.90f, 0.75f);
            light.shadows = LightShadows.None;

            int roll = Mathf.Abs((count * 2851 + 333) % 100);
            chandelier.gameObject.AddComponent<LightFixture>().EditorSetup(light, glow.GetComponent<Renderer>(),
                roll >= 70 && roll < 90, roll < 10, 3400 + count);
            count++;
        }

        private static void CreateFluorescentTube(Transform parent, SkyscraperArt art, Vector3 at, ref int count)
        {
            Transform tube = Group($"FluorescentTube_{count}", parent);
            tube.position = at;
            Box("FixtureTray", tube, Vector3.zero, new Vector3(0.35f, 0.1f, 2.5f), art.Steel, false);
            GameObject glow = Box("GlassTubes", tube, new Vector3(0f, -0.06f, 0f), new Vector3(0.24f, 0.05f, 2.4f), art.LampEmissive, false);

            Transform lampGo = Group("LightSource", tube);
            lampGo.localRotation = Quaternion.Euler(90f, 0f, 0f);
            Light light = lampGo.gameObject.AddComponent<Light>();
            light.type = LightType.Spot;
            light.range = 9f;
            light.spotAngle = 105f;
            light.intensity = 8.5f;
            light.color = new Color(0.85f, 0.92f, 1.0f);
            light.shadows = LightShadows.None;

            int roll = Mathf.Abs((count * 1777 + 99) % 100);
            tube.gameObject.AddComponent<LightFixture>().EditorSetup(light, glow.GetComponent<Renderer>(),
                roll >= 40 && roll < 70, roll < 15, 3500 + count);
            count++;
        }

        public static BreakerPanel CreateBreakerPanel(Transform parent, SkyscraperArt art, int sector, Vector3 at, Quaternion rot)
        {
            Transform breakerGroup = Group($"Sector{sector}_Breaker", parent);
            breakerGroup.SetPositionAndRotation(at, rot);
            Box("Cabinet", breakerGroup, new Vector3(0f, 1.2f, 0f), new Vector3(0.8f, 1.2f, 0.35f), art.Steel);
            Transform handle = Group("LeverHandle", breakerGroup);
            handle.localPosition = new Vector3(0.45f, 1.2f, 0f);
            Box("LeverArm", handle, new Vector3(0f, 0.15f, 0f), new Vector3(0.06f, 0.3f, 0.06f), art.Brass, false);
            handle.localRotation = Quaternion.Euler(35f, 0f, 0f);
            GameObject lamp = Box("BreakerLamp", breakerGroup, new Vector3(-0.25f, 1.65f, 0.18f),
                new Vector3(0.1f, 0.1f, 0.08f), art.LampEmissive, false);
            var breaker = breakerGroup.gameObject.AddComponent<BreakerPanel>();
            breaker.EditorSetup(sector, handle, lamp.GetComponent<Renderer>(), true);
            return breaker;
        }
    }
}
