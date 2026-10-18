using UnityEngine;
using UnityEngine.AI;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// 0.24.11: the School, Factory and Prison were built without <see cref="SolidObjectsPass"/>, so players walked
    /// through their lockers, desks, bookcases, bleachers and fence posts. Their saved scenes are made solid here,
    /// without a rebake: a box is left out (walk-through, reported) wherever the baked NavMesh runs through it, since
    /// something may route there.
    /// </summary>
    public static class LevelSolidity
    {
        public static int Apply(Transform level, string levelName) => SolidObjectsPass.Apply(level, levelName, NavMeshThrough);

        // The NavMesh under the box's middle third means the bake let walkers through: a box there could cut a route.
        private static string NavMeshThrough(Bounds box)
        {
            Vector3 c = new(box.center.x, box.min.y, box.center.z);
            float reach = Mathf.Max(0.15f, Mathf.Min(box.extents.x, box.extents.z) * 0.66f);
            return NavMesh.SamplePosition(c, out NavMeshHit hit, reach, NavMesh.AllAreas) && Mathf.Abs(hit.position.y - c.y) < 0.6f
                ? "the baked NavMesh runs through it" : null;
        }
    }
}
