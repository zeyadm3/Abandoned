using Abandoned.Core;
using System;
using Unity.Netcode;

namespace Abandoned.Contracts
{
    /// <summary>
    /// One job on the contract board (GDD 14), fully rolled: where, how hard, what's at stake. Sent to
    /// every player (the board and the run HUD), so it's network-serializable.
    /// </summary>
    [Serializable]
    public struct Contract : INetworkSerializable, IEquatable<Contract>
    {
        public string LocationId, Location, Scene, ModifierId, ModifierName;
        public int Seed, Quota, LootMin, LootMax;
        public float Stability, WindowSeconds, PayoutBonus;
        public bool PowerOff;
        /// <summary>LOW / MEDIUM / HIGH from stability, power, window and modifiers. It drives the job's monsters (ThreatDirector).</summary>
        public byte ThreatLevel;

        public void NetworkSerialize<T>(BufferSerializer<T> s) where T : IReaderWriter
        {
            // "No contract" is the default struct: its strings are null, which NGO can't write.
            if (s.IsWriter)
            {
                LocationId ??= "";
                Location ??= "";
                Scene ??= "";
                ModifierId ??= "";
                ModifierName ??= "";
            }
            s.SerializeValue(ref LocationId);
            s.SerializeValue(ref Location);
            s.SerializeValue(ref Scene);
            s.SerializeValue(ref ModifierId);
            s.SerializeValue(ref ModifierName);
            s.SerializeValue(ref Seed);
            s.SerializeValue(ref Quota);
            s.SerializeValue(ref LootMin);
            s.SerializeValue(ref LootMax);
            s.SerializeValue(ref Stability);
            s.SerializeValue(ref WindowSeconds);
            s.SerializeValue(ref PayoutBonus);
            s.SerializeValue(ref PowerOff);
            s.SerializeValue(ref ThreatLevel);
        }

        public bool IsValid => !string.IsNullOrEmpty(Scene) && Seed != 0;

        public bool Equals(Contract o) => Seed == o.Seed && LocationId == o.LocationId && Scene == o.Scene && ModifierId == o.ModifierId && Quota == o.Quota;

        public static string ThreatName(byte level) => level switch { 0 => Loc.T("LOW"), 1 => Loc.T("MEDIUM"), _ => Loc.T("HIGH") };

        /// <summary>What the level does to the job's monsters (ThreatPacingConfig), in the board's words.</summary>
        public static string ThreatNote(byte level) => level switch
        {
            0 => Loc.T("fewer monsters, slower to arrive, weaker hits"),
            1 => Loc.T("monsters arrive steadily"),
            _ => Loc.T("more monsters, sooner, harder hits"),
        };
    }
}
