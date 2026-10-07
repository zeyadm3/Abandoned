using UnityEngine;

namespace Abandoned.Core
{
    /// <summary>The demo build's rules (M8.3): how many jobs it lets a company take, and where "wishlist" goes.</summary>
    [CreateAssetMenu(menuName = "Abandoned/Core/Demo Config", fileName = "DemoConfig")]
    public class DemoConfig : ScriptableObject
    {
        public const string ResourcePath = "DemoConfig";

        [field: Tooltip("Jobs a demo company can take (about 8-12 minutes each with the HQ in between).")]
        [field: SerializeField, Min(1)] public int MaxJobs { get; private set; } = 5;
        [field: Tooltip("The game's Steam App ID for the store overlay; 0 until the real one exists (then only the URL is used).")]
        [field: SerializeField] public uint StoreAppId { get; private set; }
        [field: Tooltip("Opened in the browser when the Steam overlay isn't available.")]
        [field: SerializeField] public string StoreUrl { get; private set; } = "https://store.steampowered.com/search/?term=abandoned";
    }
}
