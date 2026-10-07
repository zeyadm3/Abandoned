using Abandoned.Interaction;
using Abandoned.Networking;
using UnityEngine;
using UnityEngine.Rendering;

namespace Abandoned.Player
{
    /// <summary>Restrained visual-only worker gait. Network movement/state drive the same pose on every copy.</summary>
    [DefaultExecutionOrder(150)]
    public class WorkerPresentation : MonoBehaviour
    {
        [SerializeField] private WorkerRig rig;
        [SerializeField] private PlayerMotor motor;
        [SerializeField] private PlayerCarrier carrier;
        [SerializeField] private PlayerRagdoll ragdoll;
        [SerializeField] private NetworkPlayer player;
        private Vector3 lastPosition, bodyRest;
        private float phase, pace, crouch, carry;
        private bool ownerHidden;
        private TextMesh[] crewNumbers;
        private int shownSeat = -1;

        private void Awake()
        {
            lastPosition = transform.position;
            if (rig != null) bodyRest = rig.transform.localPosition;
            crewNumbers = GetComponentsInChildren<TextMesh>(true);
        }

        private void LateUpdate()
        {
            if (rig == null || motor == null || ragdoll == null) return;
            if (player != null && player.IsSpawned && player.CrewSeat >= 0 && player.CrewSeat != shownSeat)
            {
                shownSeat = player.CrewSeat;
                foreach (TextMesh number in crewNumbers)
                    if (number.name == "CrewNumber") number.text = (shownSeat + 1).ToString();
            }
            // Camera-inside-body relied on backface culling with the old capsule. A shaped face/sleeve needs explicit hiding.
            bool hide = player != null && player.IsSpawned && player.IsOwner && !ragdoll.IsRagdolled;
            if (hide != ownerHidden)
            {
                ownerHidden = hide;
                foreach (Renderer renderer in rig.GetComponentsInChildren<Renderer>(true))
                    renderer.shadowCastingMode = hide ? ShadowCastingMode.ShadowsOnly : ShadowCastingMode.On;
            }

            Vector3 motion = transform.position - lastPosition;
            lastPosition = transform.position;
            if (ragdoll.IsRagdolled) return; // Local physics or the remote laid-down body owns its transform.
            float dt = Mathf.Max(Time.deltaTime, 0.0001f);
            float speed = Vector3.ProjectOnPlane(motion, Vector3.up).magnitude / dt;
            // Teleports and catch-up cannot kick a leg through a full animation cycle.
            if (speed > 12f) speed = 0f;
            pace = Mathf.Lerp(pace, motor.IsGrounded ? Mathf.Clamp01(speed / 3f) : 0f, 1f - Mathf.Exp(-10f * dt));
            phase = (phase + Mathf.Min(speed, 8f) * dt * 4.2f) % (Mathf.PI * 2f);
            crouch = Mathf.Lerp(crouch, motor.IsCrouching ? 1f : 0f, 1f - Mathf.Exp(-10f * dt));
            bool holding = carrier != null && carrier.Held != null;
            carry = Mathf.Lerp(carry, holding ? 1f : 0f, 1f - Mathf.Exp(-9f * dt));
            float stride = Mathf.Sin(phase) * pace * (motor.IsCrouching ? 12f : 23f);
            rig.transform.localPosition = bodyRest + Vector3.down * (crouch * 0.6f) + Vector3.up * (Mathf.Abs(Mathf.Sin(phase)) * pace * 0.018f);
            rig.Torso.localRotation = Quaternion.Euler(crouch * 15f + carry * 3f, 0f, Mathf.Sin(phase) * pace * 1.3f);
            rig.LeftLeg.localRotation = Quaternion.Euler(stride * (1f - crouch * 0.7f) - crouch * 78f, 0f, -crouch * 5f);
            rig.RightLeg.localRotation = Quaternion.Euler(-stride * (1f - crouch * 0.7f) - crouch * 78f, 0f, crouch * 5f);
            // Bend at actual knee pivots, keeping coveralls/boots their authored proportions while the hips lower.
            rig.LeftKnee.localRotation = Quaternion.Euler(crouch * 146f + Mathf.Max(0f, -stride) * (1f - crouch) * 0.55f, 0f, 0f);
            rig.RightKnee.localRotation = Quaternion.Euler(crouch * 146f + Mathf.Max(0f, stride) * (1f - crouch) * 0.55f, 0f, 0f);
            rig.LeftArm.localRotation = Quaternion.Euler(Mathf.Lerp(-stride * 0.55f, -52f, carry), 0f, -8f - carry * 6f);
            rig.RightArm.localRotation = Quaternion.Euler(Mathf.Lerp(stride * 0.55f, -52f, carry), 0f, 8f + carry * 6f);
        }

#if UNITY_EDITOR
        public void EditorSetup(WorkerRig worker, PlayerMotor movement, PlayerCarrier carrying, PlayerRagdoll falling, NetworkPlayer network)
        {
            rig = worker; motor = movement; carrier = carrying; ragdoll = falling; player = network;
        }
#endif
    }
}
