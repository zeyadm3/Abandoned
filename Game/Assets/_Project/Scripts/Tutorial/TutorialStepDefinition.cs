using System.Collections.Generic;
using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Tutorial
{
    /// <summary>
    /// One training lesson: what the player is told, what ends it, and what the house does when it begins. Adding
    /// a lesson is "create a definition, put it in the catalog" with no code, as long as an existing
    /// <see cref="TutorialCheck"/> and <see cref="TutorialBeat"/> cover it. Text is English here and worded where
    /// it is shown (Loc.T); "{key:Interact}" becomes the player's own binding.
    /// </summary>
    [CreateAssetMenu(menuName = "Abandoned/Tutorial/Tutorial Step", fileName = "Step_New")]
    public class TutorialStepDefinition : ScriptableObject, IValidatable
    {
        [field: Header("Identity")]
        [field: Tooltip("Unique, stable: progress and the F1 view name lessons by it.")]
        [field: SerializeField] public string Id { get; private set; } = "new.step";
        [field: Tooltip("Where in the house this lesson belongs (0 = the driveway). Resetting a chapter puts the crew at its \"chapter_N\" spot.")]
        [field: SerializeField, Min(0)] public int Chapter { get; private set; }
        [field: SerializeField] public string ChapterName { get; private set; } = "THE DRIVEWAY";

        [field: Header("What the player is told")]
        [field: SerializeField] public string DisplayName { get; private set; } = "New lesson";
        [field: SerializeField, TextArea(2, 5)] public string Description { get; private set; } = "";
        [field: Tooltip("Said instead when two or more players are on the job. Empty = the same as Description.")]
        [field: SerializeField, TextArea(2, 5)] public string CrewDescription { get; private set; } = "";
        [field: Tooltip("The dispatcher on the radio when the lesson starts. Empty = silence. A recording named by a hash of this text plays if one exists (Resources/TutorialDispatch).")]
        [field: SerializeField, TextArea(1, 3)] public string RadioLine { get; private set; } = "";

        [field: Tooltip("The trainer's spoken instruction (English, never shown: the screen has Description). A recording named by a hash of this text plays if one exists (Resources/TutorialNarration, Tools/Audio/generate_tutorial_narration.py).")]
        [field: SerializeField, TextArea(2, 4)] public string VoiceLine { get; private set; } = "";

        [field: Header("Guidance")]
        [field: Tooltip("Where the arrows lead: \"take item:lamp\", \"go area:porch\", \"use crate:hand_trolley\", \"load truck\", \"use lever\", " +
                        "\"use door:0\", \"use breaker\", \"hide closet:0\", \"away section:hallway\", \"brace section:hallway\", \"look area:porch\", \"none\". " +
                        "Empty = worked out from the check (the vase, then the truck once someone holds it).")]
        [field: SerializeField] public string Guide { get; private set; } = "";
        [field: Tooltip("HUD instruments this lesson points out, comma separated: vitals, compass, haul, pockets, battery, cargo.")]
        [field: SerializeField] public string Highlight { get; private set; } = "";

        [field: Header("What ends it")]
        [field: SerializeField] public TutorialCheck Check { get; private set; } = TutorialCheck.Read;
        [field: Tooltip("The item, section or gear id the check is about.")]
        [field: SerializeField] public string Key { get; private set; } = "";
        [field: Tooltip("The area the check is about (EnterArea, ItemInArea).")]
        [field: SerializeField] public string Area { get; private set; } = "";
        [field: Tooltip("Seconds, metres, degrees, dollars, a stage or a door number, as the check reads it.")]
        [field: SerializeField] public float Amount { get; private set; }

        [field: Header("What the house does")]
        [field: SerializeField] public TutorialBeat Beat { get; private set; } = TutorialBeat.None;
        [field: SerializeField] public string BeatKey { get; private set; } = "";
        [field: SerializeField] public float BeatAmount { get; private set; }

        [field: Header("Rules")]
        [field: Tooltip("The host may skip it from the pause menu.")]
        [field: SerializeField] public bool Skippable { get; private set; } = true;

        /// <summary>The instruction for this crew, in the player's language with their key names.</summary>
        public string Text(int crew) => TextTable.Format(Loc.T(crew > 1 && !string.IsNullOrEmpty(CrewDescription) ? CrewDescription : Description));

        public string Title => Loc.T(DisplayName);
        public string ChapterTitle => Loc.T(ChapterName);

        public bool Personal => Check is TutorialCheck.Look or TutorialCheck.Walk or TutorialCheck.Sprint or TutorialCheck.Crouch
            or TutorialCheck.Jump or TutorialCheck.Scan or TutorialCheck.Pockets or TutorialCheck.Throw;

        public void Validate(List<string> errors)
        {
            string n = $"Tutorial step '{name}'";
            if (string.IsNullOrWhiteSpace(Id)) errors.Add($"{n}: Id is empty.");
            if (string.IsNullOrWhiteSpace(Description)) errors.Add($"{n}: Description is empty.");
            bool needsKey = Check is TutorialCheck.HoldItem or TutorialCheck.DropItem or TutorialCheck.MoveItem or TutorialCheck.ItemInArea
                or TutorialCheck.ItemInTruck or TutorialCheck.HasGear or TutorialCheck.SectionStage or TutorialCheck.OffSection
                or TutorialCheck.SectionBraced or TutorialCheck.SectionDown;
            if (needsKey && string.IsNullOrWhiteSpace(Key)) errors.Add($"{n}: {Check} needs a Key.");
            if ((Check is TutorialCheck.EnterArea or TutorialCheck.ItemInArea) && string.IsNullOrWhiteSpace(Area)) errors.Add($"{n}: {Check} needs an Area.");
            if ((Beat is TutorialBeat.SpawnThreat or TutorialBeat.ThreatLeave or TutorialBeat.ArmCollapse or TutorialBeat.Collapse) && string.IsNullOrWhiteSpace(BeatKey))
                errors.Add($"{n}: {Beat} needs a BeatKey.");
            if (!TutorialGuideSpec.TryParse(Guide, out _, out string guideError)) errors.Add($"{n}: Guide {guideError}.");
        }

#if UNITY_EDITOR
        /// <summary>Editor-only: the content builder authors the default lessons in code.</summary>
        public void EditorSetup(string id, int chapter, string chapterName, string title, string description, string crewDescription,
            string radio, TutorialCheck check, string key, string area, float amount, TutorialBeat beat, string beatKey, float beatAmount, bool skippable)
        {
            Id = id;
            Chapter = chapter;
            ChapterName = chapterName;
            DisplayName = title;
            Description = description;
            CrewDescription = crewDescription ?? "";
            RadioLine = radio ?? "";
            Check = check;
            Key = key ?? "";
            Area = area ?? "";
            Amount = amount;
            Beat = beat;
            BeatKey = beatKey ?? "";
            BeatAmount = beatAmount;
            Skippable = skippable;
        }

        /// <summary>Editor-only: the arrows' target, the HUD pieces pointed out and the trainer's spoken line.</summary>
        public void EditorSetGuidance(string guide, string highlight, string voiceLine)
        {
            Guide = guide ?? "";
            Highlight = highlight ?? "";
            VoiceLine = voiceLine ?? "";
        }
#endif
    }
}
