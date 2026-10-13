using Abandoned.Audio;
using Abandoned.Core;
using Abandoned.UI;
using UnityEngine;

namespace Abandoned.Extraction
{
    /// <summary>
    /// The jewellery store's safe (0.16.6), a scene object on every machine: shows the drill while it runs,
    /// swings the door open when <see cref="RunVault"/> says so, and plays the drill's whine locally.
    /// </summary>
    public class VaultSafe : MonoBehaviour, IUsable, IHintSource
    {
        private const float WhineInterval = 0.9f;

        [SerializeField] private Transform door;
        [SerializeField] private Collider doorSolid;
        [SerializeField] private GameObject drill;
        [SerializeField] private Transform inside;

        private float nextWhine, shownOpen;

        public static VaultSafe Active { get; private set; }
        public Vector3 Front => transform.position + transform.forward * 1.3f;
        public Vector3 Inside => inside != null ? inside.position : transform.position;
        public HintId Hint => HintId.Vault;

        private void OnEnable() => Active = this;
        private void OnDisable()
        {
            if (Active == this) Active = null;
        }

        private void Update()
        {
            RunVault run = RunVault.Current;
            VaultPhase phase = run != null && run.IsSpawned ? run.Phase : VaultPhase.Locked;
            if (drill != null) drill.SetActive(phase == VaultPhase.Drilling || phase == VaultPhase.Jammed);
            float open = Mathf.MoveTowards(shownOpen, phase == VaultPhase.Open ? 1f : 0f, Time.deltaTime * 0.6f);
            if (!Mathf.Approximately(open, shownOpen) || shownOpen == 0f && open == 0f)
            {
                shownOpen = open;
                if (door != null) door.localRotation = Quaternion.Euler(0f, -100f * Mathf.SmoothStep(0f, 1f, open), 0f);
                if (doorSolid != null) doorSolid.enabled = open < 0.5f;
            }
            if (phase == VaultPhase.Drilling && Time.time >= nextWhine)
            {
                nextWhine = Time.time + WhineInterval;
                GameAudio.Play(SoundId.VaultDrill, Front + Vector3.up, 1f);
            }
        }

        public string UsePrompt(GameObject user)
        {
            RunVault run = RunVault.Current;
            if (run == null || !run.IsSpawned) return null;
            return run.Phase switch
            {
                VaultPhase.Locked => Loc.T("Set the drill on the safe ({0:0} s, very loud)", VaultConfig.Current.DrillSeconds),
                VaultPhase.Drilling => Loc.T("Drilling... {0:0}%", run.Progress01 * 100f),
                VaultPhase.Jammed => Loc.T("The drill has jammed: free it ({0:0}%)", run.Progress01 * 100f),
                _ => null,
            };
        }

        public void Use(GameObject user)
        {
            RunVault run = RunVault.Current;
            if (run != null && (run.Phase == VaultPhase.Locked || run.Phase == VaultPhase.Jammed)) run.RequestUse();
        }

#if UNITY_EDITOR
        public void EditorSetup(Transform hinge, Collider shut, GameObject drillRig, Transform shelf)
        {
            door = hinge;
            doorSolid = shut;
            drill = drillRig;
            inside = shelf;
        }
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Active = null;
    }
}
