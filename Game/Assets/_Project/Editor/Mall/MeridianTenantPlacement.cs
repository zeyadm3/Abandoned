using System.Collections.Generic;
using System.Linq;
using Abandoned.Structure;
using UnityEngine;
using static Abandoned.EditorTools.MeridianDressingUtility;

namespace Abandoned.EditorTools
{
    /// <summary>Reserves real fixture footprints so room rows can cross tile boundaries without filling haul aisles.</summary>
    public sealed class MeridianTenantPlacement
    {
        private readonly Transform mall;
        private readonly MeridianDressingClearance models;
        private readonly MallRouteClearance routes;
        private readonly List<Bounds> occupied = new();

        public MeridianTenantPlacement(Transform mall)
        {
            this.mall = mall;
            models = new MeridianDressingClearance(mall);
            routes = new MallRouteClearance(mall);
            foreach (SectionProp prop in mall.GetComponentsInChildren<SectionProp>(true))
            {
                if (prop.Section == null || Mathf.Abs(prop.transform.position.y - prop.Section.transform.position.y) > .12f) continue;
                Renderer[] parts = prop.GetComponentsInChildren<Renderer>(true);
                if (parts.Length == 0) continue;
                Bounds bounds = parts[0].bounds;
                foreach (Renderer part in parts) bounds.Encapsulate(part.bounds);
                if (bounds.size.y > .3f) occupied.Add(bounds);
            }
        }

        public GameObject Model(string key, Transform parent, int floor, float x, float z, float yaw = 0,
            Vector3? scale = null)
        {
            Vector3 at = new(x, MallLayout.FloorY(floor), z);
            Quaternion rotation = Quaternion.Euler(0, yaw, 0);
            Vector3 actualScale = scale ?? Vector3.one;
            Bounds bounds = World(models.ModelBounds(key), Matrix4x4.TRS(at, rotation, actualScale));
            if (!Reserve(bounds, floor)) return null;
            var cell = Cell(at);
            // Collision is authored later from the mesh, including the gaps under desks and between chair legs.
            return Prop(key, parent, at, rotation, Support(mall, cell, floor), actualScale, solid: false);
        }

        public Transform Assembly(string name, Transform parent, int floor, float x, float z, Vector3 size,
            float yaw = 0, float bottom = 0)
        {
            Vector3 at = new(x, MallLayout.FloorY(floor), z);
            Quaternion rotation = Quaternion.Euler(0, yaw, 0);
            Bounds bounds = World(new Bounds(Vector3.up * (bottom + size.y * .5f), size), Matrix4x4.TRS(at, rotation, Vector3.one));
            if (!Reserve(bounds, floor)) return null;
            Transform group = Group(name, parent);
            group.localPosition = at;
            group.localRotation = rotation;
            group.gameObject.AddComponent<SectionProp>().EditorSetup(Support(mall, Cell(at), floor));
            return group;
        }

        private bool Reserve(Bounds bounds, int floor)
        {
            Vector2Int centre = Cell(bounds.center);
            MallLayout.Zone? zone = MallLayout.ZoneAt(centre, floor);
            if (!zone.HasValue || Support(mall, centre, floor) == null) return false;
            if (bounds.min.x < .14f || bounds.max.x > MallLayout.TilesX * MallLayout.Tile - .14f ||
                bounds.min.z < .14f || bounds.max.z > MallLayout.TilesZ * MallLayout.Tile - .14f) return false;
            int x0 = Mathf.FloorToInt(bounds.min.x / MallLayout.Tile), x1 = Mathf.FloorToInt(bounds.max.x / MallLayout.Tile);
            int z0 = Mathf.FloorToInt(bounds.min.z / MallLayout.Tile), z1 = Mathf.FloorToInt(bounds.max.z / MallLayout.Tile);
            for (int x = x0; x <= x1; x++)
            for (int z = z0; z <= z1; z++)
            {
                var cell = new Vector2Int(x, z);
                MallLayout.Zone? other = MallLayout.ZoneAt(cell, floor);
                if (!other.HasValue || !MallLayout.SameSpace(zone.Value, other.Value)) return false;
                if (MallLootPoints.IsJackpotTile(cell, floor)) return false;
            }
            if (routes.Conflict(bounds) != null) return false;
            Bounds separated = bounds;
            separated.Expand(.12f);
            if (occupied.Any(b => b.Intersects(separated))) return false;
            occupied.Add(bounds);
            return true;
        }

        private static Vector2Int Cell(Vector3 at) => new(Mathf.FloorToInt(at.x / MallLayout.Tile),
            Mathf.FloorToInt(at.z / MallLayout.Tile));

        private static Bounds World(Bounds source, Matrix4x4 matrix)
        {
            Bounds bounds = new(matrix.MultiplyPoint3x4(source.center), Vector3.zero);
            for (int i = 0; i < 8; i++)
                bounds.Encapsulate(matrix.MultiplyPoint3x4(source.center + Vector3.Scale(source.extents,
                    new Vector3((i & 1) == 0 ? -1 : 1, (i & 2) == 0 ? -1 : 1, (i & 4) == 0 ? -1 : 1))));
            return bounds;
        }
    }
}
