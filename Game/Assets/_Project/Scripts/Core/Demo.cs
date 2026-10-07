using UnityEngine;

namespace Abandoned.Core
{
    /// <summary>
    /// Is this the demo build, and is a company's demo over? The flavour comes from BuildInfo (stamped
    /// by the Demo build); tests and the editor can force it.
    /// </summary>
    public static class Demo
    {
        private static bool? forced;
        private static int? forcedMaxJobs;
        private static DemoConfig config;

        public static bool IsDemo => forced ?? (VersionInfo.Build != null && VersionInfo.Build.Flavor == "Demo");

        public static DemoConfig Config => config != null ? config : config = Resources.Load<DemoConfig>(DemoConfig.ResourcePath);

        public static int MaxJobs => forcedMaxJobs ?? (Config != null ? Config.MaxJobs : 5);

        /// <summary>A demo company that has done every job the demo allows.</summary>
        public static bool IsOver(int jobsDone) => IsDemo && jobsDone >= MaxJobs;

        /// <summary>Tests/editor: act as (or not as) the demo, optionally with another job limit. Null = the build's own.</summary>
        public static void Force(bool? demo, int? maxJobs = null)
        {
            forced = demo;
            forcedMaxJobs = maxJobs;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            forced = null;
            forcedMaxJobs = null;
            config = null;
        }
    }
}
