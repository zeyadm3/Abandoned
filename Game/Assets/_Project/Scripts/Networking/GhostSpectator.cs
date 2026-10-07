using System.Collections.Generic;
using System.Linq;
using Abandoned.Core;
using Abandoned.Player;
using Abandoned.Threats;
using Unity.Cinemachine;
using UnityEngine;

namespace Abandoned.Networking
{
    /// <summary>
    /// Death = ghost spectator for the rest of the run (GDD 11): a moment after dying, this player's view
    /// leaves the body and orbits a living teammate (left/right mouse to switch), with threats marked on
    /// screen. Ghosts can watch but can't help: they aren't heard by the living (NetworkVoice) and make no
    /// noise. Owner only; local camera work, nothing networked beyond the death itself.
    /// </summary>
    public class GhostSpectator : MonoBehaviour
    {
        [SerializeField] private NetworkPlayer player;
        [SerializeField] private PlayerInputReader inputReader;
        [SerializeField] private CinemachineCamera ghostCamera;
        [SerializeField, Min(0f)] private float delay = 2f;
        [SerializeField] private float distance = 3.5f, height = 1.2f, sensitivity = 0.15f;

        private float deadSince = -1f, orbit, pitch = 15f;
        private int followIndex;
        private GUIStyle style, mark;

        public bool Spectating { get; private set; }
        public NetworkPlayer Following { get; private set; }

        private void LateUpdate()
        {
            if (player == null || !player.IsSpawned || !player.IsOwner) return;
            if (!player.IsDead)
            {
                deadSince = -1f;
                SetSpectating(false);
                return;
            }
            if (deadSince < 0f) deadSince = Time.time;
            if (Time.time - deadSince < delay) return;

            List<NetworkPlayer> alive = NetworkPlayer.All.Where(p => p != null && p != player && p.NetworkManager == player.NetworkManager && !p.IsDead)
                .OrderBy(p => p.OwnerClientId).ToList();
            PlayerInputFrame input = inputReader != null ? inputReader.Current : default;
            if (input.UsePressed) followIndex++;
            if (input.DropPressed) followIndex--;
            if (alive.Count == 0)
            {
                Following = null;
                SetSpectating(false); // nobody left to watch: stay with the body (the run ends)
                return;
            }
            followIndex = ((followIndex % alive.Count) + alive.Count) % alive.Count;
            Following = alive[followIndex];
            SetSpectating(true);

            orbit += input.Look.x * sensitivity;
            pitch = Mathf.Clamp(pitch - input.Look.y * sensitivity, -10f, 60f);
            Vector3 focus = (Following.Ragdoll.IsRagdolled ? Following.Ragdoll.BodyPosition : Following.transform.position) + Vector3.up * height;
            Quaternion around = Quaternion.Euler(pitch, Following.transform.eulerAngles.y + orbit, 0f);
            Vector3 want = focus - around * Vector3.forward * distance;
            // Don't look through walls: pull in to whatever is between.
            if (Physics.Linecast(focus, want, out RaycastHit hit, ~LayerMask.GetMask(GameLayers.Player, GameLayers.Loot, GameLayers.Debris, "Ignore Raycast"), QueryTriggerInteraction.Ignore))
                want = hit.point + hit.normal * 0.2f;
            ghostCamera.transform.SetPositionAndRotation(want, Quaternion.LookRotation(focus - want));
        }

        private void SetSpectating(bool on)
        {
            if (Spectating == on) return;
            Spectating = on;
            ghostCamera.gameObject.SetActive(on);
            ghostCamera.Priority = on ? 100 : 0;
        }

        private void OnGUI()
        {
            if (!Spectating) return;
            style ??= new GUIStyle(GUI.skin.label) { fontSize = 18, alignment = TextAnchor.UpperCenter, richText = true };
            mark ??= new GUIStyle(GUI.skin.label) { fontSize = 16, alignment = TextAnchor.MiddleCenter, richText = true };
            GUI.Label(new Rect(0f, Screen.height - 70f, Screen.width, 30f),
                $"<color=#bbbbff>GHOST - watching Player {Following.OwnerClientId + 1}   (left/right mouse: switch)</color>", style);
            Camera cam = Camera.main;
            if (cam == null) return;
            // Ghosts see threats (GDD 11).
            foreach (BlindOne b in BlindOne.All)
            {
                if (b == null) continue;
                Vector3 s = cam.WorldToScreenPoint(b.transform.position + Vector3.up * 2.6f);
                if (s.z > 0f) GUI.Label(new Rect(s.x - 60f, Screen.height - s.y - 12f, 120f, 24f), "<color=#ff4444>▼ BLIND ONE</color>", mark);
            }
        }
    }
}
