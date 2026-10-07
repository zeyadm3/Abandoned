using System.Collections.Generic;
using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Player
{
    /// <summary>Every coverall and hat in a fixed order: the order is the network index, so only append.</summary>
    [CreateAssetMenu(menuName = "Abandoned/Player/Cosmetic Catalog", fileName = "CosmeticCatalog")]
    public class CosmeticCatalog : ScriptableObject, IValidatable
    {
        [SerializeField] private List<CosmeticDefinition> coveralls = new();
        [SerializeField] private List<CosmeticDefinition> hats = new();
        [Tooltip("Index of the hat a new player wears.")]
        [SerializeField] private int defaultHat = 1;

        public IReadOnlyList<CosmeticDefinition> Coveralls => coveralls;
        public IReadOnlyList<CosmeticDefinition> Hats => hats;
        public int DefaultHat => defaultHat;

        public CosmeticDefinition Coverall(int index) => index >= 0 && index < coveralls.Count ? coveralls[index] : coveralls.Count > 0 ? coveralls[0] : null;

        public CosmeticDefinition Hat(int index) => index >= 0 && index < hats.Count ? hats[index] : null;

        public static int IndexOf(IReadOnlyList<CosmeticDefinition> list, string id)
        {
            for (int i = 0; i < list.Count; i++) if (list[i] != null && list[i].Id == id) return i;
            return -1;
        }

        /// <summary>What this player wears: their saved picks, if still unlocked, else the defaults.</summary>
        public CosmeticChoice FromProfile()
        {
            int c = IndexOf(coveralls, PlayerProfile.Coverall), h = IndexOf(hats, PlayerProfile.Hat);
            if (c < 0 || !PlayerProfile.Unlocked(coveralls[c])) c = 0;
            if (h < 0 || !PlayerProfile.Unlocked(hats[h])) h = Mathf.Clamp(defaultHat, 0, Mathf.Max(0, hats.Count - 1));
            return new CosmeticChoice((byte)c, (byte)h);
        }

        public void Validate(List<string> errors)
        {
            if (coveralls.Count == 0 || coveralls.Count > 255 || hats.Count > 255) errors.Add($"{name}: needs 1-255 coveralls and at most 255 hats (byte indices).");
            var ids = new HashSet<string>();
            foreach (CosmeticDefinition d in coveralls)
                if (d == null || d.Kind != CosmeticKind.Coverall || !ids.Add("c:" + d.Id)) errors.Add($"{name}: a coverall slot is empty, not a coverall, or a duplicate id.");
            foreach (CosmeticDefinition d in hats)
            {
                if (d == null || d.Kind != CosmeticKind.Hat || !ids.Add("h:" + d.Id)) errors.Add($"{name}: a hat slot is empty, not a hat, or a duplicate id.");
                else if (d.HatPrefab == null && d.Id != "none") errors.Add($"{name}: hat '{d.Id}' has no model.");
            }
            if (defaultHat < 0 || defaultHat >= hats.Count) errors.Add($"{name}: the default hat isn't in the list.");
        }

#if UNITY_EDITOR
        public void EditorSet(List<CosmeticDefinition> coverallList, List<CosmeticDefinition> hatList, int defaultHatIndex)
        {
            coveralls = coverallList;
            hats = hatList;
            defaultHat = defaultHatIndex;
        }
#endif
    }
}
