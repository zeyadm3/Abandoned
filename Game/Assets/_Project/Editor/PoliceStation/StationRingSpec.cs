using System.Collections.Generic;
using Abandoned.Core;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// What the two outer rings round a map's town are made of (StationRings): the town's ground they start from, the
    /// width of their ring roads, which of the town's streets run on out through them, and the designs each row of
    /// buildings is drawn from (indices into the map's <see cref="StationRingStock"/>).
    /// </summary>
    public sealed class StationRingSpec
    {
        /// <summary>A street out through the rings, (from, to) across its side: world x on the south and north sides,
        /// world z on the west and east. A spur starts at the first ring road (it is not one of the town's streets).</summary>
        public readonly struct Way
        {
            public readonly float From, To;
            public readonly bool Spur;
            public Way(float from, float to, bool spur) { From = from; To = to; Spur = spur; }
        }

        /// <summary>The town's ground before the rings; the rings start at its edge.</summary>
        public Rect Inner;
        public float Road = 10f;
        public int Seed = 1;
        /// <summary>Chance that the next stretch of a row is a vacant lot instead of a building.</summary>
        public float Vacancy = .1f;
        public float LampIntensity = 8f;
        public Material Lot, Yard;
        public SurfaceMaterial LotSurface = SurfaceMaterial.Dirt, YardSurface = SurfaceMaterial.Dirt;

        /// <summary>Designs for the first block's row facing the first ring road (toward the town), its row facing the
        /// second ring road (away from it), the outer edge's row, and the parked cars.</summary>
        public readonly List<int> Front = new(), Back = new(), Edge = new(), Cars = new();

        private readonly List<Way>[] ways = { new List<Way>(), new List<Way>(), new List<Way>(), new List<Way>() };

        public const int South = 0, North = 1, West = 2, East = 3;

        public List<Way> Ways(int side) => ways[side];
        public void Through(int side, float from, float to) => ways[side].Add(new Way(from, to, false));
        public void Spur(int side, float from, float to) => ways[side].Add(new Way(from, to, true));
    }
}
