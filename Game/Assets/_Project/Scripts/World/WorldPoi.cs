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
        [Tooltip("Where the road meets its ground, in its own coordinates (x, z); zero: the middle of the side facing the HQ.")]
        public Vector2 roadEntry;
        [Tooltip("Where the van parks on arrival, in the scene's own coordinates (y = yaw).")]
        public Vector3 parking;
        [Tooltip("The van loads it within this distance (m) of its ground's edge and unloads it past the second (hysteresis).")]
        public float loadDistance = 250f, unloadDistance = 400f;
        [Tooltip("False until the scene exists in the build (planned locations get a stand-in site only).")]
        public bool built = true;
        [Tooltip("World builder: measure the ground, what to hide and the parking from the saved scene.")]
        public bool measure;
        [Tooltip("The scene's own sky, ambient and fog, captured when the world is built; used while it is loaded.")]
        public PlaceLook look = new();

        /// <summary>The scene's own coordinates of a world point.</summary>
        public Vector3 ToLocal(Vector3 world) => world - worldPosition;
        public Vector3 ToWorld(Vector3 local) => local + worldPosition;

        /// <summary>Its ground in world coordinates (x, z).</summary>
        public Rect WorldGround => new(ground.xMin + worldPosition.x, ground.yMin + worldPosition.z, ground.width, ground.height);

        /// <summary>World distance (m, flat) from a world point to its ground: 0 on it.</summary>
        public float FromGround(Vector3 world)
        {
            Rect r = WorldGround;
            float dx = Mathf.Max(r.xMin - world.x, 0f, world.x - r.xMax), dz = Mathf.Max(r.yMin - world.z, 0f, world.z - r.yMax);
            return Mathf.Sqrt(dx * dx + dz * dz);
        }

        /// <summary>World position of the middle of its ground.</summary>
        public Vector3 Centre => worldPosition + new Vector3(ground.center.x, groundHeight, ground.center.y);
    }
}
