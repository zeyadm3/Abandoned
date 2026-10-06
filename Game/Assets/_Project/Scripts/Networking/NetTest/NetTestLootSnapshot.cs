using System;
using UnityEngine;

namespace Abandoned.Networking
{
    /// <summary>How one machine sees one networked loot item (JsonUtility-friendly).</summary>
    [Serializable]
    public class NetTestLootSnapshot
    {
        public ulong id;
        public string item;
        public int fullValue;
        public int currentValue;
        public bool shattered;
        public Vector3 position;
        public ulong owner;
        public string hold;
        public bool hidden;

        public static NetTestLootSnapshot Of(NetworkLoot loot) => new()
        {
            id = loot.NetworkObjectId,
            item = loot.Item.Definition != null ? loot.Item.Definition.Id : loot.name,
            fullValue = loot.Item.FullValue,
            currentValue = loot.Item.CurrentValue,
            shattered = loot.Item.IsShattered,
            position = loot.transform.position,
            owner = loot.OwnerClientId,
            hold = loot.Hold.ToString(),
            hidden = loot.Grabbable.IsPocketed,
        };
    }
}
