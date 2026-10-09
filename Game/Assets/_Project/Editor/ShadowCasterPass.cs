using Abandoned.Structure;
using UnityEngine;
using UnityEngine.Rendering;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Rebuild step (0.14.0): switches shadow casting off where it is wrong or wasted, in one place instead of
    /// in whichever builder placed the thing. Now that the lights nearest the camera all cast, what casts
    /// matters everywhere, not just round four fixed lamps.
    ///   Off: a light fixture's own housing and diffuser (they would block the light inside them), anything
    ///        flat on a surface (decals, paint, puddles, patches), see-through materials, particles.
    /// It never switches casting on: whatever a builder turned off on purpose stays off, and everything
    /// else keeps casting as it did.
    /// </summary>
    public static class ShadowCasterPass
    {
        // Thinner than this in any direction is a surface, not an object.
        private const float Flat = 0.03f;
        private const int Transparent = 3000;

        public static void Apply(Transform level, string levelName)
        {
            int cast = 0, housings = 0, flat = 0, clear = 0, already = 0;
            foreach (Renderer renderer in level.GetComponentsInChildren<Renderer>(true))
            {
                if (renderer.shadowCastingMode == ShadowCastingMode.Off) { already++; continue; }
                if (renderer.shadowCastingMode != ShadowCastingMode.On) continue; // shadows-only and two-sided are deliberate
                bool off = true;
                if (renderer is ParticleSystemRenderer || renderer is TrailRenderer || renderer is LineRenderer) already++;
                else if (renderer.GetComponentInParent<LightFixture>(true) != null) housings++;
                else if (SeeThrough(renderer)) clear++;
                else if (IsFlat(renderer)) flat++;
                else off = false;
                if (off) renderer.shadowCastingMode = ShadowCastingMode.Off;
                else cast++;
            }
            Debug.Log($"[MallValidation] {levelName}: shadow casters: {cast} cast; switched off: {housings} fixture parts, {flat} flat surfaces, " +
                      $"{clear} see-through; {already} were off already.");
        }

        private static bool SeeThrough(Renderer renderer)
        {
            Material[] materials = renderer.sharedMaterials;
            if (materials.Length == 0) return false;
            foreach (Material material in materials)
                if (material == null || material.renderQueue < Transparent) return false;
            return true;
        }

        private static bool IsFlat(Renderer renderer)
        {
            Vector3 size = Vector3.Scale(renderer.localBounds.size, renderer.transform.lossyScale);
            return Mathf.Min(Mathf.Abs(size.x), Mathf.Min(Mathf.Abs(size.y), Mathf.Abs(size.z))) < Flat;
        }
    }
}
