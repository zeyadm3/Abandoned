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
        [Tooltip("Index 0 should be \"none\" (what a new player wears).")]
        [SerializeField] private List<CosmeticDefinition> accessories = new();
        [Tooltip("Index of the hat a new player wears.")]
        [SerializeField] private int defaultHat = 1;
        [Tooltip("2026 remake: the eight workers (index 0 is the default).")]
        [SerializeField] private List<CosmeticDefinition> characters = new();
        [Tooltip("2026 remake: outfits (index 0 is the company issue every worker starts in).")]
        [SerializeField] private List<CosmeticDefinition> outfits = new();

        public IReadOnlyList<CosmeticDefinition> Coveralls => coveralls;
        public IReadOnlyList<CosmeticDefinition> Hats => hats;
        public IReadOnlyList<CosmeticDefinition> Accessories => accessories;
        public int DefaultHat => defaultHat;
        public IReadOnlyList<CosmeticDefinition> Characters => characters;
        public IReadOnlyList<CosmeticDefinition> Outfits => outfits;

        public CosmeticDefinition Character(int index) => index >= 0 && index < characters.Count ? characters[index] : characters.Count > 0 ? characters[0] : null;

        public CosmeticDefinition Outfit(int index) => index >= 0 && index < outfits.Count ? outfits[index] : outfits.Count > 0 ? outfits[0] : null;

        public IReadOnlyList<CosmeticDefinition> Of(CosmeticKind kind) => kind switch
        {
            CosmeticKind.Coverall => coveralls,
            CosmeticKind.Hat => hats,
            CosmeticKind.Accessory => accessories,
            CosmeticKind.Character => characters,
            _ => outfits,
        };

        public CosmeticDefinition Get(CosmeticKind kind, int index) => kind switch
        {
            CosmeticKind.Coverall => Coverall(index),
            CosmeticKind.Hat => Hat(index),
            CosmeticKind.Accessory => Accessory(index),
            CosmeticKind.Character => Character(index),
            _ => Outfit(index),
        };

        public CosmeticDefinition Coverall(int index) => index >= 0 && index < coveralls.Count ? coveralls[index] : coveralls.Count > 0 ? coveralls[0] : null;

        public CosmeticDefinition Hat(int index) => index >= 0 && index < hats.Count ? hats[index] : null;

        public CosmeticDefinition Accessory(int index) => index >= 0 && index < accessories.Count ? accessories[index] : null;

        public static int IndexOf(IReadOnlyList<CosmeticDefinition> list, string id)
        {
            for (int i = 0; i < list.Count; i++) if (list[i] != null && list[i].Id == id) return i;
            return -1;
        }

        /// <summary>What this player wears: their saved picks, if still unlocked, else the defaults.</summary>
        public CosmeticChoice FromProfile()
        {
            int c = IndexOf(coveralls, PlayerProfile.Coverall), h = IndexOf(hats, PlayerProfile.Hat), a = IndexOf(accessories, PlayerProfile.Accessory);
            if (c < 0 || !PlayerProfile.Unlocked(coveralls[c])) c = 0;
            if (h < 0 || !PlayerProfile.Unlocked(hats[h])) h = Mathf.Clamp(defaultHat, 0, Mathf.Max(0, hats.Count - 1));
            if (a < 0 || !PlayerProfile.Unlocked(accessories[a])) a = 0;
            int ch = IndexOf(characters, PlayerProfile.Character), o = IndexOf(outfits, PlayerProfile.Outfit);
            if (ch < 0 || !PlayerProfile.Unlocked(characters[ch])) ch = 0;
            if (o < 0 || !PlayerProfile.Unlocked(outfits[o])) o = 0;
            return new CosmeticChoice((byte)c, (byte)h, (byte)a, (byte)ch, (byte)o);
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
            if (accessories.Count > 255) errors.Add($"{name}: at most 255 accessories (byte indices).");
            foreach (CosmeticDefinition d in accessories)
            {
                if (d == null || d.Kind != CosmeticKind.Accessory || !ids.Add("a:" + d.Id)) errors.Add($"{name}: an accessory slot is empty, not an accessory, or a duplicate id.");
                else if (d.HatPrefab == null && d.Id != "none") errors.Add($"{name}: accessory '{d.Id}' has no model.");
            }
            if (characters.Count > 255 || outfits.Count > 255) errors.Add($"{name}: at most 255 characters and outfits (byte indices).");
            foreach (CosmeticDefinition d in characters)
                if (d == null || d.Kind != CosmeticKind.Character || !ids.Add("ch:" + d.Id) || d.LookMesh == null)
                    errors.Add($"{name}: a character slot is empty, not a character, a duplicate id, or has no head mesh.");
            foreach (CosmeticDefinition d in outfits)
                if (d == null || d.Kind != CosmeticKind.Outfit || !ids.Add("o:" + d.Id) || d.BuildMesh(0) == null)
                    errors.Add($"{name}: an outfit slot is empty, not an outfit, a duplicate id, or has no meshes.");
        }

#if UNITY_EDITOR
        public void EditorSet(List<CosmeticDefinition> coverallList, List<CosmeticDefinition> hatList, List<CosmeticDefinition> accessoryList, int defaultHatIndex)
        {
            coveralls = coverallList;
            hats = hatList;
            accessories = accessoryList;
            defaultHat = defaultHatIndex;
        }

        public void EditorSetLooks(List<CosmeticDefinition> characterList, List<CosmeticDefinition> outfitList)
        {
            characters = characterList;
            outfits = outfitList;
        }
#endif
    }
}
