using System.Collections.Generic;
using Abandoned.Audio;
using Abandoned.Core;
using Abandoned.Networking;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;

namespace Abandoned.UI
{
    /// <summary>
    /// The game's menus on one UI Toolkit document (M7.4), living on the session object: the main menu
    /// while offline (play solo / host, join, settings, credits, quit), the pause menu on Esc during a
    /// game (resume, settings, the crew and invites, leave). The game keeps running behind the pause
    /// menu: it's multiplayer, and solo is just hosting alone.
    /// </summary>
    public class MenuUi : MonoBehaviour
    {
        [SerializeField] private UIDocument document;
        [SerializeField] private NetworkBootstrap bootstrap;
        [OptionalReference, SerializeField] private SteamLobby lobby;
        [SerializeField] private Font font;
        [SerializeField] private Font titleFont;
        [Tooltip("Credits lines (generated from Docs/ASSET_CREDITS.md).")]
        [OptionalReference, SerializeField] private TextAsset credits;

        private readonly Stack<MenuScreen> pushed = new();
        private readonly Dictionary<MenuScreen, VisualElement> views = new();
        private MainMenuView main;
        private PlayView play;
        private PauseMenuView pause;
        private SubtitleView subtitles;
        private VisualElement hud;
        private VisualElement wear;
        private float nextCrtTwitch, crtTwitchUntil;
        private WardrobeView wardrobe;
        private HowToPlayView howToPlay;
        private DemoEndView demoEnd;
        private AchievementsView achievements;
        private bool demoEndShown;
        private bool paused, pausedForScreen;

        public static MenuUi Current { get; private set; }
        public MenuScreen Showing { get; private set; } = (MenuScreen)(-1);
        public NetworkBootstrap Bootstrap => bootstrap;
        public SteamLobby Lobby => lobby;
        public Font TitleFont => titleFont;
        public SubtitleView Subtitles => subtitles;
        public VisualElement Surface => document != null ? document.rootVisualElement : null;

        private PanelSettings scaledPanel;

        // UI Scale (UI step 5): a runtime copy of the panel settings whose reference size shrinks as the UI grows,
        // so the project's asset is never edited. Everything on this document (menus, HUD, screens) follows.
        private void ApplyUiScale()
        {
            if (document == null || document.panelSettings == null) return;
            if (scaledPanel == null)
            {
                scaledPanel = Instantiate(document.panelSettings);
                scaledPanel.name = document.panelSettings.name + " (scaled)";
                document.panelSettings = scaledPanel;
            }
            float s = GameSettings.UiScale;
            var wanted = new Vector2Int(Mathf.RoundToInt(1920f / s), Mathf.RoundToInt(1080f / s));
            if (scaledPanel.referenceResolution != wanted) scaledPanel.referenceResolution = wanted;
        }

