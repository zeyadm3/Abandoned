using Abandoned.Core;
using Abandoned.Extraction;
using UnityEngine;
using UnityEngine.AI;
using static Abandoned.EditorTools.GreyboxFactory;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Hands out the island's hiding-place indices (HidingLocker allows 0-126 per level) and wires a spot up the way
    /// the mall's lockers are: a solid box the occupant stands inside, a door the locker moves, the point they step
    /// in to and the point they step out to (where a monster reaches in from). The builders model the look.
    /// </summary>
    public static class IslandHideSpots
    {
        public const int Max = 127;
        public static int Count { get; private set; }
        public static int Cells { get; private set; }
        public static int Carts { get; private set; }
        public static int Solitary { get; private set; }
        public static int Lockers { get; private set; }

        public static void Reset() { Count = Cells = Carts = Solitary = Lockers = 0; }

        public static bool Full => Count >= Max;

        /// <summary>Makes <paramref name="root"/> a hiding place. Root space: origin on the floor at the back, Z out of
        /// the door. <paramref name="size"/> is the solid box (w, h, d) from the back wall to the door.</summary>
        public static void Wire(Transform root, Transform door, Vector3 size, Vector3 inside, HideSpotKind kind, bool carve = true)
        {
            if (Full) return;
            var solid = root.gameObject.AddComponent<BoxCollider>();
            solid.center = new Vector3(0f, size.y * .5f, size.z * .5f);
            solid.size = size;
            if (carve)
            {
                var obstacle = root.gameObject.AddComponent<NavMeshObstacle>();
                obstacle.shape = NavMeshObstacleShape.Box;
                obstacle.center = solid.center;
                obstacle.size = solid.size;
                obstacle.carving = true;
            }
            Transform inPoint = Group("Inside", root);
            inPoint.localPosition = inside;
            Transform exit = Group("Exit", root);
            exit.localPosition = new Vector3(0f, .05f, size.z + .65f);
            root.gameObject.AddComponent<HidingLocker>().EditorSetup(Count++, door, inPoint, exit, null, kind);
            switch (kind)
            {
                case HideSpotKind.Cell: Cells++; break;
                case HideSpotKind.Cart: Carts++; break;
                case HideSpotKind.Solitary: Solitary++; break;
                default: Lockers++; break;
            }
            if (root.GetComponent<SurfaceTag>() == null) root.gameObject.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Metal);
        }
    }
}
