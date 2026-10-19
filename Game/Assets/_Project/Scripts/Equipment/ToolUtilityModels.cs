using UnityEngine;

namespace Abandoned.Equipment
{
    /// <summary>Carried rescue and rigging kit. Trolleys and mats show their folded transport form in a hand slot.</summary>
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

        public static void Backpack(ToolArtBuilder a)
        {
            a.Box(new(0, .059f, .1f), new(.21f, .245f, .122f), a.Canvas);
            a.Box(new(0, .18f, .102f), new(.198f, .055f, .124f), a.Canvas);
            a.Box(new(0, .029f, .18f), new(.169f, .145f, .05f), a.Canvas);
            a.Box(new(0, .06f, .209f), new(.174f, .008f, .006f), a.Dark);
            foreach (float x in new[] { -.068f, .068f })
            {
                a.Box(new(x, .078f, .169f), new(.025f, .19f, .012f), a.Dark);
                a.Box(new(x, .011f, .19f), new(.031f, .041f, .019f), a.Rubber);
                a.Box(new(x, .011f, .202f), new(.018f, .02f, .004f), a.Steel);
                a.Cable(new(x, .162f, .036f), new(x, -.05f, .035f), new(0, -.011f, -.055f), .024f, a.Dark);
                a.Box(new(x, .04f, .217f), new(.024f, .002f, .019f), a.Tape);
            }
            foreach (float x in new[] { -.116f, .116f }) a.Box(new(x, .029f, .1f), new(.038f, .13f, .09f), a.Canvas);
            a.Cable(new(-.041f, .206f, .103f), new(.041f, .206f, .103f), new(0, .036f, 0), .012f, a.Dark);
            a.Box(new(0, -.067f, .103f), new(.21f, .025f, .127f), a.Rubber);
            for (int i = 0; i < 8; i++)
            foreach (float x in new[] { -.089f, .089f }) a.Box(new(x, -.025f + i * .027f, .164f), new(.002f, .009f, .002f), a.Tape);
        }

        public static void Fabric(ToolArtBuilder a, bool mat)
        {
            if (mat)
            {
                for (int i = 0; i < 3; i++)
                {
                    float y = -.021f + i * .042f;
                    a.Box(new(0, y, .104f), new(.26f, .04f, .25f), a.Canvas);
                    a.Box(new(0, y + .018f, .104f), new(.255f, .003f, .245f), a.Dark);
                    foreach (float x in new[] { -.122f, .122f }) a.Box(new(x, y, .104f), new(.006f, .036f, .25f), a.Dark);
                }
                foreach (float x in new[] { -.08f, .08f })
                {
                    a.Box(new(x, .09f, .104f), new(.023f, .007f, .26f), a.Dark);
                    a.Box(new(x, -.044f, .104f), new(.023f, .007f, .26f), a.Dark);
                    a.Box(new(x, .02f, -.023f), new(.023f, .13f, .007f), a.Dark);
                    a.Box(new(x, .02f, .231f), new(.023f, .13f, .007f), a.Dark);
                    a.Box(new(x, .098f, .17f), new(.034f, .012f, .029f), a.Steel);
                }
                a.Cable(new(-.044f, .098f, .103f), new(.044f, .098f, .103f), new(0, .025f, 0), .009f, a.Dark);
            }
            else
            {
                for (int i = 0; i < 4; i++) a.Box(new(0, -.02f + i * .019f, .1f), new(.245f, .018f, .22f), a.Canvas);
                for (int row = 0; row < 5; row++)
                for (int col = 0; col < 6; col++)
                {
                    Vector3 p = new(-.104f + col * .042f, .047f, .014f + row * .043f);
                    a.Box(p, new(.002f, .0015f, .041f), a.Dark);
                    a.Box(p, new(.041f, .0015f, .002f), a.Dark);
                }
                foreach (float x in new[] { -.085f, .085f })
                {
                    a.Box(new(x, .052f, .1f), new(.018f, .006f, .229f), a.Leather);
                    a.Box(new(x, -.035f, .1f), new(.018f, .006f, .229f), a.Leather);
                    foreach (float z in new[] { -.012f, .212f }) a.Box(new(x, .01f, z), new(.018f, .085f, .006f), a.Leather);
                    a.Box(new(x, .058f, .137f), new(.028f, .008f, .03f), a.Steel);
                }
            }
        }

