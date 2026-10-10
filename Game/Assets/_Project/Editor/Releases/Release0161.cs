using System.Collections.Generic;
using System.Linq;
using Abandoned.Extraction;
using UnityEditor;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// 0.16.1 breaker rooms: one breaker cabinet per power sector, on a bare wall inside that sector (service
    /// spaces first, then the concourse, then stores), added over the saved mall. Nothing is baked: a cabinet
    /// hangs on the wall above anyone's head-height clutter and carves nothing.
    /// </summary>
    public static class Release0161
    {
        public const string GroupName = "_BREAKERS_0161";
        private static readonly Vector3 Size = new(0.7f, 0.9f, 0.28f);
        private const float Lift = 1.0f;

        [MenuItem("Tools/Abandoned/Mall/Add Breaker Rooms (0.16.1)")]
        public static void Author()
        {
            ReleaseKit.Config<BreakerConfig>(ReleaseKit.ExtractionResources, BreakerConfig.ResourcePath);
            NetworkContentBuilder.CreateRunStatePrefab();
            SoundBankBuilder.CreateMissing();
            Transform mall = MallSites.OpenMall(out var scene);
            if (mall == null) return;
            Build(mall);
            MallSites.SaveMall(scene, "Breakers0161");
        }

        public static void AuthorAndExit() => ReleaseKit.RunAndExit(Author);

        public static void Build(Transform mall)
        {
            var config = AssetDatabase.LoadAssetAtPath<HorrorConfig>(HorrorContentBuilder.Path);
            int sectors = config != null ? Mathf.Min(config.Sectors, RunBreakers.MaxSectors) : 6;
            Transform root = MallSites.FreshGroup(mall, GroupName);
            List<MallSites.Site> sites = MallSites.WallSites((_, _) => true, Size, Lift);
            int placed = 0;
            for (int s = 0; s < sectors; s++)
            {
                MallSites.Site? best = null;
                foreach (MallSites.Site site in sites.OrderBy(Rank).ThenBy(x => x.At.x).ThenBy(x => x.At.z))
                {
                    if (config != null && config.SectorOf(site.At + Vector3.up * 1.4f) != s) continue;
                    if (MallSites.Check(site.At, site.Outward, Size, Lift, true, out _) != null) continue;
                    best = site;
                    break;
                }
                if (best == null)
                {
                    Debug.LogWarning($"[Breakers0161] No bare wall found for power sector {s}; it has no breaker.");
                    continue;
                }
                MallSites.Claim(best.Value, Size, Lift);
                Panel(root, s, best.Value);
                placed++;
            }
            Debug.Log($"[Breakers0161] {placed} breaker cabinets for {sectors} power sectors.");
        }

        // Service spaces first, then the concourse, then stores: a breaker belongs in the back of house.
        private static int Rank(MallSites.Site site)
        {
            MallLayout.Zone? zone = site.Zone;
            if (site.Floor < 0 || zone?.Tag == "stock") return 0;
            return zone?.Kind == MallLayout.Kind.Store ? 2 : 1;
        }

        private static void Panel(Transform parent, int sector, MallSites.Site site)
        {
            Transform root = Group($"Breaker_Sector{sector}", parent);
            root.SetPositionAndRotation(site.At, site.Facing);
            Material steel = SalvageMaterials.Get("Metal"), dark = SalvageMaterials.Get("Plastic"), stripe = SalvageMaterials.Get("Safety");
            Transform shell = Group("Cabinet", root);
            float z = Size.z * 0.5f + 0.02f, y = Lift + Size.y * 0.5f;
            Box("Body", shell, new Vector3(0f, y, z), Size, steel, false);
            Box("Door", shell, new Vector3(0f, y, Size.z + 0.03f), new Vector3(Size.x - 0.06f, Size.y - 0.06f, 0.02f), dark, false);
            Box("Stripe", shell, new Vector3(0f, Lift + Size.y - 0.06f, Size.z + 0.045f), new Vector3(Size.x - 0.1f, 0.06f, 0.01f), stripe, false);
            Box("Conduit", shell, new Vector3(0.2f, Lift + Size.y + 0.6f, 0.06f), new Vector3(0.06f, 1.2f, 0.06f), steel, false);
            MallRoomsMeshes.Merge(shell, "BreakerCabinet");
            // The handle pivots at the cabinet's side; down = off.
            Transform pivot = Group("Handle", root);
            pivot.localPosition = new Vector3(Size.x * 0.5f + 0.06f, y, Size.z * 0.5f);
            Box("Arm", pivot, new Vector3(0f, 0.16f, 0f), new Vector3(0.05f, 0.32f, 0.05f), steel, false);
            Box("Grip", pivot, new Vector3(0f, 0.34f, 0.04f), new Vector3(0.12f, 0.06f, 0.1f), stripe, false);
            pivot.localRotation = Quaternion.Euler(40f, 0f, 0f);
            GameObject lamp = Primitive(PrimitiveType.Sphere, "Lamp", root, new Vector3(-0.22f, Lift + Size.y - 0.15f, Size.z + 0.06f),
                Vector3.one * 0.07f, GetMaterial("Greybox_BreakerLamp", new Color(0.9f, 0.15f, 0.1f)), false);
            // Solid like any fitting, and something to aim E at.
            var solid = root.gameObject.AddComponent<BoxCollider>();
            solid.center = new Vector3(0f, y, z);
            solid.size = Size;
            MallSites.Bind(root.gameObject, site.Section);
            root.gameObject.AddComponent<BreakerPanel>().EditorSetup(sector, pivot, lamp.GetComponent<Renderer>());
        }
    }
}
