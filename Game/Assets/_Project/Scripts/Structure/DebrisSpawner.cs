using System.Collections.Generic;
using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Structure
{
    /// <summary>
    /// Turns a collapsed section into falling chunks: cosmetic, local only, on the Debris layer
    /// (never blocks players or loot, never deals damage), seeded so every machine starts the same
    /// break. They settle and stay as rubble until the building is restored (2026-10-08). A budget of
    /// moving chunks keeps big cascades affordable, and a resting budget keeps long jobs drawable.
    /// The solid part of the rubble is the deterministic <see cref="RubbleMound"/>, not these chunks.
    /// </summary>
    public static class DebrisSpawner
    {
        /// <summary>Chunks still tumbling (physics running).</summary>
        public static int LiveChunks { get; private set; }
        /// <summary>Chunks settled as rubble.</summary>
        public static int RestingChunks { get; private set; }

        private static readonly List<DebrisLifetime> Settled = new();
        private const float ChunkBoxScale = 0.8f;

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
                // A box round an irregular chunk overlaps its neighbours and the intact floor at the edge, enough to
                // wedge the whole break across the hole: keep it inside the chunk.
                if (!chunk.TryGetComponent(out Collider _)) chunk.AddComponent<BoxCollider>().size *= ChunkBoxScale;

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
            // A stair's bounds reach down to the floor its rubble lands on, so only flat floors get the hanging check.
            lifetime.Settle(section.Config.DebrisLifetime, visuals.MaxRestingChunks,
                section.Type == SectionType.Stair ? default : section.SurfaceBounds);
            return root;
        }

        internal static void Release(int chunks) => LiveChunks = Mathf.Max(0, LiveChunks - chunks);

        internal static void Rest(DebrisLifetime debris, int chunks)
        {
            RestingChunks += chunks;
            if (!Settled.Contains(debris)) Settled.Add(debris);
        }

        internal static void Unrest(DebrisLifetime debris, int chunks)
        {
            RestingChunks = Mathf.Max(0, RestingChunks - chunks);
            Settled.Remove(debris);
        }

        /// <summary>A section fell: rubble lying on it (within <paramref name="region"/>) falls with it.</summary>
        public static void Unsettle(Bounds region)
        {
            for (int i = Settled.Count - 1; i >= 0; i--)
                if (i < Settled.Count && Settled[i] != null) Settled[i].Unsettle(region);
        }

        private static float Next(System.Random random) => (float)random.NextDouble();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            LiveChunks = 0;
            RestingChunks = 0;
            Settled.Clear();
        }
    }
}
