using UnityEngine;

namespace Abandoned.Extraction
{
    /// <summary>
    /// The horror pacing for a level (arrival beats, pressure sounds, scares, light failure, the final
    /// phase). Positions are the level's own world coordinates, so a new level is new data, not new code.
    /// </summary>
    [CreateAssetMenu(menuName="Abandoned/Extraction/Horror Config")]
    public class HorrorConfig : ScriptableObject
    {
        [Tooltip("Scenes this pacing runs in (by scene name). Anywhere else the director stays off.")]
        [SerializeField] private string[] levels = { "Mall" };

        [field: SerializeField, Min(10)] public float FinalDelay { get; private set; } = 90f;
        [field: Tooltip("Extra seconds before the final phase when only one player is on the job.")]
        [field: SerializeField, Min(0)] public float SoloFinalDelayBonus { get; private set; } = 60f;
        [field: SerializeField, Min(45)] public float ScareInterval { get; private set; } = 95f;
        [field: SerializeField, Min(5)] public float PressureInterval { get; private set; } = 23f;
        [field: Tooltip("From this danger level a scare may also slam an open store shutter (never on anyone).")]
        [field: SerializeField, Range(0, 10)] public int SlamFromDanger { get; private set; } = 2;
        [field: SerializeField] public Bounds Building { get; private set; } = new(new Vector3(28,4,24),new Vector3(56,16,48));
        [field: SerializeField] public GameObject ScareModel { get; private set; }

        [field: Header("Arrival (seconds after the first player walks in)")]
        [field: SerializeField] public float[] ArrivalBeats { get; private set; } = { 2f, 9f, 17f, 30f, 46f };
        [field: Tooltip("Where the arrival sounds play when nobody is inside to anchor them.")]
        [field: SerializeField] public Vector3 ArrivalFallback { get; private set; } = new(28f, 0f, 10f);
        [field: Tooltip("The figure that crosses the far walkway on the fourth beat, and the door that bangs with it.")]
        [field: SerializeField] public Vector3 ArrivalFigure { get; private set; } = new(26f, 4f, 29f);
        [field: SerializeField] public Vector3 ArrivalDoor { get; private set; } = new(40f, 4f, 30f);
        [field: Tooltip("The wave of dying lights that rolls along +Z during the arrival: start/end seconds, metres per second.")]
        [field: SerializeField] public Vector2 ArrivalBlackout { get; private set; } = new(5f, 16f);
        [field: SerializeField, Min(0.1f)] public float ArrivalBlackoutSpeed { get; private set; } = 5f;

        [field: Header("Final phase")]
        [field: Tooltip("Where the roar comes from when the final phase starts.")]
        [field: SerializeField] public Vector3 RoarPoint { get; private set; } = new(28f, 8f, 24f);

        [field: Header("Lights")]
        [field: Tooltip("Lights are grouped into sectors by this cell size; one sector loses power per danger level.")]
        [field: SerializeField] public Vector3 SectorCell { get; private set; } = new(12f, 4f, 12f);
        [field: SerializeField, Min(1)] public int Sectors { get; private set; } = 6;
        [field: Tooltip("This many sectors keep their power at any danger level (the final phase turns everything red).")]
        [field: SerializeField, Min(0)] public int SectorsLitAtMaxDanger { get; private set; } = 2;

        public bool RunsIn(string sceneName) => levels != null && System.Array.IndexOf(levels, sceneName) >= 0;

        /// <summary>Which light sector a point belongs to (stable across machines: same maths, same numbers).</summary>
        public int SectorOf(Vector3 at)
        {
            int x = Mathf.FloorToInt(at.x / Mathf.Max(0.1f, SectorCell.x));
            int y = Mathf.FloorToInt(at.y / Mathf.Max(0.1f, SectorCell.y));
            int z = Mathf.FloorToInt(at.z / Mathf.Max(0.1f, SectorCell.z));
            return Mathf.Abs(x + 3 * z + y) % Mathf.Max(1, Sectors);
        }

        /// <summary>Does a sector still have power at this danger level?</summary>
        public bool SectorPowered(int sector, int danger) => sector >= Mathf.Min(danger, Sectors - SectorsLitAtMaxDanger);
    }
}
