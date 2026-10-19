using UnityEngine;

namespace Abandoned.Equipment
{
    /// <summary>Field electronics: protected lenses, formed instrument cases and serviceable mechanical fittings.</summary>
    public static class ToolElectronicModels
    {
        public static void Flashlight(ToolArtBuilder a)
        {
            a.Rod(new(0, 0, -.065f), new(0, 0, .155f), .061f, a.Paint);
            a.Grip(new(0, 0, -.045f), new(0, 0, .09f), .064f);
            a.Disc(new(0, 0, .155f), .078f, .036f, a.Steel);
            a.Disc(new(0, 0, .208f), .102f, .08f, a.Paint);
            a.Ring(new(0, 0, .252f), .108f, .013f, a.Steel, new(90, 0, 0));
            a.Disc(new(0, 0, .25f), .073f, .006f, a.Glass);
            a.Disc(new(0, 0, .245f), .025f, .009f, a.Tape);
            for (int i = 0; i < 5; i++) a.Ring(new(0, 0, .17f + i * .012f), .098f, .004f, a.Dark, new(90, 0, 0));
            a.Box(new(0, .031f, .103f), new(.024f, .009f, .039f), a.Rubber);
            a.Box(new(.034f, 0, .01f), new(.008f, .014f, .092f), a.Steel);
            a.Ring(new(0, -.025f, -.075f), .025f, .004f, a.Steel, new(90, 0, 0));
            a.Cable(new(0, -.025f, -.075f), new(.028f, -.012f, -.05f), new(0, -.045f, -.035f), .005f, a.Rubber);
        }

        public static void Radio(ToolArtBuilder a)
        {
            a.Box(new(0, .06f, .04f), new(.086f, .19f, .052f), a.Paint);
            foreach (float x in new[] { -.043f, .043f }) a.Box(new(x, .06f, .039f), new(.01f, .164f, .058f), a.Rubber);
            a.Box(new(0, .11f, .011f), new(.056f, .043f, .007f), a.Dark);
            a.Box(new(0, .111f, .006f), new(.045f, .027f, .003f), a.Screen);
            for (int i = 0; i < 7; i++) a.Box(new(0, .058f + i * .0055f, .009f), new(.052f, .0025f, .004f), a.Dark);
            for (int row = 0; row < 2; row++)
            for (int col = 0; col < 3; col++) a.Box(new((col - 1) * .016f, .015f + row * .016f, .009f), new(.011f, .011f, .004f), a.Rubber);
            a.Cylinder(new(.022f, .164f, .038f), new(.021f, .021f, .021f), a.Rubber);
            a.Cylinder(new(-.021f, .164f, .038f), new(.017f, .021f, .017f), a.Steel);
            a.Rod(new(-.026f, .168f, .054f), new(-.03f, .315f, .056f), .009f, a.Rubber);
            a.Box(new(-.049f, .08f, .04f), new(.009f, .053f, .026f), a.Rubber);
            a.Box(new(0, .063f, .072f), new(.028f, .11f, .013f), a.Steel);
            a.Box(new(0, -.036f, .04f), new(.07f, .012f, .05f), a.Dark);
            foreach (float x in new[] { -.032f, .032f }) a.Disc(new(x, -.021f, .011f), .007f, .003f, a.Steel);
        }

