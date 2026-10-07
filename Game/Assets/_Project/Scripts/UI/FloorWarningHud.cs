using Abandoned.Networking;
using Abandoned.Structure;
using UnityEngine;
using UnityEngine.UIElements;

namespace Abandoned.UI
{
    /// <summary>
    /// The floor under you, said plainly (M8.4): a steady "cracking" note while the section you stand
    /// on is cracking, and a pulsing red "it's giving way" while it's failing, so a collapse never
    /// comes out of nowhere (GDD 27: did players understand why?). Local only.
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
            Warning = under != null && under.Stage >= StructuralStage.Cracking ? under.Stage : null;
            if (label == null)
            {
                if (Warning == null || HudLayer.Root == null) return;
                label = HudLayer.Label("hud-floor");
            }
            MenuKit.Show(label, Warning != null);
            if (Warning == null) return;
            bool failing = Warning == StructuralStage.Failing;
            string text = failing ? "THE FLOOR IS GIVING WAY - GET OFF!" : "CRACKING FLOOR - TOO MUCH WEIGHT";
            if (label.text != text) label.text = text;
            label.EnableInClassList("hud-floor--failing", failing);
            label.style.opacity = failing ? 0.55f + 0.45f * Mathf.Abs(Mathf.Sin(Time.time * 7f)) : 0.9f;
        }

        private void OnDisable()
        {
            if (label != null) MenuKit.Show(label, false);
            Warning = null;
        }

        private void OnDestroy() => HudLayer.Remove(label);
    }
}
