using UnityEngine;

namespace Abandoned.Threats
{
    /// <summary>
    /// Monster animation polish (0.21.3), on every machine, on top of whatever drives the body (the procedural
    /// silhouettes or an Animator): a pivot slipped in above the model leans it into turns, jerks it back when
    /// something is thrown into it, and gives each kind its own idle when it stands still. Visual only.
    /// </summary>
    [DefaultExecutionOrder(200)]
    public class ThreatPolish : MonoBehaviour
    {
        private Transform pivot;
        private Threat threat;
        private Vector3 lastPosition;
        private float lastYaw, lean, idle, flinch, seed;
        private Vector3 flinchFrom;

        private void Start()
        {
            threat = GetComponent<Threat>();
            seed = Random.value * 10f;
            lastPosition = transform.position;
            lastYaw = transform.eulerAngles.y;
            // Everything drawn moves under one pivot this script owns outright.
            pivot = new GameObject("PolishPivot").transform;
            pivot.SetParent(transform, false);
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Transform child = transform.GetChild(i);
                if (child != pivot && child.GetComponentInChildren<Renderer>(true) != null) child.SetParent(pivot, true);
            }
        }

        /// <summary>Struck from a direction (world): a jolt away from it.</summary>
        public void Flinch(Vector3 from)
        {
            flinch = 1f;
            flinchFrom = from;
            Audio.GameAudio.Play(Audio.SoundId.ThreatFlinch, transform.position + Vector3.up * 1.4f, 1f);
        }

        private void LateUpdate()
        {
            if (pivot == null) return;
            float dt = Mathf.Max(Time.deltaTime, 0.0001f);
            Vector3 moved = transform.position - lastPosition;
            lastPosition = transform.position;
            float speed = Vector3.ProjectOnPlane(moved, Vector3.up).magnitude / dt;
            if (speed > 15f) speed = 0f;
            float yaw = transform.eulerAngles.y, yawRate = Mathf.DeltaAngle(lastYaw, yaw) / dt;
            lastYaw = yaw;
            // Leaning into a turn, more the faster it goes.
            lean = Mathf.Lerp(lean, Mathf.Clamp(-yawRate * 0.04f * Mathf.Clamp01(speed / 2f), -12f, 12f), 1f - Mathf.Exp(-6f * dt));
            idle = Mathf.Lerp(idle, speed < 0.15f ? 1f : 0f, 1f - Mathf.Exp(-3f * dt));
            flinch = Mathf.Max(0f, flinch - dt * 2.5f);
            float t = Time.time + seed;
            Vector3 offset = Vector3.zero;
            Vector3 euler = new(0f, 0f, lean);
            switch (threat != null && threat.Definition != null ? threat.Definition.Kind : ThreatKind.BlindOne)
            {
                case ThreatKind.BlindOne: euler += new Vector3(Mathf.Sin(t * 0.7f) * 4f, Mathf.Sin(t * 0.45f) * 18f, Mathf.Sin(t * 0.9f) * 6f) * idle; break; // listening, head-sweeping
                case ThreatKind.Stalker: offset.y += Mathf.Sin(t * 1.6f) * 0.008f * idle; break; // dead still, barely breathing
                case ThreatKind.Collector: euler += new Vector3(0f, (Mathf.PerlinNoise(t * 3f, 1f) - 0.5f) * 30f, 0f) * idle; break; // twitchy, peering about
                case ThreatKind.Hunter: euler.x += Mathf.Sin(t * 1.2f) * 3f * idle; offset.y += Mathf.Sin(t * 1.2f) * 0.03f * idle; break; // heavy breathing
                case ThreatKind.Weight: euler.z += Mathf.Sin(t * 0.35f) * 3f * idle; break; // ponderous sway
                case ThreatKind.Thing: euler.y += t * 25f * idle; break; // slowly turning in place
                case ThreatKind.Crawlers: offset += new Vector3(Mathf.PerlinNoise(t * 6f, 2f) - 0.5f, 0f, Mathf.PerlinNoise(t * 6f, 3f) - 0.5f) * 0.12f * idle; break; // scuttling on the spot
                default: euler.y += Mathf.Sin(t * 0.8f) * 25f * idle; break; // the Last Hunter's head swings
            }
            if (flinch > 0f)
            {
                Vector3 away = transform.InverseTransformDirection(flinchFrom);
                float k = Mathf.Sin(flinch * Mathf.PI) * flinch;
                offset += new Vector3(away.x, 0f, away.z).normalized * 0.18f * k;
                euler.x -= 14f * k;
            }
            pivot.localPosition = offset;
            pivot.localRotation = Quaternion.Euler(euler);
        }
    }
}
