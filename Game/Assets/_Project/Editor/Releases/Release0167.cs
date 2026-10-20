using System.Collections.Generic;
using System.Linq;
using Abandoned.Extraction;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using static Abandoned.EditorTools.GreyboxFactory;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// 0.16.7 security office: a monitor desk in the upper floor's closed offices (or the service core),
    /// and eight wall cameras spread through the mall, one per space where it can.
    /// </summary>
    public static class Release0167
    {
        public const string GroupName = "_SECURITY_0167";
        private static readonly Vector3 DeskSize = new(1.4f, 0.78f, 0.7f), CamSize = new(0.3f, 0.3f, 0.35f);
        private const float CamLift = 2.8f;
        private const int Cameras = 8;

        [MenuItem(ToolMenu.MallUpdates + "0.16.7 Add Security Office", priority = ToolMenu.MallP + 34)]
        public static void Author()
        {
            Transform mall = MallSites.OpenMall(out var scene);
            if (mall == null) return;
            Build(mall);
            MallSites.SaveMall(scene, "Security0167");
        }

        public static void AuthorAndExit() => ReleaseKit.RunAndExit(Author);

        public static void Build(Transform mall)
        {
            Transform root = MallSites.FreshGroup(mall, GroupName);
            var desks = MallSites.WallSites((_, _) => true, DeskSize, 0f);
            MallSites.Site desk = desks.OrderBy(s => s.Zone?.Name == "Furniture" ? 0 : s.Zone?.Tag == "stock" ? 1 : 2).ThenBy(s => s.At.x).ThenBy(s => s.At.z).FirstOrDefault();
            if (desks.Count == 0)
            {
                Debug.LogWarning("[Security0167] No wall has room for the security desk; nothing added.");
                return;
            }
            MallSites.Claim(desk, DeskSize);
            var config = AssetDatabase.LoadAssetAtPath<HorrorConfig>(HorrorContentBuilder.Path);
            Desk(root, desk, config != null ? config.SectorOf(desk.At + Vector3.up * 1.4f) : 0);

            var mounts = MallSites.WallSites((_, _) => true, CamSize, CamLift, needsFront: false);
            var used = new HashSet<string>();
            var picked = new List<MallSites.Site>();
            // One per space first, spread out; then fill to eight anywhere at least 10 m from the others.
            foreach (MallSites.Site s in MallSites.Spread(mounts, 200, 10f, 167))
            {
                if (picked.Count >= Cameras) break;
                string space = s.Zone?.Name + s.Floor;
                if (used.Contains(space)) continue;
                used.Add(space);
                picked.Add(s);
            }
            foreach (MallSites.Site s in MallSites.Spread(mounts, 200, 10f, 168))
            {
                if (picked.Count >= Cameras) break;
                if (picked.All(p => Vector3.Distance(p.At, s.At) >= 10f)) picked.Add(s);
            }
            int n = 1;
            foreach (MallSites.Site s in picked.OrderBy(s => s.Floor).ThenBy(s => s.At.z).ThenBy(s => s.At.x))
            {
                MallSites.Claim(s, CamSize, CamLift);
                Camera(root, s, $"CAM {n++}  {s.Zone?.DisplayName ?? "SERVICE"}");
            }
            Debug.Log($"[Security0167] Security desk in {desk.Zone?.Name} (floor {desk.Floor}), {picked.Count} cameras.");
        }

        private static void Desk(Transform parent, MallSites.Site site, int sector)
        {
            Transform root = Group("SecurityDesk", parent);
            root.SetPositionAndRotation(site.At, site.Facing);
            Material steel = SalvageMaterials.Get("Metal"), dark = SalvageMaterials.Get("Plastic");
            float z = DeskSize.z * 0.5f + 0.04f;
            Transform art = Group("Desk", root);
            Box("Top", art, new Vector3(0f, DeskSize.y - 0.03f, z), new Vector3(DeskSize.x, 0.05f, DeskSize.z), steel, false);
            foreach (float side in new[] { -1f, 1f })
                Box("Leg", art, new Vector3(side * (DeskSize.x * 0.5f - 0.05f), DeskSize.y * 0.5f, z), new Vector3(0.05f, DeskSize.y, DeskSize.z - 0.05f), steel, false);
            Box("MonitorCase", art, new Vector3(0f, DeskSize.y + 0.28f, 0.25f), new Vector3(0.7f, 0.48f, 0.3f), dark, false);
            Box("SideMonitor", art, new Vector3(-0.5f, DeskSize.y + 0.2f, 0.25f), new Vector3(0.35f, 0.32f, 0.28f), dark, false);
            Box("Keyboard", art, new Vector3(0f, DeskSize.y + 0.01f, 0.6f), new Vector3(0.45f, 0.02f, 0.15f), dark, false);
            MallRoomsMeshes.Merge(art, "SecurityDesk");
            GameObject screen = Primitive(PrimitiveType.Quad, "Screen", root, new Vector3(0f, DeskSize.y + 0.28f, 0.405f), new Vector3(0.62f, 0.4f, 1f),
                GetMaterial("Greybox_SecurityScreen", new Color(0.05f, 0.12f, 0.08f)), false);
            screen.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            var solid = root.gameObject.AddComponent<BoxCollider>();
            solid.center = new Vector3(0f, DeskSize.y * 0.5f + 0.2f, z);
            solid.size = DeskSize + new Vector3(0f, 0.4f, 0f);
            MallSites.Bind(root.gameObject, site.Section);
            root.gameObject.AddComponent<SecurityDesk>().EditorSetup(screen.GetComponent<Renderer>(), sector);
        }

        private static void Camera(Transform parent, MallSites.Site site, string label)
        {
            Transform root = Group($"SecurityCam_{label.Split(' ')[1]}", parent);
            root.SetPositionAndRotation(site.At, site.Facing);
            Material white = SalvageMaterials.Get("Wrap"), dark = SalvageMaterials.Get("Plastic");
            float y = CamLift + CamSize.y * 0.5f;
            Transform art = Group("Housing", root);
            Box("Bracket", art, new Vector3(0f, y + 0.08f, 0.08f), new Vector3(0.06f, 0.06f, 0.16f), dark, false);
            GameObject body = Box("Body", art, new Vector3(0f, y, 0.22f), new Vector3(0.14f, 0.14f, 0.3f), white, false);
            body.transform.localRotation = Quaternion.Euler(25f, 0f, 0f);
            MallRoomsMeshes.Merge(art, "SecurityCamera");
            Transform eye = Group("Lens", root);
            eye.localPosition = new Vector3(0f, y - 0.05f, 0.4f);
            eye.localRotation = Quaternion.Euler(25f, 0f, 0f);
            var lens = eye.gameObject.AddComponent<UnityEngine.Camera>();
            lens.fieldOfView = 78f;
            lens.nearClipPlane = 0.15f;
            lens.farClipPlane = 45f;
            lens.enabled = false;
            var data = lens.GetUniversalAdditionalCameraData();
            data.renderShadows = false;
            data.renderPostProcessing = false;
            MallSites.Bind(root.gameObject, site.Section);
            root.gameObject.AddComponent<SecurityCamera>().EditorSetup(lens, label);
        }
    }
}
