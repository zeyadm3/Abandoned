using Abandoned.Structure;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Rebuild step (0.14.0): splits the mall's light by storey so a ceiling fixture no longer lights the
    /// floor below through its slab. Every fixed renderer is tagged with the storey (or storeys) it stands
    /// in, every ceiling fixture is limited to its own storey, and every other light in the scene is set to
    /// reach all of them. The rules live in <see cref="LevelStoreys"/>, which this adds to the mall's root;
    /// lights made at run time are handled by ShadowDirector.
    /// </summary>
    public static class MeridianLightLayers
    {
        public static void Apply(Transform mall)
        {
            var storeys = mall.gameObject.AddComponent<LevelStoreys>();
            storeys.EditorSetup(MallLayout.StoryHeight, -1, MallLayout.Floors - 1, MallLayout.BuildingBounds);

            int tagged = 0, split = 0;
            foreach (Renderer renderer in mall.GetComponentsInChildren<Renderer>(true))
            {
                // Dust motes drift between storeys; they stay on the default layer every light reaches.
                if (renderer is ParticleSystemRenderer) continue;
                uint layers = storeys.RendererLayers(WorldBounds(renderer));
                renderer.renderingLayerMask = layers;
                tagged++;
                if (layers != LevelStoreys.AllLayers) split++;
            }

            int own = 0, everywhere = 0;
            foreach (Light light in Object.FindObjectsByType<Light>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                var fixture = light.GetComponentInParent<LightFixture>(true);
                bool ceiling = fixture != null && fixture.Lamp == light;
                storeys.Apply(light, ceiling);
                if (ceiling) own++;
                else everywhere++;
            }
            Debug.Log($"[MeridianLighting] Storey light layers: {tagged} renderers tagged ({split} inside the building, by storey), " +
                      $"{own} ceiling fixtures on their own storey (atrium pendants on all), {everywhere} other lights on every layer.");
        }

        // From the mesh's own bounds and the transform: a renderer that starts switched off (a damage variant)
        // reports no world bounds until it is first shown.
        private static Bounds WorldBounds(Renderer renderer)
        {
            Bounds local = renderer.localBounds;
            Matrix4x4 toWorld = renderer.transform.localToWorldMatrix;
            var world = new Bounds(toWorld.MultiplyPoint3x4(local.center), Vector3.zero);
            for (int corner = 0; corner < 8; corner++)
            {
                var sign = new Vector3((corner & 1) == 0 ? -1f : 1f, (corner & 2) == 0 ? -1f : 1f, (corner & 4) == 0 ? -1f : 1f);
                world.Encapsulate(toWorld.MultiplyPoint3x4(local.center + Vector3.Scale(local.extents, sign)));
            }
            return world;
        }
    }
}
