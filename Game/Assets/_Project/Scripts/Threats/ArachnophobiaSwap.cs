using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Threats
{
    /// <summary>
    /// Arachnophobia mode (0.19.7), on the Crawlers: with the setting on, this machine hides their legged
    /// model and shows a few lumps of grit that scurry and bob in its place. Only the look changes.
    /// </summary>
    public class ArachnophobiaSwap : MonoBehaviour
    {
        private Renderer[] original;
        private Transform lumps;
        private bool shown;
        // The lumps' one material (made here, so it is ours to destroy: swarms come and go all job).
        private Material lumpMaterial;

        private void Start()
        {
            original = GetComponentsInChildren<Renderer>(true);
            lumps = new GameObject("GritLumps").transform;
            lumps.SetParent(transform, false);
            Material grit = original.Length > 0 && original[0] != null ? original[0].sharedMaterial : null;
            for (int i = 0; i < 6; i++)
            {
                GameObject lump = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                Collider solid = lump.GetComponent<Collider>();
                if (solid != null)
                {
                    solid.enabled = false; // Destroy waits for the end of the frame
                    Destroy(solid);
                }
                lump.transform.SetParent(lumps, false);
                lump.transform.localPosition = new Vector3(Mathf.Cos(i * 1.05f) * 0.35f, 0.12f, Mathf.Sin(i * 1.05f) * 0.35f);
                lump.transform.localScale = new Vector3(0.28f, 0.18f, 0.3f) * (0.8f + 0.08f * i);
                var r = lump.GetComponent<Renderer>();
                if (lumpMaterial == null)
                {
                    Material source = grit != null ? grit : r.sharedMaterial;
                    if (source != null) lumpMaterial = new Material(source) { name = "GritLump", color = new Color(0.3f, 0.28f, 0.25f) };
                }
                if (lumpMaterial != null) r.sharedMaterial = lumpMaterial;
            }
            Apply(GameSettings.Arachnophobia);
        }

        private void Update()
        {
            bool want = GameSettings.Arachnophobia;
            if (want != shown) Apply(want);
            if (!shown || lumps == null) return;
            for (int i = 0; i < lumps.childCount; i++)
            {
                Transform l = lumps.GetChild(i);
                Vector3 p = l.localPosition;
                p.y = 0.1f + Mathf.Abs(Mathf.Sin(Time.time * 9f + i)) * 0.06f;
                l.localPosition = p;
            }
        }

        private void OnDestroy()
        {
            if (lumpMaterial != null) Destroy(lumpMaterial);
        }

        private void Apply(bool on)
        {
            shown = on;
            foreach (Renderer r in original) if (r != null) r.enabled = !on;
            if (lumps != null) lumps.gameObject.SetActive(on);
        }
    }
}
