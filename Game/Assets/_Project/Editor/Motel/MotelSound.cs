using Abandoned.Audio;
using Abandoned.EditorTools;
using Abandoned.Structure;
using UnityEngine;
namespace Abandoned.Motel
{
    public static class MotelSound
    {
        public static void Build(Transform parent, MotelStructure floors)
        {
            Transform root = MotelKit.Group("Soundscape", parent);
            Transform [] Points(string name, params Vector3 [] positions)
            {
                var points = new Transform [positions.Length];
                for (int i = 0; i <positions.Length; i++)
                {
                    points [i] = MotelKit.Group(name + i, root);
                    points [i].position = positions [i];
                }
                return points;
            }
            Transform [] powered = Points("PoweredMachine", new Vector3(66f, 5.4f, - 10f), new Vector3(6f, 1.3f, 11.25f), new Vector3(58f, 1.3f, 11.25f));
            Transform [] televisions = Points("BehindDoorTV", new Vector3(17f, 1.5f, 20f), new Vector3(53f, 5.5f, 20f), new Vector3(34f, 5.5f, 21f));
            LightFixture [] fixtures = parent.GetComponentsInChildren<LightFixture>();
            LightFixture [] PowerFor(Transform [] points)
            {
                var power = new LightFixture [points.Length];
                for (int i = 0; i <points.Length; i++)
                {
                    float closest = float.MaxValue;
                    foreach (LightFixture fixture in fixtures)
                    {
                        float distance = Vector3.Distance(points [i].position, fixture.transform.position);
                        if (distance >= closest)
                        {
                            continue;
                        }
                        closest = distance;
                        power [i] = fixture;
                    }
                }
                return power;
            }
            var supports = new StructuralSection [televisions.Length];
            for (int i = 0; i <televisions.Length; i++)
            {
                supports [i] = floors.At(televisions [i].position);
            }
            MotelSoundConfig config = SerializedWiring.LoadOrCreateAsset<MotelSoundConfig>(MotelLoot.Data + "/MotelSound.asset");
            root.gameObject.AddComponent<MotelSoundscape>().EditorSetup(config, powered, PowerFor(powered), televisions, PowerFor(televisions), supports, Points("GalleryWind", new Vector3(10f, 5f, 13f), new Vector3(54f, 5f, 13f)), Points("WaterDrip", new Vector3(18f, - .4f, 5f), new Vector3(10f, 1f, 26f), new Vector3(53f, 5f, 26f)));
        }
    }
}
