using Abandoned.Core;
using UnityEngine;
using UnityEngine.AI;

namespace Abandoned.Structure
{
    /// <summary>
    /// The solid heap a fallen section leaves where it lands (user, 2026-10-08: big rubble that blocks you).
    /// Built locally on every machine from the section, the static building below it and the collapse seed,
    /// so it is the same everywhere without being networked. One upright box collider on the Default layer
    /// blocks players, loot and monsters (its NavMeshObstacle carves); the broken slabs around it are visual.
    /// It sits on <see cref="Support"/>; if that falls too, the simulation drops the mound again.
    /// It never lands on stairs or <see cref="SectionKeepClear"/> sections, and it doesn't load the floor.
    /// </summary>
    public class RubbleMound : MonoBehaviour
    {
        private static readonly RaycastHit[] Hits = new RaycastHit[16];
        private static Mesh cube;
        private static Material fallback;

        public StructuralSection Fallen { get; private set; }
        public StructuralSection Support { get; private set; }
        /// <summary>World bounds of the solid part.</summary>
        public Bounds Solid { get; private set; }

        /// <summary>Every machine: drop the mound for a fallen section; null when it lands nowhere a mound may be.</summary>
        public static RubbleMound Drop(StructuralSection fallen, StructureConfig config)
        {
            Bounds surface = fallen.SurfaceBounds;
            if (surface.size.x < 0.5f || surface.size.z < 0.5f) return null;
            if (!Landing(fallen, surface, config.RubbleMaxDrop, out Vector3 floor, out StructuralSection support)) return null;
            if (support != null && (support.Type == SectionType.Stair || support.GetComponent<SectionKeepClear>() != null)) return null;
            // A section that falls into molten iron is gone; a mound there would be a bridge across it.
            if (LavaVolume.Covers(floor)) return null;

            var random = new System.Random(fallen.CollapseSeed ^ (fallen.Id * 3571 + 101));
            float Next() => (float)random.NextDouble();
            float width = Mathf.Clamp(surface.size.x * config.RubbleFootprint, config.RubbleWidthLimits.x, config.RubbleWidthLimits.y);
            float depth = Mathf.Clamp(surface.size.z * config.RubbleFootprint, config.RubbleWidthLimits.x, config.RubbleWidthLimits.y);
            float height = Mathf.Lerp(config.RubbleHeight.x, Mathf.Max(config.RubbleHeight.x, config.RubbleHeight.y), Next());

            var root = new GameObject($"Rubble_{fallen.name}");
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(root, fallen.gameObject.scene);
            root.transform.position = floor;
            var mound = root.AddComponent<RubbleMound>();
            mound.Fallen = fallen;
            mound.Support = support;

            var solid = root.AddComponent<BoxCollider>();
            solid.center = new Vector3(0f, height * 0.5f, 0f);
            solid.size = new Vector3(width, height, depth);
            var obstacle = root.AddComponent<NavMeshObstacle>();
            obstacle.shape = NavMeshObstacleShape.Box;
            obstacle.center = solid.center;
            obstacle.size = solid.size + new Vector3(0.3f, 0f, 0.3f);
            obstacle.carving = true;
            obstacle.carveOnlyStationary = true;
            mound.Solid = new Bounds(floor + solid.center, solid.size);

            Material material = MaterialOf(fallen);
            // A core block, then broken slabs leaning on its sides and lying on top, then loose lumps at the foot.
            Piece(root, material, new Vector3(0f, height * 0.36f, 0f), new Vector3(width * 0.94f, height * 0.72f, depth * 0.94f), new Vector3(0f, Next() * 16f - 8f, 0f));
            int slabs = 4 + random.Next(3);
            for (int i = 0; i < slabs; i++)
            {
                float angle = (i + Next() * 0.6f) / slabs * Mathf.PI * 2f;
                var side = new Vector3(Mathf.Cos(angle) * width * 0.38f, 0f, Mathf.Sin(angle) * depth * 0.38f);
                var size = new Vector3(Mathf.Lerp(0.45f, 0.8f, Next()) * width, Mathf.Lerp(0.14f, 0.26f, Next()), Mathf.Lerp(0.35f, 0.6f, Next()) * depth);
                Vector3 tilt = new(Mathf.Lerp(18f, 48f, Next()) * (Next() < 0.5f ? -1f : 1f), angle * Mathf.Rad2Deg + 90f, Next() * 20f - 10f);
                Piece(root, material, side + Vector3.up * height * Mathf.Lerp(0.35f, 0.6f, Next()), size, tilt);
            }
            for (int i = 0; i < 2; i++)
                Piece(root, material, new Vector3((Next() - 0.5f) * width * 0.3f, height - 0.1f, (Next() - 0.5f) * depth * 0.3f),
                    new Vector3(width * Mathf.Lerp(0.5f, 0.8f, Next()), 0.18f, depth * Mathf.Lerp(0.4f, 0.7f, Next())), new Vector3(Next() * 14f - 7f, Next() * 90f, Next() * 14f - 7f));
            int lumps = 6 + random.Next(5);
            for (int i = 0; i < lumps; i++)
            {
                float angle = Next() * Mathf.PI * 2f, s = Mathf.Lerp(0.2f, 0.5f, Next());
                var at = new Vector3(Mathf.Cos(angle) * (width * 0.5f + 0.05f), s * 0.4f, Mathf.Sin(angle) * (depth * 0.5f + 0.05f));
                Piece(root, material, at, new Vector3(s, s * 0.8f, s * 1.1f), new Vector3(Next() * 40f, Next() * 360f, Next() * 40f));
            }

            Physics.SyncTransforms();
            StructureSignals.RaiseRubbleLanded(mound.Solid);
            // Heard where it lands, a moment after the floor above lets go. Not for sections already down when we joined.
            if (!fallen.CollapsedQuietly) Audio.GameAudio.PlayDelayed(Audio.SoundId.RubbleLand, mound.Solid.center, 0.55f, 0.9f);
            if (GameAuthority.IsHost) mound.LiftLoot();
            return mound;
        }

