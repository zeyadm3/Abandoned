namespace Abandoned.Audio
{
    /// <summary>
    /// The mix groups (0.14.0). Each sound belongs to one; <see cref="AudioMix"/> turns the player's
    /// volume settings, the config's trims and the ducking into one gain per bus.
    /// </summary>
    public enum AudioBus
    {
        Effects,
        Footsteps,
        /// <summary>The building's warnings. Never ducked and never dulled by walls: they are information.</summary>
        Structure,
        Threats,
        Ambience,
        Music,
        Ui,
        /// <summary>The player's own heartbeat and breath, under the Vitals setting.</summary>
        Vitals,
    }
}
