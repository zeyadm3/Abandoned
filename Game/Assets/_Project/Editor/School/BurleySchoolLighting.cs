using Abandoned.Structure;
using UnityEngine;
using UnityEngine.Rendering;
using static Abandoned.EditorTools.GreyboxFactory;
using L = Abandoned.EditorTools.BurleySchoolLayout;

namespace Abandoned.EditorTools
{
    /// <summary>Cold winter daylight outside; pools of failing institutional and theatre light inside.</summary>
    public static class BurleySchoolLighting
    {
        public static readonly LevelAtmosphere.Mood Winter3PMMood = new(
            sky: new Color(.45f, .55f, .70f), equator: new Color(.57f, .64f, .74f),
            ground: new Color(.61f, .67f, .77f), fog: new Color(.72f, .78f, .86f),
            fogDensity: .0038f, bloom: .19f, vignette: .18f, grain: .07f,
            saturation: -9f, contrast: 15f, sunColor: new Color(1f, .86f, .70f),
            sunIntensity: 1.05f, sunEuler: new Vector3(24f, -140f, 0f));

        public static void Build(Transform root, BurleySchoolArt art)
        {
            Transform lights = Group("LightingSystem", root);
            BuildFluorescents(Group("FluorescentFixtures", lights), art);
            BuildGymHighBays(Group("GymnasiumHighBays", lights), art);
            BuildAuditorium(Group("AuditoriumLighting", lights), art);
            BuildExitSigns(Group("EmergencyExitSigns", lights), art);
            BuildExteriorLamps(Group("ExteriorIllumination", lights), art);
            BuildReflections(Group("LocalReflectionProbes", lights));
            BurleySchoolWinterWeather.Build(Group("WinterWeatherEffects", root), art);
        }

        private static void BuildFluorescents(Transform parent, BurleySchoolArt art)
        {
            int seed = 1951;
            Color hall = new(.76f, .86f, .83f), room = new(.9f, .9f, .76f), lab = new(.7f, .82f, .93f);
            for (int floor = 0; floor < 2; floor++)
            {
                float y = floor * L.StoryHeight + 3.58f;
                foreach (float x in new[] { 27f, 37f, 54f, 64f })
                    Troffer(parent, new Vector3(x, y, 36f), hall, 1.65f, 7f, seed++, false, art);
                foreach (Vector3 centre in new[] { new Vector3(32f, y, 26f), new Vector3(60f, y, 26f), new Vector3(32f, y, 46f), new Vector3(59f, y, 46f) })
                    Troffer(parent, centre, floor == 1 && centre.x < 40f ? lab : room, 2f, 8f, seed++, false, art);
            }
            foreach (Vector3 at in new[] { new Vector3(33f, 3.58f, 59f), new Vector3(42f, 3.58f, 72f), new Vector3(58f, 3.58f, 63f), new Vector3(60f, 3.58f, 74f) })
                Troffer(parent, at, new Color(.87f, .86f, .7f), 1.8f, 9f, seed++, at.x == 42f, art);
            foreach (Vector3 at in new[] { new Vector3(32f, 7.58f, 58f), new Vector3(46f, 7.58f, 65f), new Vector3(46f, 7.58f, 75f), new Vector3(59f, 7.58f, 58f) })
                Troffer(parent, at, new Color(.92f, .8f, .58f), 1.65f, 9f, seed++, false, art);
            foreach (Vector3 at in new[] { new Vector3(55f, -.65f, 83f), new Vector3(63f, -.65f, 89f) })
                Troffer(parent, at, new Color(.95f, .66f, .39f), 1.6f, 7f, seed++, false, art);

            Troffer(parent, new Vector3(46f, 3.58f, 25f), new Color(.93f, .83f, .66f), 1.6f, 7f, seed++, false, art);
            Troffer(parent, new Vector3(46f, 3.58f, 48f), hall, 1.4f, 7f, seed++, false, art);
            Light stair = Spot(parent, "MainStairLight", new Vector3(46f, 7.5f, 40f), new Color(.94f, .79f, .55f), 2.1f, 10f, 78f, true);
            Box("StairBulkhead", stair.transform, new Vector3(0f, 0f, -.06f), new Vector3(.5f, .22f, .14f), art.MetalSteel, false);
            stair.gameObject.AddComponent<LightFixture>().EditorSetup(stair, null, false, false, 2198);
            Light eastStair = Spot(parent, "EastStairLight", new Vector3(66f, 7.5f, 46f), hall, 1.7f, 9f, 75f, false);
            eastStair.gameObject.AddComponent<LightFixture>().EditorSetup(eastStair, null, false, false, 2199);
        }

