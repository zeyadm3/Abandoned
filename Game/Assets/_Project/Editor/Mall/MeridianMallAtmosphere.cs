using Abandoned.Structure;
using UnityEngine;
using UnityEngine.Rendering;
using static Abandoned.EditorTools.GreyboxFactory;

namespace Abandoned.EditorTools
{
    /// <summary>The empty central court retains the scale and finish of a once busy shopping centre.</summary>
    public static class MeridianMallAtmosphere
    {
        public const string GroupName = "MeridianCourt015";

        public static void Build(Transform mall)
        {
            Transform old = mall.Find(GroupName);
            if (old != null) Object.DestroyImmediate(old.gameObject);
            Transform root = Group(GroupName, mall);
            BalconyEdges(mall, Group("LayeredBalconyFascias", root));
            SuspendedInstallation(Group("AbandonedCourtInstallation", root));
            ShopLightColors(mall);
        }

        private static void BalconyEdges(Transform mall, Transform parent)
        {
            for (int floor = 1; floor < MallLayout.Floors; floor++)
            for (int n = 4; n < 8; n++)
            {
                Edge(new Vector2Int(4, n), Vector2Int.right);
                Edge(new Vector2Int(9, n), Vector2Int.left);
                Edge(new Vector2Int(n + 1, 3), Vector2Int.up);
                Edge(new Vector2Int(n + 1, 8), Vector2Int.down);

                void Edge(Vector2Int cell, Vector2Int side)
                {
                    StructuralSection support = MeridianDressingUtility.Support(mall, cell, floor);
                    if (support == null) return;
                    Transform bay = Group($"BalconyFascia_{floor}_{cell.x}_{cell.y}", parent);
                    bay.localPosition = MallLayout.TileTopCenter(cell, floor) + new Vector3(side.x, 0, side.y) * 1.97f;
                    bay.localRotation = Quaternion.LookRotation(new Vector3(side.x, 0, side.y));
                    Box("AgedStoneFascia", bay, new Vector3(0, -.19f, 0), new Vector3(3.94f, .3f, .12f), MeridianRetailMaterials.Stone, false);
                    Box("BrassReveal", bay, new Vector3(0, -.075f, .067f), new Vector3(3.92f, .045f, .035f), MeridianRetailMaterials.Brass, false);
                    Box("ShadowReveal", bay, new Vector3(0, -.37f, -.005f), new Vector3(3.93f, .055f, .1f), MeridianRetailMaterials.Dark, false);
                    bay.gameObject.AddComponent<SectionProp>().EditorSetup(support);
                }
            }
        }

        private static void SuspendedInstallation(Transform parent)
        {
            Vector3 center = new(28f, 10.8f, 24f);
            const int segments = 20;
            foreach (float radius in new[] { 2.4f, 4.1f })
            for (int i = 0; i < segments; i++)
            {
                float angle = (i + .5f) * Mathf.PI * 2 / segments;
                Vector3 radial = new(Mathf.Cos(angle), 0, Mathf.Sin(angle));
                GameObject rim = Box("SuspendedBronzeRim", parent, center + radial * radius,
                    new Vector3(radius * .318f, .11f, .075f), MeridianRetailMaterials.Brass, false);
                rim.transform.localRotation = Quaternion.LookRotation(radial);
                rim.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;
            }
            foreach (Vector3 at in new[] { new Vector3(24f, 10.8f, 24f), new Vector3(32f, 10.8f, 24f),
                         new Vector3(28f, 10.8f, 20f), new Vector3(28f, 10.8f, 28f) })
                Box("RoofSuspensionRod", parent, at + Vector3.up * .7f, new Vector3(.025f, 1.4f, .025f), MeridianRetailMaterials.Dark, false);
            Banner(new Vector3(25.3f, 9.55f, 22f), 16f, "clothing");
            Banner(new Vector3(30.7f, 9.45f, 29f), -12f, "electronics");

            void Banner(Vector3 at, float angle, string tag)
            {
                Transform banner = Group("FadedUnletteredBanner", parent);
                banner.localPosition = at;
                banner.localRotation = Quaternion.Euler(0, angle, 0);
                Box("BannerTopRail", banner, new Vector3(0, 1.1f, 0), new Vector3(1.36f, .055f, .065f), MeridianRetailMaterials.Brass, false);
                // Uneven lengths and a missing strip read as old fabric, without adding arbitrary words.
                for (int strip = 0; strip < 5; strip++)
                {
                    float height = strip == 1 ? 1.74f : strip == 4 ? 1.94f : 2.14f;
                    Box("WeatheredFabricStrip", banner, new Vector3((strip - 2) * .25f, 1.06f - height * .5f, 0),
                        new Vector3(.246f, height, .012f), MeridianRetailMaterials.Accent(tag), false);
                }
            }
        }

        private static void ShopLightColors(Transform mall)
        {
            Transform fixtures = mall.Find("MeridianFixtures");
            if (fixtures == null) return;
            foreach (LightFixture fixture in fixtures.GetComponentsInChildren<LightFixture>(true))
            {
                Light lamp = fixture.Lamp;
                if (lamp == null || !fixture.name.StartsWith("Fixture_")) continue;
                Vector3 at = fixture.transform.position;
                int floor = Mathf.FloorToInt(at.y / MallLayout.StoryHeight);
                var cell = new Vector2Int(Mathf.FloorToInt(at.x / MallLayout.Tile), Mathf.FloorToInt(at.z / MallLayout.Tile));
                string tag = MallLayout.ZoneAt(cell, floor)?.Tag;
                lamp.color = tag switch
                {
                    "cinema" => new Color(1f, .69f, .39f),
                    "clothing" => new Color(1f, .87f, .74f),
                    "jewelry" => new Color(.72f, .88f, 1f),
                    "food" => new Color(1f, .8f, .51f),
                    "furniture" => new Color(1f, .85f, .67f),
                    "stock" or "office" => new Color(.66f, .79f, .78f),
                    _ => new Color(.78f, .88f, .92f)
                };
            }
        }
    }
}
