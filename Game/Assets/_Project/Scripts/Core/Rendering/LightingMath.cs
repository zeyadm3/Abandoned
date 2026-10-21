using UnityEngine;

namespace Abandoned.Core
{
    /// <summary>URP's punctual range and spot falloff, shared by fixture authoring and runtime shadow selection.</summary>
    public static class LightingMath
    {
        public static float RangeAttenuation(float distance, float range)
        {
            float ratio = distance / Mathf.Max(.01f, range);
            float window = Mathf.Clamp01(1f - ratio * ratio * ratio * ratio);
            return window * window / Mathf.Max(.01f, distance * distance);
        }

        public static float Contribution(Light light, Vector3 point)
        {
            if (light == null || !light.isActiveAndEnabled || light.intensity <= .001f) return 0f;
            Vector3 ray = point - light.transform.position;
            float distance = ray.magnitude;
            if (distance >= light.range) return 0f;
            float cone = 1f;
            if (light.type == LightType.Spot)
            {
                float outer = Mathf.Cos(light.spotAngle * .5f * Mathf.Deg2Rad);
                float inner = Mathf.Cos(light.innerSpotAngle * .5f * Mathf.Deg2Rad);
                cone = Mathf.Clamp01((Vector3.Dot(light.transform.forward, ray / Mathf.Max(distance, .001f)) - outer)
                    / Mathf.Max(.001f, inner - outer));
                cone *= cone;
            }
            return light.intensity * RangeAttenuation(distance, light.range) * cone;
        }
    }
}
