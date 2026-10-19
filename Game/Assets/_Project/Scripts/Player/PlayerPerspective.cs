using Abandoned.Audio;
using Abandoned.Core;
using Abandoned.Networking;
using Abandoned.UI;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Abandoned.Player
{
    /// <summary>
    /// Switches between first-person and third-person perspective (F5).
    /// In first-person: Cinemachine tracks Eye, body & cosmetics cast shadows only, hands are visible.
    /// In third-person: Cinemachine tracks an over-the-shoulder boom with SphereCast collision against
    /// world geometry, full body & cosmetics are rendered, first-person hands are hidden.
    /// </summary>
    public class PlayerPerspective : MonoBehaviour
    {
        [SerializeField] private CinemachineCamera playerCamera;
        [SerializeField] private Transform cameraRoot;
        [SerializeField] private Transform eye;
        [SerializeField] private WorkerPresentation presentation;
        [SerializeField] private PlayerCosmetics cosmetics;

        private NetworkPlayer me;
        private Transform cameraTarget;

        public static bool IsThirdPerson { get; private set; }
        public static event System.Action<bool> PerspectiveChanged;

        // Over-the-right-shoulder offset relative to cameraRoot (X: right, Y: up, Z: back)
        private static readonly Vector3 ThirdPersonOffset = new(0.38f, 0.12f, -1.85f);
        private const float CollisionRadius = 0.18f;
        private const float MinDistance = 0.25f;

        private void Awake()
        {
            me = GetComponent<NetworkPlayer>();
            presentation ??= GetComponent<WorkerPresentation>();
            cosmetics ??= GetComponent<PlayerCosmetics>();
            if (cameraRoot == null) cameraRoot = transform.Find("CameraRoot");
            if (eye == null && cameraRoot != null) eye = cameraRoot.Find("Eye");
            if (playerCamera == null) playerCamera = GetComponentInChildren<CinemachineCamera>(true);

            EnsureCameraTarget();
        }

        private void EnsureCameraTarget()
        {
            if (cameraTarget != null) return;
            var go = new GameObject("CameraTarget");
            cameraTarget = go.transform;
            cameraTarget.SetParent(cameraRoot != null ? cameraRoot : transform, false);
            cameraTarget.localPosition = Vector3.zero;
            cameraTarget.localRotation = Quaternion.identity;
        }

        private void OnEnable()
        {
            if (IsThirdPerson) SetThirdPerson(true);
        }

        private void OnDisable()
        {
            if (me == null || me.IsOwner)
            {
                IsThirdPerson = false;
                PerspectiveChanged?.Invoke(false);
            }
        }

        private void Update()
        {
            if (me != null && !me.IsOwner) return;
            if (CursorOwner.UiActive || ChatView.Typing) return;

            bool pressed = InputBindings.WasPressed("TogglePerspective")
                || (Keyboard.current != null && Keyboard.current.f5Key.wasPressedThisFrame);

            if (pressed)
            {
                Toggle();
            }
        }

        /// <summary>Toggles between first-person and third-person perspective.</summary>
        public void Toggle() => SetThirdPerson(!IsThirdPerson);

        /// <summary>Sets first-person (false) or third-person (true) perspective.</summary>
        public void SetThirdPerson(bool thirdPerson)
        {
            IsThirdPerson = thirdPerson;
            EnsureCameraTarget();

            if (presentation != null) presentation.ThirdPerson = thirdPerson;
            if (cosmetics != null) cosmetics.SetThirdPerson(thirdPerson);

            if (playerCamera != null)
            {
                playerCamera.Target.TrackingTarget = thirdPerson && cameraTarget != null ? cameraTarget : eye;
            }

            PerspectiveChanged?.Invoke(thirdPerson);
            GameAudio.PlayUi(SoundId.UiClick);
        }

        private void LateUpdate()
        {
            if (!IsThirdPerson || cameraTarget == null || cameraRoot == null) return;

            Vector3 pivot = cameraRoot.position;
            Vector3 desiredWorld = cameraRoot.TransformPoint(ThirdPersonOffset);
            Vector3 toTarget = desiredWorld - pivot;
            float maxDist = toTarget.magnitude;

            Vector3 targetPos = desiredWorld;
            if (maxDist > 0.01f)
            {
                Vector3 dir = toTarget / maxDist;
                int mask = ~(1 << GameLayers.PlayerLayer | 1 << GameLayers.DebrisLayer | 1 << GameLayers.LootLayer);
                if (Physics.SphereCast(pivot, CollisionRadius, dir, out RaycastHit hit, maxDist, mask, QueryTriggerInteraction.Ignore))
                {
                    float hitDist = Mathf.Max(MinDistance, hit.distance - 0.06f);
                    targetPos = pivot + dir * hitDist;
                }
            }

            cameraTarget.position = targetPos;
            // Shake from eye trauma (explosions / heavy impacts) applies to camera target rotation as well
            cameraTarget.rotation = eye != null ? cameraRoot.rotation * eye.localRotation : cameraRoot.rotation;
        }

#if UNITY_EDITOR
        public void EditorSetup(CinemachineCamera cam, Transform root, Transform eyeTransform, WorkerPresentation pres, PlayerCosmetics cos)
        {
            playerCamera = cam;
            cameraRoot = root;
            eye = eyeTransform;
            presentation = pres;
            cosmetics = cos;
        }
#endif
    }
}
