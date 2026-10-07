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
        private UnityEngine.UIElements.Label ghostLine;
        private readonly Dictionary<Threat, UnityEngine.UIElements.Label> threatMarks = new();

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

        // The ghost's HUD (M8.1): who you're watching, and a mark over every threat (ghosts see them, GDD 11).
        private void Update()
        {
            bool on = Spectating;
            if (on && ghostLine == null) ghostLine = UI.HudLayer.Label("hud-ghost");
            if (ghostLine != null)
            {
                UI.MenuKit.Show(ghostLine, on);
                if (on)
                {
                    string line = $"GHOST - WATCHING PLAYER {Following.OwnerClientId + 1}   ({InputBindings.Display("Use")} / {InputBindings.Display("Drop")}: SWITCH)";
                    if (ghostLine.text != line) ghostLine.text = line;
                }
            }
            Camera cam = Camera.main;
            foreach (Threat t in Threat.All)
            {
                if (t == null) continue;
                if (!threatMarks.TryGetValue(t, out UnityEngine.UIElements.Label mark))
                {
                    if (!on) continue;
                    mark = UI.HudLayer.Label("hud-marker");
                    if (mark == null) continue;
                    mark.text = $"<color=#ff4444>▼ {t.DisplayName.ToUpperInvariant()}</color>";
                    threatMarks[t] = mark;
                }
                UI.MenuKit.Show(mark, on && cam != null);
                if (on && cam != null) UI.HudLayer.Place(mark, t.transform.position + Vector3.up * 2.6f, cam);
            }
            foreach (Threat gone in threatMarks.Keys.Where(k => k == null).ToList())
            {
                UI.HudLayer.Remove(threatMarks[gone]);
                threatMarks.Remove(gone);
            }
        }

        private void OnDestroy()
        {
            UI.HudLayer.Remove(ghostLine);
            foreach (UnityEngine.UIElements.Label l in threatMarks.Values) UI.HudLayer.Remove(l);
        }
    }
}
