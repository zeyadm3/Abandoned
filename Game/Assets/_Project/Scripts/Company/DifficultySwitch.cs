using Abandoned.Core;
using Abandoned.UI;
using Unity.Netcode;
using UnityEngine;

namespace Abandoned.Company
{
    /// <summary>The HQ's difficulty switch (0.19.4): everyone can read it; only the host turns it (Relaxed, Standard, Brutal).</summary>
    public class DifficultySwitch : MonoBehaviour, IUsable, IHintSource
    {
        [SerializeField] private Transform knob;

        public HintId Hint => HintId.Difficulty;

        private void Update()
        {
            CompanyService company = CompanyService.Current;
            if (knob != null && company != null) knob.localRotation = Quaternion.Euler(0f, 0f, -60f + 60f * (int)company.Difficulty);
        }

        public string UsePrompt(GameObject user)
        {
            CompanyService company = CompanyService.Current;
            if (company == null || !company.IsSpawned) return null;
            bool host = NetworkManager.Singleton != null && NetworkManager.Singleton.IsHost;
            return host ? $"Difficulty: {company.Difficulty} (turn the switch)" : $"Difficulty: {company.Difficulty} (the host sets it)";
        }

        public void Use(GameObject user)
        {
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsHost) CompanyService.Current?.RequestCycleDifficulty();
        }

#if UNITY_EDITOR
        public void EditorSetup(Transform dial) => knob = dial;
#endif
    }
}
