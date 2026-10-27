using System.Collections.Generic;
using UnityEngine;

namespace Abandoned.Onboarding
{
    /// <summary>A scene of the hiring (the interview, "you're hired"): who speaks and the lines in order. Data: a new
    /// line, reply or camera cut needs no code (CutsceneDirector plays it).</summary>
    [CreateAssetMenu(menuName = "Abandoned/Onboarding/Dialogue", fileName = "Dialogue")]
    public class DialogueDefinition : ScriptableObject
    {
        [field: SerializeField] public string Id { get; private set; }
        [field: SerializeField] public string Speaker { get; private set; } = "Dana Whitfield";
        [field: SerializeField] public string SpeakerRole { get; private set; } = "Human Resources";
        [SerializeField] private List<DialogueLine> lines = new();

        public IReadOnlyList<DialogueLine> Lines => lines;

#if UNITY_EDITOR
        public void EditorSetup(string id, string speaker, string role, List<DialogueLine> scene)
        {
            Id = id;
            Speaker = speaker;
            SpeakerRole = role;
            lines = scene;
        }
#endif
    }
}
