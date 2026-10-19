using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;

namespace Abandoned.EditorTools
{
    /// <summary>Cell furniture built once per condition; fine details stay inside the existing solid footprints.</summary>
    public static class IslandCellFurnishings
    {
        public static void Build(Transform r, IslandPrisonArt art, int variant)
        {
            Bunk(r, art, variant);
            Sanitary(r, art, variant);
            Desk(r, art, variant);
            Shelves(r, art, variant);
            IslandCellSurfaceDetails.Build(r, art, variant);
        }

        private static void Bunk(Transform r, IslandPrisonArt art, int variant)
        {
            const float x = -.58f, top = .46f;
            Box("BedFrame", r, new Vector3(x, .4f, 1.15f), new Vector3(.72f, .08f, 1.95f), art.Steel, false);
            foreach (float side in new[] { -.92f, -.24f })
            {
                Box("AngleRail", r, new Vector3(side, .37f, 1.15f), new Vector3(.04f, .12f, 1.95f), art.Rust, false);
                foreach (float z in new[] { .25f, 2.05f })
                    Box("BedLeg", r, new Vector3(side, .2f, z), new Vector3(.045f, .38f, .045f), art.Steel, false);
            }
            foreach (float z in new[] { .24f, 2.06f })
            {
                Box("WallBracket", r, new Vector3(-.94f, .53f, z), new Vector3(.035f, .28f, .13f), art.Steel, false);
                Box("Suspension", r, new Vector3(-.25f, 1.01f, z), new Vector3(.018f, 1.1f, .018f), art.Rust, false);
                Box("SuspensionEye", r, new Vector3(-.25f, 1.58f, z), new Vector3(.065f, .06f, .065f), art.Steel, false);
            }
            for (int i = 0; i < 10; i++)
                Box("SpringSlat", r, new Vector3(x, .454f, .34f + i * .18f), new Vector3(.66f, .018f, .028f), art.Bars, false);
            foreach (float xx in new[] { -.76f, -.58f, -.4f })
                Box("SpringWire", r, new Vector3(xx, .468f, 1.15f), new Vector3(.011f, .009f, 1.82f), art.Steel, false);

            if (variant is 1 or 7)
            {
                if (variant == 7)
                    Box("BedRollStrap", r, new Vector3(x, .485f, 1.72f), new Vector3(.67f, .018f, .045f), art.Canvas, false);
                return;
            }

            float thickness = variant == 5 ? .07f : .11f;
            Box("Mattress", r, new Vector3(x, top + thickness * .5f, 1.15f), new Vector3(.67f, thickness, 1.88f), art.Mattress, false);
            foreach (float side in new[] { -.914f, -.246f })
            {
                Box("MattressPiping", r, new Vector3(side, top + thickness - .016f, 1.15f), new Vector3(.009f, .012f, 1.82f), art.Canvas, false);
                for (int stitch = 0; stitch < 6; stitch++)
                    Box("TickingStitch", r, new Vector3(side, top + .034f, .4f + stitch * .27f), new Vector3(.011f, .024f, .035f), art.Blanket, false);
            }
            if (variant != 5)
            {
                Box("PillowCase", r, new Vector3(x, top + thickness + .045f, .39f), new Vector3(.5f, .08f, .3f), art.Canvas, false);
                Box("PillowHem", r, new Vector3(x + .2f, top + thickness + .087f, .39f), new Vector3(.012f, .005f, .27f), art.Mattress, false);
            }

            if (variant == 3)
            {
                for (int fold = 0; fold < 3; fold++)
                    Box("FoldedBlanket", r, new Vector3(x, top + thickness + .018f + fold * .018f, 1.81f),
                        new Vector3(.61f - fold * .015f, .017f, .42f), art.Blanket, false);
            }
            else
            {
                float length = variant == 2 ? .66f : variant == 5 ? .92f : 1.18f;
                Box("Cover", r, new Vector3(x, top + thickness + .012f, 1.54f), new Vector3(.66f, .025f, length), art.Blanket, false);
                Box("CoverTurnDown", r, new Vector3(x, top + thickness + .028f, 1.54f - length * .5f), new Vector3(.66f, .023f, .13f), art.Canvas, false);
                foreach (float side in new[] { -.907f, -.253f })
                    Box("CoverHem", r, new Vector3(side, top + thickness + .027f, 1.54f), new Vector3(.009f, .005f, length - .035f), art.Canvas, false);
                if (variant is 2 or 5)
                {
                    // Cloth hangs beside the frame, inside its collision footprint instead of crossing the cell.
                    Box("HangingCover", r, new Vector3(-.235f, .31f, 1.65f), new Vector3(.012f, .35f, .52f), art.Blanket, false);
                    for (int tear = 0; tear < 4; tear++)
                        Box("FrayedEdge", r, new Vector3(-.23f, .12f - tear * .008f, 1.44f + tear * .11f), new Vector3(.012f, .06f, .045f), art.Canvas, false);
                }
            }
            if (variant == 4)
            {
                Box("SewnPatch", r, new Vector3(-.48f, top + thickness + .029f, 1.76f), new Vector3(.22f, .006f, .24f), art.Canvas, false);
                for (int i = 0; i < 5; i++)
                    Box("RepairThread", r, new Vector3(-.58f + i * .043f, top + thickness + .034f, 1.88f), new Vector3(.017f, .003f, .027f), art.Blanket, false);
            }
        }

