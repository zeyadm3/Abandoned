using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Threats
{
    /// <summary>
    /// How strongly a blind listener perceives a noise (0..1): the noise's hearing radius, scaled by the
    /// listener's hearing and shrunk by every wall in between, then linear falloff to 0 at that radius.
    /// </summary>
    public static class Hearing
    {
        public static float Perceive(NoiseEvent e, Vector3 ear, int walls, float hearing, float wallDamping)
        {
            float radius = NoiseSystem.RadiusOf(e) * hearing * Mathf.Pow(Mathf.Clamp01(wallDamping), walls);
            if (radius <= 0f) return 0f;
            float d = Vector3.Distance(ear, e.Position);
            return d >= radius ? 0f : Mathf.Clamp01(e.Loudness * (1f - d / radius) + 0.05f);
        }
    }
}
