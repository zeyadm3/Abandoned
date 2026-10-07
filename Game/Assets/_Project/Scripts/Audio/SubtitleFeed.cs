using System;
using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Audio
{
    /// <summary>
    /// Captions for the sounds that matter (GDD 20): the building's warnings and the threats'
    /// signatures, with where they came from relative to the listener. Only sounds within hearing
    /// range are captioned, and only with Subtitles on.
    /// </summary>
    public static class SubtitleFeed
    {
        /// <summary>Caption text (with direction) for the subtitle view.</summary>
        public static event Action<string> Heard;

        public static string Caption(SoundId id) => id switch
        {
            SoundId.Creak => "floor creaks",
            SoundId.Groan => "floor groans",
            SoundId.Snap => "something snaps",
            SoundId.Crash => "FLOOR COLLAPSES",
            SoundId.BlindOneClick => "clicking",
            SoundId.CollectorJingle => "jingling footsteps",
            SoundId.Horn => "truck horn",
            SoundId.NoiseMakerShriek => "shrieking alarm",
            SoundId.RadioStatic => "radio static",
            _ => null,
        };

        internal static void Report(SoundId id, Vector3 position, float maxDistance)
        {
            if (Heard == null || !GameSettings.Subtitles) return;
            string caption = Caption(id);
            Camera ear = Camera.main;
            if (caption == null || ear == null) return;
            Vector3 to = position - ear.transform.position;
            if (to.magnitude > maxDistance) return;
            Heard($"[{caption}{Direction(ear.transform, to)}]");
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
