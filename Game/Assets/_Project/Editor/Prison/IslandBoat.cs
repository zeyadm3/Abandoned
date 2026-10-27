using Abandoned.Core;
using Abandoned.Extraction;
using Abandoned.Structure;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using K = Abandoned.EditorTools.IslandKit;
using L = Abandoned.EditorTools.IslandPrisonLayout;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The way off the island: the company's hired supply launch, moored stern-on to the quay. Functionally it is the
    /// truck (the same <see cref="TruckCargo"/> hold trigger and ride zone, ignition lever, haul board, floodlights
    /// and sanctuary), flagged as a boat so the prompts say so; its hold is the truck's bay, flush with the quay, a
    /// gangway across the gap. The hull is art only and rocks a little on the swell; the deck you stand on never moves
    /// (loot would slide off). Local +z runs from the stern (the open hold) to the bow.
    /// </summary>
    public static class IslandBoat
    {
        public const string GroupName = "Boat";
        private const float BayLength = TruckBuilder.BayLength, BayWidth = TruckBuilder.BayWidth, Floor = TruckBuilder.FloorHeight;
        private const float Gunwale = 1f, HullHalf = 1.75f, Bow = 12f, Keel = -2.4f;

        public static TruckCargo Build(Transform parent, IslandPrisonArt art)
        {
            Material lever = GetMaterial("Greybox_Ignition", new Color(1f, .85f, .1f));
            Transform group = Group(GroupName, parent);
            Transform root = Group("Truck", group);
            root.position = L.BoatStern + L.BoatForward * (BayLength * .5f);
            root.rotation = Quaternion.LookRotation(L.BoatForward, Vector3.up);
            float half = BayLength * .5f, w = BayWidth * .5f;

            // The hold: the functional bay floor, its bulwarks (colliders) and the forward bulkhead the board hangs on.
            Box("BayFloor", root, new Vector3(0f, Floor * .5f, 0f), new Vector3(BayWidth, Floor, BayLength), art.Plate);
            Box("Wall_L", root, new Vector3(-w - .1f, Floor + Gunwale * .5f, 0f), new Vector3(.2f, Gunwale, BayLength), art.Hull);
            Box("Wall_R", root, new Vector3(w + .1f, Floor + Gunwale * .5f, 0f), new Vector3(.2f, Gunwale, BayLength), art.Hull);
            Box("Wall_Front", root, new Vector3(0f, Floor + 1.1f, half + .05f), new Vector3(BayWidth + .4f, 2.2f, .1f), art.Hull);
            var ignition = Box("Ignition", root, new Vector3(w - .35f, Floor + 1.1f, half - .2f), new Vector3(.3f, .3f, .3f), lever);
            ignition.AddComponent<TruckIgnition>();
            var bay = new GameObject("Bay");
            bay.transform.SetParent(root, false);
            bay.layer = LayerMask.NameToLayer("Ignore Raycast");
            var trigger = bay.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.center = new Vector3(0f, Floor + TruckBuilder.WallHeight * .5f, 0f);
            trigger.size = new Vector3(BayWidth, TruckBuilder.WallHeight, BayLength);

            Wheelhouse(root, half, art);
            Hull(Group("HullArt", root), half, art);
            TruckBuilder.AddFloodlights(root, half);
            TruckBuilder.AddDisplay(root, half);

            var cargo = root.gameObject.AddComponent<TruckCargo>();
            root.gameObject.AddComponent<TruckSanctuary>().EditorSetup(art.Hull, TruckBuilder.TruckMirrorMaterial());
            cargo.EditorSetup(trigger, new Bounds(new Vector3(0f, 1.5f, 1f), new Vector3(BayWidth + .6f, 3.5f, BayLength + 3f)), ignition.transform);
            cargo.EditorSetBoat(true);
            foreach (Transform piece in root)
                if (piece.GetComponent<SurfaceTag>() == null) piece.gameObject.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Metal);

            // 2026 remake: the remodelled hull and wheelhouse over the code-built ones, when they have been made.
            Remake.VehicleRemake.DressBoat(root);
            Gangway(group, art);
            Moorings(group, art);
            return cargo;
        }

        // Forward of the hold: the wheelhouse (solid), windows, a mast with its lamp, the funnel.
        private static void Wheelhouse(Transform root, float half, IslandPrisonArt art)
        {
            float z0 = half + .1f, z1 = half + 3.2f, top = Floor + 2.6f;
            Box("Wheelhouse", root, new Vector3(0f, (Floor + top) * .5f, (z0 + z1) * .5f), new Vector3(BayWidth + .4f, top - Floor, z1 - z0), art.Facade);
            Box("WheelhouseRoof", root, new Vector3(0f, top + .08f, (z0 + z1) * .5f), new Vector3(BayWidth + .7f, .16f, z1 - z0 + .4f), art.Hull, false);
            foreach (float x in new[] { -.75f, .75f })
                Box("Window_F", root, new Vector3(x, Floor + 1.9f, z1 + .01f), new Vector3(.9f, .6f, .02f), art.Glass, false);
            foreach (float side in new[] { -1f, 1f })
                Box("Window_S", root, new Vector3(side * (BayWidth * .5f + .21f), Floor + 1.9f, (z0 + z1) * .5f), new Vector3(.02f, .6f, 2f), art.Glass, false);
            Box("Mast", root, new Vector3(0f, top + 2f, z1 - .4f), new Vector3(.12f, 4f, .12f), art.Steel, false);
            Box("MastLamp", root, new Vector3(0f, top + 4.05f, z1 - .4f), new Vector3(.2f, .2f, .2f), art.Ember, false);
            Box("Yard", root, new Vector3(0f, top + 3.2f, z1 - .4f), new Vector3(1.6f, .06f, .06f), art.Steel, false);
            Box("Funnel", root, new Vector3(0f, top + .7f, z0 + .7f), new Vector3(.6f, 1.2f, .6f), art.HullTrim, false);
            Box("Lifebelt", root, new Vector3(BayWidth * .5f + .22f, Floor + 1.1f, z0 + .6f), new Vector3(.06f, .55f, .55f), art.HullTrim, false);
        }

        // The hull below and round the hold: a slab-sided launch with a raked bow, red boot-top, rubbing strakes.
        private static void Hull(Transform g, float half, IslandPrisonArt art)
        {
            float stern = -half - .2f, mid = half + 3.4f;
            float top = Floor + Gunwale;
            Box("Side_L", g, new Vector3(-HullHalf, (Keel + top) * .5f, (stern + mid) * .5f), new Vector3(.12f, top - Keel, mid - stern), art.Hull, false);
            Box("Side_R", g, new Vector3(HullHalf, (Keel + top) * .5f, (stern + mid) * .5f), new Vector3(.12f, top - Keel, mid - stern), art.Hull, false);
            Box("Transom", g, new Vector3(0f, (Keel + Floor) * .5f, stern), new Vector3(HullHalf * 2f, Floor - Keel, .12f), art.Hull, false);
            Box("Bottom", g, new Vector3(0f, Keel, (stern + mid) * .5f), new Vector3(HullHalf * 2f, .12f, mid - stern), art.Hull, false);
            Box("BootTop_L", g, new Vector3(-HullHalf - .01f, -1.1f, (stern + mid) * .5f), new Vector3(.02f, .5f, mid - stern), art.HullTrim, false);
            Box("BootTop_R", g, new Vector3(HullHalf + .01f, -1.1f, (stern + mid) * .5f), new Vector3(.02f, .5f, mid - stern), art.HullTrim, false);
            Box("Strake_L", g, new Vector3(-HullHalf - .05f, top - .25f, (stern + mid) * .5f), new Vector3(.1f, .12f, mid - stern), art.DarkWood, false);
            Box("Strake_R", g, new Vector3(HullHalf + .05f, top - .25f, (stern + mid) * .5f), new Vector3(.1f, .12f, mid - stern), art.DarkWood, false);
            // The bow: two raked panels meeting at the stem, a foredeck, a winch and coiled line.
            float bowLen = Bow - mid;
            foreach (float side in new[] { -1f, 1f })
            {
                float yaw = Mathf.Atan2(HullHalf, bowLen) * Mathf.Rad2Deg;
                GameObject panel = Box(side < 0 ? "Bow_L" : "Bow_R", g, new Vector3(side * HullHalf * .5f, (Keel + top + .4f) * .5f, mid + bowLen * .5f),
                    new Vector3(.12f, top + .4f - Keel, Mathf.Sqrt(bowLen * bowLen + HullHalf * HullHalf)), art.Hull, false);
                panel.transform.localRotation = Quaternion.Euler(0f, -side * yaw, 0f);
            }
            Box("Foredeck", g, new Vector3(0f, top - .1f, mid + bowLen * .4f), new Vector3(HullHalf * 1.4f, .08f, bowLen * .8f), art.Plate, false);
            Box("Winch", g, new Vector3(0f, top + .25f, mid + 1.6f), new Vector3(.7f, .4f, .5f), art.Rust, false);
            Primitive(PrimitiveType.Cylinder, "Line", g, new Vector3(.6f, top + .06f, mid + 3f), new Vector3(.7f, .05f, .7f), art.Canvas, false);
            g.gameObject.AddComponent<IdleSway>().EditorSetup(.7f, 6.5f);
        }

        // From the quay's edge onto the hold's floor, railed both sides.
        private static void Gangway(Transform g, IslandPrisonArt art)
        {
            float x = L.BoatStern.x;
            GameObject plank = K.Span("Gangway", g, new Vector3(x - BayWidth * .5f, -.02f, L.BoatStern.z - .4f), new Vector3(x + BayWidth * .5f, .04f, .6f), art.Plate);
            plank.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Metal);
            K.Railing(g, "GangwayRail_L", new Vector3(x - BayWidth * .5f, .04f, L.BoatStern.z - .4f), new Vector3(x - BayWidth * .5f, .04f, .6f), .9f, art.Railing);
            K.Railing(g, "GangwayRail_R", new Vector3(x + BayWidth * .5f, .04f, L.BoatStern.z - .4f), new Vector3(x + BayWidth * .5f, .04f, .6f), .9f, art.Railing);
            // Water between hull and quay, under the plank: nobody falls through the gap beside it.
            K.Span("GapGuard_L", g, new Vector3(x - HullHalf - .6f, -1f, L.BoatStern.z - .4f), new Vector3(x - BayWidth * .5f, 0f, 0f), art.Hull);
            K.Span("GapGuard_R", g, new Vector3(x + BayWidth * .5f, -1f, L.BoatStern.z - .4f), new Vector3(x + HullHalf + .6f, 0f, 0f), art.Hull);
            foreach (Transform t in g.GetComponentsInChildren<Transform>())
                if (t.name.StartsWith("GapGuard") && t.GetComponent<MeshRenderer>() is MeshRenderer r) Object.DestroyImmediate(r);
        }

        // Ropes from the boat's quarters to the bollards, fenders over the side.
        private static void Moorings(Transform g, IslandPrisonArt art)
        {
            float x = L.BoatStern.x;
            foreach (float side in new[] { -1f, 1f })
            {
                Vector3 from = new(x + side * HullHalf, .9f, L.BoatStern.z - 1.2f), to = new(x + side * (HullHalf + 3.4f), .3f, .9f);
                Vector3 d = to - from;
                GameObject rope = Box("Mooring", g, (from + to) * .5f, new Vector3(.05f, .05f, d.magnitude), art.Canvas, false);
                rope.transform.rotation = Quaternion.LookRotation(d.normalized);
                Primitive(PrimitiveType.Cylinder, "Bollard", g, new Vector3(to.x, .35f, to.z), new Vector3(.4f, .35f, .4f), art.Rust, false);
                for (int i = 0; i < 3; i++)
                    Primitive(PrimitiveType.Cylinder, "Fender", g, new Vector3(x + side * (HullHalf + .15f), -.4f, L.BoatStern.z - 2.5f - i * 3f), new Vector3(.35f, .35f, .35f), art.Canvas, false);
            }
        }
    }
}
