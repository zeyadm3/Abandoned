using UnityEngine;

namespace Abandoned.Extraction
{
    /// <summary>
    /// A doorway's geometry, pure: everything is in the door's own space, where X runs along the opening,
    /// Y is up and Z goes through it, with the origin on the floor in the middle of the opening.
    /// </summary>
    public static class DoorMath
    {
        /// <summary>
        /// Does the straight walk from <paramref name="from"/> to <paramref name="to"/> pass through the
        /// opening? Both ends must be on this storey and on opposite sides of the door.
        /// </summary>
        public static bool Crosses(Vector3 from, Vector3 to, float width, float height, float sideSlack = 0.3f)
        {
            if (from.z * to.z >= 0f) return false;
            if (from.y < -1f || from.y > height + 0.5f || to.y < -1.5f || to.y > height + 1.5f) return false;
            float t = from.z / (from.z - to.z);
            float along = Mathf.Lerp(from.x, to.x, t);
            return Mathf.Abs(along) <= width * 0.5f + sideSlack;
        }

        /// <summary>Is this point (a body's feet) standing in the door's slot, where the leaves close?</summary>
        public static bool InSlot(Vector3 feet, float width, float height, float halfDepth)
        {
            return Mathf.Abs(feet.x) <= width * 0.5f && Mathf.Abs(feet.z) <= halfDepth && feet.y > -1f && feet.y < height;
        }

        /// <summary>Flat distance from a point to the opening (0 when it is right in front of it).</summary>
        public static float FlatDistance(Vector3 point, float width)
        {
            float side = Mathf.Max(0f, Mathf.Abs(point.x) - width * 0.5f);
            return Mathf.Sqrt(side * side + point.z * point.z);
        }
    }
}
