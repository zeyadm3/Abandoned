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
            /// <summary>Its loot definition's id (0.18.7: the HQ loot catalogue).</summary>
            public string Id;
            /// <summary>Value when it was found (rolled value x condition).</summary>
            public int StartValue;
            public int FinalValue;
            /// <summary>Gameplay kilograms loaded with this item, before any towing losses.</summary>
            public float Weight;
            /// <summary>Carried out in a pocket rather than loaded in the cargo.</summary>
            public bool Pocketed;
            /// <summary>One of the run's big prizes (M9.5 achievements).</summary>
            public bool Jackpot;

            public int DamageLost => StartValue - FinalValue;

            public void NetworkSerialize<T>(BufferSerializer<T> s) where T : IReaderWriter
            {
                // NGO can't write a null string, and a throw here would leave every machine on a truck that
                // has "left" with no appraisal to end the job.
                Name ??= "";
                s.SerializeValue(ref Name);
                Id ??= "";
                s.SerializeValue(ref Id);
                s.SerializeValue(ref StartValue);
                s.SerializeValue(ref FinalValue);
                s.SerializeValue(ref Weight);
                s.SerializeValue(ref Pocketed);
                s.SerializeValue(ref Jackpot);
            }
        }

        public struct Player : INetworkSerializable
        {
            public ulong ClientId;
            public string Name;
            public bool Extracted;
            /// <summary>Killed during the run (a threat), as opposed to just left behind.</summary>
            public bool Died;
            /// <summary>Pocket loot lost by being left behind.</summary>
            public int PocketValueLost;

            public void NetworkSerialize<T>(BufferSerializer<T> s) where T : IReaderWriter
            {
                s.SerializeValue(ref ClientId);
                Name ??= "";
                s.SerializeValue(ref Name);
                s.SerializeValue(ref Extracted);
                s.SerializeValue(ref Died);
                s.SerializeValue(ref PocketValueLost);
            }
        }

        public int Quota, Seed;
        public float Seconds;
        /// <summary>Nobody made it: the truck was towed home with part of its load (QA B-20).</summary>
        public bool Wiped;
        public Item[] Items = Array.Empty<Item>();
        public Player[] Players = Array.Empty<Player>();
        /// <summary>Funny stats lines (filled by the run's stats tracker, 5.4).</summary>
        public string[] Stats = Array.Empty<string>();
        /// <summary>Pay on top of the haul (0.18+: boss requests, rush orders, sets, overtime); it doesn't count toward quota.</summary>
        public int Extra;
        /// <summary>0.19.3: one line per player, a job title and why (the appraisal's "who did what").</summary>
        public string[] Blame = Array.Empty<string>();

        public int Haul => Items.Sum(i => i.FinalValue);
        public int StartValue => Items.Sum(i => i.StartValue);
        public float LoadedWeight => Items.Sum(i => i.Weight);
        public bool QuotaMet => Haul >= Quota;

        public void NetworkSerialize<T>(BufferSerializer<T> s) where T : IReaderWriter
        {
            s.SerializeValue(ref Quota);
            s.SerializeValue(ref Seed);
            s.SerializeValue(ref Seconds);
            s.SerializeValue(ref Wiped);
            SerializeArray(s, ref Items);
            SerializeArray(s, ref Players);
            s.SerializeValue(ref Extra);
            SerializeLines(s, ref Blame);
            SerializeLines(s, ref Stats);
        }

        // Null arrays and null lines are written as empty: the host fills these from several sources.
        private static void SerializeLines<T>(BufferSerializer<T> s, ref string[] lines) where T : IReaderWriter
        {
            lines ??= Array.Empty<string>();
            int n = lines.Length;
            s.SerializeValue(ref n);
            if (s.IsReader) lines = new string[n];
            for (int i = 0; i < n; i++)
            {
                lines[i] ??= "";
                s.SerializeValue(ref lines[i]);
            }
        }

        private static void SerializeArray<T, TItem>(BufferSerializer<T> s, ref TItem[] array) where T : IReaderWriter
            where TItem : struct, INetworkSerializable
        {
            array ??= Array.Empty<TItem>();
            int n = array.Length;
            s.SerializeValue(ref n);
            if (s.IsReader) array = new TItem[n];
            for (int i = 0; i < n; i++) array[i].NetworkSerialize(s);
        }

        public static RunResults From(IEnumerable<Item> items, IEnumerable<Player> players, int quota, int seed, float seconds) =>
            new() { Items = items.OrderByDescending(i => i.FinalValue).ToArray(), Players = players.ToArray(), Quota = quota, Seed = seed, Seconds = seconds };
    }
}
