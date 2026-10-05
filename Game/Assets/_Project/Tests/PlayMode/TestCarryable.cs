using Abandoned.Core;
using Abandoned.Interaction;
using UnityEngine;

namespace Abandoned.Tests
{
    /// <summary>Minimal ICarryable for interaction tests, independent of loot definitions.</summary>
    public class TestCarryable : MonoBehaviour, ICarryable, IValuable
    {
        public string displayName = "Test item";
        public CarryClass carryClass = CarryClass.OneHand;
        public float weight = 2f;
        public int value = 100;

        public string DisplayName => displayName;
        public CarryClass CarryClass => carryClass;
        public float GameplayWeight => weight;
        public int CurrentValue => value;

        /// <summary>Creates a cube grabbable; components are added in the order Grabbable.Awake expects.</summary>
        public static Grabbable Create(Vector3 position, CarryClass carryClass, float weight, float size = 0.4f)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = $"Test_{carryClass}_{weight}kg";
            go.transform.position = position;
            go.transform.localScale = Vector3.one * size;
            int lootLayer = Abandoned.Core.GameLayers.LootLayer;
            if (lootLayer >= 0) go.layer = lootLayer; // like real loot prefabs
            var body = go.AddComponent<Rigidbody>();
            body.mass = Mathf.Clamp(weight, 0.2f, 60f);
            var carryable = go.AddComponent<TestCarryable>();
            carryable.carryClass = carryClass;
            carryable.weight = weight;
            Physics.SyncTransforms();
            return go.AddComponent<Grabbable>();
        }
    }
}
