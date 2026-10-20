using UnityEngine;

namespace Abandoned.Core
{
    /// <summary>
    /// Light cookies drawn in code once per session (0.24.12): the flashlight's reflector pattern (hot centre,
    /// a faint ring where the reflector seams, smudges on the lens) and a soft diffuser for spot fixtures. A
    /// plain cone reads as a computer light; these make the beam on a wall and through the fog read as a torch.
    /// Grey in all three channels: URP colours the light with a cookie's RGB.
    /// </summary>
    public static class LightCookies
    {
        private const int Size = 256;
        private static Texture2D flashlight, fixture;

        public static Texture2D Flashlight => flashlight != null ? flashlight : flashlight = Make("FlashlightCookie", FlashlightProfile);
        public static Texture2D Fixture => fixture != null ? fixture : fixture = Make("FixtureCookie", FixtureProfile);

        private static float FlashlightProfile(float r, float angle, float x, float y)
        {
            float hot = Mathf.Exp(-r * r * 9f) * 0.55f;                      // the bulb's hot spot
            float body = 0.5f * (1f - Mathf.SmoothStep(0.55f, 1f, r));       // the reflector's spread
            float ring = 0.16f * Mathf.Exp(-Mathf.Pow((r - 0.62f) / 0.035f, 2f)); // a seam in the reflector
            float smudge = 1f - 0.18f * Mathf.PerlinNoise(x * 7.3f + 11f, y * 7.3f + 5f) * Mathf.SmoothStep(0.15f, 0.7f, r);
            float spokes = 1f - 0.04f * Mathf.Sin(angle * 9f);               // faint facets
            return Mathf.Clamp01((hot + body + ring) * smudge * spokes);
        }

        private static float FixtureProfile(float r, float angle, float x, float y)
        {
            float lobe = 1f - Mathf.SmoothStep(0.2f, 1f, r);
            float grime = 1f - 0.12f * Mathf.PerlinNoise(x * 4.1f + 3f, y * 4.1f + 9f);
            return Mathf.Clamp01((0.35f + 0.65f * lobe * lobe) * (1f - Mathf.SmoothStep(0.85f, 1f, r)) * grime);
        }

        private static Texture2D Make(string name, System.Func<float, float, float, float, float> profile)
        {
            var tex = new Texture2D(Size, Size, TextureFormat.RGBA32, false, true)
            {
                name = name,
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };
            var pixels = new Color32[Size * Size];
            for (int y = 0; y < Size; y++)
            for (int x = 0; x < Size; x++)
            {
                float u = (x + 0.5f) / Size * 2f - 1f, v = (y + 0.5f) / Size * 2f - 1f;
                float r = Mathf.Sqrt(u * u + v * v);
                byte g = (byte)(Mathf.Clamp01(r >= 1f ? 0f : profile(r, Mathf.Atan2(v, u), u, v)) * 255f);
                pixels[y * Size + x] = new Color32(g, g, g, 255);
            }
            tex.SetPixels32(pixels);
            tex.Apply(false, true);
            return tex;
        }
    }
}
