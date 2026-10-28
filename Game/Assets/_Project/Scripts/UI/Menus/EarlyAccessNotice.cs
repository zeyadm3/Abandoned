using System;
using Abandoned.Audio;
using Abandoned.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace Abandoned.UI
{
    /// <summary>
    /// The box over the title screen (0.13.4b.EEA, user 2026-10-08): very early access, gameplay a work in
    /// progress, and where to send feedback and feature wishes. Closed with Continue; every build shows it once
    /// per version (2026-10-28 UI redo: it used to block the title on every launch of a friends' build). A public
    /// build's contact comes from <see cref="Launch"/> (never a private address).
    /// </summary>
    public class EarlyAccessNotice
    {
        public const string Tag = "VERY EARLY ACCESS - GAMEPLAY WORK IN PROGRESS"; // @loc
        private const string SeenKey = "launchNotice.version";

        private static bool dismissed;
        private readonly VisualElement overlay;
        private readonly Label copied;
        private readonly Action continueToMenu;
        private readonly string email;

        public bool Visible => !dismissed;

        /// <summary>The notice is still up (the hiring waits for it).</summary>
        public static bool Open => !dismissed;

        public EarlyAccessNotice(VisualElement parent, Font titleFont, Action continueToMenu = null)
        {
            this.continueToMenu = continueToMenu;
            // A paying player has read it once; it returns when a new version changes what to expect.
            if (Prefs.GetString(SeenKey, "") == Application.version) dismissed = true;
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
            Label title = MenuKit.Text(panel, "ABANDONED", "title");
            title.AddToClassList("launch-notice__title");
            if (titleFont != null) title.style.unityFontDefinition = FontDefinition.FromFont(titleFont);
            MenuKit.Text(panel, Loc.T("TAKE WHAT'S LEFT. LEAVE WHILE YOU CAN."), "launch-notice__warning");
            MenuKit.Text(panel, Loc.T(Tag), "subtitle");
            ScrollView body = MenuKit.Scroll(panel, "launch-notice__body");
            MenuKit.Text(body, Loc.T("You're playing a very early build of ABANDONED. The gameplay is a work in progress: " +
                                "expect bugs, rough edges and things that change between versions."));
            email = Launch.FeedbackEmail;
            bool page = email.Length == 0 && Launch.HasFeedback;
            if (email.Length > 0)
            {
                MenuKit.Text(body, Loc.T("Send feedback, feature wishes and problem reports to:"));
                MenuKit.Text(body, email, "launch-notice__email");
            }
            else if (page || Launch.HasDiscord)
            {
                MenuKit.Text(body, Loc.T("Send feedback, feature wishes and problem reports with the buttons below."));
            }
            copied = MenuKit.Text(body, "", "text");
            copied.AddToClassList("text--small");

            VisualElement buttons = MenuKit.Row(panel);
            buttons.AddToClassList("launch-notice__actions");
            buttons.style.flexWrap = Wrap.Wrap;
            if (email.Length > 0)
            {
                MenuKit.Button(buttons, Loc.T("Email feedback"), OpenEmail, SoundId.UiConfirm, small: true);
                MenuKit.Button(buttons, Loc.T("Copy email"), CopyEmail, small: true);
            }
            else if (page)
            {
                bool steam = Launch.FeedbackUrl.Contains("steamcommunity.com");
                MenuKit.Button(buttons, steam ? Loc.T("Steam discussions") : Loc.T("Send feedback"), Launch.OpenFeedback, SoundId.UiConfirm, small: true);
            }
            if (Launch.HasDiscord) MenuKit.Button(buttons, "Discord", Launch.OpenDiscord, small: true);
            MenuKit.Button(buttons, Loc.T("Continue"), Dismiss, SoundId.UiConfirm, small: true).AddToClassList("launch-notice__continue");
            MenuKit.Show(overlay, !dismissed);
        }

        private void OpenEmail()
        {
            if (Application.isBatchMode) return;
            Application.OpenURL($"mailto:{email}?subject=ABANDONED%20{Application.version}%20feedback");
        }

        private void CopyEmail()
        {
            GUIUtility.systemCopyBuffer = email;
            copied.text = Loc.T("Copied.");
        }

        private void Dismiss()
        {
            dismissed = true;
            Prefs.SetString(SeenKey, Application.version);
            Prefs.Save();
            MenuKit.Show(overlay, false);
            continueToMenu?.Invoke();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => dismissed = false;
    }
}
