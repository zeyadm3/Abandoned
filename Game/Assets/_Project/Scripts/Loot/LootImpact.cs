using UnityEngine;

namespace Abandoned.Loot
{
    /// <summary>
    /// One collision a loot item's own physics saw: speed along the normal, where, the normal (pointing
    /// from the other body toward this item), and the other rigidbody if any. Networking needs the other
    /// body: when it's a kinematic copy another machine simulates, the hit only happened here.
    /// </summary>
    public readonly struct LootImpact
    {
        public readonly float Speed;
        public readonly Vector3 Point;
        public readonly Vector3 Normal;
        public readonly Rigidbody Other;

        public LootImpact(float speed, Vector3 point, Vector3 normal, Rigidbody other)
        {
            Speed = speed;
            Point = point;
            Normal = normal;
            Other = other;
        }
    }
}
