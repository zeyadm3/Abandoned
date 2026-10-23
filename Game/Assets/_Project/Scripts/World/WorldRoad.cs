using UnityEngine;

namespace Abandoned.World
{
    /// <summary>A road's centre line in world coordinates (every ten metres, at its bed height), for placing road events.</summary>
    [System.Serializable]
    public class WorldRoad
    {
        public Vector3[] points = new Vector3[0];
    }
}
