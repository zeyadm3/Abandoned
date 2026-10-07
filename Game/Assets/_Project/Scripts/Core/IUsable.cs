using UnityEngine;

namespace Abandoned.Core
{
    /// <summary>
    /// Something a player presses E on that isn't loot: the truck's ignition, later doors and switches.
    /// <see cref="Use"/> runs on the user's own machine; networked implementations ask the host.
    /// </summary>
    public interface IUsable
    {
        /// <summary>The HUD's "[E] ..." text, or null when it can't be used right now.</summary>
        string UsePrompt(GameObject user);

        void Use(GameObject user);
    }
}
