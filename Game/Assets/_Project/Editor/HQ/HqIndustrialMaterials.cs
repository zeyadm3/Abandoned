using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Abandoned.EditorTools
{
    /// <summary>Original HQ surfaces keep the company architecture independent of the mall kit.</summary>
    public static class HqIndustrialMaterials
    {
        public const string Folder = "Assets/_Project/Art/Polish/HQArchitecture";
        public static Material Concrete => Surface("Concrete", new Color(.42f, .43f, .40f), 0);
        public static Material Brick => Surface("Brick", new Color(.37f, .29f, .22f), 1);
        public static Material Cladding => Surface("Cladding", new Color(.28f, .34f, .33f), 2);
        public static Material Steel => Surface("Steel", new Color(.16f, .21f, .22f), 2);
        public static Material Vinyl => Surface("Vinyl", new Color(.38f, .41f, .38f), 0);
        public static Material Asphalt => Surface("Asphalt", new Color(.16f, .18f, .18f), 0);
        public static Material Yellow => PolishAssets.Material("HQ_OperationsYellow", new Color(.73f, .56f, .17f));
        public static Material White => PolishAssets.Material("HQ_OperationsWhite", new Color(.72f, .74f, .68f));

        public static GameObject Tile(GameObject surface, Vector2 repeat)
        {
            var properties = new MaterialPropertyBlock();
            properties.SetVector("_BaseMap_ST", new Vector4(repeat.x, repeat.y, 0f, 0f));
            surface.GetComponent<Renderer>().SetPropertyBlock(properties);
            return surface;
        }

        public static Material Glass
        {
            get
            {
                Material glass = PolishAssets.Material("HQ_OfficeGlazing", new Color(.38f, .53f, .52f, .20f), .18f);
                glass.SetFloat("_Surface", 1f);
                glass.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
                glass.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
                glass.SetFloat("_ZWrite", 0f);
                glass.SetFloat("_Cull", (float)CullMode.Off);
                glass.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                glass.SetOverrideTag("RenderType", "Transparent");
                glass.renderQueue = (int)RenderQueue.Transparent;
                return glass;
            }
        }

        private static Material Surface(string name, Color color, int pattern)
        {
            PolishAssets.EnsureFolder(Folder);
            string texturePath = Folder + "/" + name + ".asset";
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
            if (texture == null)
            {
                const int size = 256;
                texture = new Texture2D(size, size, TextureFormat.RGBA32, true)
                {
                    name = "HQ_" + name, wrapMode = TextureWrapMode.Repeat, filterMode = FilterMode.Bilinear
                };
                Color[] pixels = new Color[size * size];
                for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float patch = Mathf.PerlinNoise(x * .034f + 13f, y * .034f + 5f);
                    float tone = Mathf.Lerp(.84f, 1.10f, patch);
                    Color value = color * tone;
                    if (pattern == 1)
                    {
                        int row = y / 24;
                        int joint = (x + row % 2 * 32) % 64;
                        if (y % 24 < 2 || joint < 2) value = new Color(.37f, .37f, .32f);
                        else value *= .89f + Mathf.PerlinNoise(x / 64 + row % 2 * .5f, row + 11f) * .17f;
                    }
                    if (pattern == 2)
                    {
                        int rib = x % 16;
                        value *= rib < 3 ? .70f : rib < 5 ? 1.16f : 1f;
                    }
                    value.a = 1f;
                    pixels[y * size + x] = value;
                }
                texture.SetPixels(pixels);
                texture.Apply(true, false);
                AssetDatabase.CreateAsset(texture, texturePath);
            }
            return PolishAssets.Material("HQ_Architecture_" + name, Color.white, name is "Steel" or "Cladding" ? .62f : .93f, texture);
        }
    }
}
