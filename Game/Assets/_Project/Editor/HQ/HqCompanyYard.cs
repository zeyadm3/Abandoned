using Abandoned.Core;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using static Abandoned.EditorTools.HqSalvageProps;

namespace Abandoned.EditorTools
{
    /// <summary>A company frontage, marked loading apron and protected staff approach.</summary>
    public static class HqCompanyYard
    {
        public static void Build(Transform root)
        {
            Transform yard = Group("CompanyLoadingYard", root);
            Box("IndustrialEstateGround", yard, new Vector3(18f, -.44f, 1f), new Vector3(120f, .3f, 110f), CustomMallArt.Material("Dirt"))
                .AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Dirt);
            Box("LoadingApron", yard, new Vector3(18f, -.17f, -16f), new Vector3(56f, .32f, 32f), HqIndustrialMaterials.Asphalt)
                .AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Asphalt);
            Frontage(yard);
            Traffic(yard);
            Boundary(yard);
            MarkStatic(yard);
        }

        private static void Frontage(Transform yard)
        {
            Material steel = HqIndustrialMaterials.Steel;
            Material yellow = HqIndustrialMaterials.Yellow;
            Box("CompanyFacadeSign", yard, new Vector3(16.7f, 4.67f, -.28f), new Vector3(13.1f, 1.20f, .08f), steel, false);
            Label(yard, "ABANDONED SALVAGING", new Vector3(16.7f, 4.79f, -.33f), .28f, new Color(.86f, .83f, .68f));
            Label(yard, "INDUSTRIAL RECOVERY  /  RECLAIM  /  REPAIR", new Vector3(16.7f, 4.37f, -.334f), .092f, new Color(.63f, .70f, .66f));
            Box("OperationsFascia", yard, new Vector3(30f, 3.40f, -.26f), new Vector3(11.5f, .52f, .07f), steel, false);
            Label(yard, "OPERATIONS OFFICE / CREW ENTRANCE", new Vector3(30f, 3.4f, -.304f), .095f, new Color(.83f, .83f, .72f));

            Box("LoadingCanopy", yard, new Vector3(6f, 3.68f, -2.1f), new Vector3(9.3f, .17f, 4.4f), steel);
            foreach (float x in new[] { 1.48f, 10.52f })
            {
                Box("CanopyColumn", yard, new Vector3(x, 1.80f, -4.12f), new Vector3(.13f, 3.6f, .13f), steel);
                Box("CanopyFoot", yard, new Vector3(x, .07f, -4.12f), new Vector3(.32f, .14f, .32f), steel, false);
            }
            Box("LoadingBayFascia", yard, new Vector3(6f, 3.48f, -4.31f), new Vector3(8.8f, .38f, .06f), yellow, false);
            Label(yard, "01 / GOODS IN + VEHICLE DISPATCH", new Vector3(6f, 3.48f, -4.35f), .096f, new Color(.10f, .15f, .14f));
            Box("StaffEntranceCanopy", yard, new Vector3(30f, 3.05f, -1.18f), new Vector3(5.4f, .14f, 2.4f), steel);
            foreach (float x in new[] { 27.4f, 32.6f })
                Bar("CanopyBrace", yard, new Vector3(x, 2.4f, -.23f), new Vector3(x, 3.0f, -2.27f), .07f, .07f, steel);
            Box("EntryMat", yard, new Vector3(30f, .025f, -.7f), new Vector3(2.1f, .04f, 1.1f), CustomMallArt.Material("Rubber"), false);

            foreach (float x in new[] { 2.15f, 9.85f })
                Bollard(yard, new Vector3(x, 0f, -1.2f));
            foreach (float x in new[] { 26.9f, 33.1f })
                Bollard(yard, new Vector3(x, 0f, -2.6f));
            Box("ReceivingNotice", yard, new Vector3(12.1f, 1.8f, -.25f), new Vector3(2.65f, 1.25f, .035f), HqIndustrialMaterials.White, false);
            Label(yard, "DELIVERIES\nREPORT TO DISPATCH\nTAG / WEIGH / SORT", new Vector3(12.1f, 1.8f, -.278f), .085f, new Color(.14f, .22f, .22f));
            Box("DrainChannel", yard, new Vector3(6f, -.005f, -4.5f), new Vector3(8f, .012f, .16f), steel, false);
            for (int i = 0; i < 24; i++)
                Box("DrainGrate", yard, new Vector3(2.13f + i * .335f, .007f, -4.5f), new Vector3(.028f, .013f, .17f), HqIndustrialMaterials.White, false);
        }

        private static void Traffic(Transform yard)
        {
            Material yellow = HqIndustrialMaterials.Yellow;
            Material white = HqIndustrialMaterials.White;
            foreach (float x in new[] { 2.6f, 9.4f })
                Box("LoadingLaneEdge", yard, new Vector3(x, .012f, -13.8f), new Vector3(.10f, .006f, 19f), yellow, false);
            for (int i = 0; i < 3; i++)
                Arrow(yard, new Vector3(6f, .016f, -8f - i * 6.5f), yellow);

            foreach (float x in new[] { 17f, 22f, 27f, 32f, 37f })
                Box("FleetParkingLine", yard, new Vector3(x, .012f, -16.5f), new Vector3(.09f, .006f, 6.2f), white, false);
            Box("FleetParkingBack", yard, new Vector3(27f, .012f, -19.6f), new Vector3(20f, .006f, .09f), white, false);
            FloorLabel(yard, "CREW / FLEET", new Vector3(27f, .024f, -12.8f), .13f, new Color(.62f, .65f, .60f));
            foreach (float x in new[] { 28.65f, 31.35f })
                Box("PedestrianRoute", yard, new Vector3(x, .016f, -6.2f), new Vector3(.08f, .006f, 10.6f), yellow, false);
            for (int i = 0; i < 7; i++)
                Box("StaffCrossingStripe", yard, new Vector3(30f, .018f, -8.1f + i * .43f), new Vector3(2.5f, .006f, .19f), white, false);
            FloorLabel(yard, "STAFF ENTRY", new Vector3(30f, .025f, -4.6f), .075f, new Color(.70f, .71f, .61f));
            foreach (float x in new[] { 25.9f, 34.1f })
            {
                foreach (float z in new[] { -1.8f, -4.9f })
                    Box("WalkwayGuardPost", yard, new Vector3(x, .53f, z), new Vector3(.085f, 1.06f, .085f), yellow);
                foreach (float y in new[] { .5f, .96f })
                    Rod("WalkwayGuardRail", yard, new Vector3(x, y, -1.8f), new Vector3(x, y, -4.9f), .05f, yellow);
            }
        }

        private static void Boundary(Transform yard)
        {
            Material steel = HqIndustrialMaterials.Steel;
            foreach (float x in new[] { -7f, 43f })
            for (int i = 0; i < 8; i++)
            {
                float z = -2f - i * 4f;
                Box("EstateFencePost", yard, new Vector3(x, 1.05f, z), new Vector3(.09f, 2.1f, .09f), steel);
                if (i < 7)
                {
                    foreach (float y in new[] { .35f, 1.9f })
                        Rod("FenceRail", yard, new Vector3(x, y, z), new Vector3(x, y, z - 4f), .04f, steel);
                    for (int picket = 0; picket < 10; picket++)
                        Rod("FencePicket", yard, new Vector3(x, .3f, z - picket * .4f), new Vector3(x, 1.92f, z - picket * .4f), .018f, steel);
                }
            }
            foreach (float x in new[] { -6.4f, 41.8f })
                Box("BoundaryCurb", yard, new Vector3(x, .11f, -15f), new Vector3(.30f, .22f, 29.5f), HqIndustrialMaterials.Concrete);
            Box("WasteSortPad", yard, new Vector3(-3.9f, -.01f, -6f), new Vector3(4.0f, .16f, 6f), HqIndustrialMaterials.Concrete);
            Drum(yard, new Vector3(-4.6f, .07f, -4.4f), "WASTE OIL", 4f);
            Drum(yard, new Vector3(-3.7f, .07f, -4.4f), "FILTERS", -4f);
            Crate(yard, new Vector3(-4.6f, .07f, -6.8f), new Vector3(1.2f, 1.0f, 1f), "COLLECTION\nBOOKED");
        }

        private static void Bollard(Transform parent, Vector3 at)
        {
            Primitive(PrimitiveType.Cylinder, "LoadingBollard", parent, at + Vector3.up * .52f, new Vector3(.17f, .52f, .17f), HqIndustrialMaterials.Yellow);
            foreach (float y in new[] { .3f, .7f })
                Primitive(PrimitiveType.Cylinder, "BollardBand", parent, at + Vector3.up * y, new Vector3(.178f, .035f, .178f), HqIndustrialMaterials.Steel, false);
        }

        private static void Arrow(Transform parent, Vector3 at, Material paint)
        {
            Box("RouteArrowStem", parent, at, new Vector3(.13f, .005f, 1.5f), paint, false);
            foreach (float side in new[] { -1f, 1f })
            {
                var head = Box("RouteArrowHead", parent, at + new Vector3(side * .30f, 0f, .52f), new Vector3(.13f, .005f, .85f), paint, false);
                head.transform.localRotation = Quaternion.Euler(0f, side * -45f, 0f);
            }
        }

        private static void FloorLabel(Transform parent, string text, Vector3 at, float size, Color color)
        {
            Transform lettering = At("YardStencil", parent, at);
            lettering.localRotation = Quaternion.Euler(90f, 0f, 0f);
            Label(lettering, text, Vector3.zero, size, color);
        }
    }
}
