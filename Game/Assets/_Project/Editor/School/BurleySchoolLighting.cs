using Abandoned.Structure;
using UnityEditor;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using L = Abandoned.EditorTools.BurleySchoolLayout;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Builds and wires the dynamic lighting systems for Jackson P. Burley Middle School:
    /// High-bay industrial gym lights, fluorescent classroom and corridor troffers (with faulty/flickering logic),
    /// glowing emergency exit signs, and exterior parking/street illumination.
    /// </summary>
    public static class BurleySchoolLighting
    {
        public static void Build(Transform root, BurleySchoolArt art)
        {
            Transform lights = Group("LightingSystem", root);
            Transform fluorescents = Group("FluorescentFixtures", lights);
            Transform gymHighBays = Group("GymnasiumHighBays", lights);
            Transform exitSigns = Group("EmergencyExitSigns", lights);
            Transform exteriorLights = Group("ExteriorIllumination", lights);

            // ---- 1. Fluorescent Troffers (Corridors & Classrooms) ----
            BuildFluorescents(fluorescents, art);

            // ---- 2. Gymnasium High-Bay Fixtures ----
            BuildGymHighBays(gymHighBays, art);

            // ---- 3. Emergency Exit Signs ----
            BuildExitSigns(exitSigns, art);

            // ---- 4. Exterior Street & Lot Lamps ----
            BuildExteriorLamps(exteriorLights, art);
        }

        private static void BuildFluorescents(Transform parent, BurleySchoolArt art)
        {
            // Grid of interior fluorescent fixtures
            Vector3[] locations =
            {
                // Ground Floor Hallway
                new(28f, 3.5f, 35f), new(38f, 3.5f, 35f), new(46f, 3.5f, 35f), new(54f, 3.5f, 35f), new(64f, 3.5f, 35f),
                // Ground Classrooms & Admin
                new(34f, 3.5f, 27f), new(58f, 3.5f, 27f), new(36f, 3.5f, 45f), new(56f, 3.5f, 45f),
                // Cafeteria & Kitchen
                new(34f, 3.5f, 62f), new(44f, 3.5f, 62f), new(34f, 3.5f, 72f), new(44f, 3.5f, 72f), new(58f, 3.5f, 75f),
                // Upper Floor Hallway
                new(28f, 7.5f, 35f), new(38f, 7.5f, 35f), new(46f, 7.5f, 35f), new(54f, 7.5f, 35f), new(64f, 7.5f, 35f),
                // Upper Science Lab & Library
                new(32f, 7.5f, 27f), new(40f, 7.5f, 27f), new(58f, 7.5f, 27f),
                new(34f, 7.5f, 62f), new(48f, 7.5f, 62f), new(34f, 7.5f, 72f), new(48f, 7.5f, 72f),
                // Basement Boiler Room
                new(54f, -0.6f, 82f), new(62f, -0.6f, 82f)
            };

            for (int i = 0; i < locations.Length; i++)
            {
                Transform holder = Group($"Fluorescent_{i:00}", parent);
                holder.position = locations[i];

                // Housing chassis and diffuser panel
                Box("Chassis", holder, Vector3.zero, new Vector3(1.4f, 0.12f, 0.55f), art.MetalSteel, false);
                GameObject diffuser = Box("Diffuser", holder, new Vector3(0f, -0.04f, 0f), new Vector3(1.3f, 0.04f, 0.45f), art.FluorescentGlow, false);

                var lampRoot = Group("Lamp", holder);
                lampRoot.localRotation = Quaternion.Euler(90f, 0f, 0f);
                Light lamp = lampRoot.gameObject.AddComponent<Light>();
                lamp.type = LightType.Spot;
                lamp.range = 10.5f;
                lamp.spotAngle = 110f;
                lamp.intensity = 4.2f;
                lamp.color = new Color(0.94f, 0.98f, 0.90f);
                lamp.shadows = LightShadows.None;

                // Deterministic flicker for survival horror atmosphere
                bool isFaulty = (i % 6 == 0);
                bool isDead = (i == 13 || i == 21);
                holder.gameObject.AddComponent<LightFixture>().EditorSetup(
                    lamp, diffuser.GetComponent<Renderer>(), isFaulty, isDead, 1951 + i, art.FluorescentGlow.GetColor("_EmissionColor"));
            }
        }

        private static void BuildGymHighBays(Transform parent, BurleySchoolArt art)
        {
            // 8 High-bay metal halide lamps over the Gymnasium court (suspended from Y: 7.2m)
            for (int r = 0; r < 4; r++)
            for (int c = 0; c < 2; c++)
            {
                int index = r * 2 + c;
                float gx = 76f + c * 12f;
                float gz = 28f + r * 10.5f;
                Transform holder = Group($"GymHighBay_{index}", parent);
                holder.position = new Vector3(gx, 7.2f, gz);

                // Industrial aluminum bell reflector
                Box("BellHousing", holder, Vector3.zero, new Vector3(0.85f, 0.55f, 0.85f), art.MetalSteel, false);
                GameObject glow = Box("Bulb", holder, new Vector3(0f, -0.2f, 0f), new Vector3(0.35f, 0.35f, 0.35f), art.FluorescentGlow, false);

                var lampRoot = Group("Lamp", holder);
                lampRoot.localRotation = Quaternion.Euler(90f, 0f, 0f);
                Light lamp = lampRoot.gameObject.AddComponent<Light>();
                lamp.type = LightType.Spot;
                lamp.range = 16f;
                lamp.spotAngle = 105f;
                lamp.intensity = 6.8f;
                lamp.color = new Color(0.88f, 0.94f, 1.0f);
                lamp.shadows = LightShadows.None;

                holder.gameObject.AddComponent<LightFixture>().EditorSetup(
                    lamp, glow.GetComponent<Renderer>(), index == 3, false, 2024 + index, art.FluorescentGlow.GetColor("_EmissionColor"));
            }
        }

        private static void BuildExitSigns(Transform parent, BurleySchoolArt art)
        {
            // Emergency EXIT sign positions over egress doorways
            (Vector3 pos, float yaw)[] exitDoors =
            {
                (new(46f, 2.75f, 20.2f), 180f), // Main Portico Lobby Exit
                (new(68f, 2.75f, 35f), -90f),   // Gym Main Entrance
                (new(82f, 2.75f, 67.8f), 180f), // Gym Rear Parking Exit
                (new(24f, 2.75f, 35f), 90f),    // Auditorium Lobby Exit
                (new(12f, 2.75f, 20.2f), 180f), // Auditorium South Exit
                (new(44f, 2.75f, 52.2f), 180f), // Cafeteria Exit
                (new(46f, 6.75f, 35f), 0f),     // Upper Hall Stair Exit
                (new(54f, -0.8f, 76.2f), 180f)  // Boiler Room Exit
            };

            for (int i = 0; i < exitDoors.Length; i++)
            {
                Transform fixture = Group($"ExitSign_{i}", parent);
                fixture.position = exitDoors[i].pos;
                fixture.rotation = Quaternion.Euler(0f, exitDoors[i].yaw, 0f);

                // Exit sign box with green emissive face
                Box("BoxHousing", fixture, Vector3.zero, new Vector3(0.85f, 0.32f, 0.12f), art.MetalSteel, false);
                GameObject glow = Box("GreenFace", fixture, new Vector3(0f, 0f, -0.05f), new Vector3(0.78f, 0.25f, 0.02f), art.ExitGlow, false);

                // Text
                Label(fixture, "ExitText", "EXIT", new Vector3(0f, 0f, -0.07f), Quaternion.identity, 0.09f, Color.white);

                // Subtle emergency green glow light
                Light lamp = fixture.gameObject.AddComponent<Light>();
                lamp.type = LightType.Point;
                lamp.range = 3.5f;
                lamp.intensity = 0.38f;
                lamp.color = new Color(0.25f, 0.85f, 0.42f);
                lamp.shadows = LightShadows.None;

                fixture.gameObject.AddComponent<LightFixture>().EditorSetup(
                    lamp, glow.GetComponent<Renderer>(), i == 2, false, 700 + i, art.ExitGlow.GetColor("_EmissionColor"));
            }
        }

        private static void BuildExteriorLamps(Transform parent, BurleySchoolArt art)
        {
            // Streetlights along Rose Hill Drive (Z: -14f)
            for (int i = 0; i < 7; i++)
            {
                float x = -20f + i * 36f;
                Transform pole = Group($"RoseHillLamp_{i}", parent);
                pole.position = new Vector3(x, 0f, L.RoseHillZ + 5.5f);

                // Steel pole and cobra arm
                Box("Pole", pole, new Vector3(0f, 4.0f, 0f), new Vector3(0.2f, 8.0f, 0.2f), art.MetalSteel);
                Box("Arm", pole, new Vector3(0f, 7.8f, -1.2f), new Vector3(0.12f, 0.12f, 2.4f), art.MetalSteel);
                Box("Luminaire", pole, new Vector3(0f, 7.7f, -2.2f), new Vector3(0.45f, 0.2f, 0.7f), art.LampSodium, false);

                // Sodium warm light pool
                var lampGo = Group("LightPool", pole);
                lampGo.localPosition = new Vector3(0f, 7.5f, -2.2f);
                lampGo.localRotation = Quaternion.Euler(85f, 0f, 0f);
                Light lamp = lampGo.gameObject.AddComponent<Light>();
                lamp.type = LightType.Spot;
                lamp.range = 22f;
                lamp.spotAngle = 100f;
                lamp.intensity = 8.5f;
                lamp.color = new Color(1.0f, 0.72f, 0.35f);
                lamp.shadows = LightShadows.None;
            }
        }

        private static void Label(Transform parent, string name, string text, Vector3 at, Quaternion rotation, float size, Color color)
        {
            var label = new GameObject(name).AddComponent<TextMesh>();
            label.transform.SetParent(parent, false);
            label.transform.localPosition = at;
            label.transform.localRotation = rotation;
            label.text = text;
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.characterSize = size;
            label.fontSize = 48;
            label.color = color;
            WorldTextMaterial.Apply(label, AssetDatabase.LoadAssetAtPath<Font>(BurleySchoolArt.FontPath));
        }
    }
}
