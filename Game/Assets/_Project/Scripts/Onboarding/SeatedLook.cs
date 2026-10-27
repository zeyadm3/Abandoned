using UnityEngine;
using UnityEngine.InputSystem;

namespace Abandoned.Onboarding
{
    /// <summary>The seated first-person shot: look around within limits (you can't get up in an interview).</summary>
    public class SeatedLook : MonoBehaviour
    {
        [SerializeField] private float yawLimit = 55f, pitchLimit = 32f, sensitivity = .08f;

        private Quaternion rest;
        private float yaw, pitch;

        /// <summary>The director hands the mouse to replies; the look holds still meanwhile.</summary>
        public bool Frozen { get; set; }

        private void Awake() => rest = transform.localRotation;

        private void OnEnable()
        {
            yaw = pitch = 0f;
            transform.localRotation = rest;
        }

        private void Update()
        {
            if (Frozen || Mouse.current == null) return;
            Vector2 d = Mouse.current.delta.ReadValue() * sensitivity;
            if (Gamepad.current != null) d += Gamepad.current.rightStick.ReadValue() * 90f * Time.deltaTime;
            yaw = Mathf.Clamp(yaw + d.x, -yawLimit, yawLimit);
            pitch = Mathf.Clamp(pitch - d.y, -pitchLimit, pitchLimit);
            // Eases back toward the desk when left alone.
            if (d.sqrMagnitude < .0001f)
            {
                yaw = Mathf.MoveTowards(yaw, 0f, 6f * Time.deltaTime);
                pitch = Mathf.MoveTowards(pitch, 0f, 4f * Time.deltaTime);
            }
            transform.localRotation = rest * Quaternion.Euler(pitch, yaw, 0f);
        }
    }
}
