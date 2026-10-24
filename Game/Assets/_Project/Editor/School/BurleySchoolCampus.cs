using System.Collections.Generic;
using Abandoned.Core;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using Plan = Abandoned.EditorTools.StationTownPlan;
using Site = Abandoned.EditorTools.StationSite;
using Street = Abandoned.EditorTools.StationStreet;
using T = Abandoned.EditorTools.BurleySchoolTown;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Pine Hollow's grounds round the building, laid out so every outside door has a cleared, lit way back to the truck at the
    /// front loop: a walk from the auditorium's south door, a covered walk along the west wing past a bus lane with three
    /// buses, a path up past two portable classrooms to a fenced boiler-room service yard (its north door opens into
    /// it), and from the gym's north door round its east side and along its front. A service drive runs from Henry
    /// Avenue through the yard to the staff lot, which had no way in. Everything at the streets' height, off the NavMesh.
    /// </summary>
    public static class BurleySchoolCampus
    {
        public static void Build(Transform root, Transform ext, BurleySchoolArt art, StationArt a, StationRingStock stock, IList<int> cars)
        {
            Transform campus = Group("CampusGrounds", ext);
            Paths(root, art);
            BusLane(root, campus, art, a);
            Portable(campus, art, "PortableClassroom_A", new Vector3(10f, T.Top, 69f));
            Portable(campus, art, "PortableClassroom_B", new Vector3(10f, T.Top, 81f));
            ServiceYard(root, campus, art, a);
            StaffCars(campus, stock, cars);
            foreach ((Vector3 foot, Vector3 toward) in new[]
                     {
                         (new Vector3(-10f, T.Top, 30f), new Vector3(-6f, T.Top, 30f)), (new Vector3(-10f, T.Top, 46f), new Vector3(-6f, T.Top, 46f)),
                         (new Vector3(23.4f, T.Top, 78f), new Vector3(14f, T.Top, 78f)), (new Vector3(86f, T.Top, 14.6f), new Vector3(86f, T.Top, 18f)),
                         (new Vector3(90f, T.Top, 72.6f), new Vector3(90f, T.Top, 68f)), (new Vector3(93f, T.Top, 108.8f), new Vector3(93f, T.Top, 104f)),
                     })
                Street.Lamp(campus, a, foot, toward, 7f);
        }

        // Cleared concrete walks (the snow shovelled off), edge to edge, never overlapping.
        private static void Paths(Transform root, BurleySchoolArt art)
        {
            (string name, Rect r)[] walks =
            {
                ("FrontWestWalk", Rect.MinMaxRect(-3f, 10f, 22f, 14f)),          // from the bus walk to the loop
                ("AuditoriumDoorWalk", Rect.MinMaxRect(10f, 14f, 14f, 19.85f)),   // the west wing's south door
                ("WestWingWalk", Rect.MinMaxRect(-3f, 14f, -.15f, 59f)),          // under the bus canopy
                ("NorthWestWalk", Rect.MinMaxRect(-3f, 59f, 23f, 62f)),
                ("PortablesWalk", Rect.MinMaxRect(20f, 62f, 23f, 94f)),           // past the portables
                ("YardWalk", Rect.MinMaxRect(20f, 94f, 36f, 98f)),                // into the service yard
                ("GymDoorWalk", Rect.MinMaxRect(80f, 68.15f, 101.4f, 72f)),      // the gym's north door
                ("GymEastWalk", Rect.MinMaxRect(101.4f, 19f, 103.6f, 72f)),  // clear of the drift on the wall
                ("GymFrontWalk", Rect.MinMaxRect(70f, 15f, 103.6f, 19f)),         // back to the loop
            };
            foreach ((string name, Rect r) in walks) Plan.Ground(root, name, r, art.Sidewalk, SurfaceMaterial.Concrete);
        }

        // A pull-in off Henry along the west wing: three buses still in their bays, a kerb and a covered walk.
        private static void BusLane(Transform root, Transform campus, BurleySchoolArt art, StationArt a)
        {
            Plan.Ground(root, "BusLane", Rect.MinMaxRect(-9.5f, 18f, -3f, 58f), art.SnowyAsphalt, SurfaceMaterial.Asphalt);
            Site.Kerb(campus, new Vector3(-3f, T.Top, 18f), new Vector3(-3f, T.Top, 58f), a);
            foreach (float z in new[] { 31.3f, 44.3f })
                Box("BayLine", campus, new Vector3(-6.25f, T.Top + .006f, z), new Vector3(6f, .01f, .12f), art.YellowStripe, false);
            int bus = 0;
            foreach (float z in new[] { 24f, 37f, 50f })
                BurleySchoolCampusProps.Bus(campus, $"SchoolBus_Lane_{bus++}", new Vector3(-6.25f, T.Top, z), 0f, art);
            Transform canopy = Group("BusCanopy", campus);
            for (float z = 20f; z <= 57f; z += 6f)
                Box("Post", canopy, new Vector3(-2.75f, T.Top + 1.5f, z), new Vector3(.15f, 3f, .15f), art.MetalSteel);
            Box("Roof", canopy, new Vector3(-1.6f, T.Top + 3.06f, 38.5f), new Vector3(3.1f, .14f, 38f), art.MetalSteel, false);
            Box("Fascia", canopy, new Vector3(-3.13f, T.Top + 2.92f, 38.5f), new Vector3(.06f, .32f, 38f), art.LockerGreen, false);
            Box("RoofSnow", canopy, new Vector3(-1.6f, T.Top + 3.2f, 38.5f), new Vector3(2.95f, .14f, 37.8f), art.Snow, false);
        }

        // A portable classroom on its blocks, locked: an aluminium-skinned box, green trim, a landing and steps to its door
        // on the walk, a wall air-conditioner, snow on the roof. Solid.
        private static void Portable(Transform parent, BurleySchoolArt art, string name, Vector3 centre)
        {
            const float length = 16f, width = 8f, lift = .6f, height = 3.1f;
            Transform p = Group(name, parent);
            p.position = centre;
            Box("Body", p, new Vector3(0f, lift + height * .5f, 0f), new Vector3(length, height, width), art.PlasterWall);
            Box("Skirt", p, new Vector3(0f, lift * .5f, 0f), new Vector3(length - .2f, lift, width - .2f), art.MetalSteel, false);
            Box("RoofCap", p, new Vector3(0f, lift + height + .08f, 0f), new Vector3(length + .3f, .16f, width + .3f), art.MetalSteel, false);
            Box("RoofSnow", p, new Vector3(0f, lift + height + .22f, 0f), new Vector3(length, .14f, width), art.Snow, false);
            foreach (float side in new[] { -1f, 1f })
            {
                float z = side * (width * .5f + .012f);
                Box("Trim", p, new Vector3(0f, lift + .18f, z), new Vector3(length, .3f, .02f), art.LockerGreen, false);
                for (int i = -2; i <= 2; i++)
                {
                    Box("Window", p, new Vector3(i * 3f, lift + 1.75f, z), new Vector3(1.6f, 1.1f, .03f), art.WindowGlass, false);
                    Box("WindowFrame", p, new Vector3(i * 3f, lift + 1.75f, z), new Vector3(1.75f, 1.25f, .02f), art.MetalSteel, false);
                }
            }
            float door = length * .5f;
            Box("Door", p, new Vector3(door + .015f, lift + 1.05f, 1.5f), new Vector3(.03f, 2.1f, .95f), art.MetalSteel, false);
            Box("Landing", p, new Vector3(door + .9f, lift - .05f, 1.5f), new Vector3(1.8f, .1f, 1.8f), art.OakWood);
            for (int step = 0; step < 3; step++)
                Box($"Step_{step}", p, new Vector3(door + 1.95f + step * .3f, lift - .2f - step * .2f, 1.5f), new Vector3(.3f, .08f, 1.4f), art.OakWood);
            foreach (float z in new[] { .6f, 2.4f })
                Box("Rail", p, new Vector3(door + 1.2f, lift + .5f, z), new Vector3(2.4f, .05f, .05f), art.MetalSteel, false);
            Box("WallAc", p, new Vector3(-door - .35f, lift + 2.2f, 0f), new Vector3(.7f, .8f, 1f), art.MetalSteel, false);
            Box("AcSnow", p, new Vector3(-door - .35f, lift + 2.66f, 0f), new Vector3(.72f, .1f, 1.02f), art.Snow, false);
        }

        // Behind the boiler room: a fenced asphalt yard its north door opens into, with dumpsters, the heating-oil tank,
        // a transformer, pallets, a delivery van and a pushed-up snow heap; the drive carries on east to the staff lot.
        private static void ServiceYard(Transform root, Transform campus, BurleySchoolArt art, StationArt a)
        {
            Plan.Ground(root, "ServiceYard", Rect.MinMaxRect(36f, 92.2f, 76f, 112f), art.SnowyAsphalt, SurfaceMaterial.Asphalt);
            Plan.Ground(root, "StaffLotDrive", Rect.MinMaxRect(76f, 100f, 110f, 108f), art.SnowyAsphalt, SurfaceMaterial.Asphalt);
            Plan.Ground(root, "StaffLotLink", Rect.MinMaxRect(102f, 88f, 110f, 100f), art.SnowyAsphalt, SurfaceMaterial.Asphalt);
            Transform y = Group("BoilerServiceYard", campus);
            Site.Fence(y, "FenceNorth", new Vector3(36f, T.Top, 112f), new Vector3(76f, T.Top, 112f), a);
            Site.Fence(y, "FenceWest", new Vector3(36f, T.Top, 108f), new Vector3(36f, T.Top, 112f), a);
            Site.Fence(y, "FenceEastS", new Vector3(76f, T.Top, 92.2f), new Vector3(76f, T.Top, 100f), a);
            Site.Fence(y, "FenceEastN", new Vector3(76f, T.Top, 108f), new Vector3(76f, T.Top, 112f), a);
            Street.Dumpster(y, a, new Vector3(41f, T.Top, 110.6f), 0f, true);
            Street.Dumpster(y, a, new Vector3(45.5f, T.Top, 110.6f), 0f, false);
            Street.Dumpster(y, a, new Vector3(50f, T.Top, 110.6f), 0f, true);
            Transform tank = Group("HeatingOilTank", y);
            tank.position = new Vector3(48f, T.Top, 96.5f);
            Primitive(PrimitiveType.Cylinder, "Tank", tank, new Vector3(0f, 1.25f, 0f), new Vector3(1.7f, 2.5f, 1.7f), art.Rust).transform.localRotation =
                Quaternion.Euler(0f, 0f, 90f);
            foreach (float x in new[] { -1.6f, 1.6f })
                Box("Cradle", tank, new Vector3(x, .3f, 0f), new Vector3(.25f, .6f, 1.5f), art.Concrete);
            Box("TankSnow", tank, new Vector3(0f, 2.12f, 0f), new Vector3(4.4f, .12f, .9f), art.Snow, false);
            Box("Transformer", y, new Vector3(73.5f, T.Top + .8f, 110f), new Vector3(1.4f, 1.6f, 1.2f), art.MetalSteel);
            Box("TransformerPad", y, new Vector3(73.5f, T.Top + .05f, 110f), new Vector3(2f, .1f, 1.8f), art.Concrete);
            for (int i = 0; i < 3; i++)
                Box("PalletStack", y, new Vector3(64.5f + i * 1.4f, T.Top + .3f + i * .075f, 110.6f), new Vector3(1.2f, .6f + i * .15f, 1.2f), art.OakWood);
            Primitive(PrimitiveType.Sphere, "SnowHeap", y, new Vector3(57f, T.Top, 110f), new Vector3(4f, 2f, 3f), art.SnowDrift, false);
            Transform van = Group("DeliveryVan", y);
            van.SetPositionAndRotation(new Vector3(70f, T.Top, 96.5f), Quaternion.Euler(0f, 270f, 0f));
            StationApparatus.BoxVan(van, a, art.WhiteStripe, art.LockerGreen, 3);
            var solid = van.gameObject.AddComponent<BoxCollider>();
            solid.center = new Vector3(0f, 1.4f, 0f);
            solid.size = new Vector3(2.3f, 2.8f, 6.2f);
            Street.Lamp(y, a, new Vector3(37f, T.Top, 111f), new Vector3(45f, T.Top, 104f), 7f);
            Street.Lamp(y, a, new Vector3(75f, T.Top, 111f), new Vector3(68f, T.Top, 104f), 7f);
        }

        // A few staff cars still in the lot's bays, noses to the kerb.
        private static void StaffCars(Transform campus, StationRingStock stock, IList<int> cars)
        {
            if (cars.Count == 0) return;
            Rect lot = BurleySchoolLayout.ParkingLot;
            var rng = new System.Random(9721);
            foreach ((int stall, bool west) in new[] { (1, true), (2, true), (5, true), (9, true), (0, false), (3, false), (4, false), (8, false), (11, false) })
            {
                float z = lot.yMin + 4f + stall * 3.1f + 1.55f;
                float x = west ? lot.xMin + 4.2f : lot.xMax - 4.2f;
                stock.Place(campus, cars[rng.Next(cars.Count)], new Vector3(x, T.Top, z), west ? 270f : 90f);
            }
        }
    }
}
