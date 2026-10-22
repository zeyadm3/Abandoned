using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Abandoned.Core
{
    /// <summary>
    /// In first person your own body, hat and held tool are drawn only into shadow maps: you see your shadow, not the
    /// inside of your head. They sit on a rendering layer of their own so your flashlight, which hangs a hand's width
    /// from your eyes, can leave them out; from there they threw a huge head-and-arms shadow over whatever you lit.
    /// Every other light still sees them (LevelStoreys keeps this layer in its ceiling fixtures' masks).
    /// </summary>
    public static class OwnBodyShadows
    {
        /// <summary>Light Layer 7. Default and Light Layers 1-6 belong to LevelStoreys.</summary>
        public const uint Layer = 0x80;
        private const uint DefaultLayer = 0x01;

        /// <summary>First person: shadows only, on the own-body layer. Otherwise drawn as usual on Default.</summary>
        public static void Set(Renderer renderer, bool firstPerson)
        {
            if (renderer == null) return;
            renderer.shadowCastingMode = firstPerson ? ShadowCastingMode.ShadowsOnly : ShadowCastingMode.On;
            renderer.renderingLayerMask = firstPerson ? Layer : DefaultLayer;
        }

        /// <summary>The light keeps every shadow except the own-body layer's.</summary>
        public static void ExcludeFrom(Light light)
        {
            if (light == null) return;
            UniversalAdditionalLightData data = light.GetUniversalAdditionalLightData();
            data.customShadowLayers = true;
            data.shadowRenderingLayers = AllButOwnBody();
        }

        // A constant, as in LevelStoreys: it converts to a rendering layer mask whichever integer type the engine takes.
        private static RenderingLayerMask AllButOwnBody()
        {
            return 0x7F;
        }
    }
}
