using System.Collections.Generic;
using UnityEngine;

namespace Abandoned.Core
{
    /// <summary>
    /// A fixed ladder (Iron Factory catwalks): a climb volume in front of its face. Like the freight lift and
    /// the rope descent it is a scene object on every machine and never networked: each player's own motor
    /// reads it (owner-authoritative movement), so it doesn't depend on the player code.
    /// The pivot is the foot of the ladder at its face; forward points out, toward the climber.
    /// </summary>
    public class Ladder : MonoBehaviour
    {
        private const float HalfWidth = .6f;
        private const float Reach = .9f;
        // The volume carries on above the top so a climber is still on it while stepping onto the walkway.
        private const float AboveTop = 1.3f;

        private static readonly List<Ladder> Ladders = new();

        [Tooltip("Metres from the foot to the walkway it reaches.")]
        [SerializeField, Min(.5f)] private float height = 5f;

        public float TopY => transform.position.y + height;
        /// <summary>Horizontal direction from the climber onto the ladder (and over the top onto the walkway).</summary>
        public Vector3 Inward => -transform.forward;

        /// <summary>The ladder whose climb volume holds these feet, if any.</summary>
        public static Ladder At(Vector3 feet)
        {
            foreach (Ladder l in Ladders)
            {
                if (l == null) continue;
                Vector3 p = l.transform.InverseTransformPoint(feet);
                if (Mathf.Abs(p.x) <= HalfWidth && p.z >= 0f && p.z <= Reach && p.y >= -.1f && p.y <= l.height + AboveTop) return l;
            }
            return null;
        }

        private void OnEnable() => Ladders.Add(this);
        private void OnDisable() => Ladders.Remove(this);

#if UNITY_EDITOR
        public void EditorSetup(float rise) => height = rise;
#endif

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private void OnDrawGizmos()
        {
            if (!DebugView.Visible) return;
            Gizmos.color = new Color(.3f, 1f, .5f, .6f);
            Gizmos.matrix = transform.localToWorldMatrix;
            float tall = height + AboveTop + .1f;
            Gizmos.DrawWireCube(new Vector3(0f, tall * .5f - .1f, Reach * .5f), new Vector3(HalfWidth * 2f, tall, Reach));
        }
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Ladders.Clear();
    }
}
