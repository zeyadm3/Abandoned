using System.Collections.Generic;
using Abandoned.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace Abandoned.UI
{
    /// <summary>
    /// The menu screens, and building them again when the language changes (0.24.8). A screen is a plain tree
    /// of elements with its wording fixed when it is built, so another language means new screens. The HUD is
    /// emptied and its pieces build themselves again (0.24.8); world-opened panels rebuild when they see the
    /// language's version change (<see cref="ScreenPanel.Build"/>).
    /// </summary>
    public partial class MenuUi
    {
        private bool languageChanged;

        // Not at once: the click that changed the language is still running inside the Settings screen
        // this would throw away.
        private void OnLanguageChanged() => languageChanged = true;

        private void RebuildForLanguage()
        {
            languageChanged = false;
            LanguageFonts.Order();
            if (document == null || document.rootVisualElement == null) return;
            BuildViews(document.rootVisualElement);
            HudLayer.Clear();
            watermark?.Reword();
            foreach (MenuScreen open in pushed)
            {
                if (open == MenuScreen.HowToPlay) howToPlay.Refresh();
                else if (open == MenuScreen.Achievements) achievements.Refresh();
                else if (open == MenuScreen.Wardrobe) wardrobe.Refresh();
                else if (open == MenuScreen.DemoEnd) demoEnd.Refresh();
            }
            // Update shows the open screen again, now from the new set.
            Showing = (MenuScreen)(-1);
        }

        private void BuildViews(VisualElement root)
        {
            // New screens take the old ones' place: above the HUD and the world's panels, under the grime and effects.
            int at = -1;
            foreach (VisualElement old in views.Values)
            {
                int index = root.IndexOf(old);
                if (index >= 0 && (at < 0 || index < at)) at = index;
            }
            foreach (VisualElement old in views.Values) old.RemoveFromHierarchy();
            views.Clear();

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
            foreach (KeyValuePair<MenuScreen, VisualElement> screen in views)
            {
                VisualElement v = screen.Value;
                v.AddToClassList("menu-screen");
                v.AddToClassList("menu-screen--" + screen.Key.ToString().ToLowerInvariant());
                UiKit.FillScreen(v);
                v.style.flexDirection = FlexDirection.Column;
                v.style.justifyContent = Justify.Center;
                // Menu pages (MenuKit.Page) and the main menu sit against the left edge; notices stay centred.
                v.style.alignItems = v == main.Root || v.ClassListContains("mm-backdrop") ? Align.FlexStart : Align.Center;
                v.style.display = DisplayStyle.None;
                if (at < 0) root.Add(v);
                else root.Insert(at++, v);
            }
        }
    }
}
