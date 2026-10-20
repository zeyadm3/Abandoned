using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;

namespace Abandoned.EditorTools
{
    /// <summary>School furniture with open supports and deliberate collision envelopes.</summary>
    public static class BurleySchoolFurniture
    {
        public static Transform Table(Transform parent, string name, Vector3 at, float width, float depth,
            float height, Material top, BurleySchoolArt art)
        {
            Transform table = Group(name, parent);
            table.localPosition = at;
            Box("Top", table, new Vector3(0f, height - 0.04f, 0f), new Vector3(width, 0.08f, depth), top);
            foreach (float sx in new[] { -1f, 1f })
            foreach (float sz in new[] { -1f, 1f })
                Box($"Leg_{sx}_{sz}", table, new Vector3(sx * (width * 0.5f - 0.12f),
                    (height - 0.08f) * 0.5f, sz * (depth * 0.5f - 0.12f)),
                    new Vector3(0.07f, height - 0.08f, 0.07f), art.MetalSteel, false);
            Box("ApronFront", table, new Vector3(0f, height - 0.16f, -depth * 0.5f + 0.08f),
                new Vector3(width - 0.14f, 0.16f, 0.04f), art.MetalSteel, false);
            Box("ApronBack", table, new Vector3(0f, height - 0.16f, depth * 0.5f - 0.08f),
                new Vector3(width - 0.14f, 0.16f, 0.04f), art.MetalSteel, false);
            // One simple envelope stops walking through a table without snagging feet on every leg.
            var solid = table.gameObject.AddComponent<BoxCollider>();
            solid.center = new Vector3(0f, height * 0.5f, 0f);
            solid.size = new Vector3(width, height, depth);
            return table;
        }

        public static Transform Chair(Transform parent, string name, Vector3 at, float yaw, BurleySchoolArt art)
        {
            Transform chair = Group(name, parent);
            chair.localPosition = at;
            chair.localRotation = Quaternion.Euler(0f, yaw, 0f);
            Box("Seat", chair, new Vector3(0f, 0.45f, 0f), new Vector3(0.48f, 0.055f, 0.46f), art.OakWood, false);
            Box("Back", chair, new Vector3(0f, 0.8f, -0.2f), new Vector3(0.46f, 0.34f, 0.055f), art.OakWood, false);
            foreach (float sx in new[] { -1f, 1f })
            foreach (float sz in new[] { -1f, 1f })
                Box($"Leg_{sx}_{sz}", chair, new Vector3(sx * 0.18f, 0.23f, sz * 0.16f),
                    new Vector3(0.035f, 0.46f, 0.035f), art.MetalSteel, false);
            foreach (float sx in new[] { -1f, 1f })
                Box($"BackSupport_{sx}", chair, new Vector3(sx * 0.18f, 0.64f, -0.19f),
                    new Vector3(0.035f, 0.75f, 0.035f), art.MetalSteel, false);
            var solid = chair.gameObject.AddComponent<BoxCollider>();
            solid.center = new Vector3(0f, 0.48f, 0f);
            solid.size = new Vector3(0.5f, 0.95f, 0.5f);
            return chair;
        }

        public static void Tube(Transform parent, string name, Vector3 a, Vector3 b,
            float diameter, Material material, bool solid = false)
        {
            Vector3 delta = b - a;
            GameObject tube = Primitive(PrimitiveType.Cylinder, name, parent, (a + b) * 0.5f,
                new Vector3(diameter, delta.magnitude * 0.5f, diameter), material, solid);
            tube.transform.localRotation = Quaternion.FromToRotation(Vector3.up, delta.normalized);
        }