        private void OnEnable()
        {
            Current = this;
            ApplyUiScale();
            GameSettings.Changed += ApplyUiScale;
            VisualElement root = document.rootVisualElement;
            root.Clear();
            UiKit.FillScreen(root);
            root.style.flexDirection = FlexDirection.Column;
            root.style.overflow = Overflow.Hidden;
            root.AddToClassList("menu-root");
            root.pickingMode = PickingMode.Ignore;
            UiThemeAssets theme = Resources.Load<UiThemeAssets>(UiThemeAssets.ResourcePath);
            if (theme != null)
            {
                if (theme.Styles != null && !root.styleSheets.Contains(theme.Styles)) root.styleSheets.Add(theme.Styles);
                if (theme.BodyFont != null) font = theme.BodyFont;
                if (theme.TitleFont != null) titleFont = theme.TitleFont;
            }
            if (font != null) root.style.unityFontDefinition = FontDefinition.FromFont(font);
            root.RegisterCallback<GeometryChangedEvent>(OnRootGeometry);
            // The HUD sits under every menu; HUD components fill it (HudLayer).
            hud = new VisualElement { pickingMode = PickingMode.Ignore };
            UiKit.FillScreen(hud);
            hud.AddToClassList("hud-layer");
            root.Add(hud);
            HudLayer.Attach(hud);
            main = new MainMenuView(this);
            pause = new PauseMenuView(this);
            views[MenuScreen.Main] = main.Root;
            views[MenuScreen.Pause] = pause.Root;
            play = new PlayView(this);
            views[MenuScreen.Play] = play.Root;
            views[MenuScreen.Settings] = new SettingsView(this).Root;
            views[MenuScreen.Controls] = new ControlsView(this).Root;
            views[MenuScreen.Credits] = new CreditsView(this, credits).Root;
            wardrobe = new WardrobeView(this);
            views[MenuScreen.Wardrobe] = wardrobe.Root;
            howToPlay = new HowToPlayView(this);
            views[MenuScreen.HowToPlay] = howToPlay.Root;
            demoEnd = new DemoEndView(this);
            views[MenuScreen.DemoEnd] = demoEnd.Root;
            achievements = new AchievementsView(this);
            views[MenuScreen.Achievements] = achievements.Root;
            foreach (VisualElement v in views.Values)
            {
                UiKit.FillScreen(v);
                v.style.flexDirection = FlexDirection.Column;
                v.style.justifyContent = Justify.Center;
                v.style.alignItems = v == main.Root ? Align.FlexStart : Align.Center;
                v.style.display = DisplayStyle.None;
                root.Add(v);
            }
            wear = new VisualElement { pickingMode = PickingMode.Ignore };
            UiKit.FillScreen(wear);
            wear.AddToClassList("horror-grime");
            root.Add(wear);
            subtitles = new SubtitleView();
            root.Add(subtitles.Root);
            SubtitleFeed.Heard += subtitles.Add;
            Showing = (MenuScreen)(-1);
            nextCrtTwitch = Time.unscaledTime + 11f;
            if (GetComponent<HorrorRunHud>() == null) gameObject.AddComponent<HorrorRunHud>();
        }

        private void OnDisable()
        {
            if (document != null) document.rootVisualElement.UnregisterCallback<GeometryChangedEvent>(OnRootGeometry);
            GameSettings.Changed -= ApplyUiScale;
            CursorOwner.Set(this, false);
            if (subtitles != null) SubtitleFeed.Heard -= subtitles.Add;
            if (Current == this) Current = null;
        }

        private void OnRootGeometry(GeometryChangedEvent changed)
        {
            VisualElement root = document.rootVisualElement;
            root.EnableInClassList("menu-compact", changed.newRect.width < 1350f);
            root.EnableInClassList("menu-short", changed.newRect.height < 850f);
        }

        private void Update()
        {
            subtitles?.Tick();
            if (bootstrap == null) return;
            bool running = bootstrap.IsRunning;
            AutoOpenDemoEnd(running);
            if (!running) paused = pausedForScreen = false;
            if (EscapePressed() && !ControlsView.Busy && !ChatView.Typing)
            {
                if (pushed.Count > 0) Back();
                else if (running && paused) Resume();
                else if (running && !CursorOwner.UiActive) OpenPause();
            }

            MenuScreen baseScreen = !running ? MenuScreen.Main : paused ? MenuScreen.Pause : MenuScreen.None;
            if (baseScreen == MenuScreen.None) pushed.Clear();
            MenuScreen top = pushed.Count > 0 ? pushed.Peek() : baseScreen;
            if (top != Showing) Show(top);
            bool menuVisible = top != MenuScreen.None || CursorOwner.UiActive;
            MenuKit.Show(wear, menuVisible);
            if (menuVisible && Time.unscaledTime >= nextCrtTwitch)
            {
                crtTwitchUntil = Time.unscaledTime + 0.07f;
                nextCrtTwitch = Time.unscaledTime + Random.Range(9f, 17f);
            }
            document.rootVisualElement.EnableInClassList("crt-twitch", menuVisible && Time.unscaledTime < crtTwitchUntil);
            // During a game the menu needs the mouse; offline there's no player holding it anyway.
            CursorOwner.Set(this, running && top != MenuScreen.None);
            if (!running && Cursor.lockState != CursorLockMode.None)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            if (top == MenuScreen.Main) main.Refresh();
            else if (top == MenuScreen.Play) play.Refresh();
            else if (top == MenuScreen.Pause) pause.Refresh();
            if (Keyboard.current != null && Keyboard.current.f10Key.wasPressedThisFrame) ClipMode = !ClipMode;
            // M10.11: a free camera for trailer shots, only in a game (and gone when it ends).
            if (running && Keyboard.current != null && Keyboard.current.f9Key.wasPressedThisFrame) Player.FreeCamera.Toggle();
            else if (!running && Player.FreeCamera.Active) Player.FreeCamera.Toggle();
        }

