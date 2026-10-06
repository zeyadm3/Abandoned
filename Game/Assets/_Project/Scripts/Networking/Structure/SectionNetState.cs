using System;
using Abandoned.Structure;
using Unity.Netcode;
using UnityEngine;

namespace Abandoned.Networking
{
    /// <summary>
    /// One section's state as the host last published it (an element of StructureNetSync's list;
    /// index = section id). Quantized so a ~300-section building fits one small initial sync and a
    /// change costs ~20 bytes: health in 1/255 steps, load in <see cref="LoadStep"/> kg steps.
    /// </summary>
    public struct SectionNetState : INetworkSerializable, IEquatable<SectionNetState>
    {
        public const float LoadStep = 5f;

        public byte Stage;
        public byte Health;
        public ushort Load;
        /// <summary>Collapse seed (0 unless collapsed): every machine plays the same pre-fractured break from it.</summary>
        public int Seed;
        /// <summary>Server time the section started Failing (0 otherwise): clients sag and creak on the host's clock.</summary>
        public double FailingSince;
        /// <summary>The host's structure generation (low byte) this entry belongs to; stale entries wait after a re-roll.</summary>
        public byte Generation;

        public StructuralStage StageValue => (StructuralStage)Stage;
        public float HealthFraction => Health / 255f;
        public float LoadKg => Load * LoadStep;

        public static SectionNetState Of(StructuralSection section, double failingSince, int generation) => new()
        {
            Stage = (byte)section.Stage,
            Health = QuantizeHealth(section.HealthFraction),
            Load = QuantizeLoad(section.Load),
            Seed = section.IsCollapsed ? section.CollapseSeed : 0,
            FailingSince = section.Stage == StructuralStage.Failing ? failingSince : 0d,
            Generation = (byte)generation,
        };

        public static byte QuantizeHealth(float fraction) => (byte)Mathf.Clamp(Mathf.RoundToInt(fraction * 255f), 0, 255);

        public static ushort QuantizeLoad(float kg) => (ushort)Mathf.Clamp(Mathf.RoundToInt(kg / LoadStep), 0, ushort.MaxValue);

        /// <summary>How far into its Failing window the section is at <paramref name="serverTime"/>.</summary>
        public float FailingTimeAt(double serverTime, float failingDuration) =>
            StageValue == StructuralStage.Failing ? Mathf.Clamp((float)(serverTime - FailingSince), 0f, failingDuration) : 0f;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Stage);
            serializer.SerializeValue(ref Health);
            serializer.SerializeValue(ref Load);
            serializer.SerializeValue(ref Seed);
            serializer.SerializeValue(ref FailingSince);
            serializer.SerializeValue(ref Generation);
        }

        public bool Equals(SectionNetState other) =>
            Stage == other.Stage && Health == other.Health && Load == other.Load && Seed == other.Seed &&
            FailingSince.Equals(other.FailingSince) && Generation == other.Generation;

        public override bool Equals(object obj) => obj is SectionNetState other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(Stage, Health, Load, Seed, FailingSince, Generation);

        public override string ToString() => $"{StageValue} hp {HealthFraction:P0} load {LoadKg:0} kg seed {Seed} gen {Generation}";
    }
}
