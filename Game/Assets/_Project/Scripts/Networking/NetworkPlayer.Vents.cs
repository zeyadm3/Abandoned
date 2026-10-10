using Abandoned.Audio;
using Abandoned.Core;
using Abandoned.Extraction;
using Unity.Netcode;
using UnityEngine;

namespace Abandoned.Networking
{
    /// <summary>
    /// Crawling through a vent (0.16.4). It is a kind of hiding: the host writes <see cref="VentBase"/> + the
    /// duct into the hiding value, so every monster rule for hidden players applies (and none follows them
    /// in: <see cref="InVent"/>). The owner's machine takes its player out of the world for the crawl and
    /// puts them at the far grille when the host says the crawl is over.
    /// </summary>
    public partial class NetworkPlayer
    {
        public const int VentBase = 100;
        // Below the whole level: out of sight and out of reach while crawling.
        private const float DuctDepth = 60f;

        private readonly NetworkVariable<double> ventUntil = new(0d,
            NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        // Which end the crawl started from (it ends at the other).
        private readonly NetworkVariable<byte> ventFrom = new(0,
            NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private bool ventHere;
        // Owner: the duct this machine put its player in, and where they stood when they went in.
        private int ventDuctHere = -1;
        private Vector3 ventEntryPoint;
        private float nextRattle;

        public bool InVent => hidingIn.Value >= VentBase;
        public int VentDuct => InVent ? hidingIn.Value - VentBase : -1;
        /// <summary>Seconds of crawling left (every machine).</summary>
        public float VentRemaining => InVent ? Mathf.Max(0f, (float)(ventUntil.Value - NetworkManager.ServerTime.Time)) : 0f;

        public void RequestVent(int duct, int fromEnd)
        {
            if (IsOwner && IsSpawned && duct >= 0 && duct + VentBase <= sbyte.MaxValue) VentRpc((sbyte)duct, (byte)fromEnd);
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
        private void VentRpc(sbyte duct, byte fromEnd)
        {
            if (dead.Value || IsHiding || ragdoll.IsRagdolled || Time.time < nextHideAt || !state.Value.Crouching) return;
            VentGrille entry = VentGrille.Find(duct, fromEnd), exit = VentGrille.Find(duct, 1 - fromEnd);
            if (entry == null || exit == null || Vector3.Distance(transform.position, entry.transform.position) > VentConfig.Current.Reach + 1f) return;
            if (carrier != null && carrier.Held != null) return;
            ventFrom.Value = fromEnd;
            ventUntil.Value = NetworkManager.ServerTime.Time + VentGrille.CrawlSeconds(duct);
            hidingIn.Value = (sbyte)(VentBase + duct);
            NoiseSystem.Emit(entry.transform.position, VentConfig.Current.RattleNoise, NoiseSource.Other);
            VentSoundRpc(entry.transform.position);
        }

        // Host, each frame while crawling: the duct rattles, and at the end the player comes out.
        private void TickServerVent()
        {
            int duct = VentDuct;
            VentGrille a = VentGrille.Find(duct, 0), b = VentGrille.Find(duct, 1);
            if (a == null || b == null || dead.Value)
            {
                ServerLeaveHiding();
                return;
            }
            if (Time.time >= nextRattle)
            {
                VentConfig config = VentConfig.Current;
                nextRattle = Time.time + config.RattleInterval;
                Vector3 middle = (a.transform.position + b.transform.position) * 0.5f;
                NoiseSystem.Emit(middle, config.RattleNoise, NoiseSource.Other);
                VentSoundRpc(middle);
            }
            if (NetworkManager.ServerTime.Time >= ventUntil.Value)
            {
                Vector3 at = VentGrille.Find(duct, 1 - ventFrom.Value).OutPoint;
                ServerLeaveHiding();
                VentSoundRpc(at);
            }
        }

        [Rpc(SendTo.Everyone)]
        private void VentSoundRpc(Vector3 at) => GameAudio.Play(SoundId.VentCrawl, at, 1f);

        private void OwnerEnterVent(int duct)
        {
            if (ventHere) return;
            ventHere = true;
            ventDuctHere = duct;
            ventEntryPoint = transform.position;
            if (hideInput == null) hideInput = GetComponent<Player.PlayerInputReader>();
            if (hideHud == null) hideHud = GetComponent<Interaction.InteractionHud>();
            SetHidingControl(false);
            motor.ApplyRemoteState(true, false, true);
            VentGrille exit = VentGrille.Find(duct, 1 - ventFrom.Value);
            OwnerTeleport((exit != null ? exit.OutPoint : transform.position) + Vector3.down * DuctDepth);
            if (hideHud != null) hideHud.Override = "Crawling through the vent...";
        }

        /// <summary>
        /// Owner: out of the duct. <paramref name="stepOut"/> stands them at the far grille; a respawn that is
        /// about to place them somewhere else passes false (as for lockers). The host's "out" and its
        /// respawn or death arrive separately and in either order, so this runs once, from whichever is first.
        /// </summary>
        private void OwnerLeaveVent(bool stepOut)
        {
            if (!ventHere) return;
            ventHere = false;
            int duct = ventDuctHere;
            ventDuctHere = -1;
            if (hideHud != null) hideHud.Override = null;
            if (stepOut)
            {
                VentGrille exit = VentGrille.Find(duct, 1 - ventFrom.Value);
                if (exit != null)
                {
                    transform.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(exit.transform.forward, Vector3.up));
                    OwnerTeleport(exit.OutPoint);
                    if (look != null) look.SyncYawFromTransform();
                }
                // The grille is gone (its floor fell, the level changed): back where they went in, never
                // left parked under the level with the controls on, falling for ever.
                else OwnerTeleport(ventEntryPoint);
            }
            if (!dead.Value && !ragdoll.IsRagdolled) SetHidingControl(true);
        }

        // Owner: the crawl is dark, cramped and a few seconds long (0.22.2: the HUD's blackout).
        private void TickOwnerVentScreen()
        {
            if (ventHere) UI.HudCallout.Blackout(this, $"crawling...  {VentRemaining:0}");
            else UI.HudCallout.Blackout(this, null);
        }
    }
}
