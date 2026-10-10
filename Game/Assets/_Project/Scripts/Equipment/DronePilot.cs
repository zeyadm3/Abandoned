using Abandoned.Audio;
using Abandoned.Core;
using Abandoned.Networking;
using Abandoned.Player;
using Unity.Netcode;
using UnityEngine;

namespace Abandoned.Equipment
{
    /// <summary>
    /// The scouting drone (0.20.5). Spawned by the host and owned by its pilot, whose machine flies it (an
    /// owner NetworkTransform) and sees through its camera while the pilot's body stands still, controls off.
    /// Move, jump/crouch to climb and sink, look to turn; use again (or a flat battery) brings it home. Every
    /// machine hears its rotors; the host lets monsters hear them too.
    /// </summary>
    public class DronePilot : NetworkBehaviour
    {
        [SerializeField] private Camera lens;
        [SerializeField] private Transform rotors;

        private NetworkPlayer pilot;
        private PlayerInputReader input;
        private Camera mainCamera;
        private float battery, yaw, pitch, nextBuzz, nextNoise;
        private bool flying;

        public override void OnNetworkSpawn()
        {
            yaw = transform.eulerAngles.y;
            battery = ToolConfig.Current.DroneSeconds;
            if (!IsOwner) return;
            pilot = NetworkPlayer.Local;
            if (pilot == null) return;
            input = pilot.GetComponent<PlayerInputReader>();
            pilot.OwnerSetControl(false);
            mainCamera = Camera.main;
            if (mainCamera != null) mainCamera.enabled = false;
            if (lens != null) lens.enabled = true;
            flying = true;
        }

        public override void OnNetworkDespawn() => Land();

        private void Land()
        {
            if (!flying) return;
            flying = false;
            UI.HudCallout.Clear(this);
            if (lens != null) lens.enabled = false;
            if (mainCamera != null) mainCamera.enabled = true;
            // Not to someone in a locker or a vent (they would walk off still counted as hidden, which no
            // monster then sees), nor to a body on the floor (getting up switches the controls on itself).
            if (pilot != null && !pilot.IsDead && !pilot.IsHiding && !pilot.Ragdoll.IsRagdolled) pilot.OwnerSetControl(true);
        }

        private void Update()
        {
            if (rotors != null) rotors.Rotate(0f, 1800f * Time.deltaTime, 0f, Space.Self);
            if (Time.time >= nextBuzz)
            {
                nextBuzz = Time.time + 0.9f;
                GameAudio.Play(SoundId.DroneBuzz, transform.position, 1f);
            }
            if (IsServer && Time.time >= nextNoise)
            {
                nextNoise = Time.time + 1.5f;
                NoiseSystem.Emit(transform.position, ToolConfig.Current.DroneNoise, NoiseSource.Other);
            }
            if (!IsOwner || !flying || input == null) return;
            battery -= Time.deltaTime;
            PlayerInputFrame f = input.Current;
            // Knocked down mid-flight: getting up hands the body its controls back, and one stick must not
            // then steer the body and the drone together.
            if (battery <= 0f || f.UsePressed || pilot == null || pilot.IsDead || pilot.Ragdoll.IsRagdolled)
            {
                Land();
                RecallRpc();
                return;
            }
            UI.HudCallout.Set(this, $"DRONE  {battery:0} s      {InputBindings.Display("Use")}  bring it home      {InputBindings.Display("Sprint")} / {InputBindings.Display("Crouch")}  up / down");
            float sens = 0.12f * GameSettings.Sensitivity;
            yaw += f.Look.x * sens;
            pitch = Mathf.Clamp(pitch - f.Look.y * sens, -70f, 70f);
            transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            if (lens != null) lens.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
            float vertical = (f.JumpPressed || f.SprintHeld ? 1f : 0f) - (f.CrouchHeld ? 1f : 0f);
            Vector3 wish = transform.rotation * new Vector3(f.Move.x, 0f, f.Move.y) + Vector3.up * vertical;
            Vector3 step = Vector3.ClampMagnitude(wish, 1f) * ToolConfig.Current.DroneSpeed * Time.deltaTime;
            // Bump, don't pass: walls and floors stop it.
            if (step.sqrMagnitude > 0f && !Physics.SphereCast(transform.position, 0.25f, step.normalized, out _, step.magnitude + 0.05f, ~LayerMask.GetMask(GameLayers.Debris, "Ignore Raycast"), QueryTriggerInteraction.Ignore))
                transform.position += step;
        }

        [Rpc(SendTo.Server)]
        private void RecallRpc(RpcParams rpcParams = default)
        {
            if (rpcParams.Receive.SenderClientId == OwnerClientId && IsSpawned) NetworkObject.Despawn(true);
        }

#if UNITY_EDITOR
        public void EditorSetup(Camera camera, Transform spin)
        {
            lens = camera;
            rotors = spin;
        }
#endif
    }
}
