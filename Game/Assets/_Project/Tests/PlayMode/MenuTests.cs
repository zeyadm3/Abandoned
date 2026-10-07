using System.Collections;
using System.IO;
using Abandoned.Core;
using Abandoned.Networking;
using Abandoned.Player;
using Abandoned.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Abandoned.Tests
{
    /// <summary>
    /// M7.4: the menus at the HQ. Esc opens the pause menu (it takes the mouse), settings stack on top
    /// and Back returns, Resume gives the mouse back to the game; leaving the game lands on the main menu.
    /// </summary>
    public class MenuTests
    {
        [UnityTest]
        public IEnumerator PauseSettingsResumeThenLeaveToTheMainMenu()
        {
            yield return TestMapScene.Load("HQ");
            MenuUi menu = MenuUi.Current;
            Assert.IsNotNull(menu, "the session has menus");
            yield return null;
            Assert.AreEqual(MenuScreen.None, menu.Showing, "playing: no menu");

            menu.OpenPause();
            yield return null;
            Assert.AreEqual(MenuScreen.Pause, menu.Showing);
            Assert.IsTrue(CursorOwner.UiActive, "the pause menu has the mouse");
            yield return Capture(menu, "M7_menu_pause");
            menu.Push(MenuScreen.Settings);
            yield return null;
            Assert.AreEqual(MenuScreen.Settings, menu.Showing);
            yield return Capture(menu, "M7_menu_settings");
            menu.Push(MenuScreen.Controls);
            yield return null;
            Assert.AreEqual(MenuScreen.Controls, menu.Showing);
            yield return Capture(menu, "M7_menu_controls");
            menu.Back();
            menu.Back();
            yield return null;
            Assert.AreEqual(MenuScreen.Pause, menu.Showing, "Back returns to the pause menu");

            menu.Resume();
            yield return null;
            yield return null;
            Assert.AreEqual(MenuScreen.None, menu.Showing);
            Assert.IsFalse(CursorOwner.UiActive);
            Assert.IsTrue(TestMapScene.Player.GetComponent<PlayerLook>().CursorCaptured, "back in the game: the look has the mouse again");

            menu.Bootstrap.Disconnect();
            float until = Time.time + 8f;
            while (Time.time < until && (MenuUi.Current == null || MenuUi.Current.Bootstrap.IsRunning || MenuUi.Current.Showing != MenuScreen.Main))
                yield return null;
            Assert.IsNotNull(MenuUi.Current);
            Assert.AreEqual(MenuScreen.Main, MenuUi.Current.Showing, "offline at the HQ: the main menu");
            Assert.IsFalse(MenuUi.Current.Bootstrap.IsRunning, "leaving on purpose doesn't auto-host again");
            yield return Capture(MenuUi.Current, "M7_menu_main");
            MenuUi.Current.Push(MenuScreen.Credits);
            yield return null;
            yield return Capture(MenuUi.Current, "M7_menu_credits");
        }

        /// <summary>The scene camera plus the menu on top, into Game/Screenshots (for eyeballing the layout).</summary>
        internal static IEnumerator Capture(MenuUi menu, string name, int width = 1600, int height = 900)
        {
            PanelSettings panel = menu.GetComponent<UIDocument>().panelSettings;
            Camera camera = Camera.main;
            var rt = new RenderTexture(width, height, 24);
            bool clear = panel.clearColor;
            // The UI lays out at the capture size first (world tags move to their new spots), then the scene is
            // drawn by hand over whatever that left (batch mode never reaches end of frame), then the UI once on top.
            panel.clearColor = false;
            panel.targetTexture = rt;
            if (camera != null) camera.targetTexture = rt; // same aspect for world-anchored tags while they lay out
            yield return null;
            yield return null;
            if (camera != null) camera.Render();
            yield return null;
            if (camera != null) camera.targetTexture = null;
            RenderTexture.active = rt;
            var tex = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            RenderTexture.active = null;
            Directory.CreateDirectory(ScreenshotCapture.Folder);
            File.WriteAllBytes(Path.Combine(ScreenshotCapture.Folder, name + ".png"), tex.EncodeToPNG());
            Object.Destroy(tex);
            panel.targetTexture = null;
            panel.clearColor = clear;
            rt.Release();
            Object.Destroy(rt);
        }
    }
}
