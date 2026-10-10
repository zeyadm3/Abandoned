using Abandoned.Equipment;
using UnityEditor;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>0.17.7 glow sticks and flares: two consumables and their thrown network prefabs.</summary>
    public static class Release0177
    {
        [MenuItem("Tools/Abandoned/Content/Glow Sticks and Flares (0.17.7)")]
        public static void Author()
        {
            ReleaseKit.RegisterGear(EquipmentKind.GlowSticks, Prefab("GlowStick", new Color(0.35f, 1f, 0.45f), 2.2f, 6f, 600f, false, new Vector3(0.03f, 0.18f, 0.03f)));
            ReleaseKit.RegisterGear(EquipmentKind.Flares, Prefab("Flare", new Color(1f, 0.25f, 0.15f), 7f, 14f, 90f, true, new Vector3(0.045f, 0.24f, 0.045f)));
            ReleaseKit.Gear();
        }

        public static void AuthorAndExit() => ReleaseKit.RunAndExit(Author);

        private static GameObject Prefab(string name, Color colour, float intensity, float range, float life, bool flare, Vector3 size)
        {
            string path = $"{ReleaseKit.GearPrefabFolder}/{name}.prefab";
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null && existing.GetComponent<ThrownLight>() != null) return existing;
            PolishAssets.EnsureFolder(ReleaseKit.GearPrefabFolder);
            var root = new GameObject(name);
            root.AddComponent<Unity.Netcode.NetworkObject>().DontDestroyWithOwner = true;
            root.AddComponent<Unity.Netcode.Components.NetworkTransform>();
            var body = root.AddComponent<Rigidbody>();
            body.mass = 0.3f;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            var capsule = root.AddComponent<CapsuleCollider>();
            capsule.radius = size.x;
            capsule.height = size.y;
            capsule.direction = 2;
            Material glow = GreyboxFactory.GetMaterial($"Greybox_{name}", colour);
            glow.EnableKeyword("_EMISSION");
            glow.SetColor("_EmissionColor", colour * 3f);
            GreyboxFactory.Primitive(PrimitiveType.Capsule, "Body", root.transform, Vector3.zero, new Vector3(size.x * 2f, size.y * 0.5f, size.x * 2f), glow, false)
                .transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            var lampObject = new GameObject("Light");
            lampObject.transform.SetParent(root.transform, false);
            lampObject.transform.localPosition = Vector3.up * 0.1f;
            var lamp = lampObject.AddComponent<Light>();
            lamp.type = LightType.Point;
            lamp.color = colour;
            lamp.intensity = intensity;
            lamp.range = range;
            lamp.shadows = LightShadows.None;
            root.AddComponent<ThrownLight>().EditorSetup(lamp, life, flare);
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            NetworkObjectIds.StampPrefab(prefab);
            return prefab;
        }
    }
}
