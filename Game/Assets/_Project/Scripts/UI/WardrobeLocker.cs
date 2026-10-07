using Abandoned.Core;
using UnityEngine;

namespace Abandoned.UI
{
    /// <summary>HQ: the crew's lockers (E) open the wardrobe (coveralls and hats).</summary>
    public class WardrobeLocker : MonoBehaviour, IUsable
    {
        public string UsePrompt(GameObject user) => MenuUi.Current != null ? "Change clothes" : null;

        public void Use(GameObject user) => MenuUi.Current?.OpenWardrobe();
    }
}
