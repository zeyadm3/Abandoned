using UnityEngine;

namespace Abandoned.Core
{
    /// <summary>Scene-owned camera reach, applied after Cinemachine selects the player, ghost or photo lens.</summary>
    [DefaultExecutionOrder(10000)]
    public sealed class LevelCameraRange : MonoBehaviour
    {
        [SerializeField, Min(100f)] private float farClip = 5000f;
        private Camera eye;
        private float originalFarClip;

        private void LateUpdate()
        {
            Camera main = Camera.main;
            if (main != eye)
            {
                Restore();
                eye = main;
                if (eye != null) originalFarClip = eye.farClipPlane;
            }
            if (eye != null) eye.farClipPlane = Mathf.Max(eye.farClipPlane, farClip);
        }

        private void OnDisable() => Restore();

        private void Restore()
        {
            if (eye != null && Mathf.Approximately(eye.farClipPlane, farClip)) eye.farClipPlane = originalFarClip;
            eye = null;
        }

        private void OnGUI()
        {
            if (!DebugView.Visible || eye == null) return;
            GUI.Label(new Rect(10f, Screen.height - 50f, 420f, 20f), $"VIEW {eye.farClipPlane:0} m  {gameObject.scene.name}");
        }

#if UNITY_EDITOR
        public void EditorSetup(float distance) => farClip = Mathf.Max(100f, distance);
#endif
    }
}
