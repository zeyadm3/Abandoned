using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Abandoned.Player
{
    /// <summary>
    /// A free-flying camera for trailer shots (M10.11, F9; pair with F10 clip mode): it takes over the view
    /// from where you are, your player stands still, and the game carries on around you. Local only:
    /// nobody else sees anything. Mouse looks, WASD flies, Q/E down/up, Shift fast, the wheel sets speed.
    /// </summary>
    public class FreeCamera : MonoBehaviour
    {
        private static FreeCamera instance;

        private CinemachineCamera cam;
        private float yaw, pitch, speed = 3f;

        public static bool Active => instance != null;

        public static void Toggle()
        {
            if (instance != null)
            {
                Destroy(instance.gameObject);
                return;
            }
            Camera main = Camera.main;
            if (main == null) return;
            var go = new GameObject("FreeCamera");
            go.transform.SetPositionAndRotation(main.transform.position, main.transform.rotation);
            instance = go.AddComponent<FreeCamera>();
            instance.cam.Lens.FieldOfView = main.fieldOfView;
        }

        private void Awake()
        {
            cam = gameObject.AddComponent<CinemachineCamera>();
            cam.Priority = 1000;
            Vector3 e = transform.eulerAngles;
            yaw = e.y;
            pitch = e.x > 180f ? e.x - 360f : e.x;
            Freeze(true);
        }

        private void OnDestroy()
        {
            if (instance == this) instance = null;
            Freeze(false);
        }

        // Every player on this machine stands still while we fly (only ours reads devices anyway).
        private static void Freeze(bool frozen)
        {
            foreach (PlayerInputReader reader in FindObjectsByType<PlayerInputReader>(FindObjectsSortMode.None))
                if (reader != null) reader.Override = frozen ? default(PlayerInputFrame) : null;
        }

        private void Update()
        {
            Keyboard k = Keyboard.current;
            Mouse m = Mouse.current;
            if (m != null)
            {
                Vector2 look = m.delta.ReadValue() * 0.08f * Core.GameSettings.Sensitivity;
                yaw += look.x;
                pitch = Mathf.Clamp(pitch - look.y, -89f, 89f);
                float wheel = m.scroll.ReadValue().y;
                if (Mathf.Abs(wheel) > 0.01f) speed = Mathf.Clamp(speed * (wheel > 0f ? 1.15f : 1f / 1.15f), 0.2f, 40f);
            }
            transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
            if (k == null) return;
            Vector3 move = Vector3.zero;
            if (k.wKey.isPressed) move += Vector3.forward;
            if (k.sKey.isPressed) move += Vector3.back;
            if (k.dKey.isPressed) move += Vector3.right;
            if (k.aKey.isPressed) move += Vector3.left;
            Vector3 world = transform.rotation * move;
            if (k.eKey.isPressed) world += Vector3.up;
            if (k.qKey.isPressed) world += Vector3.down;
            float boost = k.leftShiftKey.isPressed ? 4f : 1f;
            transform.position += world * (speed * boost * Time.unscaledDeltaTime);
        }
    }
}
