using Abandoned.Audio;
using Abandoned.Core;
using Abandoned.EditorTools;
using Abandoned.Extraction;
using Abandoned.Structure;
using UnityEditor;
using UnityEngine;
namespace Abandoned.Hospital
{
    public static class HospitalServices
    {
        public static HorrorConfig Build(Transform root, HospitalKit kit, HospitalStructure floors)
        {
            StationContext c = floors.Context;
            var config = SerializedWiring.LoadOrCreateAsset<HorrorConfig>(HospitalLoot.Data + "/HospitalHorror.asset");
            var data = new SerializedObject(config);
            SerializedProperty levels = data.FindProperty("levels");
            levels.arraySize = 2;
            levels.GetArrayElementAtIndex(0).stringValue = "Hospital";
            levels.GetArrayElementAtIndex(1).stringValue = "World";
            data.FindProperty("<Building>k__BackingField").boundsValue = HospitalLayout.Bounds;
            data.FindProperty("<ArrivalFallback>k__BackingField").vector3Value = new Vector3(30f, 0f, 14f);
            data.FindProperty("<ArrivalFigure>k__BackingField").vector3Value = new Vector3(30f, 4f, 26f);
            data.FindProperty("<ArrivalDoor>k__BackingField").vector3Value = new Vector3(32f, 4f, 26f);
            data.FindProperty("<RoarPoint>k__BackingField").vector3Value = new Vector3(18f, - 2f, 38f);
            data.FindProperty("<SectorCell>k__BackingField").vector3Value = new Vector3(20f, 100f, 24f);
            data.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(config);
            StationLighting.Build(root, c.Plan, c.Art, new Color(.8f, .94f, .87f), 17023);
            foreach (LightFixture fixture in root.GetComponentsInChildren<LightFixture>())
            {
                StructuralSection ceiling = floors.At(fixture.transform.position);
                if (ceiling != null && ceiling.CanCollapse)
                {
                    fixture.gameObject.AddComponent<SectionProp>().EditorSetup(ceiling);
                }
            }
            Vector3 [] panels =
            {
                new(3f, 0f, .3f),
                new(18f, 0f, .3f),
                new(51f, 0f, .3f),
                new(2f, 0f, 47.6f),
                new(18f, 0f, 47.6f),
                new(51f, 0f, 47.6f)
            };
            for (int i = 0; i <panels.Length; i++)
            {
                Panel(root, kit, i, panels [i], i <3 ? 0f : 180f);
            }
            foreach (int floor in new []
            {
                0, 1
            })
            {
                foreach (float z in new []
                {
                    14f, 34f, 46f
                })
                {
                    Emergency(root, kit, new Vector3(30f, floor * 4f + .4f, z), floors.At(new Vector3(30f, floor * 4f, z)));
                }
            }
            foreach (float x in new []
            {
                2f, 18f, 30f
            })
            {
                Emergency(root, kit, new Vector3(x, - 3.6f, 38f), floors.At(new Vector3(x, - 4f, 38f)));
            }
            c.Hiding.Closet("Reception", new Vector3(1.4f, 0f, 5f), 90f);
            c.Hiding.Cabinet("Emergency", new Vector3(14.5f, 0f, 25f), 270f);
            c.Hiding.Cabinet("Radiology", new Vector3(26.5f, 0f, 31f), 180f);
            c.Hiding.Cabinet("Pharmacy", new Vector3(40.5f, 0f, 29f), 180f);
            c.Hiding.Closet("Chapel", new Vector3(1.6f, 0f, 38.5f), 90f);
            Sound(root, floors);
            return config;
        }
        private static void Panel(Transform parent, HospitalKit kit, int sector, Vector3 at, float yaw)
        {
            Transform root = HospitalKit.Group("Breaker_" + sector, parent);
            root.SetPositionAndRotation(at, Quaternion.Euler(0f, yaw, 0f));
            kit.Box("Cabinet", root, new Vector3(0f, 1.4f, .12f), new Vector3(.7f, .9f, .25f), kit.Metal);
            Transform lever = HospitalKit.Group("Lever", root);
            lever.localPosition = new Vector3(.2f, 1.3f, .3f);
            kit.Box("Grip", lever, new Vector3(0f, .15f, 0f), new Vector3(.1f, .3f, .08f), kit.Accent, false);
            Renderer lamp = kit.Box("Indicator", root, new Vector3(- .2f, 1.7f, .27f), new Vector3(.07f, .07f, .04f), kit.Glow, false).GetComponent<Renderer>();
            root.gameObject.AddComponent<BreakerPanel>().EditorSetup(sector, lever, lamp);
            kit.Label(root,(sector + 1).ToString(), new Vector3(0f, 2f, .28f), 180f, .035f);
        }
        private static void Emergency(Transform parent, HospitalKit kit, Vector3 at, StructuralSection support)
        {
            Transform root = HospitalKit.Group("BatteryRouteLight", parent);
            root.position = at;
            kit.Box("BatteryHousing", root, Vector3.zero, new Vector3(.4f, .12f, .1f), kit.Metal, false);
            root.gameObject.AddComponent<PowerIndependentLight>();
            Light lamp = root.gameObject.AddComponent<Light>();
            lamp.type = LightType.Point;
            lamp.color = new Color(.4f, 1f, .55f);
            lamp.range = 6f;
            lamp.intensity = .7f;
            lamp.shadows = LightShadows.None;
            root.gameObject.AddComponent<HospitalEmergencyLight>().EditorSetup(lamp, support);
            if (support != null)
            {
                root.gameObject.AddComponent<SectionProp>().EditorSetup(support);
            }
        }
        private static void Sound(Transform parent, HospitalStructure floors)
        {
            Transform root = HospitalKit.Group("Soundscape", parent);
            Transform [] Points(string name, params Vector3 [] positions)
            {
                var points = new Transform [positions.Length];
                for (int i = 0; i <positions.Length; i++)
                {
                    points [i] = HospitalKit.Group(name + i, root);
                    points [i].position = positions [i];
                }
                return points;
            }
            Transform [] monitors = Points("Monitor", new Vector3(18.7f, 1.6f, 7.5f), new Vector3(6.8f, 1.6f, 19.5f), new Vector3(7f, 5.6f, 21f), new Vector3(3f, 5.6f, 39f));
            var power = new LightFixture [monitors.Length];
            LightFixture [] fixtures = parent.GetComponentsInChildren<LightFixture>();
            for (int i = 0; i <monitors.Length; i++)
            {
                float closest = float.MaxValue;
                foreach (LightFixture fixture in fixtures)
                {
                    float distance = Vector3.Distance(monitors [i].position, fixture.transform.position);
                    if (distance >= closest)
                    {
                        continue;
                    }
                    closest = distance;
                    power [i] = fixture;
                }
            }
            HospitalSoundConfig settings = SerializedWiring.LoadOrCreateAsset<HospitalSoundConfig>(HospitalLoot.Data + "/HospitalSound.asset");
            root.gameObject.AddComponent<HospitalSoundscape>().EditorSetup(settings, monitors, power, Points("PA", new Vector3(30f, 2.8f, 14f), new Vector3(30f, 6.8f, 34f)), Points("Gurney", new Vector3(4f, .5f, 14f), new Vector3(26f, .5f, 14f)), Points("MorgueDrip", new Vector3(6f, - 2.5f, 30f), new Vector3(12f, - 2.5f, 44f)));
        }
    }
}
