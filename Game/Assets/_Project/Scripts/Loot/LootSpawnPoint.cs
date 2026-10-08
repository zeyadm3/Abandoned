using Abandoned.Interaction;
using UnityEngine;

namespace Abandoned.Loot
{
    /// <summary>
    /// A place loot may appear in a run: on the floor at this point, with the store kind it belongs to
    /// (matched against <see cref="LootDefinition.SpawnTags"/>) and the carry classes that fit here.
    /// Jackpot points (GDD 7.4: upper floors, behind weak structure) only ever take jackpot items.
    /// </summary>
    public class LootSpawnPoint : MonoBehaviour
    {
        [SerializeField] private string spawnTag = "concourse";
        [SerializeField] private CarryClass minClass = CarryClass.Pocket;
        [SerializeField] private CarryClass maxClass = CarryClass.TwoHand;
        [SerializeField] private bool jackpot;
        [Tooltip("Optional authored item IDs, empty for normal tag-based scatter.")]
        [SerializeField] private string[] allowedDefinitionIds = System.Array.Empty<string>();
        [Tooltip("Shares the limited featured-site roll with jackpots, without changing the item's Jackpot flag.")]
        [SerializeField] private bool featured;

        public string Tag => spawnTag;
        public CarryClass MinClass => minClass;
        public CarryClass MaxClass => maxClass;
        public bool IsJackpot => jackpot;
        public bool IsFeatured => featured;

        public bool Fits(LootDefinition d) =>
            d != null && d.Jackpot == jackpot && d.CarryClass >= minClass && d.CarryClass <= maxClass && d.HasSpawnTag(spawnTag)
            && (allowedDefinitionIds == null || allowedDefinitionIds.Length == 0 || System.Array.IndexOf(allowedDefinitionIds, d.Id) >= 0);

        public void EditorSetup(string spawnTagValue, CarryClass min, CarryClass max, bool isJackpot,
            string[] allowedIds = null, bool isFeatured = false)
        {
            spawnTag = spawnTagValue;
            minClass = min;
            maxClass = max;
            jackpot = isJackpot;
            allowedDefinitionIds = allowedIds ?? System.Array.Empty<string>();
            featured = isFeatured;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = jackpot ? new Color(1f, 0.8f, 0.1f) : maxClass >= CarryClass.Heavy ? new Color(1f, 0.4f, 0.2f) : new Color(0.3f, 1f, 0.4f);
            Gizmos.DrawWireCube(transform.position + Vector3.up * 0.25f, new Vector3(0.5f, 0.5f, 0.5f));
        }
    }
}
