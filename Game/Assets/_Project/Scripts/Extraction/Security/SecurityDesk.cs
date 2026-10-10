using Abandoned.Core;
using Abandoned.Networking;
using Abandoned.Player;
using Abandoned.UI;
using UnityEngine;

namespace Abandoned.Extraction
{
    /// <summary>
    /// The security office's desk (0.16.7): E sits the local player at the monitors, which then fill the
    /// screen with one camera at a time (left/right or click to switch, E or jump to get up). Watching is
    /// local and costs one extra camera render on this machine only. Needs power in its sector (or the
    /// sector's breaker). The watcher is still standing at the desk, and can't see behind them.
    /// </summary>
    public class SecurityDesk : MonoBehaviour, IUsable, IHintSource
    {
        private const int FeedWidth = 640, FeedHeight = 360;

        [SerializeField] private Renderer monitor;
        [SerializeField] private int sector;

        private RenderTexture feed;
        private int current;
        private bool watching;
        private NetworkPlayer watcher;
        private PlayerInputReader input;
        private float switchReadyAt, openedAt;
        private MaterialPropertyBlock block;

        public HintId Hint => HintId.Security;

        public bool Powered => SectorPower.Live(sector);

        public string UsePrompt(GameObject user)
        {
            if (SecurityCamera.All.Count == 0) return null;
            return Powered ? "Watch the security cameras" : "The monitors are dead (no power here)";
        }

        public void Use(GameObject user)
        {
            NetworkPlayer me = user != null ? user.GetComponentInParent<NetworkPlayer>() : null;
            if (me == null || !me.IsOwner || watching || !Powered || SecurityCamera.All.Count == 0) return;
            watcher = me;
            input = me.GetComponent<PlayerInputReader>();
            watching = true;
            openedAt = Time.time;
            me.OwnerSetControl(false);
            if (feed == null) feed = new RenderTexture(FeedWidth, FeedHeight, 16) { name = "SecurityFeed" };
            Show(current);
        }

        private void Update()
        {
            if (!watching) return;
            bool leave = watcher == null || watcher.IsDead || !Powered;
            if (!leave && input != null && Time.time > openedAt + 0.25f)
            {
                PlayerInputFrame frame = input.Current;
                leave = frame.InteractPressed || frame.JumpPressed || frame.PausePressed;
                if (Time.time >= switchReadyAt && (Mathf.Abs(frame.Move.x) > 0.6f || frame.UsePressed))
                {
                    switchReadyAt = Time.time + 0.3f;
                    int step = frame.Move.x < -0.6f ? -1 : 1;
                    Show((current + step + SecurityCamera.All.Count) % SecurityCamera.All.Count);
                }
            }
            if (leave) Stop();
        }

        private void Show(int index)
        {
            for (int i = 0; i < SecurityCamera.All.Count; i++)
            {
                Camera lens = SecurityCamera.All[i].Lens;
                if (lens == null) continue;
                lens.targetTexture = i == index ? feed : null;
                lens.enabled = i == index;
            }
            current = index;
            if (monitor != null)
            {
                block ??= new MaterialPropertyBlock();
                monitor.GetPropertyBlock(block);
                block.SetTexture("_BaseMap", feed);
                block.SetColor("_EmissionColor", Color.white * 0.6f);
                monitor.SetPropertyBlock(block);
            }
        }

        private void Stop()
        {
            watching = false;
            foreach (SecurityCamera cam in SecurityCamera.All)
                if (cam != null && cam.Lens != null)
                {
                    cam.Lens.enabled = false;
                    cam.Lens.targetTexture = null;
                }
            if (watcher != null && !watcher.IsDead) watcher.OwnerSetControl(true);
            watcher = null;
        }

        private void OnDisable()
        {
            if (watching) Stop();
        }

        private void OnDestroy()
        {
            if (feed != null) feed.Release();
        }

        // The feed, full screen, in the green of an old CRT, with the camera's name and a clock.
        private void OnGUI()
        {
            if (!watching || feed == null || current >= SecurityCamera.All.Count) return;
            var full = new Rect(0, 0, Screen.width, Screen.height);
            GUI.color = Color.black;
            GUI.DrawTexture(full, Texture2D.whiteTexture);
            GUI.color = new Color(0.75f, 1f, 0.8f, 1f);
            GUI.DrawTexture(full, feed, ScaleMode.ScaleToFit);
            GUI.color = new Color(0.8f, 1f, 0.8f, 0.95f);
            var style = new GUIStyle(GUI.skin.label) { fontSize = Mathf.RoundToInt(Screen.height * 0.028f) };
            string clock = System.TimeSpan.FromSeconds(Time.time).ToString(@"hh\:mm\:ss");
            GUI.Label(new Rect(Screen.width * 0.04f, Screen.height * 0.04f, Screen.width * 0.6f, Screen.height * 0.06f),
                $"{SecurityCamera.All[current].Label}   {clock}", style);
            style.alignment = TextAnchor.LowerCenter;
            style.fontSize = Mathf.RoundToInt(Screen.height * 0.022f);
            GUI.Label(new Rect(0, Screen.height * 0.88f, Screen.width, Screen.height * 0.08f),
                $"A / D or click: next camera     {InputBindings.Display("Interact")}: get up", style);
        }

#if UNITY_EDITOR
        public void EditorSetup(Renderer screen, int powerSector)
        {
            monitor = screen;
            sector = powerSector;
        }
#endif
    }
}
