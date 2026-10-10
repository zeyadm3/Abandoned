using System.Collections.Generic;
using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Structure
{
    /// <summary>A steady generator or battery lamp, independent of the mall's failing retail power.</summary>
    [RequireComponent(typeof(Light))]
    public sealed class SalvageWorkLight : MonoBehaviour
    {
        [SerializeField] private Light lamp;
        [OptionalReference, SerializeField] private Transform housing;
        [SerializeField] private string circuit = "Solutions Inc. clearance generator";
        [SerializeField] private bool powered = true;
        private bool attached = true;
        private static readonly List<SalvageWorkLight> all = new();

        public static IReadOnlyList<SalvageWorkLight> All => all;
        public Light Lamp => lamp;
        public Transform Housing => housing;
        public bool Lit => attached && powered && lamp != null && lamp.isActiveAndEnabled;
        public string Circuit => circuit;

        private void Awake()
        {
            if (lamp == null) lamp = GetComponent<Light>();
            LevelStoreys.LightEverything(lamp);
            Apply();
        }

        private void OnEnable()
        {
            if (!all.Contains(this)) all.Add(this);
            if (lamp == null) lamp = GetComponent<Light>();
            Apply();
        }

        private void OnDisable()
        {
            all.Remove(this);
            if (lamp != null) lamp.enabled = false;
        }

        public void SetPowered(bool on)
        {
            powered = on;
            Apply();
        }

        public void SetAttached(bool on)
        {
            attached = on;
            Apply();
        }

        private void Apply()
        {
            if (lamp != null) lamp.enabled = attached && powered && isActiveAndEnabled;
            foreach (MeshRenderer beam in GetComponentsInChildren<MeshRenderer>(true))
                beam.enabled = attached && powered && isActiveAndEnabled;
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private void OnGUI()
        {
            if (!DebugView.Visible || all.Count == 0 || all[0] != this) return;
            int lit = 0;
            foreach (SalvageWorkLight light in all) if (light != null && light.Lit) lit++;
            GUI.Label(new Rect(Screen.width - 360f, 186f, 350f, 22f),
                $"WORKSITE POWER: {lit}/{all.Count} steady lamps · shadows use shared budget");
        }

        private void OnDrawGizmos()
        {
            if (!DebugView.Visible || lamp == null) return;
            Gizmos.color = Lit ? new Color(1f, .77f, .30f, .7f) : Color.gray;
            Gizmos.DrawWireSphere(transform.position, .13f);
            Gizmos.DrawLine(transform.position, transform.position + transform.forward * Mathf.Min(3f, lamp.range));
#if UNITY_EDITOR
            UnityEditor.Handles.Label(transform.position + Vector3.up * .22f, $"{circuit}\n{(Lit ? "steady / powered" : "off")}");
#endif
        }
#endif

#if UNITY_EDITOR
        public void EditorSetup(Light light, string circuitName, Transform lampHousing = null)
        {
            lamp = light;
            housing = lampHousing;
            circuit = circuitName;
            powered = true;
            LevelStoreys.LightEverything(lamp);
        }
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => all.Clear();
    }
}
