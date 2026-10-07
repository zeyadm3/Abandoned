using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Abandoned.UI
{
    /// <summary>
    /// Floating world texts ("-$1,200") on the HUD layer (M8.1): each rises and fades over its lifetime.
    /// Created on first use and kept across scene loads; with no HUD (bare test rigs) entries still
    /// count, they just aren't drawn.
    /// </summary>
    public class FloatingTextOverlay : MonoBehaviour
    {
        private const float Lifetime = 1.4f;
        private const float RiseMetres = 0.8f;

        private sealed class Entry
        {
            public Vector3 Position;
            public float Start;
            public Label Label;
        }

        private static FloatingTextOverlay instance;
        private readonly List<Entry> entries = new();

        public static FloatingTextOverlay Instance
        {
            get
            {
                if (instance != null) return instance;
                var go = new GameObject("FloatingTextOverlay");
                DontDestroyOnLoad(go);
                instance = go.AddComponent<FloatingTextOverlay>();
                return instance;
            }
        }

        public int ActiveCount => entries.Count;

        public void Add(Vector3 position, string text, Color color)
        {
            Label label = HudLayer.Label("hud-float");
            if (label != null)
            {
                label.text = text;
                label.style.color = color;
            }
            entries.Add(new Entry { Position = position, Start = Time.time, Label = label });
        }

        private void LateUpdate()
        {
            Camera camera = Camera.main;
            for (int i = entries.Count - 1; i >= 0; i--)
            {
                Entry e = entries[i];
                float t = (Time.time - e.Start) / Lifetime;
                if (t > 1f)
                {
                    HudLayer.Remove(e.Label);
                    entries.RemoveAt(i);
                    continue;
                }
                if (e.Label == null) continue;
                e.Label.style.opacity = 1f - t * t;
                HudLayer.Place(e.Label, e.Position + Vector3.up * (RiseMetres * t), camera);
            }
        }
    }
}
