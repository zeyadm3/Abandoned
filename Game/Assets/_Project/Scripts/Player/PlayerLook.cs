using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Player
{
    /// <summary>
    /// Mouse look: yaw turns the whole body, pitch tilts the camera root. Pause (Esc) frees the
    /// cursor; clicking in the game view captures it again.
    /// </summary>
    [DefaultExecutionOrder(-50)] // After PlayerInputReader, before PlayerMotor uses transform.forward.
    public class PlayerLook : MonoBehaviour
    {
        [SerializeField] private PlayerMovementConfig config;
        [SerializeField] private PlayerInputReader inputReader;
        [SerializeField] private Transform cameraRoot;

        private float yaw;
        private float pitch;
        // Only the look that captured the cursor may free it: a remote player leaving mustn't unlock ours.
        private bool ownsCursor;

        public float Pitch => pitch;

        // Batch mode (tests, nettests) has no cursor and lockState never sticks: go by what this look asked for.
        public bool CursorCaptured => Application.isBatchMode ? ownsCursor : Cursor.lockState == CursorLockMode.Locked;

        /// <summary>Frame the cursor was last captured on; that click shouldn't also act in the game.</summary>
        public int CaptureFrame { get; private set; } = -1;

        private void Start()
        {
            // NGO sets the spawn pose after Instantiate (so after OnEnable read yaw): pick it up here too.
            SyncYawFromTransform();
            SetCaptured(true);
        }

        /// <summary>Another machine's copy: tilt the camera root (and the flashlight on it) to the owner's pitch.</summary>
        public void ApplyRemotePitch(float remotePitch)
        {
            if (cameraRoot != null) cameraRoot.localRotation = Quaternion.Euler(remotePitch, 0f, 0f);
        }

        /// <summary>Takes yaw from the body's current facing, e.g. after a network spawn placed it.</summary>
        public void SyncYawFromTransform() => yaw = transform.eulerAngles.y;

        // Re-read yaw (the body may have been moved, e.g. by ragdoll recovery) but keep pitch, and
        // never touch the cursor here: being disabled while ragdolled is not pausing.
        private void OnEnable() => SyncYawFromTransform();

        private void OnDestroy()
        {
            if (ownsCursor && CursorCaptured) SetCursorCaptured(false);
        }

        private void Update()
        {
            PlayerInputFrame input = inputReader.Current;
            if (CursorOwner.UiActive)
            {
                if (CursorCaptured) SetCaptured(false);
                return;
            }
            if (input.PausePressed) SetCaptured(false);
            else if (!CursorCaptured && (input.UsePressed || CursorOwner.ConsumeCaptureRequest())) SetCaptured(true);

            if (CursorCaptured) ApplyLook(input.Look);
        }

        /// <summary>Applies a mouse delta (already a per-frame distance, so no deltaTime).</summary>
        public void ApplyLook(Vector2 delta)
        {
            float sensitivity = config.MouseSensitivity * GameSettings.Sensitivity;
            yaw += delta.x * sensitivity;
            pitch = Mathf.Clamp(pitch - delta.y * sensitivity, -config.MaxPitch, config.MaxPitch);

            transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            cameraRoot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }

        private void SetCaptured(bool captured)
        {
            if (captured) CaptureFrame = Time.frameCount;
            ownsCursor = captured;
            SetCursorCaptured(captured);
        }

        private static void SetCursorCaptured(bool captured)
        {
            Cursor.lockState = captured ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !captured;
        }
    }
}
