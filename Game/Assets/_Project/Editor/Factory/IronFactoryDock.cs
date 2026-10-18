using Abandoned.Core;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using L = Abandoned.EditorTools.IronFactoryLayout;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The rail loading dock in front of the plant's one opening: a siding along the south wall with two dead
    /// boxcars parked on it, buffer stops, and a steel canopy over the dock where the collection truck backs
    /// in. The siding breaks around the truck's approach, so the haul lane from the dock to the truck stays
    /// clear. The wider rail yard comes with the surroundings (step 7).
    /// </summary>
    public static class IronFactoryDock
    {
        private const float RailZ = -3f;
        private const float Gauge = 1.435f;
        private const float GapHalf = 10f;

        public static void Build(Transform root, IronFactoryArt art)
        {
            Transform dock = Group("RailDock", root);
            float width = L.Width * L.Tile;
            Material ballast = GetMaterial("IronFactory_Ballast", new Color(.16f, .15f, .14f));
            Material boxcar = GetMaterial("IronFactory_Boxcar", new Color(.3f, .12f, .08f));

            // The siding, both sides of the truck's approach: walk-over height, so no colliders.
            Siding(dock, "West", 2f, L.DockX - GapHalf, art, ballast);
            Siding(dock, "East", L.DockX + GapHalf, width - 2f, art, ballast);
            foreach (float x in new[] { 2f, L.DockX - GapHalf, L.DockX + GapHalf, width - 2f })
                Box("BufferStop", dock, new Vector3(x, .5f, RailZ), new Vector3(.6f, 1f, Gauge + .6f), art.Safety);

            Boxcar(dock, "Boxcar_West", new Vector3(30f, 0f, RailZ), boxcar, art);
            Boxcar(dock, "Boxcar_East", new Vector3(92f, 0f, RailZ), boxcar, art);

            // The canopy over the dock opening and the steel plate bridging the threshold.
            float canopyY = L.DockHeight + .3f;
            Box("Canopy", dock, new Vector3(L.DockX, canopyY, -3f), new Vector3(L.DockWidth + 6f, .25f, 6f), art.Steel);
            foreach (float side in new[] { -1f, 1f })
                Box("CanopyPost", dock, new Vector3(L.DockX + side * (L.DockWidth * .5f + 2.6f), canopyY * .5f, -5.7f),
                    new Vector3(.3f, canopyY, .3f), art.Steel);
            Box("DockPlate", dock, new Vector3(L.DockX, .015f, 0f), new Vector3(L.DockWidth - .4f, .03f, 1.6f), art.Steel, false);
            Box("DockEdge", dock, new Vector3(L.DockX, .035f, -.85f), new Vector3(L.DockWidth - .4f, .02f, .12f), art.Safety, false);
            dock.gameObject.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Concrete);
            MarkStatic(dock);
        }

        private static void Siding(Transform parent, string name, float from, float to, IronFactoryArt art, Material ballast)
        {
            Transform siding = Group("Siding_" + name, parent);
            float length = to - from, mid = (from + to) * .5f;
            Box("Ballast", siding, new Vector3(mid, -.01f, RailZ), new Vector3(length, .04f, Gauge + 1.4f), ballast, false);
            foreach (float side in new[] { -1f, 1f })
                Box("Rail", siding, new Vector3(mid, .08f, RailZ + side * Gauge * .5f), new Vector3(length, .14f, .07f), art.Steel, false);
            for (float x = from + .6f; x < to; x += 2f)
                Box("Sleeper", siding, new Vector3(x, .02f, RailZ), new Vector3(.25f, .06f, Gauge + 1f), art.Rust, false);
        }

        // A dead boxcar on its trucks, doors shut: cover outside the dock, solid all the way down so nobody
        // crawls underneath into a spot no one can see.
        private static void Boxcar(Transform parent, string name, Vector3 at, Material body, IronFactoryArt art)
        {
            Transform car = Group(name, parent);
            car.position = at;
            const float length = 14f, width = 3f, height = 3.4f, deck = 1.1f;
            var solid = car.gameObject.AddComponent<BoxCollider>();
            solid.center = new Vector3(0f, (deck + height) * .5f, 0f);
            solid.size = new Vector3(length, deck + height, width);
            Box("Body", car, new Vector3(0f, deck + height * .5f, 0f), new Vector3(length, height, width), body, false);
            Box("Roof", car, new Vector3(0f, deck + height + .08f, 0f), new Vector3(length + .2f, .16f, width + .2f), art.Rust, false);
            foreach (float side in new[] { -1f, 1f })
                Box("Door", car, new Vector3(0f, deck + height * .45f, side * (width * .5f + .03f)), new Vector3(3f, height * .85f, .06f), art.Rust, false);
            foreach (float end in new[] { -1f, 1f })
            {
                Box("Truck", car, new Vector3(end * (length * .5f - 2f), .5f, 0f), new Vector3(2.6f, .7f, 2.2f), art.Bed, false);
                Box("Coupler", car, new Vector3(end * (length * .5f + .3f), .9f, 0f), new Vector3(.6f, .25f, .3f), art.Steel, false);
            }
            car.gameObject.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Metal);
        }
    }
}
