using Abandoned.Core;
using Abandoned.UI;

namespace Abandoned.Tutorial
{
    /// <summary>
    /// Whether this player has finished the training job. Local, in Prefs, like tips: it only decides whether the
    /// HQ mentions the training. Finishing it also counts as the guided first day (<see cref="FirstDay"/>), whose
    /// lessons it already covers.
    /// </summary>
    public static class TutorialProgress
    {
        private const string DoneKey = "tutorial.done";

        public static bool Done => Prefs.GetInt(DoneKey, 0) == 1;

        public static void MarkDone()
        {
            Prefs.SetInt(DoneKey, 1);
            Prefs.Save();
            if (!FirstDay.Done) FirstDay.MarkDone();
        }

        public static void Reset() => Prefs.Delete(DoneKey);
    }
}