        private static void Troffer(Transform parent, Vector3 at, Color colour, float intensity, float range, int seed, bool dead, BurleySchoolArt art)
        {
            Transform holder = Group($"Fluorescent_{seed}", parent);
            holder.position = at;
            Box("PressedSteelChassis", holder, Vector3.zero, new Vector3(1.44f, .1f, .46f), art.MetalSteel, false);
            Box("Reflector", holder, new Vector3(0f, -.058f, 0f), new Vector3(1.3f, .018f, .38f), art.Limestone, false);
            Renderer panel = null;
            foreach (float z in new[] { -.12f, .12f })
            {
                GameObject tube = Primitive(PrimitiveType.Cylinder, "FluorescentTube", holder, new Vector3(0f, -.09f, z), new Vector3(.045f, .61f, .045f), art.FluorescentGlow, false);
                tube.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                panel = tube.GetComponent<Renderer>();
                foreach (float x in new[] { -.65f, .65f })
                    Box("CeramicSocket", holder, new Vector3(x, -.08f, z), new Vector3(.065f, .09f, .075f), art.Limestone, false);
            }
            foreach (float x in new[] { -.53f, .53f })
                Box("GuardCrossbar", holder, new Vector3(x, -.14f, 0f), new Vector3(.022f, .025f, .43f), art.MetalSteel, false);
            Light lamp = Spot(holder, "Lamp", at + Vector3.down * .13f, colour, intensity, range, 100f, false);
            holder.gameObject.AddComponent<LightFixture>().EditorSetup(lamp, panel, seed % 8 == 0, dead, seed, art.FluorescentGlow.GetColor("_EmissionColor"));
        }

        private static void BuildGymHighBays(Transform parent, BurleySchoolArt art)
        {
            for (int row = 0; row < 3; row++)
            for (int col = 0; col < 2; col++)
            {
                int index = row * 2 + col;
                Transform fixture = Group($"SuspendedHighBay_{index}", parent);
                fixture.position = new Vector3(77f + col * 14f, 6.9f, 29f + row * 14.5f);
                Box("SuspensionRod", fixture, new Vector3(0f, .53f, 0f), new Vector3(.025f, 1.05f, .025f), art.MetalSteel, false);
                Primitive(PrimitiveType.Cylinder, "BallastCan", fixture, new Vector3(0f, .1f, 0f), new Vector3(.34f, .2f, .34f), art.MetalSteel, false);
                Primitive(PrimitiveType.Cylinder, "ReflectorSkirt", fixture, new Vector3(0f, -.13f, 0f), new Vector3(.8f, .075f, .8f), art.MetalSteel, false);
                GameObject bulb = Primitive(PrimitiveType.Sphere, "ArcBulb", fixture, new Vector3(0f, -.21f, 0f), new Vector3(.28f, .2f, .28f), art.FluorescentGlow, false);
                foreach (float side in new[] { -1f, 1f })
                    Box("WireGuard", fixture, new Vector3(side * .28f, -.25f, 0f), new Vector3(.018f, .18f, .5f), art.MetalSteel, false);
                Light lamp = Spot(fixture, "Lamp", fixture.position + Vector3.down * .26f, new Color(.78f, .86f, .95f), 3f, 12f, 88f, index == 1);
                fixture.gameObject.AddComponent<LightFixture>().EditorSetup(lamp, bulb.GetComponent<Renderer>(), index == 3, index == 4, 2051 + index, art.FluorescentGlow.GetColor("_EmissionColor"));
            }
        }

