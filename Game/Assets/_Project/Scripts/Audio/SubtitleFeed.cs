using System;
using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Audio
{
    /// <summary>
    /// Captions for the sounds that matter (GDD 20): the building's warnings and the threats'
    /// signatures. The text for each sound is in <see cref="SoundCatalog"/>. Only sounds within hearing range are captioned, and only with Subtitles on, which is an
    /// off-by-default accessibility option. Never says where a sound came from (user, 2026-10-08: dread).
    /// </summary>
    public static class SubtitleFeed
    {
        /// <summary>Caption text (with direction) for the subtitle view.</summary>
        public static event Action<string> Heard;

        public static string Caption(SoundId id) => SoundCatalog.Info(id).Caption;

        /// <summary>A flat sound (a siren in everyone's ears) is always in range.</summary>
        internal static void Report(SoundId id, Vector3 position, float maxDistance, bool flat)
        {
            if (Heard == null || !GameSettings.Subtitles) return;
            string caption = Caption(id);
            if (caption == null) return;
            if (!flat)
            {
                Camera ear = Camera.main;
                if (ear == null || (position - ear.transform.position).magnitude > maxDistance) return;
            }
            // 0.23.1: who's making it (never where).
            SoundInfo info = SoundCatalog.Info(id);
            string who = info.Bus == AudioBus.Threats ? "MONSTER: " : info.Bus == AudioBus.Structure ? "BUILDING: " : "";
            Heard($"[{who}{caption}]");
        }

        /// <summary>" - left", " - behind" ... from the listener's facing; nothing when it's right here.</summary>
        public static string Direction(Transform ear, Vector3 to)
        {
            Vector3 flat = new(to.x, 0f, to.z);
            if (flat.magnitude < 2f) return to.y > 2f ? " - above" : to.y < -2f ? " - below" : "";
            float angle = Vector3.SignedAngle(new Vector3(ear.forward.x, 0f, ear.forward.z), flat, Vector3.up);
            string side = Mathf.Abs(angle) <= 45f ? "ahead" : Mathf.Abs(angle) >= 135f ? "behind" : angle > 0f ? "right" : "left";
            return $" - {side}" + (to.y > 2.5f ? ", above" : to.y < -2.5f ? ", below" : "");
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Heard = null;
    }
}
