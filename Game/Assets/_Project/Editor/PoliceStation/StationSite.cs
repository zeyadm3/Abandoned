using Abandoned.Core;
using Abandoned.Extraction;
using UnityEditor;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using P = Abandoned.EditorTools.StationPlan;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The lot round a station: ground slabs round the footprint (never under it), the van's parking bay at the carry
    /// route (the open world's van is the job's truck; no map has one of its own), and the street furniture both stations
    /// use (fences, abandoned cars, kerbs). No trees, sky or grass: the world and Art > Upgrade New Maps add those.
    /// Everything lives under <c>Exterior</c>, which the NavMesh bake leaves out: monsters reach the crew only once they
    /// are inside. The scene stays at its own origin; the world moves round it.
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

        public const string VanParkingName = "VanParking";

        /// <summary>
        /// Where the crew's van parks (the open world's van is the job's truck; no map has a truck of its own): its rear
        /// at <paramref name="rear"/>, cab toward <paramref name="forward"/>, ramp back toward the building. Painted bay
        /// lines on the ground, a loading box behind the rear doors, and an empty <c>Exterior/VanParking</c> marker
        /// (position = the van's rear, forward = its heading) for the world's survey.
        /// </summary>
        public static Transform VanBay(Transform root, StationArt art, Vector3 rear, Vector3 forward)
        {
            forward.y = 0f;
            forward.Normalize();
            Transform ext = Exterior(root);
            Transform bay = Group("VanBay", ext);
            bay.SetPositionAndRotation(new Vector3(rear.x, 0f, rear.z), Quaternion.LookRotation(forward, Vector3.up));
            const float width = 3.4f, length = 8.6f, y = -.015f;
            // The van's own outline, from its rear doors to past its cab.
            foreach (float s in new[] { -1f, 1f })
                Box("BayEdge", bay, new Vector3(s * width * .5f, y, length * .5f), new Vector3(.14f, .01f, length), art.Paint, false);
            Box("BayEnd", bay, new Vector3(0f, y, length), new Vector3(width, .01f, .14f), art.Paint, false);
            // The loading box behind the doors (where the ramp comes down): yellow hatching, kept clear.
            Box("LoadEdge", bay, new Vector3(0f, y, -4.6f), new Vector3(width, .01f, .14f), art.Safety, false);
            foreach (float s in new[] { -1f, 1f })
                Box("LoadSide", bay, new Vector3(s * width * .5f, y, -2.3f), new Vector3(.14f, .01f, 4.6f), art.Safety, false);
            for (int i = 0; i < 5; i++)
            {
                GameObject hatch = Box("Hatch", bay, new Vector3(-1.2f + i * .6f, y, -2.3f), new Vector3(.1f, .01f, 3.6f), art.Safety, false);
                hatch.transform.localRotation = Quaternion.Euler(0f, 30f, 0f);
            }
            Transform marker = Group(VanParkingName, ext);
            marker.SetPositionAndRotation(new Vector3(rear.x, 0f, rear.z), Quaternion.LookRotation(forward, Vector3.up));
            return marker;
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

        /// <summary>An abandoned sedan (or a police cruiser): the detailed car of <see cref="StationVehicles"/>, solid.</summary>
        public static Transform Car(Transform parent, string name, Vector3 at, float yaw, Material body, StationArt art, bool lightBar = false,
            StationVehicleWear wear = StationVehicleWear.None)
        {
            int seed = Mathf.Abs(Mathf.RoundToInt(at.x * 7f + at.z * 13f));
            StationVehicleKind kind = lightBar ? StationVehicleKind.Cruiser : (StationVehicleKind)(new[] { 0, 2, 3, 0, 4 }[seed % 5]);
            return StationVehicles.Car(parent, name, at, yaw, kind, body, art, wear, seed);
        }

        /// <summary>A car under a tarp (the scanned covered car), or a code-built sedan if the scan is missing.</summary>
        public static Transform CoveredCar(Transform parent, Vector3 at, float yaw, StationArt art)
        {
            GameObject g = StationScans.Put(parent, "covered_car", at, yaw, new Vector3(2.4f, 2f, 5.4f), solid: true);
            return g != null ? g.transform : Car(parent, "Car", at, yaw, StationVehicleArt.For(art).Paint(Mathf.RoundToInt(at.x)), art,
                wear: StationVehicleWear.Dusty);
        }

        /// <summary>
        /// Only big things hide what is behind them: every renderer under <paramref name="root"/> smaller than
        /// <paramref name="size"/> metres on its longest side stops being an occluder (it stays static and is still
        /// culled), so the occlusion bake works on building masses and walls, not on thousands of mullions and cars.
        /// </summary>
        public static int TrimOccluders(Transform root, float size = 4f)
        {
            int trimmed = 0;
            foreach (Renderer r in root.GetComponentsInChildren<Renderer>(true))
            {
                Vector3 e = r.bounds.size;
                if (Mathf.Max(e.x, Mathf.Max(e.y, e.z)) >= size) continue;
                StaticEditorFlags flags = GameObjectUtility.GetStaticEditorFlags(r.gameObject);
                if ((flags & StaticEditorFlags.OccluderStatic) == 0) continue;
                GameObjectUtility.SetStaticEditorFlags(r.gameObject, flags & ~StaticEditorFlags.OccluderStatic);
                trimmed++;
            }
            return trimmed;
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
