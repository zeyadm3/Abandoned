using Unity.Netcode;

namespace Abandoned.Networking
{
    /// <summary>A loot item's value as the host owns it; clients only ever read this (appraisal, HUD, F1).</summary>
    public struct LootValueState : INetworkSerializeByMemcpy
    {
        public int FullValue;
        public int CurrentValue;
        public float Condition;
        public bool Shattered;
        /// <summary>False until the host has rolled the value (a zeroed state must not overwrite a client's item).</summary>
        public bool Rolled;
        public bool Appraised;

        public LootValueState(int fullValue, int currentValue, float condition, bool shattered, bool appraised = false)
        {
            FullValue = fullValue;
            CurrentValue = currentValue;
            Condition = condition;
            Shattered = shattered;
            Rolled = true;
            Appraised = appraised;
        }
    }
}
