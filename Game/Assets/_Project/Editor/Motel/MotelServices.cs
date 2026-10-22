using Abandoned.Audio;
using Abandoned.Core;
using Abandoned.EditorTools;
using Abandoned.Extraction;
using Abandoned.Structure;
using UnityEditor;
using UnityEngine;
namespace Abandoned.Motel
{
    public static class MotelServices
    {
        public static HorrorConfig Build(Transform root, MotelKit k, MotelStructure floors)
        {
            var config = SerializedWiring.LoadOrCreateAsset<HorrorConfig>(MotelLoot.Data + "/MotelHorror.asset");
            var so = new SerializedObject(config);
            SerializedProperty levels = so.FindProperty("levels");
            levels.arraySize = 2;
            levels.GetArrayElementAtIndex(0).stringValue = "Motel";
            levels.GetArrayElementAtIndex(1).stringValue = "World";
            so.FindProperty("<Building>k__BackingField").boundsValue = MotelLayout.Bounds;
            so.FindProperty("<ArrivalFallback>k__BackingField").vector3Value = new Vector3(32, 0, 18);
            so.FindProperty("<ArrivalFigure>k__BackingField").vector3Value = new Vector3(52, 4, 14);
            so.FindProperty("<ArrivalDoor>k__BackingField").vector3Value = new Vector3(60.5f, 4, 16);
            so.FindProperty("<RoarPoint>k__BackingField").vector3Value = new Vector3(32, 6, 22);
            // Tall cells keep upstairs rooms on the same wing circuit as the room below.
            so.FindProperty("<SectorCell>k__BackingField").vector3Value = new Vector3(24, 100, 24);
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(config);
            Transform services = MotelKit.Group("Services", root);
            int id = 0;
            for (int level = 0; level <2; level++)
            {
                for (int room = 0; room <8; room++)
                {
                    float x = MotelLayout.RoomX(room) + 3.5f;
                    foreach (float z in new []
                    {
                        20f, 26f
                    })
                    {
                        Fixture(services, k, new Vector3(x, level * 4 + 3.3f, z), id++, level == 0 ? floors.At(new Vector3(x, 4, z)) : null, false);
                    }
                }
                for (float x = 4; x <64; x += 8)
                {
                    Fixture(services, k, new Vector3(x, level * 4 + 3.25f, 14), id++, level == 0 ? floors.At(new Vector3(x, 4, 14)) : null, true);
                }
                foreach (float x in new []
                {
                    30f, 34f
                })
                {
                    foreach (float z in new []
                    {
                        19f, 25f
                    })
                    {
                        Fixture(services, k, new Vector3(x, level * 4 + 3.3f, z), id++, level == 0 ? floors.At(new Vector3(x, 4, z)) : null, false);
                    }
                }
                foreach (float x in new []
                {
                    2f, 30f, 62f
                })
                {
                    Emergency(services, k, new Vector3(x, level * 4 + .4f, 15f), floors.At(new Vector3(x, level * 4, 15f)));
                }
            }
            foreach (float x in new []
            {
                84f, 92f, 100f
            })
            {
                foreach (float z in new []
                {
                    6f, 16f
                })
                {
                    Fixture(services, k, new Vector3(x, 3.3f, z), id++, null, false);
                }
            }
            foreach (float x in new []
            {
                78f, 82f, 86f
            })
            {
                foreach (float z in new []
                {
                    32f, 37f
                })
                {
                    Fixture(services, k, new Vector3(x, 3.3f, z), id++, null, false);
                }
            }
            foreach (float x in new []
            {
                7f, 59f
            })
            {
                Fixture(services, k, new Vector3(x, 2.65f, 11.2f), id++, null, true);
            }
            Emergency(services, k, new Vector3(92f, .4f, 2f), floors.At(new Vector3(92f, 0f, 2f)));
            Emergency(services, k, new Vector3(82f, .4f, 29f), floors.At(new Vector3(82f, 0f, 29f)));
            Fixture(services, k, new Vector3(30f, 3.3f, 20.8f), id++, floors.At(new Vector3(30f, 4f, 20.8f)), false);
            Vector3 [] panels =
            {
                new(1f, 0f, 15.65f),
                new(28.5f, 0f, 15.65f),
                new(63f, 0f, 15.65f),
                new(1f, 0f, 28.35f),
                new(28.5f, 0f, 28.35f),
                new(63f, 0f, 28.35f)
            };
            for (int sector = 0; sector <panels.Length; sector++)
            {
                Breaker(services, k, sector, panels [sector], sector <3 ? 0f : 180f);
            }
            Transform sign = root.Find("Courtyard/RoadSign");
            Transform neonSource = MotelKit.Group("NeonLight", sign);
            neonSource.localPosition = new Vector3(0f, 5.4f, 0f);
            Light neon = neonSource.gameObject.AddComponent<Light>();
            neon.type = LightType.Point;
            neon.range = 11f;
            neon.intensity = 1.2f;
            neon.color = new Color(1f, .25f, .08f);
            neon.shadows = LightShadows.None;
            sign.gameObject.AddComponent<LightFixture>().EditorSetup(neon, sign.Find("NeonBar").GetComponent<Renderer>(), true, false, 17600);
            MotelSound.Build(root, floors);
            return config;
        }
        private static void Fixture(Transform parent, MotelKit k, Vector3 at, int id, StructuralSection ceiling, bool gallery)
        {
            Transform t = MotelKit.Group("MotelFixture_" + id, parent);
            t.localPosition = at;
            k.Box("Suspension", t, new Vector3(0f, .25f, 0f), new Vector3(.025f, .4f, .025f), k.Metal, false);
            k.Box("Housing", t, Vector3.zero, new Vector3(gallery ? .6f : .8f, .1f, .35f), k.Metal, false);
            Renderer lens = k.Box("Diffuser", t, new Vector3(0, - .07f, 0), new Vector3(gallery ? .5f : .7f, .04f, .28f), k.Glow, false).GetComponent<Renderer>();
            Transform aim = MotelKit.Group("Light", t);
            aim.localRotation = Quaternion.Euler(90, 0, 0);
            aim.localPosition = Vector3.down * .12f;
            Light lamp = aim.gameObject.AddComponent<Light>();
            lamp.type = LightType.Spot;
            lamp.range = gallery ? 10f : 8f;
            lamp.spotAngle = 105f;
            lamp.intensity = gallery ? 7f : 6f;
            lamp.color = gallery ? new Color(1, .65f, .35f) : new Color(1, .83f, .61f);
            lamp.shadows = id == 20 || id == 44 ? LightShadows.Soft : LightShadows.None;
            t.gameObject.AddComponent<LightFixture>().EditorSetup(lamp, lens, id % 9 == 4, id % 13 == 6, 17500 + id);
            k.Attach(t, ceiling);
        }
        private static void Breaker(Transform parent, MotelKit k, int sector, Vector3 at, float yaw)
        {
            Transform t = MotelKit.Group("Circuit_" + sector, parent);
            t.SetPositionAndRotation(at, Quaternion.Euler(0f, yaw, 0f));
            k.Box("Cabinet", t, new Vector3(0, 1.4f, .12f), new Vector3(.6f, .8f, .25f), k.Metal);
            Transform handle = MotelKit.Group("Lever", t);
            handle.localPosition = new Vector3(.16f, 1.3f, - .06f);
            k.Box("Grip", handle, new Vector3(0, .14f, 0), new Vector3(.08f, .28f, .08f), k.Accent, false);
            Renderer lamp = k.Box("Indicator", t, new Vector3(- .16f, 1.65f, - .03f), new Vector3(.06f, .06f, .04f), k.Glow, false).GetComponent<Renderer>();
            t.gameObject.AddComponent<BreakerPanel>().EditorSetup(sector, handle, lamp);
            k.Label(t,(sector + 1).ToString(), new Vector3(0, 1.94f, - .03f), size : .025f);
        }
        private static void Emergency(Transform parent, MotelKit k, Vector3 at, StructuralSection support)
        {
            Transform t = MotelKit.Group("BatteryRouteLight", parent);
            t.position = at;
            k.Box("Housing", t, Vector3.zero, new Vector3(.4f, .12f, .12f), k.Metal, false);
            t.gameObject.AddComponent<PowerIndependentLight>();
            Light lamp = t.gameObject.AddComponent<Light>();
            lamp.type = LightType.Point;
            lamp.range = 6f;
            lamp.intensity = .7f;
            lamp.color = new Color(.4f, 1f, .55f);
            lamp.shadows = LightShadows.None;
            t.gameObject.AddComponent<MotelEmergencyLight>().EditorSetup(lamp, support);
            k.Attach(t, support);
        }
    }
}
