using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Structure
{
    /// <summary>A local fixture fragment falls with its slab without adding gameplay collision or load.</summary>
    public sealed class MeridianFixtureFall : MonoBehaviour
    {
        [SerializeField] private StructuralSection support;
        [SerializeField] private Transform visual;
        [SerializeField] private MeridianPresentationConfig config;
        private Renderer[] sourceRenderers;
        private bool[] visibleBeforeCollapse;
        private GameObject fragment;

        private void Awake()
        {
            sourceRenderers = visual != null ? visual.GetComponentsInChildren<Renderer>(true) : System.Array.Empty<Renderer>();
            visibleBeforeCollapse = new bool[sourceRenderers.Length];
            for (int i = 0; i < sourceRenderers.Length; i++) visibleBeforeCollapse[i] = sourceRenderers[i].enabled;
        }

        private void OnEnable()
        {
            if (support == null) return;
            support.Collapsed += Fall;
            support.Restored += Restore;
        }

        private void OnDisable()
        {
            if (support == null) return;
            support.Collapsed -= Fall;
            support.Restored -= Restore;
            ClearFragment();
        }

        private void Fall(StructuralSection section)
        {
            if (visual == null || config == null || GameLayers.DebrisLayer < 0) return;
            ClearFragment();
            fragment = Instantiate(visual.gameObject, visual.position, visual.rotation);
            fragment.name = "FixtureFragment_" + section.name;
            fragment.transform.localScale = visual.lossyScale;
            foreach (Transform child in fragment.GetComponentsInChildren<Transform>(true)) child.gameObject.layer = GameLayers.DebrisLayer;
            // Imported models remain cosmetic even if a later artist adds an FBX collider.
            foreach (Collider collider in fragment.GetComponentsInChildren<Collider>(true)) Destroy(collider);
            foreach (Light light in fragment.GetComponentsInChildren<Light>(true)) Destroy(light);
            Renderer[] renderers = fragment.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++) renderers[i].enabled = i < visibleBeforeCollapse.Length && visibleBeforeCollapse[i];
            foreach (LODGroup lod in fragment.GetComponentsInChildren<LODGroup>(true)) lod.ForceLOD(-1);

            var body = fragment.AddComponent<Rigidbody>();
            body.mass = 2f;
            body.linearVelocity = Vector3.down;
            int seed = 17;
            foreach (char letter in section.name) seed = unchecked(seed * 31 + letter);
            body.angularVelocity = new Vector3(0.6f, ((seed & 255) / 255f - 0.5f) * 2f, 0.35f);
            Destroy(fragment, config.FixtureFallLifetime);
        }

        private void Restore(StructuralSection section) => ClearFragment();

        private void ClearFragment()
        {
            if (fragment != null) Destroy(fragment);
            fragment = null;
        }

        private void OnDrawGizmos()
        {
            if (!DebugView.Visible || support == null) return;
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, support.transform.position);
        }

#if UNITY_EDITOR
        public void EditorSetup(StructuralSection section, Transform fixtureVisual, MeridianPresentationConfig settings)
        {
            support = section;
            visual = fixtureVisual;
            config = settings;
        }
#endif
    }
}
