using System.Linq;
using Abandoned.Structure;
using UnityEngine;
using static Abandoned.EditorTools.MeridianDressingUtility;

namespace Abandoned.EditorTools
{
    public static class MeridianDressingCeilings
    {
        public static void Place(Transform mall, Transform group)
        {
            for (int floor = -1; floor < MallLayout.Floors; floor++)
            for (int x = 0; x < MallLayout.TilesX; x++)
            for (int z = 0; z < MallLayout.TilesZ; z++)
            {
                var cell = new Vector2Int(x, z);
                if (!Occupied(cell, floor)) continue;
                StructuralSection support = floor + 1 < MallLayout.Floors ? Support(mall, cell, floor + 1) : null;
                if (floor + 1 < MallLayout.Floors && support == null) continue;
                if (floor == MallLayout.Floors - 1 && MallLayout.IsAtriumVoid(cell)) continue;
                string key = KeyFor(cell, floor);
                Vector3 at = MallLayout.TileTopCenter(cell, floor + 1) - Vector3.up * (MallLayout.SlabThickness + 0.04f);
                Prop(key, group, at, Quaternion.identity, support);
            }
        }

        public static string KeyFor(Vector2Int cell, int floor)
        {
            if (floor < 0 || cell.x == 10) return "CeilingGrid";
            bool leak = cell.x >= 4 && cell.x <= 9 && cell.y >= 3 && cell.y <= 8;
            bool clearRoute = MallLayout.IsProtected(cell, floor)
                || MallLayout.Doors.Any(d => d.Floor == floor && (d.A == cell || d.B == cell))
                || MallLayout.AllFlights.Any(f => (f.Floor == floor || f.Floor + 1 == floor)
                    && (f.Tiles.Contains(cell) || cell == f.Start - f.Dir || cell == f.Start + f.Dir * 2));
            if (!leak || clearRoute) return "CeilingCoffer";
            return (Hash(cell, floor) % 4) switch
            {
                0 => "CeilingSagging", 1 => "CeilingDamaged", _ => "CeilingCoffer",
            };
        }
    }
}