        private static void Sanitary(Transform r, IslandPrisonArt art, int variant)
        {
            Box("Cistern", r, new Vector3(.62f, .58f, .105f), new Vector3(.39f, .27f, .17f), art.Porcelain, false);
            Box("CisternLid", r, new Vector3(.62f, .726f, .11f), new Vector3(.42f, .035f, .19f), art.Porcelain, false);
            Pipe("FlushLever", r, new Vector3(.79f, .62f, .21f), new Vector3(.69f, .62f, .21f), .028f, art.Steel);
            Pipe("FlushPipe", r, new Vector3(.62f, .46f, .13f), new Vector3(.62f, .28f, .13f), .055f, art.Steel);
            Primitive(PrimitiveType.Cylinder, "Pedestal", r, new Vector3(.62f, .15f, .43f), new Vector3(.23f, .145f, .29f), art.Porcelain, false);
            Primitive(PrimitiveType.Cylinder, "Bowl", r, new Vector3(.62f, .31f, .43f), new Vector3(.36f, .115f, .46f), art.Porcelain, false);
            Primitive(PrimitiveType.Cylinder, "BowlRecess", r, new Vector3(.62f, .427f, .43f), new Vector3(.265f, .004f, .345f), art.Bars, false);
            for (int i = 0; i < 12; i++)
            {
                float angle = i * Mathf.PI * 2f / 12f;
                GameObject seat = Box("SeatRim", r, new Vector3(.62f + Mathf.Cos(angle) * .163f, .443f, .43f + Mathf.Sin(angle) * .21f),
                    new Vector3(.096f, .022f, .045f), art.Porcelain, false);
                seat.transform.localRotation = Quaternion.Euler(0f, -angle * Mathf.Rad2Deg - 90f, 0f);
            }
            foreach (float xx in new[] { .48f, .76f })
                Box("SeatHinge", r, new Vector3(xx, .446f, .23f), new Vector3(.04f, .035f, .055f), art.Steel, false);

            Box("BasinShell", r, new Vector3(.12f, .8f, .2f), new Vector3(.42f, .12f, .32f), art.Porcelain, false);
            Box("BasinWell", r, new Vector3(.12f, .863f, .21f), new Vector3(.295f, .008f, .205f), variant == 5 ? art.Stain : art.Dado, false);
            foreach (float xx in new[] { -.074f, .314f })
                Box("BasinRim", r, new Vector3(xx, .88f, .2f), new Vector3(.035f, .03f, .32f), art.Porcelain, false);
            foreach (float zz in new[] { .055f, .345f })
                Box("BasinRim", r, new Vector3(.12f, .88f, zz), new Vector3(.36f, .03f, .03f), art.Porcelain, false);
            Primitive(PrimitiveType.Cylinder, "BasinDrain", r, new Vector3(.12f, .87f, .22f), new Vector3(.033f, .004f, .033f), art.Steel, false);
            Pipe("TapStem", r, new Vector3(.12f, .87f, .065f), new Vector3(.12f, 1.01f, .065f), .032f, art.Steel);
            Pipe("TapSpout", r, new Vector3(.12f, 1.01f, .065f), new Vector3(.12f, 1.01f, .17f), .032f, art.Steel);
            Pipe("TapHandle", r, new Vector3(.065f, .965f, .065f), new Vector3(.175f, .965f, .065f), .02f, art.Brass);
            Pipe("BasinWaste", r, new Vector3(.12f, .74f, .19f), new Vector3(.12f, .35f, .19f), .045f, art.Steel);
            Pipe("WasteReturn", r, new Vector3(.12f, .35f, .19f), new Vector3(.12f, .35f, .055f), .045f, art.Rust);
            Box("MirrorBacking", r, new Vector3(.12f, 1.35f, .03f), new Vector3(.3f, .39f, .025f), art.Bars, false);
            Box("MirrorFace", r, new Vector3(.12f, 1.35f, .047f), new Vector3(.255f, .34f, .009f), art.Steel, false);
            foreach (float yy in new[] { 1.18f, 1.52f })
                Box("MirrorClip", r, new Vector3(.12f, yy, .058f), new Vector3(.055f, .025f, .013f), art.Brass, false);
        }

