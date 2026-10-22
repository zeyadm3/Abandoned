using UnityEngine;
namespace Abandoned.Audio
{
    public sealed class MotelSoundConfig : ScriptableObject
    {
        [SerializeField,
        Range(0f, 1f)] private float neonLevel = .16f;
        [SerializeField,
        Range(0f, 1f)] private float iceLevel = .18f;
        [SerializeField,
        Range(0f, 1f)] private float windLevel = .16f;
        [SerializeField,
        Range(0f, 1f)] private float dripLevel = .2f;
        [SerializeField,
        Range(0f, 1f)] private float televisionLevel = .24f;
        [SerializeField] private Vector2 televisionEvery = new(26f, 57f);
        public float NeonLevel => neonLevel;
        public float IceLevel => iceLevel;
        public float WindLevel => windLevel;
        public float DripLevel => dripLevel;
        public float TelevisionLevel => televisionLevel;
        public Vector2 TelevisionEvery => televisionEvery;
    }
}
