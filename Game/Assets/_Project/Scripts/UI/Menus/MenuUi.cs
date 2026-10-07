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
        private PauseMenuView pause;
        private SubtitleView subtitles;
        private VisualElement hud;
        private WardrobeView wardrobe;
        private bool paused, pausedForScreen;

        public static MenuUi Current { get; private set; }
        public MenuScreen Showing { get; private set; } = (MenuScreen)(-1);
        public NetworkBootstrap Bootstrap => bootstrap;
        public SteamLobby Lobby => lobby;
        public Font TitleFont => titleFont;
        public SubtitleView Subtitles => subtitles;

        private void OnEnable()
        {
            Current = this;
            VisualElement root = document.rootVisualElement;
            root.Clear();
            root.AddToClassList("menu-root");
            root.pickingMode = PickingMode.Ignore;
            if (font != null) root.style.unityFontDefinition = FontDefinition.FromFont(font);
            // The HUD sits under every menu; HUD components fill it (HudLayer).
            hud = new VisualElement { pickingMode = PickingMode.Ignore };
            hud.AddToClassList("hud-layer");
            root.Add(hud);
            HudLayer.Attach(hud);
            main = new MainMenuView(this);
            pause = new PauseMenuView(this);
            views[MenuScreen.Main] = main.Root;
            views[MenuScreen.Pause] = pause.Root;
            views[MenuScreen.Settings] = new SettingsView(this).Root;
            views[MenuScreen.Controls] = new ControlsView(this).Root;
            views[MenuScreen.Credits] = new CreditsView(this, credits).Root;
            wardrobe = new WardrobeView(this);
            views[MenuScreen.Wardrobe] = wardrobe.Root;
            foreach (VisualElement v in views.Values) root.Add(v);
            subtitles = new SubtitleView();
            root.Add(subtitles.Root);
            SubtitleFeed.Heard += subtitles.Add;
            Showing = (MenuScreen)(-1);
        }

        private void OnDisable()
        {
            CursorOwner.Set(this, false);
            if (subtitles != null) SubtitleFeed.Heard -= subtitles.Add;
            if (Current == this) Current = null;
        }

        private void Update()
        {
            subtitles?.Tick();
            if (bootstrap == null) return;
            bool running = bootstrap.IsRunning;
            if (!running) paused = pausedForScreen = false;
            if (EscapePressed() && !ControlsView.Busy)
            {
                if (pushed.Count > 0) Back();
                else if (running && paused) Resume();
                else if (running && !CursorOwner.UiActive) OpenPause();
            }

            MenuScreen baseScreen = !running ? MenuScreen.Main : paused ? MenuScreen.Pause : MenuScreen.None;
            if (baseScreen == MenuScreen.None) pushed.Clear();
            MenuScreen top = pushed.Count > 0 ? pushed.Peek() : baseScreen;
            if (top != Showing) Show(top);
            // During a game the menu needs the mouse; offline there's no player holding it anyway.
            CursorOwner.Set(this, running && top != MenuScreen.None);
            if (!running && Cursor.lockState != CursorLockMode.None)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            if (top == MenuScreen.Main) main.Refresh();
            else if (top == MenuScreen.Pause) pause.Refresh();
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

        public void Push(MenuScreen screen) => pushed.Push(screen);

        /// <summary>A screen opened from the world (the HQ lockers): Back returns straight to the game.</summary>
        public void OpenWardrobe()
        {
            if (bootstrap == null || !bootstrap.IsRunning) return;
            paused = pausedForScreen = true;
            pushed.Clear();
            pushed.Push(MenuScreen.Wardrobe);
            wardrobe.Refresh();
            GameAudio.PlayUi(SoundId.UiOpen);
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
            foreach (KeyValuePair<MenuScreen, VisualElement> v in views) MenuKit.Show(v.Value, v.Key == screen);
            MenuKit.Show(hud, screen == MenuScreen.None);
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
