using Abandoned.Core;
using Abandoned.Extraction;
using UnityEngine;
using UnityEngine.AI;
using static Abandoned.EditorTools.GreyboxFactory;
using static Abandoned.EditorTools.MallLayout;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Roller shutters (M10.4) on the single-door stores and the front entrance. Added after the NavMesh
    /// bake (they're runtime obstacles, not walls). Stores holding a fallback rope window (GDD 6.4: Cinema,
    /// Electronics upstairs) are never lockable, so every authored way out stays reachable.
    /// </summary>
    public static class MallShutters
    {
        private const float DoorWidth = 1.4f, DoorHeight = 2.3f, WideWidth = 3.5f, WideHeight = 3.4f;

        // (floor, store side tile, outside tile, store zone name)
        private static readonly (int floor, int sx, int sz, int ox, int oz, string zone)[] Stores =
        {
            (0, 3, 7, 4, 7, "Threadbare"),
            (0, 11, 1, 10, 1, "Jewelry"),
            (0, 11, 5, 10, 5, "Security"),
            (1, 6, 2, 6, 3, "ToyArchive"),
            (2, 6, 2, 6, 3, "Quarantine"),
            (2, 11, 2, 10, 2, "Offices"),
        };

        public static int Place(Transform root)
        {
            Transform group = Group("Shutters", root);
            Material slats = GetMaterial("Greybox_Shutter", new Color(0.55f, 0.57f, 0.6f));
            Material housing = GetMaterial("Greybox_ShutterBox", new Color(0.3f, 0.31f, 0.33f));
            int index = 0;
            foreach (var s in Stores)
            {
                var a = new Vector2Int(s.sx, s.sz);
                var b = new Vector2Int(s.ox, s.oz);
                bool east = b.x != a.x;
                Vector3 start = east ? new Vector3(Mathf.Max(a.x, b.x) * Tile, FloorY(s.floor), a.y * Tile)
                                     : new Vector3(a.x * Tile, FloorY(s.floor), Mathf.Max(a.y, b.y) * Tile);
                Vector3 dir = east ? Vector3.forward : Vector3.right;
                Zone zone = System.Array.Find(ZonesByFloor[s.floor], z => z.Name == s.zone);
                var room = new Bounds();
                room.SetMinMax(new Vector3(zone.Area.xMin * Tile, FloorY(s.floor), zone.Area.yMin * Tile),
                    new Vector3(zone.Area.xMax * Tile, FloorY(s.floor) + StoryHeight, zone.Area.yMax * Tile));
                bool wide = DoorBetween(a,b,s.floor)?.Wide ?? false;
                Shutter(group, $"Shutter_{s.zone}", index++, false, start + dir * (Tile / 2f), dir, wide ? WideWidth : DoorWidth, wide ? WideHeight : DoorHeight, room, slats, housing);
            }
            // The front entrance: two wide doors on the street side, locked only on a Sealed job.
            foreach (int column in new[] { 6, 7 })
                Shutter(group, $"Shutter_Entrance_{column}", index++, true, new Vector3(column * Tile + Tile / 2f, 0f, -MallWalls.Thickness / 2f),
                    Vector3.right, WideWidth, WideHeight, default, slats, housing);
            return index;
        }

        private static void Shutter(Transform parent, string name, int index, bool entrance, Vector3 doorBase, Vector3 along,
            float width, float height, Bounds room, Material slats, Material housing)
        {
            var root = new GameObject(name).transform;
            root.SetParent(parent, false);
            root.localPosition = doorBase;
            root.localRotation = Quaternion.FromToRotation(Vector3.right, along);

            // Blocks the doorway while down (players, loot, monster sight).
            var blocker = root.gameObject.AddComponent<BoxCollider>();
            blocker.center = new Vector3(0f, height / 2f, 0f);
            blocker.size = new Vector3(width, height, 0.12f);

            var curtain = new GameObject("Curtain").transform;
            curtain.SetParent(root, false);
            curtain.localPosition = new Vector3(0f, height, 0f);
            CustomMallArt.Place("shutter", curtain, new Vector3(0f, -height, 0f), Quaternion.identity, new Vector3(width / 3.5f, height / 3.4f, 1f));
            Box("Housing", root, new Vector3(0f, height + 0.15f, 0f), new Vector3(width + 0.1f, 0.3f, 0.3f), housing, withCollider: false);
            Box("Padlock", curtain, new Vector3(0f, -height + 0.2f, 0.06f), new Vector3(0.12f, 0.15f, 0.06f), housing, withCollider: false);

            var obstacle = root.gameObject.AddComponent<NavMeshObstacle>();
            obstacle.shape = NavMeshObstacleShape.Box;
            obstacle.center = blocker.center;
            obstacle.size = new Vector3(width, height, 0.4f);
            obstacle.carving = true;
            obstacle.enabled = false;

            root.gameObject.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Metal);
            root.gameObject.AddComponent<RollerShutter>().EditorSetup(index, entrance, curtain, blocker, obstacle, room);
        }
    }
}
