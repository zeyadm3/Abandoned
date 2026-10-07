using Abandoned.UI;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

namespace Abandoned.Extraction
{
    /// <summary>
    /// The truck's haul board (UI step 3): a screen on the bay's front wall, facing the ramp, showing haul vs
    /// quota, a fill bar and the cargo space, so the crew can read the number while loading. A small UI Toolkit
    /// panel drawn into a texture on a quad; every machine draws its own from the replicated run state.
    /// </summary>
    public class TruckDisplay : MonoBehaviour
    {
        [SerializeField] private Renderer screen;
        [SerializeField] private ThemeStyleSheet theme;
        [SerializeField] private Font font;
        [SerializeField] private Vector2Int size = new(640, 320);

        private RenderTexture texture;
        private PanelSettings panel;
        private UIDocument document;
        private Label amount, quota, cargo, state;
        private VisualElement bar;
        private string shown;

        private void Start()
        {
            // Headless (nettest, servers): nothing to draw on.
            if (screen == null || SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null) return;
            texture = new RenderTexture(size.x, size.y, 0, RenderTextureFormat.ARGB32) { name = "TruckDisplay" };
            panel = ScriptableObject.CreateInstance<PanelSettings>();
            panel.themeStyleSheet = theme;
            panel.targetTexture = texture;
            panel.scaleMode = PanelScaleMode.ConstantPixelSize;
            panel.clearColor = true;
            panel.colorClearValue = new Color(0.04f, 0.045f, 0.05f, 1f);
            var host = new GameObject("TruckDisplayPanel");
            host.transform.SetParent(transform, false);
            document = host.AddComponent<UIDocument>();
            document.panelSettings = panel;
            Build(document.rootVisualElement);
            screen.material.SetTexture("_BaseMap", texture);
            screen.material.SetColor("_BaseColor", Color.white);
        }

        private void Build(VisualElement root)
        {
            root.AddToClassList("truck-display");
            if (font != null) root.style.unityFontDefinition = FontDefinition.FromFont(font);
            UiKit.Hazard(root);
            state = Text(root, "HAUL", "truck-display__label");
            amount = Text(root, "$0", "truck-display__amount");
            quota = Text(root, "QUOTA $0", "truck-display__quota");
            bar = UiKit.Bar(root, "truck-display__bar");
            cargo = Text(root, "", "truck-display__cargo");
        }

        private static Label Text(VisualElement parent, string text, string cls)
        {
            var l = new Label(text);
            l.AddToClassList(cls);
            parent.Add(l);
            return l;
        }

        private void Update()
        {
            if (document == null) return;
            RunState run = RunState.Current;
            if (run == null || !run.IsSpawned) return;
            RunNetState s = run.State;
            string key = $"{s.Haul}|{s.Quota}|{s.CargoVolume}|{s.CargoCapacity}|{s.Phase}";
            if (key == shown) return;
            shown = key;
            bool met = s.Haul >= s.Quota;
            amount.text = $"${s.Haul:N0}";
            quota.text = met ? $"QUOTA ${s.Quota:N0}  -  MET" : $"QUOTA ${s.Quota:N0}  -  ${s.Quota - s.Haul:N0} TO GO";
            quota.EnableInClassList("truck-display__quota--met", met);
            UiKit.SetBar(bar, s.Quota > 0 ? (float)s.Haul / s.Quota : 1f, met ? "good" : null);
            cargo.text = s.Overloaded ? $"OVERLOADED  {s.CargoVolume:0.0} / {s.CargoCapacity:0} m³" : $"CARGO  {s.CargoVolume:0.0} / {s.CargoCapacity:0} m³";
            cargo.EnableInClassList("truck-display__cargo--bad", s.Overloaded);
            state.text = s.Phase == RunPhase.Honking ? "LEAVING!" : "HAUL";
        }

        private void OnDestroy()
        {
            if (document != null) Destroy(document.gameObject);
            if (panel != null) Destroy(panel);
            if (texture != null) texture.Release();
        }

#if UNITY_EDITOR
        public void EditorSetup(Renderer screenRenderer, ThemeStyleSheet themeSheet, Font bodyFont)
        {
            screen = screenRenderer;
            theme = themeSheet;
            font = bodyFont;
        }
#endif
    }
}
