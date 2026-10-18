using Abandoned.Core;
using Abandoned.Equipment;
using Abandoned.Loot;
using Abandoned.Networking;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Abandoned.Tests
{
    public class FieldHudRulesTests
    {
        [Test]
        public void FourEquipmentPocketsRemainIndependent()
        {
            EquipState state = EquipState.Empty;
            for (int slot = 0; slot < EquipState.SlotCount; slot++)
            {
                Assert.AreEqual(-1, state[slot]);
                state = state.With(slot, slot + 4);
            }
            Assert.AreEqual(4, EquipState.SlotCount);
            for (int slot = 0; slot < EquipState.SlotCount; slot++) Assert.AreEqual(slot + 4, state[slot]);
            Assert.IsFalse(state.Equals(state.With(3, -1)), "the fourth pocket must replicate changes");
            Assert.AreEqual(6, state.With(0, -1)[2], "discarding never shifts other pockets");
        }

        [Test]
        public void PresentedLootRetainsItsReservedPocket()
        {
            LootHoldState state = LootHoldState.For(LootHoldMode.PocketHeld, 7, 3);
            Assert.IsTrue(state.IsHeldBy(7));
            Assert.AreEqual(3, state.PocketSlot);
            Assert.AreEqual(3, LootHoldState.For(LootHoldMode.Pocketed, 7, state.PocketSlot).PocketSlot);
        }

        [Test]
        public void SoftImpactsDoNotDepleteAnOutdoorProtectionPool()
        {
            for (int hit = 0; hit < 20; hit++) Assert.AreEqual(0, LootMath.ProtectedImpactLoss(400, 1000, 50));
            Assert.AreEqual(0, LootMath.ProtectedImpactLoss(500, 1000, 50));
            Assert.AreEqual(150, LootMath.ProtectedImpactLoss(650, 1000, 50));
            Assert.AreEqual(500, LootMath.ProtectedImpactLoss(1000, 1000, 50));
        }

        [Test]
        public void ActualPriceIsHiddenUntilHostAppraisal()
        {
            Assert.IsFalse(new LootValueState(1000, 750, 1f, false).Appraised);
            Assert.IsTrue(new LootValueState(1000, 750, 1f, false, true).Appraised);
        }

        [Test]
        public void ExtraPocketBindingsSurviveCreatingAnotherInputReader()
        {
            InputActionAsset a = ScriptableObject.CreateInstance<InputActionAsset>();
            InputActionAsset b = ScriptableObject.CreateInstance<InputActionAsset>();
            try
            {
                a.AddActionMap("Gameplay"); b.AddActionMap("Gameplay");
                InputBindings.Apply(a); InputBindings.Apply(b);
                foreach (int slot in new[] { 3, 4 })
                {
                    InputAction first = a.FindAction("Gameplay/HandSlot" + slot, true);
                    InputAction second = b.FindAction("Gameplay/HandSlot" + slot, true);
                    Assert.AreEqual(first.bindings[0].id, second.bindings[0].id);
                    first.ApplyBindingOverride("<Keyboard>/z");
                }
                b.LoadBindingOverridesFromJson(a.SaveBindingOverridesAsJson());
                Assert.AreEqual("<Keyboard>/z", b.FindAction("Gameplay/HandSlot4").bindings[0].effectivePath);
            }
            finally { Object.DestroyImmediate(a); Object.DestroyImmediate(b); }
        }
    }
}
