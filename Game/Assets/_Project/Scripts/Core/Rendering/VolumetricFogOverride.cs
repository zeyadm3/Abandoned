using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Abandoned.Core
{
    /// <summary>
    /// How thick the air is where the camera stands (0.24.12). Each map's volume profile sets it once
    /// (EditorTools.AvLook); <see cref="VolumetricFogFeature"/> marches it so the flashlight, ceiling lights and
    /// the sun cut visible beams through the dust. Density is extinction per metre, so 0.02 hides about half the
    /// light over 35 m.
    /// </summary>
    [Serializable, VolumeComponentMenu("Abandoned/Volumetric Fog")]
    public sealed class VolumetricFogOverride : VolumeComponent
    {
        public ClampedFloatParameter density = new(0f, 0f, 0.2f);
        [Tooltip("Height (m) under which the fog is at full density.")]
        public FloatParameter baseHeight = new(0f);
        [Tooltip("How fast the fog thins above the base height (per metre); 0 = the same at every height.")]
        public ClampedFloatParameter heightFalloff = new(0.05f, 0f, 2f);
        [Tooltip("Marching stops here (m). Ordinary fog takes over beyond.")]
        public ClampedFloatParameter maxDistance = new(48f, 8f, 160f);
        [Tooltip("Forward scattering: 0 even, 0.8 bright looking into a beam.")]
        public ClampedFloatParameter anisotropy = new(0.55f, -0.2f, 0.9f);
        public ColorParameter albedo = new(new Color(0.85f, 0.85f, 0.82f), false, false, true);
        [Tooltip("The sky light the dust picks up everywhere (keeps fog from reading as black smoke).")]
        public ClampedFloatParameter ambient = new(0.25f, 0f, 2f);
        public ClampedFloatParameter mainLight = new(1f, 0f, 8f);
        public ClampedFloatParameter localLights = new(1f, 0f, 8f);
        [Tooltip("Lights without shadows leak through walls, so their scattering is turned down by this much.")]
        public ClampedFloatParameter unshadowedLights = new(0.25f, 0f, 1f);
        [Tooltip("Drifting dust: 0 even, 1 patchy.")]
        public ClampedFloatParameter noise = new(0.45f, 0f, 1f);
        public ClampedFloatParameter noiseScale = new(0.18f, 0.01f, 2f);
        public Vector3Parameter wind = new(new Vector3(0.12f, 0.02f, 0.08f));

        public bool IsActive => active && density.value > 0f;
    }
}
