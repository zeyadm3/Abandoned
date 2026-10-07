using Abandoned.Core;
using Unity.Cinemachine;
using UnityEngine;

namespace Abandoned.Player
{
    /// <summary>The player's camera takes the field of view from their settings, live.</summary>
    public class PlayerFieldOfView : MonoBehaviour
    {
        [SerializeField] private CinemachineCamera playerCamera;

        private void OnEnable()
        {
            GameSettings.Changed += Apply;
            Apply();
        }

        private void OnDisable() => GameSettings.Changed -= Apply;

        private void Apply()
        {
            if (playerCamera == null) return;
            LensSettings lens = playerCamera.Lens;
            lens.FieldOfView = GameSettings.FieldOfView;
            playerCamera.Lens = lens;
        }
    }
}
