using UnityEditor;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using static Abandoned.EditorTools.HqSalvageProps;

namespace Abandoned.EditorTools
{
    /// <summary>Marked work zones and graded containers turn warehouse space into a readable salvage workflow.</summary>
    public static class HqWarehouseZones
    {
        public static void Build(Transform parent)
        {
            Transform zones = Group("WarehouseWorkflow", parent);
            Material paint = PolishAssets.Material("HQ_LanePaint", new Color(.57f, .44f, .14f), .98f);
            Outline(zones, new Vector2(16.6f, 5.9f), new Vector2(10.2f, 6.1f), paint);
            FloorWords(zones, "01 / RECEIVING + WEIGH-IN", new Vector3(16.4f, .014f, 3.15f), .145f);
            Outline(zones, new Vector2(17f, 12f), new Vector2(5.3f, 2.5f), paint);
            FloorWords(zones, "02 / SORT + GRADE", new Vector3(17f, .014f, 10.85f), .13f);
            Outline(zones, new Vector2(19.5f, 20.6f), new Vector2(5f, 13f), paint);
            FloorWords(zones, "CLEAR / HAUL ROUTE", new Vector3(19.5f, .014f, 15.1f), .145f);
            foreach (float x in new[] { 7.98f, 10.02f })
                Box("DepartureLane", zones, new Vector3(x, .005f, 13.3f), new Vector3(.055f, .006f, 26f), paint, false);
            foreach (float z in new[] { 3.2f, 10.7f, 15.6f, 22f }) Arrow(zones, new Vector3(9f, .01f, z), paint);
            ZoneSign(zones, new Vector3(4.5f, 4.44f, 17.98f), "03 / STOCK A01\nMECHANICAL", 0f, 2.65f);
            ZoneSign(zones, new Vector3(11.8f, 4.44f, 17.98f), "03 / STOCK A02\nELECTRICAL", 0f, 2.65f);
            ZoneSign(zones, new Vector3(.23f, 3.58f, 14.15f), "ELECTRONICS RECOVERY / TEST BEFORE STOWING", 270f, 4.4f);
            ZoneSign(zones, new Vector3(17f, 4.45f, 2.82f), "01 / GOODS IN  -  WEIGH  -  LOG", 0f, 6f);
            foreach (float x in new[] { 14.6f, 19.4f })
                Rod("ReceivingSignSuspension", zones, new Vector3(x, 4.75f, 2.82f), new Vector3(x, 6.92f, 2.82f), .017f, CustomMallArt.Material("Metal"));
            ZoneSign(zones, new Vector3(24.05f, 3.18f, 7.4f), "DISPATCH / COMPANY OFFICE", 90f, 3.1f);
            ZoneSign(zones, new Vector3(27.05f, 3.18f, 14f), "CREW / WELFARE", 90f, 2.9f);
            ZoneSign(zones, new Vector3(27.05f, 3.22f, 23f), "04 / MAINTENANCE + FIELD GEAR", 90f, 4.3f);
            ScrapContainer(zones, new Vector3(1.3f, 0f, 20.65f), "FERROUS / GRADE B", 0);
            ScrapContainer(zones, new Vector3(1.3f, 0f, 23.25f), "NON-FERROUS / COPPER", 1);
            ScrapContainer(zones, new Vector3(1.3f, 0f, 25.85f), "E-WASTE / RECOVERY", 2);
            MarkStatic(zones);
        }

