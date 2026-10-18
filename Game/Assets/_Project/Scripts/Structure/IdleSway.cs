using UnityEngine;

namespace Abandoned.Structure
{
    /// <summary>
    /// A slow, uneven sway for something hanging on long cables (the Iron Factory's crucible over the lava). Two
    /// out-of-step sines, so it never settles into a clean metronome. Cosmetic, every machine its own.
    /// </summary>
    public class IdleSway : MonoBehaviour
    {
        [SerializeField, Range(0f, 10f)] private float degrees = 2.5f;
        [SerializeField, Min(.5f)] private float period = 7f;

        private Quaternion rest;

        private void Awake() => rest = transform.localRotation;

        private void Update()
        {
            float t = Time.time * Mathf.PI * 2f / period;
            transform.localRotation = rest * Quaternion.Euler(Mathf.Sin(t) * degrees, 0f, Mathf.Sin(t * .63f + 1.1f) * degrees * .6f);
        }

#if UNITY_EDITOR
        public void EditorSetup(float swayDegrees, float swayPeriod)
        {
            degrees = swayDegrees;
            period = swayPeriod;
        }
#endif
    }
}
