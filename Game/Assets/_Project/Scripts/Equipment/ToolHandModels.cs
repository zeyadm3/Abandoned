using UnityEngine;

namespace Abandoned.Equipment
{
    /// <summary>Recognizable forged tools, wrapped handles and consumables rather than colour-coded placeholder blocks.</summary>
    public static class ToolHandModels
    {
        public static void Crowbar(ToolArtBuilder a)
        {
            a.Rod(new(0, 0, -.11f), new(0, 0, .44f), .025f, a.Paint);
            a.Grip(new(0, 0, -.095f), new(0, 0, .065f), .031f);
            a.Rod(new(0, 0, .44f), new(0, .025f, .51f), .029f, a.Steel);
            a.Rod(new(0, .025f, .51f), new(0, .071f, .547f), .028f, a.Steel);
            a.Beam(new(-.012f, .071f, .547f), new(-.012f, .088f, .592f), .016f, .012f, a.Steel);
            a.Beam(new(.012f, .071f, .547f), new(.012f, .088f, .592f), .016f, .012f, a.Steel);
            a.Box(new(0, .012f, -.123f), new(.038f, .012f, .042f), a.Steel, new(-15, 0, 0));
            a.Ring(new(0, 0, .087f), .03f, .011f, a.Steel, new(90, 0, 0));
        }

        public static void Sledgehammer(ToolArtBuilder a)
        {
            a.Beam(new(0, 0, -.115f), new(0, 0, .61f), .032f, .027f, a.Wood);
            a.Grip(new(0, 0, -.105f), new(0, 0, .09f), .038f);
            a.Box(new(0, 0, .6f), new(.24f, .085f, .09f), a.Dark);
            foreach (float x in new[] { -.117f, .117f }) a.Box(new(x, 0, .6f), new(.025f, .081f, .084f), a.Steel);
            a.Box(new(0, .044f, .6f), new(.041f, .006f, .04f), a.Steel);
            a.Box(new(0, .048f, .6f), new(.006f, .005f, .034f), a.Dark);
            a.Box(new(0, -.002f, .51f), new(.039f, .034f, .05f), a.Paint);
            for (int i = 0; i < 4; i++) a.Box(new(.019f, 0, .025f + i * .01f), new(.003f, .025f, .003f), a.Tape);
        }

        public static void BoltCutters(ToolArtBuilder a)
        {
            foreach (float side in new[] { -1f, 1f })
            {
                Vector3 from = new(side * .062f, 0, -.09f), to = new(side * .02f, 0, .36f);
                a.Beam(from, to, .023f, .019f, a.Paint);
                a.Grip(from, Vector3.Lerp(from, to, .35f), .031f);
                a.Beam(to, new(side * .046f, 0, .411f), .035f, .022f, a.Steel);
                a.Beam(new(side * .046f, 0, .411f), new(side * .027f, 0, .483f), .039f, .027f, a.Dark);
                a.Beam(new(side * .027f, 0, .483f), new(side * .006f, 0, .516f), .018f, .023f, a.Steel);
                a.Cylinder(new(side * .027f, .015f, .402f), new(.021f, .009f, .021f), a.Steel);
                a.Box(new(side * .027f, .021f, .402f), new(.012f, .003f, .004f), a.Dark);
            }
            a.Cylinder(new(0, 0, .362f), new(.026f, .035f, .026f), a.Steel);
            a.Box(new(0, -.015f, .414f), new(.076f, .006f, .024f), a.Steel);
        }

        public static void Lockpick(ToolArtBuilder a)
        {
            a.Box(new(0, -.009f, .08f), new(.155f, .022f, .13f), a.Leather);
            a.Box(new(0, -.021f, .08f), new(.17f, .009f, .142f), a.Dark);
            for (int i = 0; i < 4; i++)
            {
                float x = (i - 1.5f) * .031f;
                a.Box(new(x, .005f, .075f), new(.014f, .014f, .077f), a.Rubber);
                a.Beam(new(x, .009f, .11f), new(x, .009f, .21f - i * .013f), .004f, .0025f, a.Steel);
                a.Beam(new(x, .009f, .21f - i * .013f), new(x + (i % 2 == 0 ? .009f : -.006f), .012f, .219f - i * .013f), .004f, .0025f, a.Steel);
                a.Box(new(x, .009f, .042f), new(.015f, .004f, .012f), a.Steel);
            }
            foreach (float x in new[] { -.078f, .078f })
            for (int i = 0; i < 8; i++) a.Box(new(x, .004f, .024f + i * .016f), new(.003f, .002f, .008f), a.Tape);
            a.Box(new(0, .009f, .017f), new(.15f, .009f, .026f), a.Leather);
            a.Cylinder(new(.058f, .016f, .018f), new(.01f, .004f, .01f), a.Steel);
        }

