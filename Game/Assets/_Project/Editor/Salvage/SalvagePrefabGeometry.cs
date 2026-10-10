using Abandoned.Interaction;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>Shared collider and socket recipes, in the caller's local bounds/pivot coordinates.</summary>
    internal static class SalvagePrefabGeometry
    {
        private static bool VisualOnly(string id) => id is "tire_track" or "broken_glass" or "cable_bundle"
            or "dust_sheet" or "orange_netting" or "salvage_tag";

        public static void AddColliders(Transform root, string id, Bounds bounds, bool dynamic,
            PhysicsMaterial friction = null)
        {
            if (!dynamic && VisualOnly(id)) return;
            var holder = new GameObject("_CollisionProxies");
            holder.transform.SetParent(root, false);
            Vector3 c = bounds.center, s = bounds.size;
            float floor = bounds.min.y;

            switch (id)
            {
                case "oak_conference_table":
                    Box(holder.transform, "Tabletop", new Vector3(c.x, floor + s.y * .925f, c.z),
                        new Vector3(s.x, s.y * .15f, s.z), friction);
                    for (int side = -1; side <= 1; side += 2)
                    {
                        Box(holder.transform, "Pedestal_" + side, new Vector3(c.x + side * s.x * .303f,
                            floor + s.y * .43f, c.z), new Vector3(s.x * .06f, s.y * .86f, s.z * .56f), friction);
                        Box(holder.transform, "Foot_" + side, new Vector3(c.x + side * s.x * .303f,
                            floor + s.y * .065f, c.z), new Vector3(s.x * .09f, s.y * .13f, s.z * .77f), friction);
                    }
                    break;
                case "ergonomic_chair":
                case "executive_chair":
                    Box(holder.transform, "Base", new Vector3(c.x, floor + s.y * .067f, c.z),
                        new Vector3(s.x * .90f, s.y * .13f, s.z * .87f), friction);
                    Box(holder.transform, "Column", new Vector3(c.x, floor + s.y * .27f, c.z),
                        new Vector3(s.x * .16f, s.y * .38f, s.z * .16f), friction);
                    Box(holder.transform, "Seat", new Vector3(c.x, floor + s.y * .43f, c.z - s.z * .045f),
                        new Vector3(s.x * .79f, s.y * .13f, s.z * .79f), friction);
                    Box(holder.transform, "Back", new Vector3(c.x, floor + s.y * .73f, c.z + s.z * .29f),
                        new Vector3(s.x * .76f, s.y * .49f, s.z * .16f), friction, new Vector3(8, 0, 0));
                    for (int side = -1; side <= 1; side += 2)
                        Box(holder.transform, "Arm_" + side, new Vector3(c.x + side * s.x * .44f,
                            floor + s.y * .61f, c.z), new Vector3(s.x * .12f, s.y * .06f, s.z * .43f), friction);
                    break;
                case "shipping_pallet":
                    Box(holder.transform, "Deck", new Vector3(c.x, floor + s.y * .86f, c.z),
                        new Vector3(s.x, s.y * .28f, s.z), friction);
                    for (int side = -1; side <= 1; side++)
                        Box(holder.transform, "Bearer_" + side, new Vector3(c.x + side * s.x * .39f,
                            floor + s.y * .38f, c.z), new Vector3(s.x * .14f, s.y * .76f, s.z), friction);
                    break;
                case "modular_shelving":
                    for (int x = -1; x <= 1; x += 2)
                    for (int z = -1; z <= 1; z += 2)
                        Box(holder.transform, $"Upright_{x}_{z}", new Vector3(c.x + x * s.x * .476f, c.y,
                            c.z + z * s.z * .45f), new Vector3(s.x * .048f, s.y, s.z * .10f), friction);
                    for (int shelf = 0; shelf < 4; shelf++)
                        Box(holder.transform, "Shelf_" + shelf, new Vector3(c.x,
                            floor + s.y * (.07f + shelf * .30f), c.z), new Vector3(s.x, s.y * .026f, s.z), friction);
                    break;
                case "garment_rack":
                    Box(holder.transform, "HangingRail", new Vector3(c.x, floor + s.y * .97f, c.z),
                        new Vector3(s.x, s.y * .06f, s.z * .08f), friction);
                    for (int side = -1; side <= 1; side += 2)
                    {
                        Box(holder.transform, "RackPost_" + side, new Vector3(c.x + side * s.x * .45f, c.y, c.z),
                            new Vector3(s.x * .04f, s.y, s.z * .10f), friction);
                        Box(holder.transform, "RackFoot_" + side, new Vector3(c.x + side * s.x * .45f,
                            floor + s.y * .045f, c.z), new Vector3(s.x * .13f, s.y * .09f, s.z), friction);
                    }
                    break;
                case "flatpack_desk":
                    Box(holder.transform, "Desktop", new Vector3(c.x, floor + s.y * .945f, c.z),
                        new Vector3(s.x, s.y * .11f, s.z), friction);
                    Box(holder.transform, "LeftSide", new Vector3(c.x - s.x * .405f, floor + s.y * .47f, c.z),
                        new Vector3(s.x * .04f, s.y * .94f, s.z * .84f), friction);
                    Box(holder.transform, "RightSupport", new Vector3(c.x + s.x * .366f,
                        floor + s.y * .47f, c.z + s.z * .33f), new Vector3(s.x * .055f, s.y * .94f, s.z * .08f), friction);
                    Box(holder.transform, "Drawer", new Vector3(c.x - s.x * .23f, floor + s.y * .78f,
                        c.z - s.z * .20f), new Vector3(s.x * .30f, s.y * .25f, s.z * .56f), friction);
                    Box(holder.transform, "LoosePanel", new Vector3(c.x + s.x * .10f, floor + s.y * .035f,
                        c.z), new Vector3(s.x * .56f, s.y * .07f, s.z * .83f), friction);
                    break;
                case "heavy_dolly":
                    Box(holder.transform, "DollyFrame", new Vector3(c.x, floor + s.y * .53f, c.z + s.z * .24f),
                        new Vector3(s.x * .75f, s.y * .94f, s.z * .13f), friction);
                    Box(holder.transform, "Toe", new Vector3(c.x, floor + s.y * .063f, c.z - s.z * .16f),
                        new Vector3(s.x, s.y * .07f, s.z * .64f), friction);
                    Box(holder.transform, "Wheels", new Vector3(c.x, floor + s.y * .12f, c.z + s.z * .24f),
                        new Vector3(s.x, s.y * .23f, s.z * .57f), friction);
                    break;
                case "demolition_fence":
                    Box(holder.transform, "FencePanel", new Vector3(c.x, floor + s.y * .55f, c.z),
                        new Vector3(s.x * .94f, s.y * .90f, .07f), friction);
                    for (int side = -1; side <= 1; side += 2)
                        Box(holder.transform, "FenceFoot_" + side, new Vector3(c.x + side * s.x * .46f,
                            floor + .04f, c.z), new Vector3(.16f, .08f, s.z), friction);
                    break;
                case "exposed_partition":
                    for (int stud = 0; stud < 5; stud++)
                        Box(holder.transform, "Stud_" + stud, new Vector3(c.x + s.x * (-.48f + stud * .24f), c.y, c.z),
                            new Vector3(.055f, s.y, .10f), friction);
                    Box(holder.transform, "RemainingBoard", new Vector3(c.x - s.x * .365f, c.y, c.z + .055f),
                        new Vector3(s.x * .225f, s.y * .98f, .04f), friction);
                    Box(holder.transform, "UpperBoard", new Vector3(c.x - s.x * .07f, floor + s.y * .88f,
                        c.z + .055f), new Vector3(s.x * .37f, s.y * .20f, .04f), friction);
                    Box(holder.transform, "LowerBoard", new Vector3(c.x + s.x * .17f, floor + s.y * .10f,
                        c.z + .055f), new Vector3(s.x * .58f, s.y * .20f, .04f), friction);
                    break;
                case "work_floodlight":
                    Box(holder.transform, "Mast", new Vector3(c.x, floor + s.y * .47f, c.z),
                        new Vector3(.08f, s.y * .89f, .08f), friction);
                    Box(holder.transform, "LampHeads", new Vector3(c.x, floor + s.y * .94f, c.z),
                        new Vector3(s.x * .90f, s.y * .12f, .14f), friction);
                    break;
                case "fabric_roll":
                case "shrinkwrap_roll":
                    Capsule(holder.transform, "Roll", bounds, friction);
                    break;
                case "mannequin":
                    Box(holder.transform, "DisplayBody", new Vector3(c.x, floor + s.y * .56f, c.z),
                        new Vector3(s.x * .68f, s.y * .88f, s.z * .60f), friction);
                    Box(holder.transform, "DisplayBase", new Vector3(c.x, floor + s.y * .04f, c.z),
                        new Vector3(s.x, s.y * .08f, s.z), friction);
                    break;
                case "pallet_jack":
                    for (int side = -1; side <= 1; side += 2)
                        Box(holder.transform, "Fork_" + side, new Vector3(c.x + side * s.x * .37f,
                            floor + s.y * .095f, c.z + s.z * .14f), new Vector3(s.x * .27f, s.y * .11f, s.z * .71f), friction);
                    Box(holder.transform, "Pump", new Vector3(c.x, floor + s.y * .18f, c.z - s.z * .24f),
                        new Vector3(s.x, s.y * .36f, s.z * .22f), friction);
                    Box(holder.transform, "Tiller", new Vector3(c.x, floor + s.y * .65f, c.z - s.z * .38f),
                        new Vector3(s.x * .15f, s.y * .70f, s.z * .15f), friction);
                    break;
                default:
                    Vector3 size = bounds.size;
                    size.y = Mathf.Max(size.y, dynamic ? .025f : .015f);
                    Box(holder.transform, "Body", bounds.center, size, friction);
                    break;
            }
        }

        private static void Box(Transform parent, string name, Vector3 centre, Vector3 size,
            PhysicsMaterial material, Vector3? euler = null)
        {
            var proxy = new GameObject(name);
            proxy.transform.SetParent(parent, false);
            proxy.transform.localPosition = centre;
            proxy.transform.localRotation = Quaternion.Euler(euler ?? Vector3.zero);
            var collider = proxy.AddComponent<BoxCollider>();
            collider.size = new Vector3(Mathf.Max(size.x, .015f), Mathf.Max(size.y, .015f), Mathf.Max(size.z, .015f));
            collider.sharedMaterial = material;
        }

        private static void Capsule(Transform parent, string name, Bounds bounds, PhysicsMaterial material)
        {
            var proxy = new GameObject(name);
            proxy.transform.SetParent(parent, false);
            proxy.transform.localPosition = bounds.center;
            var collider = proxy.AddComponent<CapsuleCollider>();
            Vector3 s = bounds.size;
            int axis = s.x >= s.y && s.x >= s.z ? 0 : s.y >= s.z ? 1 : 2;
            collider.direction = axis;
            collider.radius = Mathf.Max(.015f, (axis == 0 ? Mathf.Max(s.y, s.z)
                : axis == 1 ? Mathf.Max(s.x, s.z) : Mathf.Max(s.x, s.y)) * .5f);
            collider.height = Mathf.Max(s[axis], collider.radius * 2);
            collider.sharedMaterial = material;
        }

        public static Vector3[] GripPoints(string id, Bounds bounds)
        {
            var points = CarryPointLayout.Generate(bounds.size, 4).ToArray();
            float aboveFloor = Mathf.Min(bounds.size.y * .65f, .80f);
            if (id is "oak_conference_table" or "flatpack_desk") aboveFloor = bounds.size.y * .84f;
            float y = bounds.min.y + aboveFloor;
            for (int i = 0; i < points.Length; i++)
                points[i] = bounds.center + new Vector3(points[i].x, y - bounds.center.y, points[i].z);
            return points;
        }

        public static void AuthorSockets(Transform root, string id, Bounds bounds, bool nested)
        {
            Transform parent = root;
            if (nested)
            {
                var sockets = new GameObject("Sockets");
                sockets.transform.SetParent(root, false);
                parent = sockets.transform;
            }
            Vector3 front = id is "cargo_crate" or "plastic_sorting_crate" or "generator_hub" or "salvage_tag"
                ? Vector3.forward : Vector3.back;
            Vector3 interaction = bounds.center + front * (bounds.extents.z + .05f);
            interaction.y = bounds.min.y + Mathf.Min(bounds.size.y * .60f, 1.20f);
            Socket(parent, "InteractionAnchor", interaction, Quaternion.LookRotation(front));
            Socket(parent, "OutlineAnchor", bounds.center, Quaternion.identity);
            Vector3[] points = GripPoints(id, bounds);
            for (int i = 0; i < points.Length; i++)
                Socket(parent, "CarryGrip_" + (char)('A' + i), points[i], Quaternion.identity);
            Vector3 label = bounds.center + front * (bounds.extents.z + .003f);
            label.y = bounds.min.y + bounds.size.y * .68f;
            Socket(parent, "LabelAnchor", label, Quaternion.LookRotation(front));
        }

        private static void Socket(Transform parent, string name, Vector3 at, Quaternion rotation)
        {
            Transform socket = parent.Find(name);
            if (socket == null)
            {
                var created = new GameObject(name);
                created.transform.SetParent(parent, false);
                socket = created.transform;
            }
            socket.SetLocalPositionAndRotation(at, rotation);
            socket.localScale = Vector3.one;
        }
    }
}
