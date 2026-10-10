using System.Linq;
using Abandoned.Extraction;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using static Abandoned.EditorTools.GreyboxFactory;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// 0.16.6 vault drill: a floor safe against a bare wall in the ground-floor jewellery bay (zone "Jewelry",
    /// now Nordic Habitat's floor), or any ground-floor store if it has no room. Hollow, with a hinged door
    /// and a drill rig that shows while drilling.
    /// </summary>
    public static class Release0166
    {
        public const string GroupName = "_VAULT_0166";
        private static Vector3 Size = new(1.3f, 1.7f, 1.0f);
        // Tried in order until one fits: the stores are densely dressed.
        private static readonly Vector3[] Sizes = { new(1.3f, 1.7f, 1.0f), new(1.0f, 1.4f, 0.8f), new(0.85f, 1.2f, 0.7f) };

        [MenuItem("Tools/Abandoned/Mall/Add Vault Safe (0.16.6)")]
        public static void Author()
        {
            ReleaseKit.Config<VaultConfig>(ReleaseKit.ExtractionResources, VaultConfig.ResourcePath);
            NetworkContentBuilder.CreateRunStatePrefab();
            SoundBankBuilder.CreateMissing();
            Transform mall = MallSites.OpenMall(out var scene);
            if (mall == null) return;
            Build(mall);
            MallSites.SaveMall(scene, "Vault0166");
        }

        public static void AuthorAndExit() => ReleaseKit.RunAndExit(Author);

        public static void Build(Transform mall)
        {
            Transform root = MallSites.FreshGroup(mall, GroupName);
            MallSites.Site pick = default;
            bool found = false;
            foreach (Vector3 size in Sizes)
            {
                Size = size;
                var sites = MallSites.WallSites((t, f) => MallLayout.ZoneAt(t, f)?.Kind == MallLayout.Kind.Store, Size, 0f);
                if (sites.Count == 0) continue;
                pick = sites.OrderBy(s => s.Zone?.Name == "Jewelry" ? 0 : 1).ThenBy(s => s.Floor == 0 ? 0 : 1).ThenBy(s => s.At.x).ThenBy(s => s.At.z).First();
                found = true;
                break;
            }
            if (!found)
            {
                Debug.LogWarning("[Vault0166] No store wall has room for the safe.");
                return;
            }
            Safe(root, pick);
            Debug.Log($"[Vault0166] Safe in {pick.Zone?.Name} at {pick.At}.");
        }

        private static void Safe(Transform parent, MallSites.Site site)
        {
            Transform root = Group("JewellerySafe", parent);
            root.SetPositionAndRotation(site.At, site.Facing);
            Material steel = SalvageMaterials.Get("Metal"), dark = SalvageMaterials.Get("Plastic"), brass = SalvageMaterials.Get("Copper");
            float w = Size.x, h = Size.y, d = Size.z, t = 0.1f;
            Transform shell = Group("Shell", root);
            Box("Back", shell, new Vector3(0f, h * 0.5f, t * 0.5f + 0.04f), new Vector3(w, h, t), steel);
            Box("Left", shell, new Vector3(-w * 0.5f + t * 0.5f, h * 0.5f, d * 0.5f + 0.04f), new Vector3(t, h, d), steel);
            Box("Right", shell, new Vector3(w * 0.5f - t * 0.5f, h * 0.5f, d * 0.5f + 0.04f), new Vector3(t, h, d), steel);
            Box("Top", shell, new Vector3(0f, h - t * 0.5f, d * 0.5f + 0.04f), new Vector3(w, t, d), steel);
            Box("Base", shell, new Vector3(0f, t, d * 0.5f + 0.04f), new Vector3(w, t * 2f, d), steel);
            Box("Shelf", shell, new Vector3(0f, h * 0.55f, d * 0.5f + 0.04f), new Vector3(w - 2f * t, 0.04f, d - 0.1f), dark);
            // The door hangs on its left edge (seen from the front).
            Transform hinge = Group("Door", root);
            hinge.localPosition = new Vector3(-w * 0.5f, 0f, d + 0.04f);
            GameObject slab = Box("Slab", hinge, new Vector3(w * 0.5f, h * 0.5f, 0.06f), new Vector3(w, h, 0.12f), steel);
            Primitive(PrimitiveType.Cylinder, "Dial", hinge, new Vector3(w * 0.5f, h * 0.6f, 0.15f), new Vector3(0.28f, 0.03f, 0.28f), brass, false)
                .transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            Box("Handle", hinge, new Vector3(w * 0.75f, h * 0.45f, 0.16f), new Vector3(0.05f, 0.3f, 0.06f), brass, false);
            // The drill on its stand, clamped to the door.
            var rig = Group("DrillRig", root);
            Box("Stand", rig, new Vector3(0.3f, 0.6f, d + 0.6f), new Vector3(0.12f, 1.2f, 0.12f), SalvageMaterials.Get("Safety"), false);
            Box("Motor", rig, new Vector3(0.3f, 1.05f, d + 0.4f), new Vector3(0.22f, 0.22f, 0.45f), SalvageMaterials.Get("Safety"), false);
            Box("Bit", rig, new Vector3(0.3f, 1.05f, d + 0.16f), new Vector3(0.05f, 0.05f, 0.2f), steel, false);
            rig.gameObject.SetActive(false);
            Transform inside = Group("Inside", root);
            inside.localPosition = new Vector3(0f, t * 2f, d * 0.5f + 0.04f);
            var carve = root.gameObject.AddComponent<NavMeshObstacle>();
            carve.shape = NavMeshObstacleShape.Box;
            carve.center = new Vector3(0f, h * 0.5f, d * 0.5f);
            carve.size = new Vector3(w, h, d);
            carve.carving = true;
            root.gameObject.AddComponent<Core.SurfaceTag>().EditorSet(Core.SurfaceMaterial.Metal);
            MallSites.Bind(root.gameObject, site.Section);
            root.gameObject.AddComponent<VaultSafe>().EditorSetup(hinge, slab.GetComponent<Collider>(), rig.gameObject, inside);
        }
    }
}
