using System;
using System.Collections.Generic;
using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Audio
{
    /// <summary>
    /// Every library sound in one asset (Data/Audio/Resources/SoundBank): named cues, and impacts and
    /// footsteps per surface material. Swapping a sound is a data change; a cue without clips falls
    /// back to the synthesised placeholder.
    /// </summary>
    [CreateAssetMenu(menuName = "Abandoned/Audio/Sound Bank", fileName = "SoundBank")]
    public class SoundBank : ScriptableObject
    {
        public const string ResourcePath = "SoundBank";

        [Serializable]
        public class Named
        {
            public SoundId Id;
            public SoundCue Cue = new();
        }

        [Serializable]
        public class ByMaterial
        {
            public SurfaceMaterial Material;
            [Tooltip("Knocks and taps.")] public SoundCue Light = new();
            [Tooltip("Drops and hard hits (louder than HeavyFrom).")] public SoundCue Heavy = new();
        }

        [SerializeField] private List<Named> cues = new();
        [SerializeField] private List<ByMaterial> impacts = new();
        [SerializeField] private List<ByMaterial> footsteps = new();
        [Tooltip("Impact volume (0..1) from which the heavy variation plays.")]
        [SerializeField, Range(0f, 1f)] private float heavyFrom = 0.6f;

        public IReadOnlyList<Named> Cues => cues;
        public IReadOnlyList<ByMaterial> Impacts => impacts;
        public IReadOnlyList<ByMaterial> Footsteps => footsteps;

        public SoundCue Get(SoundId id)
        {
            foreach (Named n in cues) if (n.Id == id) return n.Cue;
            return null;
        }

        public SoundCue Impact(SurfaceMaterial material, float volume01) => Pick(impacts, material, volume01 >= heavyFrom);

        public SoundCue Footstep(SurfaceMaterial material) => Pick(footsteps, material, heavy: false);

        private static SoundCue Pick(List<ByMaterial> list, SurfaceMaterial material, bool heavy)
        {
            foreach (ByMaterial m in list)
                if (m.Material == material) return heavy && m.Heavy.HasClips ? m.Heavy : m.Light;
            return null;
        }

#if UNITY_EDITOR
        public void EditorSet(List<Named> named, List<ByMaterial> impactList, List<ByMaterial> footstepList)
        {
            cues = named;
            impacts = impactList;
            footsteps = footstepList;
        }
#endif
    }
}
