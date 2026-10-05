using System.Collections.Generic;
using UnityEngine;

namespace Abandoned.UI
{
    /// <summary>
    /// Draws floating world texts with OnGUI (placeholder until the UI milestone). Created on
    /// first use and kept across scene loads.
    /// </summary>
    public class FloatingTextOverlay : MonoBehaviour
    {
        private const float Lifetime = 1.4f;
        private const float RiseMetres = 0.8f;

        private struct Entry
        {
            public Vector3 Position;
            public string Text;
            public Color Color;
            public float Start;
        }

        private static FloatingTextOverlay instance;
        private readonly List<Entry> entries = new();
        private GUIStyle style;

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

        public void Add(Vector3 position, string text, Color color) =>
            entries.Add(new Entry { Position = position, Text = text, Color = color, Start = Time.time });

        private void Update() => entries.RemoveAll(e => Time.time - e.Start > Lifetime);

        private void OnGUI()
        {
            Camera camera = Camera.main;
            if (camera == null || entries.Count == 0) return;
            style ??= new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 20, fontStyle = FontStyle.Bold };

            foreach (Entry e in entries)
            {
                float t = (Time.time - e.Start) / Lifetime;
                Vector3 screen = camera.WorldToScreenPoint(e.Position + Vector3.up * (RiseMetres * t));
                if (screen.z <= 0f) continue;
                Color c = e.Color;
                c.a = 1f - t * t;
                style.normal.textColor = c;
                GUI.Label(new Rect(screen.x - 100f, Screen.height - screen.y - 15f, 200f, 30f), e.Text, style);
            }
        }
    }
}
