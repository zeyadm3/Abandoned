using System;
using Abandoned.Audio;
using Abandoned.Core;
using Abandoned.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Abandoned.Onboarding
{
    /// <summary>
    /// The hiring scene's subtitles on the menus' document: letterbox bars, the speaker's name and role, the line,
    /// the player's replies as buttons, and how to move on or skip (with the hold-to-skip filling up).
    /// </summary>
    public sealed class DialogueView
    {
        private readonly VisualElement root, replies, skipFill;
        private readonly Label speaker, role, line, hint;

        public DialogueView()
        {
            root = new VisualElement { pickingMode = PickingMode.Ignore };
            root.AddToClassList("dialogue");
            UiKit.FillScreen(root);
            foreach (bool top in new[] { true, false })
            {
                var bar = new VisualElement { pickingMode = PickingMode.Ignore };
                bar.AddToClassList("dialogue__bar");
                bar.style.position = Position.Absolute;
                bar.style.left = 0;
                bar.style.right = 0;
                bar.style.height = Length.Percent(9f);
                if (top) bar.style.top = 0;
                else bar.style.bottom = 0;
                bar.style.backgroundColor = Color.black;
                root.Add(bar);
            }
            var box = new VisualElement { pickingMode = PickingMode.Ignore };
            box.AddToClassList("dialogue__box");
            box.style.position = Position.Absolute;
            box.style.left = Length.Percent(18f);
            box.style.right = Length.Percent(18f);
            box.style.bottom = Length.Percent(11f);
            box.style.paddingLeft = box.style.paddingRight = 22f;
            box.style.paddingTop = box.style.paddingBottom = 14f;
            box.style.backgroundColor = new Color(.04f, .04f, .045f, .78f);
            root.Add(box);
            VisualElement head = MenuKit.Row(box);
            speaker = MenuKit.Text(head, "", "dialogue__speaker");
            speaker.style.unityFontStyleAndWeight = FontStyle.Bold;
            speaker.style.color = new Color(.91f, .72f, .38f);
            role = MenuKit.Text(head, "", "dialogue__role");
            role.style.marginLeft = 10f;
            role.style.opacity = .6f;
            line = MenuKit.Text(box, "", "dialogue__line");
            line.style.fontSize = 24f;
            line.style.whiteSpace = WhiteSpace.Normal;
            line.style.marginTop = 6f;
            replies = new VisualElement();
            replies.AddToClassList("dialogue__replies");
            replies.style.marginTop = 10f;
            box.Add(replies);
            hint = MenuKit.Text(box, "", "dialogue__hint");
            hint.style.opacity = .55f;
            hint.style.fontSize = 13f;
            hint.style.marginTop = 8f;
            var track = new VisualElement { pickingMode = PickingMode.Ignore };
            track.style.height = 2f;
            track.style.marginTop = 4f;
            track.style.backgroundColor = new Color(1f, 1f, 1f, .12f);
            box.Add(track);
            skipFill = new VisualElement { pickingMode = PickingMode.Ignore };
            skipFill.style.height = Length.Percent(100f);
            skipFill.style.width = Length.Percent(0f);
            skipFill.style.backgroundColor = new Color(.91f, .72f, .38f);
            track.Add(skipFill);
            Show(false);
        }

        public void Show(bool visible)
        {
            VisualElement surface = MenuUi.Current != null ? MenuUi.Current.Surface : Fallback();
            if (visible && surface != null && root.parent != surface) surface.Add(root);
            MenuKit.Show(root, visible);
            hint.text = Loc.T("Click to continue   -   Hold SPACE to skip");
        }

        // The office played on its own has no menus: a document of its own, on the menus' panel settings.
        private static UIDocument fallback;

        private static VisualElement Fallback()
        {
            if (fallback == null)
            {
                PanelSettings settings = Resources.Load<PanelSettings>("HrOfficePanel");
                if (settings == null) return null;
                fallback = new GameObject("HrOfficeSubtitles").AddComponent<UIDocument>();
                fallback.panelSettings = settings;
                UiThemeAssets.Apply(fallback.rootVisualElement);
            }
            return fallback.rootVisualElement;
        }

        public void Say(string who, string what, string text)
        {
            speaker.text = who;
            role.text = what;
            line.text = text;
        }

        public void Replies(string[] options, Action<int> picked)
        {
            replies.Clear();
            replies.pickingMode = options == null ? PickingMode.Ignore : PickingMode.Position;
            if (options == null) return;
            for (int i = 0; i < options.Length; i++)
            {
                int index = i;
                Button b = MenuKit.Button(replies, $"{i + 1}.  {options[i]}", () => picked?.Invoke(index), SoundId.UiConfirm, small: true);
                b.AddToClassList("dialogue__reply");
                if (i == 0) b.schedule.Execute(() => b.Focus());
            }
        }

        private float shownSkip = -1f;

        // Called every frame while a scene plays: only touch the style (and so the layout) when the fill moves.
        public void SkipProgress(float amount)
        {
            amount = Mathf.Clamp01(amount);
            if (Mathf.Approximately(amount, shownSkip)) return;
            shownSkip = amount;
            skipFill.style.width = Length.Percent(amount * 100f);
        }
    }
}
