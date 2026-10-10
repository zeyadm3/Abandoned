using Abandoned.Extraction;
using UnityEditor;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;

namespace Abandoned.EditorTools
{
    /// <summary>0.19.1 replacement worker: RunHires on the RunState prefab, tunables, and the agency radio beside the truck's ignition.</summary>
    public static class Release0191
    {
        [MenuItem("Tools/Abandoned/Mall/Add Agency Radio (0.19.1)")]
        public static void Author()
        {
            ReleaseKit.Config<BonusConfig>(ReleaseKit.ExtractionResources, BonusConfig.ResourcePath);
            NetworkContentBuilder.CreateRunStatePrefab();
            Transform mall = MallSites.OpenMall(out var scene);
            if (mall == null) return;
            TruckCargo truck = Object.FindAnyObjectByType<TruckCargo>(FindObjectsInactive.Include);
            if (truck == null || truck.Ignition == null)
            {
                Debug.LogWarning("[Hire0191] No truck ignition found; the agency radio was not placed.");
                return;
            }
            Transform old = truck.Ignition.parent.Find("AgencyRadio");
            if (old != null) Object.DestroyImmediate(old.gameObject);
            Transform radio = Group("AgencyRadio", truck.Ignition.parent);
            radio.SetPositionAndRotation(truck.Ignition.position + truck.Ignition.right * 0.45f, truck.Ignition.rotation);
            Box("Body", radio, Vector3.zero, new Vector3(0.2f, 0.13f, 0.09f), SalvageMaterials.Get("Plastic"));
            Box("Dial", radio, new Vector3(0.05f, 0.02f, 0.05f), new Vector3(0.05f, 0.05f, 0.02f), SalvageMaterials.Get("Safety"), false);
            Primitive(PrimitiveType.Cylinder, "Antenna", radio, new Vector3(-0.07f, 0.16f, 0f), new Vector3(0.01f, 0.1f, 0.01f), SalvageMaterials.Get("Metal"), false);
            radio.gameObject.AddComponent<HireRadio>();
            Debug.Log($"[Hire0191] Agency radio beside the ignition at {radio.position}.");
            MallSites.SaveMall(scene, "Hire0191");
        }

        public static void AuthorAndExit() => ReleaseKit.RunAndExit(Author);
    }
}
