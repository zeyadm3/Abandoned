using Abandoned.Structure;
using UnityEngine;
using UnityEngine.Rendering;
using static Abandoned.EditorTools.GreyboxFactory;

namespace Abandoned.EditorTools
{
    /// <summary>HQ-only shop lighting: tired cool tubes and readable, warm work pools.</summary>
    public static class HqSalvageAtmosphere
    {
        public static readonly LevelAtmosphere.Mood Mood = new(
            sky: new Color(.095f, .11f, .117f), equator: new Color(.055f, .062f, .064f),
            ground: new Color(.028f, .026f, .022f), fog: new Color(.22f, .24f, .245f),
            fogDensity: .007f, bloom: .20f, vignette: .17f, grain: .12f,
            saturation: -13f, contrast: 10f, sunColor: new Color(.86f, .91f, .95f),
            sunIntensity: .16f, sunEuler: new Vector3(50f, -30f, 0f));

        public static void Build(Transform root)
        {
            Transform lights = Group("SalvageShopLights", root);
            Material metal = HqSalvageAtmosphereAssets.Solid("LampHousing", new Color(.19f, .22f, .22f), .55f);
            Material rust = HqSalvageAtmosphereAssets.Solid("LampRust", new Color(.34f, .21f, .13f), .22f);
            Material rubber = HqSalvageAtmosphereAssets.Solid("LampCable", new Color(.035f, .039f, .036f));
            Material cool = HqSalvageAtmosphereAssets.Solid("ShopTubeGlow", new Color(.65f, .78f, .82f), glow: 2.8f);
            Material warm = HqSalvageAtmosphereAssets.Solid("BareBulbGlow", new Color(1f, .61f, .28f), glow: 3.4f);

            Fluorescent("Arrival", new Vector3(5.2f, 3.62f, 3.4f), 0f, 11f, 7f, false, 104);
            Fluorescent("Garage", new Vector3(6f, 3.58f, 9f), 0f, 12f, 7.5f, false, 213);
            Fluorescent("ScrapStacks", new Vector3(9.1f, 3.61f, 14.3f), 90f, 8f, 5.8f, true, 328);
            Fluorescent("OfficeEntry", new Vector3(15.1f, 3.58f, 2.6f), 0f, 9f, 6f, false, 441);
            Fluorescent("OfficeBack", new Vector3(21.1f, 3.62f, 7.8f), 90f, 8f, 5.6f, true, 552);
            Fluorescent("Workshop", new Vector3(18f, 3.55f, 13.5f), 0f, 11f, 6.8f, false, 665);
            Fluorescent("SpareParts", new Vector3(22.1f, 3.65f, 12.2f), 90f, 7f, 4.8f, false, 776);

            // The working surfaces stay steady while two secondary ballasts sputter.
            Pendant("DispatchDesk", new Vector3(18f, 2.68f, 5f), 8.5f, 4.5f, 811);
            Pendant("ScrapDepot", new Vector3(5.3f, 2.86f, 14.7f), 8f, 4.4f, 823);
            Pendant("GearBench", new Vector3(18f, 2.72f, 13.5f), 7f, 4.2f, 837);
            Pendant("LockerBay", new Vector3(1.7f, 2.84f, 6.6f), 3.8f, 3.3f, 849);

            HqSalvageAir.Build(root);

            void Fluorescent(string name, Vector3 at, float yaw, float intensity, float range, bool faulty, int seed)
            {
                Transform holder = Group("Tube_" + name, lights);
                holder.SetLocalPositionAndRotation(at, Quaternion.Euler(0f, yaw, 0f));
                Box("BallastHousing", holder, Vector3.zero, new Vector3(1.72f, .10f, .34f), metal, false);
                Box("Reflector", holder, new Vector3(0f, -.067f, 0f), new Vector3(1.60f, .025f, .29f), metal, false);
                foreach (float z in new[] { -.092f, .092f })
                {
                    GameObject tube = Primitive(PrimitiveType.Cylinder, "FluorescentTube", holder,
                        new Vector3(0f, -.097f, z), new Vector3(.048f, .76f, .048f), cool, false);
                    tube.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                    foreach (float x in new[] { -.79f, .79f })
                        Box("PorcelainSocket", holder, new Vector3(x, -.087f, z), new Vector3(.07f, .085f, .07f), rubber, false);
                }
                foreach (float x in new[] { -.60f, .60f })
                {
                    float drop = 3.94f - at.y;
                    Box("SuspensionRod", holder, new Vector3(x, drop * .5f, 0f), new Vector3(.012f, drop, .012f), rust, false);
                    Box("RustClip", holder, new Vector3(x, .063f, 0f), new Vector3(.13f, .027f, .37f), rust, false);
                }
                Light lamp = Lamp(holder, LightType.Spot, new Color(.70f, .81f, .85f), intensity, range);
                lamp.transform.localPosition = Vector3.down * .15f;
                lamp.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                lamp.spotAngle = 118f;
                lamp.innerSpotAngle = 88f;
                holder.gameObject.AddComponent<LightFixture>().EditorSetup(lamp, null, faulty, false, seed);
                NoSelfShadows(holder);
            }

            void Pendant(string name, Vector3 at, float intensity, float range, int seed)
            {
                Transform holder = Group("HangingBulb_" + name, lights);
                holder.localPosition = at;
                float drop = 3.96f - at.y;
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
