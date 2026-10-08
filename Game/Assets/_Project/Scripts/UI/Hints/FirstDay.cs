using Abandoned.Core;
using UnityEngine;

namespace Abandoned.UI
{
    /// <summary>
    /// Whether this player has finished the guided first job (<see cref="FirstDayGuide"/>). Local, in Prefs,
    /// like tips; "Show them again" in Settings replays it.
    /// </summary>
    public static class FirstDay
    {
        private const string DoneKey = "firstday.done";

        public static bool Done => Prefs.GetInt(DoneKey, 0) == 1;

        /// <summary>The guide is on screen right now.</summary>
        public static bool Active { get; internal set; }

        public static void MarkDone()
        {
            Prefs.SetInt(DoneKey, 1);
            Prefs.Save();
            Active = false;
        }

        public static void Reset() => Prefs.Delete(DoneKey);

        /// <summary>Tips that say what the guide is already saying stay quiet while it is on screen.</summary>
        public static bool Covers(HintId id) =>
            Active && (id == HintId.HqBoard || id == HintId.HqVan || id == HintId.RunGoal || id == HintId.PickUp);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Active = false;
    }
}
