using Abandoned.Audio;
using Abandoned.Core;
using Abandoned.Equipment;
using Abandoned.Networking;
using Unity.Netcode;
using UnityEngine;

namespace Abandoned.Extraction
{
    /// <summary>
    /// Store alarms (0.16.5), on the run state's object. Host: the run seed wires a few stores; the first
    /// living player to walk into a wired store trips it once, and it rings for half a minute, drawing every
    /// monster that listens. Bolt cutters on the alarm box (E) disarm it, before or during. Masks are by
    /// store shutter index.
    /// </summary>
    public class RunAlarms : NetworkBehaviour
    {
        private readonly NetworkVariable<int> wired = new();
        private readonly NetworkVariable<int> disarmed = new();
        private readonly NetworkVariable<int> tripped = new();
        private readonly NetworkList<double> ringUntil = new();
        private RunState run;
        private float armAt, nextNoise;
        private bool rolled;

        public static RunAlarms Current { get; private set; }

        public bool Wired(int store) => (wired.Value & (1 << store)) != 0;
        public bool Disarmed(int store) => (disarmed.Value & (1 << store)) != 0;
        public bool Armed(int store) => Wired(store) && !Disarmed(store) && (tripped.Value & (1 << store)) == 0;
        public bool Ringing(int store) => store >= 0 && store < ringUntil.Count && !Disarmed(store) && run != null && ringUntil[store] > run.Now;

        public override void OnNetworkSpawn()
        {
            Current = this;
            run = GetComponent<RunState>();
            if (!IsServer) return;
            armAt = Time.time + AlarmConfig.Current.ArmDelay;
            if (ringUntil.Count == 0) for (int i = 0; i < 32; i++) ringUntil.Add(0d);
        }

        public override void OnNetworkDespawn()
        {
            if (Current == this) Current = null;
        }

        private void Update()
        {
            if (!IsServer || !IsSpawned || run == null) return;
            if (Time.time < armAt) return;
            if (!rolled) Roll(); // by now the run state has its seed
            foreach (StoreAlarm alarm in StoreAlarm.All)
            {
                if (alarm == null || !Armed(alarm.Store)) continue;
                RollerShutter shutter = alarm.Shutter;
                if (shutter == null) continue;
                foreach (NetworkPlayer p in NetworkPlayer.All)
                {
                    if (p == null || p.IsDead || !shutter.InRoom(p.transform.position)) continue;
                    Trip(alarm);
                    break;
                }
            }
            if (Time.time < nextNoise) return;
            nextNoise = Time.time + AlarmConfig.Current.RingNoiseInterval;
            foreach (StoreAlarm alarm in StoreAlarm.All)
                if (alarm != null && Ringing(alarm.Store)) NoiseSystem.Emit(alarm.transform.position, AlarmConfig.Current.RingNoise, NoiseSource.Other);
        }

        private void Roll()
        {
            rolled = true;
            var random = new System.Random(run.State.Seed ^ 0x0a1a12);
            int mask = 0;
            // One roll per index, in index order: the same seed wires the same stores.
            for (int i = 0; i < 32; i++)
            {
                double roll = random.NextDouble();
                if (StoreAlarm.ForStore(i) != null && roll < AlarmConfig.Current.WiredChance) mask |= 1 << i;
            }
            wired.Value = mask;
        }

        private void Trip(StoreAlarm alarm)
        {
            tripped.Value |= 1 << alarm.Store;
            ringUntil[alarm.Store] = run.Now + AlarmConfig.Current.RingSeconds;
            nextNoise = 0f;
            Debug.Log($"[Run] Store alarm {alarm.Store} tripped.");
        }

        public void RequestDisarm(int store) => DisarmRpc(store);

        [Rpc(SendTo.Server)]
        private void DisarmRpc(int store, RpcParams rpcParams = default)
        {
            StoreAlarm alarm = StoreAlarm.ForStore(store);
            NetworkObject player = NetworkManager.ConnectedClients.TryGetValue(rpcParams.Receive.SenderClientId, out NetworkClient c) ? c.PlayerObject : null;
            if (alarm == null || player == null || !Wired(store) || Disarmed(store)) return;
            if (!player.TryGetComponent(out PlayerEquipment gear) || !gear.Has(EquipmentKind.BoltCutters)) return;
            if (Vector3.Distance(player.transform.position, alarm.transform.position) > AlarmConfig.Current.Reach + 1.5f) return;
            disarmed.Value |= 1 << store;
            CutRpc(alarm.transform.position);
        }

        [Rpc(SendTo.Everyone)]
        private void CutRpc(Vector3 at) => GameAudio.Play(SoundId.AlarmCut, at, 1f);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Current = null;
    }
}
