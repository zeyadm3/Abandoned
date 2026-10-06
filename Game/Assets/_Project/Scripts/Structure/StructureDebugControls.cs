using Abandoned.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Abandoned.Structure
{
    /// <summary>
    /// Debug keys for fast structure testing (host only): − / = change stability by 10%,
    /// F2 restores every section and re-rolls pre-damage with a new seed. Only while the F1 debug
    /// view is on, and only in the editor or development builds, so they can't fire by accident.
    /// </summary>
    public class StructureDebugControls : MonoBehaviour
    {
        [SerializeField] private StructureSimulation simulation;
        [SerializeField, Range(0.01f, 0.5f)] private float stabilityStep = 0.1f;

        private AbandonedInput input;
        private InputAction down, up, reroll;

        private void Awake()
        {
            input = new AbandonedInput();
            // Looked up by name so this doesn't depend on wrapper regeneration order.
            down = input.asset.FindAction("Debug/StabilityDown", true);
            up = input.asset.FindAction("Debug/StabilityUp", true);
            reroll = input.asset.FindAction("Debug/RerollStructure", true);
        }

        private void OnEnable()
        {
            down.Enable();
            up.Enable();
            reroll.Enable();
        }

        private void OnDisable()
        {
            down.Disable();
            up.Disable();
            reroll.Disable();
        }

        private void OnDestroy() => input?.Dispose();

        private void Update()
        {
            if (!GameAuthority.IsHost || !DebugView.Visible || !Debug.isDebugBuild) return;
            if (down.WasPressedThisFrame()) simulation.ApplyStability(simulation.Stability - stabilityStep, simulation.Seed);
            if (up.WasPressedThisFrame()) simulation.ApplyStability(simulation.Stability + stabilityStep, simulation.Seed);
            if (reroll.WasPressedThisFrame()) simulation.ApplyStability(simulation.Stability, simulation.Seed + 1);
        }
    }
}
