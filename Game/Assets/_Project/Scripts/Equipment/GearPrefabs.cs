using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Abandoned.Equipment
{
    /// <summary>
    /// The network prefabs gear spawns from 0.17 on (crash mat, glow stick, flare, floodlight, drone), by
    /// equipment kind, so new gear needs no new field on the player prefab. Data/Equipment/Resources/GearPrefabs.
    /// </summary>
    [CreateAssetMenu(menuName = "Abandoned/Equipment/Gear Prefabs", fileName = "GearPrefabs")]
    public class GearPrefabs : ScriptableObject
    {
        public const string ResourcePath = "GearPrefabs";

        [Serializable]
        public struct Entry
        {
            public EquipmentKind Kind;
            public NetworkObject Prefab;
        }

        [SerializeField] private List<Entry> entries = new();

        public IReadOnlyList<Entry> Entries => entries;

        public NetworkObject For(EquipmentKind kind)
        {
            foreach (Entry e in entries) if (e.Kind == kind) return e.Prefab;
            return null;
        }

        private static GearPrefabs current;

        public static GearPrefabs Current
        {
            get
            {
                if (current != null) return current;
                current = Resources.Load<GearPrefabs>(ResourcePath);
                if (current == null) current = CreateInstance<GearPrefabs>();
                return current;
            }
        }

#if UNITY_EDITOR
        public void EditorSet(EquipmentKind kind, NetworkObject prefab)
        {
            entries.RemoveAll(e => e.Kind == kind);
            entries.Add(new Entry { Kind = kind, Prefab = prefab });
        }
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => current = null;
    }
}
