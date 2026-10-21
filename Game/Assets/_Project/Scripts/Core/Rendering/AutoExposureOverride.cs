using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Abandoned.Core
{
    /// <summary>
    /// Eye adaptation (0.24.12). Deliberately soft: <see cref="strength"/> 0.3 pulls the exposure only part of the
    /// way toward <see cref="key"/>, inside a narrow range, so a dark hall stays dark (the horror is in the dark) but
    /// stepping in from daylight, or a flashlight in your face, settles over a second or two instead of staying wrong.
    /// </summary>
    [Serializable, VolumeComponentMenu("Abandoned/Auto Exposure")]
    public sealed class AutoExposureOverride : VolumeComponent
    {
        public BoolParameter enabled = new(false);
        [Tooltip("The average brightness the eye aims for (linear, centre-weighted).")]
        public ClampedFloatParameter key = new(0.1f, 0.005f, 1f);
        [Tooltip("0 = never adapts, 1 = always reaches the key.")]
        public ClampedFloatParameter strength = new(0.3f, 0f, 1f);
        public ClampedFloatParameter minEv = new(-1.5f, -4f, 0f);
        public ClampedFloatParameter maxEv = new(1f, 0f, 4f);
        [Tooltip("Stops per second when the view gets brighter (pupils close fast).")]
        public ClampedFloatParameter speedDown = new(2.5f, 0.1f, 10f);
        [Tooltip("Stops per second when the view gets darker (eyes open slowly).")]
        public ClampedFloatParameter speedUp = new(0.7f, 0.1f, 10f);

        public bool IsActive => active && enabled.value && strength.value > 0f;
    }
}
