using System.Collections.Generic;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>Fixture supports use the same ceiling mesh and imported coordinate space as dressing.</summary>
    public static class MeridianLightingCeilingBounds
    {
        private static readonly Dictionary<string, float> MinimumByModel = new();

        public static float MinimumY(string key)
        {
            if (MinimumByModel.TryGetValue(key, out float minimum)) return minimum;
            GameObject model = MeridianArt.Model(key);
            if (model == null) throw new System.InvalidOperationException($"Cannot measure Meridian ceiling {key}.");
            minimum = float.PositiveInfinity;
            foreach (Renderer renderer in model.GetComponentsInChildren<Renderer>(true))
            {
                Bounds bounds = renderer.localBounds;
                // All eight corners survive importer axis changes and nonuniform child transforms.
                for (int corner = 0; corner < 8; corner++)
                {
                    Vector3 local = bounds.center + Vector3.Scale(bounds.extents,
                        new Vector3((corner & 1) == 0 ? -1f : 1f, (corner & 2) == 0 ? -1f : 1f,
                            (corner & 4) == 0 ? -1f : 1f));
                    Vector3 inModel = model.transform.InverseTransformPoint(renderer.transform.TransformPoint(local));
                    minimum = Mathf.Min(minimum, inModel.y);
                }
            }
            if (float.IsPositiveInfinity(minimum))
                throw new System.InvalidOperationException($"Meridian ceiling {key} has no renderer bounds.");
            MinimumByModel[key] = minimum;
            return minimum;
        }
    }
}
