using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;

namespace Abandoned.Extraction
{
    /// <summary>What the truck drove off with, sent by the host to everyone for the appraisal (GDD 10, PLAYBOOK 5.4).</summary>
    public sealed class RunResults : INetworkSerializable
    {
        public struct Item : INetworkSerializable
        {
            public string Name;
            /// <summary>Value when it was found (rolled value x condition).</summary>
            public int StartValue;
            public int FinalValue;
            /// <summary>Carried out in a pocket rather than loaded in the cargo.</summary>
            public bool Pocketed;

            public int DamageLost => StartValue - FinalValue;

            public void NetworkSerialize<T>(BufferSerializer<T> s) where T : IReaderWriter
            {
                s.SerializeValue(ref Name);
                s.SerializeValue(ref StartValue);
                s.SerializeValue(ref FinalValue);
                s.SerializeValue(ref Pocketed);
            }
        }

        public struct Player : INetworkSerializable
        {
            public ulong ClientId;
            public string Name;
            public bool Extracted;
            /// <summary>Pocket loot lost by being left behind.</summary>
            public int PocketValueLost;

            public void NetworkSerialize<T>(BufferSerializer<T> s) where T : IReaderWriter
            {
                s.SerializeValue(ref ClientId);
                s.SerializeValue(ref Name);
                s.SerializeValue(ref Extracted);
                s.SerializeValue(ref PocketValueLost);
            }
        }

        public int Quota, Seed;
        public float Seconds;
        public Item[] Items = Array.Empty<Item>();
        public Player[] Players = Array.Empty<Player>();
        /// <summary>Funny stats lines (filled by the run's stats tracker, 5.4).</summary>
        public string[] Stats = Array.Empty<string>();

        public int Haul => Items.Sum(i => i.FinalValue);
        public int StartValue => Items.Sum(i => i.StartValue);
        public bool QuotaMet => Haul >= Quota;

        public void NetworkSerialize<T>(BufferSerializer<T> s) where T : IReaderWriter
        {
            s.SerializeValue(ref Quota);
            s.SerializeValue(ref Seed);
            s.SerializeValue(ref Seconds);
            SerializeArray(s, ref Items);
            SerializeArray(s, ref Players);
            int n = Stats.Length;
            s.SerializeValue(ref n);
            if (s.IsReader) Stats = new string[n];
            for (int i = 0; i < n; i++) s.SerializeValue(ref Stats[i]);
        }

        private static void SerializeArray<T, TItem>(BufferSerializer<T> s, ref TItem[] array) where T : IReaderWriter
            where TItem : struct, INetworkSerializable
        {
            int n = array.Length;
            s.SerializeValue(ref n);
            if (s.IsReader) array = new TItem[n];
            for (int i = 0; i < n; i++) array[i].NetworkSerialize(s);
        }

        public static RunResults From(IEnumerable<Item> items, IEnumerable<Player> players, int quota, int seed, float seconds) =>
            new() { Items = items.OrderByDescending(i => i.FinalValue).ToArray(), Players = players.ToArray(), Quota = quota, Seed = seed, Seconds = seconds };
    }
}