        public static void SupportJack(ToolArtBuilder a)
        {
            a.Box(new(0, -.085f, .105f), new(.15f, .014f, .14f), a.Steel);
            a.Cylinder(new(0, .011f, .105f), new(.046f, .185f, .046f), a.Paint);
            a.Cylinder(new(0, .135f, .105f), new(.029f, .16f, .029f), a.Steel);
            a.Ring(new(0, .084f, .105f), .068f, .035f, a.Dark);
            a.Box(new(0, .222f, .105f), new(.142f, .014f, .12f), a.Steel);
            a.Rod(new(-.066f, .09f, .105f), new(.066f, .09f, .105f), .012f, a.Steel);
            foreach (float x in new[] { -.067f, .067f }) a.Cylinder(new(x, .09f, .105f), new(.018f, .026f, .018f), a.Rubber);
            a.Rod(new(-.035f, -.03f, .105f), new(.035f, -.03f, .105f), .009f, a.Steel);
            for (int i = 0; i < 8; i++) a.Ring(new(0, .113f + i * .012f, .105f), .032f, .003f, a.Dark);
            a.Screws(new(0, 0, .105f), .112f, .1f, -.076f);
        }

        public static void RopePulley(ToolArtBuilder a)
        {
            Vector3 previous = new(.034f, -.015f, .092f);
            for (int i = 1; i <= 96; i++)
            {
                float t = i / 96f, angle = t * Mathf.PI * 6f, radius = Mathf.Lerp(.034f, .108f, t);
                Vector3 next = new(Mathf.Cos(angle) * radius, -.015f, .092f + Mathf.Sin(angle) * radius);
                a.Rod(previous, next, .012f, a.Canvas); previous = next;
            }
            a.Cable(previous, new(.074f, .019f, .21f), new(.025f, -.015f, .016f), .012f, a.Canvas);
            foreach (float y in new[] { .013f, .048f }) a.Box(new(0, y, .218f), new(.083f, .008f, .105f), a.Steel);
            a.Cylinder(new(0, .032f, .218f), new(.07f, .027f, .07f), a.Dark);
            a.Ring(new(0, .032f, .218f), .076f, .014f, a.Paint);
            a.Cylinder(new(0, .056f, .218f), new(.015f, .008f, .015f), a.Steel);
            a.Ring(new(0, .035f, .293f), .051f, .013f, a.Steel);
            a.Box(new(.026f, .035f, .3f), new(.011f, .015f, .035f), a.Dark, new(0, -22, 0));
            foreach (float x in new[] { -.074f, .074f }) a.Box(new(x, -.015f, .09f), new(.023f, .031f, .155f), a.Dark);
            a.Box(new(0, -.005f, .08f), new(.07f, .012f, .028f), a.Leather);
        }

        public static void Trolley(ToolArtBuilder a, bool flatbed)
        {
            float half = flatbed ? .137f : .107f, height = flatbed ? .225f : .325f;
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
            a.Box(new(0, -.009f, flatbed ? .23f : .175f), new(half * 2f + .01f, .019f, flatbed ? .3f : .15f), a.Paint);
            for (int i = 0; i < 5; i++) a.Box(new((i - 2) * half * .35f, .003f, flatbed ? .23f : .175f), new(.008f, .003f, flatbed ? .27f : .12f), a.Rubber);
            if (flatbed)
            {
                foreach (float x in new[] { -half, half })
                {
                    a.Box(new(x, -.036f, .339f), new(.027f, .055f, .036f), a.Steel);
                    a.Cylinder(new(x, -.061f, .342f), new(.055f, .027f, .055f), a.Rubber, new(0, 0, 90));
                }
                a.Box(new(0, -.023f, .24f), new(.22f, .009f, .26f), a.Steel);
            }
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
