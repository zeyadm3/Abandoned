using Abandoned.Structure;
using UnityEngine;
using static Abandoned.EditorTools.MeridianDressingUtility;

namespace Abandoned.EditorTools
{
    /// <summary>Furniture forms useful, recognizable places around the mall's continuous shopping route.</summary>
    public static class MeridianMallCourt
    {
        public static void Build(Transform mall)
        {
            Transform root = Group("MallPublicSpaces", mall);
            FloorProp("Fountain", new Vector3(28, 0, 29), 0);
            FloorProp("Kiosk", new Vector3(28, 0, 17.8f), 0, 180);
            FloorProp("MallDirectory", new Vector3(31.65f, 0, 9.5f), 0, -90);
            foreach (float x in new[] { 22f, 34f })
            {
                FloorProp("ConcourseBench", new Vector3(x, 0, 30), 0, x < 28 ? 90 : -90);
                FloorProp("Planter", new Vector3(x, 0, 32.2f), 0, 0, new Vector3(.68f, .8f, .68f));
                FloorProp("MallBin", new Vector3(x, 0, 28), 0);
            }
            FloorProp("ConcourseBench", new Vector3(16.35f, 4, 29.6f), 1, 90);
            FloorProp("ConcourseBench", new Vector3(39.65f, 4, 29.6f), 1, -90);

            void FloorProp(string key, Vector3 at, int floor, float yaw = 0, Vector3? scale = null)
            {
                var cell = new Vector2Int(Mathf.FloorToInt(at.x / MallLayout.Tile), Mathf.FloorToInt(at.z / MallLayout.Tile));
                StructuralSection support = Support(mall, cell, floor);
                if (support != null) Prop(key, root, at, Quaternion.Euler(0, yaw, 0), support, scale);
            }
        }

        public static void LightColors(Transform mall)
        {
            Transform root = mall.Find("MeridianFixtures");
            if (root == null) return;
            foreach (LightFixture fixture in root.GetComponentsInChildren<LightFixture>(true))
            {
                if (fixture.Lamp == null || !fixture.name.StartsWith("Fixture_")) continue;
                Vector3 at = fixture.transform.position;
                int floor = Mathf.FloorToInt(at.y / MallLayout.StoryHeight);
                var cell = new Vector2Int(Mathf.FloorToInt(at.x / MallLayout.Tile), Mathf.FloorToInt(at.z / MallLayout.Tile));
                fixture.Lamp.color = MallLayout.ZoneAt(cell, floor)?.Tag switch
                {
                    "cinema" => new Color(1, .75f, .51f),
                    "food" => new Color(1, .83f, .61f),
                    "clothing" or "furniture" => new Color(1, .88f, .74f),
                    "stock" => new Color(.73f, .84f, .79f),
                    _ => new Color(.8f, .9f, .94f)
                };
            }
        }
    }
}
