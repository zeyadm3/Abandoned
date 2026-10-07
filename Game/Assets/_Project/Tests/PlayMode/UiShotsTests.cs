using System.Collections;
using Abandoned.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Abandoned.Tests
{
    /// <summary>
    /// UI overhaul: renders each screen at 16:9, 16:10 and ultrawide into Game/Screenshots (UI_*) so the
    /// layout can be checked by eye. Asserts only that the screens exist; the pictures are the point.
    /// </summary>
    public class UiShotsTests
    {
        private static readonly (string tag, int w, int h)[] Sizes = { ("169", 1600, 900), ("1610", 1440, 900), ("uw", 2560, 1080) };

        internal static IEnumerator Shots(string name)
        {
            foreach ((string tag, int w, int h) in Sizes) yield return MenuTests.Capture(MenuUi.Current, $"UI_{name}_{tag}", w, h);
        }

        [UnityTest]
        public IEnumerator MenusAtEveryAspect()
        {
            yield return TestMapScene.Load("HQ");
            MenuUi menu = MenuUi.Current;
            Assert.IsNotNull(menu);
            menu.OpenPause();
            yield return null;
            yield return Shots("pause");
            menu.Push(MenuScreen.Settings);
            yield return null;
            yield return Shots("settings");
            menu.Back();
            menu.Back();
            yield return null;
            menu.Bootstrap.Disconnect();
            float until = Time.time + 8f;
            while (Time.time < until && (MenuUi.Current == null || MenuUi.Current.Showing != MenuScreen.Main)) yield return null;
            Assert.AreEqual(MenuScreen.Main, MenuUi.Current.Showing);
            yield return Shots("main");
        }
    }
}
