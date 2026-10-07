using System.Collections.Generic;
using UnityEngine;

namespace Abandoned.Core
{
    /// <summary>
    /// Rope columns rigged over holes (M10.3): anything inside one comes down at a gentle speed instead of
    /// falling. Pulleys register here on every machine; the player motor (owner) and the pulley itself
    /// (host, for loot) read it, so neither depends on the gear code.
    /// </summary>
    public static class SafeDescent
    {
        private readonly struct Column
        {
            public readonly Vector3 Top;
            public readonly float Radius, Depth, Speed;

            public Column(Vector3 top, float radius, float depth, float speed)
            {
                Top = top;
                Radius = radius;
                Depth = depth;
                Speed = speed;
            }
        }

        private static readonly Dictionary<Object, Column> Columns = new();

        public static void Register(Object owner, Vector3 top, float radius, float depth, float speed) =>
            Columns[owner] = new Column(top, radius, depth, speed);

        public static void Unregister(Object owner) => Columns.Remove(owner);

        /// <summary>The slowest descent speed (m/s) of any column covering this point, or 0 when none does.</summary>
        public static float SpeedAt(Vector3 position)
        {
            float speed = 0f;
            foreach (KeyValuePair<Object, Column> pair in Columns)
            {
                if (pair.Key == null) continue;
                Column c = pair.Value;
                Vector3 d = position - c.Top;
                if (d.y > 0.5f || d.y < -c.Depth || new Vector2(d.x, d.z).sqrMagnitude > c.Radius * c.Radius) continue;
                speed = speed <= 0f ? c.Speed : Mathf.Min(speed, c.Speed);
            }
            return speed;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Columns.Clear();
    }
}
