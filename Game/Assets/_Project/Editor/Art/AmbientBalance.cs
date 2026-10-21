using UnityEngine;
using UnityEngine.SceneManagement;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// 0.24.13: the sealed maps' fill light. With scanned surfaces the mall and the iron works read as black between
    /// fixtures, and the walls' new detail was lost there, so their ambient is lifted about 30 % (to the absolute values
    /// below, so running it again never compounds). Still the darkest maps; the power controller scales from these, so
    /// a blackout darkens as before. Applied by AvOverhaul after the map builders.
    /// </summary>
    public static class AmbientBalance
    {
        public static bool Apply(Scene scene)
        {
            string n = scene.name.ToLowerInvariant();
            if (n.Contains("mall"))
                Set(new Color(0.085f, 0.104f, 0.13f), new Color(0.05f, 0.064f, 0.066f), new Color(0.024f, 0.027f, 0.023f));
            else if (n.Contains("factory"))
                Set(new Color(0.065f, 0.061f, 0.055f), new Color(0.042f, 0.036f, 0.031f), new Color(0.018f, 0.014f, 0.012f));
            else return false;
            return true;
        }

        private static void Set(Color sky, Color equator, Color ground)
        {
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = sky;
            RenderSettings.ambientEquatorColor = equator;
            RenderSettings.ambientGroundColor = ground;
        }
    }
}
