using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;

namespace Abandoned.EditorTools
{
    /// <summary>Fixed paint, marks and service fittings give small cells a history without adding pickup clutter.</summary>
    public static class IslandCellSurfaceDetails
    {
        public static void Build(Transform r, IslandPrisonArt art, int variant)
        {
            Box("BackDado", r, new Vector3(0f, .6f, .014f), new Vector3(1.86f, 1.2f, .012f), art.Dado, false);
            foreach (float x in new[] { -.923f, .923f })
            {
                Box("SideDado", r, new Vector3(x, .61f, 1.24f), new Vector3(.009f, 1.18f, 2.31f), art.Dado, false);
                Box("PaintLine", r, new Vector3(x + (x < 0f ? .008f : -.008f), 1.216f, 1.24f), new Vector3(.006f, .025f, 2.31f), art.Facade, false);
                Box("Skirting", r, new Vector3(x + (x < 0f ? .008f : -.008f), .075f, 1.24f), new Vector3(.02f, .12f, 2.31f), art.FacadeDark, false);
            }
            Box("BackPaintLine", r, new Vector3(0f, 1.217f, .026f), new Vector3(1.85f, .025f, .006f), art.Facade, false);
            Box("BackSkirting", r, new Vector3(0f, .075f, .033f), new Vector3(1.85f, .12f, .025f), art.FacadeDark, false);
            Vent(r, art, variant);
            Wear(r, art, variant);
            PersonalTraces(r, art, variant);
        }

        private static void Vent(Transform r, IslandPrisonArt art, int variant)
        {
            Box("VentRecess", r, new Vector3(-.48f, 2.79f, .035f), new Vector3(.62f, .26f, .026f), art.Bars, false);
            foreach (float y in new[] { 2.655f, 2.925f })
                Box("VentFrame", r, new Vector3(-.48f, y, .062f), new Vector3(.68f, .03f, .045f), art.Steel, false);
            foreach (float x in new[] { -.805f, -.155f })
                Box("VentFrame", r, new Vector3(x, 2.79f, .062f), new Vector3(.03f, .24f, .045f), art.Steel, false);
            for (int i = 0; i < 5; i++)
            {
                GameObject slat = Box("VentLouvre", r, new Vector3(-.48f, 2.69f + i * .05f, .07f), new Vector3(.59f, .024f, .055f), variant == 5 ? art.Rust : art.Railing, false);
                slat.transform.localRotation = Quaternion.Euler(22f, 0f, 0f);
            }
            IslandCellFurnishings.Pipe("LightingConduit", r, new Vector3(.76f, 1.95f, .047f), new Vector3(.76f, 3.42f, .047f), .018f, art.Steel);
            Box("ConduitJunction", r, new Vector3(.76f, 2.18f, .045f), new Vector3(.095f, .13f, .055f), art.Steel, false);
            foreach (float y in new[] { 2.4f, 3.06f })
                Box("ConduitClip", r, new Vector3(.76f, y, .062f), new Vector3(.055f, .025f, .028f), art.Rust, false);
            // A dead recessed fitting belongs to the fixed room; it creates no extra light or interactable.
            Box("CeilingFittingBase", r, new Vector3(.14f, 3.42f, .085f), new Vector3(.39f, .19f, .13f), art.Steel, false);
            Box("CeilingFittingLens", r, new Vector3(.14f, 3.39f, .158f), new Vector3(.3f, .11f, .018f), art.LampOff, false);
            foreach (float x in new[] { .02f, .14f, .26f })
                Box("FittingGuard", r, new Vector3(x, 3.39f, .176f), new Vector3(.012f, .15f, .014f), art.Bars, false);
        }

        private static void Wear(Transform r, IslandPrisonArt art, int variant)
        {
            int patches = variant is 1 or 2 or 5 or 7 ? 8 : 3;
            for (int i = 0; i < patches; i++)
            {
                float h = IslandKit.Hash(i + variant * 23, 801);
                float y = .3f + IslandKit.Hash(i, variant + 71) * .74f;
                GameObject scar = Box("FlakedDado", r, new Vector3(-.76f + h * 1.45f, y, .024f),
                    new Vector3(.035f + h * .09f, .06f + IslandKit.Hash(i, 91) * .12f, .005f), art.FacadeDark, false);
                scar.transform.localRotation = Quaternion.Euler(0f, 0f, h * 30f - 15f);
            }
            foreach (float z in new[] { .75f, 1.38f, 2.03f })
            {
                float y = .96f + IslandKit.Hash((int)(z * 100f), variant) * .14f;
                Box("BedWallRub", r, new Vector3(-.914f, y, z), new Vector3(.004f, .018f, .16f + variant * .008f), art.Stain, false);
            }
            if (variant is 2 or 5 or 7)
            {
                for (int i = 0; i < 5; i++)
                {
                    float length = .22f + IslandKit.Hash(i, variant + 15) * .55f;
                    Box("OldDampTrace", r, new Vector3(-.82f + i * .045f, 2.58f - length * .5f, .018f),
                        new Vector3(.014f, length, .004f), art.Stain, false);
                }
            }
            // Inset lines remain thin enough to read as damage to the wall, never a new dark panel.
            for (int i = 0; i < 3; i++)
            {
                GameObject crack = Box("HairlineCrack", r, new Vector3(.61f + i * .03f, 2.62f - i * .12f, .012f),
                    new Vector3(.006f, .17f, .004f), art.Crack, false);
                crack.transform.localRotation = Quaternion.Euler(0f, 0f, (i % 2 == 0 ? 1f : -1f) * (11f + variant));
            }
        }

        private static void PersonalTraces(Transform r, IslandPrisonArt art, int variant)
        {
            if (variant is 1 or 5 or 7)
            {
                // Discoloured rectangles and tape ghosts are what's left after possessions were removed.
                Box("RemovedNoticeGhost", r, new Vector3(.914f, 1.78f, 1.53f), new Vector3(.004f, .3f, .39f), art.Facade, false);
                foreach (float z in new[] { 1.35f, 1.71f })
                    Box("OldTape", r, new Vector3(.909f, 1.91f, z), new Vector3(.004f, .045f, .065f), art.Canvas, false);
            }
            else
            {
                int notes = variant == 6 ? 4 : 2;
                for (int i = 0; i < notes; i++)
                {
                    float z = 1.28f + (i % 2) * .29f, y = 1.65f + i / 2 * .25f;
                    Box("PinnedPaperRemnant", r, new Vector3(.914f, y, z), new Vector3(.004f, .2f, .23f), art.Paper, false);
                    Box("PaperPin", r, new Vector3(.906f, y + .085f, z), new Vector3(.009f, .018f, .018f), art.Steel, false);
                    for (int line = 0; line < 3; line++)
                        Box("FadedPencilLine", r, new Vector3(.909f, y + .04f - line * .035f, z + line * .008f),
                            new Vector3(.003f, .003f, .1f - line * .018f), art.Stain, false);
                }
            }
            int groups = variant == 4 ? 3 : variant == 7 ? 4 : 1;
            for (int group = 0; group < groups; group++)
            {
                float x = -.65f + group * .16f;
                for (int day = 0; day < 4; day++)
                    Box("ScratchedTally", r, new Vector3(x + day * .027f, 2.25f, .013f), new Vector3(.005f, .085f, .004f), art.Stain, false);
                GameObject slash = Box("TallySlash", r, new Vector3(x + .04f, 2.25f, .017f), new Vector3(.005f, .15f, .004f), art.Stain, false);
                slash.transform.localRotation = Quaternion.Euler(0f, 0f, -55f);
            }
        }
    }
}
