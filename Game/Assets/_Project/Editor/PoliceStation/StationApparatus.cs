using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using M = Abandoned.EditorTools.StationMeshes;
using R = Abandoned.EditorTools.StationMeshes.Ring;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The big vehicles, original and built in code: the fire company's pumpers and its aerial ladder truck (a custom cab,
    /// roll-up compartments, the pump panel's gauges and outlets, the hose bed, racked ground ladders, dual rear wheels,
    /// light bars and a lattice aerial on its turntable), a box van and a tow truck. Each draws into the root it is given
    /// (+z forward, ground at y 0) and adds nothing solid; the caller owns the collider (a fitting in a room, or the lot).
    /// </summary>
    public static class StationApparatus
    {
        public const float Half = 1.24f;

        /// <summary>A pumper 9.6 m long, its nose at z +4.8.</summary>
        public static void Pumper(Transform t, StationArt a, Material paint, int seed)
        {
            StationVehicleArt v = StationVehicleArt.For(a);
            Cab(t, a, v, paint, 2f, 4.8f);
            Body(t, a, v, paint, -4.75f, 1.95f, 2.6f, 4, seed, -3.125f);
            PumpPanel(t, v, 1.45f);
            HoseBed(t, a, v, -4.6f, 1.6f, 2.6f);
            GroundLadder(t, v, 1, 2.72f, -4.2f, 1.6f);
            Wheels(t, a, v, 3.25f, new[] { -2.5f, -3.75f });
            Tailboard(t, v, -4.75f);
        }

        /// <summary>The aerial ladder truck, 12.4 m: the ladder nested along its top from the turntable at the back.</summary>
        public static void LadderTruck(Transform t, StationArt a, Material paint, int seed)
        {
            StationVehicleArt v = StationVehicleArt.For(a);
            Cab(t, a, v, paint, 3.4f, 6.2f);
            Body(t, a, v, paint, -6.1f, 3.35f, 2.3f, 5, seed, -3.825f);
            Wheels(t, a, v, 4.65f, new[] { -3.2f, -4.45f });
            Tailboard(t, v, -6.1f);
            Transform turn = Group("Turntable", t);
            turn.localPosition = new Vector3(0f, 2.3f, -4.6f);
            M.Draw(turn, "Ring", M.Lathe(a, "Turntable", new[] { new Vector2(1.05f, 0f), new Vector2(1.05f, .22f), new Vector2(1.05f, .22f), new Vector2(0f, .26f) }),
                Vector3.zero, Vector3.zero, Vector3.one, v.Trim);
            Box("Pedestal", turn, new Vector3(0f, .7f, 0f), new Vector3(1.1f, .9f, 1.3f), paint, false);
            Box("Console", turn, new Vector3(.7f, .55f, .5f), new Vector3(.3f, .5f, .4f), v.Chrome, false);
            // Three nested sections, each a pair of trussed rails, resting in the cradle over the cab.
            for (int sct = 0; sct < 3; sct++)
            {
                float w = 1.16f - sct * .16f, y = 3.32f + sct * .13f, len = 10.2f - sct * .5f, z0 = -4.6f + sct * .2f;
                LadderSection(t, v, w, y, z0, z0 + len, sct == 0 ? v.Chrome : v.SteelWheel);
            }
            Box("Cradle", t, new Vector3(0f, 3.15f, 4.3f), new Vector3(1.3f, .12f, .3f), v.Trim, false);
            foreach (float s in new[] { -1f, 1f }) Box("CradlePost", t, new Vector3(s * .6f, 3.05f, 4.3f), new Vector3(.08f, .3f, .08f), v.Trim, false);
            Box("Tip", t, new Vector3(0f, 3.5f, 6f), new Vector3(1f, .5f, .55f), v.Chrome, false);
            foreach (float s in new[] { -1f, 1f })
            foreach (float z in new[] { -3.85f, 2.8f })
            {
                Box("Outrigger", t, new Vector3(s * (Half + .12f), .72f, z), new Vector3(.24f, .4f, .5f), a.Safety, false);
                Box("Jack", t, new Vector3(s * (Half + .12f), .38f, z), new Vector3(.14f, .3f, .14f), v.Chrome, false);
            }
            GroundLadder(t, v, 1, 2.42f, -5.6f, 2.4f);
            GroundLadder(t, v, -1, 2.42f, -5.6f, 2.4f);
        }

        // ---- The fire-apparatus parts -------------------------------------------------------------------------

        // A custom cab from z0 to z1: a squared loft with a white roof, a big split windscreen, the light bar and the bumper.
        private static void Cab(Transform t, StationArt a, StationVehicleArt v, Material paint, float z0, float z1)
        {
            string key = $"FireCab_{Mathf.RoundToInt((z1 - z0) * 100)}";
            M.Draw(t, "Cab", M.Loft(a, key, new[]
            {
                new R(z0, .78f, 2.96f, Half, Half - .04f, 10f), new R(z1 - .35f, .78f, 2.96f, Half, Half - .04f, 10f),
                new R(z1 - .1f, .8f, 2.82f, Half - .01f, Half - .06f, 8f), new R(z1, .86f, 2.36f, Half - .05f, Half - .12f, 6f),
            }), Vector3.zero, Vector3.zero, Vector3.one, paint);
            Box("RoofCap", t, new Vector3(0f, 2.975f, (z0 + z1) * .5f - .15f), new Vector3(Half * 2f - .12f, .04f, z1 - z0 - .5f), v.Livery, false);
            GameObject screen = Box("Windscreen", t, new Vector3(0f, 2.3f, z1 - .06f), new Vector3(Half * 2f - .3f, .9f, .05f), v.Glass, false);
            screen.transform.localRotation = Quaternion.Euler(-9f, 0f, 0f);
            Box("ScreenSplit", t, new Vector3(0f, 2.3f, z1 - .02f), new Vector3(.06f, .92f, .04f), v.Trim, false);
            foreach (float s in new[] { -1f, 1f })
            {
                float x = s * (Half + .006f);
                Box("CabWindowF", t, new Vector3(x, 2.25f, z1 - .75f), new Vector3(.02f, .85f, .95f), v.Glass, false);
                Box("CabWindowR", t, new Vector3(x, 2.3f, z0 + .7f), new Vector3(.02f, .75f, .9f), v.Glass, false);
                Box("DoorLine", t, new Vector3(x, 1.7f, z1 - 1.3f), new Vector3(.008f, 1.8f, .01f), v.Well, false);
                Box("DoorLine", t, new Vector3(x, 1.7f, z0 + 1.25f), new Vector3(.008f, 1.8f, .01f), v.Well, false);
                Box("Stripe", t, new Vector3(x, 1.3f, (z0 + z1) * .5f), new Vector3(.01f, .1f, z1 - z0 - .2f), v.Livery, false);
                Box("Step", t, new Vector3(s * (Half - .25f), .55f, z1 - 1.3f), new Vector3(.5f, .05f, .5f), v.Chrome, false);
                Box("Step", t, new Vector3(s * (Half - .25f), .55f, z0 + .9f), new Vector3(.5f, .05f, .5f), v.Chrome, false);
                Box("Handrail", t, new Vector3(x + s * .03f, 1.8f, z1 - 1.45f), new Vector3(.04f, .9f, .04f), v.Chrome, false);
                // West-coast mirrors on tall arms.
                Box("MirrorArm", t, new Vector3(s * (Half + .2f), 2.15f, z1 - .35f), new Vector3(.4f, .04f, .04f), v.Chrome, false);
                Box("Mirror", t, new Vector3(s * (Half + .4f), 2.15f, z1 - .35f), new Vector3(.08f, .55f, .26f), v.Trim, false);
                Box("HeadLamp", t, new Vector3(s * .82f, 1.25f, z1 + .02f), new Vector3(.34f, .22f, .06f), v.HeadLens, false);
                Box("Warning", t, new Vector3(s * .82f, 1.6f, z1 + .01f), new Vector3(.3f, .14f, .06f), v.TailLens, false);
                Box("Turn", t, new Vector3(s * 1.08f, 1.25f, z1 - .02f), new Vector3(.12f, .12f, .06f), v.Amber, false);
            }
            Box("Grille", t, new Vector3(0f, 1.3f, z1 + .01f), new Vector3(1.1f, .7f, .05f), v.Chrome, false);
            for (int i = 0; i < 7; i++) Box("GrilleSlat", t, new Vector3(0f, 1.02f + i * .09f, z1 + .04f), new Vector3(1.05f, .03f, .02f), v.Trim, false);
            Box("Bumper", t, new Vector3(0f, .62f, z1 + .2f), new Vector3(Half * 2f + .05f, .28f, .3f), v.Chrome, false);
            Box("BumperWell", t, new Vector3(0f, .78f, z1 + .2f), new Vector3(1.4f, .02f, .26f), v.Trim, false);
            Transform siren = Group("Siren", t);
            siren.localPosition = new Vector3(.62f, .92f, z1 + .05f);
            M.Draw(siren, "Body", M.Lathe(a, "QSiren", new[]
            {
                new Vector2(0f, 0f), new Vector2(.15f, 0f), new Vector2(.15f, 0f), new Vector2(.17f, .2f), new Vector2(.15f, .38f), new Vector2(.15f, .38f), new Vector2(0f, .42f),
            }), Vector3.zero, new Vector3(90f, 0f, 0f), Vector3.one, v.Chrome);
            Box("LightBar", t, new Vector3(0f, 3.06f, z1 - .55f), new Vector3(2f, .12f, .32f), v.Trim, false);
            for (int i = 0; i < 6; i++)
                Box("Lens", t, new Vector3(-.85f + i * .34f, 3.15f, z1 - .55f), new Vector3(.3f, .1f, .28f), i % 3 == 1 ? v.HeadLens : v.TailLens, false);
            Box("Plate", t, new Vector3(0f, .62f, z1 + .355f), new Vector3(.52f, .11f, .01f), v.Plate, false);
        }

        // The body behind the cab: compartments with roll-up doors down each side between red posts.
        private static void Body(Transform t, StationArt a, StationVehicleArt v, Material paint, float z0, float z1, float top, int bays, int seed, float wheels)
        {
            float len = z1 - z0, mid = (z0 + z1) * .5f;
            Box("Body", t, new Vector3(0f, (.78f + top) * .5f, mid), new Vector3(Half * 2f - .02f, top - .78f, len), paint, false);
            Box("BodyTop", t, new Vector3(0f, top + .02f, mid), new Vector3(Half * 2f, .04f, len), v.Chrome, false);
            Box("Undercarriage", t, new Vector3(0f, .55f, mid + 1f), new Vector3(1.3f, .4f, len + 3f), v.Under, false);
            float bay = (len - .9f) / bays;
            foreach (float s in new[] { -1f, 1f })
            {
                float x = s * (Half + .005f);
                Box("Rub", t, new Vector3(x, .9f, mid), new Vector3(.03f, .1f, len), v.Chrome, false);
                Box("Stripe", t, new Vector3(x, 2.35f, mid), new Vector3(.01f, .12f, len), v.Livery, false);
                for (int i = 0; i < bays; i++)
                {
                    float zc = z0 + .45f + bay * (i + .5f);
                    if (Mathf.Abs(zc - wheels) < bay * .5f) continue;   // over the rear wheels: plain paint
                    Box("Shutter", t, new Vector3(x, 1.6f, zc), new Vector3(.02f, 1.25f, bay - .2f), v.SteelWheel, false);
                    for (int r = 0; r < 12; r++)
                        Box("Slat", t, new Vector3(x + s * .006f, 1.03f + r * .1f, zc), new Vector3(.01f, .015f, bay - .22f), v.Chrome, false);
                    Box("ShutterBar", t, new Vector3(x + s * .012f, 1.02f, zc), new Vector3(.02f, .04f, bay - .3f), v.Chrome, false);
                    if ((i + seed) % 5 == 2)
                    {
                        // One compartment left open: shelves and gear inside the dark.
                        Box("OpenBay", t, new Vector3(x + s * .008f, 1.6f, zc), new Vector3(.012f, 1.2f, bay - .24f), v.Well, false);
                        Box("Gear", t, new Vector3(x - s * .02f, 1.25f, zc), new Vector3(.02f, .35f, bay * .5f), a.Safety, false);
                    }
                }
            }
            for (int i = 0; i < 3; i++)
                Box("RearLamp", t, new Vector3(-Half + .25f + i * .2f, 1.4f, z0 - .01f), new Vector3(.16f, .16f, .04f), i == 1 ? v.Amber : v.TailLens, false);
            for (int i = 0; i < 3; i++)
                Box("RearLamp", t, new Vector3(Half - .25f - i * .2f, 1.4f, z0 - .01f), new Vector3(.16f, .16f, .04f), i == 1 ? v.Amber : v.TailLens, false);
            foreach (float s in new[] { -1f, 1f }) Box("Beacon", t, new Vector3(s * (Half - .15f), top + .12f, z0 + .15f), new Vector3(.22f, .2f, .22f), v.TailLens, false);
        }

        private static void PumpPanel(Transform t, StationVehicleArt v, float z)
        {
            foreach (float s in new[] { -1f, 1f })
            {
                float x = s * (Half + .01f);
                Box("Panel", t, new Vector3(x, 1.75f, z), new Vector3(.02f, 1.4f, .9f), v.Chrome, false);
                for (int g = 0; g < 4; g++)
                {
                    GameObject gauge = Primitive(PrimitiveType.Cylinder, "Gauge", t, new Vector3(x + s * .015f, 2.2f, z - .3f + g * .2f), new Vector3(.14f, .01f, .14f), v.HeadLens, false);
                    gauge.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                    GameObject bezel = Primitive(PrimitiveType.Cylinder, "Bezel", t, new Vector3(x + s * .01f, 2.2f, z - .3f + g * .2f), new Vector3(.17f, .008f, .17f), v.Trim, false);
                    bezel.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                }
                for (int o = 0; o < 2; o++)
                {
                    float zo = z - .22f + o * .44f;
                    GameObject outlet = Primitive(PrimitiveType.Cylinder, "Outlet", t, new Vector3(x + s * .09f, 1.2f, zo), new Vector3(.16f, .09f, .16f), v.Chrome, false);
                    outlet.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                    GameObject cap = Primitive(PrimitiveType.Cylinder, "Cap", t, new Vector3(x + s * .19f, 1.2f, zo), new Vector3(.2f, .02f, .2f), v.Chrome, false);
                    cap.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                    Box("Valve", t, new Vector3(x + s * .06f, 1.6f, zo), new Vector3(.06f, .04f, .22f), v.TailLens, false);
                }
                Box("Throttle", t, new Vector3(x + s * .04f, 1.85f, z + .32f), new Vector3(.05f, .1f, .1f), v.Trim, false);
            }
        }

        // Folded hose in the bed on top of the body, two colours, and the bed's dividers.
        private static void HoseBed(Transform t, StationArt a, StationVehicleArt v, float z0, float z1, float top)
        {
            Material yellow = a.Plain("HoseYellow", new Color(.8f, .62f, .16f), .15f);
            Material white = a.Plain("HoseCanvas", new Color(.62f, .56f, .4f), .1f);
            float len = z1 - z0;
            for (int lane = 0; lane < 3; lane++)
            {
                float x = -.75f + lane * .75f;
                for (int layer = 0; layer < 5; layer++)
                    Box("Fold", t, new Vector3(x, top + .06f + layer * .07f, (z0 + z1) * .5f), new Vector3(.66f, .065f, len - .2f), (lane + layer) % 2 == 0 ? yellow : white, false);
                if (lane < 2) Box("Divider", t, new Vector3(x + .375f, top + .2f, (z0 + z1) * .5f), new Vector3(.02f, .4f, len), v.Chrome, false);
            }
            Box("HoseLoop", t, new Vector3(-.3f, top + .25f, z0 - .1f), new Vector3(.6f, .3f, .12f), yellow, false);
        }

        // A ground ladder racked along one side of the body's top.
        private static void GroundLadder(Transform t, StationVehicleArt v, int side, float y, float z0, float z1Offset)
        {
            float x = side * (Half - .2f), z1 = z0 + (z1Offset > 2f ? 8.2f : 6.2f);
            foreach (float dx in new[] { -.18f, .18f }) Box("LadderRail", t, new Vector3(x + dx, y + .34f, (z0 + z1) * .5f), new Vector3(.05f, .1f, z1 - z0), v.SteelWheel, false);
            for (float z = z0 + .15f; z < z1; z += .32f) Box("LadderRung", t, new Vector3(x, y + .34f, z), new Vector3(.36f, .03f, .03f), v.SteelWheel, false);
            foreach (float z in new[] { z0 + .4f, z1 - .4f }) Box("LadderBracket", t, new Vector3(x, y + .14f, z), new Vector3(.5f, .3f, .06f), v.Trim, false);
        }

        private static void LadderSection(Transform t, StationVehicleArt v, float width, float y, float z0, float z1, Material metal)
        {
            float mid = (z0 + z1) * .5f, len = z1 - z0;
            foreach (float s in new[] { -1f, 1f })
            {
                float x = s * width * .5f;
                Box("Chord", t, new Vector3(x, y, mid), new Vector3(.06f, .06f, len), metal, false);
                Box("Handrail", t, new Vector3(x, y + .42f, mid), new Vector3(.05f, .05f, len - .3f), metal, false);
                int n = Mathf.FloorToInt(len / .6f);
                for (int i = 0; i <= n; i++)
                {
                    float z = z0 + .15f + i * (len - .3f) / n;
                    Box("Post", t, new Vector3(x, y + .21f, z), new Vector3(.03f, .42f, .03f), metal, false);
                    if (i < n) M.Strut(t, "Brace", new Vector3(x, y, z), new Vector3(x, y + .42f, z + (len - .3f) / n), .025f, metal);
                }
            }
            for (float z = z0 + .2f; z < z1 - .1f; z += .36f) Box("Rung", t, new Vector3(0f, y + .02f, z), new Vector3(width, .035f, .035f), metal, false);
        }

        // Single front wheels and dual rears on steel rims, with black fenders over them.
        private static void Wheels(Transform t, StationArt a, StationVehicleArt v, float front, float[] rears)
        {
            const float r = .52f;
            Mesh tyre = M.Tyre(a, r, .3f), rim = M.Rim(a, r * .64f, .26f);
            foreach (float s in new[] { -1f, 1f })
            {
                Vector3 turn = new(0f, 0f, s > 0f ? -90f : 90f);
                M.Draw(t, "Tyre", tyre, new Vector3(s * (Half - .2f), r, front), turn, Vector3.one, v.Tyre);
                M.Draw(t, "Rim", rim, new Vector3(s * (Half - .2f), r, front), turn, Vector3.one, v.Chrome);
                Box("Fender", t, new Vector3(s * (Half - .15f), 2f * r + .08f, front), new Vector3(.4f, .05f, 1.3f), v.Trim, false);
                foreach (float z in rears)
                {
                    // Duals: the outer tyre's face just proud of the body, the inner one tucked behind it.
                    foreach (float dx in new[] { -.46f, -.14f })
                        M.Draw(t, "Tyre", tyre, new Vector3(s * (Half + dx), r, z), turn, Vector3.one, v.Tyre);
                    M.Draw(t, "Rim", rim, new Vector3(s * (Half - .14f), r, z), turn, Vector3.one, v.SteelWheel);
                }
                Box("Fender", t, new Vector3(s * (Half - .1f), 2f * r + .08f, (rears[0] + rears[rears.Length - 1]) * .5f),
                    new Vector3(.5f, .05f, Mathf.Abs(rears[0] - rears[rears.Length - 1]) + 1.3f), v.Trim, false);
            }
            foreach (float z in new[] { front, (rears[0] + rears[rears.Length - 1]) * .5f })
                Box("WellLiner", t, new Vector3(0f, .75f, z), new Vector3(Half * 2f - .9f, .9f, z == front ? 1.2f : Mathf.Abs(rears[0] - rears[rears.Length - 1]) + 1.2f), v.Well, false);
        }

        private static void Tailboard(Transform t, StationVehicleArt v, float z0)
        {
            Box("Tailboard", t, new Vector3(0f, .62f, z0 - .25f), new Vector3(Half * 2f, .06f, .5f), v.Chrome, false);
            foreach (float s in new[] { -1f, 1f }) Box("Grab", t, new Vector3(s * (Half - .1f), 1.7f, z0 - .04f), new Vector3(.04f, 1f, .04f), v.Chrome, false);
        }

        // ---- Road vehicles ------------------------------------------------------------------------------------

        /// <summary>A box van (an evidence van, a parcel van), 6 m long: a rounded cab and a tall box, its nose at z +3.</summary>
        public static void BoxVan(Transform t, StationArt a, Material paint, Material stripe, int seed)
        {
            StationVehicleArt v = StationVehicleArt.For(a);
            const float h = 1.05f;
            M.Draw(t, "Cab", M.Loft(a, "VanCab", new[]
            {
                new R(1.3f, .42f, 2.3f, h, h - .04f, 8f), new R(2f, .42f, 2.24f, h, h - .06f, 7f), new R(2.55f, .42f, 1.55f, h - .02f, h - .1f, 5f),
                new R(2.95f, .45f, 1.15f, h - .06f, h - .14f, 4f), new R(3.05f, .55f, 1.0f, h - .12f, h - .2f, 3f),
            }), Vector3.zero, Vector3.zero, Vector3.one, paint);
            M.Draw(t, "Box", M.Loft(a, "VanBox", new[]
            {
                new R(-3f, .5f, 2.75f, 1.12f, 1.1f, 10f), new R(1.32f, .5f, 2.75f, 1.12f, 1.1f, 10f),
            }), Vector3.zero, Vector3.zero, Vector3.one, paint);
            GameObject screen = Box("Windscreen", t, new Vector3(0f, 1.82f, 2.35f), new Vector3(1.85f, .65f, .04f), v.Glass, false);
            screen.transform.localRotation = Quaternion.Euler(-38f, 0f, 0f);
            foreach (float s in new[] { -1f, 1f })
            {
                Box("SideWindow", t, new Vector3(s * (h - .02f), 1.82f, 1.75f), new Vector3(.02f, .55f, .7f), v.Glass, false);
                Box("Stripe", t, new Vector3(s * 1.125f, 1.55f, -.8f), new Vector3(.01f, .3f, 4.3f), stripe, false);
                Box("HeadLamp", t, new Vector3(s * .66f, .95f, 3.04f), new Vector3(.3f, .16f, .05f), v.HeadLens, false);
                Box("TailLamp", t, new Vector3(s * .95f, 1f, -3.01f), new Vector3(.14f, .5f, .04f), v.TailLens, false);
                Box("Mirror", t, new Vector3(s * (h + .15f), 1.75f, 2.1f), new Vector3(.06f, .3f, .18f), v.Trim, false);
            }
            Box("Grille", t, new Vector3(0f, .85f, 3.05f), new Vector3(1.1f, .24f, .04f), v.Trim, false);
            Box("BumperF", t, new Vector3(0f, .52f, 3.1f), new Vector3(2.05f, .2f, .16f), v.Trim, false);
            Box("BumperR", t, new Vector3(0f, .58f, -3.06f), new Vector3(2.2f, .16f, .14f), v.Trim, false);
            Box("RearSplit", t, new Vector3(0f, 1.6f, -3.005f), new Vector3(.02f, 2.1f, .01f), v.Well, false);
            foreach (float s in new[] { -1f, 1f }) Box("RearHandle", t, new Vector3(s * .12f, 1.5f, -3.015f), new Vector3(.04f, .3f, .02f), v.Chrome, false);
            Box("Under", t, new Vector3(0f, .35f, 0f), new Vector3(1.7f, .2f, 5.6f), v.Under, false);
            SimpleWheels(t, a, v, .4f, 1f, new[] { 2.05f, -1.9f });
        }

        /// <summary>A tow truck, 7.4 m: a pickup-type cab and a wrecker boom with its winch and hook, its nose at z +3.7.</summary>
        public static void TowTruck(Transform t, StationArt a, Material paint)
        {
            StationVehicleArt v = StationVehicleArt.For(a);
            const float h = 1.15f;
            M.Draw(t, "Cab", M.Loft(a, "TowCab", new[]
            {
                new R(.6f, .55f, 2.35f, h, h - .08f, 8f), new R(1.75f, .55f, 2.35f, h, h - .1f, 7f), new R(2.1f, .55f, 1.45f, h, h - .08f, 7f),
                new R(3.45f, .55f, 1.4f, h - .02f, h - .1f, 7f), new R(3.75f, .62f, 1.2f, h - .1f, h - .2f, 4f),
            }), Vector3.zero, Vector3.zero, Vector3.one, paint);
            GameObject screen = Box("Windscreen", t, new Vector3(0f, 1.95f, 1.9f), new Vector3(2f, .7f, .04f), v.Glass, false);
            screen.transform.localRotation = Quaternion.Euler(-22f, 0f, 0f);
            Box("Bed", t, new Vector3(0f, .95f, -1.4f), new Vector3(2.3f, .5f, 4f), v.SteelWheel, false);
            Box("BedDeck", t, new Vector3(0f, 1.22f, -1.4f), new Vector3(2.32f, .04f, 4f), v.Chrome, false);
            Transform boom = Group("Boom", t);
            boom.localPosition = new Vector3(0f, 1.3f, -.9f);
            boom.localRotation = Quaternion.Euler(24f, 0f, 0f);
            Box("Arm", boom, new Vector3(0f, 0f, -1.6f), new Vector3(.3f, .32f, 3.4f), paint, false);
            Box("Inner", boom, new Vector3(0f, .02f, -3.4f), new Vector3(.22f, .24f, .9f), v.SteelWheel, false);
            Box("Sheave", boom, new Vector3(0f, 0f, -3.9f), new Vector3(.12f, .36f, .36f), v.Trim, false);
            M.Strut(t, "Cable", new Vector3(0f, 2.75f, -4.45f), new Vector3(0f, 1.5f, -4.45f), .02f, v.Trim);
            Box("Hook", t, new Vector3(0f, 1.42f, -4.45f), new Vector3(.06f, .16f, .12f), v.Chrome, false);
            Box("Winch", t, new Vector3(0f, 1.4f, -.5f), new Vector3(.8f, .35f, .4f), v.Trim, false);
            Box("LightBar", t, new Vector3(0f, 2.42f, 1.25f), new Vector3(1.5f, .1f, .28f), v.Trim, false);
            for (int i = 0; i < 4; i++) Box("Lens", t, new Vector3(-.54f + i * .36f, 2.5f, 1.25f), new Vector3(.32f, .08f, .25f), v.Amber, false);
            foreach (float s in new[] { -1f, 1f })
            {
                Box("SideWindow", t, new Vector3(s * (h - .06f), 1.95f, 1.15f), new Vector3(.02f, .55f, .9f), v.Glass, false);
                Box("HeadLamp", t, new Vector3(s * .72f, 1.1f, 3.73f), new Vector3(.3f, .18f, .05f), v.HeadLens, false);
                Box("Mirror", t, new Vector3(s * (h + .18f), 1.9f, 1.7f), new Vector3(.06f, .34f, .2f), v.Chrome, false);
                Box("Outrigger", t, new Vector3(s * 1.05f, .55f, -3.2f), new Vector3(.2f, .3f, .5f), a.Safety, false);
            }
            Box("Grille", t, new Vector3(0f, 1.05f, 3.76f), new Vector3(1.05f, .45f, .04f), v.Chrome, false);
            Box("BumperF", t, new Vector3(0f, .66f, 3.8f), new Vector3(2.25f, .22f, .2f), v.Chrome, false);
            Box("Under", t, new Vector3(0f, .45f, 0f), new Vector3(1.6f, .25f, 6.6f), v.Under, false);
            SimpleWheels(t, a, v, .48f, h - .1f, new[] { 2.6f, -1.5f, -2.7f });
        }

        private static void SimpleWheels(Transform t, StationArt a, StationVehicleArt v, float r, float x, float[] axles)
        {
            Mesh tyre = M.Tyre(a, r, .26f), rim = M.Rim(a, r * .64f, .22f);
            foreach (float z in axles)
            {
                foreach (float s in new[] { -1f, 1f })
                {
                    Vector3 turn = new(0f, 0f, s > 0f ? -90f : 90f);
                    M.Draw(t, "Tyre", tyre, new Vector3(s * x, r, z), turn, Vector3.one, v.Tyre);
                    M.Draw(t, "Rim", rim, new Vector3(s * (x + .02f), r, z), turn, Vector3.one, v.SteelWheel);
                }
                Box("WellLiner", t, new Vector3(0f, r, z), new Vector3(x * 2f - .3f, r * 1.4f, r * 2.2f), v.Well, false);
            }
        }
    }
}