        private static void BuildAuditorium(Transform parent, BurleySchoolArt art)
        {
            int seed = 2200;
            foreach (Vector3 at in new[] { new Vector3(6f, 6.8f, 28f), new Vector3(18f, 6.8f, 28f), new Vector3(6f, 6.8f, 41f), new Vector3(18f, 6.8f, 41f) })
            {
                Transform house = Group("TheatreHousePendant", parent);
                house.position = at;
                Box("DropRod", house, new Vector3(0f, .6f, 0f), new Vector3(.025f, 1.2f, .025f), art.MetalSteel, false);
                Primitive(PrimitiveType.Cylinder, "BronzeLantern", house, Vector3.zero, new Vector3(.46f, .26f, .46f), art.GoldAccent, false);
                GameObject lens = Primitive(PrimitiveType.Cylinder, "WarmLens", house, new Vector3(0f, -.27f, 0f), new Vector3(.36f, .025f, .36f), art.LampSodium, false);
                Light light = Spot(house, "Lamp", at + Vector3.down * .31f, new Color(1f, .67f, .36f), 2.25f, 12f, 87f, false);
                house.gameObject.AddComponent<LightFixture>().EditorSetup(light, lens.GetComponent<Renderer>(), false, false, seed++, art.LampSodium.GetColor("_EmissionColor"));
            }
            Transform stage = Group("StageWash", parent);
            stage.position = new Vector3(12f, 6.7f, 45f);
            stage.rotation = Quaternion.LookRotation(new Vector3(0f, -5.2f, 7f));
            Box("TheatreSpotBarrel", stage, Vector3.zero, new Vector3(.44f, .44f, .64f), art.MetalSteel, false);
            GameObject glass = Box("AmberLens", stage, new Vector3(0f, 0f, .34f), new Vector3(.34f, .34f, .03f), art.LampSodium, false);
            Light wash = stage.gameObject.AddComponent<Light>();
            wash.type = LightType.Spot; wash.range = 15f; wash.spotAngle = 62f; wash.innerSpotAngle = 30f;
            wash.intensity = 3.2f; wash.color = new Color(1f, .72f, .46f); wash.shadows = LightShadows.Soft; wash.shadowStrength = .68f;
            stage.gameObject.AddComponent<LightFixture>().EditorSetup(wash, glass.GetComponent<Renderer>(), false, false, seed, art.LampSodium.GetColor("_EmissionColor"));
        }

        private static void BuildExitSigns(Transform parent, BurleySchoolArt art)
        {
            (Vector3 at, float yaw, string text)[] points =
            {
                (new Vector3(46f, 2.78f, 20.3f), 180f, "EXIT"),
                (new Vector3(12f, 2.78f, 20.3f), 180f, "EXIT"),
                (new Vector3(82f, 2.78f, 67.7f), 0f, "EXIT"),
                (new Vector3(62f, 2.78f, 91.7f), 0f, "EXIT"),
                (new Vector3(46f, 6.75f, 44.7f), 0f, "EXIT  •  STAIRS"),
                (new Vector3(66f, 6.75f, 50.7f), 0f, "EXIT  •  STAIRS")
            };
            for (int i = 0; i < points.Length; i++)
            {
                Transform sign = Group($"ExitSign_{i}", parent);
                sign.SetPositionAndRotation(points[i].at, Quaternion.Euler(0f, points[i].yaw, 0f));
                Box("Chassis", sign, Vector3.zero, new Vector3(.94f, .32f, .12f), art.MetalSteel, false);
                Box("BatteryCase", sign, new Vector3(0f, .2f, 0f), new Vector3(.3f, .1f, .12f), art.MetalSteel, false);
                Box("EmergencyGlow", sign, new Vector3(0f, 0f, -.065f), new Vector3(.86f, .25f, .02f), art.ExitGlow, false);
                Label(sign, "ExitText", points[i].text, new Vector3(0f, 0f, -.08f), Quaternion.identity, i < 4 ? .1f : .065f, new Color(.92f, 1f, .91f));
            }
        }

