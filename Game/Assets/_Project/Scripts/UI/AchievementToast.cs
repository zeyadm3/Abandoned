using System.Collections.Generic;
using Abandoned.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace Abandoned.UI
{
    /// <summary>"ACHIEVEMENT UNLOCKED" on the HUD for a few seconds (M9.5), one after another.</summary>
    public class AchievementToast : MonoBehaviour
    {
        private const float Seconds = 4.5f;

        private readonly Queue<AchievementDefinition> waiting = new();
        private Label label;
        private float until;

        public AchievementDefinition Showing { get; private set; }

        private void OnEnable() => Achievements.Unlocked += Enqueue;

        private void OnDisable() => Achievements.Unlocked -= Enqueue;

        private void Enqueue(AchievementDefinition a) => waiting.Enqueue(a);

        private void Update()
        {
            if (Showing != null && Time.time < until) return;
            Showing = null;
            if (label != null) MenuKit.Show(label, false);
            if (waiting.Count == 0) return;
            if (label == null && (label = HudLayer.Label("hud-panel", "hud-toast")) == null) return;
            Showing = waiting.Dequeue();
            until = Time.time + Seconds;
            label.text = $"<b>ACHIEVEMENT UNLOCKED</b>\n{Showing.DisplayName}: {Showing.Description}";
            MenuKit.Show(label, true);
            Audio.GameAudio.PlayUi(Audio.SoundId.UiConfirm);
        }

        private void OnDestroy() => HudLayer.Remove(label);
    }
}
