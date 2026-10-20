using System.Linq;
using Abandoned.Extraction;
using UnityEditor;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// 0.17.5 live water: flooded patches in the basement (the low ground), larger than the 0.17.3 puddles.
    /// Every puddle and flood is water: live while its power sector has power, so a thrown breaker lights a
    /// sector and arms its water.
    /// </summary>
    public static class Release0175
    {
        public const string GroupName = "_FLOODS_0175";
        private const float Half = 1.7f;

        [MenuItem(ToolMenu.MallUpdates + "0.17.5 Add Flooded Areas", priority = ToolMenu.MallP + 37)]
        public static void Author()
        {
            ReleaseKit.Config<GroundConfig>(ReleaseKit.ExtractionResources, GroundConfig.ResourcePath);
            SoundBankBuilder.CreateMissing();
            Transform mall = MallSites.OpenMall(out var scene);
            if (mall == null) return;
            Build(mall);
            MallSites.SaveMall(scene, "Floods0175");
        }

        public static void AuthorAndExit() => ReleaseKit.RunAndExit(Author);

        public static void Build(Transform mall)
        {
            Transform root = MallSites.FreshGroup(mall, GroupName);
            var config = AssetDatabase.LoadAssetAtPath<HorrorConfig>(HorrorContentBuilder.Path);
            var sites = MallSites.FlatSites((_, f) => f < 0, Half);
            if (sites.Count < 2) sites.AddRange(MallSites.FlatSites((t, f) => f == 0 && MallLayout.ZoneAt(t, f)?.Tag == "food", Half));
            int n = 0;
            Material water = GetMaterial("Greybox_FloodWater", new Color(0.05f, 0.06f, 0.065f));
            if (water.HasProperty("_Smoothness")) water.SetFloat("_Smoothness", 0.97f);
            foreach (MallSites.Site site in MallSites.Spread(sites, 3, 6f, 175))
            {
                MallSites.Claim(site, new Vector3(Half * 2f, 0.2f, Half * 2f));
                Transform flood = Group($"Flood_{n++}", root);
                flood.position = site.At;
                Transform art = Group("Art", flood);
                Primitive(PrimitiveType.Cube, "Water", art, new Vector3(0f, 0.012f, 0f), new Vector3(Half * 2f, 0.008f, Half * 2f), water, false);
                Primitive(PrimitiveType.Cylinder, "Spill", art, new Vector3(Half, 0.011f, 0.3f), new Vector3(1.4f, 0.004f, 1.1f), water, false);
                MallRoomsMeshes.Merge(art, "FloodWater");
                MallSites.Bind(flood.gameObject, site.Section);
                int sector = config != null ? config.SectorOf(site.At + Vector3.up * 1.4f) : 0;
                flood.gameObject.AddComponent<NoisyGround>().EditorSetup(GroundKind.Puddle, new Vector2(Half + 0.1f, Half + 0.1f), sector);
            }
            Debug.Log($"[Floods0175] {n} flooded areas (plus every 0.17.3 puddle) are live while their sector has power.");
        }
    }
}
