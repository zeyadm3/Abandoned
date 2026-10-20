using System;

namespace Abandoned.Tutorial
{
    /// <summary>
    /// A lesson's Guide line read into what to do and where: "take item:lamp", "use crate:hand_trolley",
    /// "load truck", "away section:hallway", "go area:porch", "none". An empty line lets the guide work it out
    /// from the lesson's check (<see cref="TutorialGuides"/>).
    /// </summary>
    public readonly struct TutorialGuideSpec
    {
        public readonly TutorialGuideKind Kind;
        public readonly TutorialGuideTarget Target;
        public readonly string Key;

        public TutorialGuideSpec(TutorialGuideKind kind, TutorialGuideTarget target = TutorialGuideTarget.None, string key = "")
        {
            Kind = kind;
            Target = target;
            Key = key ?? "";
        }

        public static readonly TutorialGuideSpec Nothing = new(TutorialGuideKind.None);

        /// <summary>Stable within a lesson: a change of target starts its bars again.</summary>
        public string Id => $"{Kind}/{Target}/{Key}";

        /// <summary>Reads a Guide line; false (with <paramref name="error"/>) when it can't be read.</summary>
        public static bool TryParse(string line, out TutorialGuideSpec spec, out string error)
        {
            spec = Nothing;
            error = null;
            string text = (line ?? "").Trim();
            if (text.Length == 0) return true;
            string[] words = text.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
            if (!Enum.TryParse(words[0], true, out TutorialGuideKind kind))
            {
                error = $"'{words[0]}' is not a guide action (go, take, use, load, hide, away, brace, look, wait, none)";
                return false;
            }
            if (kind is TutorialGuideKind.None or TutorialGuideKind.Wait)
            {
                spec = new TutorialGuideSpec(kind);
                return true;
            }
            if (words.Length < 2)
            {
                error = $"'{text}' needs a target (\"{words[0]} area:porch\", \"{words[0]} truck\")";
                return false;
            }
            string target = words[1];
            int colon = target.IndexOf(':');
            string type = colon >= 0 ? target.Substring(0, colon) : target;
            string key = colon >= 0 ? target.Substring(colon + 1) : "";
            if (!Enum.TryParse(type, true, out TutorialGuideTarget what) || what == TutorialGuideTarget.None)
            {
                error = $"'{type}' is not a guide target (area, spot, item, truck, lever, crate, door, breaker, closet, section)";
                return false;
            }
            bool keyed = what is not (TutorialGuideTarget.Truck or TutorialGuideTarget.Lever or TutorialGuideTarget.Breaker);
            if (keyed && key.Length == 0)
            {
                error = $"'{target}' needs a key after the colon";
                return false;
            }
            spec = new TutorialGuideSpec(kind, what, key);
            return true;
        }
    }
}
