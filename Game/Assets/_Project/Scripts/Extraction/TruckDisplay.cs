using Abandoned.Core;
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
            // 0.24.8: the menu's fallback fonts, or the board shows boxes in every script the display font lacks.
            LanguageFonts.Apply(panel);
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
            UiThemeAssets.Apply(root);
            UiKit.Hazard(root);
            state = Text(root, Loc.T("RECOVERY MANIFEST"), "truck-display__label");
            amount = Text(root, "$0", "truck-display__amount");
            quota = Text(root, Loc.T("QUOTA $0"), "truck-display__quota");
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
            document.rootVisualElement.EnableInClassList("colorblind", Abandoned.Core.GameSettings.ColorblindScanner);
            RunState run = RunState.Current;
            if (run == null || !run.IsSpawned) return;
            RunNetState s = run.State;
            // The language's version is in the key: the board is its own document, outside the HUD that is built again.
            string key = $"{s.Haul}|{s.Quota}|{s.CargoVolume}|{s.CargoCapacity}|{s.Phase}|{Languages.Version}";
            if (key == shown) return;
            shown = key;
            bool met = s.Haul >= s.Quota;
            amount.text = $"${s.Haul:N0}";
            quota.text = met ? Loc.T("QUOTA ${0:N0}  -  MET", s.Quota) : Loc.T("QUOTA ${0:N0}  -  ${1:N0} TO GO", s.Quota, s.Quota - s.Haul);
            quota.EnableInClassList("truck-display__quota--met", met);
            UiKit.SetBar(bar, s.Quota > 0 ? (float)s.Haul / s.Quota : 1f, met ? "good" : null);
            cargo.text = s.Overloaded ? Loc.T("OVERLOADED  {0:0.0} / {1:0} m³", s.CargoVolume, s.CargoCapacity) : Loc.T("CARGO  {0:0.0} / {1:0} m³", s.CargoVolume, s.CargoCapacity);
            cargo.EnableInClassList("truck-display__cargo--bad", s.Overloaded);
            state.text = s.Phase == RunPhase.Honking ? Loc.T("EVACUATING") : Loc.T("RECOVERY MANIFEST");
        }

        private void OnDestroy()
        {
            if (document != null) Destroy(document.gameObject);
            if (panel != null) Destroy(panel);
            if (texture != null) { texture.Release(); Destroy(texture); }
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
