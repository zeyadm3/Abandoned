using Abandoned.Structure;
using UnityEngine;
using UnityEngine.Rendering;
using static Abandoned.EditorTools.GreyboxFactory;

namespace Abandoned.EditorTools
{
    /// <summary>Suspended warehouse tubes, clerestory daylight and warm service-wing work pools.</summary>
    public static class HqSalvageAtmosphere
    {
        public static readonly LevelAtmosphere.Mood Mood = new(
            sky: new Color(.12f, .132f, .14f), equator: new Color(.079f, .085f, .087f),
            ground: new Color(.038f, .035f, .030f), fog: new Color(.22f, .24f, .245f),
            fogDensity: .0045f, bloom: .20f, vignette: .14f, grain: .10f,
            saturation: -10f, contrast: 8f, sunColor: new Color(.86f, .91f, .95f),
            sunIntensity: .10f, sunEuler: new Vector3(50f, -30f, 0f));

        public static void Build(Transform root)
        {
            Transform lights = Group("SalvageShopLights", root);
            Material metal = HqSalvageAtmosphereAssets.Solid("LampHousing", new Color(.19f, .22f, .22f), .55f);
            Material rust = HqSalvageAtmosphereAssets.Solid("LampRust", new Color(.34f, .21f, .13f), .22f);
            Material rubber = HqSalvageAtmosphereAssets.Solid("LampCable", new Color(.035f, .039f, .036f));
            Material cool = HqSalvageAtmosphereAssets.Solid("ShopTubeGlow", new Color(.65f, .78f, .82f), glow: 2.8f);
            Material warm = HqSalvageAtmosphereAssets.Solid("BareBulbGlow", new Color(1f, .61f, .28f), glow: 3.4f);

            for (int bay = 0; bay < 3; bay++)
            for (int aisle = 0; aisle < 3; aisle++)
            {
                Vector3 at = new(5f + aisle * 7f, 5.5f, 5f + bay * 8f);
                bool faulty = bay == 2 && aisle == 2;
                Fluorescent($"Warehouse_{bay}_{aisle}", at, 0f, faulty ? 30f : 42f, 11f,
                    faulty, 104 + bay * 100 + aisle * 17);
            }
            Fluorescent("Dispatch", new Vector3(30f, 3.35f, 5.1f), 0f, 19f, 7f, false, 441);
            Fluorescent("CrewRoom", new Vector3(31.5f, 3.35f, 14f), 0f, 15f, 6.8f, false, 552, 164.3437f);
            Fluorescent("Workshop", new Vector3(31.5f, 3.35f, 23.1f), 90f, 20f, 7f, false, 665);
            Fluorescent("SpareParts", new Vector3(28.8f, 3.35f, 20f), 0f, 10f, 4.5f, true, 776);
            Fluorescent("ServiceSpine", new Vector3(25.5f, 3.35f, 15.2f), 90f, 11f, 5.7f, false, 790);

            // The working surfaces stay steady while two secondary ballasts sputter.
            Pendant("DispatchDesk", HqLayout.Dispatch + Vector3.up * 2.8f, 12f, 5.2f, 811);
            Pendant("ScrapDepot", HqLayout.Depot + Vector3.up * 3.05f, 12f, 5.2f, 823);
            Pendant("GearBench", HqLayout.Maintenance + Vector3.up * 2.9f, 11f, 5f, 837);
            Pendant("CrewLounge", new Vector3(30f, 2.85f, 14.1f), 9f, 4.8f, 849);

            // These weak, broad fills belong to the glazed roof slots, leaving shop lamps dominant.
            foreach (float z in new[] { 6f, 14f, 22f })
            {
                Transform opening = Group("ClerestoryFill_" + z, lights);
                opening.localPosition = new Vector3(HqLayout.HallWidth * .5f, HqLayout.Ridge - .25f, z);
                Light daylight = Lamp(opening, LightType.Spot, new Color(.67f, .78f, .87f), 12f, 13f);
                daylight.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                daylight.spotAngle = 102f;
                daylight.innerSpotAngle = 72f;
            }

            HqSalvageAir.Build(root);

            void Fluorescent(string name, Vector3 at, float yaw, float intensity, float range, bool faulty, int seed,
                float spotAngle = 118f)
            {
                Transform holder = Group("Tube_" + name, lights);
                holder.SetLocalPositionAndRotation(at, Quaternion.Euler(0f, yaw, 0f));
                float length = at.x < HqLayout.HallWidth ? 2.5f : 1.72f;
                Box("BallastHousing", holder, Vector3.zero, new Vector3(length, .10f, .34f), metal, false);
                Box("Reflector", holder, new Vector3(0f, -.067f, 0f), new Vector3(length - .12f, .025f, .29f), metal, false);
                foreach (float z in new[] { -.092f, .092f })
                {
                    GameObject tube = Primitive(PrimitiveType.Cylinder, "FluorescentTube", holder,
                        new Vector3(0f, -.097f, z), new Vector3(.048f, (length - .20f) * .5f, .048f), cool, false);
                    tube.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                    foreach (float x in new[] { -length * .5f + .07f, length * .5f - .07f })
                        Box("PorcelainSocket", holder, new Vector3(x, -.087f, z), new Vector3(.07f, .085f, .07f), rubber, false);
                }
                foreach (float x in new[] { -length * .35f, length * .35f })
                {
                    float drop = SuspensionHeight(at) - at.y;
                    Box("SuspensionRod", holder, new Vector3(x, drop * .5f, 0f), new Vector3(.012f, drop, .012f), rust, false);
                    Box("RustClip", holder, new Vector3(x, .063f, 0f), new Vector3(.13f, .027f, .37f), rust, false);
                }
                Light lamp = Lamp(holder, LightType.Spot, new Color(.79f, .87f, .88f), intensity, range);
                lamp.transform.localPosition = Vector3.down * .15f;
                lamp.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                lamp.spotAngle = spotAngle;
                lamp.innerSpotAngle = 88f;
                holder.gameObject.AddComponent<LightFixture>().EditorSetup(lamp, null, faulty, false, seed);
                NoSelfShadows(holder);
            }

            void Pendant(string name, Vector3 at, float intensity, float range, int seed)
            {
                Transform holder = Group("HangingBulb_" + name, lights);
                holder.localPosition = at;
                float drop = SuspensionHeight(at) - at.y;
                Box("ExposedFlex", holder, new Vector3(0f, (drop + .155f) * .5f, 0f), new Vector3(.021f, drop - .155f, .021f), rubber, false);
                Primitive(PrimitiveType.Cylinder, "Socket", holder, new Vector3(0f, .10f, 0f), new Vector3(.085f, .055f, .085f), metal, false);
                Primitive(PrimitiveType.Sphere, "ExposedGlassBulb", holder, new Vector3(0f, -.019f, 0f), new Vector3(.13f, .18f, .13f), warm, false);
                Primitive(PrimitiveType.Cylinder, "MountingRose", holder, new Vector3(0f, drop, 0f), new Vector3(.15f, .017f, .15f), rust, false);
                Light lamp = Lamp(holder, LightType.Point, new Color(1f, .65f, .34f), intensity, range);
                lamp.transform.localPosition = Vector3.down * .10f;
                holder.gameObject.AddComponent<LightFixture>().EditorSetup(lamp, null, false, false, seed);
                NoSelfShadows(holder);
            }
        }

        private static float SuspensionHeight(Vector3 at) =>
            at.x < HqLayout.HallWidth ? HqLayout.WarehouseTrussBottom : HqLayout.ServiceCeiling;

        private static Light Lamp(Transform parent, LightType type, Color color, float intensity, float range)
        {
            var lamp = new GameObject("RealtimeLight").AddComponent<Light>();
            lamp.transform.SetParent(parent, false);
            lamp.type = type;
            lamp.color = color;
            lamp.intensity = intensity;
            lamp.range = range;
            lamp.shadows = LightShadows.None; // ShadowDirector owns the runtime budget, including the two-point-light cap.
            LevelStoreys.LightEverything(lamp);
            return lamp;
        }

        private static void NoSelfShadows(Transform fixture)
        {
            foreach (Renderer renderer in fixture.GetComponentsInChildren<Renderer>())
                renderer.shadowCastingMode = ShadowCastingMode.Off;
        }
    }
}
