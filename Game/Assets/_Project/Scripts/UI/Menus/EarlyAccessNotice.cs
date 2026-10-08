using Abandoned.Audio;
using UnityEngine;
using UnityEngine.UIElements;

namespace Abandoned.UI
{
    /// <summary>
    /// The box over the title screen on every launch of the friends-and-family build (0.13.4b.EEA, user
    /// 2026-10-08): very early access, gameplay a work in progress, and where to send feedback and feature
    /// wishes. Closed with OK; comes back next launch.
    /// </summary>
    public class EarlyAccessNotice
    {
        public const string FeedbackEmail = "zeyad.m.a.2005@gmail.com";
        public const string Tag = "VERY EARLY ACCESS - GAMEPLAY WORK IN PROGRESS";

        private static bool dismissed;
        private readonly VisualElement overlay;
        private readonly Label copied;

        public EarlyAccessNotice(VisualElement parent, Font titleFont)
        {
            overlay = new VisualElement();
            overlay.AddToClassList("backdrop");
            overlay.AddToClassList("backdrop--dim");
            overlay.AddToClassList("backdrop--center");
            overlay.style.position = Position.Absolute;
            overlay.style.left = overlay.style.right = overlay.style.top = overlay.style.bottom = 0;
            parent.Add(overlay);

            VisualElement panel = MenuKit.Panel(overlay, wide: true);
            panel.style.maxWidth = 760;
            Label title = MenuKit.Text(panel, "EXCLUSIVE EARLY ACCESS", "title");
            if (titleFont != null) title.style.unityFontDefinition = FontDefinition.FromFont(titleFont);
            MenuKit.Text(panel, Tag, "subtitle");
            UiKit.Hazard(panel);
            MenuKit.Text(panel, "You're playing a very early build of ABANDONED. The gameplay is a work in progress: " +
                                "expect bugs, rough edges and things that change between versions.");
            MenuKit.Text(panel, "If you have any feedback, or options and gameplay features you want implemented, email:");
            MenuKit.Text(panel, FeedbackEmail, "heading");
            copied = MenuKit.Text(panel, "", "text");
            copied.AddToClassList("text--small");

            VisualElement buttons = MenuKit.Row(panel);
            buttons.style.flexWrap = Wrap.Wrap;
            MenuKit.Button(buttons, "Email feedback", OpenEmail, SoundId.UiConfirm, small: true);
            MenuKit.Button(buttons, "Copy email", CopyEmail, small: true);
            MenuKit.Button(buttons, "OK", Dismiss, SoundId.UiConfirm, small: true);
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
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => dismissed = false;
    }
}
