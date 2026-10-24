using System.Collections.Generic;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The rough draft's buildings (Docs/CITY_PLAN.md step 4): the land between the streets, filled with plain
    /// extruded boxes whose height comes from the district they stand in. No facades, no windows, no interiors —
    /// this is the massing you drive past to judge whether the city reads right, and nothing more.
    ///
    /// Blocks are worked out from the same grid maths <see cref="CityRoadNetwork"/> lays its streets on, so a block
    /// is exactly the square the streets leave behind. Everything in a cell merges into one mesh per district kind,
    /// and the boxes of one district share a single rotated holder for their colliders, so a whole cell costs a
    /// handful of objects instead of hundreds.
    /// </summary>
    public static class CityMassing
    {
        /// <summary>Pavement between a street's edge and the building line.</summary>
        private const float Pavement = 3f;
        /// <summary>Gap left between two buildings on the same block.</summary>
        private const float LotGap = 4f;

        /// <summary>One building, before it is turned into triangles.</summary>
        private readonly struct Lot
        {
            public readonly Vector2 Centre, Size;
            public readonly float Height, Yaw;
            public readonly CityKind Kind;

            public Lot(Vector2 centre, Vector2 size, float height, float yaw, CityKind kind)
            {
                Centre = centre; Size = size; Height = height; Yaw = yaw; Kind = kind;
            }
        }

        /// <summary>Builds every block under <paramref name="parent"/>. Returns the cells and buildings made.</summary>
        public static (int cells, int buildings) Build(Transform parent, Material[] byKind)
        {
            var byCell = new Dictionary<Vector2Int, List<Lot>>();
            int buildings = 0;
            foreach (CityPlan.District d in CityPlan.Districts)
                foreach (Lot lot in Blocks(d))
                {
                    Vector2Int cell = CityPlan.CellOf(lot.Centre);
                    if (!byCell.TryGetValue(cell, out List<Lot> list)) byCell[cell] = list = new List<Lot>();
                    list.Add(lot);
                    buildings++;
                }

            Transform group = GreyboxFactory.Group("Blocks", parent);
            foreach (KeyValuePair<Vector2Int, List<Lot>> cell in byCell) Cell(group, cell.Key, cell.Value, byKind);
            return (byCell.Count, buildings);
        }

        // Every buildable lot in a district: its grid walked square by square, each square checked against the
        // water, the parks, the named roads and the ground a salvage location needs, then cut into lots.
        private static IEnumerable<Lot> Blocks(CityPlan.District d)
        {
            if (d.Kind == CityKind.Park) yield break;
            Rect b = d.Bounds();
            float reach = Mathf.Max(b.width, b.height) * .75f;
            Vector2 centre = b.center;
            float rad = d.GridAngle * Mathf.Deg2Rad;
            Vector2 along = new(Mathf.Cos(rad), Mathf.Sin(rad)), across = new(-along.y, along.x);
            float inset = CityPlan.ArterialHalfWidth + Pavement;
            float side = CityPlan.Block - inset * 2f;
            if (side < 20f) yield break;

            for (float u = -reach; u <= reach; u += CityPlan.Block)
                for (float v = -reach; v <= reach; v += CityPlan.Block)
                {
                    Vector2 at = centre + along * (u + CityPlan.Block * .5f) + across * (v + CityPlan.Block * .5f);
                    if (!Buildable(d, at)) continue;
                    foreach (Lot lot in Lots(d, at, side, along, across)) yield return lot;
                }
        }

        private static bool Buildable(CityPlan.District d, Vector2 at)
        {
            if (!d.Contains(at) || CityPlan.AtSea(at)) return false;
            if (CityPlan.FromRiver(at) < CityPlan.RiverWidth * .5f + CityPlan.Block * .5f) return false;
            foreach (Rect park in CityPlan.Parks)
                if (park.Contains(at)) return false;
            // Each map's real ground, measured from its own scene: the Mall needs 640 m and the Hospital 88, so
            // one figure for all ten would either pave over the Mall or leave a hole round the Hospital.
            foreach (CityPlan.Site s in CityPlan.Sites)
            {
                Rect clear = CitySites.GroundOf(s);
                if (at.x > clear.xMin - CityPlan.Block * .5f && at.x < clear.xMax + CityPlan.Block * .5f &&
                    at.y > clear.yMin - CityPlan.Block * .5f && at.y < clear.yMax + CityPlan.Block * .5f) return false;
            }
            if (Vector2.Distance(at, CityPlan.CircusCentre) < CityPlan.CircusRing + CityPlan.Block * .6f) return false;
            foreach (CityPlan.Arterial a in CityPlan.Named)
                if (Near(a.Points, at, a.HalfWidth + CityPlan.Block * .5f)) return false;
            return true;
        }

        // How a block is divided, and how tall what stands on it is. The kind is the whole of the draft's variety.
        private static IEnumerable<Lot> Lots(CityPlan.District d, Vector2 at, float side, Vector2 along, Vector2 across)
        {
            (int nx, int nz, float extra) = d.Kind switch
            {
                CityKind.Downtown => (1, 1, 10f),
                CityKind.Civic => (1, 1, 14f),
                CityKind.Industrial => (2, 1, 6f),
                CityKind.Retail => (2, 1, 2f),
                CityKind.Waterfront => (2, 1, 4f),
                CityKind.Suburb => (3, 3, 2f),
                _ => (2, 2, 2f),
            };
            float usable = side - extra * 2f;
            float lotX = (usable - LotGap * (nx - 1)) / nx, lotZ = (usable - LotGap * (nz - 1)) / nz;
            if (lotX < 6f || lotZ < 6f) yield break;
            int seed = Mathf.RoundToInt(at.x * .1f) * 73856093 ^ Mathf.RoundToInt(at.y * .1f) * 19349663;
            for (int ix = 0; ix < nx; ix++)
                for (int iz = 0; iz < nz; iz++)
                {
                    float ox = -usable * .5f + lotX * .5f + ix * (lotX + LotGap);
                    float oz = -usable * .5f + lotZ * .5f + iz * (lotZ + LotGap);
                    float roll = Hash(seed, ix * 31 + iz);
                    // Most of a street is of a piece; the odd one stands well above it, which is what reads as a city.
                    float t = roll < .82f ? roll * .5f : .55f + (roll - .82f) * 2.5f;
                    float height = Mathf.Lerp(d.Storeys.x, d.Storeys.y, Mathf.Clamp01(t));
                    var shrink = new Vector2(lotX * (.88f + Hash(seed, ix + 7) * .1f),
                                             lotZ * (.88f + Hash(seed, iz + 11) * .1f));
                    yield return new Lot(at + along * ox + across * oz, shrink, height, d.GridAngle, d.Kind);
                }
        }

        // One cell: a mesh per district kind, and one rotated holder per angle carrying all that cell's colliders.
        private static void Cell(Transform group, Vector2Int at, List<Lot> lots, Material[] byKind)
        {
            Transform cell = GreyboxFactory.Group($"Blocks_{at.x}_{at.y}", group);
            CityCells.Register(at, cell);
            var pieces = new Dictionary<CityKind, CityMesh.Piece>();
            var holders = new Dictionary<float, Transform>();
            foreach (Lot lot in lots)
            {
                if (!pieces.TryGetValue(lot.Kind, out CityMesh.Piece piece))
                    pieces[lot.Kind] = piece = new CityMesh.Piece();
                piece.Box(lot.Centre, lot.Size, lot.Height, lot.Yaw);

                if (!holders.TryGetValue(lot.Yaw, out Transform holder))
                {
                    holder = GreyboxFactory.Group($"Collision_{lot.Yaw:0}", cell);
                    holder.rotation = Quaternion.Euler(0f, -lot.Yaw, 0f);
                    holders[lot.Yaw] = holder;
                }
                BoxCollider box = holder.gameObject.AddComponent<BoxCollider>();
                box.center = holder.InverseTransformPoint(
                    new Vector3(lot.Centre.x, CityPlan.GroundY + lot.Height * .5f, lot.Centre.y));
                box.size = new Vector3(lot.Size.x, lot.Height, lot.Size.y);
            }
            foreach (KeyValuePair<CityKind, CityMesh.Piece> piece in pieces)
            {
                Mesh mesh = piece.Value.Save($"Blocks_{piece.Key}_{at.x}_{at.y}");
                if (mesh == null) continue;
                GameObject go = CityMesh.Render(piece.Key.ToString(), cell, mesh, byKind[(int)piece.Key]);
                go.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            }
        }

        private static bool Near(Vector2[] line, Vector2 p, float within)
        {
            for (int i = 0; i < line.Length - 1; i++)
            {
                Vector2 a = line[i], ab = line[i + 1] - a;
                float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / Mathf.Max(ab.sqrMagnitude, .001f));
                if (Vector2.Distance(p, a + ab * t) < within) return true;
            }
            return false;
        }

        /// <summary>Repeatable noise, so a rebuild puts the same city back.</summary>
        private static float Hash(int seed, int salt)
        {
            uint h = (uint)(seed ^ (salt + 0x9E3779B9));
            h ^= h >> 16;
            h *= 0x7FEB352D;
            h ^= h >> 15;
            h *= 0x846CA68B;
            h ^= h >> 16;
            return (h & 0xFFFFFF) / (float)0x1000000;
        }
    }
}