        private bool clipMode;

        /// <summary>
        /// Clip mode (M8.7, F10): the HUD, tips and captions go away so the screen is just the game, for
        /// capturing trailer shots (GDD 29). Menus still open over it.
        /// </summary>
        public bool ClipMode
        {
            get => clipMode;
            set
            {
                clipMode = value;
                MenuKit.Show(hud, Showing == MenuScreen.None && !clipMode);
                if (subtitles != null) MenuKit.Show(subtitles.Root, !clipMode);
            }
        }

        // The demo's last job is done: once per arrival at the HQ, the crew gets the end screen.
        private void AutoOpenDemoEnd(bool running)
        {
            Company.CompanyService company = Company.CompanyService.Current;
            bool over = running && company != null && company.IsSpawned && company.DemoOver
                        && FindAnyObjectByType<Company.ContractBoard>() != null;
            if (!over) { demoEndShown = false; return; }
            if (!demoEndShown && !paused && !CursorOwner.UiActive) OpenDemoEnd();
        }

        public void OpenPause()
        {
            paused = true;
            pausedForScreen = false;
            GameAudio.PlayUi(SoundId.UiOpen);
        }

        public void Resume()
        {
            if (!paused) return;
            paused = pausedForScreen = false;
            pushed.Clear();
            GameAudio.PlayUi(SoundId.UiClose);
            CursorOwner.RequestCapture();
        }

        public void Push(MenuScreen screen)
        {
            pushed.Push(screen);
            if (screen == MenuScreen.HowToPlay) howToPlay.Refresh();
            if (screen == MenuScreen.Achievements) achievements.Refresh();
        }

        /// <summary>A screen opened from the world (the HQ lockers): Back returns straight to the game.</summary>
        public void OpenWardrobe()
        {
            if (!OpenFromWorld(MenuScreen.Wardrobe)) return;
            wardrobe.Refresh();
        }

        /// <summary>The demo's end screen (M8.3), shown on arriving at the HQ once the demo's jobs are done.</summary>
        public void OpenDemoEnd()
        {
            if (!OpenFromWorld(MenuScreen.DemoEnd)) return;
            demoEndShown = true;
            demoEnd.Refresh();
        }

        private bool OpenFromWorld(MenuScreen screen)
        {
            if (bootstrap == null || !bootstrap.IsRunning) return false;
            paused = pausedForScreen = true;
            pushed.Clear();
            pushed.Push(screen);
            GameAudio.PlayUi(SoundId.UiOpen);
            return true;
        }

        public void Back()
        {
            if (pushed.Count == 0) return;
            pushed.Pop();
            GameAudio.PlayUi(SoundId.UiBack);
            Prefs.Save(); // leaving a settings screen by Esc counts as done too
            Voice.VoiceSettings.Flush();
            if (pushed.Count == 0 && pausedForScreen) Resume();
        }

        public void Quit()
        {
            Prefs.Save();
            Voice.VoiceSettings.Flush();
            Application.Quit();
        }

        private void Show(MenuScreen screen)
        {
            Showing = screen;
            if (screen != MenuScreen.Wardrobe) WardrobePreview.Hide();
            foreach (KeyValuePair<MenuScreen, VisualElement> v in views) MenuKit.Show(v.Value, v.Key == screen);
            MenuKit.Show(hud, screen == MenuScreen.None && !clipMode);
        }

        private static bool EscapePressed() => Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;

#if UNITY_EDITOR
        public void EditorSetup(UIDocument doc, NetworkBootstrap networkBootstrap, SteamLobby steamLobby, Font body, Font title, TextAsset creditLines)
        {
            document = doc;
            bootstrap = networkBootstrap;
            lobby = steamLobby;
            font = body;
            titleFont = title;
            credits = creditLines;
        }
#endif
    }
}
