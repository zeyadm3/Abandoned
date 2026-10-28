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
            levels.arraySize = 1;
            levels.GetArrayElementAtIndex(0).stringValue = "Hospital";
            data.FindProperty("<Building>k__BackingField").boundsValue = HospitalLayout.Bounds;
            data.FindProperty("<ArrivalFallback>k__BackingField").vector3Value = new Vector3(28f, 0f, 10f);
            data.FindProperty("<ArrivalFigure>k__BackingField").vector3Value = new Vector3(28f, 4f, 22f);
            data.FindProperty("<ArrivalDoor>k__BackingField").vector3Value = new Vector3(34f, 4f, 22f);
            data.FindProperty("<RoarPoint>k__BackingField").vector3Value = new Vector3(18f, - 2f, 38f);
            data.FindProperty("<SectorCell>k__BackingField").vector3Value = new Vector3(20f, 100f, 24f);
            data.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(config);
            HospitalLighting.Build(root, floors);
            Vector3 [] panels =
            {
                new(4, 0, 8.25f),
                new(22, 0, .25f),
                new(55.5f, 0, 6),
                new(4, 0, 47.75f),
                new(30, 0, 47.75f),
                new(55.5f, 0, 46)
            };
            float [] yaw =
            {
                0f,
                0f,
                270f,
                180f,
                180f,
                270f
            };
            for (int i = 0; i <panels.Length; i++)
            {
                Panel(root, kit, i, panels [i], yaw [i]);
            }
            foreach (int floor in new []
            {
                0, 1
            })
            {
                foreach (float x in new []
                {
                    22f, 34f
                })
                {
                    foreach (float z in new []
                    {
                        10f, 38f
                    })
                    {
                        Emergency(root, kit, new Vector3(x, floor * 4 + .4f, z), floors.At(new Vector3(x, floor * 4, z)));
                    }
                }
            }
            foreach (int floor in new []
            {
                2, 3
            })
            {
                foreach (float x in new []
                {
                    14f, 50f
                })
                {
                    Emergency(root, kit, new Vector3(x, floor * 4 + .4f, 38), floors.At(new Vector3(x, floor * 4, 38)));
                }
            }
            foreach (float x in new []
            {
                2f, 18f, 30f
            })
            {
                Emergency(root, kit, new Vector3(x, - 3.6f, 38), floors.At(new Vector3(x, - 4, 38)));
            }
            HospitalSound.Build(root);
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
    }
}