        public static void Detector(ToolArtBuilder a, int kind)
        {
            a.Box(new(0, .013f, .13f), new(kind == 1 ? .16f : .13f, .08f, .22f), a.Paint);
            foreach (float x in new[] { -.064f, .064f }) a.Box(new(x, .014f, .13f), new(.018f, .09f, .205f), a.Rubber);
            a.Box(new(0, .055f, .125f), new(.108f, .013f, .132f), a.Dark);
            a.Box(new(0, .063f, .13f), new(.088f, .003f, .103f), a.Screen);
            a.Screws(new(0, 0, .13f), .098f, .174f, .057f);
            for (int i = 0; i < 3; i++) a.Box(new((i - 1) * .028f, .057f, .032f), new(.02f, .008f, .014f), i == 2 ? a.Paint : a.Rubber);
            a.Grille(new(0, .056f, .218f), new(.071f, .024f), 4, a.Steel);
            if (kind == 0)
            {
                a.Box(new(0, -.066f, .06f), new(.056f, .095f, .062f), a.Rubber, new(-14, 0, 0));
                a.Rod(new(-.038f, -.012f, .235f), new(-.038f, -.012f, .292f), .022f, a.Steel);
                a.Rod(new(.038f, -.012f, .235f), new(.038f, -.012f, .292f), .022f, a.Steel);
                a.Cable(new(.04f, -.023f, .19f), new(.035f, -.07f, .08f), new(.033f, -.012f, .03f), .007f, a.Rubber);
            }
            else if (kind == 1)
            {
                a.Disc(new(0, .014f, .255f), .088f, .045f, a.Rubber);
                a.Disc(new(0, .014f, .28f), .065f, .008f, a.Dark);
                a.Box(new(0, -.035f, .13f), new(.1f, .026f, .16f), a.Rubber);
                a.Rod(new(-.065f, .046f, .21f), new(-.065f, .046f, .28f), .006f, a.Steel);
            }
            else
            {
                a.Box(new(0, -.076f, .075f), new(.047f, .115f, .064f), a.Rubber, new(-18, 0, 0));
                a.Disc(new(0, .008f, .267f), .082f, .067f, a.Dark);
                a.Ring(new(0, .008f, .303f), .086f, .013f, a.Steel, new(90, 0, 0));
                a.Disc(new(0, .008f, .309f), .055f, .004f, a.Glass);
                a.Box(new(.044f, -.028f, .27f), new(.021f, .025f, .023f), a.Dark);
            }
        }

        public static void NightVision(ToolArtBuilder a)
        {
            a.Box(new(0, 0, .09f), new(.17f, .065f, .075f), a.Paint);
            foreach (float x in new[] { -.047f, .047f })
            {
                a.Rod(new(x, 0, -.014f), new(x, 0, .204f), .057f, a.Rubber);
                a.Ring(new(x, 0, .2f), .069f, .018f, a.Steel, new(90, 0, 0));
                a.Disc(new(x, 0, .211f), .047f, .005f, a.Glass);
                a.Ring(new(x, 0, -.016f), .071f, .023f, a.Rubber, new(90, 0, 0));
                a.Disc(new(x, 0, -.028f), .04f, .006f, a.Glass);
                for (int i = 0; i < 4; i++) a.Ring(new(x, 0, .105f + i * .012f), .061f, .004f, a.Dark, new(90, 0, 0));
            }
            a.Box(new(0, .054f, .09f), new(.046f, .05f, .045f), a.Steel);
            a.Box(new(0, .08f, .09f), new(.071f, .014f, .058f), a.Dark);
            a.Cylinder(new(.025f, .043f, .11f), new(.019f, .02f, .019f), a.Rubber);
            a.Screws(new(0, 0, .09f), .132f, .047f, .035f);
        }

        public static void Battery(ToolArtBuilder a)
        {
            a.Box(new(0, .02f, .08f), new(.095f, .057f, .15f), a.Paint);
            for (int i = 0; i < 3; i++) a.Rod(new((i - 1) * .029f, .018f, .025f), new((i - 1) * .029f, .018f, .128f), .027f, a.Rubber);
            a.Box(new(0, .022f, .15f), new(.095f, .065f, .025f), a.Dark);
            foreach (float x in new[] { -.024f, .024f }) a.Box(new(x, .057f, .148f), new(.017f, .005f, .019f), a.Steel);
            a.Box(new(0, .052f, .077f), new(.074f, .004f, .048f), a.Tape);
            a.Box(new(0, -.018f, .079f), new(.057f, .006f, .11f), a.Dark);
            a.Screws(new(0, 0, .075f), .073f, .105f, .049f);
        }

