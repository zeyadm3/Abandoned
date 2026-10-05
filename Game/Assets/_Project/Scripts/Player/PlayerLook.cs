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

        private void OnEnable()
        {
            yaw = transform.eulerAngles.y;
            pitch = 0f;
            SetCursorCaptured(true);
        }

        private void OnDisable() => SetCursorCaptured(false);

        private void Update()
        {
            PlayerInputFrame input = inputReader.Current;
            if (input.PausePressed) SetCursorCaptured(false);
            else if (!CursorCaptured && input.UsePressed) SetCursorCaptured(true);

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

        private static void SetCursorCaptured(bool captured)
        {
            Cursor.lockState = captured ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !captured;
        }
    }
}
