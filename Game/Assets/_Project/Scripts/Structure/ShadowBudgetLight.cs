using System.Collections.Generic;
using UnityEngine;

namespace Abandoned.Structure
{
    /// <summary>
    /// Puts a plain scene light (a street lamp) in the shadow budget beside the ceiling fixtures: it casts
    /// shadows while it is one of the lights nearest the camera (<see cref="ShadowDirector"/>).
    /// </summary>
    [RequireComponent(typeof(Light))]
    public class ShadowBudgetLight : MonoBehaviour
    {
        private static readonly List<ShadowBudgetLight> all = new();

        public static IReadOnlyList<ShadowBudgetLight> All => all;
        public Light Lamp { get; private set; }

        private void Awake() => Lamp = GetComponent<Light>();

        private void OnEnable()
        {
            if (!all.Contains(this)) all.Add(this);
        }

        private void OnDisable() => all.Remove(this);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => all.Clear();
    }
}
