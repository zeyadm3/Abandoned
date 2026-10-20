using Abandoned.Equipment;
using UnityEditor;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;

namespace Abandoned.EditorTools
{
    /// <summary>0.20.0 portable floodlight: the catalog item, its icon, and the tripod lamp's network prefab.</summary>
    public static class Release0200
    {
        public const string PrefabPath = ReleaseKit.GearPrefabFolder + "/Floodlight.prefab";

        [MenuItem(ToolMenu.Features + "0.20.0 Portable Floodlight", priority = ToolMenu.FeaturesP + 15)]
        public static void Author()
        {
            ReleaseKit.RegisterGear(EquipmentKind.Floodlight, Prefab());
            ReleaseKit.Gear();
        }

        public static void AuthorAndExit() => ReleaseKit.RunAndExit(Author);

        private static GameObject Prefab()
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (existing != null && existing.GetComponent<PlacedFloodlight>() != null) return existing;
            PolishAssets.EnsureFolder(ReleaseKit.GearPrefabFolder);
            var root = new GameObject("Floodlight");
            root.AddComponent<Unity.Netcode.NetworkObject>().DontDestroyWithOwner = true;
            Material yellow = SalvageMaterials.Get("Safety"), steel = SalvageMaterials.Get("Metal");
            for (int i = 0; i < 3; i++)
            {
                float a = i * 120f * Mathf.Deg2Rad;
                Vector3 foot = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * 0.45f, top = Vector3.up * 1.5f;
                GameObject leg = Primitive(PrimitiveType.Cylinder, "Leg", root.transform, (foot + top) * 0.5f, new Vector3(0.04f, Vector3.Distance(foot, top) * 0.5f, 0.04f), steel, false);
                leg.transform.localRotation = Quaternion.FromToRotation(Vector3.up, top - foot);
            }
            Box("Head", root.transform, new Vector3(0f, 1.6f, 0.05f), new Vector3(0.42f, 0.32f, 0.16f), yellow);
            Material glow = GetMaterial("Greybox_FloodlightGlass", new Color(1f, 0.95f, 0.8f));
            glow.EnableKeyword("_EMISSION");
            glow.SetColor("_EmissionColor", new Color(1f, 0.95f, 0.8f) * 4f);
            Box("Glass", root.transform, new Vector3(0f, 1.6f, 0.135f), new Vector3(0.36f, 0.26f, 0.01f), glow, false);
            var lampObject = new GameObject("Lamp");
            lampObject.transform.SetParent(root.transform, false);
            lampObject.transform.localPosition = new Vector3(0f, 1.6f, 0.2f);
            lampObject.transform.localRotation = Quaternion.Euler(12f, 0f, 0f);
            var lamp = lampObject.AddComponent<Light>();
            lamp.type = LightType.Spot;
            lamp.color = new Color(1f, 0.95f, 0.85f);
            lamp.shadows = LightShadows.None;
            var col = root.AddComponent<CapsuleCollider>();
            col.center = Vector3.up * 0.85f;
            col.height = 1.7f;
            col.radius = 0.3f;
            root.AddComponent<PlacedFloodlight>().EditorSetup(lamp);
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Object.DestroyImmediate(root);
            NetworkObjectIds.StampPrefab(prefab);
            return prefab;
        }
    }
}
