using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Abandoned.Structure
{
    /// <summary>
    /// Stops ceiling lights shining through floors (0.14.0). Light has no idea a slab is in the way unless
    /// it casts shadows, and only a few lights can. So each storey of a level is its own rendering layer:
    /// the level's fixed geometry is tagged with the storey it stands in, and a ceiling fixture lights only
    /// its own storey. Everything that moves (crew, loot, monsters, debris) stays on the default layer,
    /// which every light reaches, and every light that isn't a ceiling fixture (flashlights, the moon,
    /// street lamps, the truck) reaches every layer.
    ///
    /// The rebuild puts this on a level's root and tags its renderers and lights (Editor: MeridianLightLayers).
    /// A level without it is lit exactly as before. Lights made at run time are brought into line by
    /// <see cref="ShadowDirector"/>; anything that makes one should call <see cref="LightEverything"/>.
    /// </summary>
    public class LevelStoreys : MonoBehaviour
    {
        /// <summary>Every rendering layer the project defines (Default and Light Layer 1 to 7).</summary>
        public const uint AllLayers = 0xFF;

        [SerializeField] private float storeyHeight = 4f;
        [SerializeField] private int lowest = -1, highest = 2;
        [Tooltip("Only geometry inside this box is split by storey; the outside is lit by everything.")]
        [SerializeField] private Bounds building;
        [Tooltip("A fixture whose light reaches at least this far (m) is an atrium light and lights every storey.")]
        [SerializeField] private float fullHeightRange = 10f;

        public static LevelStoreys Current { get; private set; }

        private void OnEnable() => Current = this;

        private void OnDisable()
        {
            if (Current == this) Current = null;
        }

        public int StoreyAt(float y) => Mathf.Clamp(Mathf.FloorToInt((y + 0.05f) / storeyHeight), lowest, highest);

        /// <summary>
        /// The layers for a piece of fixed geometry: one per storey its height overlaps (a slab is one
        /// storey's floor and the ceiling of the one below, so it gets both). Never none.
        /// </summary>
        public uint RendererLayers(Bounds world)
        {
            if (!building.Intersects(world)) return AllLayers;
            uint layers = 0;
            for (int s = lowest; s <= highest; s++)
            {
                float from = s * storeyHeight - 0.05f, to = (s + 1) * storeyHeight - 0.05f;
                if (world.max.y > from && world.min.y < to) layers |= 1u << (s - lowest + 1);
            }
            return layers != 0 ? layers : AllLayers;
        }

        /// <summary>A ceiling fixture lights its own storey and whatever moves; any other light lights everything.</summary>
        public void Apply(Light light, bool ceilingFixture)
        {
            if (light == null) return;
            bool ownStorey = ceilingFixture && light.type != LightType.Directional && light.range < fullHeightRange;
            UniversalAdditionalLightData data = light.GetUniversalAdditionalLightData();
            data.renderingLayers = ownStorey ? StoreyAndDefault(StoreyAt(light.transform.position.y) - lowest) : Everything();
        }

        /// <summary>For lights made at run time (a truck lamp, the night-vision illuminator): reach every layer.</summary>
        public static void LightEverything(Light light)
        {
            if (light != null) light.GetUniversalAdditionalLightData().renderingLayers = Everything();
        }

        // Written as constants on purpose: a constant converts to a rendering layer mask whichever integer
        // type the engine's conversion takes.
        private static RenderingLayerMask Everything()
        {
            return 0xFF;
        }

        // Bit 0 is Default (everything that moves); storey n of the level is bit n + 1.
        private static RenderingLayerMask StoreyAndDefault(int index)
        {
            switch (index)
            {
                case 0: return 0x03;
                case 1: return 0x05;
                case 2: return 0x09;
                case 3: return 0x11;
                case 4: return 0x21;
                case 5: return 0x41;
                default: return 0xFF;
            }
        }

#if UNITY_EDITOR
        public void EditorSetup(float height, int lowestStorey, int highestStorey, Bounds buildingBounds)
        {
            storeyHeight = height;
            lowest = lowestStorey;
            highest = highestStorey;
            building = buildingBounds;
        }
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Current = null;
    }
}
