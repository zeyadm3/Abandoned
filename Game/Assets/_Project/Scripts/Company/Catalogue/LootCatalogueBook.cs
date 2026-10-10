using Abandoned.Core;
using Abandoned.Loot;
using Abandoned.UI;
using UnityEngine;

namespace Abandoned.Company
{
    /// <summary>
    /// The HQ's loot catalogue (0.18.7): a ledger on a lectern; E opens <see cref="LootCatalogueScreen"/>
    /// on this machine. Every kind of loot the company has ever sold is filled in; the rest are blank lines.
    /// </summary>
    public class LootCatalogueBook : MonoBehaviour, IUsable, IHintSource
    {
        [SerializeField] private LootCatalog catalog;

        public static bool Open { get; set; }
        public static LootCatalog Catalog { get; private set; }
        public HintId Hint => HintId.Catalogue;

        private void OnEnable() => Catalog = catalog;

        public string UsePrompt(GameObject user) => "Read the loot catalogue";

        public void Use(GameObject user)
        {
            Catalog = catalog;
            Open = true;
        }

#if UNITY_EDITOR
        public void EditorSetup(LootCatalog loot) => catalog = loot;
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Open = false;
            Catalog = null;
        }
    }
}
