using System.Collections.Generic;
using UnityEngine;

namespace Abandoned.Interaction
{
    /// <summary>
    /// Generates carry points from an item's local bounding size, so a new Heavy/Huge item needs no
    /// hand-placed handles: the two ends of its longest horizontal axis first (where two people would
    /// hold a rack or a sofa), then the middles of the two long sides. Points sit on the faces at the
    /// item's centre height (loot pivots are at the centre).
    /// </summary>
    public static class CarryPointLayout
    {
        public static List<Vector3> Generate(Vector3 size, int count)
        {
            var points = new List<Vector3>(count);
            bool longIsZ = size.z >= size.x;
            float halfLong = (longIsZ ? size.z : size.x) * 0.5f;
            float halfShort = (longIsZ ? size.x : size.z) * 0.5f;
            Vector3 along = longIsZ ? Vector3.forward : Vector3.right;
            Vector3 across = longIsZ ? Vector3.right : Vector3.forward;
            Vector3[] candidates =
            {
                along * halfLong,
                -along * halfLong,
                across * halfShort,
                -across * halfShort,
            };
            for (int i = 0; i < Mathf.Clamp(count, 1, candidates.Length); i++) points.Add(candidates[i]);
            return points;
        }
    }
}
