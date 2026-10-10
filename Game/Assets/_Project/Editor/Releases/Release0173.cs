using System.Linq;
using Abandoned.Extraction;
using UnityEditor;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// 0.17.3 noisy ground: about two dozen patches of broken glass, loose tiles and puddles on free floor
    /// across the mall (puddles mostly in the food court and basement, glass by the shopfronts). Flat art with
    /// no colliders, merged per patch.
    /// </summary>
    public static class Release0173
    {
        public const string GroupName = "_NOISY_GROUND_0173";
        private static readonly Vector3 Size = new(1.8f, 0.3f, 1.8f);
        private const int Patches = 24;

        [MenuItem("Tools/Abandoned/Mall/Add Noisy Ground (0.17.3)")]
        public static void Author()
        {
            ReleaseKit.Config<GroundConfig>(ReleaseKit.ExtractionResources, GroundConfig.ResourcePath);
            SoundBankBuilder.CreateMissing();
            Transform mall = MallSites.OpenMall(out var scene);
            if (mall == null) return;
            Build(mall);
            MallSites.SaveMall(scene, "Ground0173");
        }

        public static void AuthorAndExit() => ReleaseKit.RunAndExit(Author);

        public static void Build(Transform mall)
        {
            Transform root = MallSites.FreshGroup(mall, GroupName);
            var config = AssetDatabase.LoadAssetAtPath<HorrorConfig>(HorrorContentBuilder.Path);
            var sites = MallSites.WallSites((_, _) => false, Size); // (keeps the search state fresh)
            sites = MallSites.FloorSites((_, _) => true, Size);
            var random = new System.Random(173);
            int placed = 0;
            foreach (MallSites.Site site in MallSites.Spread(sites, Patches, 6f, 173))
            {
                string zone = site.Zone?.Tag ?? "";
                GroundKind kind = site.Floor < 0 || zone == "food" ? (random.NextDouble() < 0.7 ? GroundKind.Puddle : GroundKind.LooseTiles)
                    : site.Zone?.Kind == MallLayout.Kind.Store ? (random.NextDouble() < 0.6 ? GroundKind.BrokenGlass : GroundKind.LooseTiles)
                    : (random.NextDouble() < 0.5 ? GroundKind.BrokenGlass : random.NextDouble() < 0.5 ? GroundKind.Puddle : GroundKind.LooseTiles);
                MallSites.Claim(site, Size);
                int sector = config != null ? config.SectorOf(site.At + Vector3.up * 1.4f) : 0;
                Patch(root, placed++, kind, site, sector, random);
            }
            Debug.Log($"[Ground0173] {placed} noisy ground patches.");
        }

        private static void Patch(Transform parent, int n, GroundKind kind, MallSites.Site site, int sector, System.Random random)
        {
            Transform root = Group($"{kind}_{n}", parent);
            root.SetPositionAndRotation(site.At, Quaternion.Euler(0f, (float)random.NextDouble() * 360f, 0f));
            Transform art = Group("Art", root);
            float R() => (float)random.NextDouble();
            switch (kind)
            {
                case GroundKind.BrokenGlass:
                    for (int i = 0; i < 22; i++)
                    {
                        GameObject shard = Box("Shard", art, new Vector3((R() - 0.5f) * 1.5f, 0.006f, (R() - 0.5f) * 1.5f),
                            new Vector3(0.04f + R() * 0.16f, 0.006f, 0.03f + R() * 0.12f), SalvageMaterials.Get("Glass"), false);
                        shard.transform.localRotation = Quaternion.Euler((R() - 0.5f) * 12f, R() * 360f, (R() - 0.5f) * 12f);
                    }
                    break;
                case GroundKind.LooseTiles:
                    for (int i = 0; i < 7; i++)
                    {
                        GameObject tile = Box("Tile", art, new Vector3((R() - 0.5f) * 1.3f, 0.015f, (R() - 0.5f) * 1.3f),
                            new Vector3(0.3f, 0.02f, 0.3f), SalvageMaterials.Get(i % 3 == 0 ? "Concrete" : "Wrap"), false);
                        tile.transform.localRotation = Quaternion.Euler((R() - 0.5f) * 14f, R() * 90f, (R() - 0.5f) * 14f);
                    }
                    break;
                default:
                    Material water = GetMaterial("Greybox_Puddle", new Color(0.06f, 0.07f, 0.08f));
                    if (water.HasProperty("_Smoothness")) water.SetFloat("_Smoothness", 0.96f);
                    Primitive(PrimitiveType.Cylinder, "Water", art, new Vector3(0f, 0.004f, 0f), new Vector3(1.7f, 0.002f, 1.3f), water, false);
                    Primitive(PrimitiveType.Cylinder, "Water", art, new Vector3(0.5f, 0.005f, 0.3f), new Vector3(0.8f, 0.002f, 0.7f), water, false);
                    break;
            }
            MallRoomsMeshes.Merge(art, $"Ground_{kind}");
            MallSites.Bind(root.gameObject, site.Section);
            root.gameObject.AddComponent<NoisyGround>().EditorSetup(kind, new Vector2(0.85f, 0.85f), sector);
        }
    }
}
