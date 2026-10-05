using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Player
{
    /// <summary>
    /// F1 overlay and gizmos for movement: speed, state, stamina, multipliers, capsule and velocity.
    /// </summary>
    public class PlayerMovementDebug : MonoBehaviour
    {
        [SerializeField] private PlayerMotor motor;
        [SerializeField] private PlayerStamina stamina;
        [SerializeField] private PlayerCameraFeel cameraFeel;
        [SerializeField] private PlayerFootsteps footsteps;
        [SerializeField] private int fontSize = 14;

        private GUIStyle style;

        private void OnGUI()
        {
            if (!DebugView.Visible) return;

            style ??= new GUIStyle(GUI.skin.box) { alignment = TextAnchor.UpperLeft, fontSize = fontSize, richText = true };

            string state = !motor.IsGrounded ? "Air" : motor.IsCrouching ? "Crouch" : motor.IsSprinting ? "Sprint" : "Walk";
            string text =
                "<b>MOVEMENT</b>\n" +
                $"State      {state}\n" +
                $"Speed      {motor.HorizontalSpeed:F2} m/s\n" +
                $"Vertical   {motor.Velocity.y:F2} m/s\n" +
                $"Grounded   {motor.IsGrounded}\n" +
                $"Height     {motor.CurrentHeight:F2} m\n" +
                $"Stamina    {stamina.Current:F0}/{stamina.Max:F0}{(stamina.IsExhausted ? "  <color=#FF6060>EXHAUSTED</color>" : "")}\n" +
                $"Speed x    {motor.SpeedMultiplier:F2}\n" +
                $"Drain x    {motor.StaminaDrainMultiplier:F2}\n" +
                $"Bob        {cameraFeel.BobOffset.y:F3}  Dip {cameraFeel.Dip:F2}  Shake {cameraFeel.Trauma:F2}\n" +
                $"Steps      {footsteps.StepCount} ({footsteps.LastSurface}, vol {footsteps.LastVolume:F2})";

            var content = new GUIContent(text);
            Vector2 size = style.CalcSize(content);
            GUI.Box(new Rect(Screen.width - size.x - 10, 10, size.x, size.y), content, style);
        }

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying || !DebugView.Visible || motor == null) return;

            Vector3 p = transform.position;
            float h = motor.CurrentHeight, r = motor.Config.Radius;
            Gizmos.color = motor.IsGrounded ? Color.green : Color.yellow;
            Gizmos.DrawWireSphere(p + Vector3.up * r, r);
            Gizmos.DrawWireSphere(p + Vector3.up * (h - r), r);
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(p + Vector3.up * 0.1f, p + Vector3.up * 0.1f + motor.Velocity * 0.25f);
        }
    }
}
