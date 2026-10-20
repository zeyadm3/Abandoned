using System.Collections;
using Abandoned.Core;
using Abandoned.Interaction;
using Abandoned.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Abandoned.Tests
{
    /// <summary>Measure rendered text, not just the existence of the menu or prompt.</summary>
    public class UiTextLayoutTests
    {
        private static readonly (int width, int height, float scale)[] Sizes =
        {
            (1280, 720, 1.25f), (1024, 768, 1.5f), (1920, 1080, 1f), (2560, 1080, 1.5f)
        };

        [TearDown]
        public void CloseStations()
        {
            Abandoned.Company.ContractBoard.Open = false;
            Abandoned.Equipment.ShopTerminal.Open = false;
            Abandoned.Equipment.GearRack.Open = false;
        }

        [UnityTest]
        public IEnumerator CrewTextUsesThePanelWidth()
        {
            yield return TestMapScene.Load("HQ");
            MenuUi menu = MenuUi.Current;
            menu.OpenPause();
            yield return null;
            foreach (var size in Sizes)
                yield return AtSize(menu, size, () =>
                {
                    VisualElement crew = menu.Surface.Q(className: "pause__crew-list");
                    Assert.Greater(crew.layout.width, 200f, "crew list must fill the nested scroll viewport");
                    Label tags = crew.Q<Label>(className: "crew-row__tags");
                    Assert.IsNotNull(tags, "the host row exists");
                    AssertSingleLine(tags);
                    Label name = crew.Q<Label>(className: "crew-row__name");
                    Assert.Greater(name.contentRect.width, 120f, "player name has a readable text column");
                    Assert.Less(tags.worldBound.yMax, crew.worldBound.yMax + 1f, "tags stay inside the crew row");
                    VisualElement body = menu.Surface.Q<ScrollView>(className: "pause__crew-body").contentContainer;
                    foreach (VisualElement child in body.Children())
                        Assert.LessOrEqual(child.worldBound.xMax, body.worldBound.xMax + 1f, "crew notes and host rules stay inside the panel");
                }, "crew");
        }

        [UnityTest]
        public IEnumerator WorldPromptsHaveHorizontalTextAndStayOnScreen()
        {
            yield return TestMapScene.Load("HQ");
            MenuUi menu = MenuUi.Current;
            PlayerTestRig.ForExisting(TestMapScene.Player);
            Camera camera = Camera.main;
            var terminal = GameObject.CreatePrimitive(PrimitiveType.Cube);
            terminal.name = "Prompt layout test terminal";
            terminal.transform.position = camera.transform.position + camera.transform.forward * 1.2f;
            terminal.transform.localScale = Vector3.one * .3f;
            terminal.AddComponent<Abandoned.Equipment.ShopTerminal>();
            Physics.SyncTransforms();
            var hud = TestMapScene.Player.GetComponent<InteractionHud>();
            try
            {
                // Use a real usable target and InteractionHud, including positioning/clamping.
                foreach (var size in Sizes)
                    yield return AtSize(menu, size, () =>
                    {
                        VisualElement prompt = HudLayer.Root.Q(className: "field-prompt");
                        Assert.AreEqual(DisplayStyle.Flex, prompt.resolvedStyle.display, "target is in view");
                        Assert.AreEqual("[E] Shop for gear", hud.Prompt);
                        AssertSingleLine(prompt.Q<Label>(className: "hud-prompt__text"));
                        Assert.Greater(prompt.layout.width, 150f, "prompt owns its children's width");
                        Assert.Greater(prompt.layout.height, 18f, "prompt owns its children's height");
                        AssertInside(prompt, HudLayer.Root);
                        AssertSeparate("hq-brief", "field-recording");
                        AssertSeparate("hq-brief", "field-compass");
                        AssertSeparate("field-note", "field-pockets");
                    }, "prompt");
                hud.Override = "[Space] Inspect the field guide / monster manual and review the recovery instructions";
                yield return AtSize(menu, Sizes[1], () =>
                {
                    VisualElement prompt = HudLayer.Root.Q(className: "field-prompt");
                    Label text = prompt.Q<Label>(className: "hud-prompt__text");
                    Assert.Greater(text.contentRect.width, 140f, "long prompts wrap in a readable column");
                    Assert.Greater(text.contentRect.height, 20f, "long prompts may use multiple lines");
                    AssertSingleLine(prompt.Q<Label>(className: "keycap"));
                    AssertInside(text, prompt);
                }, "long_prompt");
            }
            finally
            {
                hud.Override = null;
                Object.Destroy(terminal);
            }
        }

        [UnityTest]
        public IEnumerator MenusAndStationsKeepReadableTextColumns()
        {
            yield return TestMapScene.Load("HQ");
            MenuUi menu = MenuUi.Current;
            menu.OpenPause();
            yield return null;
            foreach (MenuScreen screen in new[] { MenuScreen.Settings, MenuScreen.Controls, MenuScreen.HowToPlay,
                         MenuScreen.Achievements, MenuScreen.Credits, MenuScreen.Play, MenuScreen.Wardrobe })
            {
                if (screen == MenuScreen.Wardrobe) menu.OpenWardrobe();
                else menu.Push(screen);
                yield return null;
                foreach (var size in new[] { Sizes[0], Sizes[1] })
                    yield return AtSize(menu, size, () => AssertReadable(menu.Surface), screen.ToString());
                menu.Back();
                yield return null;
            }
            menu.Resume();
            yield return null;
            foreach (string station in new[] { "board", "shop", "rack" })
            {
                Abandoned.Company.ContractBoard.Open = station == "board";
                Abandoned.Equipment.ShopTerminal.Open = station == "shop";
                Abandoned.Equipment.GearRack.Open = station == "rack";
                yield return null;
                foreach (var size in new[] { Sizes[0], Sizes[1] })
                    yield return AtSize(menu, size, () => AssertReadable(menu.Surface), station);
            }
            Abandoned.Company.ContractBoard.Open = false;
            Abandoned.Equipment.ShopTerminal.Open = false;
            Abandoned.Equipment.GearRack.Open = false;
        }

        private static void AssertReadable(VisualElement root)
        {
            foreach (TextElement text in root.Query<TextElement>().ToList())
            {
                if (string.IsNullOrWhiteSpace(text.text) || text.text.Length < 8 ||
                    text.resolvedStyle.whiteSpace != WhiteSpace.Normal) continue;
                if (!HudLayer.TryLocalRect(text, root, out _)) continue;
                Vector2 natural = text.MeasureTextSize(text.text, 0, VisualElement.MeasureMode.Undefined,
                    0, VisualElement.MeasureMode.Undefined);
                Assert.GreaterOrEqual(text.contentRect.width + 1f, Mathf.Min(48f, natural.x),
                    $"{string.Join(" ", text.GetClasses())}: '{text.text}' collapsed into a narrow column");
            }
        }

        private static void AssertSeparate(string first, string second)
        {
            VisualElement root = HudLayer.Root;
            if (HudLayer.TryLocalRect(root.Q(className: first), root, out Rect a) &&
                HudLayer.TryLocalRect(root.Q(className: second), root, out Rect b))
                Assert.IsFalse(a.Overlaps(b), $"{first} and {second} must not overlap");
        }

        private static void AssertInside(VisualElement child, VisualElement parent)
        {
            Assert.GreaterOrEqual(child.worldBound.xMin, parent.worldBound.xMin - 1f);
            Assert.LessOrEqual(child.worldBound.xMax, parent.worldBound.xMax + 1f);
            Assert.GreaterOrEqual(child.worldBound.yMin, parent.worldBound.yMin - 1f);
            Assert.LessOrEqual(child.worldBound.yMax, parent.worldBound.yMax + 1f);
        }

        private static void AssertSingleLine(Label label)
        {
            Vector2 natural = label.MeasureTextSize(label.text, 0, VisualElement.MeasureMode.Undefined,
                0, VisualElement.MeasureMode.Undefined);
            Assert.GreaterOrEqual(label.contentRect.width + 1f, natural.x, $"{label.text}: enough width for the whole phrase");
            Assert.LessOrEqual(label.contentRect.height, natural.y + 2f, $"{label.text}: a single horizontal line");
        }

        private static IEnumerator AtSize(MenuUi menu, (int width, int height, float scale) size,
            System.Action check, string screenshot)
        {
            PanelSettings panel = menu.GetComponent<UIDocument>().panelSettings;
            float oldScale = GameSettings.UiScale;
            RenderTexture previous = panel.targetTexture;
            var target = new RenderTexture(size.width, size.height, 24);
            try
            {
                GameSettings.UiScale = size.scale;
                panel.targetTexture = target;
                for (int frame = 0; frame < 12; frame++) yield return null;
                yield return new WaitForSeconds(.6f); // Let menu transitions settle before visual QA.
                // Save the rendered result even when an assertion catches a regression.
                yield return MenuTests.Capture(menu, $"UI_text_{screenshot}_{size.width}_{size.height}_{size.scale}", size.width, size.height);
                panel.targetTexture = target;
                for (int frame = 0; frame < 6; frame++) yield return null;
                check();
            }
            finally
            {
                panel.targetTexture = previous;
                GameSettings.UiScale = oldScale;
                target.Release();
                Object.Destroy(target);
            }
        }
    }
}
