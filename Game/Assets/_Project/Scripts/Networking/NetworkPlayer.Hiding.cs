using System;
using Abandoned.Core;
using Abandoned.Extraction;
using Abandoned.Interaction;
using Abandoned.Player;
using Unity.Netcode;
using UnityEngine;

namespace Abandoned.Networking
{
    /// <summary>
    /// Hiding in a locker (0.16.0). The host decides who is in which locker and writes it here; the owner's
    /// machine steps its own player in and out (it owns its movement) and stops walking and reaching for
    /// things meanwhile. Monsters read <see cref="IsHiding"/> on the host.
    /// </summary>
    public partial class NetworkPlayer
    {
        // Host-written: the locker this player is hiding in, -1 for none.
        private readonly NetworkVariable<sbyte> hidingIn = new(-1,
            NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private float nextHideAt;
        // Owner: this machine has put its player in a locker (and which), until it has taken them out again.
        private bool hiddenHere;
        private HidingLocker hiddenIn;
        private PlayerInputReader hideInput;
        private PlayerStamina hideStamina;
        private InteractionHud hideHud;

        public int HidingIn => hidingIn.Value;
        public bool IsHiding => hidingIn.Value >= 0;

        /// <summary>
        /// Every machine: a player climbed into a locker or out of one (who, the locker before, the locker
        /// now; -1 = none). Raised before the player has moved, so on the host a monster can still judge
        /// whether it saw them go in.
        /// </summary>
        public static event Action<NetworkPlayer, int, int> HidingChanged;

        private void SpawnHiding() => hidingIn.OnValueChanged += OnHidingChanged;

        private void DespawnHiding()
        {
            hidingIn.OnValueChanged -= OnHidingChanged;
            if (hideHud != null) hideHud.Override = null;
        }

        /// <summary>This machine's player asks to climb into a locker.</summary>
        public void RequestHide(int locker)
        {
            if (IsOwner && IsSpawned && locker >= 0 && locker <= sbyte.MaxValue) HideRpc((sbyte)locker);
        }

        /// <summary>This machine's player asks to climb out.</summary>
        public void RequestLeaveHiding()
        {
            if (IsOwner && IsSpawned) HideRpc(-1);
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
        private void HideRpc(sbyte locker)
        {
            if (locker < 0)
            {
                ServerLeaveHiding();
                return;
            }
            if (dead.Value || IsHiding || ragdoll.IsRagdolled || Time.time < nextHideAt) return;
            HidingLocker target = HidingLocker.ByIndex(locker);
            if (target == null || !target.Usable || target.Occupant != null) return;
            DoorConfig config = DoorConfig.Current;
            if (Vector3.Distance(transform.position, target.ExitPoint) > config.HideReach) return;
            // Nothing bigger than a pocket fits in there with you.
            if (carrier != null && carrier.Held != null) return;
            hidingIn.Value = locker;
            NoiseSystem.Emit(target.ExitPoint, config.LockerNoise, NoiseSource.Other);
        }

        /// <summary>Host: out of the locker (climbed out, dragged out, or the locker is gone).</summary>
        public void ServerLeaveHiding()
        {
            if (!IsServer || !IsHiding) return;
            HidingLocker locker = InVent ? null : HidingLocker.ByIndex(hidingIn.Value);
            DoorConfig config = DoorConfig.Current;
            hidingIn.Value = -1;
            nextHideAt = Time.time + config.HideCooldown;
            if (locker != null) NoiseSystem.Emit(locker.ExitPoint, config.LockerNoise, NoiseSource.Other);
        }

        // Host: a locker that fell with its floor, or an occupant knocked down, doesn't hold anyone.
        private void TickServerHiding()
        {
            if (!IsHiding) return;
            if (InVent)
            {
                TickServerVent();
                return;
            }
            HidingLocker locker = HidingLocker.ByIndex(hidingIn.Value);
            if (locker == null || !locker.Usable || dead.Value || state.Value.Ragdolled) ServerLeaveHiding();
        }

        private void OnHidingChanged(sbyte was, sbyte now)
        {
            HidingChanged?.Invoke(this, was, now);
            if (!IsOwner) return;
            // 0.16.4: a vent crawl is hiding too, at VentBase + the duct.
            if (was >= VentBase) OwnerLeaveVent(true);
            if (now >= VentBase) OwnerEnterVent(now - VentBase);
            else if (now >= 0) OwnerEnterLocker(HidingLocker.ByIndex(now));
            else if (was < VentBase) OwnerLeaveLocker(true);
        }

        private void OwnerEnterLocker(HidingLocker locker)
        {
            if (locker == null || hiddenHere) return;
            hiddenHere = true;
            hiddenIn = locker;
            if (hideInput == null) hideInput = GetComponent<PlayerInputReader>();
            if (hideStamina == null) hideStamina = GetComponent<PlayerStamina>();
            if (hideHud == null) hideHud = GetComponent<InteractionHud>();
            SetHidingControl(false);
            // The motor is off from here on: don't leave "sprinting" standing for the others to hear.
            motor.ApplyRemoteState(true, false, motor.IsCrouching);
            transform.rotation = locker.Facing;
            OwnerTeleport(locker.InsidePoint);
            if (look != null)
            {
                look.SyncYawFromTransform();
                look.LimitYaw(transform.eulerAngles.y, DoorConfig.Current.HideLookAngle);
            }
            if (hideHud != null) hideHud.Override = $"[{InputBindings.Display("Interact")}] Climb out";
        }

        /// <summary>
        /// Owner: back out of the locker. <paramref name="stepOut"/> moves them to the spot in front of it;
        /// a respawn that is about to place them somewhere else passes false.
        /// </summary>
        private void OwnerLeaveLocker(bool stepOut)
        {
            if (!hiddenHere) return;
            hiddenHere = false;
            HidingLocker locker = hiddenIn;
            hiddenIn = null;
            if (look != null) look.ClearYawLimit();
            if (hideHud != null) hideHud.Override = null;
            if (stepOut && locker != null)
            {
                OwnerTeleport(locker.ExitPoint);
                if (look != null) look.SyncYawFromTransform();
            }
            // Dead or down: whatever put them there switches the controls back on when they get up.
            if (!dead.Value && !ragdoll.IsRagdolled) SetHidingControl(true);
        }

        // Owner, each frame while hidden: E or jump climbs out; breath comes back while you wait.
        private void TickOwnerHiding()
        {
            if (!hiddenHere) return;
            if (hideStamina != null) hideStamina.Tick(Time.deltaTime);
            if (hideInput == null) return;
            PlayerInputFrame input = hideInput.Current;
            if (input.InteractPressed || input.JumpPressed) RequestLeaveHiding();
        }

        /// <summary>Owner (0.16.7+): stop or restart walking and reaching, for screens that take over the view.</summary>
        public void OwnerSetControl(bool on)
        {
            if (IsOwner) SetHidingControl(on);
        }

        private void SetHidingControl(bool on)
        {
            foreach (Behaviour b in ownerOnlyBehaviours)
                if (b is PlayerMotor || b is PlayerInteractor) b.enabled = on;
        }
    }
}
