using Abandoned.Equipment;
using UnityEditor;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>0.17.1 crash mat: the catalog item, its icon and its network prefab (a static soft pad).</summary>
    public static class Release0171
    {
        public const string PrefabPath = ReleaseKit.GearPrefabFolder + "/CrashMat.prefab";

        [MenuItem(ToolMenu.Features + "0.17.1 Crash Mat", priority = ToolMenu.FeaturesP + 2)]
        public static void Author()
        {
            ReleaseKit.RegisterGear(EquipmentKind.CrashMat, Prefab());
            ReleaseKit.Gear();
        }

        public static void AuthorAndExit() => ReleaseKit.RunAndExit(Author);

        private static GameObject Prefab()
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (existing != null && existing.GetComponent<CrashMat>() != null) return existing;
            PolishAssets.EnsureFolder(ReleaseKit.GearPrefabFolder);
            var root = new GameObject("CrashMat");
            root.AddComponent<Unity.Netcode.NetworkObject>().DontDestroyWithOwner = true;
            Material blue = GreyboxFactory.GetMaterial("Greybox_CrashMat", new Color(0.18f, 0.36f, 0.72f));
            Material seam = GreyboxFactory.GetMaterial("Greybox_CrashMatSeam", new Color(0.1f, 0.12f, 0.2f));
            GreyboxFactory.Box("Pad", root.transform, new Vector3(0f, 0.175f, 0f), new Vector3(2.2f, 0.35f, 2.2f), blue);
            for (int i = -1; i <= 1; i++)
                GreyboxFactory.Box("Seam", root.transform, new Vector3(i * 0.73f, 0.352f, 0f), new Vector3(0.03f, 0.005f, 2.2f), seam, false);
            root.AddComponent<Core.SurfaceTag>().EditorSet(Core.SurfaceMaterial.Fabric);
            root.AddComponent<CrashMat>();
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Object.DestroyImmediate(root);
            NetworkObjectIds.StampPrefab(prefab);
            return prefab;
        }
    }
}
