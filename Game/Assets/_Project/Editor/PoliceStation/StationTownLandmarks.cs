using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using F = Abandoned.EditorTools.StationTownFacade;
using M = Abandoned.EditorTools.StationMeshes;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The neighbourhood's bigger and odder buildings (see StationTownBuildings.cs): a clapboard house under a gable roof
    /// with its porch, a warehouse with its loading dock, an auto garage, a brick church with its spire, a gas station
    /// with its canopy, pumps and kiosk.
    /// </summary>
    public static partial class StationTownBuildings
    {
        private static void House(Transform b, StationArt a, StationTownArt t, float w, float d, int seed)
        {
            const float wall = 5.8f, rise = 2.6f;
            Material siding = t.Siding(seed);
            Mass(b, w, d, wall, siding);
            Box("Foundation", b, new Vector3(0f, .25f, -d * .5f), new Vector3(w + .08f, .5f, d + .08f), t.OldConcrete, false);
            GableRoof(b, a, t, siding, w, d, wall, rise, alongX: true);
            Transform front = F.Face(b, F.Side.Front, w, d);
            Material trim = t.Frame, shutters = t.Paint(seed + 1);
            // The porch across the front: deck, posts, rails and its own shallow roof.
            Box("PorchDeck", b, new Vector3(0f, .22f, 1.1f), new Vector3(w, .45f, 2.2f), a.Board);
            for (int i = 0; i < 2; i++) Box("PorchStep", b, new Vector3(0f, .075f + i * .15f, 2.4f - i * .3f), new Vector3(1.6f, .15f + i * .15f, .3f), a.Board);
            foreach (float x in new[] { -w * .5f + .2f, -1.1f, 1.1f, w * .5f - .2f })
                Primitive(PrimitiveType.Cylinder, "PorchPost", b, new Vector3(x, 1.65f, 2.05f), new Vector3(.18f, 1.2f, .18f), trim, false);
            foreach (float s in new[] { -1f, 1f })
            {
                float from = s * 1.1f, to = s * (w * .5f - .2f);
                Box("PorchRail", b, new Vector3((from + to) * .5f, 1.35f, 2.05f), new Vector3(Mathf.Abs(to - from), .06f, .06f), trim, false);
                for (float x = Mathf.Min(from, to) + .15f; x < Mathf.Max(from, to); x += .15f)
                    Box("Spindle", b, new Vector3(x, .9f, 2.05f), new Vector3(.03f, .9f, .03f), trim, false);
            }
            GameObject porchRoof = Box("PorchRoof", b, new Vector3(0f, 2.95f, 1.15f), new Vector3(w + .2f, .12f, 2.5f), t.Shingles, false);
            porchRoof.transform.localRotation = Quaternion.Euler(10f, 0f, 0f);
            F.Door(front, a, t, 0f, 1f, 2.3f, t.Paint(seed + 2));
            foreach (float s in new[] { -1f, 1f }) Box("Sidelight", front, new Vector3(s * .75f, 1.3f, .06f), new Vector3(.3f, 1.9f, .02f), t.Window, false);
            foreach (float x in new[] { -w * .3f, w * .3f })
            {
                HouseWindow(front, a, t, x, 1f, 1.6f, shutters, seed);
                HouseWindow(front, a, t, x, 3.6f, 1.4f, shutters, seed + 2);
            }
            HouseWindow(front, a, t, 0f, 3.6f, 1.2f, shutters, seed + 4);
            foreach (F.Side side in new[] { F.Side.Left, F.Side.Right, F.Side.Back })
            {
                Transform f = F.Face(b, side, w, d);
                float span = side == F.Side.Back ? w : d;
                foreach (float x in new[] { -span * .25f, span * .25f })
                {
                    F.Window(f, a, t, x, 1f, 1f, 1.5f, F.PaneFor(seed + (int)side + Mathf.RoundToInt(x)), trim, trim, true, false);
                    F.Window(f, a, t, x, 3.6f, 1f, 1.3f, F.PaneFor(seed + (int)side * 3 + Mathf.RoundToInt(x)), trim, trim, true, false);
                }
            }
            Box("Chimney", b, new Vector3(-w * .5f - .3f, (wall + rise + 1f) * .5f, -d * .5f), new Vector3(.9f, wall + rise + 1f, 1.1f), t.Brick(seed));
            Box("ChimneyCap", b, new Vector3(-w * .5f - .3f, wall + rise + 1.05f, -d * .5f), new Vector3(1.05f, .1f, 1.25f), a.Trim, false);
            F.Gutters(front, t, w, wall);
            Box("Meter", b, new Vector3(w * .5f + .08f, 1.4f, -d * .7f), new Vector3(.14f, .4f, .3f), a.DarkSteel, false);
        }

        // A house window: frame, glass, sill and a pair of louvred shutters.
        private static void HouseWindow(Transform face, StationArt a, StationTownArt t, float x, float sill, float h, Material shutters, int seed)
        {
            F.Window(face, a, t, x, sill, .95f, h, F.PaneFor(seed + Mathf.RoundToInt(x * 3f + sill)), t.Frame, t.Frame, true, false);
            foreach (float s in new[] { -1f, 1f })
            {
                bool hanging = (seed + Mathf.RoundToInt(x)) % 7 == 3 && s > 0f;
                GameObject sh = Box("Shutter", face, new Vector3(x + s * .78f, sill + h * .5f - (hanging ? .1f : 0f), .05f), new Vector3(.45f, h, .04f), shutters, false);
                if (hanging) sh.transform.localRotation = Quaternion.Euler(0f, 0f, -9f);
                for (int i = 0; i < 6; i++)
                    Box("Louvre", face, new Vector3(x + s * .78f, sill + .15f + i * h / 6.5f, .075f), new Vector3(.38f, .025f, .01f), t.FrameDark, false);
            }
        }

        // A gable roof on a wall top: the attic as a prism in the wall material and two shingled planes overhanging it.
        // alongX: the ridge runs along the building's width (gables at its sides); otherwise front to back.
        private static void GableRoof(Transform b, StationArt a, StationTownArt t, Material gable, float w, float d, float wall, float rise, bool alongX,
            Material roof = null, float zFrom = 0f)
        {
            float span = alongX ? d : w, length = alongX ? w : d, cz = zFrom - d * .5f;
            Mesh prism = M.Prism(a);
            M.Draw(b, "Attic", prism, new Vector3(0f, wall, cz), alongX ? new Vector3(0f, 90f, 0f) : Vector3.zero,
                new Vector3(span, rise, length), gable);
            float half = span * .5f, slope = Mathf.Sqrt(half * half + rise * rise);
            Material cover = roof != null ? roof : t.Shingles;
            foreach (float s in new[] { -1f, 1f })
            {
                // In the roof's own frame: u across the span (s = which side), y up; the plane's middle and its fall line.
                Vector3 across = alongX ? Vector3.forward : Vector3.right;
                Vector3 mid = new Vector3(0f, wall + rise * .5f, cz) + across * (s * half * .5f);
                Vector3 fall = (across * (s * half) - Vector3.up * rise).normalized;
                Vector3 normal = (across * (s * rise) + Vector3.up * half).normalized;
                GameObject plane = Box("RoofPlane", b, mid + normal * .07f, new Vector3(length + .5f, .12f, slope + .5f), cover, false);
                plane.transform.localRotation = Quaternion.LookRotation(fall, normal);
            }
            Box("Ridge", b, new Vector3(0f, wall + rise + .1f, cz), alongX ? new Vector3(length + .5f, .12f, .25f) : new Vector3(.25f, .12f, length + .5f), cover, false);
        }

        private static void Warehouse(Transform b, StationArt a, StationTownArt t, float w, float d, int seed)
        {
            const float h = 7.4f;
            Material wall = seed % 3 == 0 ? t.Corrugated : seed % 3 == 1 ? t.OldConcrete : t.ScanBrick;
            Mass(b, w, d, h, wall);
            Box("Plinth", b, new Vector3(0f, .6f, .03f), new Vector3(w + .06f, 1.2f, .08f), t.OldConcrete, false);
            Transform front = F.Face(b, F.Side.Front, w, d);
            int doors = w > 16f ? 2 : 1;
            for (int i = 0; i < doors; i++)
            {
                float x = doors == 1 ? -w * .15f : -w * .3f + i * w * .3f;
                RollerDoor(front, a, t, x, 4.2f, 4.6f, (seed + i) % 3 == 0 ? .9f : 0f);
            }
            // The loading dock under the doors: a concrete apron at truck-bed height with rubber bumpers and steps.
            float dockW = doors == 1 ? 6f : w * .75f, dockX = -w * .15f;
            Box("Dock", b, new Vector3(dockX, .6f, 1.4f), new Vector3(dockW, 1.2f, 2.8f), t.OldConcrete);
            Box("DockEdge", b, new Vector3(dockX, 1.18f, 2.8f), new Vector3(dockW, .06f, .08f), a.Safety, false);
            for (float x = dockX - dockW * .5f + .6f; x < dockX + dockW * .5f; x += 1.6f)
                Box("Bumper", b, new Vector3(x, .8f, 2.85f), new Vector3(.35f, .5f, .15f), a.Rubber, false);
            for (int i = 0; i < 4; i++)
                Box("DockStep", b, new Vector3(dockX + dockW * .5f + .6f, .15f + i * .3f, 1.4f - i * .3f), new Vector3(1f, .3f + i * .3f, .3f), t.OldConcrete);
            F.Door(front, a, t, w * .38f, 1f, 2.3f, t.DoorSteel);
            F.WallLamp(front, a, w * .38f, 2.9f);
            for (float x = -w * .5f + 1.5f; x < w * .5f - 1f; x += 2.6f)
                F.Window(front, a, t, x, 5.6f, 1.8f, .9f, F.PaneFor(seed + Mathf.RoundToInt(x)), t.FrameDark, t.FrameDark, false, false);
            Box("SignBand", front, new Vector3(0f, 6.9f, .05f), new Vector3(w * .5f, .7f, .06f), t.Sign(seed), false);
            Box("SignMark", front, new Vector3(-w * .18f, 6.9f, .09f), new Vector3(.5f, .5f, .02f), t.SignLight, false);
            GableRoof(b, a, t, wall, w, d, h, 1.4f, alongX: false, roof: t.Corrugated);
            foreach (F.Side side in new[] { F.Side.Left, F.Side.Right })
            {
                Transform f = F.Face(b, side, w, d);
                for (float x = -d * .5f + 2f; x < d * .5f - 1.5f; x += 3f)
                    F.Window(f, a, t, x, 5.6f, 1.8f, .9f, F.PaneFor(seed + (int)side * 5 + Mathf.RoundToInt(x)), t.FrameDark, t.FrameDark, false, false);
                F.Gutters(f, t, d, h);
            }
            Vent(b, a, new Vector3(-w * .2f, h + .7f, -d * .5f));
            Vent(b, a, new Vector3(w * .2f, h + .7f, -d * .5f));
        }

        private static void RollerDoor(Transform face, StationArt a, StationTownArt t, float x, float w, float h, float gap)
        {
            Box("DoorFrame", face, new Vector3(x, h * .5f, .05f), new Vector3(w + .3f, h + .2f, .1f), a.Safety, false);
            Box("DoorDrum", face, new Vector3(x, h + .35f, .25f), new Vector3(w + .3f, .5f, .4f), a.DarkSteel, false);
            float shut = h - gap;
            Box("Shutter", face, new Vector3(x, gap + shut * .5f, .07f), new Vector3(w, shut, .03f), a.Steel, false);
            for (float y = gap + .15f; y < h; y += .15f) Box("Slat", face, new Vector3(x, y, .088f), new Vector3(w - .04f, .025f, .008f), a.DarkSteel, false);
            Box("BottomBar", face, new Vector3(x, gap + .05f, .095f), new Vector3(w, .1f, .04f), a.DarkSteel, false);
            if (gap > .1f) Box("Gap", face, new Vector3(x, gap * .5f, .06f), new Vector3(w, gap, .02f), t.Void, false);
        }

        private static void Garage(Transform b, StationArt a, StationTownArt t, float w, float d, int seed)
        {
            const float h = 5.6f;
            Material wall = seed % 2 == 0 ? t.Stucco : t.Brick(seed);
            Mass(b, w, d, h, wall);
            Transform front = F.Face(b, F.Side.Front, w, d);
            int bays = Mathf.Max(2, Mathf.FloorToInt((w - 4f) / 4f));
            for (int i = 0; i < bays; i++)
                RollerDoor(front, a, t, -w * .5f + 2.6f + i * 4f, 3.4f, 3.8f, i == 0 ? 3.4f : (i % 2 == 1 ? .5f : 0f));
            float office = w * .5f - 1.6f;
            F.Window(front, a, t, office, 1f, 1.8f, 1.8f, F.Pane.Glass, t.FrameDark, a.Trim);
            F.Door(front, a, t, office - 1.6f, .95f, 2.2f, t.DoorSteel);
            Box("SignBand", front, new Vector3(0f, 4.7f, .06f), new Vector3(w - 1f, .9f, .1f), t.Sign(seed + 3), false);
            for (int i = 0; i < 3; i++)
                Box("SignStripe", front, new Vector3(-w * .3f + i * w * .3f, 4.7f, .115f), new Vector3(w * .18f, .5f, .01f), i == 1 ? t.SignLight : t.Sign(seed + i), false);
            F.FlatRoof(b, a, t, wall, w, d, h, .7f);
            for (int i = 0; i < 3; i++) Scan(b, "old_tyre", new Vector3(w * .5f + .7f, 0f, .8f + i * .9f), 90f + i * 12f, new Vector3(.9f, .9f, .9f));
            Scan(b, "Barrel_01", new Vector3(-w * .5f - .6f, 0f, 1.2f), 10f, new Vector3(.75f, 1.1f, .75f));
            Scan(b, "barrel_03", new Vector3(-w * .5f - .6f, 0f, 2.1f), 70f, new Vector3(.75f, 1.1f, .75f));
        }

        private static void Church(Transform b, StationArt a, StationTownArt t, float w, float d, int seed)
        {
            const float nave = 9f, towerW = 5.6f, towerH = 16f;
            Material wall = seed % 2 == 0 ? t.Stone : t.Brick(seed + 1);
            Box("Nave", b, new Vector3(0f, nave * .5f, -towerW - (d - towerW) * .5f), new Vector3(w, nave, d - towerW), wall);
            GableRoof(b, a, t, wall, w, d - towerW, nave, 5f, alongX: false, roof: t.Shingles, zFrom: -towerW);
            Box("Tower", b, new Vector3(0f, towerH * .5f, -towerW * .5f), new Vector3(towerW, towerH, towerW), wall);
            Transform front = F.Face(b, F.Side.Front, w, d);
            F.Door(front, a, t, -.7f, 1.2f, 3.2f, t.DoorWood, .6f);
            F.Door(front, a, t, .7f, 1.2f, 3.2f, t.DoorWood, .6f);
            GameObject rose = Primitive(PrimitiveType.Cylinder, "RoseWindow", front, new Vector3(0f, 7f, .05f), new Vector3(2.2f, .03f, 2.2f), t.Window, false);
            rose.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            GameObject ring = Primitive(PrimitiveType.Cylinder, "RoseRing", front, new Vector3(0f, 7f, .03f), new Vector3(2.6f, .03f, 2.6f), a.Trim, false);
            ring.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            for (int i = 0; i < 4; i++)
            {
                GameObject spoke = Box("RoseTracery", front, new Vector3(0f, 7f, .085f), new Vector3(2.1f, .06f, .02f), a.Trim, false);
                spoke.transform.localRotation = Quaternion.Euler(0f, 0f, i * 45f);
            }
            GameObject clock = Primitive(PrimitiveType.Cylinder, "Clock", front, new Vector3(0f, 10.5f, .05f), new Vector3(1.5f, .03f, 1.5f), a.White, false);
            clock.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            Box("ClockHand", front, new Vector3(.2f, 10.6f, .08f), new Vector3(.5f, .05f, .02f), a.Rubber, false).transform.localRotation = Quaternion.Euler(0f, 0f, 35f);
            // The belfry: louvred openings on all four sides of the tower top.
            foreach (F.Side side in new[] { F.Side.Front, F.Side.Back, F.Side.Left, F.Side.Right })
            {
                Transform f = Group("Belfry_" + side, b);
                f.localPosition = side switch
                {
                    F.Side.Front => Vector3.zero, F.Side.Back => new Vector3(0f, 0f, -towerW),
                    F.Side.Left => new Vector3(-towerW * .5f, 0f, -towerW * .5f), _ => new Vector3(towerW * .5f, 0f, -towerW * .5f),
                };
                f.localRotation = Quaternion.Euler(0f, side switch { F.Side.Back => 180f, F.Side.Left => -90f, F.Side.Right => 90f, _ => 0f }, 0f);
                Box("Opening", f, new Vector3(0f, 13.4f, .04f), new Vector3(1.6f, 2.6f, .02f), t.Void, false);
                for (int i = 0; i < 7; i++) Box("Louvre", f, new Vector3(0f, 12.3f + i * .34f, .09f), new Vector3(1.5f, .06f, .06f), a.DarkWood, false);
                Box("Arch", f, new Vector3(0f, 14.85f, .07f), new Vector3(1.9f, .3f, .1f), a.Trim, false);
            }
            Box("TowerCornice", b, new Vector3(0f, towerH - .2f, -towerW * .5f), new Vector3(towerW + .4f, .4f, towerW + .4f), a.Trim, false);
            M.Draw(b, "Spire", M.Lathe(a, "Spire", new[] { new Vector2(0f, 0f), new Vector2(3.6f, 0f), new Vector2(3.6f, 0f), new Vector2(0f, 9f) }, 4),
                new Vector3(0f, towerH, -towerW * .5f), new Vector3(0f, 45f, 0f), Vector3.one, t.Shingles);
            Box("CrossUp", b, new Vector3(0f, towerH + 9.6f, -towerW * .5f), new Vector3(.12f, 1.4f, .12f), a.Brass, false);
            Box("CrossArm", b, new Vector3(0f, towerH + 9.9f, -towerW * .5f), new Vector3(.7f, .12f, .12f), a.Brass, false);
            // The nave's sides: tall lancets between stepped buttresses.
            foreach (F.Side side in new[] { F.Side.Left, F.Side.Right })
            {
                Transform f = F.Face(b, side, w, d);
                // A left face runs back to front along its x, a right face front to back: the tower end differs.
                float lo = side == F.Side.Left ? -d * .5f + 1.8f : -d * .5f + towerW + 1.8f;
                float hi = side == F.Side.Left ? d * .5f - towerW - 1.8f : d * .5f - 1.8f, step = (hi - lo) / 4f;
                for (int i = 0; i < 5; i++)
                {
                    float x = lo + i * step;
                    F.Window(f, a, t, x, 2.2f, 1.1f, 4.2f, i == 2 ? F.Pane.Boarded : F.Pane.Glass, a.Trim, a.Trim, true, true);
                    GameObject point = Box("Lancet", f, new Vector3(x, 6.6f, .06f), new Vector3(.8f, .8f, .04f), t.Window, false);
                    point.transform.localRotation = Quaternion.Euler(0f, 0f, 45f);
                    if (i == 4) continue;
                    Box("Buttress", f, new Vector3(x + step * .5f, 2.5f, .4f), new Vector3(.6f, 5f, .8f), wall);
                    Box("ButtressCap", f, new Vector3(x + step * .5f, 5.1f, .3f), new Vector3(.7f, .25f, .6f), a.Trim, false);
                }
            }
        }

        private static void GasStation(Transform b, StationArt a, StationTownArt t, float w, float d, int seed)
        {
            // The kiosk at the back of the forecourt.
            const float kioskW = 10f, kioskD = 7f, kioskH = 3.9f;
            Transform kiosk = Group("Kiosk", b);
            kiosk.localPosition = new Vector3(-w * .15f, 0f, -d + kioskD);
            Box("Mass", kiosk, new Vector3(0f, kioskH * .5f, -kioskD * .5f), new Vector3(kioskW, kioskH, kioskD), t.Stucco);
            F.Storefront(F.Face(kiosk, F.Side.Front, kioskW, kioskD), a, t, kioskW - .4f, kioskH, seed);
            F.FlatRoof(kiosk, a, t, t.Stucco, kioskW, kioskD, kioskH, .5f);
            Scan(kiosk, "exterior_aircon_unit", new Vector3(2f, kioskH, -4f), 0f, new Vector3(1.2f, 1.3f, 1.2f));
            Box("IceChest", kiosk, new Vector3(kioskW * .5f - .9f, .6f, .5f), new Vector3(1.4f, 1.2f, .7f), a.White);
            // The canopy over the pumps, on four steel columns, its fascia in the brand's colour.
            float cz = -d * .38f, cw = Mathf.Min(16f, w - 4f), cd = 9f;
            Material brand = t.Sign(seed);
            Box("Canopy", b, new Vector3(0f, 5.05f, cz), new Vector3(cw, .5f, cd), a.White, false);
            Box("FasciaF", b, new Vector3(0f, 5.05f, cz + cd * .5f + .02f), new Vector3(cw + .04f, .8f, .06f), brand, false);
            Box("FasciaB", b, new Vector3(0f, 5.05f, cz - cd * .5f - .02f), new Vector3(cw + .04f, .8f, .06f), brand, false);
            Box("FasciaStripe", b, new Vector3(0f, 4.85f, cz + cd * .5f + .06f), new Vector3(cw + .04f, .12f, .02f), t.SignLight, false);
            for (int i = 0; i < 6; i++)
                Box("CanopyLight", b, new Vector3(-cw * .35f + (i % 3) * cw * .35f, 4.79f, cz + (i < 3 ? -2f : 2f)), new Vector3(1.2f, .04f, .6f), a.LampOff, false);
            foreach (float x in new[] { -cw * .3f, cw * .3f })
            foreach (float z in new[] { cz - 1.6f, cz + 1.6f })
                Box("Column", b, new Vector3(x, 2.4f, z), new Vector3(.4f, 4.8f, .4f), a.White);
            foreach (float x in new[] { -cw * .3f, cw * .3f })
            {
                Box("Island", b, new Vector3(x, .12f, cz), new Vector3(1.3f, .24f, 6f), t.OldConcrete);
                foreach (float z in new[] { cz - 1.6f, cz + 1.6f })
                {
                    Box("Bollard", b, new Vector3(x, .6f, z + Mathf.Sign(z - cz) * 1.3f), new Vector3(.22f, 1f, .22f), a.Safety, false);
                }
                foreach (float z in new[] { cz - .7f, cz + .7f }) Pump(b, a, t, new Vector3(x, .24f, z), brand);
            }
            // The price sign at the front corner: blank tiles where the numbers were.
            Transform pylon = Group("PriceSign", b);
            pylon.localPosition = new Vector3(w * .5f - 1.5f, 0f, -1f);
            foreach (float x in new[] { -.9f, .9f }) Box("Leg", pylon, new Vector3(x, 2.5f, 0f), new Vector3(.25f, 5f, .25f), a.DarkSteel);
            Box("Panel", pylon, new Vector3(0f, 5.6f, 0f), new Vector3(2.4f, 2.6f, .35f), brand, false);
            for (int i = 0; i < 3; i++) Box("Tile", pylon, new Vector3(0f, 6.4f - i * .7f, .19f), new Vector3(1.8f, .5f, .02f), t.Void, false);
            Box("AirPump", b, new Vector3(w * .5f - 2f, .7f, -d * .75f), new Vector3(.6f, 1.4f, .5f), t.Sign(seed + 2));
            StationScans.Put(b, "metal_trash_can", b.TransformPoint(new Vector3(-cw * .3f + .9f, .24f, cz + 2.6f)), b.eulerAngles.y, new Vector3(.6f, 1f, .6f));
        }

        private static void Pump(Transform b, StationArt a, StationTownArt t, Vector3 at, Material brand)
        {
            Transform p = Group("Pump", b);
            p.localPosition = at;
            Box("Body", p, new Vector3(0f, .95f, 0f), new Vector3(.55f, 1.9f, .9f), a.White);
            Box("Head", p, new Vector3(0f, 2.05f, 0f), new Vector3(.6f, .35f, .95f), brand, false);
            foreach (float s in new[] { -1f, 1f })
            {
                Box("Display", p, new Vector3(s * .28f, 1.45f, 0f), new Vector3(.02f, .45f, .55f), t.Void, false);
                Box("Holster", p, new Vector3(s * .29f, 1f, .3f), new Vector3(.06f, .3f, .14f), a.DarkSteel, false);
                Box("Nozzle", p, new Vector3(s * .33f, 1.05f, .3f), new Vector3(.05f, .1f, .25f), a.Rubber, false);
                M.Strut(p, "Hose", new Vector3(s * .3f, 1.7f, .4f), new Vector3(s * .36f, .9f, .32f), .03f, a.Rubber);
            }
        }
    }
}
