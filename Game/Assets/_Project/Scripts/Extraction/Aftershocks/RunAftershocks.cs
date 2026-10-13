using Abandoned.Audio;
using Abandoned.Core;
using Abandoned.Networking;
using Abandoned.Structure;
using Unity.Netcode;
using UnityEngine;

namespace Abandoned.Extraction
{
    /// <summary>
    /// Aftershocks (0.17.4), on the run state's object. Host: every few minutes the ground rumbles for a
    /// few seconds (everyone hears and feels it coming, and dispatch calls it), then a shock cracks a handful
    /// of random collapsible sections through the normal damage path. Nothing falls from the shock alone;
    /// weight does the rest.
    /// </summary>
    public class RunAftershocks : NetworkBehaviour
    {
        private RunState run;
        private System.Random random;
        private float nextShockAt = -1f, shockAt = -1f;

        public override void OnNetworkSpawn()
        {
            run = GetComponent<RunState>();
            if (!IsServer) return;
            nextShockAt = Time.time + AftershockConfig.Current.FirstAfter;
        }

        private void Update()
        {
            if (!IsServer || !IsSpawned || run == null || run.State.Phase != RunPhase.Running) return;
            AftershockConfig config = AftershockConfig.Current;
            random ??= new System.Random(run.State.Seed ^ 0x0a0174);
            if (shockAt < 0f && Time.time >= nextShockAt - config.Warning)
            {
                shockAt = nextShockAt;
                WarnRpc(config.Warning);
            }
            if (shockAt < 0f || Time.time < shockAt) return;
            shockAt = -1f;
            nextShockAt = Time.time + Mathf.Lerp(config.Gap.x, config.Gap.y, (float)random.NextDouble());
            StructureSimulation structure = BuildingDecay.Current != null ? BuildingDecay.Current.Structure : null;
            int cracked = structure != null
                ? structure.AgeRandomSections(random.Next(config.Sections.x, config.Sections.y + 1), config.Damage, config.HealthFloor, random)
                : 0;
            ShockRpc();
            Debug.Log($"[Run] Aftershock: {cracked} sections cracked.");
        }

        // Everyone: a rumble building under their feet, the lights jittering, dispatch on the radio.
        [Rpc(SendTo.Everyone)]
        private void WarnRpc(float seconds)
        {
            Vector3 at = NetworkPlayer.Local != null ? NetworkPlayer.Local.transform.position : Vector3.zero;
            GameAudio.Play(SoundId.DeepRumble, at + Vector3.down * 3f, 1f);
            GameAudio.PlayDelayed(SoundId.DeepRumble, at + Vector3.down * 3f, seconds * 0.5f);
            RunHorrorDirector.Say(Loc.T("TREMOR / brace yourselves"), seconds);
            LightFixture.Disturb(seconds);
        }

        [Rpc(SendTo.Everyone)]
        private void ShockRpc()
        {
            Vector3 at = NetworkPlayer.Local != null ? NetworkPlayer.Local.transform.position : Vector3.zero;
            GameAudio.Play(SoundId.Aftershock, at, 1f);
            CameraShake.EmitCollapse(at, AftershockConfig.Current.Shake);
            LightFixture.Disturb(3f);
        }
    }
}
