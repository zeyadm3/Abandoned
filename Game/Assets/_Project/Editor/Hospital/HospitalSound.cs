using Abandoned.Audio;
using Abandoned.EditorTools;
using Abandoned.Structure;
using UnityEngine;
namespace Abandoned.Hospital
{
    public static class HospitalSound
    {
        public static void Build(Transform parent)
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
            Transform [] monitors = Points("Monitor", new Vector3(6, 1.6f, 26), new Vector3(10, 1.6f, 30), new Vector3(6, 5.6f, 28), new Vector3(6, 9.6f, 44));
            var power = new LightFixture [monitors.Length];
            LightFixture [] fixtures = parent.GetComponentsInChildren<LightFixture>();
            for (int i = 0; i <monitors.Length; i++)
            {
                float nearest = float.MaxValue;
                foreach (LightFixture fixture in fixtures)
                {
                    float distance = Vector3.Distance(monitors [i].position, fixture.transform.position);
                    if (distance >= nearest)
                    {
                        continue;
                    }
                    nearest = distance;
                    power [i] = fixture;
                }
            }
            var settings = SerializedWiring.LoadOrCreateAsset<HospitalSoundConfig>(HospitalLoot.Data + "/HospitalSound.asset");
            root.gameObject.AddComponent<HospitalSoundscape>().EditorSetup(settings, monitors, power, Points("PA", new Vector3(28, 6.5f, 10), new Vector3(34, 6.5f, 38)), Points("Gurney", new Vector3(34, .5f, 18), new Vector3(34, .5f, 38)), Points("MorgueDrip", new Vector3(6, - 2.5f, 30), new Vector3(12, - 2.5f, 44)));
        }
    }
}
