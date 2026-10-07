using Abandoned.Core;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Abandoned.Tests
{
    /// <summary>M7.4c: rebinding is saved once and applied to every action set, and clashes are found.</summary>
    public class InputBindingsTests
    {
        private string saved;

        [SetUp]
        public void SetUp() => saved = InputBindings.Json;

        [TearDown]
        public void TearDown()
        {
            // Put back whatever the developer had (tests share the editor's PlayerPrefs).
            var restore = new AbandonedInput();
            if (!string.IsNullOrEmpty(saved)) restore.asset.LoadBindingOverridesFromJson(saved);
            if (string.IsNullOrEmpty(saved)) InputBindings.ResetAll();
            else InputBindings.Save(restore.asset);
            Free(restore);
        }

        // The generated Dispose uses Destroy, which edit mode refuses.
        private static void Free(AbandonedInput input) => Object.DestroyImmediate(input.asset);

        [Test]
        public void ARebindReachesEveryInstanceAndTheHudPrompt()
        {
            InputBindings.ResetAll();
            var edited = new AbandonedInput();
            var player = new AbandonedInput();
            InputBindings.Apply(player.asset);
            Assert.AreEqual("<Keyboard>/e", player.Gameplay.Interact.bindings[0].effectivePath);

            int changes = 0;
            void Count() => changes++;
            InputBindings.Changed += Count;
            edited.Gameplay.Interact.ApplyBindingOverride(0, "<Keyboard>/g");
            InputBindings.Save(edited.asset);
            InputBindings.Changed -= Count;
            Assert.AreEqual(1, changes, "live objects are told");

            InputBindings.Apply(player.asset);
            Assert.AreEqual("<Keyboard>/g", player.Gameplay.Interact.bindings[0].effectivePath);
            Assert.AreEqual("G", InputBindings.Display("Interact"));

            InputBindings.ResetAll();
            InputBindings.Apply(player.asset);
            Assert.AreEqual("<Keyboard>/e", player.Gameplay.Interact.bindings[0].effectivePath, "reset to defaults");
            Free(edited);
            Free(player);
        }

        [Test]
        public void ClashesNameTheOtherAction()
        {
            var input = new AbandonedInput();
            InputAction jump = input.Gameplay.Jump;
            Assert.AreEqual("Crouch", InputBindings.ConflictWith(input.asset, jump, 0, "<Keyboard>/c"));
            Assert.AreEqual("Move up", InputBindings.ConflictWith(input.asset, jump, 0, "<Keyboard>/w"));
            Assert.IsNull(InputBindings.ConflictWith(input.asset, jump, 0, "<Keyboard>/k"));
            Free(input);
        }
    }
}