        public static void Ring(Transform parent, string name, Vector3 center, float radius,
            float width, Material material, int segments = 36)
        {
            Transform ring = Group(name, parent);
            ring.localPosition = center;
            for (int i = 0; i < segments; i++)
            {
                float angle = i * Mathf.PI * 2f / segments;
                float next = (i + 1) * Mathf.PI * 2f / segments;
                Vector3 a = new(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
                Vector3 b = new(Mathf.Cos(next) * radius, 0f, Mathf.Sin(next) * radius);
                GameObject line = Box($"Segment_{i:00}", ring, (a + b) * 0.5f,
                    new Vector3(width, 0.015f, Vector3.Distance(a, b) + width * 0.25f), material, false);
                line.transform.localRotation = Quaternion.LookRotation(b - a, Vector3.up);
            }
        }

        public static void Cabinet(Transform parent, string name, Vector3 at, float width,
            float height, float depth, BurleySchoolArt art)
        {
            Transform cabinet = Group(name, parent);
            cabinet.localPosition = at;
            Box("Body", cabinet, new Vector3(0f, height * 0.5f, 0f), new Vector3(width, height, depth), art.OakWood);
            Box("ToeRecess", cabinet, new Vector3(0f, 0.07f, -depth * 0.5f - 0.002f),
                new Vector3(width - 0.08f, 0.14f, 0.015f), art.BrickDark, false);
            for (int i = 0; i < 2; i++)
            {
                float x = (i == 0 ? -1f : 1f) * width * 0.25f;
                Box($"Door_{i}", cabinet, new Vector3(x, height * 0.52f, -depth * 0.5f - 0.02f),
                    new Vector3(width * 0.5f - 0.04f, height - 0.22f, 0.035f), art.OakWood, false);
                Box($"Inset_{i}", cabinet, new Vector3(x, height * 0.52f, -depth * 0.5f - 0.04f),
                    new Vector3(width * 0.5f - 0.15f, height - 0.38f, 0.012f), art.StageWood, false);
                Box($"Pull_{i}", cabinet, new Vector3(x + (i == 0 ? 0.12f : -0.12f),
                    height * 0.6f, -depth * 0.5f - 0.075f), new Vector3(0.025f, 0.13f, 0.035f), art.MetalSteel, false);
            }
        }

        public static void Radiator(Transform parent, Vector3 at, float yaw, BurleySchoolArt art)
        {
            Transform radiator = Group("CastIronRadiator", parent);
            radiator.localPosition = at;
            radiator.localRotation = Quaternion.Euler(0f, yaw, 0f);
            for (int i = 0; i < 12; i++)
            {
                float x = (i - 5.5f) * 0.11f;
                Box($"Fin_{i}", radiator, new Vector3(x, 0.38f, 0f), new Vector3(0.06f, 0.62f, 0.22f), art.MetalSteel, false);
            }
            Tube(radiator, "Return", new Vector3(-0.8f, 0.14f, 0f), new Vector3(0.8f, 0.14f, 0f), 0.06f, art.MetalSteel);
            Tube(radiator, "Supply", new Vector3(-0.8f, 0.64f, 0f), new Vector3(0.8f, 0.64f, 0f), 0.06f, art.MetalSteel);
            Box("Valve", radiator, new Vector3(0.83f, 0.65f, 0f), new Vector3(0.1f, 0.1f, 0.09f), art.GoldAccent, false);
            var solid = radiator.gameObject.AddComponent<BoxCollider>();
            solid.center = new Vector3(0f, 0.38f, 0f);
            solid.size = new Vector3(1.8f, 0.76f, 0.26f);
        }

        public static void Wear(Transform parent, Vector3 at, Vector2 size, int seed, BurleySchoolArt art)
        {
            Transform wear = Group($"WaterDamage_{seed}", parent);
            wear.localPosition = at;
            // Thin floor scraps and a broken edge, with no collision or extra supporting floor surface.
            for (int i = 0; i < 7; i++)
            {
                float x = Mathf.Sin(seed * 0.73f + i * 3.1f) * size.x * 0.42f;
                float z = Mathf.Cos(seed * 0.39f + i * 2.4f) * size.y * 0.42f;
                GameObject scrap = Box($"PlasterFlake_{i}", wear, new Vector3(x, 0.008f + i * 0.0005f, z),
                    new Vector3(0.12f + i * 0.026f, 0.008f, 0.08f + i * 0.017f), art.PlasterWall, false);
                scrap.transform.localRotation = Quaternion.Euler(0f, seed * 13f + i * 41f, 0f);
            }
        }
    }
}
