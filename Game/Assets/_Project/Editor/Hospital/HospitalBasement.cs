using Abandoned.EditorTools;
using UnityEngine;
using F = Abandoned.EditorTools.StationFurniture;
namespace Abandoned.Hospital
{
    public static class HospitalBasement
    {
        public static void Dress(StationContext c)
        {
            HospitalServiceProps.MorgueBank(c, "Morgue", new Vector3(5f, - 4f, 27f));
            HospitalServiceProps.MorgueBank(c, "Morgue", new Vector3(5f, - 4f, 32f));
            F.Counter(c.Props, "Morgue", new Vector3(13.5f, - 4f, 34.8f), 180f, 3f, 1f, c.Art.Chrome);
            F.Table(c.Props, "Autopsy", new Vector3(7f, - 4f, 44f), 0f, 1f, 2.6f, c.Art.Chrome);
            F.Sink(c.Props, "Autopsy", new Vector3(5f, - 4f, 41f), 90f);
            F.Counter(c.Props, "Autopsy", new Vector3(11f, - 4f, 47f), 180f, 6f, 1f, c.Art.Chrome);
            HospitalServiceProps.Boiler(c, "Boiler", new Vector3(23f, - 4f, 27.5f));
            HospitalServiceProps.Boiler(c, "Boiler", new Vector3(29f, - 4f, 27.5f));
            F.Table(c.Props, "Boiler", new Vector3(30.8f, - 4f, 32f), 270f, 2.2f, .7f, c.Art.Steel);
            foreach (float z in new []
            {
                42f, 45f
            })
            {
                F.Shelving(c.Props, "BasementStores", new Vector3(21f, - 4f, z), 90f, 2.4f,(int) z);
                F.Shelving(c.Props, "BasementStores", new Vector3(31f, - 4f, z), 270f, 2.4f,(int) z + 5);
            }
            c.Hiding.Cabinet("Morgue", new Vector3(14.6f, - 4f, 26f), 270f);
            c.Hiding.Closet("Boiler", new Vector3(30.5f, - 4f, 34f), 270f);
            c.Hiding.Locker("BasementStores", new Vector3(22f, - 4f, 46.2f), 180f);
            Transform pipes = c.Props.Hang("Boiler", "BoilerManifold", new Vector3(25f, - .7f, 29f), 0f);
            foreach (float offset in new []
            {
                - .4f, 0f, .4f
            })
            {
                StationProps.B(pipes, "SteamMain", new Vector3(0f, offset, 0f), new Vector3(12f, .12f, .12f), c.Art.Steel);
            }
        }
    }
}
