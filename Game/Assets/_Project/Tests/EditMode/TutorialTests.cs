using System.Collections.Generic;
using System.Linq;
using Abandoned.EditorTools;
using Abandoned.Tutorial;
using NUnit.Framework;
using UnityEngine;

namespace Abandoned.Tests
{
    /// <summary>The training job's data and rules (no scene): lessons, chapters, the health floor, clip names, build scenes.</summary>
    public class TutorialTests
    {
        [Test]
        public void DefaultLessonsHaveUniqueIdsAndRunThroughTheChaptersInOrder()
        {
            var ids = new HashSet<string>();
            int chapter = 0;
            foreach (TutorialLessons.Lesson lesson in TutorialLessons.All)
            {
                Assert.IsTrue(ids.Add(lesson.Id), $"'{lesson.Id}' is in the list twice");
                Assert.GreaterOrEqual(lesson.Chapter, chapter, $"'{lesson.Id}' goes back a chapter");
                Assert.Less(lesson.Chapter, TutorialLessons.Chapters.Length, $"'{lesson.Id}' has no chapter name");
                chapter = lesson.Chapter;
            }
            Assert.AreEqual(TutorialLessons.Chapters.Length - 1, chapter, "every chapter is taught");
            Assert.AreEqual(TutorialCheck.Lever, TutorialLessons.All.Last().Check, "the training ends with the truck leaving");
        }

        [Test]
        public void EveryDefaultLessonNamesWhatItsCheckAndBeatNeed()
        {
            foreach (TutorialLessons.Lesson lesson in TutorialLessons.All)
            {
                var step = ScriptableObject.CreateInstance<TutorialStepDefinition>();
                step.EditorSetup(lesson.Id, lesson.Chapter, TutorialLessons.Chapters[lesson.Chapter], lesson.Title, lesson.Text, lesson.Crew,
                    lesson.Radio, lesson.Check, lesson.Key, lesson.Area, lesson.Amount, lesson.Beat, lesson.BeatKey, lesson.BeatAmount, true);
                step.EditorSetGuidance(lesson.Guide, lesson.Hud, lesson.Voice);
                var errors = new List<string>();
                step.Validate(errors);
                CollectionAssert.IsEmpty(errors, string.Join("\n", errors));
                Object.DestroyImmediate(step);
            }
        }

        [Test]
        public void TheStructureLessonsNameTheHouseSections()
        {
            var keys = TutorialLessons.All.Where(l => l.Check is TutorialCheck.SectionStage or TutorialCheck.OffSection or TutorialCheck.SectionBraced or TutorialCheck.SectionDown)
                .Select(l => l.Key).Concat(TutorialLessons.All.Where(l => l.Beat is TutorialBeat.ArmCollapse or TutorialBeat.Collapse).Select(l => l.BeatKey));
            foreach (string key in keys)
                Assert.That(key == TutorialHouseStructure.HallwayKey || key == TutorialHouseStructure.BedroomBKey, $"no section '{key}' in the house");
        }

        [Test]
        public void ChapterStartFindsTheFirstLessonOfTheChapter()
        {
            var catalog = ScriptableObject.CreateInstance<TutorialCatalog>();
            var steps = new List<TutorialStepDefinition>();
            int[] chapters = { 0, 0, 1, 1, 1, 2 };
            for (int i = 0; i < chapters.Length; i++)
            {
                var s = ScriptableObject.CreateInstance<TutorialStepDefinition>();
                s.EditorSetup("s" + i, chapters[i], "C", "T", "Do it.", "", "", TutorialCheck.Read, "", "", 1f, TutorialBeat.None, "", 0f, true);
                steps.Add(s);
            }
            catalog.EditorSet(steps);
            Assert.AreEqual(0, catalog.ChapterStart(1));
            Assert.AreEqual(2, catalog.ChapterStart(4));
            Assert.AreEqual(5, catalog.ChapterStart(5));
            Assert.AreEqual(3, catalog.Chapters);
            Assert.AreEqual(3, catalog.IndexOf("s3"));
            foreach (TutorialStepDefinition s in steps) Object.DestroyImmediate(s);
            Object.DestroyImmediate(catalog);
        }

