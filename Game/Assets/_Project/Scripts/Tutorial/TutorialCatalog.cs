using System.Collections.Generic;
using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Tutorial
{
    /// <summary>
    /// The training job's lessons, in the order they are taught. Lives at Data/Tutorial/Resources/TutorialCatalog;
    /// reorder, add or remove lessons here (the step number is the network's, so every machine must agree, and
    /// they do: the catalog ships with the build).
    /// </summary>
    [CreateAssetMenu(menuName = "Abandoned/Tutorial/Tutorial Catalog", fileName = "TutorialCatalog")]
    public class TutorialCatalog : ScriptableObject, IValidatable
    {
        public const string ResourcePath = "TutorialCatalog";

        [SerializeField] private List<TutorialStepDefinition> steps = new();

        public IReadOnlyList<TutorialStepDefinition> Steps => steps;
        public int Count => steps.Count;

        public TutorialStepDefinition At(int index) => index >= 0 && index < steps.Count ? steps[index] : null;

        public int IndexOf(string id)
        {
            for (int i = 0; i < steps.Count; i++) if (steps[i] != null && steps[i].Id == id) return i;
            return -1;
        }

        /// <summary>The first lesson of the chapter that <paramref name="index"/> belongs to.</summary>
        public int ChapterStart(int index)
        {
            TutorialStepDefinition step = At(index);
            if (step == null) return Mathf.Clamp(index, 0, Mathf.Max(0, steps.Count - 1));
            int start = index;
            while (start > 0 && At(start - 1) is TutorialStepDefinition before && before.Chapter == step.Chapter) start--;
            return start;
        }

        /// <summary>How many chapters there are (the highest chapter number + 1).</summary>
        public int Chapters
        {
            get
            {
                int highest = -1;
                foreach (TutorialStepDefinition s in steps) if (s != null) highest = Mathf.Max(highest, s.Chapter);
                return highest + 1;
            }
        }

        public void Validate(List<string> errors)
        {
            var ids = new HashSet<string>();
            int chapter = 0;
            for (int i = 0; i < steps.Count; i++)
            {
                TutorialStepDefinition s = steps[i];
                if (s == null) { errors.Add($"Tutorial catalog: slot {i} is empty."); continue; }
                if (!ids.Add(s.Id)) errors.Add($"Tutorial catalog: '{s.Id}' is in it twice.");
                if (s.Chapter < chapter) errors.Add($"Tutorial catalog: '{s.Id}' goes back to chapter {s.Chapter} after chapter {chapter}; chapters must run in order.");
                chapter = Mathf.Max(chapter, s.Chapter);
                s.Validate(errors);
            }
        }

        private static TutorialCatalog current;

        public static TutorialCatalog Current
        {
            get
            {
                if (current != null) return current;
                current = Resources.Load<TutorialCatalog>(ResourcePath);
                if (current == null) current = CreateInstance<TutorialCatalog>();
                return current;
            }
        }

#if UNITY_EDITOR
        public void EditorSet(List<TutorialStepDefinition> list) => steps = list;
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => current = null;
    }
}
