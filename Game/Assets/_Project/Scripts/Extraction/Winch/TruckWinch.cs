using Abandoned.Company;
using Abandoned.Core;
using Abandoned.Interaction;
using Abandoned.Loot;
using Abandoned.UI;
using Unity.Netcode;
using UnityEngine;

namespace Abandoned.Extraction
{
    /// <summary>
    /// The truck's winch (0.20.3, a truck upgrade): E at the drum hooks the nearest Heavy or Huge item lying
    /// within reach of the ramp, and the host reels it across the court and up into the bay. The cable shows
    /// on every machine; anyone grabbing the item, or it sticking, unhooks it. Scene object on the truck;
    /// the hooked item is on <see cref="RunWinch"/>.
    /// </summary>
    public class TruckWinch : MonoBehaviour, IUsable, IHintSource
    {
        [SerializeField] private Transform drum;
        [SerializeField] private Transform rampFoot;
        [SerializeField] private Transform bay;
        [SerializeField] private LineRenderer cable;

        public static TruckWinch Active { get; private set; }
        public Vector3 Drum => drum != null ? drum.position : transform.position;
        public Vector3 RampFoot => rampFoot != null ? rampFoot.position : transform.position;
        public Vector3 Bay => bay != null ? bay.position : transform.position;
        public HintId Hint => HintId.Winch;

        private void OnEnable() => Active = this;
        private void OnDisable()
        {
            if (Active == this) Active = null;
        }

        private static bool Owned => CompanyService.Current != null && CompanyService.Current.IsSpawned && CompanyService.Current.HasUpgrade(TruckUpgradeKind.Winch);

        private void LateUpdate()
        {
            Transform target = RunWinch.Current != null ? RunWinch.Current.Target : null;
            if (cable == null) return;
            cable.enabled = target != null;
            if (target == null) return;
            cable.SetPosition(0, Drum);
            cable.SetPosition(1, target.position);
        }

        public string UsePrompt(GameObject user)
        {
            if (!Owned || RunWinch.Current == null || !RunWinch.Current.IsSpawned) return null;
            return RunWinch.Current.Target != null ? Loc.T("Unhook the winch") : Loc.T("Run the winch out to the nearest heavy item");
        }

        public void Use(GameObject user) => RunWinch.Current?.RequestToggle();

#if UNITY_EDITOR
        public void EditorSetup(Transform reel, Transform foot, Transform inside, LineRenderer line)
        {
            drum = reel;
            rampFoot = foot;
            bay = inside;
            cable = line;
        }
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Active = null;
    }
}
