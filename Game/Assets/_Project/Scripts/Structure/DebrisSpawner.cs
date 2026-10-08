using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Structure
{
    /// <summary>
    /// Turns a collapsed section into falling chunks: cosmetic, local only, on the Debris layer
    /// (never blocks players or loot, never deals damage), seeded so every machine starts the same
    /// break, and removed after a few seconds. A global chunk budget keeps big cascades affordable.
    /// </summary>
    public static class DebrisSpawner
    {
        public static int LiveChunks { get; private set; }

        public static GameObject Spawn(StructuralSection section, GameObject fracturedPrefab, Material material,
            StructureVisualConfig visuals, int seed, bool preserveMaterials = false)
        {
            int budget = Mathf.Max(0, visuals.MaxLiveChunks - LiveChunks);
            if (budget == 0) return null;
            Transform visual = section.Visual;
            GameObject root = fracturedPrefab != null
                ? Object.Instantiate(fracturedPrefab, visual.position, visual.rotation)
                : Object.Instantiate(visual.gameObject, visual.position, visual.rotation);
            if (fracturedPrefab != null) root.transform.localScale = Vector3.one;
            root.name = $"Debris_{section.name}";

            int layer = GameLayers.DebrisLayer;
            // Every collider below the fragment is cosmetic, including imported nested mesh children.
            if (layer >= 0) foreach (Transform child in root.GetComponentsInChildren<Transform>(true)) child.gameObject.layer = layer;
            var random = new System.Random(seed);
            var lifetime = root.AddComponent<DebrisLifetime>();
            int used = 0;
            foreach (Renderer r in root.GetComponentsInChildren<Renderer>(true))
            {
                GameObject chunk = r.gameObject;
                if (used >= budget)
                {
                    Object.Destroy(chunk);
                    continue;
                }
                if (layer >= 0) chunk.layer = layer;
                r.enabled = true;
                if (material != null && !preserveMaterials) r.sharedMaterial = material;
                if (!chunk.TryGetComponent(out Collider _)) chunk.AddComponent<BoxCollider>();

                Rigidbody body = chunk.TryGetComponent(out Rigidbody existing) ? existing : chunk.AddComponent<Rigidbody>();
                body.isKinematic = false;
                body.useGravity = true;
                body.mass = visuals.ChunkMass;
                Vector3 offset = chunk.transform.position - root.transform.position;
                Vector3 outward = new Vector3(offset.x, 0f, offset.z).normalized;
                body.linearVelocity = outward * (visuals.BurstSpeed * Next(random)) + Vector3.down * Next(random);
                body.angularVelocity = new Vector3(Next(random) * 2f - 1f, Next(random) * 2f - 1f, Next(random) * 2f - 1f) * visuals.BurstSpin;
                lifetime.RecordLaunch(body.linearVelocity);
                used++;
            }

            LiveChunks += used;
            lifetime.Track(used);
            Object.Destroy(root, section.Config.DebrisLifetime);
            return root;
        }

        internal static void Release(int chunks) => LiveChunks = Mathf.Max(0, LiveChunks - chunks);

        private static float Next(System.Random random) => (float)random.NextDouble();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => LiveChunks = 0;
    }
}