        private static void Desk(Transform r, IslandPrisonArt art, int variant)
        {
            Box("DeskTop", r, new Vector3(.74f, .76f, 1.55f), new Vector3(.5f, .045f, .7f), art.Steel, false);
            Box("DeskWear", r, new Vector3(.74f, .787f, 1.55f), new Vector3(.42f, .008f, .62f), art.Board, false);
            foreach (float z in new[] { 1.29f, 1.81f })
            {
                Box("DeskBracket", r, new Vector3(.948f, .57f, z), new Vector3(.035f, .36f, .055f), art.Steel, false);
                Pipe("DeskStay", r, new Vector3(.945f, .43f, z), new Vector3(.55f, .73f, z), .022f, art.Steel);
                Box("Hinge", r, new Vector3(.947f, .756f, z), new Vector3(.065f, .06f, .1f), art.Rust, false);
            }
            Primitive(PrimitiveType.Cylinder, "StoolSeat", r, new Vector3(.55f, .235f, 1.5f), new Vector3(.29f, .025f, .29f), art.Steel, false);
            Pipe("StoolPost", r, new Vector3(.55f, .015f, 1.5f), new Vector3(.55f, .21f, 1.5f), .045f, art.Steel);
            Primitive(PrimitiveType.Cylinder, "StoolFoot", r, new Vector3(.55f, .02f, 1.5f), new Vector3(.18f, .018f, .18f), art.Rust, false);
            for (int scratch = 0; scratch < 3 + variant % 3; scratch++)
            {
                GameObject cut = Box("DeskScratch", r, new Vector3(.63f + scratch * .035f, .793f, 1.38f + (scratch % 2) * .09f), new Vector3(.009f, .002f, .13f), art.Crack, false);
                cut.transform.localRotation = Quaternion.Euler(0f, -24f + scratch * 11f, 0f);
            }
        }

        private static void Shelves(Transform r, IslandPrisonArt art, int variant)
        {
            foreach (float y in new[] { 1.48f, 1.8f })
            {
                Box("FixedShelf", r, new Vector3(-.32f, y, .15f), new Vector3(.82f, .035f, .27f), art.Steel, false);
                Box("ShelfRolledLip", r, new Vector3(-.32f, y + .027f, .28f), new Vector3(.82f, .035f, .015f), art.Rust, false);
                foreach (float x in new[] { -.65f, .015f })
                {
                    Box("ShelfMount", r, new Vector3(x, y - .06f, .035f), new Vector3(.045f, .17f, .02f), art.Steel, false);
                    Pipe("ShelfStay", r, new Vector3(x, y - .13f, .035f), new Vector3(x, y - .025f, .245f), .014f, art.Steel);
                }
            }
            if (variant is 0 or 3 or 4 or 6)
            {
                Box("AttachedCloth", r, new Vector3(-.56f, 1.32f, .285f), new Vector3(.21f, .34f, .012f), art.Canvas, false);
                Box("ClothHem", r, new Vector3(-.56f, 1.16f, .293f), new Vector3(.21f, .018f, .006f), art.Blanket, false);
            }
        }

        public static GameObject Pipe(string name, Transform r, Vector3 from, Vector3 to, float width, Material material)
        {
            Vector3 delta = to - from;
            GameObject go = Primitive(PrimitiveType.Cylinder, name, r, (from + to) * .5f, new Vector3(width, delta.magnitude * .5f, width), material, false);
            go.transform.localRotation = Quaternion.FromToRotation(Vector3.up, delta.normalized);
            return go;
        }
    }
}
