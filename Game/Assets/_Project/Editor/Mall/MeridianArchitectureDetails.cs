using System.Linq;
using UnityEditor;
using UnityEngine;
using static Abandoned.EditorTools.MeridianDressingUtility;

namespace Abandoned.EditorTools
{
    /// <summary>Kit details fit the authored shell; the shell remains the source of all solid collision.</summary>
    public static class MeridianArchitectureDetails
    {
        public static void Place(Transform mall)
        {
            Transform group = Group("MeridianArchitectureDetails", mall);
            Shopfronts(mall, Group("Storefronts", group));
            MeridianArchitectureFacade.Place(mall, Group("PermanentFacade", group));
            Balustrades(mall);
            Skylight(mall);
        }

        private static void Shopfronts(Transform mall, Transform group)
        {
            Transform walls = mall.Find("Walls");
            if (walls == null) return;
            foreach (Transform panel in walls.GetComponentsInChildren<Transform>().Where(t => t.name.StartsWith("Partition_")))
            {
                if (!ReadPanel(panel.name, out int floor, out Vector2Int cell, out char side)) continue;
                Vector2Int direction = side == 'E' ? Vector2Int.right : Vector2Int.up;
                MallLayout.Zone? a = MallLayout.ZoneAt(cell, floor), b = MallLayout.ZoneAt(cell + direction, floor);
                if (!a.HasValue || !b.HasValue) continue;
                bool storeA = a.Value.Kind == MallLayout.Kind.Store, storeB = b.Value.Kind == MallLayout.Kind.Store;
                if (storeA == storeB || a.Value.Tag == "stock" || b.Value.Tag == "stock") continue;
                Vector3 front = new(direction.x, 0f, direction.y);
                if (storeB) front = -front;
                MallLayout.Door? door = MallLayout.DoorBetween(cell, cell + direction, floor);
                if (door.HasValue)
                {
                    // The kit entry's stone piers leave only 3.37 m; our haul doors must retain their full 4 m.
                    Header(group, panel.position, front, door.Value.Wide ? MallWalls.WideWidth : 1.8f,
                        door.Value.Wide ? MallWalls.WideHeight : 2.5f, MallLayout.StoryHeight - MallLayout.SlabThickness);
                    continue;
                }
                string key = Hash(cell, floor) % 5 == 0 ? "StorefrontBoarded" : "StorefrontGlazed";
                GameObject model = Prop(key, group, panel.position - front * 0.025f, Quaternion.LookRotation(front));
                Permanent(model);
                // The intact wall is the back of an emptied display bay, visible through the dirty glass.
                MeridianArchitectureTrim.WallBands(group, panel.position, front, true);
            }
        }

        public static void Header(Transform group, Vector3 basePosition, Vector3 front, float width, float openingHeight, float wallHeight)
        {
            float height = wallHeight - openingHeight;
            if (height <= 0.01f) return;
            Quaternion rotation = Quaternion.LookRotation(front);
            MeridianArchitectureTrim.Box("StoreBulkhead", group, basePosition + Vector3.up * (openingHeight + height * 0.5f),
                rotation, new Vector3(width, height, 0.30f), MeridianArt.Material("StorefrontTrim"));
            MeridianArchitectureTrim.Box("BrassHeaderReveal", group,
                basePosition + Vector3.up * (openingHeight + 0.035f) + front * 0.17f,
                rotation, new Vector3(width, 0.035f, 0.025f), MeridianArchitectureTrim.Brass());
        }

        private static void Balustrades(Transform mall)
        {
            Transform walls = mall.Find("Walls");
            if (walls == null) return;
            foreach (Transform rail in walls.GetComponentsInChildren<Transform>().Where(t => t.name.StartsWith("Rail_")).ToArray())
            {
                if (rail.Find("GlassRail4m") != null || rail.GetComponent<BoxCollider>() == null) continue;
                bool alongZ = rail.localScale.z > rail.localScale.x;
                float length = Mathf.Max(rail.localScale.x, rail.localScale.z), depth = Mathf.Min(rail.localScale.x, rail.localScale.z);
                if (length < 0.1f || depth < 0.01f || rail.localScale.y < 0.1f) continue;
                Vector3 scale = new(1f / length, 1f / rail.localScale.y, 1f / depth);
                Prop("GlassRail4m", rail, Vector3.down * 0.5f, Quaternion.Euler(0f, alongZ ? 90f : 0f, 0f), null, scale);
                // Removing the cube renderer prevents SectionProp restoration from showing an opaque rail again.
                if (rail.TryGetComponent(out MeshRenderer renderer)) Object.DestroyImmediate(renderer);
                if (rail.TryGetComponent(out MeshFilter filter)) Object.DestroyImmediate(filter);
            }
        }

        private static void Skylight(Transform mall)
        {
            Transform sky = mall.Find("Roof/Skylight");
            if (sky == null) return;
            foreach (Transform child in sky.Cast<Transform>().ToArray()) Object.DestroyImmediate(child.gameObject);
            Vector3 at = new(MallLayout.Atrium.center.x * MallLayout.Tile, MallLayout.Floors * MallLayout.StoryHeight + 0.18f,
                MallLayout.Atrium.center.y * MallLayout.Tile);
            GameObject model = Prop("SkylightFrame", sky, at, Quaternion.identity);
            Permanent(model);
        }

        public static bool ReadPanel(string name, out int floor, out Vector2Int cell, out char side)
        {
            floor = 0; cell = default; side = default;
            string[] parts = name.Split('_');
            if (parts.Length != 5 || !int.TryParse(parts[1], out floor) || !int.TryParse(parts[2], out int x)
                || !int.TryParse(parts[3], out int z) || parts[4].Length != 1) return false;
            cell = new Vector2Int(x, z);
            side = parts[4][0];
            return true;
        }

        public static void Permanent(GameObject model)
        {
            if (model == null) return;
            foreach (Renderer renderer in model.GetComponentsInChildren<Renderer>(true))
                GameObjectUtility.SetStaticEditorFlags(renderer.gameObject, StaticEditorFlags.BatchingStatic | StaticEditorFlags.OccludeeStatic);
        }
    }
}
