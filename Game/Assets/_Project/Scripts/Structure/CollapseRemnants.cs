using System.Collections.Generic;
using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Structure
{
    /// <summary>
    /// What a broken floor leaves behind at the edges still held by the floor or wall beside it: a jagged lip of the
    /// broken slab hinged down into the hole, and the bars that held it, torn out and bent down. Both drop into place
    /// over a moment as the floor goes. The lip is solid (user, 2026-10-22: you walked through it): one box per piece on
    /// the Default layer, built from the collapse seed so every machine has the same edge, never networked, never
    /// damaging (the rubble mound's rule). The bars are thin and stay visual only. A lip goes when the floor holding it
    /// falls; all of it goes when the building is restored.
    /// </summary>
    public class CollapseRemnants : MonoBehaviour
    {
        private sealed class Bar
        {
            public Transform Root, Elbow;
            public Quaternion Rest;
            public float Droop, Bend;
        }

        private sealed class Side
        {
            public GameObject Root;
            public Transform Hinge;
            public Vector3 Axis;
            public float Tilt;
            public StructuralSection Holder;
            public readonly List<Bar> Bars = new();
        }

        // Default layer: players, loot and monsters collide with it (Debris ignores players and loot).
        private const int SolidLayer = 0;
        // A box round an irregular piece is a little larger than it; this keeps the edge you stand on close to what you see.
        private const float LipBoxScale = 0.9f;

        private static Mesh prism;
        private static Material rust;

        private readonly Side[] sides = new Side[4];
        private StructuralSection section;
        private float started, duration;

        /// <param name="pieces">The break's chunks chosen to stay, each with the held side it hangs from.</param>
        public static CollapseRemnants Create(StructuralSection section, CollapseEdges edges, IReadOnlyList<(GameObject chunk, int side)> pieces,
            StructureVisualConfig visuals, int seed)
        {
            var go = new GameObject($"Remnants_{section.name}");
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(go, section.gameObject.scene);
            var remnants = go.AddComponent<CollapseRemnants>();
            remnants.section = section;
            remnants.started = Time.time;
            remnants.duration = visuals.RemnantDrop;
            var random = new System.Random(seed ^ 0x6eba7);
            float Next() => (float)random.NextDouble();
            int layer = GameLayers.DebrisLayer;
            Bounds hole = edges.Hole;
            float slab = Mathf.Clamp(hole.size.y, 0.1f, 0.4f);
            Material steel = visuals.RebarMaterial != null ? visuals.RebarMaterial : Rust();

            for (int i = 0; i < 4; i++)
            {
                if (!edges.Held[i]) continue;
                Vector3 o = CollapseEdges.Outward[i];
                var side = new Side { Root = new GameObject($"Edge_{i}"), Holder = edges.Neighbour[i] };
                side.Root.layer = Mathf.Max(0, layer);
                side.Root.transform.SetParent(go.transform, false);
                side.Root.transform.position = new Vector3(hole.center.x + o.x * hole.extents.x, hole.max.y, hole.center.z + o.z * hole.extents.z);
                side.Hinge = new GameObject("Lip").transform;
                side.Hinge.SetParent(side.Root.transform, false);
                side.Axis = Vector3.Cross(o, Vector3.up); // a positive turn about it drops the side towards the hole
                side.Tilt = Mathf.Lerp(visuals.RemnantTilt.x, visuals.RemnantTilt.y, Next());
                remnants.sides[i] = side;
                Bars(side, o, hole, slab, visuals, steel, random, layer);
                if (side.Holder != null) side.Holder.Collapsed += remnants.OnHolderCollapsed;
            }

            foreach ((GameObject chunk, int s) in pieces)
            {
                Side side = remnants.sides[s];
                if (side == null || chunk == null) continue;
                chunk.transform.SetParent(side.Hinge, true);
                // No two lips break the same: each piece a little twisted and dropped about its own middle.
                Vector3 twist = new(Next() * 10f - 5f, Next() * 8f - 4f, Next() * 10f - 5f);
                chunk.transform.rotation = Quaternion.Euler(twist) * chunk.transform.rotation;
                chunk.transform.position += Vector3.down * Next() * 0.03f;
                // A lip is solid: you can stand on it, bump into it, and loot can land on it. Same box on every machine,
                // built from the seed, like the rubble mound.
                foreach (Collider c in chunk.GetComponentsInChildren<Collider>(true)) Destroy(c);
                foreach (Transform t in chunk.GetComponentsInChildren<Transform>(true)) t.gameObject.layer = SolidLayer;
                if (chunk.TryGetComponent(out MeshFilter filter) && filter.sharedMesh != null)
                {
                    var box = chunk.AddComponent<BoxCollider>();
                    box.center = filter.sharedMesh.bounds.center;
                    box.size = filter.sharedMesh.bounds.size * LipBoxScale;
                }
            }
            // The lips' colliders swing down with their hinge: a kinematic body moves them properly.
            foreach (Side side in remnants.sides)
                if (side != null) side.Hinge.gameObject.AddComponent<Rigidbody>().isKinematic = true;
            section.Restored += remnants.OnSectionRestored;
            remnants.Pose(0f);
            return remnants;
        }

        private static void Bars(Side side, Vector3 o, Bounds hole, float slab, StructureVisualConfig visuals, Material steel, System.Random random, int layer)
        {
            float Next() => (float)random.NextDouble();
            Vector3 along = o.x != 0f ? Vector3.forward : Vector3.right;
            float half = (o.x != 0f ? hole.extents.z : hole.extents.x) - 0.12f;
            int row = 0;
            for (float s = -half; s <= half; s += visuals.RebarSpacing, row++)
            {
                // Two layers of bars, top and bottom of the slab; the bars start a little inside the floor still standing.
                float depth = row % 2 == 0 ? 0.3f : 0.7f;
                Vector3 at = side.Root.transform.position + along * (s + (Next() - 0.5f) * 0.08f) + o * 0.03f + Vector3.down * slab * depth;
                var bar = new Bar
                {
                    Root = new GameObject("Bar").transform,
                    Droop = Mathf.Lerp(8f, 38f, Next()),
                    Bend = Mathf.Lerp(20f, 75f, Next()),
                };
                bar.Root.SetParent(side.Root.transform, true);
                bar.Root.position = at;
                bar.Rest = Quaternion.LookRotation(-o, Vector3.up) * Quaternion.Euler(0f, Next() * 24f - 12f, 0f);
                bar.Root.rotation = bar.Rest;
                float first = Mathf.Lerp(visuals.RebarLength.x, visuals.RebarLength.y, Next());
                Steel(bar.Root, first, visuals.RebarRadius, steel, layer);
                // One in five snapped off short; the rest go on past a bend.
                if (Next() > 0.2f)
                {
                    bar.Elbow = new GameObject("Bend").transform;
                    bar.Elbow.SetParent(bar.Root, false);
                    bar.Elbow.localPosition = new Vector3(0f, 0f, first);
                    Steel(bar.Elbow, Mathf.Lerp(visuals.RebarLength.x, visuals.RebarLength.y, Next()), visuals.RebarRadius, steel, layer);
                }
                side.Bars.Add(bar);
            }
        }

        private static void Steel(Transform parent, float length, float radius, Material material, int layer)
        {
            var go = new GameObject("Steel", typeof(MeshFilter), typeof(MeshRenderer));
            go.layer = Mathf.Max(0, layer);
            go.transform.SetParent(parent, false);
            go.transform.localScale = new Vector3(radius * 2f, radius * 2f, length);
            go.GetComponent<MeshFilter>().sharedMesh = Prism();
            go.GetComponent<MeshRenderer>().sharedMaterial = material;
        }

        private void Update()
        {
            float t = duration > 0f ? (Time.time - started) / duration : 1f;
            Pose(Mathf.Clamp01(t));
            if (t >= 1f) enabled = false;
        }

        // 0 = level, as the floor was; 1 = hung. A small overshoot, as a lip snaps down and bounces on its bars.
        private void Pose(float t)
        {
            float k = t >= 1f ? 1f : 1f + 2.2f * Mathf.Pow(t - 1f, 3f) + 1.2f * Mathf.Pow(t - 1f, 2f);
            foreach (Side side in sides)
            {
                if (side == null || side.Root == null) continue;
                side.Hinge.localRotation = Quaternion.AngleAxis(side.Tilt * k, side.Axis);
                foreach (Bar bar in side.Bars)
                {
                    bar.Root.rotation = bar.Rest * Quaternion.Euler(bar.Droop * k, 0f, 0f);
                    if (bar.Elbow != null) bar.Elbow.localRotation = Quaternion.Euler(bar.Bend * k, 0f, 0f);
                }
            }
        }

        // The floor holding a lip fell too: the lip and its bars go with it (that floor's own break covers it).
        private void OnHolderCollapsed(StructuralSection holder)
        {
            for (int i = 0; i < 4; i++)
            {
                if (sides[i] == null || sides[i].Holder != holder) continue;
                if (sides[i].Root != null) Destroy(sides[i].Root);
                sides[i] = null;
            }
        }

        private void OnSectionRestored(StructuralSection s) => Destroy(gameObject);

        private void OnDestroy()
        {
            if (section != null) section.Restored -= OnSectionRestored;
            foreach (Side side in sides)
                if (side?.Holder != null) side.Holder.Collapsed -= OnHolderCollapsed;
        }

        // A six-sided bar 1 m long on +Z and 1 m across, scaled per piece.
        private static Mesh Prism()
        {
            if (prism != null) return prism;
            const int faces = 6;
            var vertices = new Vector3[faces * 2];
            var normals = new Vector3[faces * 2];
            var triangles = new int[faces * 6];
            for (int i = 0; i < faces; i++)
            {
                float a = i * Mathf.PI * 2f / faces;
                var ring = new Vector3(Mathf.Cos(a) * 0.5f, Mathf.Sin(a) * 0.5f, 0f);
                vertices[i] = ring;
                vertices[i + faces] = ring + Vector3.forward;
                normals[i] = normals[i + faces] = ring.normalized;
                int next = (i + 1) % faces, t = i * 6;
                triangles[t] = i; triangles[t + 1] = next + faces; triangles[t + 2] = i + faces;
                triangles[t + 3] = i; triangles[t + 4] = next; triangles[t + 5] = next + faces;
            }
            prism = new Mesh { name = "RebarPrism", vertices = vertices, normals = normals, triangles = triangles };
            prism.RecalculateBounds();
            return prism;
        }

        private static Material Rust()
        {
            if (rust != null) return rust;
            rust = new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = "RebarRust" };
            rust.SetColor("_BaseColor", new Color(0.24f, 0.14f, 0.09f));
            rust.SetFloat("_Metallic", 0.55f);
            rust.SetFloat("_Smoothness", 0.28f);
            return rust;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            prism = null;
            rust = null;
        }
    }
}
