using UnityEngine;

namespace Abandoned.Core
{
    /// <summary>
    /// Early Access links (M10.11): the community Discord and a feedback form. Empty = the button is
    /// hidden, so a build never shows a dead link. Set them once the server and form exist.
    /// </summary>
    [CreateAssetMenu(menuName = "Abandoned/Core/Launch Config", fileName = "LaunchConfig")]
    public class LaunchConfig : ScriptableObject
    {
        public const string ResourcePath = "LaunchConfig";

        [field: Tooltip("Discord invite (https://discord.gg/...).")]
        [field: SerializeField] public string DiscordUrl { get; private set; } = "";
        [field: Tooltip("Bug report / feedback form (Steam discussions, a Google form...).")]
        [field: SerializeField] public string FeedbackUrl { get; private set; } = "";
    }
}