        // Straight down from under the fallen section: the first solid thing that isn't the section itself,
        // a player, loot or debris. Another mound counts (heaps stack) and passes on what it stands on.
        private static bool Landing(StructuralSection fallen, Bounds surface, float maxDrop, out Vector3 floor, out StructuralSection support)
        {
            floor = default;
            support = null;
            int mask = (1 << 0) | (1 << Mathf.Max(0, GameLayers.StructureLayer));
            var from = new Vector3(surface.center.x, surface.min.y - 0.05f, surface.center.z);
            int count = Physics.RaycastNonAlloc(from, Vector3.down, Hits, maxDrop, mask, QueryTriggerInteraction.Ignore);
            float best = float.MaxValue;
            bool found = false;
            for (int i = 0; i < count; i++)
            {
                RaycastHit hit = Hits[i];
                if (hit.distance >= best || hit.collider.transform.IsChildOf(fallen.transform)) continue;
                if (hit.collider.attachedRigidbody != null && !hit.collider.attachedRigidbody.isKinematic) continue;
                best = hit.distance;
                floor = hit.point;
                RubbleMound under = hit.collider.GetComponentInParent<RubbleMound>();
                support = under != null ? under.Support : hit.collider.GetComponentInParent<StructuralSection>();
                found = true;
            }
            return found;
        }

        private static void Piece(GameObject root, Material material, Vector3 localPosition, Vector3 size, Vector3 euler)
        {
            var piece = new GameObject("Slab", typeof(MeshFilter), typeof(MeshRenderer));
            piece.transform.SetParent(root.transform, false);
            piece.transform.localPosition = localPosition;
            piece.transform.localRotation = Quaternion.Euler(euler);
            piece.transform.localScale = size;
            piece.GetComponent<MeshFilter>().sharedMesh = Cube();
            piece.GetComponent<MeshRenderer>().sharedMaterial = material;
        }

        private static Mesh Cube()
        {
            if (cube != null) return cube;
            GameObject temp = GameObject.CreatePrimitive(PrimitiveType.Cube);
            // Inactive at once: its collider must not catch this frame's landing rays.
            temp.SetActive(false);
            cube = temp.GetComponent<MeshFilter>().sharedMesh;
            Destroy(temp);
            return cube;
        }

        private static Material MaterialOf(StructuralSection fallen)
        {
            if (fallen.Visual != null)
                foreach (Renderer r in fallen.Visual.GetComponentsInChildren<Renderer>(true))
                    if (r.sharedMaterial != null) return r.sharedMaterial;
            if (fallback == null)
            {
                Shader lit = Shader.Find("Universal Render Pipeline/Lit");
                fallback = new Material(lit != null ? lit : Shader.Find("Standard")) { name = "Rubble_Fallback" };
                fallback.SetColor("_BaseColor", new Color(0.45f, 0.43f, 0.4f));
            }
            return fallback;
        }

        // Host: resting loot the heap landed on is set on top of it instead of being buried.
        private void LiftLoot()
        {
            int loot = GameLayers.LootLayer;
            if (loot < 0) return;
            Collider[] inside = Physics.OverlapBox(Solid.center, Solid.extents, Quaternion.identity, 1 << loot, QueryTriggerInteraction.Ignore);
            foreach (Collider c in inside)
            {
                Rigidbody body = c.attachedRigidbody;
                if (body == null || body.isKinematic) continue;
                body.position = new Vector3(body.position.x, Solid.max.y + 0.4f, body.position.z);
                body.WakeUp();
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            cube = null;
            fallback = null;
        }
    }
}
