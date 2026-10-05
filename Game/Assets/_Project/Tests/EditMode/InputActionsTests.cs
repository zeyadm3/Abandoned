using NUnit.Framework;
using UnityEditor;
using UnityEngine.InputSystem;

namespace Abandoned.Tests
{
    /// <summary>Controls must match GDD section 19; remapping later starts from these defaults.</summary>
    public class InputActionsTests
    {
        private const string AssetPath = "Assets/_Project/Data/Core/AbandonedInput.inputactions";

        [TestCase("Gameplay", "Sprint", "<Keyboard>/leftShift")]
        [TestCase("Gameplay", "Crouch", "<Keyboard>/c")]
        [TestCase("Gameplay", "Jump", "<Keyboard>/space")]
        [TestCase("Gameplay", "Interact", "<Keyboard>/e")]
        [TestCase("Gameplay", "Use", "<Mouse>/leftButton")]
        [TestCase("Gameplay", "Drop", "<Mouse>/rightButton")]
        [TestCase("Gameplay", "Scan", "<Keyboard>/q")]
        [TestCase("Gameplay", "Flashlight", "<Keyboard>/f")]
        [TestCase("Gameplay", "HandSlot1", "<Keyboard>/1")]
        [TestCase("Gameplay", "HandSlot2", "<Keyboard>/2")]
        [TestCase("Gameplay", "Inventory", "<Keyboard>/tab")]
        [TestCase("Gameplay", "PushToTalk", "<Keyboard>/v")]
        [TestCase("Gameplay", "Radio", "<Keyboard>/r")]
        [TestCase("Gameplay", "Pause", "<Keyboard>/escape")]
        [TestCase("Gameplay", "Look", "<Mouse>/delta")]
        [TestCase("Debug", "ToggleDebug", "<Keyboard>/f1")]
        [TestCase("Debug", "ToggleRagdoll", "<Keyboard>/k")]
        [TestCase("UI", "Cancel", "<Keyboard>/escape")]
        [TestCase("UI", "Point", "<Mouse>/position")]
        public void ActionHasDefaultBinding(string map, string action, string path)
        {
            var asset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(AssetPath);
            Assert.IsNotNull(asset, "Input asset missing");
            InputAction a = asset.FindActionMap(map, true).FindAction(action, true);
            Assert.That(a.bindings, Has.Some.Matches<InputBinding>(b => b.path == path),
                $"{map}/{action} should be bound to {path}");
        }

        [Test]
        public void MoveIsWasdComposite()
        {
            // Reads the asset rather than `new AbandonedInput()`: its Dispose calls Destroy, which errors in Edit mode.
            var asset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(AssetPath);
            InputAction move = asset.FindActionMap("Gameplay", true).FindAction("Move", true);
            string[] parts = { "<Keyboard>/w", "<Keyboard>/s", "<Keyboard>/a", "<Keyboard>/d" };
            foreach (string part in parts)
                Assert.That(move.bindings, Has.Some.Matches<InputBinding>(b => b.isPartOfComposite && b.path == part), part);
        }
    }
}
