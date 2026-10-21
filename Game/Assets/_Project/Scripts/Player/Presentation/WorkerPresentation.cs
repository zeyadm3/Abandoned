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
        private GameObject heldTool;
        private Equipment.EquipmentDefinition shownTool;
        private Equipment.PlayerEquipment gear;

        /// <summary>When true, this local player's body and held gear are rendered for third-person camera.</summary>
        public bool ThirdPerson { get; set; }

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
            // Camera-inside-body relied on backface culling with the old capsule. A shaped face/sleeve needs explicit hiding in first person.
            bool hide = player != null && player.IsSpawned && player.IsOwner && !ragdoll.IsRagdolled && !ThirdPerson;
            if (hide != ownerHidden)
            {
                ownerHidden = hide;
                foreach (Renderer renderer in rig.GetComponentsInChildren<Renderer>(true))
                    renderer.shadowCastingMode = hide ? ShadowCastingMode.ShadowsOnly : ShadowCastingMode.On;
                if (heldTool != null)
                    foreach (Renderer r in heldTool.GetComponentsInChildren<Renderer>())
                        r.shadowCastingMode = hide ? ShadowCastingMode.ShadowsOnly : ShadowCastingMode.On;
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
            rig.transform.localPosition = bodyRest + Vector3.down * (crouch * 0.63f) + Vector3.up * (Mathf.Abs(Mathf.Sin(phase)) * pace * 0.018f);
            rig.Torso.localRotation = Quaternion.Euler(crouch * 15f + carry * 3f, 0f, Mathf.Sin(phase) * pace * 1.3f);
            rig.LeftLeg.localRotation = Quaternion.Euler(stride * (1f - crouch * 0.7f) - crouch * 78f, 0f, -crouch * 5f);
            rig.RightLeg.localRotation = Quaternion.Euler(-stride * (1f - crouch * 0.7f) - crouch * 78f, 0f, crouch * 5f);
            // Bend at actual knee pivots, keeping coveralls/boots their authored proportions while the hips lower.
            rig.LeftKnee.localRotation = Quaternion.Euler(crouch * 146f + Mathf.Max(0f, -stride) * (1f - crouch) * 0.55f, 0f, 0f);
            rig.RightKnee.localRotation = Quaternion.Euler(crouch * 146f + Mathf.Max(0f, stride) * (1f - crouch) * 0.55f, 0f, 0f);
            rig.LeftArm.localRotation = Quaternion.Euler(Mathf.Lerp(-stride * 0.55f, -40f, carry), 0f, -9f - carry * 4f);
            rig.RightArm.localRotation = Quaternion.Euler(Mathf.Lerp(stride * 0.55f, -40f, carry), 0f, 9f + carry * 4f);
            // A relaxed bend that swings a little with the stride; carrying brings both forearms up under the load.
            Elbow(rig.LeftElbow, Mathf.Lerp(-10f - Mathf.Max(0f, stride) * 0.4f, -40f, carry));
            Elbow(rig.RightElbow, Mathf.Lerp(-10f - Mathf.Max(0f, -stride) * 0.4f, -40f, carry));
            if (!holding && player != null) Emote(player.CurrentEmote(out float t), t);
            HeldTool(holding);
        }

        // 0.21.1: the gear in this worker's hand, as the crew sees it (hidden on your own body, like the rest).
        private void HeldTool(bool holding)
        {
            if (gear == null && player != null) gear = player.GetComponent<Equipment.PlayerEquipment>();
            Equipment.EquipmentDefinition inHand = !holding && gear != null && gear.IsSpawned ? gear.InHand : null;
            if (inHand != shownTool)
            {
                shownTool = inHand;
                if (heldTool != null) Destroy(heldTool);
                if (inHand != null)
                {
                    // The hand is on the forearm now (the old worker's arm was one piece).
                    bool forearm = rig.RightElbow != null;
                    heldTool = Equipment.ToolModels.Build(inHand, forearm ? rig.RightElbow : rig.RightArm);
                    heldTool.transform.localPosition = forearm ? new Vector3(0f, -0.31f, 0.03f) : new Vector3(0f, -0.55f, 0.06f);
                    heldTool.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                    if (ownerHidden)
                        foreach (Renderer r in heldTool.GetComponentsInChildren<Renderer>()) r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
                }
            }
            // Holding gear: the arm comes up to carry it in front, not swinging at the side.
            if (heldTool != null)
            {
                rig.RightArm.localRotation = Quaternion.Slerp(rig.RightArm.localRotation, Quaternion.Euler(-35f, 0f, 6f), 0.8f);
                Elbow(rig.RightElbow, -45f, 0.8f);
            }
        }

        // 0.19.5: point, wave or panic over the walking pose, eased in and out (hands must be free).
        private void Emote(Networking.Emote emote, float t)
        {
            if (emote == Networking.Emote.None) return;
            float w = Mathf.Clamp01(Mathf.Min(t, 1f - t) * 6f);
            float clock = Time.time;
            switch (emote)
            {
                case Networking.Emote.Point:
                    rig.RightArm.localRotation = Quaternion.Slerp(rig.RightArm.localRotation, Quaternion.Euler(-90f, 0f, 4f), w);
                    Elbow(rig.RightElbow, 0f, w);
                    break;
                case Networking.Emote.Wave:
                    rig.RightArm.localRotation = Quaternion.Slerp(rig.RightArm.localRotation, Quaternion.Euler(-160f, 0f, 20f), w);
                    Elbow(rig.RightElbow, -35f + Mathf.Sin(clock * 12f) * 25f, w);
                    break;
                default:
                    rig.LeftArm.localRotation = Quaternion.Slerp(rig.LeftArm.localRotation, Quaternion.Euler(-150f + Mathf.Sin(clock * 19f) * 25f, 0f, -25f), w);
                    rig.RightArm.localRotation = Quaternion.Slerp(rig.RightArm.localRotation, Quaternion.Euler(-150f + Mathf.Sin(clock * 21f + 1f) * 25f, 0f, 25f), w);
                    Elbow(rig.LeftElbow, -30f, w);
                    Elbow(rig.RightElbow, -30f, w);
                    rig.Torso.localRotation *= Quaternion.Euler(0f, Mathf.Sin(clock * 15f) * 8f * w, 0f);
                    break;
            }
        }

        private static void Elbow(Transform elbow, float bend, float weight = 1f)
        {
            if (elbow == null) return;
            elbow.localRotation = Quaternion.Slerp(elbow.localRotation, Quaternion.Euler(bend, 0f, 0f), weight);
        }

#if UNITY_EDITOR
        public void EditorSetup(WorkerRig worker, PlayerMotor movement, PlayerCarrier carrying, PlayerRagdoll falling, NetworkPlayer network)
        {
            rig = worker; motor = movement; carrier = carrying; ragdoll = falling; player = network;
        }
#endif
    }
}
