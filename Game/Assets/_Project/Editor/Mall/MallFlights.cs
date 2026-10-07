using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Escalators and stairs: one root per flight (one StructuralSection), visual steps plus one smooth
    /// sloped collider so players and carried loot glide up instead of snagging on step edges.
    /// </summary>
    public static class MallFlights
    {
        private const int Steps = 20;
        private const float StepThickness = 0.35f, RampThickness = 0.2f;
        private const float EscalatorWidth = 2.4f, StairWidth = 3f, SideHeight = 1f;

        public static float WidthOf(MallLayout.Flight f) => f.CanCollapse ? EscalatorWidth : StairWidth;

        public static Transform Build(MallLayout.Flight f, Transform parent, Material steps, Material sides)
        {
            float length = 2f * MallLayout.Tile, rise = MallLayout.StoryHeight;
            bool escalator = f.CanCollapse;
            float width = WidthOf(f);

            // Pivot at the foot: the middle of the edge the flight starts from, on the lower floor.
            Vector3 dir = new(f.Dir.x, 0f, f.Dir.y);
            Vector3 startCenter = MallLayout.TileTopCenter(f.Start, f.Floor);
            var root = new GameObject(f.Name).transform;
            root.SetParent(parent, false);
            root.localPosition = startCenter - dir * (MallLayout.Tile / 2f);
            root.localRotation = Quaternion.LookRotation(dir, Vector3.up);

            Transform visual = Group("Visual", root);
            float stepRise = rise / Steps, stepDepth = length / Steps;
            for (int s = 0; s < Steps; s++)
            {
                float top = (s + 1) * stepRise;
                Box($"Step_{s}", visual, new Vector3(0f, top - StepThickness / 2f, (s + 0.5f) * stepDepth),
                    new Vector3(width, StepThickness, stepDepth), steps, withCollider: false);
            }
            // Side rails (balustrades on escalators): nobody steps off a flight over the atrium.
            {
                float angle = Mathf.Atan2(rise, length) * Mathf.Rad2Deg;
                foreach (float side in new[] { -1f, 1f })
                {
                    var rail = Box($"Balustrade_{(side < 0 ? "L" : "R")}", visual,
                        new Vector3(side * (width / 2f + 0.1f), rise / 2f + SideHeight / 2f, length / 2f),
                        new Vector3(0.15f, SideHeight, Mathf.Sqrt(length * length + rise * rise)), sides);
                    rail.transform.localRotation = Quaternion.Euler(-angle, 0f, 0f);
                }
            }

            float rad = Mathf.Atan2(rise, length);
            var ramp = new GameObject("Ramp").transform;
            ramp.SetParent(root, false);
            Vector3 mid = new(0f, rise / 2f + stepRise / 2f, length / 2f);
            ramp.localPosition = mid - new Vector3(0f, Mathf.Cos(rad), -Mathf.Sin(rad)) * (RampThickness / 2f);
            ramp.localRotation = Quaternion.Euler(-rad * Mathf.Rad2Deg, 0f, 0f);
            ramp.gameObject.AddComponent<BoxCollider>().size = new Vector3(width, RampThickness, Mathf.Sqrt(length * length + rise * rise));
            return root;
        }
    }
}
