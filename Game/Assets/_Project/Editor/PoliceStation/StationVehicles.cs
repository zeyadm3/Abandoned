using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using M = Abandoned.EditorTools.StationMeshes;
using S = Abandoned.EditorTools.StationVehicleShape;
using W = Abandoned.EditorTools.StationVehicleWear;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Parked cars for the stations' lots and streets, original and built in code: a smooth lofted body, a glasshouse of
    /// tinted glass under a painted roof, pillars, wheel wells, lathed tyres and rims, lamps, grille, bumpers, plates,
    /// mirrors, handles and wipers; a police cruiser adds its light bar, push bar, spotlight and white doors. Wear
    /// (<see cref="StationVehicleWear"/>) flattens tyres, crazes or strips the glass, burns the shell, opens a door. One
    /// box collider on the root; everything else is visual. The root stands on the ground, +z forward.
    /// </summary>
    public static class StationVehicles
    {
        public static Transform Car(Transform parent, string name, Vector3 at, float yaw, StationVehicleKind kind, Material paint,
            StationArt a, W wear = W.None, int seed = 0)
        {
            S s = S.For(kind);
            StationVehicleArt v = StationVehicleArt.For(a);
            bool burnt = (wear & W.Burnt) != 0;
            Material body = burnt ? v.Burnt : paint;
            Transform car = Group(name, parent);
            car.SetPositionAndRotation(at, Quaternion.Euler(0f, yaw, 0f));
            Transform shell = Group("Shell", car);
            // A burnt-out car sits on its rims, the tyres gone.
            float drop = burnt ? s.Radius * .36f : 0f;
            shell.localPosition = Vector3.down * drop;

            M.Draw(shell, "Body", M.Loft(a, "Car_" + s.Key + "_Body", s.Arched), Vector3.zero, Vector3.zero, Vector3.one, body);
            bool roofless = (wear & W.RoofCut) != 0;
            if (!burnt && !roofless)
            {
                Material glass = (wear & W.Smashed) != 0 ? v.Crazed : (wear & W.Dusty) != 0 ? v.DustyGlass : v.Glass;
                M.Draw(shell, "Glass", M.Loft(a, "Car_" + s.Key + "_Glass", s.Glass), Vector3.zero, Vector3.zero, Vector3.one, glass);
            }
            else Cabin(shell, s, v);
            if (!roofless)
            {
                M.Draw(shell, "Roof", M.Loft(a, "Car_" + s.Key + "_Roof", s.Roof), Vector3.zero, Vector3.zero, Vector3.one, body);
                Pillars(shell, s, body);
            }
            Wheels(car, shell, s, a, v, wear, seed);
            Lamps(shell, s, v, burnt);
            Trim(shell, s, v, body, seed);
            if (s.BedTo - s.BedFrom > .1f) Bed(shell, s, body, v);
            if (kind == StationVehicleKind.Cruiser) Police(shell, s, v, burnt);
            if ((wear & W.DoorOpen) != 0) OpenDoor(shell, s, body, v);
            if ((wear & W.Dusty) != 0 && !burnt)
                M.Draw(shell, "Dust", M.Loft(a, "Car_" + s.Key + "_Roof", s.Roof), new Vector3(0f, .004f, 0f), Vector3.zero, new Vector3(1.004f, 1f, 1.004f), v.Dust);

            float tilt = ((wear & W.FlatFront) != 0 ? 1.4f : 0f) - ((wear & W.FlatRear) != 0 ? 1.4f : 0f);
            if (Mathf.Abs(tilt) > .01f) car.localRotation = Quaternion.Euler(tilt, yaw, 0f);
            var solid = car.gameObject.AddComponent<BoxCollider>();
            solid.center = new Vector3(0f, s.Top * .5f, (s.Nose + s.Tail) * .5f);
            solid.size = new Vector3(s.Half * 2f + .1f, s.Top, s.Length + .1f);
            return car;
        }

        // The roof's posts: A at the windscreen, B between the doors, C at the back light.
        private static void Pillars(Transform shell, S s, Material body)
        {
            foreach (float side in new[] { -1f, 1f })
            {
                float foot = s.Half * .9f, head = s.RoofHalf - .01f, top = s.Top - .02f;
                M.Strut(shell, "PillarA", new Vector3(side * foot, s.Belt, s.GlassFront - .04f), new Vector3(side * head, top, s.RoofFront + .02f), .07f, body);
                M.Strut(shell, "PillarC", new Vector3(side * foot, s.Belt, s.GlassRear + .1f), new Vector3(side * head, top, s.RoofRear - .02f), .14f, body, .1f);
                float b = (s.RoofFront + s.RoofRear) * .5f + .05f;
                M.Strut(shell, "PillarB", new Vector3(side * (foot + .02f), s.Belt, b), new Vector3(side * head, top, b), .06f, body, .1f);
            }
        }

        // No glass (burnt or roof cut): seats, a wheel and the dash show.
        private static void Cabin(Transform shell, S s, StationVehicleArt v)
        {
            float mid = (s.GlassFront + s.GlassRear) * .5f, y = s.Belt;
            foreach (float x in new[] { -.4f, .4f })
            {
                Box("SeatBase", shell, new Vector3(x, y - .25f, mid + .25f), new Vector3(.5f, .18f, .5f), v.Seat, false);
                GameObject back = Box("SeatBack", shell, new Vector3(x, y + .05f, mid), new Vector3(.5f, .6f, .14f), v.Seat, false);
                back.transform.localRotation = Quaternion.Euler(-12f, 0f, 0f);
            }
            Box("RearSeat", shell, new Vector3(0f, y - .2f, s.GlassRear + .5f), new Vector3(1.4f, .3f, .55f), v.Seat, false);
            Box("Dash", shell, new Vector3(0f, y + .02f, s.GlassFront - .2f), new Vector3(1.5f, .22f, .35f), v.Trim, false);
            GameObject wheel = Primitive(PrimitiveType.Cylinder, "SteeringWheel", shell, new Vector3(-.4f, y + .12f, s.GlassFront - .5f), new Vector3(.36f, .015f, .36f), v.Trim, false);
            wheel.transform.localRotation = Quaternion.Euler(65f, 0f, 0f);
        }

        private static void Wheels(Transform car, Transform shell, S s, StationArt a, StationVehicleArt v, W wear, int seed)
        {
            bool burnt = (wear & W.Burnt) != 0;
            Mesh tyre = M.Tyre(a, s.Radius, .22f), rim = M.Rim(a, s.Radius * .64f, .2f);
            bool hubcaps = !burnt && seed % 3 != 0;
            foreach (float z in new[] { s.Front, s.Rear })
            foreach (float side in new[] { -1f, 1f })
            {
                bool right = side > 0f;
                float x = side * (s.Half - .13f);
                Vector3 turn = new(0f, 0f, right ? -90f : 90f);
                bool flat = z > 0f ? (wear & W.FlatFront) != 0 : (wear & W.FlatRear) != 0;
                // A flat sits the corner down on its squashed tyre (the mesh's x is up once it is turned on its side).
                float centre = s.Radius * (flat ? .9f : 1f);
                if (!burnt) M.Draw(car, "Tyre", tyre, new Vector3(x, centre, z), turn, flat ? new Vector3(.9f, 1f, 1f) : Vector3.one, v.Tyre);
                GameObject r = M.Draw(burnt ? shell : car, "Rim", rim, new Vector3(side * (s.Half - .135f), burnt ? s.Radius : centre, z), turn, Vector3.one,
                    burnt ? v.Rust : hubcaps ? v.Chrome : v.SteelWheel);
                Primitive(PrimitiveType.Cylinder, "Hub", r.transform, new Vector3(0f, .1f, 0f), new Vector3(.12f, .02f, .12f), v.Trim, false);
            }
            // Inside each pair of arches: the dark well and the suspension, so nobody sees through under the car.
            foreach (float z in new[] { s.Front, s.Rear })
            {
                float low = s.Radius * .55f, high = 2f * s.Radius + .07f;
                Box("WellLiner", shell, new Vector3(0f, (low + high) * .5f, z), new Vector3(s.Half * 2f - .5f, high - low, s.Radius * 2f + .12f), v.Well, false);
            }
            // The underside: nothing to see the sky through.
            Box("Underbody", shell, new Vector3(0f, s.Radius * .7f, (s.Nose + s.Tail) * .5f), new Vector3(s.Half * 2f - .3f, .12f, s.Length - .6f), v.Under, false);
        }

        private static void Lamps(Transform shell, S s, StationVehicleArt v, bool burnt)
        {
            StationMeshes.Ring nose = s.Body[s.Body.Length - 2], tail = s.Body[1];
            float headY = Mathf.Lerp(nose.Bottom, nose.Top, .72f), tailY = Mathf.Lerp(tail.Bottom, tail.Top, .78f);
            float headZ = s.Nose - .03f, tailZ = s.Tail + .03f;
            foreach (float side in new[] { -1f, 1f })
            {
                float hx = side * s.Half * .62f;
                Box("HeadBezel", shell, new Vector3(hx, headY, headZ), new Vector3(.38f, .15f, .08f), v.Chrome, false);
                Box("HeadLens", shell, new Vector3(hx, headY, headZ + .03f), new Vector3(.33f, .11f, .04f), burnt ? v.Trim : v.HeadLens, false);
                Box("Indicator", shell, new Vector3(side * s.Half * .86f, headY - .02f, headZ - .04f), new Vector3(.12f, .08f, .06f), burnt ? v.Trim : v.Amber, false);
                Box("TailLamp", shell, new Vector3(side * s.Half * .66f, tailY, tailZ), new Vector3(.4f, .16f, .07f), burnt ? v.Trim : v.TailLens, false);
                Box("Reverse", shell, new Vector3(side * s.Half * .4f, tailY, tailZ - .01f), new Vector3(.1f, .1f, .05f), burnt ? v.Trim : v.HeadLens, false);
            }
            Box("Grille", shell, new Vector3(0f, headY - .03f, headZ + .02f), new Vector3(s.Half * .78f, .17f, .05f), v.Trim, false);
            for (int i = 0; i < 4; i++)
                Box("GrilleBar", shell, new Vector3(0f, headY - .1f + i * .045f, headZ + .05f), new Vector3(s.Half * .76f, .012f, .01f), v.Chrome, false);
        }

        private static void Trim(Transform shell, S s, StationVehicleArt v, Material body, int seed)
        {
            StationMeshes.Ring nose = s.Body[s.Body.Length - 1], tail = s.Body[0];
            float width = s.Half * 1.86f;
            Material bumper = seed % 2 == 0 ? v.Chrome : v.Trim;
            Box("BumperF", shell, new Vector3(0f, nose.Bottom + .02f, s.Nose + .04f), new Vector3(width, .16f, .16f), bumper, false);
            Box("BumperR", shell, new Vector3(0f, tail.Bottom + .02f, s.Tail - .04f), new Vector3(width, .16f, .16f), bumper, false);
            Box("PlateF", shell, new Vector3(0f, nose.Bottom + .02f, s.Nose + .125f), new Vector3(.52f, .11f, .01f), v.Plate, false);
            Box("PlateR", shell, new Vector3(0f, tail.Bottom + .2f, s.Tail - .005f), new Vector3(.52f, .11f, .01f), v.Plate, false);
            Primitive(PrimitiveType.Cylinder, "Exhaust", shell, new Vector3(s.Half * .5f, .26f, s.Tail + .1f), new Vector3(.07f, .14f, .07f), v.Rust, false)
                .transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            float doorZ = (s.RoofFront + s.RoofRear) * .5f;
            foreach (float side in new[] { -1f, 1f })
            {
                float x = side * (s.Half + .004f);
                // Shut lines of the doors and the rubbing strip along the side.
                Box("RubStrip", shell, new Vector3(x, s.Belt - .38f, (s.Front + s.Rear) * .5f), new Vector3(.012f, .05f, s.Front - s.Rear - .9f), v.Trim, false);
                foreach (float z in new[] { s.GlassFront - .05f, doorZ, s.Rear + s.Radius + .15f })
                    Box("ShutLine", shell, new Vector3(x, (s.Belt + .35f) * .5f, z), new Vector3(.008f, s.Belt - .4f, .008f), v.Well, false);
                Box("Handle", shell, new Vector3(side * (s.Half + .012f), s.Belt - .12f, doorZ + .3f), new Vector3(.02f, .035f, .14f), v.Chrome, false);
                Box("Handle", shell, new Vector3(side * (s.Half + .012f), s.Belt - .12f, doorZ - .55f), new Vector3(.02f, .035f, .14f), v.Chrome, false);
                Transform mirror = Group("Mirror", shell);
                mirror.localPosition = new Vector3(side * (s.Half + .06f), s.Belt + .1f, s.GlassFront - .15f);
                Box("Arm", mirror, new Vector3(-side * .04f, -.04f, 0f), new Vector3(.08f, .03f, .05f), v.Trim, false);
                Box("Head", mirror, new Vector3(side * .03f, 0f, 0f), new Vector3(.1f, .12f, .16f), body, false);
                Box("Glass", mirror, new Vector3(side * .03f, 0f, -.081f), new Vector3(.08f, .1f, .005f), v.Chrome, false);
            }
            for (int i = 0; i < 2; i++)
            {
                GameObject wiper = Box("Wiper", shell, new Vector3(-.35f + i * .55f, s.Belt + .02f, s.GlassFront - .02f), new Vector3(.5f, .015f, .015f), v.Trim, false);
                wiper.transform.localRotation = Quaternion.Euler(0f, -8f, 0f);
            }
            if (seed % 4 == 1) Primitive(PrimitiveType.Cylinder, "Antenna", shell, new Vector3(-s.Half * .7f, s.Belt + .45f, s.Tail + .5f), new Vector3(.008f, .45f, .008f), v.Chrome, false);
        }

        private static void Bed(Transform shell, S s, Material body, StationVehicleArt v)
        {
            float len = s.BedTo - s.BedFrom, mid = (s.BedFrom + s.BedTo) * .5f, floor = s.Body[1].Top;
            foreach (float side in new[] { -1f, 1f })
            {
                Box("BedSide", shell, new Vector3(side * (s.Half - .04f), floor + .2f, mid), new Vector3(.08f, .42f, len), body, false);
                Box("BedRail", shell, new Vector3(side * (s.Half - .04f), floor + .42f, mid), new Vector3(.12f, .03f, len), v.Trim, false);
            }
            Box("Tailgate", shell, new Vector3(0f, floor + .2f, s.BedFrom + .04f), new Vector3(s.Half * 2f - .02f, .42f, .08f), body, false);
            Box("BedFloor", shell, new Vector3(0f, floor + .01f, mid), new Vector3(s.Half * 2f - .2f, .02f, len - .1f), v.Trim, false);
            for (int i = 0; i < 5; i++)
                Box("Rib", shell, new Vector3(-.6f + i * .3f, floor + .03f, mid), new Vector3(.04f, .02f, len - .2f), v.Rust, false);
        }

        private static void Police(Transform shell, S s, StationVehicleArt v, bool burnt)
        {
            float roof = s.Roof[0].Top, mid = (s.RoofFront + s.RoofRear) * .5f + .1f;
            Box("BarBase", shell, new Vector3(0f, roof + .04f, mid), new Vector3(1.3f, .07f, .3f), v.Trim, false);
            for (int i = 0; i < 4; i++)
                Box("BarLens", shell, new Vector3(-.48f + i * .32f, roof + .11f, mid), new Vector3(.3f, .08f, .26f),
                    burnt ? v.Trim : i < 2 ? v.TailLens : v.BlueLens, false);
            foreach (float x in new[] { -.66f, .66f }) Box("BarCap", shell, new Vector3(x, roof + .1f, mid), new Vector3(.04f, .1f, .28f), v.Chrome, false);
            foreach (float side in new[] { -1f, 1f })
                Box("BarFoot", shell, new Vector3(side * .5f, roof + .015f, mid), new Vector3(.05f, .03f, .32f), v.Trim, false);
            // The push bar on the nose.
            Transform push = Group("PushBar", shell);
            push.localPosition = new Vector3(0f, 0f, s.Nose + .16f);
            foreach (float x in new[] { -.35f, .35f }) Box("Upright", push, new Vector3(x, .62f, 0f), new Vector3(.05f, .62f, .05f), v.Trim, false);
            foreach (float y in new[] { .46f, .78f }) Box("Rail", push, new Vector3(0f, y, 0f), new Vector3(.82f, .05f, .05f), v.Trim, false);
            Transform spot = Group("Spotlight", shell);
            spot.localPosition = new Vector3(-s.Half - .04f, s.Belt + .2f, s.GlassFront - .1f);
            Primitive(PrimitiveType.Cylinder, "Lamp", spot, new Vector3(0f, 0f, .05f), new Vector3(.13f, .07f, .13f), v.Chrome, false)
                .transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            Box("Stem", spot, new Vector3(.04f, -.05f, 0f), new Vector3(.03f, .1f, .03f), v.Chrome, false);
            if (burnt) return;
            // The white doors of the livery.
            float doorZ = (s.RoofFront + s.RoofRear) * .5f;
            foreach (float side in new[] { -1f, 1f })
                Box("DoorLivery", shell, new Vector3(side * (s.Half + .006f), (s.Belt + .4f) * .5f + .05f, doorZ - .05f),
                    new Vector3(.012f, s.Belt - .52f, s.GlassFront - s.Rear - s.Radius - .3f), v.Livery, false);
        }

        // The driver's door hangs open on its hinges; the opening behind it is dark.
        private static void OpenDoor(Transform shell, S s, Material body, StationVehicleArt v)
        {
            float doorZ = (s.RoofFront + s.RoofRear) * .5f, front = s.GlassFront - .05f, len = front - doorZ;
            Box("DoorOpening", shell, new Vector3(-(s.Half + .006f), (s.Belt + .35f) * .5f + .1f, (front + doorZ) * .5f),
                new Vector3(.01f, s.Belt - .45f, len - .04f), v.Under, false);
            Transform hinge = Group("Door", shell);
            hinge.localPosition = new Vector3(-s.Half - .02f, 0f, front);
            hinge.localRotation = Quaternion.Euler(0f, -55f, 0f);
            Box("Skin", hinge, new Vector3(0f, (s.Belt + .35f) * .5f + .05f, -len * .5f), new Vector3(.07f, s.Belt - .4f, len), body, false);
            Box("Frame", hinge, new Vector3(.02f, s.Belt + .2f, -len * .5f), new Vector3(.03f, .4f, len - .05f), v.Trim, false);
            Box("Panel", hinge, new Vector3(.045f, (s.Belt + .35f) * .5f + .05f, -len * .5f), new Vector3(.01f, s.Belt - .5f, len - .1f), v.Seat, false);
        }
    }
}
