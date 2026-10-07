using Abandoned.Core;
using Abandoned.Networking;
using Abandoned.Structure;
using Unity.Netcode;
using UnityEngine;

namespace Abandoned.Threats
{
    /// <summary>It moves above the halls. The advance has an audible windup; weak sections crack under it.</summary>
    public class TheWeight : RoamingThreat
    {
        private readonly NetworkVariable<bool> pressing = new();
        private float nextPress, strikeAt;
        private StructuralSection pressureSection;
        public override string DisplayName => "The Weight";
        public override ThreatMotion DesiredMotion => pressing.Value ? ThreatMotion.Special : ThreatMotion.Idle;
        protected override void HostTick()
        {
            if (pressing.Value)
            {
                Stop();
                if (Time.time < strikeAt) return;
                if (pressureSection != null && !pressureSection.IsCollapsed)
                {
                    pressureSection.ApplyImpact(Momentum);
                    NoiseSystem.Emit(pressureSection.transform.position, 0.85f, NoiseSource.Other);
                    StrikeRpc(pressureSection.transform.position);
                }
                pressing.Value = false;
                nextPress = Time.time + Interval;
                return;
            }
            NetworkPlayer prey = Nearest();
            if (prey != null) Move(PositionOf(prey), WalkSpeed); else Wander();
            if (Time.time < nextPress) return;
            pressureSection = null;
            float best = 7f;
            foreach (StructuralSection section in FindObjectsByType<StructuralSection>(FindObjectsSortMode.None))
            {
                if (!section.CanCollapse || section.IsCollapsed) continue;
                float d = Vector3.Distance(section.transform.position, transform.position + Vector3.up * 2f);
                if (d < best) { pressureSection = section; best = d; }
            }
            if (pressureSection == null) { nextPress = Time.time + 3f; return; }
            pressing.Value = true;
            strikeAt = Time.time + 2.5f;
            WarnRpc(pressureSection.transform.position);
        }
        [Rpc(SendTo.Everyone)]
        private void WarnRpc(Vector3 at) => ThreatHorrorAudio.PlaySignature(ThreatKind.Weight, at, 0.8f);
        [Rpc(SendTo.Everyone)]
        private void StrikeRpc(Vector3 at)
        {
            Abandoned.Audio.GameAudio.Play(Abandoned.Audio.SoundId.Groan, at, 1f);
            CameraShake.Emit(at, Momentum);
        }
    }
}
