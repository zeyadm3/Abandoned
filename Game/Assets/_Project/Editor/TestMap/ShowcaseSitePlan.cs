using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Where everything on the TestMap site is, in world metres (x east, z north; the warehouse is x 0..20, z 0..16).
    /// The terrain's height, ground paint, trees and grass, the fence and the yard buildings all read this one plan, so
    /// they agree: nothing grows through a slab, a road or a wall.
    ///
    ///   The yard: a fenced lot, x -40..62, z -54..52, flat at <see cref="Ground"/>. The warehouse and the gallery stand
    ///   where they always did; the car park in front is now a cracked asphalt forecourt reaching the south gate.
    ///   East: a gravel lot with shipping containers and a water tower. West: the shell of the old boiler house and its
    ///   chimney. North: rough grass behind the gallery.
    ///   Outside: wooded hills on three sides, rising to a ridge in the north; the gravel road leaves the south gate and
    ///   winds south-west into the trees.
    /// </summary>
    public static class ShowcaseSitePlan
    {
        /// <summary>The yard's ground: just under the warehouse slab and the paving, so nothing shares their plane.</summary>
        public const float Ground = -.03f;

        public static readonly Rect Yard = Rect.MinMaxRect(-40f, -54f, 62f, 52f);
        public static readonly Rect Gate = Rect.MinMaxRect(3f, -55f, 17f, -53f);

        /// <summary>The terrain: 640 m square round the yard. Past it, a ring of far hills (ShowcaseTerrain.Horizon).</summary>
        public static readonly Rect Land = Rect.MinMaxRect(-310f, -330f, 330f, 310f);
        public const float TerrainBase = -14f, TerrainHeight = 130f;

        public static readonly Rect Forecourt = Rect.MinMaxRect(-8f, -50f, 30f, -1.5f);
        public static readonly Rect GravelLot = Rect.MinMaxRect(28f, -48f, 60f, 46f);
        public static readonly Rect BoilerPlot = Rect.MinMaxRect(-33f, -2f, -15f, 13f);
        public static readonly Vector3 Chimney = new(-29f, 0f, 18f);
        public static readonly Vector3 WaterTower = new(49f, 0f, 36f);
        public static readonly Rect Meadow = Rect.MinMaxRect(-38f, 36f, 60f, 50f);

        /// <summary>The gravel road's centre line, from the gate out of the map (world x, z).</summary>
        public static readonly Vector2[] Road =
        {
            new(10f, -48f), new(10f, -62f), new(8f, -78f), new(-2f, -96f), new(-20f, -114f), new(-44f, -132f),
            new(-74f, -150f), new(-108f, -172f), new(-140f, -198f), new(-170f, -232f), new(-196f, -270f), new(-215f, -312f),
            new(-226f, -340f),
        };
        public const float RoadHalfWidth = 3.4f;

        /// <summary>Footprints kept clear of trees, shrubs and grass (buildings, slabs, the forecourt, the yard's lanes).</summary>
        public static readonly Rect[] Clear =
        {
            Rect.MinMaxRect(-4f, -3f, 24f, 20f),       // warehouse, apron, path
            Rect.MinMaxRect(-8f, 18f, 28f, 36f),       // gallery
            Rect.MinMaxRect(-34f, -3f, -14f, 14f),     // boiler house
            Rect.MinMaxRect(-32f, 15f, -26f, 21f),     // chimney
            Rect.MinMaxRect(43f, 30f, 55f, 42f),       // water tower
            Rect.MinMaxRect(30f, -40f, 58f, 22f),      // containers and the lot's lanes
        };

        public static bool InYard(float x, float z, float margin = 0f) =>
            x > Yard.xMin - margin && x < Yard.xMax + margin && z > Yard.yMin - margin && z < Yard.yMax + margin;

        /// <summary>How far (x, z) is outside the fence (0 inside).</summary>
        public static float OutsideYard(float x, float z)
        {
            float dx = Mathf.Max(Yard.xMin - x, 0f, x - Yard.xMax);
            float dz = Mathf.Max(Yard.yMin - z, 0f, z - Yard.yMax);
            return Mathf.Sqrt(dx * dx + dz * dz);
        }

        public static bool Cleared(float x, float z, float margin = 0f)
        {
            foreach (Rect r in Clear)
                if (x > r.xMin - margin && x < r.xMax + margin && z > r.yMin - margin && z < r.yMax + margin) return true;
            return false;
        }

        /// <summary>Distance from (x, z) to the road's centre line, and how far along the road (metres) that nearest point is.</summary>
        public static (float distance, float along) ToRoad(float x, float z)
        {
            var p = new Vector2(x, z);
            float best = float.MaxValue, bestAlong = 0f, along = 0f;
            for (int i = 0; i < Road.Length - 1; i++)
            {
                Vector2 a = Road[i], b = Road[i + 1], ab = b - a;
                float length = ab.magnitude;
                float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / (length * length));
                float d = (a + ab * t - p).sqrMagnitude;
                if (d < best)
                {
                    best = d;
                    bestAlong = along + t * length;
                }
                along += length;
            }
            return (Mathf.Sqrt(best), bestAlong);
        }

        public static float RoadLength
        {
            get
            {
                float length = 0f;
                for (int i = 0; i < Road.Length - 1; i++) length += (Road[i + 1] - Road[i]).magnitude;
                return length;
            }
        }

        /// <summary>The road's centre at <paramref name="along"/> metres from the gate.</summary>
        public static Vector2 RoadAt(float along)
        {
            for (int i = 0; i < Road.Length - 1; i++)
            {
                float length = (Road[i + 1] - Road[i]).magnitude;
                if (along <= length) return Vector2.Lerp(Road[i], Road[i + 1], along / length);
                along -= length;
            }
            return Road[^1];
        }

        public static float Smooth(float edge0, float edge1, float x)
        {
            float t = Mathf.Clamp01((x - edge0) / (edge1 - edge0));
            return t * t * (3f - 2f * t);
        }

        /// <summary>Fractal Perlin noise, about 0..1.</summary>
        public static float Fbm(float x, float z, int octaves)
        {
            float sum = 0f, amplitude = .5f, total = 0f;
            for (int i = 0; i < octaves; i++)
            {
                sum += Mathf.PerlinNoise(x + i * 17.3f, z - i * 9.1f) * amplitude;
                total += amplitude;
                x *= 2.03f;
                z *= 2.03f;
                amplitude *= .5f;
            }
            return sum / total;
        }
    }
}
