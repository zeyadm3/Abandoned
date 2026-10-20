using Abandoned.Equipment;
using Unity.Netcode.Components;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using static Abandoned.EditorTools.GreyboxFactory;

namespace Abandoned.EditorTools
{
    /// <summary>0.20.5 drone: the catalog item, its icon and the quadcopter's network prefab (owner-flown, with its own camera).</summary>
    public static class Release0205
    {
        public const string PrefabPath = ReleaseKit.GearPrefabFolder + "/Drone.prefab";

        [MenuItem(ToolMenu.Features + "0.20.5 Scout Drone", priority = ToolMenu.FeaturesP + 18)]
        public static void Author()
        {
            ReleaseKit.RegisterGear(EquipmentKind.Drone, Prefab());
            ReleaseKit.Gear();
        }

        public static void AuthorAndExit() => ReleaseKit.RunAndExit(Author);

        private static GameObject Prefab()
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (existing != null && existing.GetComponent<DronePilot>() != null) return existing;
            PolishAssets.EnsureFolder(ReleaseKit.GearPrefabFolder);
            var root = new GameObject("Drone");
            root.AddComponent<Unity.Netcode.NetworkObject>().DontDestroyWithOwner = false;
            var nt = root.AddComponent<NetworkTransform>();
            nt.AuthorityMode = NetworkTransform.AuthorityModes.Owner;
            nt.SyncScaleX = nt.SyncScaleY = nt.SyncScaleZ = false;
            nt.Interpolate = true;
            var body = root.AddComponent<Rigidbody>();
            body.isKinematic = true;
            root.AddComponent<SphereCollider>().radius = 0.25f;
            Material dark = SalvageMaterials.Get("Plastic"), steel = SalvageMaterials.Get("Metal");
            Box("Body", root.transform, Vector3.zero, new Vector3(0.22f, 0.08f, 0.3f), dark, false);
            Transform rotors = Group("Rotors", root.transform);
            foreach (Vector2 c in new[] { new Vector2(-0.2f, -0.2f), new Vector2(0.2f, -0.2f), new Vector2(-0.2f, 0.2f), new Vector2(0.2f, 0.2f) })
            {
                Box("Arm", root.transform, new Vector3(c.x * 0.5f, 0f, c.y * 0.5f), new Vector3(0.03f, 0.02f, 0.03f) + new Vector3(Mathf.Abs(c.x), 0f, Mathf.Abs(c.y)) * 0.7f, steel, false);
                Primitive(PrimitiveType.Cylinder, "Rotor", rotors, new Vector3(c.x, 0.05f, c.y), new Vector3(0.16f, 0.005f, 0.16f), steel, false);
            }
            var camObject = new GameObject("Lens");
            camObject.transform.SetParent(root.transform, false);
            camObject.transform.localPosition = new Vector3(0f, -0.02f, 0.16f);
            var lens = camObject.AddComponent<Camera>();
            lens.fieldOfView = 80f;
            lens.nearClipPlane = 0.05f;
            lens.enabled = false;
            UniversalAdditionalCameraData data = lens.GetUniversalAdditionalCameraData();
            data.renderShadows = true;
            root.AddComponent<DronePilot>().EditorSetup(lens, rotors);
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Object.DestroyImmediate(root);
            NetworkObjectIds.StampPrefab(prefab);
            return prefab;
        }
    }
}
