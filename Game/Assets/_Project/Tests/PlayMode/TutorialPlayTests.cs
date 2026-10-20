using System.Collections;
using System.Linq;
using Abandoned.Extraction;
using Abandoned.Networking;
using Abandoned.Tutorial;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Abandoned.Tests
{
    /// <summary>
    /// The training house, hosted solo: the training starts on lesson one with its salvage in the house, nobody can
    /// die there, and skipping through every lesson runs each one's beat (the visitor, the falling floor, the doors)
    /// without an error, until the truck leaves and the training counts as done.
    /// </summary>
    public class TutorialPlayTests
    {
        [UnityTest, Timeout(300000)]
        public IEnumerator TheTrainingRunsFromTheFirstLessonToTheTruck()
        {
            yield return TestMapScene.Load(TutorialMode.Scene);
            for (int i = 0; i < 240 && RunTraining.Current == null; i++) yield return null;
            Assert.IsTrue(TutorialMode.Active, "the training house is the training");
            RunTraining training = RunTraining.Current;
            Assert.IsNotNull(training, "the run state carries the training");
            Assert.AreEqual(0, training.Step, "it starts on the first lesson");
            Assert.Greater(TutorialLootSpot.All.Count, 0, "the house has its salvage spots");
            Assert.IsNotNull(training.Item("fridge"), "the fridge is real salvage");
            Assert.AreEqual(TutorialConfig.Current.Quota, RunState.Current.State.Quota, "the training's own quota");

            // Nobody dies in training: a killing blow stops at the floor.
            NetworkPlayer me = NetworkPlayer.Local;
            me.DealDamage(10000f, "test");
            Assert.IsFalse(me.IsDead);
            Assert.AreEqual(TutorialMode.HealthFloor, me.Health, 0.01f);
            me.ServerHeal(me.MaxHealth);

            // The host skips every lesson but the last; each one's beat runs on the way.
            int lessons = training.Catalog.Count;
            for (int guard = 0; guard < lessons * 2 && training.Step < lessons - 1; guard++)
            {
                training.RequestSkip();
                yield return new WaitForSeconds(0.3f);
            }
            Assert.AreEqual(lessons - 1, training.Step, "every lesson but the truck can be skipped");
            Assert.IsTrue(training.VisitorGone || Object.FindObjectsByType<Threats.Threat>(FindObjectsSortMode.None).Length <= 1, "at most the one visitor");

            // The last lesson: the truck leaves.
            me.OwnerTeleport(TruckCargo.Current.Ignition.position + Vector3.up * 0.1f);
            yield return new WaitForSeconds(0.5f);
            RunState.Current.RequestDepart();
            float leave = Time.time + RunState.Current.HonkSeconds + 6f;
            while (!training.Finished && Time.time < leave)
            {
                if (RunState.Current.State.Phase == RunPhase.Running) RunState.Current.RequestDepart();
                yield return null;
            }
            Assert.IsTrue(training.Finished, "the truck leaving finishes the training");
        }

        [UnityTest, Timeout(120000)]
        public IEnumerator ACollapsedFloorIsSafeToFallThrough()
        {
            yield return TestMapScene.Load(TutorialMode.Scene);
            for (int i = 0; i < 240 && RunTraining.Current == null; i++) yield return null;
            Structure.StructuralSection patch = TutorialSection.Find("bedroom_b");
            Assert.IsNotNull(patch, "the south bedroom's rotten floor is a named section");
            Assert.IsTrue(patch.CanCollapse);
            Structure.StructuralSection hallway = TutorialSection.Find("hallway");
            Assert.IsNotNull(hallway, "the hallway is a named section");
            // Stand in the middle of the patch, let it fail, and land in the basement unhurt.
            NetworkPlayer me = NetworkPlayer.Local;
            Bounds b = patch.SurfaceBounds;
            me.OwnerTeleport(new Vector3(b.center.x + 1f, b.max.y + 0.1f, b.center.z));
            yield return new WaitForSeconds(0.5f);
            float health = me.Health;
            Object.FindAnyObjectByType<Structure.StructureSimulation>().StartFailing(patch);
            yield return new WaitForSeconds(5f);
            Assert.IsTrue(patch.IsCollapsed);
            float y = me.Ragdoll.IsRagdolled ? me.Ragdoll.BodyPosition.y : me.transform.position.y;
            Assert.Less(y, -0.5f, "fell into the basement (perhaps onto the rubble)");
            Assert.IsFalse(me.IsDead);
            Assert.GreaterOrEqual(me.Health, health - 1f, "a fall into the basement is under the safe height");
        }
    }
}