        private static void ScrapContainer(Transform parent, Vector3 floor, string grade, int kind)
        {
            Transform skip = At("GradedScrapContainer", parent, floor, 270f);
            Material body = PolishAssets.Material("HQ_ScrapSkip", new Color(.34f, .33f, .22f), .91f);
            Material metal = CustomMallArt.Material("Metal"), rust = CustomMallArt.Material("Rust");
            Box("SkipFloor", skip, Vector3.up * .17f, new Vector3(2.04f, .2f, 1.44f), body);
            foreach (float x in new[] { -.97f, .97f })
                Box("SkipSide", skip, new Vector3(x, .68f, 0f), new Vector3(.12f, 1.2f, 1.44f), body);
            foreach (float z in new[] { -.67f, .67f })
            {
                Box("SkipPanel", skip, new Vector3(0f, .65f, z), new Vector3(2f, 1.05f, .12f), body);
                Box("RustedRim", skip, new Vector3(0f, 1.24f, z), new Vector3(2.12f, .095f, .17f), rust, false);
            }
            for (int i = 0; i < 8; i++)
            {
                Vector3 at = new Vector3(-.67f + i % 4 * .42f, .38f + i / 4 * .28f, -.32f + i % 3 * .27f);
                if (kind == 1)
                    Rod("CopperReclaim", skip, at, at + new Vector3(.38f, .11f, .23f), .086f,
                        PolishAssets.Material("HQ_ReclaimedCopper", new Color(.47f, .28f, .17f), .62f));
                else
                {
                    GameObject scrap = Box(kind == 0 ? "FerrousOffcut" : "ReclaimedElectronics", skip, at,
                        new Vector3(.45f, kind == 0 ? .08f : .12f, .22f), kind == 0 ? metal : CustomMallArt.Material("Trim"), false);
                    scrap.transform.localRotation = Quaternion.Euler(i % 2 * 18f, i * 29f, -8f + i * 3f);
                }
            }
            Box("GradePlate", skip, new Vector3(0f, .77f, -.74f), new Vector3(1.58f, .33f, .025f), CustomMallArt.Material("Paper"), false);
            Label(skip, grade, new Vector3(0f, .77f, -.76f), .07f);
            foreach (float x in new[] { -.79f, .79f })
                Box("ForkPocket", skip, new Vector3(x, .075f, -.2f), new Vector3(.23f, .13f, .8f), CustomMallArt.Material("Rubber"), false);
        }

        private static void Outline(Transform parent, Vector2 centre, Vector2 size, Material paint)
        {
            foreach (float x in new[] { centre.x - size.x * .5f, centre.x + size.x * .5f })
                Box("PaintedZoneEdge", parent, new Vector3(x, .005f, centre.y), new Vector3(.075f, .006f, size.y), paint, false);
            foreach (float z in new[] { centre.y - size.y * .5f, centre.y + size.y * .5f })
                Box("PaintedZoneEdge", parent, new Vector3(centre.x, .005f, z), new Vector3(size.x, .006f, .075f), paint, false);
        }

        private static void Arrow(Transform parent, Vector3 position, Material paint)
        {
            Box("LaneArrowStem", parent, position, new Vector3(.13f, .007f, .85f), paint, false);
            for (int side = -1; side <= 1; side += 2)
            {
                Vector3 from = position + new Vector3(side * .38f, 0f, .05f);
                Vector3 to = position + new Vector3(0f, 0f, .45f);
                GameObject head = Box("LaneArrowHead", parent, (from + to) * .5f,
                    new Vector3(.12f, .007f, Vector3.Distance(from, to)), paint, false);
                head.transform.localRotation = Quaternion.LookRotation(to - from, Vector3.up);
            }
        }

        private static void ZoneSign(Transform parent, Vector3 centre, string text, float yaw, float width)
        {
            Transform sign = At("OperationsZoneSign", parent, centre, yaw);
            Box("SignPanel", sign, Vector3.zero, new Vector3(width, .6f, .035f), CustomMallArt.Material("Hazard"), false);
            Label(sign, text, new Vector3(0f, 0f, -.023f), text.Contains("\n") ? .10f : .105f);
        }

        private static void FloorWords(Transform parent, string words, Vector3 position, float size)
        {
            TextMesh label = new GameObject("PaintedFloor_" + words).AddComponent<TextMesh>();
            label.transform.SetParent(parent, false);
            label.transform.SetLocalPositionAndRotation(position, Quaternion.Euler(90f, 0f, 0f));
            label.text = words;
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.fontSize = 64;
            label.characterSize = size;
            label.color = new Color(.56f, .44f, .16f);
            WorldTextMaterial.Apply(label, AssetDatabase.LoadAssetAtPath<Font>("Assets/_Project/Art/ThirdParty/Fonts/SairaStencilOne/SairaStencilOne-Regular.ttf"));
        }
    }
}