        public static void Canister(ToolArtBuilder a, int kind)
        {
            if (kind == 0)
            {
                a.Cylinder(new(0, .06f, .06f), new(.062f, .16f, .062f), a.Paint);
                a.Ring(new(0, -.018f, .06f), .063f, .009f, a.Steel);
                a.Ring(new(0, .139f, .06f), .059f, .009f, a.Steel);
                a.Cylinder(new(0, .147f, .06f), new(.028f, .015f, .028f), a.Dark);
                a.Box(new(0, .159f, .064f), new(.022f, .013f, .024f), a.Rubber);
                a.Disc(new(0, .157f, .078f), .005f, .004f, a.Steel);
                a.Cylinder(new(0, .052f, .06f), new(.063f, .058f, .063f), a.Tape);
                a.Box(new(0, .05f, .092f), new(.026f, .032f, .002f), a.Dark);
            }
            else if (kind == 1)
            {
                a.Rod(new(0, 0, -.025f), new(0, 0, .235f), .019f, a.Paint);
                a.Rod(new(0, 0, .003f), new(0, 0, .214f), .015f, a.Tape);
                a.Disc(new(0, 0, -.018f), .025f, .018f, a.Dark);
                a.Disc(new(0, 0, .226f), .025f, .024f, a.Rubber);
                a.Ring(new(0, .021f, .235f), .026f, .006f, a.Rubber, new(90, 0, 0));
                a.Box(new(.016f, 0, .204f), new(.012f, .008f, .052f), a.Rubber);
            }
            else
            {
                a.Rod(new(0, 0, -.025f), new(0, 0, .25f), .034f, a.Paint);
                a.Disc(new(0, 0, .255f), .037f, .026f, a.Dark);
                a.Ring(new(0, 0, .274f), .04f, .009f, a.Steel, new(90, 0, 0));
                a.Grip(new(0, 0, -.021f), new(0, 0, .08f), .037f);
                a.Ring(new(0, 0, .16f), .038f, .027f, a.Tape, new(90, 0, 0));
                a.Box(new(.018f, 0, .025f), new(.006f, .016f, .115f), a.Steel);
                a.Cable(new(0, -.015f, -.032f), new(.022f, -.009f, -.027f), new(0, -.026f, -.025f), .0035f, a.Rubber);
            }
        }

        public static void Planks(ToolArtBuilder a)
        {
            for (int i = 0; i < 3; i++)
            {
                float x = (i - 1) * .075f;
                a.Box(new(x, 0, .19f + (i - 1) * .012f), new(.072f, .035f, .59f), a.Wood);
                for (int edge = 0; edge < 2; edge++)
                {
                    a.Box(new(x, .019f, -.07f + edge * .53f), new(.06f, .002f, .017f), a.Steel);
                    foreach (float dx in new[] { -.018f, .018f }) a.Cylinder(new(x + dx, .021f, -.07f + edge * .53f), new(.005f, .002f, .005f), a.Dark);
                }
            }
            foreach (float z in new[] { .035f, .335f })
            {
                a.Box(new(0, .022f, z), new(.231f, .006f, .025f), a.Canvas);
                a.Box(new(0, -.023f, z), new(.231f, .006f, .025f), a.Canvas);
                foreach (float x in new[] { -.114f, .114f }) a.Box(new(x, 0, z), new(.006f, .05f, .025f), a.Canvas);
                a.Box(new(.05f, .028f, z), new(.036f, .009f, .033f), a.Steel);
            }
        }
    }
}
