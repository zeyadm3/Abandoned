using UnityEngine;

namespace Abandoned.Core
{
    /// <summary>F1: recent noise events as fading rings (gizmos) and on-screen labels with source and radius.</summary>
    public class NoiseDebugView : MonoBehaviour
    {
        private GUIStyle style;

        private void OnGUI()
        {
            Camera camera = Camera.main;
            if (!DebugView.Visible || camera == null) return;
            style ??= new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 12, richText = true };

            foreach (NoiseEvent e in NoiseSystem.RecentEvents)
            {
                float age = Time.time - e.Time;
                if (age > NoiseSystem.HistorySeconds) continue;
                Vector3 screen = camera.WorldToScreenPoint(e.Position + Vector3.up * 0.3f);
                if (screen.z <= 0f) continue;
                float alpha = 1f - age / NoiseSystem.HistorySeconds;
                string colour = ColorUtility.ToHtmlStringRGBA(new Color(0.5f, 0.85f, 1f, alpha));
                GUI.Label(new Rect(screen.x - 70f, Screen.height - screen.y - 10f, 140f, 20f),
                    $"<color=#{colour}>(( {e.Source} {NoiseSystem.RadiusOf(e):0}m ))</color>", style);
            }
        }

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying || !DebugView.Visible) return;
            foreach (NoiseEvent e in NoiseSystem.RecentEvents)
            {
                float alpha = 1f - (Time.time - e.Time) / NoiseSystem.HistorySeconds;
                if (alpha <= 0f) continue;
                Gizmos.color = new Color(0.5f, 0.85f, 1f, alpha);
                Gizmos.DrawWireSphere(e.Position, NoiseSystem.RadiusOf(e));
            }
        }
    }
}
