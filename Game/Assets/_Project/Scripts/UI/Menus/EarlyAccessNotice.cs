using System;
using Abandoned.Audio;
using UnityEngine;
using UnityEngine.UIElements;

namespace Abandoned.UI
{
    /// <summary>
    /// The box over the title screen on every launch of the friends-and-family build (0.13.4b.EEA, user
    /// 2026-10-08): very early access, gameplay a work in progress, and where to send feedback and feature
    /// wishes. Closed with Continue; comes back next launch.
    /// </summary>
    public class EarlyAccessNotice
    {
        public const string FeedbackEmail = "zeyad.m.a.2005@gmail.com";
        public const string Tag = "VERY EARLY ACCESS - GAMEPLAY WORK IN PROGRESS";

        private static bool dismissed;
        private readonly VisualElement overlay;
        private readonly Label copied;
        private readonly Action continueToMenu;

        public bool Visible => !dismissed;

        public EarlyAccessNotice(VisualElement parent, Font titleFont, Action continueToMenu = null)
        {
            this.continueToMenu = continueToMenu;
            overlay = new VisualElement();
            overlay.AddToClassList("backdrop");
            overlay.AddToClassList("backdrop--dim");
            overlay.AddToClassList("backdrop--center");
            overlay.AddToClassList("launch-notice");
            overlay.style.position = Position.Absolute;
            overlay.style.left = overlay.style.right = overlay.style.top = overlay.style.bottom = 0;
            parent.Add(overlay);

            VisualElement panel = MenuKit.Panel(overlay, wide: true);
            panel.AddToClassList("launch-notice__panel");
            panel.style.width = 880f;
            panel.style.maxWidth = Length.Percent(90f);
            panel.style.height = StyleKeyword.Auto;
            panel.style.maxHeight = Length.Percent(90f);
            MenuKit.Text(panel, "BEFORE YOU ENTER / EARLY ACCESS RECORD", "menu-record");
            Label title = MenuKit.Text(panel, "ABANDONED", "title");
            title.AddToClassList("launch-notice__title");
            if (titleFont != null) title.style.unityFontDefinition = FontDefinition.FromFont(titleFont);
            MenuKit.Text(panel, "TAKE WHAT'S LEFT. LEAVE WHILE YOU CAN.", "launch-notice__warning");
            MenuKit.Text(panel, Tag, "subtitle");
            ScrollView body = MenuKit.Scroll(panel, "launch-notice__body");
            MenuKit.Text(body, "You're playing a very early build of ABANDONED. The gameplay is a work in progress: " +
                                "expect bugs, rough edges and things that change between versions.");
            MenuKit.Text(body, "Send feedback, feature wishes and problem reports to:");
            MenuKit.Text(body, FeedbackEmail, "launch-notice__email");
            copied = MenuKit.Text(body, "", "text");
            copied.AddToClassList("text--small");

            VisualElement buttons = MenuKit.Row(panel);
            buttons.AddToClassList("launch-notice__actions");
            buttons.style.flexWrap = Wrap.Wrap;
            MenuKit.Button(buttons, "Email feedback", OpenEmail, SoundId.UiConfirm, small: true);
            MenuKit.Button(buttons, "Copy email", CopyEmail, small: true);
            MenuKit.Button(buttons, "Continue", Dismiss, SoundId.UiConfirm, small: true).AddToClassList("launch-notice__continue");
            MenuKit.Show(overlay, !dismissed);
        }

        private static void OpenEmail()
        {
            if (Application.isBatchMode) return;
            Application.OpenURL($"mailto:{FeedbackEmail}?subject=ABANDONED%20{Application.version}%20feedback");
        }

        private void CopyEmail()
        {
            GUIUtility.systemCopyBuffer = FeedbackEmail;
            copied.text = "Copied.";
        }

        private void Dismiss()
        {
            dismissed = true;
            MenuKit.Show(overlay, false);
            continueToMenu?.Invoke();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => dismissed = false;
    }
}
