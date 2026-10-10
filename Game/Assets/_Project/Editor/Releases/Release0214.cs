using Abandoned.Extraction;
using UnityEditor;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// 0.21.4 truck interior: gauges and a quota lamp in the cab's binnacle behind the wheel, a dome light, and a
    /// horn button beside the ignition. Added under the saved truck; its 0.15.7b art is untouched.
    /// </summary>
    public static class Release0214
    {
        public const string GroupName = "_CAB_0214";

        [MenuItem("Tools/Abandoned/Mall/Truck Cab Instruments (0.21.4)")]
        public static void Author()
        {
            NetworkContentBuilder.CreateRunStatePrefab();
            SoundBankBuilder.CreateMissing();
            Transform mall = MallSites.OpenMall(out var scene);
            if (mall == null) return;
            Transform art = null;
            foreach (Transform t in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (t.name == CollectionTruck0157.GroupName) art = t;
            TruckCargo truck = Object.FindAnyObjectByType<TruckCargo>(FindObjectsInactive.Include);
            if (art == null || truck == null)
            {
                Debug.LogWarning("[Cab0214] The 0.15.7b truck wasn't found; nothing added.");
                return;
            }
            Transform truckRoot = art.parent;
            Transform old = truckRoot.Find(GroupName);
            if (old != null) Object.DestroyImmediate(old.gameObject);
            Transform root = Group(GroupName, truckRoot);
            Material dark = SalvageMaterials.Get("Plastic"), face = SalvageMaterials.Get("Paper"), red = SalvageMaterials.Get("Safety");
            Box("Binnacle", root, new Vector3(-0.63f, 1.82f, 4.53f), new Vector3(0.55f, 0.18f, 0.08f), dark, false);
            Transform haul = Dial(root, "HaulGauge", new Vector3(-0.78f, 1.82f, 4.485f), face, red);
            Transform fill = Dial(root, "CargoGauge", new Vector3(-0.48f, 1.82f, 4.485f), face, red);
            GameObject lamp = Primitive(PrimitiveType.Sphere, "QuotaLamp", root, new Vector3(-0.63f, 1.86f, 4.48f), Vector3.one * 0.03f,
                GetMaterial("Greybox_QuotaLamp", new Color(0.6f, 0.1f, 0.05f)), false);
            var domeObject = new GameObject("DomeLight");
            domeObject.transform.SetParent(root, false);
            domeObject.transform.localPosition = new Vector3(0f, 2.5f, 3.8f);
            var dome = domeObject.AddComponent<Light>();
            dome.type = LightType.Point;
            dome.range = 3f;
            dome.intensity = 1.6f;
            dome.color = new Color(1f, 0.9f, 0.7f);
            dome.shadows = LightShadows.None;
            dome.enabled = false;
            // The horn button rides on the ignition panel, where the crew actually stands.
            GameObject button = Primitive(PrimitiveType.Cylinder, "HornButton", root, Vector3.zero, new Vector3(0.09f, 0.02f, 0.09f), red, true);
            button.transform.position = truck.Ignition.position - truck.Ignition.right * 0.45f;
            button.transform.rotation = truck.Ignition.rotation * Quaternion.Euler(90f, 0f, 0f);
            root.gameObject.AddComponent<TruckDashboard>().EditorSetup(haul, fill, lamp.GetComponent<Renderer>(), dome);
            Debug.Log("[Cab0214] Cab gauges, quota lamp, dome light and horn button added.");
            MallSites.SaveMall(scene, "Cab0214");
        }

        public static void AuthorAndExit() => ReleaseKit.RunAndExit(Author);

        // A round dial facing the driver (-Z), with a needle that turns about the dial's own Z.
        private static Transform Dial(Transform parent, string name, Vector3 at, Material face, Material needle)
        {
            Transform dial = Group(name, parent);
            dial.localPosition = at;
            Primitive(PrimitiveType.Cylinder, "Face", dial, Vector3.zero, new Vector3(0.12f, 0.004f, 0.12f), face, false)
                .transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            Transform pivot = Group("Needle", dial);
            pivot.localPosition = new Vector3(0f, 0f, -0.006f);
            Box("Hand", pivot, new Vector3(0f, 0.025f, 0f), new Vector3(0.005f, 0.05f, 0.002f), needle, false);
            return pivot;
        }
    }
}
