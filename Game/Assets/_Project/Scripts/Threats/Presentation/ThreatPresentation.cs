using UnityEngine;

namespace Abandoned.Threats
{
    /// <summary>Visual-only movement from replicated displacement. No Animator, physics, AI or network authority changes.</summary>
    public class ThreatPresentation : MonoBehaviour
    {
        public enum Silhouette { BlindOne, Stalker, Collector, Hunter }
        [SerializeField] private Silhouette kind;
        [SerializeField] private Transform visual, leftArm, rightArm, leftLeg, rightLeg, head;
        private Vector3 lastPosition, rest;
        private float phase, motion;

        private void Awake()
        {
            lastPosition = transform.position;
            if (visual != null) rest = visual.localPosition;
        }

        private void LateUpdate()
        {
            if (visual == null) return;
            float dt = Mathf.Max(Time.deltaTime, 0.0001f);
            float speed = Vector3.ProjectOnPlane(transform.position - lastPosition, Vector3.up).magnitude / dt;
            lastPosition = transform.position;
            if (speed > 15f) speed = 0f;
            motion = Mathf.Lerp(motion, Mathf.Clamp01(speed / 1.4f), 1f - Mathf.Exp(-8f * dt));
            float cadence = kind == Silhouette.Collector ? 6.5f : kind == Silhouette.Hunter ? 2.3f : 3.3f;
            phase = (phase + speed * cadence * dt) % (Mathf.PI * 2f);
            float stride = Mathf.Sin(phase) * motion;
            float slow = Mathf.Sin(Time.time * (kind == Silhouette.BlindOne ? 1.1f : 0.7f));
            float bodyBob = kind == Silhouette.Hunter ? 0.032f : kind == Silhouette.Collector ? 0.022f : 0.01f;
            visual.localPosition = rest + Vector3.up * (Mathf.Abs(stride) * bodyBob + slow * 0.006f);
            visual.localRotation = Quaternion.Euler(0f, 0f, kind == Silhouette.Stalker ? 0f : stride * 1.4f);
            float legSwing = kind == Silhouette.Hunter ? 11f : kind == Silhouette.Stalker ? 8f : 19f;
            if (leftLeg != null) leftLeg.localRotation = Quaternion.Euler(stride * legSwing, 0f, 0f);
            if (rightLeg != null) rightLeg.localRotation = Quaternion.Euler(-stride * legSwing, 0f, 0f);
            float armSwing = kind == Silhouette.BlindOne ? 8f : kind == Silhouette.Hunter ? 6f : 10f;
            if (leftArm != null) leftArm.localRotation = Quaternion.Euler(-stride * armSwing - (kind == Silhouette.BlindOne ? 18f : 0f), 0f, -4f);
            if (rightArm != null) rightArm.localRotation = Quaternion.Euler(stride * armSwing - (kind == Silhouette.BlindOne ? 18f : 0f), 0f, 4f);
            if (head != null) head.localRotation = Quaternion.Euler(kind == Silhouette.Collector ? 8f : 0f, slow * 3f, kind == Silhouette.BlindOne ? slow * 4f : 0f);
        }

#if UNITY_EDITOR
        public void EditorSetup(Silhouette style, Transform model, Transform armL, Transform armR, Transform legL, Transform legR, Transform skull)
        {
            kind = style; visual = model; leftArm = armL; rightArm = armR; leftLeg = legL; rightLeg = legR; head = skull;
        }
#endif
    }
}
