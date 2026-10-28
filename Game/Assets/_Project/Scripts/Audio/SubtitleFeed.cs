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
        /// <summary>Caption text for the subtitle view (who, never where).</summary>
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
            // The catalog keeps captions in English; they are worded here, in the listener's language (0.24.8).
            string line = Core.Loc.T(caption);
            if (info.Bus == AudioBus.Threats) line = Core.Loc.T("MONSTER: {0}", line);
            else if (info.Bus == AudioBus.Structure) line = Core.Loc.T("BUILDING: {0}", line);
            Heard($"[{line}]");
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Heard = null;
    }
}
