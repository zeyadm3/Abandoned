using Abandoned.Audio;
using Abandoned.Contracts;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Abandoned.Company
{
    /// <summary>Local telephone voice, chosen from the shared contract; changing terms stay on the job sheet.</summary>
    public static class DispatchBriefingAudio
    {
        private static SoundPlayback playback;

        public const string RoutineTranscript = "Abandoned Salvaging dispatch. Review the job sheet before departure. " +
            "Keep the crew together, listen for floor warnings, and load salvage into the truck. Bring everyone home.";
        public const string PowerOffTranscript = "Abandoned Salvaging dispatch. This job has no power. " +
            "Bring charged flashlights and keep the crew together. Review the job sheet, and listen for floor warnings.";
        public const string HighThreatTranscript = "Abandoned Salvaging dispatch. This is a high threat job. " +
            "Expect more monsters, sooner. Keep noise down, read the field guide, and bring the crew home together.";
        public const string SealedTranscript = "Abandoned Salvaging dispatch. Access is shuttered on this job. " +
            "Pack bolt cutters or a crowbar before departure. Review the job sheet, and leave room for salvage in the truck.";

        public static bool Play(CompanyService company, Vector3 position)
        {
            Stop();
            if (!TryChoose(company, out SoundId id)) return false;
            playback = GameAudio.PlayCancellableRecording(id, position);
            return playback.IsPlaying;
        }

        /// <summary>The exact recorded words, independent of volume, hearing distance or caption settings.</summary>
        public static string TranscriptFor(CompanyService company)
        {
            if (!TryChoose(company, out SoundId id)) return null;
            return id switch
            {
                SoundId.DispatchBriefingPowerOff => PowerOffTranscript,
                SoundId.DispatchBriefingHighThreat => HighThreatTranscript,
                SoundId.DispatchBriefingSealed => SealedTranscript,
                _ => RoutineTranscript,
            };
        }

        public static void Stop()
        {
            playback.Stop();
            playback = default;
        }

        private static bool TryChoose(CompanyService company, out SoundId id)
        {
            id = SoundId.DispatchBriefingRoutine;
            if (company == null || company.Selected < 0 || company.Selected >= company.Board.Count) return false;
            Contract contract = company.Board[company.Selected];
            if (!contract.IsValid) return false;
            // Missing a shutter tool can strand the trip; the complete briefing shows every other risk too.
            ContractModifier modifier = company.ModifierOf(contract);
            if (modifier != null && modifier.Sealed) id = SoundId.DispatchBriefingSealed;
            else if (contract.ThreatLevel >= 2) id = SoundId.DispatchBriefingHighThreat;
            else if (contract.PowerOff) id = SoundId.DispatchBriefingPowerOff;
            return true;
        }

        private static void OnSceneUnloaded(Scene scene) => Stop();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Stop();
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }
    }
}
