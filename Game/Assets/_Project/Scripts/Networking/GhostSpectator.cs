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
    /// Death = ghost spectator for the rest of the run (GDD 11). UI step 3: first the camera pulls out and
    /// circles your own ragdoll for a few seconds (the "what just happened" moment), then YOU DIED with what
    /// killed you, then the view leaves the body and orbits a living teammate (left/right mouse to switch),
    /// with threats marked on screen. Ghosts can watch but can't help: they aren't heard by the living (NetworkVoice) and make no
    /// noise. Owner only; local camera work, nothing networked beyond the death itself.
    /// </summary>
    public class GhostSpectator : MonoBehaviour
    {
        [SerializeField] private NetworkPlayer player;
        [SerializeField] private PlayerInputReader inputReader;
        [SerializeField] private CinemachineCamera ghostCamera;
        [Tooltip("Seconds watching your own body before YOU DIED, and before becoming a ghost.")]
        [SerializeField, Min(0f)] private float deathCamSeconds = 2.5f;
        [SerializeField, Min(0f)] private float delay = 5.5f;
        [SerializeField] private float distance = 3.5f, height = 1.2f, sensitivity = 0.15f;

        private float deadSince = -1f, orbit, pitch = 15f;
        private int followIndex;
        private UnityEngine.UIElements.VisualElement ghostBar, deathCard;
        private UnityEngine.UIElements.Label deathCause;
        private string ghostShown;
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
            if (Time.time - deadSince < delay)
            {
                DeathCam(Time.time - deadSince);
                return;
            }

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
            if (Spectating == on && ghostCamera.gameObject.activeSelf == on) return;
            Spectating = on;
            ghostCamera.gameObject.SetActive(on);
            ghostCamera.Priority = on ? 100 : 0;
        }

        /// <summary>The seconds since death that are the death camera / the YOU DIED card (tests, the HUD).</summary>
        public bool ShowingDeathCard => player != null && player.IsDead && deadSince >= 0f && Time.time - deadSince >= deathCamSeconds && Time.time - deadSince < delay;

        // Your own body from above and behind, slowly circling and pulling back (the ghost camera, borrowed).
        private void DeathCam(float t)
        {
            if (!ghostCamera.gameObject.activeSelf)
            {
                ghostCamera.gameObject.SetActive(true);
                ghostCamera.Priority = 100;
            }
            Vector3 body = player.Ragdoll.IsRagdolled ? player.Ragdoll.BodyPosition : player.transform.position;
            float yaw = player.transform.eulerAngles.y + 160f + t * 14f;
            float back = Mathf.Lerp(1.6f, 3.4f, Mathf.SmoothStep(0f, 1f, t / delay));
            Vector3 want = body + Quaternion.Euler(0f, yaw, 0f) * Vector3.forward * back + Vector3.up * Mathf.Lerp(1.4f, 2.6f, t / delay);
            if (Physics.Linecast(body + Vector3.up * 0.3f, want, out RaycastHit hit, ~LayerMask.GetMask(GameLayers.Player, GameLayers.Loot, GameLayers.Debris, "Ignore Raycast"), QueryTriggerInteraction.Ignore))
                want = hit.point + hit.normal * 0.2f;
            ghostCamera.transform.SetPositionAndRotation(want, Quaternion.LookRotation(body - want));
        }

        // The ghost's HUD (M8.1): who you're watching, and a mark over every threat (ghosts see them, GDD 11).
        private void Update()
        {
            bool on = Spectating;
            DeathCard();
            if (on && ghostBar == null) ghostBar = UI.HudLayer.Add(new UnityEngine.UIElements.VisualElement(), "ghost-bar");
            if (ghostBar != null)
            {
                UI.MenuKit.Show(ghostBar, on);
                if (on)
                {
                    string line = $"GHOST   Watching {Following.DisplayName}   [{InputBindings.Display("Drop")}] previous   [{InputBindings.Display("Use")}] next";
                    if (line != ghostShown)
                    {
                        ghostShown = line;
                        UI.UiKit.Prompt(ghostBar, line);
                    }
                }
            }
            Camera cam = Camera.main;
            foreach (Threat t in Threat.All)
            {
                if (t == null) continue;
                if (!threatMarks.TryGetValue(t, out UnityEngine.UIElements.Label mark))
                {
                    if (!on) continue;
                    mark = UI.HudLayer.Label("hud-marker", "threat-mark");
                    if (mark == null) continue;
                    mark.text = $"\u25BC {t.DisplayName.ToUpperInvariant()}";
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

        // YOU DIED and what did it, between the death camera and the ghost.
        private void DeathCard()
        {
            bool show = ShowingDeathCard;
            if (deathCard == null)
            {
                if (!show || UI.HudLayer.Root == null) return;
                deathCard = UI.HudLayer.Add(new UnityEngine.UIElements.VisualElement(), "death-card");
                var title = new UnityEngine.UIElements.Label("YOU DIED") { pickingMode = UnityEngine.UIElements.PickingMode.Ignore };
                title.AddToClassList("death-card__title");
                deathCard.Add(title);
                deathCause = new UnityEngine.UIElements.Label { pickingMode = UnityEngine.UIElements.PickingMode.Ignore };
                deathCause.AddToClassList("death-card__cause");
                deathCard.Add(deathCause);
                var note = new UnityEngine.UIElements.Label("Your pockets dropped where you fell. Watch your crew, or wait for a medkit.") { pickingMode = UnityEngine.UIElements.PickingMode.Ignore };
                note.AddToClassList("death-card__note");
                deathCard.Add(note);
            }
            if (show && deathCause.text != player.DeathCause) deathCause.text = player.DeathCause;
            UI.MenuKit.Show(deathCard, show);
        }

        private void OnDestroy()
        {
            UI.HudLayer.Remove(ghostBar);
            UI.HudLayer.Remove(deathCard);
            foreach (UnityEngine.UIElements.Label l in threatMarks.Values) UI.HudLayer.Remove(l);
        }
    }
}
