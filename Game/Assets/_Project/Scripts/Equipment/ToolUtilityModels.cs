using UnityEngine;

namespace Abandoned.Equipment
{
    /// <summary>Carried rescue kit. The trolley shows its folded transport form in a hand slot.</summary>
    public static class ToolUtilityModels
    {
        public static void Medkit(ToolArtBuilder a)
        {
            a.Box(new(0, .006f, .1f), new(.22f, .096f, .165f), a.Paint);
            a.Box(new(0, .013f, .1f), new(.225f, .009f, .17f), a.Rubber);
            a.Box(new(0, .052f, .1f), new(.205f, .018f, .15f), a.Paint);
            a.Box(new(0, .063f, .1f), new(.083f, .003f, .025f), a.Tape);
            a.Box(new(0, .063f, .1f), new(.025f, .003f, .083f), a.Tape);
            foreach (float x in new[] { -.072f, .072f })
            {
                a.Box(new(x, .006f, .01f), new(.028f, .041f, .015f), a.Dark);
                a.Box(new(x, .011f, .001f), new(.019f, .017f, .006f), a.Steel);
                a.Box(new(x, .015f, .188f), new(.038f, .021f, .016f), a.Steel);
            }
            a.Box(new(0, .004f, -.013f), new(.096f, .024f, .015f), a.Rubber);
            foreach (float x in new[] { -.055f, .055f }) a.Box(new(x, .004f, -.001f), new(.016f, .033f, .032f), a.Steel);
            foreach (float x in new[] { -.104f, .104f })
            foreach (float z in new[] { .032f, .167f }) a.Box(new(x, 0, z), new(.027f, .085f, .033f), a.Rubber);
            a.Screws(new(0, 0, .1f), .171f, .113f, .063f);
        }

        public static void Trolley(ToolArtBuilder a)
        {
            const float half = .107f, height = .325f;
            foreach (float x in new[] { -half, half })
            {
                a.Rod(new(x, -.025f, .09f), new(x, height, .14f), .018f, a.Paint);
                a.Cylinder(new(x * 1.16f, -.04f, .092f), new(.075f, .029f, .075f), a.Rubber, new(0, 0, 90));
                a.Cylinder(new(x * 1.18f, -.04f, .092f), new(.035f, .032f, .035f), a.Steel, new(0, 0, 90));
                a.Cylinder(new(x, .095f, .107f), new(.025f, .027f, .025f), a.Steel, new(0, 0, 90));
                a.Beam(new(x, -.005f, .13f), new(x, .13f, .235f), .014f, .014f, a.Steel);
            }
            a.Grip(new(-half, height, .14f), new(half, height, .14f), .024f);
            foreach (float y in new[] { .025f, .135f, .22f })
                if (y < height - .025f) a.Rod(new(-half, y, .105f), new(half, y, .105f), .014f, a.Steel);
            a.Box(new(0, -.009f, .175f), new(half * 2f + .01f, .019f, .15f), a.Paint);
            for (int i = 0; i < 5; i++) a.Box(new((i - 2) * half * .35f, .003f, .175f), new(.008f, .003f, .12f), a.Rubber);
        }

        public static void TransitCase(ToolArtBuilder a)
        {
            a.Box(new(0, .014f, .095f), new(.18f, .11f, .17f), a.Paint);
            a.Box(new(0, .018f, .095f), new(.184f, .007f, .174f), a.Dark);
            foreach (float x in new[] { -.07f, .07f })
            {
                a.Box(new(x, .014f, .007f), new(.023f, .038f, .012f), a.Steel);
                a.Box(new(x, .014f, .181f), new(.028f, .03f, .012f), a.Dark);
            }
            a.Box(new(0, .014f, -.01f), new(.08f, .022f, .014f), a.Rubber);
            a.Screws(new(0, 0, .095f), .13f, .12f, .071f);
        }
    }
}
