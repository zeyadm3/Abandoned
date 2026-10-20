using System.Linq;
using Abandoned.Extraction;
using UnityEditor;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;

namespace Abandoned.EditorTools
{
    /// <summary>0.16.5 store alarms: one alarm box per store shutter, on a wall inside the store nearest its shopfront.</summary>
    public static class Release0165
    {
        public const string GroupName = "_ALARMS_0165";
        private static readonly Vector3 Size = new(0.32f, 0.32f, 0.12f);
        private const float Lift = 1.9f;

        [MenuItem(ToolMenu.MallUpdates + "0.16.5 Add Store Alarms", priority = ToolMenu.MallP + 32)]
        public static void Author()
        {
            ReleaseKit.Config<AlarmConfig>(ReleaseKit.ExtractionResources, AlarmConfig.ResourcePath);
            NetworkContentBuilder.CreateRunStatePrefab();
            SoundBankBuilder.CreateMissing();
            Transform mall = MallSites.OpenMall(out var scene);
            if (mall == null) return;
            Build(mall);
            MallSites.SaveMall(scene, "Alarms0165");
        }

        public static void AuthorAndExit() => ReleaseKit.RunAndExit(Author);

        public static void Build(Transform mall)
        {
            Transform root = MallSites.FreshGroup(mall, GroupName);
            var sites = MallSites.WallSites((_, _) => true, Size, Lift, needsFront: false);
            int placed = 0;
            foreach (RollerShutter shutter in mall.GetComponentsInChildren<RollerShutter>(true).Where(s => !s.Entrance).OrderBy(s => s.Index))
            {
                var inside = sites.Where(s => shutter.InRoom(s.At + s.Outward * 0.6f) && MallSites.Check(s.At, s.Outward, Size, Lift, false, out _) == null)
                    .OrderBy(s => Vector3.Distance(s.At, shutter.transform.position)).ToList();
                if (inside.Count == 0)
                {
                    Debug.LogWarning($"[Alarms0165] No wall inside store {shutter.Index} for its alarm box.");
                    continue;
                }
                MallSites.Site site = inside[0];
                MallSites.Claim(site, Size, Lift);
                Box(root, shutter, site);
                placed++;
            }
            Debug.Log($"[Alarms0165] {placed} store alarm boxes.");
        }

        private static void Box(Transform parent, RollerShutter shutter, MallSites.Site site)
        {
            Transform root = Group($"Alarm_Store{shutter.Index}", parent);
            root.SetPositionAndRotation(site.At, site.Facing);
            float y = Lift + Size.y * 0.5f;
            Transform art = Group("Box", root);
            GreyboxFactory.Box("Body", art, new Vector3(0f, y, Size.z * 0.5f), Size, SalvageMaterials.Get("Paint"), false);
            GreyboxFactory.Box("Bell", art, new Vector3(0f, y + 0.02f, Size.z + 0.03f), new Vector3(0.2f, 0.2f, 0.06f), SalvageMaterials.Get("Metal"), false);
            MallRoomsMeshes.Merge(art, "AlarmBox");
            GameObject lamp = Primitive(PrimitiveType.Sphere, "Lamp", root, new Vector3(0.11f, y - 0.11f, Size.z + 0.02f), Vector3.one * 0.05f,
                GetMaterial("Greybox_AlarmLamp", new Color(0.3f, 0.05f, 0.05f)), false);
            var hit = root.gameObject.AddComponent<BoxCollider>();
            hit.center = new Vector3(0f, y, Size.z * 0.5f);
            hit.size = Size + new Vector3(0.1f, 0.1f, 0.1f);
            MallSites.Bind(root.gameObject, site.Section);
            root.gameObject.AddComponent<StoreAlarm>().EditorSetup(shutter, lamp.GetComponent<Renderer>());
        }
    }
}
