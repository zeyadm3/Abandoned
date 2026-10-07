using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>Gear-specific silhouettes avoid arbitrary renamed library pictures and missing-cell wrench art.</summary>
    public static class UiGearIconBuilder
    {
        public const string Folder = "Assets/_Project/Art/Generated/GearIcons";
        private const int Size = 96;
        private static readonly Color Ink = new(0.75f, 0.77f, 0.66f);
        private static readonly string[] Ids =
        {
            "flashlight", "radio", "hand_trolley", "medkit", "planks", "noise_maker", "stress_scanner", "crowbar",
            "backpack", "support_jack", "flatbed", "rope_pulley", "bolt_cutters", "motion_detector", "night_vision", "battery", "transport_van"
        };

        [MenuItem("Tools/Abandoned/UI/Create Gear Icons")]
        public static void Create()
        {
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Art/Generated")) AssetDatabase.CreateFolder("Assets/_Project/Art", "Generated");
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets/_Project/Art/Generated", "GearIcons");
            foreach (string id in Ids)
            {
                var texture = new Texture2D(Size, Size, TextureFormat.RGBA32, false);
                var shapes = Shapes(id);
                var pixels = new Color[Size * Size];
                for (int y = 0; y < Size; y++)
                    for (int x = 0; x < Size; x++)
                    {
                        Vector2 at = new((x + 0.5f) / Size, (y + 0.5f) / Size);
                        bool inside = false;
                        foreach (System.Func<Vector2, bool> shape in shapes) if (shape(at)) { inside = true; break; }
                        pixels[y * Size + x] = inside ? Ink : Color.clear;
                    }
                texture.SetPixels(pixels);
                texture.Apply();
                string path = $"{Folder}/{id}.png";
                File.WriteAllBytes(path, texture.EncodeToPNG());
                Object.DestroyImmediate(texture);
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
                var importer = (TextureImporter)AssetImporter.GetAtPath(path);
                importer.textureType = TextureImporterType.Default;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.npotScale = TextureImporterNPOTScale.None;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.filterMode = FilterMode.Bilinear;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }
        }

        private static List<System.Func<Vector2, bool>> Shapes(string id)
        {
            var shapes = new List<System.Func<Vector2, bool>>();
            void Box(float x, float y, float width, float height) => shapes.Add(p => p.x >= x && p.x <= x + width && p.y >= y && p.y <= y + height);
            void Disc(float x, float y, float radius) => shapes.Add(p => Vector2.Distance(p, new Vector2(x, y)) <= radius);
            void Ring(float x, float y, float radius, float thickness) => shapes.Add(p => Mathf.Abs(Vector2.Distance(p, new Vector2(x, y)) - radius) <= thickness);
            void Line(float ax, float ay, float bx, float by, float thickness)
            {
                Vector2 a = new(ax, ay), b = new(bx, by), delta = b - a;
                shapes.Add(p => Vector2.Distance(p, a + delta * Mathf.Clamp01(Vector2.Dot(p - a, delta) / delta.sqrMagnitude)) <= thickness);
            }
            void Frame(float x, float y, float width, float height, float thickness)
            {
                Box(x, y, width, thickness); Box(x, y + height - thickness, width, thickness);
                Box(x, y, thickness, height); Box(x + width - thickness, y, thickness, height);
            }
            switch (id)
            {
                case "transport_van":
                    Frame(0.14f, 0.32f, 0.72f, 0.35f, 0.06f); Box(0.16f, 0.34f, 0.4f, 0.29f);
                    Disc(0.29f, 0.26f, 0.1f); Disc(0.72f, 0.26f, 0.1f); Line(0.57f, 0.37f, 0.57f, 0.64f, 0.035f); break;
                case "flashlight":
                    Box(0.35f, 0.15f, 0.3f, 0.43f); Box(0.28f, 0.57f, 0.44f, 0.12f);
                    Line(0.29f, 0.74f, 0.2f, 0.87f, 0.015f); Line(0.5f, 0.74f, 0.5f, 0.9f, 0.015f); Line(0.71f, 0.74f, 0.8f, 0.87f, 0.015f); break;
                case "radio":
                    Frame(0.27f, 0.14f, 0.46f, 0.56f, 0.055f); Box(0.33f, 0.43f, 0.32f, 0.09f);
                    Box(0.35f, 0.27f, 0.12f, 0.08f); Line(0.62f, 0.7f, 0.65f, 0.88f, 0.027f); break;
                case "medkit":
                    Frame(0.16f, 0.22f, 0.68f, 0.49f, 0.065f); Frame(0.35f, 0.7f, 0.3f, 0.13f, 0.035f);
                    Box(0.43f, 0.33f, 0.14f, 0.27f); Box(0.36f, 0.4f, 0.28f, 0.13f); break;
                case "battery":
                    Frame(0.33f, 0.15f, 0.34f, 0.64f, 0.055f); Box(0.43f, 0.79f, 0.14f, 0.07f);
                    Box(0.4f, 0.31f, 0.2f, 0.24f); break;
                case "planks":
                    Box(0.17f, 0.22f, 0.66f, 0.12f); Box(0.2f, 0.43f, 0.6f, 0.12f); Box(0.17f, 0.64f, 0.66f, 0.12f);
                    Line(0.3f, 0.15f, 0.3f, 0.83f, 0.018f); Line(0.7f, 0.15f, 0.7f, 0.83f, 0.018f); break;
                case "crowbar":
                    Line(0.3f, 0.15f, 0.65f, 0.75f, 0.045f); Line(0.65f, 0.75f, 0.52f, 0.85f, 0.045f);
                    Line(0.52f, 0.85f, 0.43f, 0.75f, 0.035f); break;
                case "bolt_cutters":
                    Line(0.25f, 0.15f, 0.54f, 0.68f, 0.045f); Line(0.75f, 0.15f, 0.46f, 0.68f, 0.045f);
                    Line(0.46f, 0.68f, 0.33f, 0.82f, 0.05f); Line(0.54f, 0.68f, 0.67f, 0.82f, 0.05f); Disc(0.5f, 0.58f, 0.075f); break;
                case "hand_trolley":
                    Line(0.37f, 0.25f, 0.37f, 0.82f, 0.04f); Line(0.37f, 0.82f, 0.65f, 0.82f, 0.04f);
                    Box(0.35f, 0.2f, 0.42f, 0.08f); Disc(0.34f, 0.2f, 0.1f); Disc(0.68f, 0.2f, 0.07f); Frame(0.39f, 0.36f, 0.31f, 0.31f, 0.045f); break;
                case "flatbed":
                    Box(0.16f, 0.3f, 0.68f, 0.08f); Disc(0.28f, 0.21f, 0.09f); Disc(0.72f, 0.21f, 0.09f);
                    Line(0.2f, 0.38f, 0.2f, 0.79f, 0.035f); Line(0.2f, 0.79f, 0.41f, 0.79f, 0.035f); break;
                case "backpack":
                    Frame(0.25f, 0.18f, 0.5f, 0.6f, 0.065f); Frame(0.35f, 0.25f, 0.3f, 0.22f, 0.035f);
                    Line(0.2f, 0.23f, 0.2f, 0.68f, 0.025f); Line(0.8f, 0.23f, 0.8f, 0.68f, 0.025f); Ring(0.5f, 0.77f, 0.11f, 0.03f); break;
                case "support_jack":
                    Box(0.2f, 0.16f, 0.6f, 0.08f); Box(0.42f, 0.24f, 0.16f, 0.5f); Box(0.25f, 0.74f, 0.5f, 0.08f);
                    Line(0.5f, 0.44f, 0.73f, 0.54f, 0.035f); break;
                case "rope_pulley":
                    Ring(0.5f, 0.7f, 0.15f, 0.04f); Disc(0.5f, 0.7f, 0.035f);
                    Line(0.35f, 0.7f, 0.35f, 0.19f, 0.025f); Line(0.65f, 0.7f, 0.65f, 0.25f, 0.025f);
                    Ring(0.65f, 0.19f, 0.07f, 0.023f); break;
                case "night_vision":
                    Ring(0.32f, 0.44f, 0.17f, 0.06f); Ring(0.68f, 0.44f, 0.17f, 0.06f);
                    Box(0.42f, 0.48f, 0.16f, 0.11f); Line(0.2f, 0.62f, 0.8f, 0.62f, 0.025f); break;
                default:
                    Frame(0.27f, 0.15f, 0.46f, 0.66f, 0.055f); Frame(0.34f, 0.42f, 0.32f, 0.29f, 0.027f);
                    Disc(0.39f, 0.28f, 0.045f); Disc(0.61f, 0.28f, 0.045f);
                    if (id == "motion_detector") { Ring(0.5f, 0.56f, 0.09f, 0.015f); Line(0.5f, 0.56f, 0.59f, 0.64f, 0.018f); }
                    else if (id == "noise_maker") { Line(0.4f, 0.48f, 0.46f, 0.61f, 0.016f); Line(0.46f, 0.61f, 0.53f, 0.49f, 0.016f); Line(0.53f, 0.49f, 0.61f, 0.63f, 0.016f); }
                    else { Line(0.36f, 0.48f, 0.43f, 0.55f, 0.016f); Line(0.43f, 0.55f, 0.55f, 0.49f, 0.016f); Line(0.55f, 0.49f, 0.64f, 0.63f, 0.016f); }
                    break;
            }
            return shapes;
        }
    }
}
