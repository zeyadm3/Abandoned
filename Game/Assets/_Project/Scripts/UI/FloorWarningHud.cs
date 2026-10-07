using Abandoned.Networking;
using Abandoned.Structure;
using UnityEngine;
using UnityEngine.UIElements;

namespace Abandoned.UI
{
    /// <summary>
    /// A small final cue only while the supporting floor is actively failing. Earlier strain is read
    /// from the building's cracks, motion and sounds, with optional directional subtitles.
    /// </summary>
    public class FloorWarningHud : MonoBehaviour
    {
        private Label label;

        /// <summary>Tests: the stage being warned about (null = nothing).</summary>
        public StructuralStage? Warning { get; private set; }

        private void Update()
        {
            NetworkPlayer me = NetworkPlayer.Local;
            StructuralSection under = me != null && me.IsSpawned && !me.IsDead ? SectionQuery.Under(me.transform.position) : null;
            Warning = under != null && under.Stage == StructuralStage.Failing ? under.Stage : null;
            if (label == null)
            {
                if (Warning == null || HudLayer.Root == null) return;
                label = HudLayer.Label("hud-floor");
            }
            MenuKit.Show(label, Warning != null);
            if (Warning == null) return;
            string text = "FLOOR GIVING WAY · MOVE";
            if (label.text != text) label.text = text;
            label.style.opacity = 0.85f + 0.15f * Mathf.Abs(Mathf.Sin(Time.time * 3f));
        }

        private void OnDisable()
        {
            if (label != null) MenuKit.Show(label, false);
            Warning = null;
        }

        private void OnDestroy() => HudLayer.Remove(label);
    }
}
