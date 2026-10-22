using UnityEngine;

namespace Abandoned.World
{
    /// <summary>
    /// The back of the van, drawn the same on every machine from the host's "ramp down" flag: parked, the loading ramp
    /// is out and the rear doors are swung open; with someone at the wheel the ramp slides in under the deck, the doors
    /// shut and a solid panel keeps the cargo in.
    /// </summary>
    public class VanTailgate : MonoBehaviour
    {
        [SerializeField] private Transform ramp;
        [SerializeField] private Transform leftDoor, rightDoor;
        [SerializeField] private Collider rearPanel;
        [SerializeField] private Vector3 rampOutPosition, rampInPosition;
        [SerializeField] private Vector3 rampOutEuler, rampInEuler;
        [SerializeField] private float doorOpenYaw = 105f;
        [Tooltip("Seconds to go from out to in.")]
        [SerializeField] private float seconds = 1.6f;

        private float shown = -1f;

        public bool Down => shown > .99f;

        public void Show(bool down)
        {
            float target = down ? 1f : 0f;
            if (shown < 0f) shown = target;
            else if (Mathf.Approximately(shown, target)) return;
            else shown = Mathf.MoveTowards(shown, target, Time.deltaTime / seconds);
            float t = Mathf.SmoothStep(0f, 1f, shown);
            if (ramp != null)
            {
                ramp.localPosition = Vector3.Lerp(rampInPosition, rampOutPosition, t);
                ramp.localRotation = Quaternion.Slerp(Quaternion.Euler(rampInEuler), Quaternion.Euler(rampOutEuler), t);
            }
            // The doors swing only once the ramp is nearly in, and open before it comes out.
            float doors = Mathf.Clamp01((t - .15f) / .3f);
            if (leftDoor != null) leftDoor.localRotation = Quaternion.Euler(0f, doorOpenYaw * doors, 0f);
            if (rightDoor != null) rightDoor.localRotation = Quaternion.Euler(0f, -doorOpenYaw * doors, 0f);
            if (rearPanel != null) rearPanel.enabled = t < .05f;
        }

        public void EditorSetup(Transform rampPanel, Vector3 outPosition, Vector3 outEuler, Vector3 inPosition, Vector3 inEuler,
            Transform left, Transform right, Collider panel)
        {
            ramp = rampPanel;
            rampOutPosition = outPosition;
            rampOutEuler = outEuler;
            rampInPosition = inPosition;
            rampInEuler = inEuler;
            leftDoor = left;
            rightDoor = right;
            rearPanel = panel;
        }
    }
}
