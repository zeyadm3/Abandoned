using Abandoned.Core;
using Abandoned.Extraction;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using P = Abandoned.EditorTools.StationPlan;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The lot round a station: ground slabs round the footprint (never under it), the company truck backed up to the
    /// carry route with the shared collection apron and markings, and the street furniture both stations use (fences,
    /// abandoned cars, trees, kerbs). Everything lives under <c>Exterior</c> (or <c>Truck</c>), which the NavMesh bake
    /// leaves out: monsters reach the crew only once they are inside. The open world places the whole scene; nothing here
    /// assumes a world position.
    /// </summary>
    public static class StationSite
    {
        public static Transform Exterior(Transform root)
        {
            Transform t = root.Find("Exterior");
            return t != null ? t : Group("Exterior", root);
        }

        public static Transform GroundSlabs(Transform root)
        {
            Transform exterior = Exterior(root), t = exterior.Find("GroundSlabs");
            return t != null ? t : Group("GroundSlabs", exterior);
        }

        /// <summary>A flat piece of ground, top at <paramref name="top"/>, tagged for footsteps.</summary>
        public static GameObject Slab(Transform parent, string name, Rect r, Material m, SurfaceMaterial surface, float top = -.02f, float thick = .4f)
        {
            GameObject g = Box(name, parent, new Vector3(r.center.x, top - thick * .5f, r.center.y), new Vector3(r.width, thick, r.height), m);
            g.AddComponent<SurfaceTag>().EditorSet(surface);
            return g;
        }

        /// <summary>
        /// The truck backed up with its rear at <paramref name="rear"/>, cab toward <paramref name="forward"/> (the ramp
        /// runs back toward the building), on the shared concrete apron with the collection markings.
        /// </summary>
        public static TruckCargo Truck(Transform root, StationArt art, Vector3 rear, Vector3 forward)
        {
            forward.y = 0f;
            forward.Normalize();
            Transform ground = GroundSlabs(root);
            Vector3 apronCentre = rear - forward * 6.5f;
            GameObject apron = Box("DockApron", ground, new Vector3(apronCentre.x, -.16f, apronCentre.z),
                new Vector3(MeridianCollectionBayBuilder.ApproachWidth + .6f, .12f, 4.4f), art.Concrete);
            // The collection bay widens the apron along its local x: turn it so x runs across the approach.
            apron.transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
            apron.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Concrete);
            TruckCargo cargo = TruckBuilder.Build(Group("Truck", root), rear, forward);
            MeridianCollectionBayBuilder.Build(root, cargo);
            return cargo;
        }

        /// <summary>A chain-link fence from a to b on posts every 2.5 m: solid, 2.2 m, with a mesh you see through.</summary>
        public static void Fence(Transform parent, string name, Vector3 a, Vector3 b, StationArt art, float height = 2.2f)
        {
            Transform fence = Group(name, parent);
            Vector3 d = b - a;
            float length = d.magnitude;
            fence.SetPositionAndRotation(a, Quaternion.LookRotation(d.normalized, Vector3.up));
            int posts = Mathf.Max(2, Mathf.CeilToInt(length / 2.5f) + 1);
            for (int i = 0; i < posts; i++)
                Primitive(PrimitiveType.Cylinder, "Post", fence, new Vector3(0f, height * .5f, length * i / (posts - 1)),
                    new Vector3(.07f, height * .5f, .07f), art.Steel, false);
            Box("TopRail", fence, new Vector3(0f, height - .05f, length * .5f), new Vector3(.05f, .05f, length), art.Steel, false);
            Box("Mesh", fence, new Vector3(0f, height * .5f, length * .5f), new Vector3(.015f, height - .1f, length), art.Grating, false);
            var solid = fence.gameObject.AddComponent<BoxCollider>();
            solid.center = new Vector3(0f, height * .5f, length * .5f);
            solid.size = new Vector3(.12f, height, length);
        }

        /// <summary>An abandoned sedan (or a patrol car with a light bar): solid, flat tyres, dust on the glass.</summary>
        public static Transform Car(Transform parent, string name, Vector3 at, float yaw, Material body, StationArt art, bool lightBar = false)
        {
            Transform car = Group(name, parent);
            car.SetPositionAndRotation(at, Quaternion.Euler(0f, yaw, 0f));
            Box("Body", car, new Vector3(0f, .62f, 0f), new Vector3(1.8f, .62f, 4.7f), body, false);
            Box("Cabin", car, new Vector3(0f, 1.18f, -.2f), new Vector3(1.6f, .52f, 2.3f), body, false);
            Box("Windscreen", car, new Vector3(0f, 1.18f, .96f), new Vector3(1.5f, .46f, .04f), art.Glass, false);
            Box("RearScreen", car, new Vector3(0f, 1.18f, -1.36f), new Vector3(1.5f, .42f, .04f), art.Glass, false);
            foreach (float side in new[] { -1f, 1f })
            {
                Box("SideGlass", car, new Vector3(side * .81f, 1.2f, -.2f), new Vector3(.02f, .38f, 2.1f), art.Glass, false);
                foreach (float z in new[] { -1.45f, 1.45f })
                {
                    GameObject wheel = Primitive(PrimitiveType.Cylinder, "Tyre", car, new Vector3(side * .78f, .3f, z), new Vector3(.62f, .12f, .62f), art.Rubber, false);
                    wheel.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                }
            }
            Box("Bumper_F", car, new Vector3(0f, .42f, 2.38f), new Vector3(1.82f, .18f, .1f), art.Chrome, false);
            Box("Bumper_R", car, new Vector3(0f, .42f, -2.38f), new Vector3(1.82f, .18f, .1f), art.Chrome, false);
            if (lightBar)
            {
                Box("LightBar", car, new Vector3(0f, 1.5f, -.1f), new Vector3(1.2f, .1f, .25f), art.Rubber, false);
                Box("Lens_R", car, new Vector3(-.35f, 1.52f, -.1f), new Vector3(.45f, .08f, .22f), art.Red, false);
                Box("Lens_B", car, new Vector3(.35f, 1.52f, -.1f), new Vector3(.45f, .08f, .22f), art.AccentDark, false);
                Box("Door_L", car, new Vector3(-.905f, .75f, .1f), new Vector3(.01f, .5f, 1.9f), art.White, false);
                Box("Door_R", car, new Vector3(.905f, .75f, .1f), new Vector3(.01f, .5f, 1.9f), art.White, false);
            }
            var solid = car.gameObject.AddComponent<BoxCollider>();
            solid.center = new Vector3(0f, .75f, 0f);
            solid.size = new Vector3(1.85f, 1.5f, 4.8f);
            return car;
        }

        /// <summary>A plain tree (the map-site pass swaps trees by name and shape for scanned ones).</summary>
        public static void Tree(Transform parent, Vector3 at, float height, StationArt art, Material leaves)
        {
            Transform tree = Group("Tree", parent);
            tree.position = at;
            Primitive(PrimitiveType.Cylinder, "Trunk", tree, new Vector3(0f, height * .3f, 0f), new Vector3(.3f, height * .3f, .3f), art.DarkWood);
            Primitive(PrimitiveType.Sphere, "Canopy", tree, new Vector3(0f, height * .7f, 0f), new Vector3(height * .5f, height * .55f, height * .5f), leaves, false);
        }

        /// <summary>A kerb along a line (a low solid strip).</summary>
        public static void Kerb(Transform parent, Vector3 a, Vector3 b, StationArt art)
        {
            Vector3 d = b - a;
            GameObject k = Box("Kerb", parent, (a + b) * .5f + Vector3.up * .06f, new Vector3(.25f, .16f, d.magnitude), art.Kerb, false);
            k.transform.rotation = Quaternion.LookRotation(d.normalized, Vector3.up);
        }

        /// <summary>The four slabs round the footprint, to <paramref name="margin"/> metres out (west, east, south, north).</summary>
        public static void Surround(Transform root, StationPlan plan, Vector4 margin, Material west, Material east, Material south, Material north)
        {
            Transform g = GroundSlabs(root);
            float w = plan.Width * P.Tile, d = plan.Depth * P.Tile;
            Slab(g, "South", new Rect(-margin.x, -margin.z, w + margin.x + margin.y, margin.z), south, Surface(south));
            Slab(g, "North", new Rect(-margin.x, d, w + margin.x + margin.y, margin.w), north, Surface(north));
            Slab(g, "West", new Rect(-margin.x, 0f, margin.x, d), west, Surface(west));
            Slab(g, "East", new Rect(w, 0f, margin.y, d), east, Surface(east));
        }

        private static SurfaceMaterial Surface(Material m) =>
            m != null && m.name.EndsWith("Asphalt") ? SurfaceMaterial.Asphalt :
            m != null && (m.name.EndsWith("Grass") || m.name.EndsWith("Dirt")) ? SurfaceMaterial.Dirt : SurfaceMaterial.Concrete;
    }
}
