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

        public float Pitch => pitch;

        public bool CursorCaptured => Cursor.lockState == CursorLockMode.Locked;

        /// <summary>Frame the cursor was last captured on; that click shouldn't also act in the game.</summary>
        public int CaptureFrame { get; private set; } = -1;

        private void Start() => SetCaptured(true);

        // Re-read yaw (the body may have been moved, e.g. by ragdoll recovery) but keep pitch, and
        // never touch the cursor here: being disabled while ragdolled is not pausing.
        private void OnEnable() => yaw = transform.eulerAngles.y;

        private void OnDestroy()
        {
            if (CursorCaptured) SetCursorCaptured(false);
        }

        private void Update()
        {
            PlayerInputFrame input = inputReader.Current;
            if (input.PausePressed) SetCaptured(false);
            else if (!CursorCaptured && input.UsePressed) SetCaptured(true);

            if (CursorCaptured) ApplyLook(input.Look);
        }

        /// <summary>Applies a mouse delta (already a per-frame distance, so no deltaTime).</summary>
        public void ApplyLook(Vector2 delta)
        {
            yaw += delta.x * config.MouseSensitivity;
            pitch = Mathf.Clamp(pitch - delta.y * config.MouseSensitivity, -config.MaxPitch, config.MaxPitch);

            transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            cameraRoot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }

        private void SetCaptured(bool captured)
        {
            if (captured) CaptureFrame = Time.frameCount;
            SetCursorCaptured(captured);
        }

        private static void SetCursorCaptured(bool captured)
        {
            Cursor.lockState = captured ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !captured;
        }
    }
}
