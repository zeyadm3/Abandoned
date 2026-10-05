using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Loot
{
    /// <summary>
    /// Cosmetic shards for a shattered item: local only, on the Debris layer (never blocks
    /// players or loot), removed after a few seconds.
    /// </summary>
    public static class ShatterEffect
    {
        private const int Pieces = 8;
        private const float Lifetime = 4f;
        private const float BurstSpeed = 2.5f;

        public static void Spawn(Vector3 center, Vector3 size, Material material)
        {
            int layer = GameLayers.DebrisLayer;
            float piece = Mathf.Max(0.03f, Mathf.Min(size.x, Mathf.Min(size.y, size.z)) * 0.35f);
            var random = new System.Random(center.GetHashCode());

            for (int i = 0; i < Pieces; i++)
            {
                GameObject shard = GameObject.CreatePrimitive(PrimitiveType.Cube);
                shard.name = "Shard";
                if (layer >= 0) shard.layer = layer;
                Vector3 offset = new((float)random.NextDouble() - 0.5f, (float)random.NextDouble() - 0.5f, (float)random.NextDouble() - 0.5f);
                shard.transform.SetPositionAndRotation(center + Vector3.Scale(offset, size),
                    Quaternion.Euler(random.Next(360), random.Next(360), random.Next(360)));
                shard.transform.localScale = Vector3.one * piece;
                if (material != null) shard.GetComponent<Renderer>().sharedMaterial = material;

                var body = shard.AddComponent<Rigidbody>();
                body.mass = 0.05f;
                body.linearVelocity = (offset.normalized + Vector3.up * 0.5f) * BurstSpeed;
                Object.Destroy(shard, Lifetime);
            }
        }
    }
}
