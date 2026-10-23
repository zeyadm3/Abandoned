using UnityEngine;
using UnityEngine.Rendering;

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
        [Tooltip("The world's own sun and grade: off while a place with its own is loaded.")]
        [SerializeField] private Light sun;
        [SerializeField] private Volume look;

        private PlaceLook worldLook;

        public static WorldRoot Current { get; private set; }

        /// <summary>World position of the game's origin right now.</summary>
        public Vector3 Offset { get; private set; }

        private void Awake()
        {
            Current = this;
            worldLook = PlaceLook.Capture();
        }

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

        /// <summary>Every machine: a place's own sky, sun and grade while it is loaded (null: the world's again).</summary>
        public void UsePlaceLook(WorldPoi poi)
        {
            bool own = poi != null && poi.look.captured;
            if (own) poi.look.Apply();
            else worldLook?.Apply();
            if (sun != null) sun.enabled = !own;
            if (look != null) look.enabled = !own;
            if (own) RenderSettings.sun = null;
            else if (sun != null) RenderSettings.sun = sun;
        }

        public void EditorSetup(GameObject[] sites, Light worldSun, Volume worldLookVolume)
        {
            standIns = sites;
            sun = worldSun;
            look = worldLookVolume;
        }
    }
}
