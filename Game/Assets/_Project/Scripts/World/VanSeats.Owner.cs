using Abandoned.Core;
using Abandoned.Interaction;
using Abandoned.Networking;
using Abandoned.Player;
using UnityEngine;

namespace Abandoned.World
{
    /// <summary>The local player's side of a seat: in and out, the view turning with the van, and the driver's hands.</summary>
    public partial class VanSeats
    {
        private void TickOwner(NetworkPlayer p, int seat)
        {
            VanRider rider = p.GetComponent<VanRider>() ?? p.gameObject.AddComponent<VanRider>();
            var look = p.GetComponent<PlayerLook>();
            var hud = p.GetComponent<InteractionHud>();
            var controller = p.GetComponent<CharacterController>();
            float vanYaw = transform.eulerAngles.y;

            if (rider.OwnerSeat != seat)
            {
                if (rider.OwnerSeat < 0) Enter(p, look, controller, rider, vanYaw);
                else if (seat < 0) Leave(p, look, hud, controller, rider.OwnerSeat);
                rider.OwnerSeat = seat;
            }
            if (seat < 0) return;

            Pin(p, seat);
            // The van turned under us: so does the view, and the head stays within its arc of the windscreen.
            if (look != null)
            {
                p.transform.rotation = Quaternion.Euler(0f, p.transform.eulerAngles.y + Mathf.DeltaAngle(rider.LastVanYaw, vanYaw), 0f);
                look.SyncYawFromTransform();
                look.LimitYaw(vanYaw, lookAngle);
            }
            rider.LastVanYaw = vanYaw;

            var reader = p.GetComponent<PlayerInputReader>();
            PlayerInputFrame input = reader != null ? reader.Current : default;
            bool menus = CursorOwner.UiActive;
            if (!menus && input.InteractPressed && Time.frameCount > rider.SeatedFrame + 1)
            {
                RequestLeave();
                return;
            }
            if (seat == DriverSeat && van != null && !VanTestDriver.Driving)
            {
                van.SendDrive(menus ? VanDriveInput.Parked : new VanDriveInput(input.Move.y, input.Move.x, input.CrouchHeld), !menus && input.UseHeld);
                // At the job, the horn is the truck's lever: it calls the crew and starts the departure countdown.
                Extraction.RunState run = Extraction.RunState.Current;
                if (!menus && input.UsePressed && run != null && run.State.Phase == Extraction.RunPhase.Running && WorldJob.JobPlace >= 0
                    && WorldJob.VanFrom(WorldJob.JobPlace) <= 25f)
                    run.RequestDepart();
            }
            if (hud != null) hud.Override = HudLine(seat);
        }

        private void Enter(NetworkPlayer p, PlayerLook look, CharacterController controller, VanRider rider, float vanYaw)
        {
            p.OwnerSetControl(false);
            // Off while seated: the van's body moves the player, and an enabled controller would also block it.
            if (controller != null) controller.enabled = false;
            p.transform.rotation = Quaternion.Euler(0f, vanYaw, 0f);
            if (look != null) look.SyncYawFromTransform();
            rider.LastVanYaw = vanYaw;
            rider.SeatedFrame = Time.frameCount;
        }

        private void Leave(NetworkPlayer p, PlayerLook look, InteractionHud hud, CharacterController controller, int seat)
        {
            if (look != null) look.ClearYawLimit();
            if (hud != null) hud.Override = null;
            if (controller != null) controller.enabled = true;
            p.OwnerTeleport(ExitPoint(seat));
            if (look != null) look.SyncYawFromTransform();
            if (!p.IsDead && !p.Ragdoll.IsRagdolled) p.OwnerSetControl(true);
        }

        /// <summary>Where someone climbing out of a seat stands: on whatever ground is under its door.</summary>
        public Vector3 ExitPoint(int seat)
        {
            Vector3 at = exits[seat].position;
            int mask = ~LayerMask.GetMask(GameLayers.Player, GameLayers.Loot, GameLayers.Debris, "Ignore Raycast");
            if (Physics.Raycast(at + Vector3.up * 2.5f, Vector3.down, out RaycastHit hit, 6f, mask, QueryTriggerInteraction.Ignore))
                return hit.point + Vector3.up * .05f;
            return at;
        }

        // Where the job is: the place and how far, or home once the job has left it.
        private static string JobLine()
        {
            int job = WorldJob.JobPlace;
            if (job < 0 || WorldState.Current == null) return "";
            Extraction.RunState run = Extraction.RunState.Current;
            bool done = run != null && run.State.Phase == Extraction.RunPhase.Departed;
            int place = done ? WorldState.Current.Atlas.Home : job;
            float d = WorldJob.VanFrom(place);
            string name = Loc.T(WorldState.Current.Atlas.Pois[place].scene);
            if (d <= 25f) return done ? "" : $" · {Loc.T("At {0}: [{1}] horn to leave", name, InputBindings.Display("Use"))}";
            return $" · {(done ? Loc.T("Drive back to the HQ ({0:0.0} km)", d / 1000f) : Loc.T("Job: {0} ({1:0.0} km)", name, d / 1000f))}";
        }

        private string HudLine(int seat)
        {
            string leave = Loc.T("[{0}] Get out", InputBindings.Display("Interact"));
            if (van == null) return leave;
            VanNetState s = van.State;
            string gauges = Loc.T("Fuel {0:0} L · Damage {1:0}% · {2:0} km/h", s.Fuel, s.Damage, van.Drive.SpeedKmh) + JobLine();
            if (seat != DriverSeat) return $"{leave} · {gauges}";
            string status = s.BrokenDown ? Loc.T("Broken down: get out and repair it")
                : s.Fuel <= 0f ? Loc.T("Out of fuel")
                : Loc.T("[W/S] Drive · [A/D] Steer · [{0}] Handbrake · [{1}] Horn", InputBindings.Display("Crouch"), InputBindings.Display("Use"));
            return $"{leave} · {status} · {gauges}";
        }
    }
}
