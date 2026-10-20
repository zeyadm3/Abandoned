using UnityEditor;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;

namespace Abandoned.EditorTools
{
    /// <summary>School masonry, metalwork and painted curves with dimensions in metres.</summary>
    public static class BurleySchoolArchitecture
    {
        public static void WindowWall(Transform parent, string name, Vector3 foot, float length,
            float height, bool alongZ, BurleySchoolArt art)
        {
            Transform wall = Group(name, parent);
            wall.localPosition = foot;
            wall.localRotation = Quaternion.Euler(0f, alongZ ? -90f : 0f, 0f);
            int bays = Mathf.Max(1, Mathf.RoundToInt(length / 4f));
            float bay = length / bays;
            int stories = height > 4.2f ? 2 : 1;
            for (int i = 0; i <= bays; i++)
            {
                float x = -length * 0.5f + i * bay;
                bool edge = i == 0 || i == bays;
                if (edge) x += i == 0 ? 0.095f : -0.095f;
                Box($"MasonryPier_{i}", wall, new Vector3(x, height * 0.5f, 0f),
                    new Vector3(edge ? 0.19f : 0.38f, height, 0.36f), art.Brick);
            }

            for (int story = 0; story < stories; story++)
            for (int i = 0; i < bays; i++)
            {
                float x = -length * 0.5f + (i + 0.5f) * bay;
                float y = story * 4f;
                float opening = bay - 0.46f;
                Transform window = Group($"Bay_{story}_{i}", wall);
                window.localPosition = new Vector3(x, y, 0f);
                Box("BrickApron", window, new Vector3(0f, 0.52f, 0f), new Vector3(bay, 1.04f, 0.24f), art.Brick);
                Box("LimestoneSill", window, new Vector3(0f, 1.08f, 0f), new Vector3(opening + 0.22f, 0.12f, 0.46f), art.Limestone, false);
                Box("BrickSpandrel", window, new Vector3(0f, 3.6f, 0f), new Vector3(bay, 0.8f, 0.24f), art.Brick);
                Box("Lintel", window, new Vector3(0f, 3.22f, 0f), new Vector3(opening + 0.2f, 0.16f, 0.34f), art.Limestone, false);
                // The pane remains solid even where the player sees through its clear centre.
                Box("WinterGlass", window, new Vector3(0f, 2.15f, 0f), new Vector3(opening, 2f, 0.055f), art.WindowGlass);
                foreach (float side in new[] { -1f, 1f })
                    Box(side < 0f ? "FrameL" : "FrameR", window, new Vector3(side * opening * 0.5f, 2.15f, 0f),
                        new Vector3(0.1f, 2.1f, 0.16f), art.MetalSteel, false);
                Box("MeetingRail", window, new Vector3(0f, 2.15f, 0f), new Vector3(opening, 0.08f, 0.16f), art.MetalSteel, false);
                for (int mullion = 1; mullion < 3; mullion++)
                    Box($"Mullion_{mullion}", window, new Vector3(-opening * 0.5f + opening * mullion / 3f, 2.15f, 0f),
                        new Vector3(0.055f, 2.05f, 0.12f), art.MetalSteel, false);
                Box("SnowOnSill", window, new Vector3(0f, 1.17f, 0f), new Vector3(opening + 0.1f, 0.06f, 0.38f), art.Snow, false);
            }
            Box("ParapetBand", wall, new Vector3(0f, height - 0.1f, 0f), new Vector3(length, 0.2f, 0.28f), art.Brick);
        }

        public static void Clerestory(Transform parent, string name, Vector3 foot, float length,
            float height, bool alongZ, BurleySchoolArt art)
        {
            Transform wall = Group(name, parent);
            wall.localPosition = foot;
            wall.localRotation = Quaternion.Euler(0f, alongZ ? -90f : 0f, 0f);
            Box("LowerMasonry", wall, new Vector3(0f, 2.75f, 0f), new Vector3(length, 5.5f, 0.24f), art.Brick);
            Box("Header", wall, new Vector3(0f, 7.85f, 0f), new Vector3(length, height - 7.5f, 0.24f), art.Brick);
            int bays = Mathf.Max(1, Mathf.RoundToInt(length / 4f));
            float bay = length / bays;
            for (int i = 0; i < bays; i++)
            {
                float x = -length * 0.5f + (i + 0.5f) * bay;
                Box($"HighGlass_{i}", wall, new Vector3(x, 6.5f, 0f), new Vector3(bay - 0.24f, 2f, 0.06f), art.WindowGlass);
                Box($"HighPier_{i}", wall, new Vector3(x - bay * 0.5f, 6.5f, 0f), new Vector3(0.24f, 2f, 0.34f), art.Brick);
                Box($"HighFrame_{i}", wall, new Vector3(x, 6.5f, 0f), new Vector3(0.07f, 2f, 0.18f), art.MetalSteel, false);
                Box($"HighSill_{i}", wall, new Vector3(x, 5.5f, 0f), new Vector3(bay, 0.15f, 0.4f), art.Limestone, false);
            }
        }

