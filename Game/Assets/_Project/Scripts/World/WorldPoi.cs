using UnityEngine;

namespace Abandoned.World
{
    /// <summary>
    /// One place in the open world: a scene (a salvage location or the HQ) and where its own origin sits in the world.
    /// A scene is never moved: the world is moved under it instead (<see cref="WorldState"/>), so every map keeps its
    /// static batching, NavMesh and absolute coordinates.
    /// </summary>
    [System.Serializable]
    public class WorldPoi
    {
        [Tooltip("Short id, also the scene's name in the build.")]
        public string scene;
        [Tooltip("Where the scene's own origin (0,0,0) sits in world coordinates.")]
        public Vector3 worldPosition;
        [Tooltip("The scene's own ground, in its own coordinates (x, z): the world terrain is cut away here.")]
        public Rect ground;
        [Tooltip("Height of the scene's ground at that edge, in its own coordinates.")]
        public float groundHeight;
        [Tooltip("Objects (paths from a scene root, e.g. HQ/IndustrialEstateSurroundings/IndustrialEstateLandscape/EstateScenery) switched off in the world: far horizons and skies the world replaces.")]
        public string[] hideInWorld = new string[0];
        [Tooltip("Where the van parks on arrival, in the scene's own coordinates (y = yaw).")]
        public Vector3 parking;
        public float loadDistance = 700f, unloadDistance = 900f;
        [Tooltip("False until the scene exists in the build (planned locations get a stand-in site only).")]
        public bool built = true;

        /// <summary>The scene's own coordinates of a world point.</summary>
        public Vector3 ToLocal(Vector3 world) => world - worldPosition;
        public Vector3 ToWorld(Vector3 local) => local + worldPosition;

        /// <summary>World position of the middle of its ground.</summary>
        public Vector3 Centre => worldPosition + new Vector3(ground.center.x, groundHeight, ground.center.y);
    }
}
