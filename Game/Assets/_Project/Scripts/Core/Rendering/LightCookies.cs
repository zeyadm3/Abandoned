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

        // Both keep most of the light (the flashlight was made brighter on request in 0.13.1): a cookie only multiplies,
        // so the pattern lives in a narrow band near 1 and the spot's own cone does the falloff at the edge.
        private static float FlashlightProfile(float r, float angle, float x, float y)
        {
            float body = Mathf.Lerp(0.86f, 0.62f, Mathf.SmoothStep(0.45f, 1f, r));        // the reflector's spread
            float hot = 0.14f * Mathf.Exp(-r * r * 12f);                                   // the bulb's hot spot
            float ring = 0.08f * Mathf.Exp(-Mathf.Pow((r - 0.62f) / 0.035f, 2f));          // a seam in the reflector
            float smudge = 1f - 0.07f * Mathf.PerlinNoise(x * 7.3f + 11f, y * 7.3f + 5f) * Mathf.SmoothStep(0.15f, 0.7f, r);
            float facets = 1f - 0.02f * Mathf.Sin(angle * 9f);
            return Mathf.Clamp01((body + hot + ring) * smudge * facets);
        }

        private static float FixtureProfile(float r, float angle, float x, float y)
        {
            float lobe = 1f - Mathf.SmoothStep(0.25f, 1f, r);
            float grime = 1f - 0.05f * Mathf.PerlinNoise(x * 4.1f + 3f, y * 4.1f + 9f);
            return Mathf.Clamp01((0.85f + 0.15f * lobe) * grime);
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
