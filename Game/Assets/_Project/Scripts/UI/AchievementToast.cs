using System.Collections.Generic;
using Abandoned.Core;
using UnityEngine;

namespace Abandoned.UI
{
    /// <summary>"ACHIEVEMENT UNLOCKED" (M9.5), one after another, through the toast feed (UI step 3).</summary>
    public class AchievementToast : MonoBehaviour
    {
        private const float Seconds = 4.5f;

        private readonly Queue<AchievementDefinition> waiting = new();
        private float until;

        /// <summary>Tests: the achievement being announced now.</summary>
        public AchievementDefinition Showing { get; private set; }

        private void OnEnable() => Achievements.Unlocked += Enqueue;

        private void OnDisable() => Achievements.Unlocked -= Enqueue;

        private void Enqueue(AchievementDefinition a) => waiting.Enqueue(a);

        private void Update()
        {
            if (Showing != null && Time.time < until) return;
            Showing = null;
            if (waiting.Count == 0) return;
            Showing = waiting.Dequeue();
            until = Time.time + Seconds;
            ToastFeed.Show("ACHIEVEMENT UNLOCKED", $"{Showing.DisplayName}: {Showing.Description}", "icon/trophy", ToastFeed.Kind.Good);
        }
    }
}
