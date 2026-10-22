using UnityEngine;

namespace Abandoned.World
{
    /// <summary>
    /// The parent of everything in the World scene that belongs to the land (terrain, roads, site stand-ins). The game's
    /// origin is always the origin of the place the crew is at (<see cref="WorldState"/>'s anchor): this object sits at
    /// minus that place's world position, so the place's own scene can load exactly where it was built.
    /// </summary>
    public class WorldRoot : MonoBehaviour
    {
        [Tooltip("Per place in the atlas: flat ground filling its site while its scene isn't loaded.")]
        [SerializeField] private GameObject[] standIns = new GameObject[0];

        public static WorldRoot Current { get; private set; }

        /// <summary>World position of the game's origin right now.</summary>
        public Vector3 Offset { get; private set; }

        private void Awake() => Current = this;

        private void OnDestroy()
        {
            if (Current == this) Current = null;
        }

        public Vector3 ToWorld(Vector3 engine) => engine + Offset;
        public Vector3 ToEngine(Vector3 world) => world - Offset;

        /// <summary>Every machine: move the land so <paramref name="offset"/> (a world position) is the game's origin.</summary>
        public void SetOffset(Vector3 offset)
        {
            Offset = offset;
            transform.position = -offset;
            Physics.SyncTransforms();
        }

        public void ShowStandIn(int poi, bool show)
        {
            if (poi >= 0 && poi < standIns.Length && standIns[poi] != null) standIns[poi].SetActive(show);
        }

        public void EditorSetup(GameObject[] sites) => standIns = sites;
    }
}