        public static void NoiseMaker(ToolArtBuilder a)
        {
            a.Box(new(0, 0, .08f), new(.092f, .115f, .135f), a.Paint);
            a.Disc(new(0, 0, .155f), .105f, .023f, a.Dark);
            a.Ring(new(0, 0, .172f), .107f, .008f, a.Steel, new(90, 0, 0));
            for (int i = -3; i <= 3; i++) a.Box(new(i * .011f, 0, .176f), new(.004f, .077f - Mathf.Abs(i) * .009f, .005f), a.Steel);
            a.Box(new(0, .062f, .065f), new(.048f, .012f, .046f), a.Rubber);
            a.Ring(new(0, .078f, .025f), .035f, .004f, a.Steel, new(90, 0, 0));
            a.Box(new(0, -.061f, .079f), new(.071f, .009f, .096f), a.Dark);
            a.Screws(new(0, 0, .078f), .066f, .1f, .059f);
        }

        public static void Drone(ToolArtBuilder a)
        {
            a.Box(new(0, .016f, .14f), new(.115f, .06f, .16f), a.Paint);
            a.Box(new(0, .051f, .14f), new(.073f, .013f, .111f), a.Rubber);
            foreach (float side in new[] { -1f, 1f })
            foreach (float fore in new[] { -1f, 1f })
            {
                Vector3 motor = new(side * .14f, .024f, .14f + fore * .115f);
                a.Beam(new(side * .04f, .017f, .14f + fore * .045f), motor, .025f, .019f, a.Dark);
                a.Cylinder(motor, new(.034f, .027f, .034f), a.Steel);
                a.Box(motor + Vector3.up * .017f, new(.139f, .003f, .015f), a.Rubber, new(0, side * fore * 24f, 0));
                a.Ring(motor + Vector3.up * .014f, .16f, .004f, a.Dark);
                a.Rod(motor + new Vector3(-.06f, 0, 0), motor + new Vector3(.06f, 0, 0), .004f, a.Dark);
                a.Rod(motor + new Vector3(0, 0, -.06f), motor + new Vector3(0, 0, .06f), .004f, a.Dark);
            }
            a.Box(new(0, -.03f, .217f), new(.051f, .035f, .045f), a.Dark);
            a.Disc(new(0, -.03f, .244f), .027f, .009f, a.Glass);
            foreach (float x in new[] { -.045f, .045f }) a.Rod(new(x, -.02f, .095f), new(x, -.065f, .21f), .008f, a.Steel);
            a.Screws(new(0, 0, .14f), .082f, .124f, .051f);
        }

        public static void Floodlight(ToolArtBuilder a)
        {
            a.Box(new(0, .11f, .135f), new(.235f, .17f, .055f), a.Paint);
            a.Box(new(0, .11f, .169f), new(.212f, .148f, .012f), a.Dark);
            a.Box(new(0, .11f, .177f), new(.193f, .13f, .004f), a.Glass);
            for (int row = 0; row < 3; row++)
            for (int col = 0; col < 5; col++) a.Disc(new((col - 2) * .034f, .11f + (row - 1) * .037f, .176f), .018f, .003f, a.Tape);
            foreach (float x in new[] { -.133f, .133f })
            {
                a.Rod(new(x, -.025f, .09f), new(x, .12f, .135f), .015f, a.Steel);
                a.Cylinder(new(x, .11f, .135f), new(.026f, .02f, .026f), a.Rubber, new(0, 0, 90));
                a.Rod(new(x, -.025f, .09f), new(x, -.025f, .205f), .015f, a.Dark);
            }
            a.Rod(new(-.133f, -.025f, .2f), new(.133f, -.025f, .2f), .015f, a.Dark);
            for (int i = 0; i < 7; i++) a.Box(new((i - 3) * .027f, .11f, .099f), new(.007f, .14f, .031f), a.Steel);
            a.Box(new(0, .214f, .132f), new(.105f, .014f, .03f), a.Rubber);
            foreach (float x in new[] { -.045f, .045f }) a.Box(new(x, .198f, .132f), new(.013f, .035f, .027f), a.Dark);
            a.Box(new(0, -.026f, .126f), new(.13f, .032f, .075f), a.Rubber);
        }
    }
}
