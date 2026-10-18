using System;
using System.Linq;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// 0.24.11: the district's ground slab (one 1 km box collider 0.5 m under the mall's ground floor) ran through the
    /// basement stairwell and haul-ramp openings, so a player climbing out of the basement hit its underside and stuck
    /// at y -2.5. Its single collider becomes four around the basement's footprint; inside the building the mall's
    /// own floor tiles are the ground. The visible mesh, the NavMesh and everything outdoors are unchanged.
    /// </summary>
    public static class DistrictGroundHole
    {
        // The basement (MallLayout.Basement, tiles 10..13 x 6..11) in world metres.
        private static readonly Rect Hole = new(40f, 24f, 16f, 24f);

        /// <summary>Returns whether it changed anything (already cut: false).</summary>
        public static bool Apply(Transform mall)
        {
            Transform ground = mall.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == "DistrictGround");
            if (ground == null) throw new InvalidOperationException("The district ground slab is missing.");
            BoxCollider[] boxes = ground.GetComponents<BoxCollider>();
            if (boxes.Length != 1) return false;
            Bounds slab = boxes[0].bounds;
            UnityEngine.Object.DestroyImmediate(boxes[0]);
            Add(ground, slab, slab.min.x, Hole.xMin, slab.min.z, slab.max.z);
            Add(ground, slab, Hole.xMax, slab.max.x, slab.min.z, slab.max.z);
            Add(ground, slab, Hole.xMin, Hole.xMax, slab.min.z, Hole.yMin);
            Add(ground, slab, Hole.xMin, Hole.xMax, Hole.yMax, slab.max.z);
            return true;
        }

        // A world-space box of the slab between these x and z limits, as a collider in the slab's own (scaled) space.
        private static void Add(Transform ground, Bounds slab, float x0, float x1, float z0, float z1)
        {
            if (x1 - x0 < 0.01f || z1 - z0 < 0.01f) return;
            var world = new Bounds(new Vector3((x0 + x1) * 0.5f, slab.center.y, (z0 + z1) * 0.5f), new Vector3(x1 - x0, slab.size.y, z1 - z0));
            var box = ground.gameObject.AddComponent<BoxCollider>();
            Vector3 scale = ground.lossyScale;
            box.center = ground.InverseTransformPoint(world.center);
            box.size = new Vector3(world.size.x / scale.x, world.size.y / scale.y, world.size.z / scale.z);
        }
    }
}
