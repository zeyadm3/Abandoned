using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Structure
{
    /// <summary>
    /// F1: stage-coloured gizmos for every section, and on screen the load/capacity, health and
    /// stage of nearby sections plus the building's stability and seed. On a client the numbers are the
    /// host's, replicated (health to 1/255, load to 5 kg), and the header says MIRROR.
    /// </summary>
    public class StructureDebugView : MonoBehaviour
    {
        [SerializeField] private StructureSimulation simulation;
        [SerializeField, Min(1f)] private float labelRange = 18f;

        private GUIStyle label;
        private GUIStyle box;

        public static Color ColourFor(StructuralStage stage) => stage switch
        {
            StructuralStage.Stable => new Color(0.3f, 0.9f, 0.4f),
            StructuralStage.Stressed => new Color(1f, 0.85f, 0.2f),
            StructuralStage.Cracking => new Color(1f, 0.5f, 0.1f),
            StructuralStage.Failing => new Color(1f, 0.15f, 0.1f),
            _ => new Color(0.4f, 0.4f, 0.4f),
        };

        // QA P-08: a debug overlay only; in player builds it never draws, so it doesn't sit in the GUI loop either.
        private void Start()
        {
            if (!Abandoned.Core.DevTools.Enabled) enabled = false;
        }

        private void OnGUI()
        {
            Camera camera = Camera.main;
            if (!DebugView.Visible || camera == null || simulation == null) return;
            label ??= new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 11, richText = true };
            box ??= new GUIStyle(GUI.skin.box) { alignment = TextAnchor.UpperLeft, fontSize = 14, richText = true };

            string role = simulation.IsMirror ? "<color=#7fd4ff>MIRROR of host</color>" : "<color=#7fff7f>HOST sim</color>";
            string header = $"<b>STRUCTURE</b>  {role}  stability {simulation.Stability:P0}  seed {simulation.Seed}  " +
                            $"collapses {simulation.CollapseCount}  gen {simulation.Generation}\n" +
                            (simulation.IsMirror ? "keys are host-only" : "[-]/[=] stability   [F2] re-roll damage   (keys work while F1 is on)");
            var content = new GUIContent(header);
            Vector2 size = box.CalcSize(content);
            GUI.Box(new Rect(10f, Screen.height - size.y - 10f, size.x, size.y), content, box);

            foreach (StructuralSection s in simulation.Sections)
            {
                if (s.IsCollapsed) continue;
                Vector3 world = s.SurfaceBounds.center + Vector3.up * 0.4f;
                if (Vector3.Distance(camera.transform.position, world) > labelRange) continue;
                Vector3 screen = camera.WorldToScreenPoint(world);
                if (screen.z <= 0f) continue;
                string colour = ColorUtility.ToHtmlStringRGB(ColourFor(s.Stage));
                string text = $"<color=#{colour}><b>{s.Stage}</b></color>\n{s.Load:0}/{s.Capacity:0} kg  HP {s.HealthFraction:P0}" +
                              (s.Stage == StructuralStage.Failing ? $"\n{s.Config.FailingDuration - s.FailingTime:0.0}s" : "");
                GUI.Label(new Rect(screen.x - 80f, Screen.height - screen.y - 24f, 160f, 48f), text, label);
            }
        }

        private void OnDrawGizmos()
        {
            if (simulation == null || !Application.isPlaying || !DebugView.Visible) return;
            foreach (StructuralSection s in simulation.Sections)
            {
                Bounds b = s.SurfaceBounds;
                Gizmos.color = ColourFor(s.Stage);
                Gizmos.DrawWireCube(b.center, b.size);
                // Load bar: fills along the section's width as load approaches capacity.
                float fill = Mathf.Clamp01(s.LoadRatio);
                Vector3 start = new(b.min.x, b.max.y + 0.05f, b.center.z);
                Gizmos.DrawLine(start, start + Vector3.right * (b.size.x * fill));
            }
        }
    }
}
