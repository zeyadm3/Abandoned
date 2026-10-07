using UnityEngine;

namespace Abandoned.Extraction
{
    [CreateAssetMenu(menuName="Abandoned/Extraction/Horror Config")]
    public class HorrorConfig : ScriptableObject
    {
        [field: SerializeField, Min(10)] public float FinalDelay { get; private set; } = 90f;
        [field: SerializeField, Min(45)] public float ScareInterval { get; private set; } = 95f;
        [field: SerializeField, Min(5)] public float PressureInterval { get; private set; } = 23f;
        [field: SerializeField] public Bounds Building { get; private set; } = new(new Vector3(28,4,24),new Vector3(56,16,48));
        [field: SerializeField] public GameObject ScareModel { get; private set; }
    }
}
