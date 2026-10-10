using System.Collections.Generic;
using Abandoned.Structure;
using Unity.Netcode;
using UnityEngine;

namespace Abandoned.Equipment
{
    /// <summary>
    /// A portable floodlight (0.20.0): a work lamp on a tripod that lights a room until its battery runs
    /// down, then flickers out and is gone. Only the first one or two standing cast shadows (oldest first,
    /// the same on every machine), so the 0.14.0 shadow atlas never overflows.
    /// </summary>
    public class PlacedFloodlight : NetworkBehaviour
    {
        private static readonly List<PlacedFloodlight> Live = new();

        [SerializeField] private Light lamp;

        private readonly NetworkVariable<double> lit = new();
        private float baseIntensity;

        public override void OnNetworkSpawn()
        {
            Live.Add(this);
            Live.Sort((a, b) => a.NetworkObjectId.CompareTo(b.NetworkObjectId));
            if (IsServer) lit.Value = NetworkManager.ServerTime.Time;
            if (lamp == null) return;
            ToolConfig config = ToolConfig.Current;
            lamp.intensity = baseIntensity = config.FloodlightIntensity;
            lamp.range = config.FloodlightRange;
            lamp.spotAngle = config.FloodlightAngle;
            LevelStoreys.LightEverything(lamp);
        }

        public override void OnNetworkDespawn() => Live.Remove(this);

        private void Update()
        {
            if (!IsSpawned || lamp == null) return;
            ToolConfig config = ToolConfig.Current;
            float left = config.FloodlightSeconds - (float)(NetworkManager.ServerTime.Time - lit.Value);
            int rank = Live.IndexOf(this);
            lamp.shadows = rank >= 0 && rank < config.FloodlightShadows ? LightShadows.Soft : LightShadows.None;
            // The last fifteen seconds: the battery sags and the lamp stutters.
            float level = left > 15f ? 1f : Mathf.Clamp01(left / 15f) * (Mathf.PerlinNoise(Time.time * 12f, NetworkObjectId) > 0.3f ? 1f : 0.2f);
            lamp.intensity = baseIntensity * level;
            if (IsServer && left <= 0f) NetworkObject.Despawn(true);
        }

#if UNITY_EDITOR
        public void EditorSetup(Light light) => lamp = light;
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Live.Clear();
    }
}
