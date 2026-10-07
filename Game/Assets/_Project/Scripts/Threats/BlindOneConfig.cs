using System.Collections.Generic;
using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Threats
{
    /// <summary>The Blind One's tuning (GDD 9: can't see, hunts by sound). One asset in Data/Threats.</summary>
    [CreateAssetMenu(menuName = "Abandoned/Threats/Blind One Config", fileName = "BlindOneConfig")]
    public class BlindOneConfig : ScriptableObject, IValidatable
    {
        [Header("Hearing")]
        [Tooltip("Multiplies every noise's hearing radius (danger raises it).")]
        [field: SerializeField, Range(0.2f, 3f)] public float Hearing { get; private set; } = 1f;
        [Tooltip("Each wall between it and a noise multiplies the radius by this.")]
        [field: SerializeField, Range(0f, 1f)] public float WallDamping { get; private set; } = 0.6f;
        [Tooltip("How long (s) a heard noise stays interesting; its pull fades over this time.")]
        [field: SerializeField, Range(1f, 30f)] public float Memory { get; private set; } = 8f;

        [Header("Behaviour")]
        [field: SerializeField, Range(0.5f, 4f)] public float WalkSpeed { get; private set; } = 1.6f;
        [field: SerializeField, Range(1f, 10f)] public float HuntSpeed { get; private set; } = 4.2f;
        [Tooltip("Perceived strength (0..1) that makes it hunt straight away.")]
        [field: SerializeField, Range(0.05f, 1f)] public float HuntThreshold { get; private set; } = 0.45f;
        [Tooltip("This many noises heard within HuntWindow also make it hunt (someone keeps making noise).")]
        [field: SerializeField, Range(2, 10)] public int HuntNoiseCount { get; private set; } = 3;
        [field: SerializeField, Range(1f, 15f)] public float HuntWindow { get; private set; } = 5f;
        [Tooltip("A hunt ends this long (s) after the last noise; it goes to look where it was.")]
        [field: SerializeField, Range(1f, 20f)] public float HuntForget { get; private set; } = 6f;
        [Tooltip("Seconds it stands and listens where a noise came from before wandering off.")]
        [field: SerializeField, Range(0f, 15f)] public float Linger { get; private set; } = 4f;
        [field: SerializeField, Range(2f, 40f)] public float WanderRadius { get; private set; } = 16f;

        [Header("Attack")]
        [Tooltip("Touching distance (m, horizontal): it kills on contact.")]
        [field: SerializeField, Range(0.5f, 3f)] public float AttackRange { get; private set; } = 1.2f;
        [field: SerializeField, Range(0f, 10f)] public float AttackPause { get; private set; } = 2.5f;

        [Header("Presence")]
        [Tooltip("Seconds into a run before it appears (players get a head start).")]
        [field: SerializeField, Range(0f, 300f)] public float SpawnDelay { get; private set; } = 45f;
        [Tooltip("Seconds between its clicks while walking / hunting (its signature sound, GDD 9).")]
        [field: SerializeField, Range(0.1f, 3f)] public float ClickInterval { get; private set; } = 0.9f;
        [field: SerializeField, Range(0.05f, 2f)] public float HuntClickInterval { get; private set; } = 0.3f;

        public void Validate(List<string> errors)
        {
            if (HuntSpeed < WalkSpeed) errors.Add($"{name}: HuntSpeed should be at least WalkSpeed.");
        }
    }
}
