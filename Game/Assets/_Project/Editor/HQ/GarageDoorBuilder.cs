using Abandoned.Company;
using UnityEditor;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The HQ's sectional car garage door (original, code-built): four ribbed steel sections with a row of
    /// grimy windows and a pull handle, side and overhead tracks, a header box and the spring shaft. Saved
    /// as Prefabs/HQ/GarageDoor; HqBuilder places it from HqPlacements.
    /// </summary>
    public static class GarageDoorBuilder
    {
        public const string PrefabPath = "Assets/_Project/Prefabs/HQ/GarageDoor.prefab";
        public const float Width = 8f, Height = 3.4f;
        private const int Sections = 4;
        private const float Thickness = 0.06f, TrackDepth = 3.6f;

        [MenuItem("Tools/Abandoned/HQ/Create Garage Door Prefab")]
        public static GameObject Create()
        {
            Material steel = CustomMallArt.Material("Metal"), rust = CustomMallArt.Material("Rust"), trim = CustomMallArt.Material("Trim");
            Material glass = CustomMallArt.Material("Glass"), rubber = CustomMallArt.Material("Rubber");
            var root = new GameObject("GarageDoor");
            float h = Height / Sections;

            Transform panels = Group("Sections", root.transform);
            var sections = new Transform[Sections];
            for (int i = 0; i < Sections; i++)
            {
                Transform section = Group($"Section_{i}", panels);
                sections[i] = section;
                // The section's own collider: it blocks wherever it is along the track.
                var collider = section.gameObject.AddComponent<BoxCollider>();
                collider.center = new Vector3(0f, h * 0.5f, 0f);
                collider.size = new Vector3(Width, h, Thickness);
                Box("Skin", section, new Vector3(0f, h * 0.5f, 0f), new Vector3(Width, h - 0.012f, Thickness), steel, false);
                // Pressed ribs on both faces, with rust weeping along the lowest ones.
                for (int r = 1; r <= 2; r++)
                    foreach (float face in new[] { 1f, -1f })
                        Box("Rib", section, new Vector3(0f, h * r / 3f, face * (Thickness * 0.5f + 0.01f)), new Vector3(Width - 0.1f, 0.035f, 0.02f), i == 0 ? rust : trim, false);
                if (i == 2)
                    for (int w = 0; w < 6; w++)
                    {
                        float x = -Width * 0.5f + Width * (w + 0.5f) / 6f;
                        Box("Window", section, new Vector3(x, h * 0.5f, 0f), new Vector3(Width / 6f - 0.45f, h * 0.42f, Thickness + 0.012f), glass, false);
                    }
                if (i == 0)
                {
                    Box("BottomSeal", section, new Vector3(0f, 0.02f, 0f), new Vector3(Width, 0.04f, Thickness + 0.03f), rubber, false);
                    foreach (float face in new[] { 1f, -1f })
                        Box("Handle", section, new Vector3(0f, h * 0.45f, face * (Thickness * 0.5f + 0.04f)), new Vector3(0.36f, 0.05f, 0.05f), trim, false);
                }
            }

            // Fixed hardware: vertical tracks, overhead tracks running back inside, the header box and spring shaft.
            Transform hardware = Group("Hardware", root.transform);
            foreach (float side in new[] { -1f, 1f })
            {
                float x = side * (Width * 0.5f + 0.06f);
                Box("SideTrack", hardware, new Vector3(x, Height * 0.5f, -0.06f), new Vector3(0.07f, Height, 0.1f), steel, false);
                Box("OverheadTrack", hardware, new Vector3(x, Height + 0.05f, -TrackDepth * 0.5f), new Vector3(0.07f, 0.1f, TrackDepth), steel, false);
                Box("Hanger", hardware, new Vector3(x, Height + 0.3f, -TrackDepth + 0.1f), new Vector3(0.05f, 0.5f, 0.05f), rust, false);
            }
            Box("Header", hardware, new Vector3(0f, Height + 0.22f, 0.02f), new Vector3(Width + 0.3f, 0.44f, 0.16f), trim, false);
            Primitive(PrimitiveType.Cylinder, "SpringShaft", hardware, new Vector3(0f, Height + 0.32f, -0.22f), new Vector3(0.07f, Width * 0.5f, 0.07f), steel, false)
                .transform.localRotation = Quaternion.Euler(0f, 0f, 90f);

            root.AddComponent<Abandoned.Core.SurfaceTag>().EditorSet(Abandoned.Core.SurfaceMaterial.Metal);
            root.AddComponent<GarageDoor>().EditorSetup(sections, Width, Height, 0f);
            PolishAssets.EnsureFolder(System.IO.Path.GetDirectoryName(PrefabPath).Replace('\\', '/'));
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Object.DestroyImmediate(root);
            return prefab;
        }
    }
}