        private static void BuildExteriorLamps(Transform parent, BurleySchoolArt art)
        {
            Rect lot = L.ParkingLot;
            Vector3[] points =
            {
                new(lot.xMin + 1f, 0f, lot.yMin + 1f), new(lot.xMax - 1f, 0f, lot.yMin + 1f),
                new(lot.xMin + 1f, 0f, lot.yMax - 1f), new(lot.xMax - 1f, 0f, lot.yMax - 1f)
            };
            for (int i = 0; i < points.Length; i++)
            {
                Transform pole = Group($"ParkingLotPole_{i}", parent);
                pole.position = points[i];
                Box("ConcreteFooting", pole, new Vector3(0f, .24f, 0f), new Vector3(.48f, .48f, .48f), art.Concrete);
                Box("SteelPost", pole, new Vector3(0f, 3.96f, 0f), new Vector3(.13f, 7.55f, .13f), art.MetalSteel);
                Box("LampArm", pole, new Vector3(0f, 7.6f, .48f), new Vector3(.1f, .1f, 1.0f), art.MetalSteel, false);
                Box("ShoeBoxHousing", pole, new Vector3(0f, 7.55f, .95f), new Vector3(.65f, .2f, .85f), art.MetalSteel, false);
                Box("SodiumLens", pole, new Vector3(0f, 7.43f, .95f), new Vector3(.53f, .035f, .7f), art.LampSodium, false);
                Spot(pole, "ParkingLight", pole.position + new Vector3(0f, 7.4f, .95f), new Color(1f, .75f, .44f), 2.2f, 17f, 105f, false);
            }
            foreach (float x in new[] { 42f, 50f })
            {
                Transform lamp = Group("EntranceWallLamp", parent);
                lamp.position = new Vector3(x, 3f, 19.73f);
                Box("Backplate", lamp, Vector3.zero, new Vector3(.18f, .35f, .08f), art.MetalSteel, false);
                Box("FrostedLantern", lamp, new Vector3(0f, -.1f, -.15f), new Vector3(.23f, .35f, .22f), art.LampSodium, false);
                Spot(lamp, "EntrancePool", lamp.position + new Vector3(0f, -.29f, -.16f), new Color(1f, .74f, .42f), 1.4f, 6f, 95f, false);
            }
        }

        private static Light Spot(Transform parent, string name, Vector3 worldPosition, Color color, float intensity, float range, float angle, bool shadow)
        {
            Transform holder = Group(name, parent);
            holder.position = worldPosition;
            holder.rotation = Quaternion.Euler(90f, 0f, 0f);
            Light light = holder.gameObject.AddComponent<Light>();
            light.type = LightType.Spot; light.color = color; light.intensity = intensity;
            light.range = range; light.spotAngle = angle; light.innerSpotAngle = angle * .55f;
            light.shadows = shadow ? LightShadows.Soft : LightShadows.None;
            light.shadowStrength = .65f; light.shadowBias = .07f; light.shadowNormalBias = .3f;
            light.renderMode = LightRenderMode.Auto;
            return light;
        }

        private static void BuildReflections(Transform parent)
        {
            (string name, Vector3 at, Vector3 size)[] rooms =
            {
                ("Auditorium", new Vector3(12f, 2f, 38f), new Vector3(23f, 7f, 35f)),
                ("GroundAcademic", new Vector3(46f, 1.8f, 36f), new Vector3(43f, 3.4f, 31f)),
                ("UpperAcademic", new Vector3(46f, 5.8f, 36f), new Vector3(43f, 3.4f, 31f)),
                ("Library", new Vector3(46f, 5.8f, 66f), new Vector3(43f, 3.4f, 27f)),
                ("Gymnasium", new Vector3(84f, 2.3f, 44f), new Vector3(31f, 7f, 47f)),
                ("Boiler", new Vector3(56f, -1.8f, 86f), new Vector3(23f, 3.1f, 11f))
            };
            foreach (var room in rooms)
            {
                Transform at = Group(room.name + "Reflection", parent);
                at.position = room.at;
                var probe = at.gameObject.AddComponent<ReflectionProbe>();
                probe.mode = ReflectionProbeMode.Realtime;
                probe.refreshMode = ReflectionProbeRefreshMode.OnAwake;
                probe.timeSlicingMode = ReflectionProbeTimeSlicingMode.IndividualFaces;
                probe.resolution = 64; probe.size = room.size; probe.center = Vector3.zero;
                probe.boxProjection = true; probe.blendDistance = 1.25f; probe.intensity = .65f;
                probe.cullingMask = LayerMask.GetMask("Default");
                probe.nearClipPlane = .15f; probe.farClipPlane = 55f;
                probe.clearFlags = ReflectionProbeClearFlags.SolidColor;
                probe.backgroundColor = new Color(.12f, .15f, .19f);
            }
        }
    }
}
