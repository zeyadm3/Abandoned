using System.Collections.Generic;
using Abandoned.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Rebuild-time audit: every solid collider a player can bump into must sit inside visible geometry.
    /// Samples points through each collider's volume and checks them against the world bounds of visible
    /// renderers; anything mostly uncovered is an "invisible wall" and is logged as an error, unless it's
    /// marked <see cref="IntentionalInvisibleCollider"/>, in which case it's listed with its reason.
    /// </summary>
    public static class InvisibleColliderAudit
    {
        private const float Tolerance = 0.12f, MinCoverage = 0.75f;

        public static int Run(Transform level, string levelName)
        {
            Physics.SyncTransforms();
            var visible = new List<Bounds>();
            foreach (Renderer r in level.GetComponentsInChildren<Renderer>())
                if (IsVisible(r))
                {
                    Bounds b = r.bounds;
                    b.Expand(Tolerance * 2f);
                    visible.Add(b);
                }

            int problems = 0, intentional = 0;
            var nearby = new List<Bounds>();
            foreach (Collider c in level.GetComponentsInChildren<Collider>())
            {
                if (!c.enabled || c.isTrigger || !BlocksPlayers(c.gameObject.layer)) continue;
                var marker = c.GetComponentInParent<IntentionalInvisibleCollider>();
                if (marker != null)
                {
                    intentional++;
                    Debug.Log($"[MallValidation] {levelName}: intentional invisible collider {Path(c)}: {marker.Reason}");
                    continue;
                }
                nearby.Clear();
                Bounds own = c.bounds;
                foreach (Bounds b in visible) if (b.Intersects(own)) nearby.Add(b);
                float coverage = Coverage(c, nearby);
                if (coverage >= MinCoverage) continue;
                problems++;
                Debug.LogError($"[MallValidation] {levelName}: invisible collider {Path(c)} (layer {LayerMask.LayerToName(c.gameObject.layer)}, " +
                               $"centre {own.center}, size {own.size}): only {coverage:P0} of it is inside visible geometry.");
            }
            Debug.Log($"[MallValidation] {levelName}: invisible colliders: {problems} problem(s), {intentional} intentional.");
            return problems;
        }

        private static bool BlocksPlayers(int layer) =>
            layer != GameLayers.DebrisLayer && layer != GameLayers.PlayerLayer && !Physics.GetIgnoreLayerCollision(layer, GameLayers.PlayerLayer);

        private static bool IsVisible(Renderer r)
        {
            if (!r.enabled || !r.gameObject.activeInHierarchy || r.shadowCastingMode == ShadowCastingMode.ShadowsOnly) return false;
            if (r is ParticleSystemRenderer || r.gameObject.layer == GameLayers.DebrisLayer) return false;
            if (r.sharedMaterial == null) return false;
            return r is SkinnedMeshRenderer || (r.TryGetComponent(out MeshFilter filter) && filter.sharedMesh != null);
        }

        // Fraction of sample points inside the collider that fall inside some visible renderer's bounds.
        private static float Coverage(Collider c, List<Bounds> visible)
        {
            int inside = 0, total = 0;
            foreach (Vector3 p in Samples(c))
            {
                total++;
                foreach (Bounds b in visible)
                    if (b.Contains(p)) { inside++; break; }
            }
            return total == 0 ? 1f : inside / (float)total;
        }

        private static IEnumerable<Vector3> Samples(Collider c)
        {
            if (c is BoxCollider box)
            {
                // A 3 x 3 x 3 grid through the oriented box itself, so thin rotated panels sample correctly.
                for (int x = 0; x < 3; x++)
                for (int y = 0; y < 3; y++)
                for (int z = 0; z < 3; z++)
                {
                    Vector3 f = new Vector3(x, y, z) / 3f + Vector3.one / 6f - Vector3.one * 0.5f;
                    yield return box.transform.TransformPoint(box.center + Vector3.Scale(f, box.size));
                }
                yield break;
            }
            Bounds b = c.bounds;
            for (int x = 0; x < 3; x++)
            for (int y = 0; y < 3; y++)
            for (int z = 0; z < 3; z++)
            {
                Vector3 p = b.min + Vector3.Scale(b.size, new Vector3(x, y, z) / 3f + Vector3.one / 6f);
                // Mesh colliders that aren't convex can't answer ClosestPoint; their bounds are the sample.
                if (c is MeshCollider { convex: false } || (c.ClosestPoint(p) - p).sqrMagnitude < 1e-4f) yield return p;
            }
        }

        private static string Path(Component c)
        {
            string path = c.name;
            for (Transform t = c.transform.parent; t != null; t = t.parent) path = t.name + "/" + path;
            return path;
        }
    }
}
