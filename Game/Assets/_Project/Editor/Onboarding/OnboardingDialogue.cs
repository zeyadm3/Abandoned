using System.Collections.Generic;
using Abandoned.Onboarding;
using UnityEditor;
using UnityEngine;

namespace Abandoned.EditorTools.Hiring
{
    /// <summary>
    /// The hiring's two scenes as data (Data/Onboarding/Dialogue_*.asset), written from here. Every line the manager
    /// speaks goes through Say(...), which is how Tools/Audio/generate_hr_interview.py finds the lines to record; the
    /// player's replies go through You(...). Shots: 0 seated first person, 1 the door, 2 over your shoulder, 3 the
    /// contract on the desk, 4 the handshake from the side, 5 the window. Mostly first person, cutting away at the beats.
    /// </summary>
    public static class OnboardingDialogue
    {
        public const string Folder = "Assets/_Project/Data/Onboarding";
        public const string InterviewPath = Folder + "/Dialogue_Interview.asset";
        public const string HiredPath = Folder + "/Dialogue_Hired.asset";
        private const string Speaker = "Dana Whitfield", Role = "Human Resources"; // @loc

        private static string Say(string text) => text;
        private static string You(string text) => text;

        private static DialogueLine L(string text, int shot = -1, float hold = .45f, string when = "", params (string you, string answer)[] replies)
        {
            var line = new DialogueLine { text = text, shot = shot, hold = hold, condition = when };
            foreach ((string you, string answer) in replies) line.replies.Add(new DialogueReply { text = you, answer = answer });
            return line;
        }

        public static List<DialogueLine> Interview() => new()
        {
            L(Say("Come in, come in. Sit anywhere that isn't the plant."), shot: 1, hold: .2f),
            L(Say("Dana Whitfield, Human Resources. Abandoned Salvaging. You saw the advert?"), shot: 0, replies: new[]
            {
                (You("Every word of it."), Say("Then you're the first.")),
                (You("I skimmed it."), Say("Honest. That'll help, out there.")),
                (You("There was an advert?"), Say("There was a sign on a fence. Same thing.")),
            }),
            L(Say("We go into buildings nobody else will. Malls, schools, a prison on a rock. We bring out what's worth money.")),
            L(Say("The floors are tired. They crack, they groan, and sometimes they go. Weight is everything: yours, and whatever you're carrying."), shot: 2),
            L(Say("And the buildings aren't always empty. Don't ask me what's in them."), shot: 0, replies: new[]
            {
                (You("What's in them?"), Say("I said don't ask me. The field guide on the dispatch bench knows more than I do.")),
                (You("Is it safe?"), Say("It's a job. It pays because it isn't.")),
                (You("Sounds fine."), Say("Good. Hold on to that.")),
            }),
            L(Say("Here's how hiring works. There's a training job, a house on Larch Court. Empty. Nothing pays, and nothing in there can kill you."), shot: 3),
            L(Say("A trainer talks you through it on the radio. Carry things, read the floors, load the truck. Do it properly."), shot: 0),
            L(Say("Come back here after, and if you've done it right, you're hired. Simple as that."), replies: new[]
            {
                (You("When do I start?"), Say("Now. The van's outside.")),
                (You("And if I don't do it right?"), Say("Then you do it again. We're short-handed.")),
            }),
            L(Say("Go on. Larch Court. I'll have the papers ready."), shot: 5, hold: .8f),
        };

        public static List<DialogueLine> Hired() => new()
        {
            L(Say("There you are. Sit, sit."), shot: 1, hold: .2f),
            L(Say("The trainer says you were quick. Don't be that quick on a real floor."), shot: 0, when: "quick"),
            L(Say("The trainer says you took your time. Good. The ones who rush don't stay long."), shot: 0, when: "slow"),
            L(Say("The trainer says you listened. Most don't."), shot: 0, when: "steady"),
            L(Say("So. You're hired. Welcome to Abandoned Salvaging."), replies: new[]
            {
                (You("Thanks."), Say("Don't thank me. Thank whoever left all that stuff behind.")),
                (You("What's the pay?"), Say("A cut of what you bring out. So bring out more.")),
                (You("Do I get a uniform?"), Say("You get a locker. Pick your kit in a minute.")),
            }),
            L(Say("Sign here. And here. That one says the company isn't responsible for... well. Anything."), shot: 3, hold: .8f),
            L(Say("Right. Pick your kit, then the HQ is yours. The telephone on the dispatch bench has the jobs."), shot: 4),
            L(Say("And mind the floors."), shot: 5, hold: 1f),
        };

        [MenuItem(ToolMenu.Hq + "Build Hiring Dialogue", priority = ToolMenu.HqP + 82)]
        public static void Build()
        {
            PolishAssets.EnsureFolder(Folder);
            Write(InterviewPath, "interview", Interview());
            Write(HiredPath, "hired", Hired());
            AssetDatabase.SaveAssets();
            Debug.Log("[Onboarding] Hiring dialogue written.");
        }

        public static DialogueDefinition Write(string path, string id, List<DialogueLine> lines)
        {
            var d = AssetDatabase.LoadAssetAtPath<DialogueDefinition>(path);
            if (d == null)
            {
                d = ScriptableObject.CreateInstance<DialogueDefinition>();
                AssetDatabase.CreateAsset(d, path);
            }
            d.EditorSetup(id, Speaker, Role, lines);
            EditorUtility.SetDirty(d);
            return d;
        }
    }
}
