using Abandoned.Core;
using Abandoned.Extraction;
using UnityEngine;
using UnityEngine.AI;
using static Abandoned.EditorTools.GreyboxFactory;
using static Abandoned.EditorTools.MallLayout;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The mall's sliding doors (0.16.0). They stand only in openings the building already has, so no wall is
    /// cut and the baked NavMesh and occlusion stay as they are: a shut door carves its doorway at run time,
    /// like a shutter.
    ///   Store doors: a pair of panels in each store's four-metre doorway, on the store's side of the shutter.
    ///   Service doors: one steel panel across the mouth of the service corridor on each floor.
    /// </summary>
    public static class MallDoorBuilder
    {
        // On the store's side of the wall: clear of the fascia's back (0.12 m), the shutter housing (0.14 m)
        // and the posters behind the glazing (0.17 m).
        private const float StoreTrack = 0.24f;
        // Up to the underside of the fascia (3.03 m).
        private const float StoreLeafHeight = 2.98f, ServiceLeafHeight = 2.5f, LeafThickness = 0.05f, Lift = 0.02f;
        // The service corridor's mouth (world): just outside the stair rail, from the tool store's wall corner.
        private const float ServicePlane = 43.82f, ServiceSouth = 32.12f;

        public static int Build(Transform parent)
        {
            int index = 0;
            for (int floor = 0; floor < Floors; floor++)
            foreach (Door door in Doors)
            {
                if (door.Floor != floor || !door.Wide) continue;
                Zone? a = ZoneAt(door.A, floor), b = ZoneAt(door.B, floor);
                if (!a.HasValue || !b.HasValue) continue;
                bool storeIsA = a.Value.Kind == Kind.Store;
                if (storeIsA == (b.Value.Kind == Kind.Store)) continue; // not a storefront
                Zone store = storeIsA ? a.Value : b.Value;
                Vector3 inside = TileTopCenter(storeIsA ? door.A : door.B, floor);
                Vector3 outside = TileTopCenter(storeIsA ? door.B : door.A, floor);
                StoreDoor(parent, $"StoreDoor_{floor}_{store.Name}", index++, (inside + outside) * 0.5f, (inside - outside).normalized, store.Tag);
            }
            for (int floor = 0; floor < Floors; floor++) ServiceDoor(parent, $"ServiceDoor_{floor}", index++, floor);
            return index;
        }

        private static void StoreDoor(Transform parent, string name, int index, Vector3 at, Vector3 intoStore, string tag)
        {
            const float width = MallWalls.WideWidth, leafWidth = width * 0.5f;
            Transform root = Group(name, parent);
            // Z points into the store, X runs along the storefront.
            root.SetPositionAndRotation(at, Quaternion.LookRotation(intoStore));
            Material panel = MeridianRetailMaterials.Accent(tag), frame = MeridianRetailMaterials.Brass, trim = MeridianRetailMaterials.Dark;

            // The track the panels hang from, long enough to carry both clear of the opening.
            Transform fixedParts = Group("Track", root);
            Box("Rail", fixedParts, new Vector3(0f, Lift + StoreLeafHeight + 0.05f, StoreTrack), new Vector3(width * 2f, 0.1f, 0.09f), trim, false);
            MallRoomsMeshes.Merge(fixedParts, "StoreTrack");

            var leaves = new Transform[2];
            var shut = new Vector3[2];
            var open = new Vector3[2];
            for (int i = 0; i < 2; i++)
            {
                float side = i == 0 ? -1f : 1f;
                // The pull is on the edge that meets the other panel.
                leaves[i] = Leaf(root, i == 0 ? "Leaf_L" : "Leaf_R", leafWidth, StoreLeafHeight, panel, frame, trim, frame, -side, $"StoreLeaf{(i == 0 ? "L" : "R")}");
                shut[i] = new Vector3(side * leafWidth * 0.5f, Lift, StoreTrack);
                open[i] = new Vector3(side * leafWidth * 1.5f, Lift, StoreTrack);
            }
            Finish(root, index, DoorKind.Store, leaves, shut, open, width, StoreLeafHeight, StoreTrack);
        }

        private static void ServiceDoor(Transform parent, string name, int index, int floor)
        {
            // Ground: up to where the stair rail starts. Upstairs the rail is below the floor: up to the floor's edge.
            float north = floor == 0 ? 35.56f : 36.04f;
            float width = north - ServiceSouth, travel = width + 0.04f;
            Transform root = Group(name, parent);
            // X runs north along the mouth, so the panel slides away beside the stairs; Z faces the promenade.
            root.SetPositionAndRotation(new Vector3(ServicePlane, FloorY(floor), (ServiceSouth + north) * 0.5f), Quaternion.LookRotation(Vector3.left));
            Material steel = SalvageMaterials.Get("Metal"), trim = SalvageMaterials.Get("Rubber"), warning = SalvageMaterials.Get("Safety");

            Transform fixedParts = Group("Track", root);
            float top = Lift + ServiceLeafHeight + 0.05f;
            Box("Rail", fixedParts, new Vector3(travel * 0.5f, top, 0f), new Vector3(width + travel + 0.1f, 0.1f, 0.1f), trim, false);
            // Posts carry the rail where there is no wall to fix it to: at the stop and at its far end.
            foreach (float x in new[] { width * 0.5f + 0.02f, width * 0.5f + travel + 0.02f })
                Box("Post", fixedParts, new Vector3(x, top * 0.5f, 0.1f), new Vector3(0.07f, top, 0.07f), trim, false);
            MallRoomsMeshes.Merge(fixedParts, $"ServiceTrack{floor}");

            // The pull is on the trailing edge: at the wall when shut, at the mouth's far jamb when open.
            Transform leaf = Leaf(root, "Leaf", width, ServiceLeafHeight, steel, trim, trim, warning, -1f, $"ServiceLeaf{floor}", warning);
            Finish(root, index, DoorKind.Service, new[] { leaf }, new[] { new Vector3(0f, Lift, 0f) }, new[] { new Vector3(travel, Lift, 0f) },
                width, ServiceLeafHeight, 0f);
        }

        // One sliding panel, its origin at the bottom middle. pullSide: the edge (-1 or +1 along X) that carries the pull.
        private static Transform Leaf(Transform root, string name, float width, float height, Material panel, Material frame, Material trim,
            Material pull, float pullSide, string meshKey, Material band = null)
        {
            const float stile = 0.07f, rail = 0.09f;
            Transform leaf = Group(name, root);
            float inner = width - stile * 2f;
            Box("Panel", leaf, new Vector3(0f, height * 0.5f, 0f), new Vector3(inner, height - rail * 2f, 0.03f), panel, false);
            foreach (float side in new[] { -1f, 1f })
                Box("Stile", leaf, new Vector3(side * (width - stile) * 0.5f, height * 0.5f, 0f), new Vector3(stile, height, LeafThickness), frame, false);
            Box("TopRail", leaf, new Vector3(0f, height - rail * 0.5f, 0f), new Vector3(inner, rail, LeafThickness), frame, false);
            Box("BottomRail", leaf, new Vector3(0f, rail * 0.5f, 0f), new Vector3(inner, rail, LeafThickness), frame, false);
            Box("MidRail", leaf, new Vector3(0f, 1.05f, 0f), new Vector3(inner, 0.07f, LeafThickness), frame, false);
            Box("KickPlate", leaf, new Vector3(0f, 0.27f, 0f), new Vector3(inner, 0.36f, 0.04f), trim, false);
            // The pull stands a little proud of that edge. With the door open the panel lies behind the wall
            // beside the opening (behind glass, from the concourse): the pull is what shows in the opening,
            // so the door can be slid shut from either side of the wall.
            float pullX = pullSide * (width * 0.5f + 0.045f);
            Box("Pull", leaf, new Vector3(pullX, 1.4f, 0f), new Vector3(0.035f, 0.6f, 0.13f), pull, false);
            if (band != null) Box("HazardBand", leaf, new Vector3(0f, 1.72f, 0f), new Vector3(inner, 0.16f, 0.036f), band, false);
            MallRoomsMeshes.Merge(leaf, meshKey);

            // Solid where it stands: shut it fills the doorway, open it lies against the wall beside it.
            var solid = leaf.gameObject.AddComponent<BoxCollider>();
            solid.center = new Vector3(0f, height * 0.5f, 0f);
            solid.size = new Vector3(width, height, 0.06f);
            var grip = leaf.gameObject.AddComponent<BoxCollider>();
            grip.center = new Vector3(pullX, 1.4f, 0f);
            grip.size = new Vector3(0.09f, 0.6f, 0.16f);
            leaf.gameObject.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Metal);
            return leaf;
        }

        private static void Finish(Transform root, int index, DoorKind kind, Transform[] leaves, Vector3[] shut, Vector3[] open,
            float width, float height, float track)
        {
            if (index >= 64) throw new System.InvalidOperationException("More than 64 doors.");
            // Thicker than the panels: a thrown item or a line of sight never slips through a shut door.
            var blocker = root.gameObject.AddComponent<BoxCollider>();
            blocker.center = new Vector3(0f, Lift + height * 0.5f, track);
            blocker.size = new Vector3(width, height, 0.3f);
            var obstacle = root.gameObject.AddComponent<NavMeshObstacle>();
            obstacle.shape = NavMeshObstacleShape.Box;
            obstacle.center = new Vector3(0f, height * 0.5f, track * 0.5f);
            obstacle.size = new Vector3(width + 0.2f, height, 0.6f);
            obstacle.carving = true;
            root.gameObject.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Metal);
            root.gameObject.AddComponent<SlidingDoor>().EditorSetup(index, kind, leaves, shut, open, blocker, obstacle, width, height);
            // Saved open, like the shutters: each run shuts the ones it rolls.
            for (int i = 0; i < leaves.Length; i++) leaves[i].localPosition = open[i];
            blocker.enabled = false;
            obstacle.enabled = false;
        }
    }
}
