using UnityEngine;

namespace Abandoned.Core
{
    /// <summary>
    /// One achievement (M9.5): unlocked when a play stat reaches a threshold. A new achievement is a new
    /// asset in the catalog, no code (unless it needs a stat nobody counts yet).
    /// </summary>
    [CreateAssetMenu(menuName = "Abandoned/Core/Achievement", fileName = "Achievement")]
    public class AchievementDefinition : ScriptableObject
    {
        [field: SerializeField] public string Id { get; private set; }
        [field: SerializeField] public string DisplayName { get; private set; }
        [field: SerializeField, TextArea] public string Description { get; private set; }
        [field: Tooltip("The stat it watches (see Achievements.Stat* names).")]
        [field: SerializeField] public string Stat { get; private set; }
        [field: SerializeField, Min(1)] public long Threshold { get; private set; } = 1;
        [field: Tooltip("Its API name in Steamworks (set up on the partner site with the real App ID).")]
        [field: SerializeField] public string SteamName { get; private set; }

#if UNITY_EDITOR
        public void EditorSetup(string id, string displayName, string description, string stat, long threshold, string steamName)
        {
            Id = id;
            DisplayName = displayName;
            Description = description;
            Stat = stat;
            Threshold = threshold;
            SteamName = steamName;
        }
#endif
    }
}
