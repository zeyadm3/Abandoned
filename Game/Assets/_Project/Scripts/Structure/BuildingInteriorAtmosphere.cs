using UnityEngine;

namespace Abandoned.Structure
{
    /// <summary>Local presentation follows the listener through the building's threshold.</summary>
    public class BuildingInteriorAtmosphere : MonoBehaviour
    {
        [SerializeField] private Bounds building = new(new Vector3(28f, 4f, 24f), new Vector3(56f, 16f, 48f));
        [SerializeField] private Color interiorFog = new(0.055f, 0.064f, 0.071f);
        [SerializeField] private float interiorDensity = 0.038f;
        private Color exteriorFog;
        private float exteriorDensity;
        private float blend;
        private bool captured;
        public static bool Present { get; private set; }
        public static float InsideBlend { get; private set; }

        private void OnEnable() => Present = true;

        private void Start()
        {
            exteriorFog = RenderSettings.fogColor;
            exteriorDensity = RenderSettings.fogDensity;
            captured = true;
        }

        private void LateUpdate()
        {
            Camera eye = Camera.main;
            if (eye == null || !captured) return;
            Vector3 point = eye.transform.position;
            float target = 0f;
            if (building.Contains(point))
            {
                Vector3 min = building.min, max = building.max;
                float depth = Mathf.Min(point.x - min.x, max.x - point.x, point.z - min.z, max.z - point.z);
                target = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(depth / 2.5f));
            }
            blend = Mathf.MoveTowards(blend, target, Time.deltaTime * 0.8f);
            InsideBlend = blend;
            RenderSettings.fogColor = Color.Lerp(exteriorFog, interiorFog, blend);
            RenderSettings.fogDensity = Mathf.Lerp(exteriorDensity, interiorDensity, blend);
        }

        private void OnDisable()
        {
            InsideBlend = 0f;
            Present = false;
            if (!captured) return;
            RenderSettings.fogColor = exteriorFog;
            RenderSettings.fogDensity = exteriorDensity;
        }

#if UNITY_EDITOR
        public void EditorSetup(Bounds bounds) => building = bounds;
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() { InsideBlend = 0f; Present = false; }
    }
}
