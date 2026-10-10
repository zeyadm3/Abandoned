using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Extraction
{
    /// <summary>
    /// The mall's freight lift (0.16.3), a scene object on every machine: places its platform from
    /// <see cref="RunLift"/> and the server clock, and shuts the landing gates where the platform isn't.
    /// Players ride it through <see cref="MovingPlatform"/>; loot rides on its deck. Nothing here is
    /// networked itself.
    /// </summary>
    public class FreightLift : MonoBehaviour
    {
        [SerializeField] private MovingPlatform platform;
        [Tooltip("Height (m) from the bottom landing to the top one.")]
        [SerializeField, Min(1f)] private float rise = 4f;
        [Tooltip("The deck's walkable size (x, z) in the platform's own space.")]
        [SerializeField] private Vector2 deckSize = new(3.4f, 3.4f);
        [Tooltip("Gate at the bottom landing (solid unless the platform is parked there), then at the top.")]
        [SerializeField] private GameObject bottomGate;
        [SerializeField] private GameObject topGate;
        [SerializeField] private LiftButton[] buttons;
        [Tooltip("The power sector its motor hangs off (a thrown breaker there runs it in a power cut).")]
        [SerializeField] private int sector;

        private Vector3 bottom;

        public static FreightLift Active { get; private set; }
        public float Rise => rise;
        public int Sector => sector;
        public Vector3 Deck => platform != null ? platform.transform.position : transform.position;

        private void Awake() => bottom = platform != null ? platform.transform.position : transform.position;
        private void OnEnable() => Active = this;
        private void OnDisable()
        {
            if (Active == this) Active = null;
        }

        private void Update()
        {
            RunLift run = RunLift.Current;
            float h = 0f;
            bool moving = false;
            if (run != null && run.IsSpawned) h = run.Height(out moving);
            if (platform != null) platform.MoveTo(bottom + Vector3.up * (rise * h));
            if (bottomGate != null) bottomGate.SetActive(moving || h > 0.01f);
            if (topGate != null) topGate.SetActive(moving || h < 0.99f);
        }

        /// <summary>Is a point standing on the deck (anywhere above it, up to head height)?</summary>
        public bool OnDeck(Vector3 world)
        {
            if (platform == null) return false;
            Vector3 local = platform.transform.InverseTransformPoint(world);
            return Mathf.Abs(local.x) <= deckSize.x * 0.5f && Mathf.Abs(local.z) <= deckSize.y * 0.5f && local.y > -0.3f && local.y < 2.4f;
        }

        public float ButtonDistance(Vector3 world)
        {
            float best = float.MaxValue;
            if (buttons == null) return best;
            foreach (LiftButton b in buttons)
                if (b != null) best = Mathf.Min(best, Vector3.Distance(b.transform.position, world));
            return best;
        }

        private void OnDrawGizmos()
        {
            if (!DebugView.Visible && Application.isPlaying) return;
            Gizmos.color = new Color(1f, 0.8f, 0.2f, 0.8f);
            Vector3 c = Deck + Vector3.up * 1.2f;
            Gizmos.DrawWireCube(c, new Vector3(deckSize.x, 2.4f, deckSize.y));
        }

#if UNITY_EDITOR
        public void EditorSetup(MovingPlatform deck, float height, Vector2 size, GameObject lowGate, GameObject highGate, LiftButton[] panel, int powerSector)
        {
            platform = deck;
            rise = height;
            deckSize = size;
            bottomGate = lowGate;
            topGate = highGate;
            buttons = panel;
            sector = powerSector;
        }
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Active = null;
    }
}
