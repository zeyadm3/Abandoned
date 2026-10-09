using Abandoned.Core;
using Abandoned.Extraction;
using UnityEngine;

namespace Abandoned.Audio
{
    /// <summary>
    /// A room with a sound of its own: a duct still moving air, or water dripping into a flooded floor
    /// (character 2). The loop is only heard near the room, and once the building has started to turn
    /// something answers from inside it now and then.
    /// </summary>
    public class HorrorAreaAmbience : MonoBehaviour
    {
        [SerializeField] private Vector3 size = new(12f, 4f, 12f);
        [SerializeField] private int character;

        private const float LoopVolume = 0.24f, HearFrom = 5f;

        private SoundLoop loop;
        private float nextSound;

        private bool Flooded => character == 2;

        private void Start()
        {
            loop = GameAudio.Loop(Flooded ? SoundId.WaterDrip : SoundId.VentDrone, transform, "AreaLoop", spatial: true);
            loop.SetRange(2f, Mathf.Max(size.x, size.z) * 0.5f + 5f, AudioRolloffMode.Logarithmic);
            // Rooms don't all speak at once: each starts its own clock from where it stands.
            int room = (Mathf.RoundToInt(transform.position.x * 7f) ^ Mathf.RoundToInt(transform.position.z * 13f)) & int.MaxValue;
            nextSound = Time.time + 17f + character * 4f + room % 11;
        }

        private void Update()
        {
            Camera ear = Camera.main;
            if (ear == null) return;
            var bounds = new Bounds(transform.position, size);
            float distance = Vector3.Distance(ear.transform.position, bounds.ClosestPoint(ear.transform.position));
            float nearby = Mathf.Clamp01(1f - distance / HearFrom);
            loop.Level = LoopVolume * nearby;
            if (nearby <= 0f || Time.time < nextSound) return;
            int danger = RunState.Current != null ? RunState.Current.State.Danger : 0;
            nextSound = Time.time + Mathf.Max(8f, 28f - danger * 3f) + Random.Range(0f, 6f);
            if (danger <= 0) return;
            Vector3 above = transform.position + Vector3.up * 2f;
            float volume = 0.2f + danger * 0.025f;
            if (Flooded) GameAudio.Play(SoundId.DistantSettle, above, volume * 2f);
            else GameAudio.Play(SoundId.CeilingScuttle, above, volume);
        }

        private void OnDrawGizmos()
        {
            if (!DebugView.Visible) return;
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(transform.position, size);
        }

#if UNITY_EDITOR
        public void EditorSetup(Vector3 area, int soundCharacter)
        {
            size = area;
            character = soundCharacter;
        }
#endif
    }
}
