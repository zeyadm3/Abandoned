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

        // The feed, full screen (0.22.8: UI Toolkit in the revamp's style, was IMGUI): the picture, the camera's
        // name and a clock in the corner, the keys along the bottom.
        private UnityEngine.UIElements.VisualElement screen;
        private UnityEngine.UIElements.Label caption, help;

        private void LateUpdate()
        {
            if (!watching)
            {
                if (screen != null) screen.style.display = UnityEngine.UIElements.DisplayStyle.None;
                return;
            }
            if ((screen == null || screen.panel == null) && UI.HudLayer.Root != null)
            {
                screen = UI.HudLayer.Add(new UnityEngine.UIElements.VisualElement(), "monitor");
                caption = new UnityEngine.UIElements.Label();
                caption.AddToClassList("monitor__caption");
                screen.Add(caption);
                help = new UnityEngine.UIElements.Label();
                help.AddToClassList("monitor__help");
                screen.Add(help);
            }
            if (screen == null || feed == null || current >= SecurityCamera.All.Count) return;
            screen.style.display = UnityEngine.UIElements.DisplayStyle.Flex;
            screen.style.backgroundImage = UnityEngine.UIElements.Background.FromRenderTexture(feed);
            caption.text = $"{SecurityCamera.All[current].Label}     {System.TimeSpan.FromSeconds(Time.time):hh\\:mm\\:ss}";
            help.text = $"A / D or click   next camera          {InputBindings.Display("Interact")}   get up";
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