        public static void Cornice(Transform parent, string name, Vector3 centre, float length, bool alongZ, BurleySchoolArt art)
        {
            Transform trim = Group(name, parent);
            trim.localPosition = centre;
            trim.localRotation = Quaternion.Euler(0f, alongZ ? 90f : 0f, 0f);
            Box("DentilBed", trim, new Vector3(0f, -0.42f, 0f), new Vector3(length, 0.16f, 0.36f), art.BrickDark, false);
            Box("Fascia", trim, new Vector3(0f, -0.22f, 0f), new Vector3(length, 0.24f, 0.42f), art.Limestone, false);
            Box("Coping", trim, Vector3.zero, new Vector3(length + 0.15f, 0.18f, 0.55f), art.Limestone, false);
            Box("SnowLip", trim, new Vector3(0f, 0.14f, 0f), new Vector3(length + 0.1f, 0.1f, 0.5f), art.Snow, false);
            int count = Mathf.FloorToInt(length / 0.8f);
            for (int i = 0; i < count; i++)
                Box($"Dentil_{i}", trim, new Vector3(-length * 0.5f + 0.4f + i * 0.8f, -0.36f, 0f),
                    new Vector3(0.22f, 0.22f, 0.48f), art.Limestone, false);
        }

        public static void Cylinder(Transform parent, string name, Vector3 centre, float radius,
            float height, Material material, bool solid = false)
        {
            Primitive(PrimitiveType.Cylinder, name, parent, centre, new Vector3(radius * 2f, height * 0.5f, radius * 2f), material, solid);
        }

        public static void Rod(Transform parent, string name, Vector3 start, Vector3 end, float radius, Material material, bool solid = false)
        {
            Vector3 delta = end - start;
            GameObject rod = Primitive(PrimitiveType.Cylinder, name, parent, (start + end) * 0.5f,
                new Vector3(radius * 2f, delta.magnitude * 0.5f, radius * 2f), material, solid);
            rod.transform.localRotation = Quaternion.FromToRotation(Vector3.up, delta.normalized);
        }

        public static void PaintLine(Transform parent, string name, Vector3 start, Vector3 end, float width, Material material)
        {
            Vector3 delta = end - start;
            GameObject line = Box(name, parent, (start + end) * 0.5f, new Vector3(width, 0.012f, delta.magnitude + width * 0.3f), material, false);
            line.transform.localRotation = Quaternion.LookRotation(delta.normalized, Vector3.up);
        }

        public static void Arc(Transform parent, string name, Vector3 centre, float radius, float startDegrees,
            float sweepDegrees, float width, Material material, int segments = 40)
        {
            Transform arc = Group(name, parent);
            for (int i = 0; i < segments; i++)
            {
                float a = (startDegrees + sweepDegrees * i / segments) * Mathf.Deg2Rad;
                float b = (startDegrees + sweepDegrees * (i + 1) / segments) * Mathf.Deg2Rad;
                Vector3 start = centre + new Vector3(Mathf.Sin(a), 0f, Mathf.Cos(a)) * radius;
                Vector3 end = centre + new Vector3(Mathf.Sin(b), 0f, Mathf.Cos(b)) * radius;
                PaintLine(arc, $"Segment_{i:00}", start, end, width, material);
            }
        }

        public static void Pediment(Transform parent, BurleySchoolArt art)
        {
            const string meshPath = BurleySchoolArt.Folder + "/Meshes/EntrancePediment.asset";
            PolishAssets.EnsureFolder(BurleySchoolArt.Folder + "/Meshes");
            Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
            if (mesh == null)
            {
                Vector3[] points = { new(-4.35f, 0f, -0.6f), new(4.35f, 0f, -0.6f), new(0f, 1.55f, -0.6f),
                    new(-4.35f, 0f, 0.6f), new(4.35f, 0f, 0.6f), new(0f, 1.55f, 0.6f) };
                int[] faces = { 0, 2, 1, 3, 4, 5, 0, 1, 4, 0, 4, 3, 1, 2, 5, 1, 5, 4, 2, 0, 3, 2, 3, 5 };
                var vertices = new Vector3[faces.Length];
                var uv = new Vector2[faces.Length];
                var triangles = new int[faces.Length];
                for (int i = 0; i < faces.Length; i++)
                {
                    vertices[i] = points[faces[i]];
                    uv[i] = new Vector2(vertices[i].x, vertices[i].y);
                    triangles[i] = i;
                }
                mesh = new Mesh { name = "BurleyEntrancePediment", vertices = vertices, uv = uv, triangles = triangles };
                mesh.RecalculateNormals();
                mesh.RecalculateBounds();
                AssetDatabase.CreateAsset(mesh, meshPath);
            }
            Transform gable = Group("TriangularPediment", parent);
            gable.gameObject.AddComponent<MeshFilter>().sharedMesh = mesh;
            gable.gameObject.AddComponent<MeshRenderer>().sharedMaterial = art.Limestone;
            Rod(gable, "RakingCorniceL", new Vector3(-4.45f, 0f, -0.65f), new Vector3(0f, 1.6f, -0.65f), 0.12f, art.Limestone);
            Rod(gable, "RakingCorniceR", new Vector3(0f, 1.6f, -0.65f), new Vector3(4.45f, 0f, -0.65f), 0.12f, art.Limestone);
            Rod(gable, "SnowRidgeL", new Vector3(-4.45f, 0.09f, -0.65f), new Vector3(0f, 1.69f, -0.65f), 0.08f, art.Snow);
            Rod(gable, "SnowRidgeR", new Vector3(0f, 1.69f, -0.65f), new Vector3(4.45f, 0.09f, -0.65f), 0.08f, art.Snow);
        }
    }
}
