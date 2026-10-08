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
    /// <summary>Moving between screens: pause, resume, push, back, world-opened screens and quitting.</summary>
    public partial class MenuUi
    {
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
            MenuScreen previous = Showing;
            Showing = screen;
            // Screens never just swap: boot and the pause menu flicker in, going back to the game switches
            // the "set" off like an old CRT, and moving between menus is a hard cut through static.
            if (previous == (MenuScreen)(-1)) fx.Play(MenuTransition.FlickerIn);
            else if (screen == MenuScreen.None) fx.Play(MenuTransition.CrtOff);
            else if (previous == MenuScreen.None) fx.Play(MenuTransition.FlickerIn);
            else fx.Play(MenuTransition.StaticCut);
            if (previous == MenuScreen.None && views.TryGetValue(screen, out VisualElement opening)) fx.FadeIn(opening);
            if (screen != MenuScreen.Wardrobe) WardrobePreview.Hide();
            foreach (KeyValuePair<MenuScreen, VisualElement> v in views) MenuKit.Show(v.Value, v.Key == screen);
            MenuKit.Show(hud, screen == MenuScreen.None && !clipMode);
        }
    }
}
