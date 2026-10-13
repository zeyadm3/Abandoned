using Abandoned.Audio;
using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Company
{
    /// <summary>The HQ workbench telephone shares the host's contract board and job briefings.</summary>
    public class DispatchTelephone : ContractBoard
    {
        public static DispatchTelephone Current { get; private set; }

        private void OnEnable() => Current = this;

        public override string UsePrompt(GameObject user) => CompanyService.Current == null ? null
            : CompanyService.Current.DemoOver ? Loc.T("The demo is over - thanks for playing!")
            : Loc.T("Call dispatch: contracts and briefing");

        public override void Use(GameObject user)
        {
            if (CompanyService.Current == null) return;
            if (CompanyService.Current.DemoOver)
            {
                UI.MenuUi.Current?.OpenDemoEnd();
                return;
            }
            ContractBoard.FromTelephone = true;
            ContractBoard.Open = true;
            PlayLine();
        }

        public static void PlayLine()
        {
            Vector3 at = Current != null ? Current.transform.position : Vector3.zero;
            GameAudio.Play(SoundId.VoicemailBeep, at, 0.7f);
            if (!DispatchBriefingAudio.Play(CompanyService.Current, at))
                GameAudio.Play(SoundId.RadioStatic, at, 0.35f);
        }

        private void OnDisable()
        {
            if (Current != this) return;
            Current = null;
            if (ContractBoard.FromTelephone)
            {
                ContractBoard.Open = false;
                DispatchBriefingAudio.Stop();
            }
        }

        private void OnDrawGizmos()
        {
            if (Application.isPlaying && !DebugView.Visible) return;
            Gizmos.color = new Color(0.75f, 0.65f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, 0.3f);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Current = null;
    }
}
