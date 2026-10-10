namespace Abandoned.Core
{
    /// <summary>Settings added from 0.19.7 on (each one key in Prefs, raises <see cref="GameSettings.Changed"/>).</summary>
    public static partial class GameSettings
    {
        private static bool? arachnophobia, streamerMode;

        /// <summary>0.19.7: the Crawlers are drawn as scurrying lumps of dust and grit, not legs.</summary>
        public static bool Arachnophobia
        {
            get => arachnophobia ??= Prefs.GetInt("settings.arachnophobia", 0) == 1;
            set => Set(ref arachnophobia, "settings.arachnophobia", value);
        }

        /// <summary>0.19.7: other players' names read "Crewmate N", and addresses and ports are hidden.</summary>
        public static bool StreamerMode
        {
            get => streamerMode ??= Prefs.GetInt("settings.streamer", 0) == 1;
            set => Set(ref streamerMode, "settings.streamer", value);
        }

        private static float? padLookSpeed;

        /// <summary>0.19.8: how fast the right stick turns the view at full tilt (mouse-pixels a second).</summary>
        public static float PadLookSpeed
        {
            get => padLookSpeed ??= Prefs.GetFloat("settings.padlook", 900f);
            set => Set(ref padLookSpeed, "settings.padlook", value);
        }

        // @@settings
    }
}
