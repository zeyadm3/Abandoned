using System.Collections.Generic;
using System.Linq;
using Abandoned.Extraction;
using UnityEditor;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// 0.16.4 vents: floor-level grilles on bare walls, in pairs. Each store gets a duct out to somewhere
    /// else (another floor when one is in range, else the concourse or the service core). The duct itself
    /// is not modelled: crawling it is a few dark seconds (NetworkPlayer.Vents).
    /// </summary>
    public static class Release0164
    {
        public const string GroupName = "_VENTS_0164";
        private static readonly Vector3 Size = new(0.9f, 0.6f, 0.1f);
        private const float Lift = 0.12f;

        [MenuItem("Tools/Abandoned/Mall/Add Vents (0.16.4)")]
        public static void Author()
        {
            ReleaseKit.Config<VentConfig>(ReleaseKit.ExtractionResources, VentConfig.ResourcePath);
            SoundBankBuilder.CreateMissing();
            Transform mall = MallSites.OpenMall(out var scene);
            if (mall == null) return;
            Build(mall);
            MallSites.SaveMall(scene, "Vents0164");
        }

        public static void AuthorAndExit() => ReleaseKit.RunAndExit(Author);

        public static void Build(Transform mall)
        {
            Transform root = MallSites.FreshGroup(mall, GroupName);
            List<MallSites.Site> sites = MallSites.WallSites((_, _) => true, Size, Lift);
            var stores = sites.Where(s => s.Zone?.Kind == MallLayout.Kind.Store).GroupBy(s => s.Zone?.Name + s.Floor).OrderBy(g => g.Key);
            int duct = 0;
            foreach (var store in stores)
            {
                if (duct >= 27) break;
                bool done = false;
                foreach (MallSites.Site a in store.OrderBy(s => s.At.x).ThenBy(s => s.At.z))
                {
                    if (MallSites.Check(a.At, a.Outward, Size, Lift, true, out _) != null) continue;
                    // The far end: out of this store, 6-30 m away, another floor first.
                    var far = sites.Where(b => b.Zone?.Name != a.Zone?.Name && Vector3.Distance(a.At, b.At) is >= 6f and <= 30f
                                               && MallSites.Check(b.At, b.Outward, Size, Lift, true, out _) == null)
                        .OrderBy(b => b.Floor == a.Floor ? 1 : 0).ThenBy(b => Vector3.Distance(a.At, b.At)).FirstOrDefault();
                    if (far.Section == null && far.At == Vector3.zero) continue;
                    MallSites.Claim(a, Size, Lift);
                    MallSites.Claim(far, Size, Lift);
                    Grille(root, duct, 0, a);
                    Grille(root, duct, 1, far);
                    Debug.Log($"[Vents0164] Duct {duct}: {a.Zone?.Name} (floor {a.Floor}) to {far.Zone?.Name} (floor {far.Floor}).");
                    duct++;
                    done = true;
                    break;
                }
                if (!done) Debug.LogWarning($"[Vents0164] No vent pair found for {store.Key}.");
            }
            Debug.Log($"[Vents0164] {duct} vent ducts placed.");
        }

        private static void Grille(Transform parent, int duct, int end, MallSites.Site site)
        {
            Transform root = Group($"Vent_{duct}_{"AB"[end]}", parent);
            root.SetPositionAndRotation(site.At, site.Facing);
            Material frame = SalvageMaterials.Get("Metal"), dark = SalvageMaterials.Get("Plastic");
            Transform art = Group("Grille", root);
            float y = Lift + Size.y * 0.5f, z = Size.z * 0.5f;
            Box("Back", art, new Vector3(0f, y, 0.01f), new Vector3(Size.x - 0.06f, Size.y - 0.06f, 0.02f), dark, false);
            Box("FrameTop", art, new Vector3(0f, Lift + Size.y - 0.03f, z), new Vector3(Size.x, 0.06f, Size.z), frame, false);
            Box("FrameBottom", art, new Vector3(0f, Lift + 0.03f, z), new Vector3(Size.x, 0.06f, Size.z), frame, false);
            foreach (float side in new[] { -1f, 1f })
                Box("FrameSide", art, new Vector3(side * (Size.x - 0.06f) * 0.5f, y, z), new Vector3(0.06f, Size.y, Size.z), frame, false);
            for (int i = 0; i < 6; i++)
            {
                GameObject slat = Box("Slat", art, new Vector3(0f, Lift + 0.1f + i * 0.08f, z), new Vector3(Size.x - 0.1f, 0.02f, 0.06f), frame, false);
                slat.transform.localRotation = Quaternion.Euler(-30f, 0f, 0f);
            }
            MallRoomsMeshes.Merge(art, "VentGrille");
            var hit = root.gameObject.AddComponent<BoxCollider>();
            hit.center = new Vector3(0f, y, z);
            hit.size = Size;
            Transform exit = Group("Out", root);
            exit.localPosition = new Vector3(0f, 0.05f, 0.85f);
            MallSites.Bind(root.gameObject, site.Section);
            root.gameObject.AddComponent<VentGrille>().EditorSetup(duct, end, exit);
        }
    }
}