        [Test]
        public void TheHealthFloorLetsHitsHurtButNeverKill()
        {
            float floor = TutorialMode.HealthFloor;
            Assert.AreEqual(10f, TutorialMode.Clamp(100f, 10f), 1e-4f, "an ordinary hit is untouched");
            Assert.AreEqual(100f - floor, TutorialMode.Clamp(100f, 500f), 1e-4f, "a killing blow stops at the floor");
            Assert.AreEqual(0f, TutorialMode.Clamp(floor, 25f), 1e-4f, "already at the floor: nothing more");
        }

        [Test]
        public void DispatcherClipsAreNamedAsTheGeneratorNamesThem()
        {
            // Tools/Audio/generate_tutorial_dispatch.py: "td_" + the first 8 hex digits of the MD5 of the whole line.
            Assert.AreEqual("td_ad22e239", TutorialRadioVoice.ClipName("DISPATCH: That'll do. Bring it home."));
        }

        [Test]
        public void TrainerClipsAreNamedAsTheGeneratorNamesThem()
        {
            // Tools/Audio/generate_tutorial_narration.py: "tn_" + the first 8 hex digits of the MD5 of the spoken line.
            Assert.AreEqual("tn_685b96e5", TutorialRadioVoice.NarrationClipName("And jump."));
        }

        [Test]
        public void EveryDefaultLessonIsSpokenWithoutNamingAKey()
        {
            foreach (TutorialLessons.Lesson lesson in TutorialLessons.All)
            {
                Assert.IsFalse(string.IsNullOrWhiteSpace(lesson.Voice), $"'{lesson.Id}' has no spoken line");
                Assert.IsFalse(lesson.Voice.Contains("{"), $"'{lesson.Id}' speaks a key token: the card shows keys, the voice never does");
            }
        }

        [Test]
        public void GuideLinesAreRead()
        {
            Assert.IsTrue(TutorialGuideSpec.TryParse("take item:lamp", out TutorialGuideSpec take, out _));
            Assert.AreEqual(TutorialGuideKind.Take, take.Kind);
            Assert.AreEqual(TutorialGuideTarget.Item, take.Target);
            Assert.AreEqual("lamp", take.Key);
            Assert.IsTrue(TutorialGuideSpec.TryParse("load truck", out TutorialGuideSpec load, out _));
            Assert.AreEqual(TutorialGuideTarget.Truck, load.Target);
            Assert.IsTrue(TutorialGuideSpec.TryParse("", out TutorialGuideSpec empty, out _), "empty = worked out from the check");
            Assert.AreEqual(TutorialGuideKind.None, empty.Kind);
            Assert.IsTrue(TutorialGuideSpec.TryParse("wait", out _, out _));
            Assert.IsFalse(TutorialGuideSpec.TryParse("fly area:porch", out _, out _), "not an action");
            Assert.IsFalse(TutorialGuideSpec.TryParse("go somewhere:porch", out _, out _), "not a target");
            Assert.IsFalse(TutorialGuideSpec.TryParse("take item", out _, out _), "an item needs its key");
        }

        [Test]
        public void EveryHudFrameALessonAsksForExists()
        {
            foreach (TutorialLessons.Lesson lesson in TutorialLessons.All)
                foreach (string piece in lesson.Hud.Split(new[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries))
                    Assert.IsTrue(TutorialHighlights.Knows(piece.Trim()), $"'{lesson.Id}' frames '{piece}', which the HUD doesn't have");
        }

        [Test]
        public void TheTrainingHouseShipsButIsNeverAContractDestination()
        {
            CollectionAssert.Contains(BuildScenes.All, TutorialHouseLayout.ScenePath);
            CollectionAssert.Contains(BuildScenes.ForPlayer(BuildFlavor.Shareable), TutorialHouseLayout.ScenePath);
            CollectionAssert.Contains(BuildScenes.ForPlayer(BuildFlavor.Release), TutorialHouseLayout.ScenePath);
            CollectionAssert.DoesNotContain(BuildScenes.SalvageScenes, TutorialHouseLayout.ScenePath);
            Assert.AreEqual(TutorialMode.Scene, TutorialHouseLayout.SceneName);
        }

        [Test]
        public void TheHallwayHoldsAPersonButNotAPersonAndTheClock()
        {
            // Floor capacity 3000 kg x the hallway's share x timber (0.75): a person alone sits under the stressed
            // ratio (0.9), a person and the grandfather clock (70 kg) overload it.
            float capacity = 3000f * TutorialHouseStructure.HallwayCapacity * .75f;
            Assert.Less(80f / capacity, .9f);
            Assert.Greater((80f + 70f) / capacity, 1f);
        }
    }
}
