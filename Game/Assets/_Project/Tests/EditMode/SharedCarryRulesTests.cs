using System.Collections.Generic;
using Abandoned.Interaction;
using Abandoned.Loot;
using Abandoned.Networking;
using Abandoned.Player;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Abandoned.Tests
{
    /// <summary>Shared-carry rules that need no scene: carry point layout, crew sizes, tether, HUD text, replicated state, data.</summary>
    public class SharedCarryRulesTests
    {
        private const string ConfigPath = "Assets/_Project/Data/Interaction/SharedCarryConfig.asset";

        private static SharedCarryConfig Config() => AssetDatabase.LoadAssetAtPath<SharedCarryConfig>(ConfigPath);

        [Test]
        public void TwoPointsSitAtTheEndsOfTheLongestHorizontalAxis()
        {
            // Server rack: 0.6 wide, 2 tall, 1 deep -> handles front and back, never on the tall axis.
            List<Vector3> rack = CarryPointLayout.Generate(new Vector3(0.6f, 2f, 1f), 2);
            CollectionAssert.AreEqual(new[] { new Vector3(0f, 0f, 0.5f), new Vector3(0f, 0f, -0.5f) }, rack);
            List<Vector3> wide = CarryPointLayout.Generate(new Vector3(2f, 1f, 0.5f), 2);
            CollectionAssert.AreEqual(new[] { new Vector3(1f, 0f, 0f), new Vector3(-1f, 0f, 0f) }, wide);
        }

        [Test]
        public void FourPointsAddTheMiddlesOfTheLongSides()
        {
            List<Vector3> piano = CarryPointLayout.Generate(new Vector3(1.5f, 1f, 2.4f), 4);
            CollectionAssert.AreEqual(new[]
            {
                new Vector3(0f, 0f, 1.2f), new Vector3(0f, 0f, -1.2f), new Vector3(0.75f, 0f, 0f), new Vector3(-0.75f, 0f, 0f),
            }, piano);
            Assert.AreEqual(4, CarryPointLayout.Generate(Vector3.one, 9).Count, "never more than four handles");
            Assert.AreEqual(1, CarryPointLayout.Generate(Vector3.one, 0).Count);
        }

        [Test]
        public void CrewSizesComeFromTheConfigUnlessTheDefinitionOverrides()
        {
            SharedCarryConfig config = Config();
            Assert.IsNotNull(config, "run RebuildContent");
            Assert.AreEqual(2, config.RequiredFor(CarryClass.Heavy, 0));
            Assert.AreEqual(3, config.RequiredFor(CarryClass.Huge, 0), "GDD: 3-4 people for Huge");
            Assert.AreEqual(4, config.RequiredFor(CarryClass.Huge, 4));
            Assert.AreEqual(4, config.RequiredFor(CarryClass.Heavy, 9), "clamped to the most handles an item can have");
            Assert.AreEqual(2, config.PointCountFor(CarryClass.Heavy, 2));
            Assert.AreEqual(4, config.PointCountFor(CarryClass.Huge, 3));
            Assert.AreEqual(3, config.PointCountFor(CarryClass.Heavy, 3), "always a handle per required carrier");
            var errors = new List<string>();
            config.Validate(errors);
            Assert.IsEmpty(errors);
        }

        [Test]
        public void TetherLetsYouMoveFreelyInsideTheSlackOnly()
        {
            Vector3 v = new(0f, 0f, 2f);
            Assert.AreEqual(v, TetherMath.Constrain(new Vector3(0f, 5f, 0.5f), v, 0.9f, 1.4f, 3f), "inside the slack (height ignored)");
            Assert.AreEqual(Vector3.zero, TetherMath.Constrain(new Vector3(0f, 0f, 1f), v, 0.9f, 1.4f, 3f), "walking further away is blocked");
            Vector3 sideways = TetherMath.Constrain(new Vector3(0f, 0f, 1f), new Vector3(1f, 0f, 1f), 0.9f, 1.4f, 3f);
            Assert.AreEqual(new Vector3(1f, 0f, 0f), sideways, "moving around the handle still works");
            Assert.AreEqual(-v, TetherMath.Constrain(new Vector3(0f, 0f, 1f), -v, 0.9f, 1.4f, 3f), "walking back toward it is free");
            Vector3 pulled = TetherMath.Constrain(new Vector3(0f, 0f, 3f), Vector3.zero, 0.9f, 1.4f, 3f);
            Assert.AreEqual(-3f, pulled.z, 1e-4f, "far away: pulled back at up to the pull speed");
        }

        [Test]
        public void HudTextSaysHowManyMorePeopleAreNeeded()
        {
            Assert.AreEqual("Carrying 1/2 - needs 1 more person (dragging)", SharedCarryText.Status(1, 2, true));
            Assert.AreEqual("Carrying 1/3 - needs 2 more people (barely budges)", SharedCarryText.Status(1, 3, false));
            Assert.AreEqual("Carrying 3/3 - lifted", SharedCarryText.Status(3, 3, false));
            Assert.AreEqual("needs 2 people", SharedCarryText.Crew(0, 2));
            Assert.AreEqual("2/3 holding", SharedCarryText.Crew(2, 3));
        }

        [Test]
        public void ReplicatedCrewStateTracksHoldersAndGrips()
        {
            var s = default(SharedCarryState);
            Assert.AreEqual(0, s.Count);
            s.Set(1, 42, new Vector3(0.5f, 0f, 0.5f));
            s.Set(3, 7, Vector3.right);
            Assert.AreEqual(2, s.Count);
            Assert.AreEqual(1, s.IndexOf(42));
            Assert.AreEqual(3, s.IndexOf(7));
            Assert.AreEqual(-1, s.IndexOf(0), "0 means a free handle, never a holder");
            Assert.AreEqual(new Vector3(0.5f, 0f, 0.5f), s.GripAt(1));
            s.Set(1, 0, Vector3.one);
            Assert.AreEqual(Vector3.zero, s.GripAt(1), "a free handle carries no grip");
            Assert.AreEqual(1, s.Count);
        }

        [Test]
        public void OnlyHeavyAndHugeLootIsCarriedTogether()
        {
            foreach ((string id, bool shared) in new[] { ("server_rack", true), ("grand_piano", true), ("laptop", false), ("flatscreen_tv", false) })
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/_Project/Prefabs/Loot/Loot_{id}.prefab");
                Assert.AreEqual(shared, prefab.GetComponent<SharedCarryable>() != null, $"{id}: SharedCarryable");
                Assert.AreEqual(shared, prefab.GetComponent<NetworkSharedCarry>() != null, $"{id}: NetworkSharedCarry");
            }
        }

        [Test]
        public void TheMilitaryGeneratorNeedsFourPeople()
        {
            // GDD 7.1: "Needs 4 people or a trolley + ramp" - data, not code.
            var generator = AssetDatabase.LoadAssetAtPath<LootDefinition>("Assets/_Project/Data/Loot/Loot_military_generator.asset");
            Assert.AreEqual(4, Config().RequiredFor(generator.CarryClass, generator.RequiredCarriers));
            var piano = AssetDatabase.LoadAssetAtPath<LootDefinition>("Assets/_Project/Data/Loot/Loot_grand_piano.asset");
            Assert.AreEqual(3, Config().RequiredFor(piano.CarryClass, piano.RequiredCarriers));
        }

        [Test]
        public void DefinitionsRejectCrewDataOnLightItemsAndTooFewHandles()
        {
            var light = ScriptableObject.CreateInstance<LootDefinition>();
            light.EditorSetup("x", "X", 1, 1, 1f, CarryClass.OneHand, Vector3.one * 0.3f, Fragility.Low, Core.SurfaceMaterial.Plastic, 0.1f, 1f, PlaceholderShape.Cube, Color.white);
            SetCrew(light, 2, new Vector3[0]);
            var errors = new List<string>();
            light.Validate(errors);
            Assert.IsTrue(errors.Exists(e => e.Contains("only Heavy/Huge")), string.Join("\n", errors));

            var heavy = ScriptableObject.CreateInstance<LootDefinition>();
            heavy.EditorSetup("y", "Y", 1, 1, 300f, CarryClass.Heavy, Vector3.one, Fragility.Low, Core.SurfaceMaterial.Metal, 0.1f, 1f, PlaceholderShape.Cube, Color.white);
            SetCrew(heavy, 3, new[] { Vector3.forward, Vector3.back });
            errors.Clear();
            heavy.Validate(errors);
            Assert.IsTrue(errors.Exists(e => e.Contains("more than its carry points")), string.Join("\n", errors));
            Object.DestroyImmediate(light);
            Object.DestroyImmediate(heavy);
        }

        private static void SetCrew(LootDefinition definition, int required, Vector3[] points)
        {
            var so = new SerializedObject(definition);
            so.FindProperty("<RequiredCarriers>k__BackingField").intValue = required;
            SerializedProperty array = so.FindProperty("<CarryPoints>k__BackingField");
            array.arraySize = points.Length;
            for (int i = 0; i < points.Length; i++) array.GetArrayElementAtIndex(i).vector3Value = points[i];
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
