using System;
using Abandoned.Player;
using UnityEngine;

namespace Abandoned.Onboarding
{
    /// <summary>
    /// The HR office scene (Scenes/HrOffice, built by Editor/Onboarding/HrOfficeBuilder): its two scenes, the director
    /// that plays them and the cosmetic catalog the character creator dresses you from. Offline only: on arrival it
    /// hands itself to <see cref="OnboardingFlow"/>, which decides what happens here.
    /// </summary>
    public class HrOffice : MonoBehaviour
    {
        public const string Scene = "HrOffice";

        [SerializeField] private CutsceneDirector director;
        [SerializeField] private DialogueDefinition interview;
        [SerializeField] private DialogueDefinition hired;
        [SerializeField] private CosmeticCatalog catalog;

        public static HrOffice Current { get; private set; }
        public CosmeticCatalog Catalog => catalog;

        private void OnEnable() => Current = this;

        private void OnDisable()
        {
            if (Current == this) Current = null;
        }

        private void Start() => OnboardingFlow.EnteredOffice(this);

        public void PlayInterview(Action done) => director.Play(interview, done);

        public void PlayHired(Action done) => director.Play(hired, done);

#if UNITY_EDITOR
        public void EditorSetup(CutsceneDirector cutscenes, DialogueDefinition first, DialogueDefinition second, CosmeticCatalog cosmetics)
        {
            director = cutscenes;
            interview = first;
            hired = second;
            catalog = cosmetics;
        }
#endif
    }
}
