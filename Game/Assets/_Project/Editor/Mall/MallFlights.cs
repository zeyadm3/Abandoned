using Abandoned.Player;
using UnityEditor;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Escalators and stairs: one root per flight (one StructuralSection). The custom kit model is the
    /// visible steps; one smooth sloped collider through the tread centres carries players and loot so
    /// nobody snags on a step edge, and two side rails follow the model's handrails.
    /// </summary>
    public static class MallFlights
    {
        public const string RampName = "Ramp", VisualName = "Visual";
        public const int Steps = 20;
        private const float RampThickness = 0.25f, RailHeight = 1.05f, RailThickness = 0.15f;
        private const float EscalatorWidth = 2.4f, StairWidth = 3f;
        private const string MovementConfigPath = "Assets/_Project/Data/Player/PlayerMovementConfig.asset";

        public static float WidthOf(MallLayout.Flight f) => f.CanCollapse ? EscalatorWidth : StairWidth;
        public static float Length => 2f * MallLayout.Tile;
        public static float Rise => MallLayout.StoryHeight;

        public static Transform Build(MallLayout.Flight f, Transform parent)
        {
            bool escalator = f.CanCollapse;
            float width = WidthOf(f), length = Length, rise = Rise;
            CheckAgainstMovement(f.Name, rise, length);

            // Pivot at the foot: the middle of the edge the flight starts from, on the lower floor.
            Vector3 dir = new(f.Dir.x, 0f, f.Dir.y);
            Vector3 startCenter = MallLayout.TileTopCenter(f.Start, f.Floor);
            var root = new GameObject(f.Name).transform;
            root.SetParent(parent, false);
            root.localPosition = startCenter - dir * (MallLayout.Tile / 2f);
            root.localRotation = Quaternion.LookRotation(dir, Vector3.up);

            var visual = new GameObject(VisualName).transform;
            visual.SetParent(root, false);
            GameObject model = CustomMallArt.Place(escalator ? "escalator" : "stairs", visual, Vector3.zero, Quaternion.identity);
            // The kit authors 20 treads over 8 m x 4 m; fit it to the layout's flight if that ever changes.
            Bounds b = ModelFit.LocalBounds(model, root);
            Debug.Log($"[Flights] {f.Name}: model bounds centre {b.center} size {b.size}, {model.GetComponentsInChildren<Renderer>(true).Length} renderer(s).");
            if (b.size.z > 0.1f && Mathf.Abs(b.size.z - length) > 0.15f)
            {
                model.transform.localScale = new Vector3(1f, rise / Mathf.Max(0.1f, b.max.y), length / b.size.z);
                b = ModelFit.LocalBounds(model, root);
                model.transform.localPosition += new Vector3(-b.center.x, 0f, -b.min.z);
            }

            float angle = Mathf.Atan2(rise, length);
            float slope = Mathf.Sqrt(length * length + rise * rise);
            Vector3 up = new(0f, Mathf.Cos(angle), -Mathf.Sin(angle));
            // Top surface through every tread's centre (stepRise/2 above the nosing line): feet sink no
            // more than half a step at a nosing, and both ends meet their floors within half a step.
            float lift = rise / Steps / 2f;
            Vector3 surfaceMid = new(0f, rise / 2f + lift, length / 2f);
            Quaternion tilt = Quaternion.Euler(-angle * Mathf.Rad2Deg, 0f, 0f);

            var ramp = new GameObject(RampName).transform;
            ramp.SetParent(root, false);
            ramp.localPosition = surfaceMid - up * (RampThickness / 2f);
            ramp.localRotation = tilt;
            ramp.gameObject.AddComponent<BoxCollider>().size = new Vector3(width, RampThickness, slope);

            // Side rails stand on the ramp surface up to the model's handrail, so the edge over the atrium
            // or the stairwell is closed but the flight's own width stays free.
            foreach (float side in new[] { -1f, 1f })
            {
                var rail = new GameObject(side < 0 ? "SideRail_L" : "SideRail_R").transform;
                rail.SetParent(visual, false);
                rail.localPosition = surfaceMid + up * (RailHeight / 2f) + Vector3.right * side * (width / 2f + RailThickness / 2f);
                rail.localRotation = tilt;
                rail.gameObject.AddComponent<BoxCollider>().size = new Vector3(RailThickness, RailHeight, slope);
            }
            return root;
        }

        /// <summary>The flight must be walkable with the player's own controller limits, not just look like stairs.</summary>
        private static void CheckAgainstMovement(string name, float rise, float length)
        {
            var movement = AssetDatabase.LoadAssetAtPath<PlayerMovementConfig>(MovementConfigPath);
            if (movement == null)
            {
                Debug.LogError($"[Flights] {MovementConfigPath} missing; can't check {name} against the player's limits.");
                return;
            }
            float slope = Mathf.Atan2(rise, length) * Mathf.Rad2Deg;
            if (slope > movement.SlopeLimit - 5f)
                Debug.LogError($"[Flights] {name}: {slope:0.#}° is steeper than the player can walk (slope limit {movement.SlopeLimit}°).");
            // The ramp's ends sit half a step above their floors; that lip must be a step the controller takes.
            if (rise / Steps / 2f > movement.StepOffset)
                Debug.LogError($"[Flights] {name}: the ramp lip ({rise / Steps / 2f:0.00} m) is higher than the step offset ({movement.StepOffset} m).");
        }
    }
}
